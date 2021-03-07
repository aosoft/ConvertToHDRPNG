using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertToHDRPNG
{
    readonly struct Color
    {
        public float R { get; }
        public float G { get; }
        public float B { get; }

        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static Color operator +(in Color a, in Color b) => new Color(a.R + b.R, a.G + b.G, a.B + b.B);

        public Color Rec709ToRec2020() => new Color(
            0.627402f * R + 0.329292f * G + 0.043306f * B,
            0.069095f * R + 0.919544f * G + 0.011360f * B,
            0.016394f * R + 0.088028f * G + 0.895578f * B);

        private const double MagickScale = 65535.0;
        private const double HdrScale = 80.0 / 10000.0;

        public Color LinearToST2084()
        {
            static double fn(double color)
            {
                color /= MagickScale;
                var m1 = 2610.0 / 4096.0 / 4;
                var m2 = 2523.0 / 4096.0 * 128;
                var c1 = 3424.0 / 4096.0;
                var c2 = 2413.0 / 4096.0 * 32;
                var c3 = 2392.0 / 4096.0 * 32;
                var cp = Math.Pow(Math.Abs(color * HdrScale), m1);
                return Math.Pow((c1 + c2 * cp) / (1 + c3 * cp), m2) * MagickScale;
            }

            return new Color((float)fn(R), (float)fn(G), (float)fn(B));
        }
    }
}
