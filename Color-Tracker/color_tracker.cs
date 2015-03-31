using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Threading;

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace tracking
{
    /// <summary>
    /// Finds specified colors in images
    /// </summary>
    class color_finder
    {
        // Data types
        public struct color_to_find
        {
            public Color name;
            public HSLFiltering filter;
        }
        /// <summary>
        /// Stores a color and the rectangle surrounding where it was found in an image
        /// </summary>
        public struct found_color_rectangle
        {
            public Color name;
            public Rectangle location;
        }
        /// <summary>
        /// Stores a color and center coordinates where it was found in an image
        /// </summary>
        public struct found_color_center
        {
            public Color name;
            public int x;
            public int y;
        }

        // Internal variables
        private List<color_to_find> colors = new List<color_to_find>();
        private List<int> thread_index;
        private BackgroundWorker[] bw;
        private AutoResetEvent[] evs;
        private Mutex input_mutex = new Mutex(false);
        private Mutex found_locations_mutex = new Mutex(false);
        private List<found_color_rectangle> color_locations = new List<found_color_rectangle>();
        private Bitmap current_image;

        // Constructors
        public color_finder()
        {
        }

        // Properties
        /// <summary>
        /// Returns the value of the number of colors currently being targeted
        /// </summary>
        public int num_colors_to_find
        {
            get { return colors.Count; }
        }
        /// <summary>
        /// Returns the value of the number of colors last found in the current scene
        /// </summary>
        public int num_colors_found
        {
            get { return color_locations.Count; }
        }
        /// <summary>
        /// Returns the colors currently being targeted
        /// </summary>
        public List<color_to_find> colors_to_find
        {
            get { return colors; }
        }
        /// <summary>
        /// Returns the rectangles surrounding the color blobs last located in the current scene
        /// <para>Note: the rectangle only surrounds the largest blob of each color</para>
        /// </summary>
        public List<found_color_rectangle> found_color_locations_rectangle
        {
            get { return color_locations; }
        }
        /// <summary>
        /// Returns the center of the color blobs last located within the current scene
        /// <para>Note: the center is found only for the largest blob of each color</para>
        /// </summary>
        public List<found_color_center> found_color_locations_center
        {
            get
            {
                found_color_center new_location = new found_color_center();
                List<found_color_center> center_locations = new List<found_color_center>();
                foreach (found_color_rectangle fcr in color_locations)
                {
                    new_location.name = fcr.name;
                    new_location.x = fcr.location.X + (fcr.location.Width / 2);
                    new_location.y = fcr.location.Y + (fcr.location.Height / 2);
                    center_locations.Add(new_location);
                }

                return center_locations;
            }
        }

        // Adding/Removing targeted colors functions
        /// <summary>
        /// Adds a color to the currently targeted color list
        /// </summary>
        public void add_color(Color name, int min_hue = -1, int max_hue = -1, double min_sat = -1.0, double max_sat = -1.0, double min_lum = -1.0, double max_lum = -1.0)
        {
            HSLFiltering filter = new HSLFiltering();
            if (min_hue >= 0 && max_hue >= 0 && max_hue > min_hue && max_hue <= 360)
                filter.Hue = new IntRange(min_hue, max_hue);
            if (min_sat >= 0 && max_sat >= 0 && max_sat > min_sat && max_sat <= 1.0)
                filter.Saturation = new DoubleRange(min_sat, max_sat);
            if (min_lum >= 0 && max_lum >= 0 && max_lum > min_lum && max_lum <= 1.0)
                filter.Luminance = new DoubleRange(min_lum, max_lum);
            add_color(name, filter);
        }
        public void add_color(Color name, HSLFiltering filter)
        {
            color_to_find new_color = new color_to_find();
            new_color.name = name;
            new_color.filter = filter;
            colors.Add(new_color);
        }
        public void add_color(color_to_find new_color)
        {
            colors.Add(new_color);
        }
        /// <summary>
        /// Clears all colors from the currently targeted color list
        /// </summary>
        public void clear_colors()
        {
            colors.Clear();
        }

        // Find color(s) functions
        /// <summary>
        /// Finds an individual color in the current image, does not modify current target colors, does modify found locations
        /// <para>Updates current image to new_image if provided</para>
        /// </summary>
        public List<found_color_rectangle> find_color(out List<found_color_center> output_centers, int min_hue = -1, int max_hue = -1, double min_sat = -1.0, double max_sat = -1.0, double min_lum = -1.0, double max_lum = -1.0, Bitmap new_image = null)
        {
            // Setup color filter
            color_to_find c = new color_to_find();
            c.filter = new HSLFiltering();
            if (min_hue >= 0 && max_hue >= 0 && max_hue > min_hue)
                c.filter.Hue = new IntRange(min_hue, max_hue);
            if (min_sat >= 0 && max_sat >= 0 && max_sat > min_sat)
                c.filter.Saturation = new DoubleRange(min_sat, max_sat);
            if (min_lum >= 0 && max_lum >= 0 && max_lum > min_lum)
                c.filter.Luminance = new DoubleRange(min_lum, max_lum);

            return find_color(out output_centers, c, new_image);
        }
        public List<found_color_rectangle> find_color(out List<found_color_center> output_centers, HSLFiltering filter, Bitmap new_image = null)
        {
            color_to_find c = new color_to_find();
            c.filter = filter;
            return find_color(out output_centers, c, new_image);
        }
        public List<found_color_rectangle> find_color(out List<found_color_center> output_centers, color_to_find c, Bitmap new_image = null)
        {
            if (new_image != null)
                current_image = new_image;

            // Setup blob filter
            BlobCounterBase blob_counter = new BlobCounter();
            blob_counter.FilterBlobs = true;
            blob_counter.MinWidth = 4;
            blob_counter.MinHeight = 4;
            blob_counter.ObjectsOrder = ObjectsOrder.Size;

            // Find colors
            color_locations.Clear();  // Clear old results
            Bitmap filtered_image = c.filter.Apply(current_image);
            blob_counter.ProcessImage(filtered_image);
            Blob[] blobs = blob_counter.GetObjectsInformation();
            if (blobs.Length > 0)  // Something was found
            {
                found_color_rectangle new_location = new found_color_rectangle();
                new_location.name = c.name;
                new_location.location = blobs[0].Rectangle;
                color_locations.Add(new_location);
            }

            output_centers = found_color_locations_center;
            return color_locations;
        }
        /// <summary>
        /// Finds currently targeted colors in the current image
        /// <para>Updates current image to new_image if provided</para>
        /// </summary>
        public List<found_color_rectangle> find_colors(out List<found_color_center> output_centers, Bitmap new_image = null)
        {
            // Copy new image is nessasary
            if (new_image != null)
                current_image = new_image;

            color_locations.Clear();  // Clear old results

            // Setup threading
            bw = new BackgroundWorker[colors.Count];
            evs = new AutoResetEvent[colors.Count];
            thread_index = new List<int>();
            for (int i = 0; i < colors.Count; ++i)
            {
                bw[i] = new BackgroundWorker();
                evs[i] = new AutoResetEvent(false);
                thread_index.Add(i);
            }

            // Dispatch threads
            for (int i = 0; i < colors.Count; ++i)
            {
                bw[i].DoWork += new DoWorkEventHandler(thread_find_colors);
                bw[i].RunWorkerAsync();
            }

            // Wait until processing is done
            WaitHandle.WaitAll(evs);

            // Output
            output_centers = found_color_locations_center;
            return color_locations;
        }

        // Internal functions
        private void thread_find_colors(object sender, DoWorkEventArgs e)
        {
            input_mutex.WaitOne();  // Wait for input to be available
            int i = thread_index[0];  // Get which colors and events to do work on
            thread_index.RemoveAt(0);
            Bitmap image_copy = AForge.Imaging.Image.Clone(current_image);
            input_mutex.ReleaseMutex();

            // Setup blob filter
            BlobCounterBase blob_counter = new BlobCounter();
            blob_counter.FilterBlobs = true;
            blob_counter.MinWidth = 4;
            blob_counter.MinHeight = 4;
            blob_counter.ObjectsOrder = ObjectsOrder.Size;

            // Find colors
            Bitmap filtered_image = colors[i].filter.Apply(image_copy);  // Apply HSV color filter
            blob_counter.ProcessImage(filtered_image);  // Apply blob filter
            Blob[] blobs = blob_counter.GetObjectsInformation();  // Extract blobs
            if (blobs.Length > 0)  // Something was found
            {
                found_color_rectangle new_location = new found_color_rectangle();
                new_location.name = colors[i].name;
                new_location.location = blobs[0].Rectangle;
                found_locations_mutex.WaitOne();  // Wait for access to found locations data
                color_locations.Add(new_location);
                found_locations_mutex.ReleaseMutex();
            }

            evs[i].Set();  // Report done
        }
    }
}
