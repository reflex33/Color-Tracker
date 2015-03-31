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
            color_finder f = new color_finder();
            color_finder.hsb_filter red = new color_finder.hsb_filter(0.0, 0.05, 0.95, 1.0, 0.95, 1.0);
            color_finder.hsb_filter blue = new color_finder.hsb_filter(239, 241, 0.95, 1.0, 0.95, 1.0);
            color_finder.hsb_filter green = new color_finder.hsb_filter(119, 121, 0.95, 1.0, 0.95, 1.0);
            color_finder.hsb_filter magenta = new color_finder.hsb_filter(299, 301, 0.95, 1.0, 0.95, 1.0);
            color_finder.hsb_filter white = new color_finder.hsb_filter(0.0, 0.05, 0.0, 0.05, 0.98, 1.0);
            f.add_color(System.Drawing.Color.Red, red);
            f.add_color(System.Drawing.Color.Blue, blue);
            f.add_color(System.Drawing.Color.Green, green);
            f.add_color(System.Drawing.Color.Magenta, magenta);
            f.add_color(System.Drawing.Color.White, white);

            List<color_finder.found_color_center> output_centers = new List<color_finder.found_color_center>();

            DateTime t1 = DateTime.Now;
            f.find_colors(out output_centers, image);
            DateTime t2 = DateTime.Now;
            Console.WriteLine(t2 - t1);
        }
    }
}
