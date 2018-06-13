using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace File2BMP
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BitmapFileHeader
        {
            public ushort bfType;
            public ulong bfSize;
            //public ushort bfReserved1;
            //public ushort bfReserved2;
            public uint bfOffBits;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BitmapInfoHeader
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }


        static void Main(string[] args)
        {
            Console.WriteLine("File2BMP v1.0");
            Console.WriteLine();

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: File2BMP.exe <files>");
                Console.WriteLine("*.* -> *.*.bmp");
                Console.WriteLine("*.*.bmp -> *.*");
                return;
            }
            foreach (var s in args)
            {
                var filename = s;
                Console.WriteLine($"Opening {filename}");

                if (!File.Exists(filename))
                {
                    Console.WriteLine("Error: Can't open file.");
                    continue;
                }

                var fi = new FileInfo(filename);
                var fileSize = fi.Length;
                if (fileSize == 0)
                {
                    Console.WriteLine("Error: No contents.");
                    continue;
                }

                if (filename.ToLower().EndsWith(".bmp"))
                {
                    Console.WriteLine("Restoring from BMP...");
                    using (var fs = new FileStream(Environment.CurrentDirectory + "/" + fi.Name.Remove(fi.Name.Length - 4), FileMode.Create))
                    using (var src = new FileStream(filename, FileMode.Open))
                    {
                        src.Seek(2, SeekOrigin.Begin);
                        var buffer = new byte[8];
                        src.Read(buffer, 0, 8);
                        var size = BitConverter.ToUInt64(buffer, 0);

                        src.Seek(54, SeekOrigin.Begin);
                        src.CopyTo(fs);
                        fs.SetLength((long)size);
                        fs.Flush();
                    }
                }
                else
                {
                    Console.WriteLine("Converting to BMP...");
                    const int bytesPerPixel = 4;
                    var bmpDataSize = ((fileSize - 1) / bytesPerPixel + 1) * bytesPerPixel;
                    var height = (int)Math.Sqrt(bmpDataSize / bytesPerPixel);
                    var width = (int)(height + (bmpDataSize / bytesPerPixel - height * height - 1) / height + 1);
                    using (var fs = new FileStream(Environment.CurrentDirectory + "/" + fi.Name + ".bmp", FileMode.Create))
                    using (var src = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        fs.Write(MakeBitmapFileHeader((ulong)fileSize), 0, 14);
                        fs.Write(MakeBitmapInfoHeader(width, height, bytesPerPixel * 8), 0, 40);
                        fs.Flush();
                        src.CopyTo(fs);
                        fs.Flush();
                        //Padding
                        var padding = height * width * bytesPerPixel - fileSize;
                        for (var i = 0; i < padding; i++) fs.WriteByte(0);
                        fs.Flush();
                    }
                }
                Console.WriteLine("Complete.\r");
            }
        }

        static unsafe byte[] MakeBitmapFileHeader(ulong fileSize)
        {
            var header = new BitmapFileHeader
            {
                bfType = 0x4D42,
                bfSize = fileSize,
                //bfReserved1 = 0,
                //bfReserved2 = 0,
                bfOffBits = 54,
            };

            const int size = 14;
            var bytes = new byte[size];
            fixed (byte* pbytes = bytes)
            {
                *(BitmapFileHeader*)pbytes = header;
            }
            return bytes;
        }

        static unsafe byte[] MakeBitmapInfoHeader(int width, int height, ushort bitCount)
        {
            var header = new BitmapInfoHeader
            {
                biSize = 40,
                biWidth = width,
                biHeight = height,
                biPlanes = 1,
                biBitCount = bitCount,
                biCompression = 0,
                biSizeImage = 0,
                biXPelsPerMeter = 0,
                biYPelsPerMeter = 0,
                biClrUsed = 0,
                biClrImportant = 0
            };

            const int size = 40;
            var bytes = new byte[size];
            fixed (byte* pbytes = bytes)
            {
                *(BitmapInfoHeader*)pbytes = header;
            }
            return bytes;
        }
    }
}
