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
            List<color_finder.search_color> ctf = new List<color_finder.search_color>();

            color_finder.search_color red = new color_finder.search_color();
            red.filter = new hsb_filter(0.0, 0.05, 0.95, 1.0, 0.95, 1.0);
            red.name = System.Drawing.Color.Red;
            ctf.Add(red);

            color_finder.search_color blue = new color_finder.search_color();
            blue.filter = new hsb_filter(239, 241, 0.95, 1.0, 0.95, 1.0);
            blue.name = System.Drawing.Color.Blue;
            ctf.Add(blue);

            color_finder.search_color green = new color_finder.search_color();
            green.filter = new hsb_filter(119, 121, 0.95, 1.0, 0.95, 1.0);
            green.name = System.Drawing.Color.Green;
            ctf.Add(green);

            color_finder.search_color magenta = new color_finder.search_color();
            magenta.filter = new hsb_filter(299, 301, 0.95, 1.0, 0.95, 1.0);
            magenta.name = System.Drawing.Color.Magenta;
            ctf.Add(magenta);

            color_finder.search_color white = new color_finder.search_color();
            white.filter = new hsb_filter(0.0, 0.05, 0.0, 0.05, 0.98, 1.0);
            white.name = System.Drawing.Color.White;
            ctf.Add(white);

            List<color_finder.found_color> output = new List<color_finder.found_color>();

            try
            {
                DateTime t1 = DateTime.Now;
                for (int i = 0; i < 30; ++i)
                    output = color_finder.find_colors(ctf, image);
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
