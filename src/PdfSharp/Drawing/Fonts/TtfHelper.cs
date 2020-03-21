using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace PdfSharp.Drawing.Fonts
{
    /// <summary>
    /// Helper class for TTF-related stuff.
    /// </summary>
    public static class TtfHelper
    {
        /// <summary>
        /// This is a magic number that can be found in every TTF file.
        /// </summary>
        public const uint MAGIC_NUMBER = 0x5F0F3CF5;

        /// <summary>
        /// Gets the <see cref="FontStyle"/> from the TTF <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The TTF stream.</param>
        /// <returns>The <see cref="FontStyle"/>.</returns>
        public static FontStyle GetFontStyle(Stream stream)
        {
            var data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return GetFontStyle(data);
        }

        /// <summary>
        /// Gets the <see cref="FontStyle"/> from the TTF file at <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The TTF file path.</param>
        /// <returns>The <see cref="FontStyle"/>.</returns>
        public static FontStyle GetFontStyle(string filePath)
        {
            return GetFontStyle(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// Gets the <see cref="FontStyle"/> from the TTF <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The TTF data.</param>
        /// <returns>The <see cref="FontStyle"/>.</returns>
        public static FontStyle GetFontStyle(byte[] data)
        {
            // NOTE: TTF originates from Apple, and Apple uses Unix. Unix is big endian, 
            // Windows is little endian, so we need to pay attention to the endianness of the data to
            // keep the code cross-platform.
            var magicBytes = BitConverter.GetBytes(MAGIC_NUMBER);
            if (BitConverter.IsLittleEndian)
            {
                magicBytes = magicBytes.Reverse().ToArray();
            }

            var magicIndex = FindInArray(data, magicBytes);
            if (magicIndex < 0)
            {
                throw new InvalidDataException("The file is not a valid Open Type font file, magic number 0x5F0F3CF5 not found.");
            }

            var styleBytes = new byte[2];
            const int styleBitsOffsetFromMagicNumber = 28;
            Array.Copy(data, magicIndex + magicBytes.Length + styleBitsOffsetFromMagicNumber, styleBytes, 0, styleBytes.Length);
            if (BitConverter.IsLittleEndian)
            {
                styleBytes = styleBytes.Reverse().ToArray();
            }

            var styleValue = BitConverter.ToUInt16(styleBytes, 0);
            return Int16ToFontStyle(styleValue);
        }

        /// <summary>
        /// Gets the font family name from the TTF file at <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The TTF file path.</param>
        /// <returns>The font family name.</returns>
        public static string GetFontFamilyName(string filePath)
        {
            var fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(filePath);
            return fontCollection.Families[0].Name;
        }

        private static FontStyle Int16ToFontStyle(UInt16 i)
        {
            // https://docs.microsoft.com/en-us/typography/opentype/spec/head
            // Bit 0: Bold (if set to 1);
            // Bit 1: Italic (if set to 1)
            // Bit 2: Underline (if set to 1)
            // Bit 3: Outline (if set to 1)
            // Bit 4: Shadow (if set to 1)
            // Bit 5: Condensed (if set to 1)
            // Bit 6: Extended (if set to 1)
            // Bits 7–15: Reserved (set to 0).

            const UInt16 boldFlag = 1;
            const UInt16 italicFlag = 2;
            var style = FontStyle.Regular;
            if ((boldFlag & i) != 0)
            {
                style |= FontStyle.Bold;
            }

            if ((italicFlag & i) != 0)
            {
                style |= FontStyle.Italic;
            }

            return style;
        }

        private static int FindInArray(byte[] arrayToSearchIn, byte[] arrayToFind)
        {
            for (int i = 0; i < arrayToSearchIn.Length; i++)
            {
                bool isFound = true;
                for (int j = 0; j < arrayToFind.Length; j++)
                {
                    if (arrayToSearchIn[i + j] != arrayToFind[j])
                    {
                        isFound = false;
                        break;
                    }
                }

                if (isFound)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}