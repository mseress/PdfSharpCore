#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange
//
// Copyright (c) 2005-2017 empira Software GmbH, Cologne Area (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using System.Globalization;
using PdfSharp.Fonts;
#if CORE || GDI
using GdiFont = System.Drawing.Font;
#endif
using PdfSharp.Fonts.OpenType;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PdfSharp.Drawing.Fonts;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// The bytes of a font file.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class XFontSource
    {
        // Implementation Notes
        // 
        // * XFontSource represents a single font (file) in memory.
        // * An XFontSource hold a reference to it OpenTypeFontface.
        // * To prevent large heap fragmentation this class must exists only once.
        // * TODO: ttcf

        // Signature of a true type collection font.
        const uint ttcf = 0x66637474;

        public const string FALLBACK_FONT = "stsong.ttf";

        /// <summary>
        /// (fontName, fileName)
        /// </summary>
        private static readonly Dictionary<XFontSourceKey, string> _fontFilePaths = new Dictionary<XFontSourceKey, string>();

        static XFontSource()
        {
            var searchingPaths = new List<string>();
            // Application
            var executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            searchingPaths.Add(executingPath);
            // Windows fonts
            var systemFontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            if (!string.IsNullOrWhiteSpace(systemFontsPath))
            {
                searchingPaths.Add(systemFontsPath);
            }
            else
            {
                // Debian fonts
                searchingPaths.Add("/usr/share/fonts/truetype");
                //searchingPaths.Add("/usr/share/X11/fonts");
                //searchingPaths.Add("/usr/X11R6/lib/X11/fonts");
                //searchingPaths.Add("~/.fonts");
            }

            foreach (var searchingPath in searchingPaths)
            {
                // TODO: *.ttc not supported yet!
                var fileNames = new List<string>();
                try
                {
                    fileNames = Directory.GetFiles(searchingPath, "*", SearchOption.AllDirectories)
                        .Where(f => Path.GetExtension(f).ToLower() == ".ttf")
                        .ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    continue;
                }

                foreach (var filePath in fileNames)
                {
                    try
                    {
                        var fontStyle = TtfHelper.GetFontStyle(filePath);
                        var key = new XFontSourceKey(TtfHelper.GetFontFamilyName(filePath), fontStyle);
                        if (!_fontFilePaths.ContainsKey(key))
                        {
                            _fontFilePaths.Add(key, filePath);
                            Debug.WriteLine($"Added font '{key.FontFamilyName}': {filePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        XFontSource(byte[] bytes, ulong key)
        {
            _fontName = null;
            _bytes = bytes;
            _key = key;
        }

        /// <summary>
        /// Gets an existing font source or creates a new one.
        /// A new font source is cached in font factory.
        /// </summary>
        public static XFontSource GetOrCreateFrom(byte[] bytes)
        {
            ulong key = FontHelper.CalcChecksum(bytes);
            XFontSource fontSource;
            if (!FontFactory.TryGetFontSourceByKey(key, out fontSource))
            {
                fontSource = new XFontSource(bytes, key);
                // Theoretically the font source could be created by a differend thread in the meantime.
                fontSource = FontFactory.CacheFontSource(fontSource);
            }
            return fontSource;
        }

        public static XFontSource GetOrCreateFromGdi(string typefaceKey, GdiFont gdiFont)
        {
            byte[] bytes = ReadFontBytesFromGdi(gdiFont);
            XFontSource fontSource = GetOrCreateFrom(typefaceKey, bytes);
            return fontSource;
        }

        private static byte[] ReadFontBytesFromGdi(GdiFont gdiFont)
        {
            var fontKey = new XFontSourceKey(gdiFont.Name, gdiFont.Style);
            if (_fontFilePaths.ContainsKey(fontKey))
            {
                var filePath = _fontFilePaths[fontKey];
                Debug.WriteLine($"Retrieving font '{fontKey.ToString()}' from {filePath}");
                return File.ReadAllBytes(filePath);
            }

            // use embedded resource font if the specified one is not found            
            using (var fontStream = Assembly.GetAssembly(typeof(XFontSource)).GetManifestResourceStream($"PdfSharp.Assets.{FALLBACK_FONT}"))
            {
                Debug.WriteLine($"'{fontKey.ToString()}' not found, falling back by default to '{FALLBACK_FONT}'.");
                var fontData = new byte[fontStream.Length];
                fontStream.Read(fontData, 0, (int)fontStream.Length);
                return fontData;
            }
        }

        static XFontSource GetOrCreateFrom(string typefaceKey, byte[] fontBytes)
        {
            XFontSource fontSource;
            ulong key = FontHelper.CalcChecksum(fontBytes);
            if (FontFactory.TryGetFontSourceByKey(key, out fontSource))
            {
                // The font source already exists, but is not yet cached under the specified typeface key.
                FontFactory.CacheExistingFontSourceWithNewTypefaceKey(typefaceKey, fontSource);
            }
            else
            {
                // No font source exists. Create new one and cache it.
                fontSource = new XFontSource(fontBytes, key);
                FontFactory.CacheNewFontSource(typefaceKey, fontSource);
            }
            return fontSource;
        }

        public static XFontSource CreateCompiledFont(byte[] bytes)
        {
            XFontSource fontSource = new XFontSource(bytes, 0);
            return fontSource;
        }

        /// <summary>
        /// Gets or sets the fontface.
        /// </summary>
        internal OpenTypeFontface Fontface
        {
            get { return _fontface; }
            set
            {
                _fontface = value;
                _fontName = value.name.FullFontName;
            }
        }
        OpenTypeFontface _fontface;

        /// <summary>
        /// Gets the key that uniquely identifies this font source.
        /// </summary>
        internal ulong Key
        {
            get
            {
                if (_key == 0)
                    _key = FontHelper.CalcChecksum(Bytes);
                return _key;
            }
        }
        ulong _key;

        public void IncrementKey()
        {
            // HACK: Depends on implementation of CalcChecksum.
            // Increment check sum and keep length untouched.
            _key += 1ul << 32;
        }

        /// <summary>
        /// Gets the name of the font's name table.
        /// </summary>
        public string FontName
        {
            get { return _fontName; }
        }
        string _fontName;

        /// <summary>
        /// Gets the bytes of the font.
        /// </summary>
        public byte[] Bytes
        {
            get { return _bytes; }
        }
        readonly byte[] _bytes;

        public override int GetHashCode()
        {
            return (int)((Key >> 32) ^ Key);
        }

        public override bool Equals(object obj)
        {
            XFontSource fontSource = obj as XFontSource;
            if (fontSource == null)
                return false;
            return Key == fontSource.Key;
        }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSha rper disable UnusedMember.Local
        internal string DebuggerDisplay
        // ReShar per restore UnusedMember.Local
        {
            // The key is converted to a value a human can remember during debugging.
            get { return String.Format(CultureInfo.InvariantCulture, "XFontSource: '{0}', keyhash={1}", FontName, Key % 99991 /* largest prime number less than 100000 */); }
        }

        /// <summary>
        /// A key that combines the font family name and the font style to uniquely identify a specific font.
        /// </summary>
        private struct XFontSourceKey : IEquatable<XFontSourceKey>
        {
            public string FontFamilyName { get; set; }

            public System.Drawing.FontStyle FontStyle { get; set; }

            public XFontSourceKey(string fontFamilyName, System.Drawing.FontStyle fontStyle)
            {
                if (String.IsNullOrWhiteSpace(fontFamilyName))
                {
                    throw new ArgumentException("Value cannot be null or empty.", nameof(fontFamilyName));
                }

                this.FontFamilyName = fontFamilyName;
                this.FontStyle = fontStyle;
            }

            public bool Equals(XFontSourceKey other)
            {
                return this.FontStyle == other.FontStyle && this.FontFamilyName == other.FontFamilyName;
            }

            public override bool Equals(object obj)
            {
                if (obj is XFontSourceKey fontSourceKey)
                {
                    return this.Equals(fontSourceKey);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return this.FontFamilyName.GetHashCode() + this.FontStyle.GetHashCode();
            }

            public override string ToString()
            {
                return $"{this.FontFamilyName}, {this.FontStyle.ToString()}";
            }
        }
    }
}