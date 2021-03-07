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

        public unsafe void Convert(string input, string output)
        {
            using var src = new MagickImage(input);
            int width = src.Width;
            int height = src.Height;
            var srcPixels = src.GetPixelsUnsafe();
            Process(output, width, height, (dstPixels, bt2100pq) =>
            {
                Func<Color, Color> fn = bt2100pq ? src => src.Rec709ToRec2020().LinearToST2084() : src => src;
                Parallel.For(0, height, y =>
                {
                    var src = (float*)srcPixels.GetAreaPointer(0, y, width, 1).ToPointer();
                    var srcChannels = srcPixels.Channels;
                    var dst = (float*)dstPixels.GetAreaPointer(0, y, width, 1).ToPointer();
                    var dstChannels = dstPixels.Channels;
                    for (int x = 0; x < width; x++)
                    {
                        var dstColor = fn(new Color(src[0], src[1], src[2]));

                        dst[0] = dstColor.R;
                        dst[1] = dstColor.G;
                        dst[2] = dstColor.B;
                        src += srcChannels;
                        dst += dstChannels;
                    }
                });
            });
        }

        public unsafe void Gradation(string output, int width = 1024, int height = 1024, float start = 0.0f, float end = 1.0f) =>
            Process(output, width, height, (dstPixels, bt2100pq) =>
            {
                Parallel.For(0, height, y =>
                {
                    var dst = (float*)dstPixels.GetAreaPointer(0, y, width, 1).ToPointer();
                    var dstChannels = dstPixels.Channels;
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
                        dst[0] = dstColor.R;
                        dst[1] = dstColor.G;
                        dst[2] = dstColor.B;
                        dst += dstChannels;
                    }
                });
            });

        private void Process(string output, int width, int height, Action<IUnsafePixelCollection<float>, bool> fn)
        {
            using var dst = new MagickImage(MagickColor.FromRgb(0, 0, 0), width, height);
            bool exr = string.Equals(".exr", System.IO.Path.GetExtension(output), StringComparison.OrdinalIgnoreCase);

            dst.ColorSpace = exr ? ColorSpace.RGB : ColorSpace.sRGB;
            fn(dst.GetPixelsUnsafe(), !exr);
            dst.Write(output, exr ? MagickFormat.Exr : MagickFormat.Png48);
        }

    }
}
