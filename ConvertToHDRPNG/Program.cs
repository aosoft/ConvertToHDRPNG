using System;
using System.Threading.Tasks;
using Cocona;
using ImageMagick;


namespace ConvertToHDRPNG
{
    class Program
    {
        static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        public void Convert(string input, string output)
        {
        }

        public void Gradation(string output, int width = 1024, int height = 1024, float start = 0.0f, float end = 1.0f) =>
            Process(output, width, height, (dstPixels, bt2100pq) =>
            {
                Parallel.For(0, height, y =>
                {
                    var yy = (float)y / height;
                    Func<float, Color> fn;
                    Func<float, Color> fn2;
                    if (yy < 0.25)
                    {
                        fn2 = f => new Color(f, 0, 0);
                    }
                    else if (yy < 0.5)
                    {
                        fn2 = f => new Color(0, f, 0);
                    }
                    else if (yy < 0.75)
                    {
                        fn2 = f => new Color(0, 0, f);
                    }
                    else
                    {
                        fn2 = f => new Color(f, f, f);
                    }
                    if (bt2100pq)
                    {
                        fn = f => fn2(f).Rec709ToRec2020().LinearToST2084();
                    }
                    else
                    {
                        fn = fn2;
                    }
                    for (int x = 0; x < width; x++)
                    {
                        var dstColor = fn((start + x * (end - start) / width) * 65535.0f);
                        dstPixels.SetPixel(new Pixel(x, y, new float[] { dstColor.R, dstColor.G, dstColor.B }));
                    }
                });
            });

        private void Process(string output, int width, int height, Action<IPixelCollection<float>, bool> fn)
        {
            using var dst = new MagickImage(MagickColor.FromRgb(0, 0, 0), width, height);
            bool exr = string.Equals(".exr", System.IO.Path.GetExtension(output), StringComparison.OrdinalIgnoreCase);

            dst.ColorSpace = exr ? ColorSpace.RGB : ColorSpace.sRGB;
            fn(dst.GetPixels(), !exr);
            dst.Write(output, exr ? MagickFormat.Exr : MagickFormat.Png48);
        }

    }
}
