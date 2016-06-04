using SharpFont;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Test
{
    class Program
    {
        const string ComparisonPath = "../../../../font_rasters/";

        static void Main(string[] args)
        {
            using (var file = File.OpenRead("../../../Fonts/OpenSans-Regular.ttf"))
            {
                var typeface = new FontFace(file);

                for (int c = 0; c <= char.MaxValue; c++)
                {
                    Console.WriteLine("Dump {0}: {1}", c, (char)c);
                    var comparisonFile = Path.Combine(ComparisonPath, c + ".png");
                    Surface surface = RenderGlyph(typeface, (char)c, 32);
                    SaveSurface(surface, comparisonFile);
                }
            }
        }

        static unsafe Surface RenderGlyph(FontFace typeface, char c, float pixelSize)
        {
            Surface surface = new Surface();

            var glyph = typeface.GetGlyph(c, pixelSize);
            if (glyph != null)
            {
                surface = new Surface
                {
                    Bits = Marshal.AllocHGlobal(glyph.RenderWidth * glyph.RenderHeight),
                    Width = glyph.RenderWidth,
                    Height = glyph.RenderHeight,
                    Pitch = glyph.RenderWidth
                };

                var stuff = (byte*)surface.Bits;
                for (int i = 0; i < surface.Width * surface.Height; i++)
                    *stuff++ = 0;

                glyph.RenderTo(surface);
            }

            return surface;
        }

        static unsafe void SaveSurface(Surface surface, string fileName)
        {
            if (surface.Width > 0 && surface.Height > 0)
            {
                var bitmap = new Bitmap(surface.Width, surface.Height, PixelFormat.Format24bppRgb);
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, surface.Width, surface.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                for (int y = 0; y < surface.Height; y++)
                {
                    var dest = (byte*)bitmapData.Scan0 + y * bitmapData.Stride;
                    var src = (byte*)surface.Bits + y * surface.Pitch;

                    for (int x = 0; x < surface.Width; x++)
                    {
                        var b = *src++;
                        *dest++ = b;
                        *dest++ = b;
                        *dest++ = b;
                    }
                }

                bitmap.UnlockBits(bitmapData);
                bitmap.Save(fileName);
                bitmap.Dispose();
                Marshal.FreeHGlobal(surface.Bits);
            }
        }

    }
}
