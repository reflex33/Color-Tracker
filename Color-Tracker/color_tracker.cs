using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Threading;

using AForge.Imaging;

namespace tracking
{
    public class hsb_filter
    {
        private bool _filter_hue = false;
        public bool filter_hue
        {
            get { return _filter_hue; }
            set
            {
                if (value == false)
                {
                    _min_hue = -1;
                    _max_hue = -1;
                    _filter_hue = false;
                }
                else  // value == true
                {
                    if (_min_hue >= 0 && _min_hue <= 360 && _max_hue >= 0 && _max_hue <= 360 && _min_hue < _max_hue)
                        _filter_hue = true;
                    else
                        throw new System.ArgumentException("Min and max hue values are invalid");
                }
            }
        }
        private double _min_hue = -1;
        public double min_hue
        {
            get { return _min_hue; }
            set
            {
                _min_hue = value;
                if (_min_hue >= 0 && _min_hue <= 360 && _max_hue >= 0 && _max_hue <= 360 && _min_hue < _max_hue)
                    _filter_hue = true;
                else
                    _filter_hue = false;
            }
        }
        private double _max_hue = -1;
        public double max_hue
        {
            get { return _max_hue; }
            set
            {
                _max_hue = value;
                if (_min_hue >= 0 && _min_hue <= 360 && _max_hue >= 0 && _max_hue <= 360 && _min_hue < _max_hue)
                    _filter_hue = true;
                else
                    _filter_hue = false;
            }
        }

        private bool _filter_saturation = false;
        public bool filter_saturation
        {
            get { return _filter_saturation; }
            set
            {
                if (value == false)
                {
                    _min_saturation = -1;
                    _max_saturation = -1;
                    _filter_saturation = false;
                }
                else  // value == true
                {
                    if (_min_saturation >= 0 && _min_saturation <= 1.0 && _max_saturation >= 0 && _max_saturation <= 1.0 && _min_saturation < _max_saturation)
                        _filter_saturation = true;
                    else
                        throw new System.ArgumentException("Min and max saturation values are invalid");
                }
            }
        }
        private double _min_saturation = -1;
        public double min_saturation
        {
            get { return _min_saturation; }
            set
            {
                _min_saturation = value;
                if (_min_saturation >= 0 && _min_saturation <= 1.0 && _max_saturation >= 0 && _max_saturation <= 1.0 && _min_saturation < _max_saturation)
                    _filter_saturation = true;
                else
                    _filter_saturation = false;
            }
        }
        private double _max_saturation = -1;
        public double max_saturation
        {
            get { return _max_saturation; }
            set
            {
                _max_saturation = value;
                if (_min_saturation >= 0 && _min_saturation <= 1.0 && _max_saturation >= 0 && _max_saturation <= 1.0 && _min_saturation < _max_saturation)
                    _filter_saturation = true;
                else
                    _filter_saturation = false;
            }
        }

        private bool _filter_brightness = false;
        public bool filter_brightness
        {
            get { return _filter_brightness; }
            set
            {
                if (value == false)
                {
                    _min_brightness = -1;
                    _max_brightness = -1;
                    _filter_saturation = false;
                }
                else  // value == true
                {
                    if (_min_brightness >= 0 && _min_brightness <= 1.0 && _max_brightness >= 0 && _max_brightness <= 1.0 && _min_brightness < _max_brightness)
                        _filter_brightness = true;
                    else
                        throw new System.ArgumentException("Min and max brightness values are invalid");
                }
            }
        }
        private double _min_brightness = -1;
        public double min_brightness
        {
            get { return _min_brightness; }
            set
            {
                _min_brightness = value;
                if (_min_brightness >= 0 && _min_brightness <= 1.0 && _max_brightness >= 0 && _max_brightness <= 1.0 && _min_brightness < _max_brightness)
                    _filter_brightness = true;
                else
                    _filter_brightness = false;
            }
        }
        private double _max_brightness = -1;
        public double max_brightness
        {
            get { return _max_brightness; }
            set
            {
                _max_brightness = value;
                if (_min_brightness >= 0 && _min_brightness <= 1.0 && _max_brightness >= 0 && _max_brightness <= 1.0 && _min_brightness < _max_brightness)
                    _filter_brightness = true;
                else
                    _filter_brightness = false;
            }
        }

        public hsb_filter(double min_h = -1, double max_h = -1, double min_s = -1, double max_s = -1, double min_b = -1, double max_b = -1)
        {
            min_hue = min_h;
            max_hue = max_h;
            min_saturation = min_s;
            max_saturation = max_s;
            min_brightness = min_b;
            max_brightness = max_b;
        }
    }
    public class hsb_image
    {
        public struct hsb_pixel
        {
            public double hue;
            public double saturation;
            public double brightness;
        }

        private hsb_pixel[,] the_image;
        public int width { get; private set; }
        public int height { get; private set; }

        public hsb_image()
        {
        }
        public hsb_image(Bitmap input_image)
            : this()
        {
            set_image(input_image);
        }

        private byte[] raw_input_pixel_bytes;  // Data where image is marshaled into for speed
        public void set_image(Bitmap input_image)
        {
            if (input_image == null)
                throw new System.ArgumentException("Input image can't be NULL");
            if (System.Drawing.Bitmap.GetPixelFormatSize(input_image.PixelFormat) != 24)
                throw new System.ArgumentException("Input image must be 24bit color depth");

            int NUM_OF_THREADS = 8;
            width = input_image.Width;
            height = input_image.Height;

            // Marshall the image for speed
            Rectangle rect = new Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData bitmap_data = input_image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, input_image.PixelFormat);
            IntPtr Iptr = bitmap_data.Scan0;
            int step_size = 3;  // 3 bytes per pixel
            int pixel_count = width * height;
            raw_input_pixel_bytes = new byte[pixel_count * step_size];
            System.Runtime.InteropServices.Marshal.Copy(Iptr, raw_input_pixel_bytes, 0, raw_input_pixel_bytes.Length);
            input_image.UnlockBits(bitmap_data);
            the_image = new hsb_pixel[width, height];

            int num_of_lines = height / NUM_OF_THREADS;
            BackgroundWorker[] threads = new BackgroundWorker[NUM_OF_THREADS];
            AutoResetEvent[] thread_done = new AutoResetEvent[NUM_OF_THREADS];
            for (int i = 0; i < NUM_OF_THREADS; ++i)
            {
                threads[i] = new BackgroundWorker();
                threads[i].DoWork += new DoWorkEventHandler(thread_convert_image);
                thread_done[i] = new AutoResetEvent(false);
                Tuple<int, int, AutoResetEvent> work_data;
                if (i == NUM_OF_THREADS - 1)  // Make sure to get all of the rows of the image in the last thread, fixes if we have a non-whole-number of rows after dividing up the work
                    work_data = new Tuple<int, int, AutoResetEvent>(i * num_of_lines, height, thread_done[i]);
                else
                    work_data = new Tuple<int, int, AutoResetEvent>(i * num_of_lines, (i + 1) * num_of_lines, thread_done[i]);
                threads[i].RunWorkerAsync(work_data);
            }

            // Wait until processing is done
            WaitHandle.WaitAll(thread_done);
        }
        private void thread_convert_image(object sender, DoWorkEventArgs e)
        {
            Tuple<int, int, AutoResetEvent> work_data = (Tuple<int, int, AutoResetEvent>)e.Argument;
            int start_row = work_data.Item1;
            int end_row = work_data.Item2;  // Don't actually process this row, stop one short of it
            AutoResetEvent done = work_data.Item3;

            for (int x = 0; x < width; ++x)
                for (int y = work_data.Item1; y < work_data.Item2; ++y)
                {
                    int i = ((y * width) + x) * 3;  // 3 bytes per pixel
                    double B = raw_input_pixel_bytes[i] / 255.0;
                    double G = raw_input_pixel_bytes[i + 1] / 255.0;
                    double R = raw_input_pixel_bytes[i + 2] / 255.0;
                    double temp = Math.Max(R, G);
                    double M = Math.Max(temp, B);
                    temp = Math.Min(R, G);
                    double m = Math.Min(temp, B);
                    double C = M - m;

                    if (C == 0)
                        the_image[x, y].hue = 0;
                    else if (M == R)
                        the_image[x, y].hue = 60 * (((G - B) / C) % 6);
                    else if (M == G)
                        the_image[x, y].hue = 60 * (((B - R) / C) + 2);
                    else if (M == B)
                        the_image[x, y].hue = 60 * (((R - G) / C) + 4);
                    if (the_image[x, y].hue < 0)
                        the_image[x, y].hue = 360 + the_image[x, y].hue;

                    the_image[x, y].brightness = M;

                    if (the_image[x, y].brightness == 0)
                        the_image[x, y].saturation = 0;
                    else
                        the_image[x, y].saturation = C / the_image[x, y].brightness;
                }

            done.Set();
        }

        public hsb_pixel get_pixel(int x, int y)
        {
            if (the_image == null || x < 0 || x >= the_image.GetLength(0) || y < 0 || y >= the_image.GetLength(1))
                throw new System.ArgumentException("Coordinates out of range");

            return the_image[x, y];
        }
    }

    public static class color_finder
    {
        public struct color_to_find
        {
            public Color name;
            public hsb_filter filter;
        }
        public struct found_color
        {
            public Color name;
            public Rectangle location;
            public int x
            {
                get
                {
                    if (location == null)
                        throw new NullReferenceException("The found color is currently empty!");

                    return location.X + (location.Width / 2);
                }
            }
            public int y
            {
                get
                {
                    if (location == null)
                        throw new NullReferenceException("The found color is currently empty!");

                    return location.Y + (location.Height / 2);
                }
            }
        }

        public static List<found_color> find_colors(List<color_to_find> colors, Bitmap the_image)
        {
            hsb_image converted_image = new hsb_image(the_image);

            BackgroundWorker[] threads = new BackgroundWorker[colors.Count];
            AutoResetEvent[] thread_done = new AutoResetEvent[colors.Count];
            Bitmap[] copies_of_image = new Bitmap[colors.Count];
            List<found_color> found_colors = new List<found_color>();
            for (int i = 0; i < colors.Count; ++i)
            {
                copies_of_image[i] = AForge.Imaging.Image.Clone(the_image);
                threads[i] = new BackgroundWorker();
                threads[i].DoWork += new DoWorkEventHandler(thread_find_colors);
                thread_done[i] = new AutoResetEvent(false);
                Tuple<Bitmap, hsb_image, color_to_find, List<found_color>, AutoResetEvent> work_data = new Tuple<Bitmap, hsb_image, color_to_find, List<found_color>, AutoResetEvent>(copies_of_image[i], converted_image, colors[i], found_colors, thread_done[i]);
                threads[i].RunWorkerAsync(work_data);
            }

            // Wait until processing is done
            WaitHandle.WaitAll(thread_done);

            // Output
            return found_colors;
        }
        private static void thread_find_colors(object sender, DoWorkEventArgs e)
        {
            Tuple<Bitmap, hsb_image, color_to_find, List<found_color>, AutoResetEvent> work_data = (Tuple<Bitmap, hsb_image, color_to_find, List<found_color>, AutoResetEvent>)e.Argument;
            Bitmap filtered_image = work_data.Item1;
            hsb_image converted_image = work_data.Item2;
            color_to_find c = work_data.Item3;
            List<found_color> output = work_data.Item4;
            AutoResetEvent done = work_data.Item5;
            int width = filtered_image.Width;
            int height = filtered_image.Height;

            // Marshall the image for speed
            Rectangle rect = new Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData bitmap_data = filtered_image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, filtered_image.PixelFormat);
            IntPtr Iptr = bitmap_data.Scan0;
            int step_size = 3;  // 3 bytes per pixel
            int pixel_count = width * height;
            byte[] raw_pixel_bytes = new byte[pixel_count * step_size];
            System.Runtime.InteropServices.Marshal.Copy(Iptr, raw_pixel_bytes, 0, raw_pixel_bytes.Length);

            // Filter image
            int i;
            hsb_image.hsb_pixel p;
            double hue;
            double saturation;
            double brightness;
            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    i = ((y * width) + x) * 3;  // 3 bytes per pixel
                    p = converted_image.get_pixel(x, y);  // Stored here to remove redundant function calls below
                    hue = p.hue;                          // Stored here to remove redundant function calls below
                    saturation = p.saturation;            // Stored here to remove redundant function calls below
                    brightness = p.brightness;            // Stored here to remove redundant function calls below
                    if (c.filter.filter_hue)
                        if (hue < c.filter.min_hue || hue > c.filter.max_hue)
                        {
                            // Set the pixel to black
                            raw_pixel_bytes[i] = 0;
                            raw_pixel_bytes[i + 1] = 0;
                            raw_pixel_bytes[i + 2] = 0;

                            continue;
                        }

                    if (c.filter.filter_saturation)
                        if (saturation < c.filter.min_saturation || saturation > c.filter.max_saturation)
                        {
                            // Set the pixel to black
                            raw_pixel_bytes[i] = 0;
                            raw_pixel_bytes[i + 1] = 0;
                            raw_pixel_bytes[i + 2] = 0;

                            continue;
                        }

                    if (c.filter.filter_brightness)
                        if (brightness < c.filter.min_brightness || brightness > c.filter.max_brightness)
                        {
                            // Set the pixel to black
                            raw_pixel_bytes[i] = 0;
                            raw_pixel_bytes[i + 1] = 0;
                            raw_pixel_bytes[i + 2] = 0;
                        }
                }

            // Marshal the bytes back into the image
            System.Runtime.InteropServices.Marshal.Copy(raw_pixel_bytes, 0, Iptr, raw_pixel_bytes.Length);
            filtered_image.UnlockBits(bitmap_data);

            // Find colors (using BlobCounter of AForge)
            BlobCounterBase blob_counter = new BlobCounter();
            blob_counter.FilterBlobs = true;
            blob_counter.MinWidth = 4;
            blob_counter.MinHeight = 4;
            blob_counter.ObjectsOrder = ObjectsOrder.Size;
            blob_counter.ProcessImage(filtered_image);  // Apply blob filter
            Blob[] blobs = blob_counter.GetObjectsInformation();  // Extract blobs
            if (blobs.Length > 0)  // Something was found
            {
                found_color new_location = new found_color();
                new_location.name = c.name;
                new_location.location = blobs[0].Rectangle;
                lock (output)
                {
                    output.Add(new_location);
                }
            }

            done.Set();  // Report done
        }
    }
}