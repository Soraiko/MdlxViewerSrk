using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MdlxViewer
{
    public class SrkBinary
    {
        public static List<DateTime> filesUniqueIDs = new List<DateTime>(0);
        public static List<string> filesFname = new List<string>(0);

        public static List<byte[]> filesBytes = new List<byte[]>(0);
        public static List<Texture2D> filesT2D = new List<Texture2D>(0);


        public static byte[] GetBytesArray(string fname)
        {
            byte[] output = EmptyFile;
            if (File.Exists(fname))
            {
                DateTime currTD = File.GetLastWriteTime(fname);
                int index = filesFname.IndexOf(fname);
                if (index > -1)
                {
                    DateTime lastTD = filesUniqueIDs[index];
                    if ((lastTD - currTD).ToString()[0] == '-')
                    {
                        FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        output = new byte[fs.Length];
                        fs.Read(output, 0, output.Length);
                        fs.Close();
                    }
                    else
                    {
                        output = filesBytes[index];
                    }
                }
                else
                {
                    FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    output = new byte[fs.Length];
                    fs.Read(output, 0, output.Length);
                    filesFname.Add(fname);
                    filesUniqueIDs.Add(currTD);
                    filesBytes.Add(output);
                    filesT2D.Add(EmptyT2D);
                    fs.Close();
                }
            }
            return output;
        }
        public static System.Drawing.Bitmap EmptyBMP;
        public static Texture2D EmptyT2D;
        public static byte[] EmptyFile;

        public static void InitEmptyData()
        {
            EmptyBMP = new System.Drawing.Bitmap(1, 1);
            EmptyBMP.SetPixel(0, 0, System.Drawing.Color.FromArgb(255,255,255));
            EmptyT2D = new Texture2D(Program.MainView.graphics.GraphicsDevice, 1, 1);
            EmptyT2D.SetData<Color>(new Color[] { new Color(255, 255, 255)});
            EmptyFile = new byte[1024]; 
        }

        public static Texture2D GetT2D(string fname)
        {
            Texture2D output = EmptyT2D;
            if (View.AllowTextures || fname.Contains("boneTexture"))
            if (File.Exists(fname))
            {
                DateTime currTD = File.GetLastWriteTime(fname);
                int index = filesFname.IndexOf(fname);
                if (index > -1)
                {
                    DateTime lastTD = filesUniqueIDs[index];
                    if ((lastTD - currTD).ToString()[0]=='-')
                    {
                        FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        if (GetImageFormat(fs) != ImageFormat.unknown)
                        {
                            filesUniqueIDs[index] = currTD;
                            output = Texture2D.FromStream(Program.MainView.graphics.GraphicsDevice, fs);
                            filesT2D[index] = output;
                        }
                        fs.Close();
                    }
                    else
                    {
                        output = filesT2D[index];
                    }
                }
                else
                {
                    FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    if (GetImageFormat(fs)!=ImageFormat.unknown)
                    {
                        output = Texture2D.FromStream(Program.MainView.graphics.GraphicsDevice, fs);
                        filesFname.Add(fname);
                        filesUniqueIDs.Add(currTD);
                        filesBytes.Add(EmptyFile);
                        filesT2D.Add(output);
                    }
                    fs.Close();
                }
            }
            return output;
        }

        public enum ImageFormat
        {
            bmp,
            jpeg,
            gif,
            tiff,
            png,
            unknown
        }

        public static ImageFormat GetImageFormat(FileStream fs)
        {
            byte[] bytes = new byte[32];

            if (fs.Length>=32)
            {
                fs.Read(bytes,0,32);
            }

            int notZeroCount = 0;
            for (int i=0;i< bytes.Length;i++)
            {
                if (bytes[i]==0)
                {
                    notZeroCount++;
                }
            }
            if (notZeroCount == 32)
                return ImageFormat.unknown;
            // see http://www.mikekunz.com/image_file_header.html  
            var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat.tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat.tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.jpeg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.jpeg;

            return ImageFormat.unknown;
        }

        public static byte[] lastBuffer = new byte[0];
        public static string lastBufferFilename = "";
        public bool DataEdited;

        public byte[] Buffer
        {
            get;
            set;
        }

        public static void Align16(ref int valeur)
        {
            while (valeur % 16 > 0)
                valeur++;
        }

        public static void AlignBy(ref int valeur, int by)
        {
            while (valeur % by > 0)
                valeur++;
        }

        public static bool GetBytes(string filename)
        {
            FileStream fileStream;
            lastBufferFilename = "";
            try
            {
                fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                lastBufferFilename = filename;
            }
            catch (IOException)
            {
                //Console.WriteLine("Unable to open " + Path.GetFileName(filename) + ".");
                //Console.WriteLine("This file in being used by another process.");
                return false;
            }
            lastBuffer = new byte[fileStream.Length];
            fileStream.Read(lastBuffer, 0, lastBuffer.Length);
            fileStream.Close();
            return lastBuffer.Length > 0;
        }

        public SrkBinary()
        {
            this.Buffer = lastBuffer;
        }

        public SrkBinary(ref byte[] buffer)
        {
            this.Buffer = buffer;
        }

        public Int32 Pointer
        {
            get; set;
        }

        /*public unsafe System.Int16 ReadShort(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 2 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.Int16*)(ptr);
        }*/

        public System.Int16 ReadShort(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 2 > this.Buffer.Length)
                return 0;
            return BitConverter.ToInt16(this.Buffer, (int)address);
        }

        /*public unsafe System.UInt16 ReadUShort(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 2 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.UInt16*)(ptr);
        }*/

        public System.UInt16 ReadUShort(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 2 > this.Buffer.Length)
                return 0;
            return BitConverter.ToUInt16(this.Buffer, (int)address);
        }

        /*public unsafe System.Int32 ReadInt(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.Int32*)(ptr);
        }*/

        public System.Int32 ReadInt(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            return BitConverter.ToInt32(this.Buffer, (int)address);
        }

        /*public unsafe System.UInt32 ReadUInt(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.UInt32*)(ptr);
        }*/

        public System.UInt32 ReadUInt(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            return BitConverter.ToUInt32(this.Buffer, (int)address);
        }

        /*public unsafe System.Int64 ReadInt64(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 8 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.Int64*)(ptr);
        }*/

        public System.Int64 ReadInt64(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 8 > this.Buffer.Length)
                return 0;
            return BitConverter.ToInt64(this.Buffer, (int)address);
        }

        /*public unsafe System.UInt64 ReadUInt64(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 8 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.UInt64*)(ptr);
        }*/

        public System.UInt64 ReadUInt64(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 8 > this.Buffer.Length)
                return 0;
            return BitConverter.ToUInt64(this.Buffer, (int)address);
        }

        /*public unsafe System.Single ReadFloat(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.Single*)(ptr);
        }*/

        public System.Single ReadFloat(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            return BitConverter.ToSingle(this.Buffer, (int)address);
        }

        public System.String ReadASCII(System.Int64 address, System.Int64 length, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + length > this.Buffer.Length)
                return "";

            for (System.Int64 i = 0; i < length; i++)
                if (this.Buffer[address + i] < 32 || this.Buffer[address + i] > 126)
                {
                    length = i;
                    break;
                }
            System.Byte[] result = new System.Byte[length];
            System.Array.Copy(this.Buffer, address, result, 0, length);
            return Encoding.ASCII.GetString(result);
        }


        public void WriteInt(System.Int64 address, System.Int16 valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = BitConverter.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }

        public void WriteInt(System.Int64 address, System.Int32 valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = BitConverter.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }

        public void WriteInt(System.Int64 address, System.UInt32 valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = BitConverter.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }

        public void WriteInt(System.Int64 address, System.Int64 valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = BitConverter.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }

        public void WriteFloat(System.Int64 address, System.Single valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = BitConverter.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }

        public void WriteString(System.Int64 address, System.String valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = Encoding.ASCII.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }
    }
}

