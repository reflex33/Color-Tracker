using System;
using System.Collections.Generic;

using hsb;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Drawing.Bitmap image = new System.Drawing.Bitmap("test.bmp");
            List<hsb_filter> ctf = new List<hsb_filter>();

            hsb_filter red = new hsb_filter(0.0, 0.05, 0.95, 1.0, 0.95, 1.0);
            red.name = System.Drawing.Color.Red;
            ctf.Add(red);

            hsb_filter blue = new hsb_filter(239, 241, 0.95, 1.0, 0.95, 1.0);
            blue.name = System.Drawing.Color.Blue;
            ctf.Add(blue);

            hsb_filter green = new hsb_filter(119, 121, 0.95, 1.0, 0.95, 1.0);
            green.name = System.Drawing.Color.Green;
            ctf.Add(green);

            hsb_filter magenta = new hsb_filter(299, 301, 0.95, 1.0, 0.95, 1.0);
            magenta.name = System.Drawing.Color.Magenta;
            ctf.Add(magenta);

            hsb_filter white = new hsb_filter(0.0, 0.05, 0.0, 0.05, 0.98, 1.0);
            white.name = System.Drawing.Color.White;
            ctf.Add(white);

            List<blob> output = new List<blob>();
            hsb_image converted_image = new hsb_image();

            try
            {
                DateTime t1 = DateTime.Now;
                for (int i = 0; i < 30; ++i)
                {
                    converted_image.set_image(image);
                    output = converted_image.find_colors(ctf);
                }
                DateTime t2 = DateTime.Now;
                Console.WriteLine(t2 - t1);
            }
            catch (System.ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
