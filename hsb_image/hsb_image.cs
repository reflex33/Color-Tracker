using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Threading;

namespace hsb_image
{
    public struct hsb_pixel
    {
        public double hue;
        public double saturation;
        public double brightness;
    }
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

        public bool is_empty
        {
            get { return (!filter_hue && !filter_saturation && !filter_brightness); }
        }
    }
    public class hsb_image
    {
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
        public class search_color
        {
            public Color name;
            public hsb_filter filter = new hsb_filter();
        }
        public struct found_color
        {
            public Color name;
            public Rectangle location;
            public int x
            {
                get
                {
                    if (location == null || location.IsEmpty)
                        throw new NullReferenceException("The found color is currently empty!");

                    return location.X + (location.Width / 2);
                }
            }
            public int y
            {
                get
                {
                    if (location == null || location.IsEmpty)
                        throw new NullReferenceException("The found color is currently empty!");

                    return location.Y + (location.Height / 2);
                }
            }
        }
        private class blob
        {
            public int label;
            public int num_of_pixels = 0;
            public int min_x = -1;
            public int min_y = -1;
            public int max_x = -1;
            public int max_y = -1;
        }

        public static List<found_color> find_colors(List<search_color> colors, Bitmap the_image)
        {
            hsb_image converted_image = new hsb_image(the_image);

            // Check for errors in the colors to find, throw exceptions here so "user" can consume them (errors in the threading below can't throw to the user)
            foreach (search_color color_to_find in colors)
            {
                if (color_to_find.name.IsEmpty || color_to_find.filter.is_empty)
                    throw new System.ArgumentException("Filters must not be empty!");
            }

            BackgroundWorker[] threads = new BackgroundWorker[colors.Count];
            AutoResetEvent[] thread_done = new AutoResetEvent[colors.Count];
            Bitmap[] copies_of_image = new Bitmap[colors.Count];
            List<found_color> found_colors = new List<found_color>();
            for (int i = 0; i < colors.Count; ++i)
            {
                threads[i] = new BackgroundWorker();
                threads[i].DoWork += new DoWorkEventHandler(thread_find_colors);
                thread_done[i] = new AutoResetEvent(false);
                Tuple<hsb_image, search_color, List<found_color>, AutoResetEvent> work_data = new Tuple<hsb_image, search_color, List<found_color>, AutoResetEvent>(converted_image, colors[i], found_colors, thread_done[i]);
                threads[i].RunWorkerAsync(work_data);
            }

            // Wait until processing is done
            WaitHandle.WaitAll(thread_done);

            // Output
            return found_colors;
        }
        private static void thread_find_colors(object sender, DoWorkEventArgs e)
        {
            Tuple<hsb_image, search_color, List<found_color>, AutoResetEvent> work_data = (Tuple<hsb_image, search_color, List<found_color>, AutoResetEvent>)e.Argument;
            hsb_image image = work_data.Item1;
            search_color color = work_data.Item2;
            List<found_color> output = work_data.Item3;
            AutoResetEvent done = work_data.Item4;
            int width = image.width;
            int height = image.height;

            // Filter image and populate blob buffer
            hsb_image.hsb_pixel p;
            double hue;
            double saturation;
            double brightness;
            int[,] label_buffer = new int[width, height];
            int label = 1;
            List<int> label_table = new List<int>();
            label_table.Add(-1);  // Add a dummy label at the '0' position... makes stuff easier later and matches the notes for blob detection
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                {
                    p = image.get_pixel(x, y);  // Stored here to remove redundant function calls below
                    hue = p.hue;                          // Stored here to remove redundant function calls below
                    saturation = p.saturation;            // Stored here to remove redundant function calls below
                    brightness = p.brightness;            // Stored here to remove redundant function calls below
                    if (color.filter.filter_hue)
                        if (hue < color.filter.min_hue || hue > color.filter.max_hue)
                        {
                            label_buffer[x, y] = 0;  // Set the label buffer "pixel" to 0
                            continue;
                        }

                    if (color.filter.filter_saturation)
                        if (saturation < color.filter.min_saturation || saturation > color.filter.max_saturation)
                        {
                            label_buffer[x, y] = 0;  // Set the label buffer "pixel" to 0
                            continue;
                        }

                    if (color.filter.filter_brightness)
                        if (brightness < color.filter.min_brightness || brightness > color.filter.max_brightness)
                        {
                            label_buffer[x, y] = 0;// Set the label buffer "pixel" to 0
                            continue;
                        }

                    //// If we get to this point, the pixel is not filtered out, and therefore should needs a label in the label buffer ////

                    // Get the four "pixels" for the label kernel
                    int A_pixel;
                    if (x - 1 < 0 || y - 1 < 0)  // A_pixel is off the image
                        A_pixel = 0;
                    else
                        A_pixel = label_buffer[x - 1, y - 1];
                    int B_pixel;
                    if (y - 1 < 0)  // B_pixel is off the image
                        B_pixel = 0;
                    else
                        B_pixel = label_buffer[x, y - 1];
                    int C_pixel;
                    if (x + 1 > width - 1 || y - 1 < 0)  // C_pixel is off the image
                        C_pixel = 0;
                    else
                        C_pixel = label_buffer[x + 1, y - 1];
                    int D_pixel;
                    if (x - 1 < 0)  // D_pixel is off the image
                        D_pixel = 0;
                    else
                        D_pixel = label_buffer[x - 1, y];

                    // Apply label kernel
                    if (A_pixel == 0 && B_pixel == 0 && C_pixel == 0 && D_pixel == 0)  // New label
                    {
                        label_buffer[x, y] = label;
                        label_table.Add(label);
                        ++label;
                    }
                    else
                    {
                        // Find the lowest label number that isn't zero
                        int calculated_label = int.MaxValue;
                        if (A_pixel != 0)
                            calculated_label = A_pixel;
                        if (B_pixel != 0 && B_pixel < calculated_label)
                            calculated_label = B_pixel;
                        if (C_pixel != 0 && C_pixel < calculated_label)
                            calculated_label = C_pixel;
                        if (D_pixel != 0 && D_pixel < calculated_label)
                            calculated_label = D_pixel;

                        // The "pixel" in the buffer is the calculated value.
                        // Also update the label table indicating if two blobs are connected
                        label_buffer[x, y] = calculated_label;
                        label_table[A_pixel] = calculated_label;
                        label_table[B_pixel] = calculated_label;
                        label_table[C_pixel] = calculated_label;
                        label_table[D_pixel] = calculated_label;
                    }
                }

            // Reduce label table
            for (int i = 1; i < label_table.Count; ++i)
            {
                if (i == label_table[i])
                    continue;

                int next_label_entry_to_check = label_table[i];
                while (next_label_entry_to_check != label_table[next_label_entry_to_check])
                    next_label_entry_to_check = label_table[next_label_entry_to_check];

                label_table[i] = next_label_entry_to_check;
            }

            // Find blobs
            List<blob> blobs = new List<blob>();
            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    if (label_buffer[x, y] == 0)
                        continue;

                    int reduced_label = label_table[label_buffer[x, y]];  // Which label does this pixel reduce to?

                    int blob_owner = -1;
                    for (int i = 0; i < blobs.Count; ++i)
                        if (blobs[i].label == reduced_label)
                            blob_owner = i;

                    if (blob_owner == -1)  // New blob
                    {
                        blob new_blob = new blob();
                        ++new_blob.num_of_pixels;
                        new_blob.min_x = x;
                        new_blob.min_y = y;
                        new_blob.max_x = x;
                        new_blob.max_y = y;
                        new_blob.label = reduced_label;
                        blobs.Add(new_blob);
                    }
                    else
                    {
                        ++blobs[blob_owner].num_of_pixels;
                        if (x < blobs[blob_owner].min_x)
                            blobs[blob_owner].min_x = x;
                        if (y < blobs[blob_owner].min_y)
                            blobs[blob_owner].min_y = y;
                        if (x > blobs[blob_owner].max_x)
                            blobs[blob_owner].max_x = x;
                        if (y > blobs[blob_owner].max_y)
                            blobs[blob_owner].max_y = y;
                    }
                }

            // Find the largest blob
            int largest_blob = -1;
            int largest_blob_pixels = -1;
            for (int i = 0; i < blobs.Count; ++i)
                if (blobs[i].num_of_pixels > largest_blob_pixels)
                    largest_blob = i;
            if (largest_blob >= 0)
            {
                Rectangle r = new Rectangle();
                r.X = blobs[largest_blob].min_x;
                r.Y = blobs[largest_blob].min_y;
                r.Width = blobs[largest_blob].max_x - r.X + 1;
                r.Height = blobs[largest_blob].max_y - r.Y + 1;
                found_color new_location = new found_color();
                new_location.name = color.name;
                new_location.location = r;
                lock (output)
                {
                    output.Add(new_location);
                }
            }

            done.Set();  // Report done
        }
    }
}