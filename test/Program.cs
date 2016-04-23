using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using tracking;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Drawing.Bitmap image = new System.Drawing.Bitmap("test.bmp");
            List<color_finder.color_to_find> ctf = new List<color_finder.color_to_find>();

            color_finder.color_to_find red = new color_finder.color_to_find();
            red.filter = new hsb_filter(0.0, 0.05, 0.95, 1.0, 0.95, 1.0);
            red.name = System.Drawing.Color.Red;
            ctf.Add(red);

            color_finder.color_to_find blue = new color_finder.color_to_find();
            blue.filter = new hsb_filter(239, 241, 0.95, 1.0, 0.95, 1.0);
            blue.name = System.Drawing.Color.Blue;
            ctf.Add(blue);

            color_finder.color_to_find green = new color_finder.color_to_find();
            green.filter = new hsb_filter(119, 121, 0.95, 1.0, 0.95, 1.0);
            green.name = System.Drawing.Color.Green;
            ctf.Add(green);

            color_finder.color_to_find magenta = new color_finder.color_to_find();
            magenta.filter = new hsb_filter(299, 301, 0.95, 1.0, 0.95, 1.0);
            magenta.name = System.Drawing.Color.Magenta;
            ctf.Add(magenta);

            color_finder.color_to_find white = new color_finder.color_to_find();
            white.filter = new hsb_filter(0.0, 0.05, 0.0, 0.05, 0.98, 1.0);
            white.name = System.Drawing.Color.White;
            ctf.Add(white);

            List<color_finder.found_color> output = new List<color_finder.found_color>();

            DateTime t1 = DateTime.Now;
            for(int i = 0; i < 30; ++i)
                output = color_finder.find_colors(ctf, image);
            DateTime t2 = DateTime.Now;
            Console.WriteLine(t2 - t1);
        }
    }
}
