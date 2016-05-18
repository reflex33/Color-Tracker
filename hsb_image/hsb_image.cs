using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Threading;

namespace hsb
{
    public class blob
    {
        public int num_of_pixels { get; internal set; } = 0;
        public string name { get; internal set; }
        public Rectangle rectangle
        {
            get
            {
                if (num_of_pixels <= 0)
                    throw new NullReferenceException("The blob is currently empty!");

                return new Rectangle(left, top, width, height);
            }
        }
        public int center_x
        {
            get
            {
                if (num_of_pixels <= 0)
                    throw new NullReferenceException("The blob is currently empty!");

                return (left + width / 2);
            }
        }
        public int center_y
        {
            get
            {
                if (num_of_pixels <= 0)
                    throw new NullReferenceException("The blob is currently empty!");

                return (top + height / 2);
            }
        }
        public int width
        {
            get { return (right - left + 1); }
        }
        public int height
        {
            get { return (bottom - top + 1); }
        }

        internal int label;
        public int left { get; internal set; }
        public int top { get; internal set; }
        public int right { get; internal set; }
        public int bottom { get; internal set; }
    }
    public struct hsb_pixel
    {
        public double hue;
        public double saturation;
        public double brightness;
    }
    public class hsb_filter
    {
        public string name;

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
            get { return (name == null || (!filter_hue && !filter_saturation && !filter_brightness)); }
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

            int NUM_OF_THREADS = Environment.ProcessorCount;
            width = input_image.Width;
            height = input_image.Height;

            // Marshall the image for speed
            Rectangle rect = new Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData bitmap_data = input_image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, input_image.PixelFormat);
            IntPtr Iptr = bitmap_data.Scan0;
            int image_stride = bitmap_data.Stride;
            raw_input_pixel_bytes = new byte[image_stride * height];
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
                Tuple<int, int, int, AutoResetEvent> work_data;
                if (i == NUM_OF_THREADS - 1)  // Make sure to get all of the rows of the image in the last thread, fixes if we have a non-whole-number of rows after dividing up the work
                    work_data = new Tuple<int, int, int, AutoResetEvent>(i * num_of_lines, height, image_stride, thread_done[i]);
                else
                    work_data = new Tuple<int, int, int, AutoResetEvent>(i * num_of_lines, (i + 1) * num_of_lines, image_stride, thread_done[i]);
                threads[i].RunWorkerAsync(work_data);
            }

            // Wait until processing is done
            WaitHandle.WaitAll(thread_done);
        }
        private void thread_convert_image(object sender, DoWorkEventArgs e)
        {
            Tuple<int, int, int, AutoResetEvent> work_data = (Tuple<int, int, int, AutoResetEvent>)e.Argument;
            int start_row = work_data.Item1;
            int end_row = work_data.Item2;  // Don't actually process this row, stop one short of it
            int stride = work_data.Item3;
            AutoResetEvent done = work_data.Item4;

            for (int x = 0; x < width; ++x)
                for (int y = work_data.Item1; y < work_data.Item2; ++y)
                {
                    int i = ((y * stride) + x * 3);  // 3 bytes per pixel
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

        /// <summary>
        /// Finds colors within the image.  Only returns the largest blob (by number of pixels) of the that color.
        /// </summary>
        /// <param name="colors">The colors to search for</param>
        /// <returns>The largest blob (by number of pixels) of each color</returns>
        public List<blob> find_colors(List<hsb_filter> colors)
        {
            if (the_image == null)
                throw new NullReferenceException("Image is empty!");

            // Check for errors in the colors to find, throw exceptions here so "user" can consume them (errors in the threading below can't throw to the user)
            foreach (hsb_filter color_to_find in colors)
            {
                if (color_to_find.is_empty)
                    throw new System.ArgumentException("Filters must not be empty!");
            }

            BackgroundWorker[] threads = new BackgroundWorker[colors.Count];
            AutoResetEvent[] thread_done = new AutoResetEvent[colors.Count];
            Bitmap[] copies_of_image = new Bitmap[colors.Count];
            List<blob> found_colors = new List<blob>();
            for (int i = 0; i < colors.Count; ++i)
            {
                threads[i] = new BackgroundWorker();
                threads[i].DoWork += new DoWorkEventHandler(thread_find_colors);
                thread_done[i] = new AutoResetEvent(false);
                Tuple<hsb_filter, List<blob>, AutoResetEvent> work_data = new Tuple<hsb_filter, List<blob>, AutoResetEvent>(colors[i], found_colors, thread_done[i]);
                threads[i].RunWorkerAsync(work_data);
            }

            // Wait until processing is done
            WaitHandle.WaitAll(thread_done);

            // Output
            return found_colors;
        }
        private void thread_find_colors(object sender, DoWorkEventArgs e)
        {
            Tuple<hsb_filter, List<blob>, AutoResetEvent> work_data = (Tuple<hsb_filter, List<blob>, AutoResetEvent>)e.Argument;
            hsb_filter color = work_data.Item1;
            List<blob> output = work_data.Item2;
            AutoResetEvent done = work_data.Item3;

            // Filter image and populate blob buffer
            hsb_pixel p;
            double hue;
            double saturation;
            double brightness;
            int[,] label_buffer = new int[width, height];
            int label = 1;
            List<int> label_table = new List<int>();
            label_table.Add(-1);  // Add a dummy label at the '0' position... makes stuff easier later and matches the notes for blob detection
            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    p = get_pixel(x, y);        // Stored here to remove redundant function calls below
                    hue = p.hue;                // Stored here to remove redundant function calls below
                    saturation = p.saturation;  // Stored here to remove redundant function calls below
                    brightness = p.brightness;  // Stored here to remove redundant function calls below
                    if (color.filter_hue)
                        if (hue < color.min_hue || hue > color.max_hue)
                        {
                            label_buffer[x, y] = 0;  // Set the label buffer "pixel" to 0
                            continue;
                        }

                    if (color.filter_saturation)
                        if (saturation < color.min_saturation || saturation > color.max_saturation)
                        {
                            label_buffer[x, y] = 0;  // Set the label buffer "pixel" to 0
                            continue;
                        }

                    if (color.filter_brightness)
                        if (brightness < color.min_brightness || brightness > color.max_brightness)
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
                    if (x - 1 < 0)  // B_pixel is off the image
                        B_pixel = 0;
                    else
                        B_pixel = label_buffer[x - 1, y];
                    int C_pixel;
                    if (x - 1 < 0 || y + 1 >= height)  // C_pixel is off the image
                        C_pixel = 0;
                    else
                        C_pixel = label_buffer[x - 1, y + 1];
                    int D_pixel;
                    if (y - 1 < 0)  // D_pixel is off the image
                        D_pixel = 0;
                    else
                        D_pixel = label_buffer[x, y - 1];

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
                        new_blob.name = color.name;
                        new_blob.num_of_pixels = 1;
                        new_blob.left = x;
                        new_blob.top = y;
                        new_blob.right = x;
                        new_blob.bottom = y;
                        new_blob.label = reduced_label;
                        blobs.Add(new_blob);
                    }
                    else
                    {
                        ++blobs[blob_owner].num_of_pixels;
                        if (x < blobs[blob_owner].left)
                            blobs[blob_owner].left = x;
                        if (y < blobs[blob_owner].top)
                            blobs[blob_owner].top = y;
                        if (x > blobs[blob_owner].right)
                            blobs[blob_owner].right = x;
                        if (y > blobs[blob_owner].bottom)
                            blobs[blob_owner].bottom = y;
                    }
                }

            // Find the largest blob
            int largest_blob = -1;
            int largest_blob_pixels = -1;
            for (int i = 0; i < blobs.Count; ++i)
                if (blobs[i].num_of_pixels > largest_blob_pixels)
                {
                    largest_blob = i;
                    largest_blob_pixels = blobs[i].num_of_pixels;
                }
            if (largest_blob >= 0)
            {
                lock (output)
                {
                    output.Add(blobs[largest_blob]);
                }
            }

            done.Set();  // Report done
        }
    }
}