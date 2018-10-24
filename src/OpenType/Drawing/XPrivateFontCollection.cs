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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using PdfSharp.Fonts;
using System.Drawing;
using System.Drawing.Drawing2D;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
using GdiPrivateFontCollection = System.Drawing.Text.PrivateFontCollection;

namespace PdfSharp.Drawing
{
#if true
    ///<summary>
    /// Makes fonts that are not installed on the system available within the current application domain.<br/>
    /// In Silverlight required for all fonts used in PDF documents.
    /// </summary>
    public sealed class XPrivateFontCollection
    {
        // This one is global and can only grow. It is not possible to remove fonts that have been added.

        /// <summary>
        /// Initializes a new instance of the <see cref="XPrivateFontCollection"/> class.
        /// </summary>
        XPrivateFontCollection()
        {
            // HACK: Use one global PrivateFontCollection in GDI+
        }

#if GDI
        //internal PrivateFontCollection PrivateFontCollection
        //{
        //  get { return privateFontCollection; }
        //  set { privateFontCollection = value; }
        //}

        GdiPrivateFontCollection GetPrivateFontCollection()
        {
            // Create only if really needed.
            if (_privateFontCollection == null)
                _privateFontCollection = new GdiPrivateFontCollection();
            return _privateFontCollection;
        }

        // PrivateFontCollection of GDI+
        private GdiPrivateFontCollection _privateFontCollection;
#endif

        /// <summary>
        /// Gets the global font collection.
        /// </summary>
        internal static XPrivateFontCollection Singleton
        {
            get { return _singleton; }
        }
        internal static XPrivateFontCollection _singleton = new XPrivateFontCollection();

#if GDI
        /// <summary>
        /// Adds the font data to the font collections.
        /// </summary>
        [Obsolete("Use Add(Stream stream)")]
        public void AddFont(byte[] data, string familyName)
        {
            if (String.IsNullOrEmpty(familyName))
                throw new ArgumentNullException("familyName");

            //if (glyphTypeface == null)
            //  throw new ArgumentNullException("glyphTypeface");

            // Add to GDI+ PrivateFontCollection
            int length = data.Length;

            // Copy data without unsafe code 
            IntPtr ip = Marshal.AllocCoTaskMem(length);
            Marshal.Copy(data, 0, ip, length);
            GetPrivateFontCollection().AddMemoryFont(ip, length);
            // Do not free the memory here, AddMemoryFont stores a pointer, not a copy!
            //Marshal.FreeCoTaskMem(ip);
            //privateFonts.Add(glyphTypeface);
        }
#endif

        /// <summary>
        /// Adds the specified font data to the global PrivateFontCollection.
        /// Family name and style are automatically retrieved from the font.
        /// </summary>
#if GDI
        [Obsolete("Use Add(Stream stream)")]
#else
        [Obsolete("Use the GDI build of PDFsharp and use Add(Stream stream)")]
#endif
        public static void AddFont(string filename)
        {
            throw new NotImplementedException();
            //XGlyphTypeface glyphTypeface = new XGlyphTypeface(filename);
            //Global.AddGlyphTypeface(glyphTypeface);
        }

#if GDI
        /// <summary>
        /// Adds the specified font data to the global PrivateFontCollection.
        /// Family name and style are automatically retrieved from the font.
        /// </summary>
        [Obsolete("Use Add(stream).")]
        public static void AddFont(Stream stream)
        {
            Add(stream);
        }

        /// <summary>
        /// Adds the specified font data to the global PrivateFontCollection.
        /// Family name and style are automatically retrieved from the font.
        /// </summary>
        public static void Add(Stream stream)
        {
            int length = (int)stream.Length;
            byte[] bytes = new byte[length];
            stream.Read(bytes, 0, length);
            Add(bytes);
        }

        /// <summary>
        /// Adds the specified font data to the global PrivateFontCollection.
        /// Family name and style are automatically retrieved from the font.
        /// </summary>
        public static void Add(byte[] font)
        {
            IntPtr unmanagedPointer = Marshal.AllocCoTaskMem(font.Length);
            Marshal.Copy(font, 0, unmanagedPointer, font.Length);
            Singleton.GetPrivateFontCollection().AddMemoryFont(unmanagedPointer, font.Length);
            // Do not free the memory here, AddMemoryFont stores a pointer, not a copy!
            //Marshal.FreeCoTaskMem(ip);

            XFontSource fontSource = XFontSource.GetOrCreateFrom(font);

            string familyName = fontSource.FontName;

            if (familyName.EndsWith(" Regular", StringComparison.OrdinalIgnoreCase))
                familyName = familyName.Substring(0, familyName.Length - 8);

            bool bold = fontSource.Fontface.os2.IsBold;
            bool italic = fontSource.Fontface.os2.IsItalic;
            IncompetentlyMakeAHackToFixAProblemYouWoldNeverHaveIfYouUseAFontResolver(fontSource, ref familyName, ref bold, ref italic);
            string key = MakeKey(familyName, bold, italic);
            Singleton._fontSources.Add(key, fontSource);

            string typefaceKey = XGlyphTypeface.ComputeKey(familyName, bold, italic);
            FontFactory.CacheExistingFontSourceWithNewTypefaceKey(typefaceKey, fontSource);
        }

        static void IncompetentlyMakeAHackToFixAProblemYouWoldNeverHaveIfYouUseAFontResolver(XFontSource fontSource,
            ref string familyName, ref bool bold, ref bool italic)
        {
            const string regularSuffix = " Regular";
            const string boldSuffix = " Bold";
            const string italicSuffix = " Italic";
            const string boldItalicSuffix = " Bold Italic";
            const string italicBoldSuffix = " Italic Bold";

            if (familyName.EndsWith(regularSuffix, StringComparison.OrdinalIgnoreCase))
            {
                familyName = familyName.Substring(0, familyName.Length - regularSuffix.Length);
                Debug.Assert(!bold && !italic);
                bold = italic = false;
            }
            else if (familyName.EndsWith(boldItalicSuffix, StringComparison.OrdinalIgnoreCase) || familyName.EndsWith(italicBoldSuffix, StringComparison.OrdinalIgnoreCase))
            {
                familyName = familyName.Substring(0, familyName.Length - boldItalicSuffix.Length);
                Debug.Assert(bold && italic);
                bold = italic = true;
            }
            else if (familyName.EndsWith(boldSuffix, StringComparison.OrdinalIgnoreCase))
            {
                familyName = familyName.Substring(0, familyName.Length - boldSuffix.Length);
                Debug.Assert(bold && !italic);
                bold = true;
                italic = false;
            }
            else if (familyName.EndsWith(italicSuffix, StringComparison.OrdinalIgnoreCase))
            {
                familyName = familyName.Substring(0, familyName.Length - italicSuffix.Length);
                Debug.Assert(!bold && italic);
                bold = false;
                italic = true;
            }
            else
            {
                Debug.Assert(!bold && !italic);
                bold = false;
                italic = false;
            }
        }
#endif

        /// <summary>
        /// Adds the specified font data to the global PrivateFontCollection.
        /// Family name and style are automatically retrieved from the font.
        /// </summary>
#if GDI
        [Obsolete("Use Add(Stream stream)")]
#else
        [Obsolete("Use the GDI build of PDFsharp and use Add(Stream stream)")]
#endif
        public static void AddFont(Stream stream, string facename)
        {
            throw new NotImplementedException();
            //XGlyphTypeface glyphTypeface = new XGlyphTypeface(stream, facename);
            //Global.AddGlyphTypeface(glyphTypeface);
        }

        //        /// <summary>
        //        /// Adds XGlyphTypeface to internal collection.
        //        /// Family name and style are automatically retrieved from the font.
        //        /// </summary>
        //        void AddGlyphTypeface(XGlyphTypeface glyphTypeface)
        //        {
        //            string name = MakeName(glyphTypeface);
        //            if (_typefaces.ContainsKey(name))
        //                throw new InvalidOperationException(PSSR.FontAlreadyAdded(glyphTypeface.DisplayName));

        //            _typefaces.Add(name, glyphTypeface);
        //            //Debug.WriteLine("Font added: " + name);

        //#if GDI
        //            // Add to GDI+ PrivateFontCollection singleton.
        //            byte[] data = glyphTypeface.Fontface.FontSource.Bytes;
        //            int length = data.Length;

        //            IntPtr ip = Marshal.AllocCoTaskMem(length);
        //            Marshal.Copy(data, 0, ip, length);
        //            _privateFontCollection.AddMemoryFont(ip, length);
        //            // Do not free the memory here, AddMemoryFont stores a pointer, not a copy!
        //            // Marshal.FreeCoTaskMem(ip);
        //#endif

        //#if WPF
        //#endif
        //        }


        //internal static XGlyphTypeface TryGetXGlyphTypeface(string familyName, XFontStyle style)
        //{
        //    string name = MakeName(familyName, style);

        //    XGlyphTypeface typeface;
        //    _global._typefaces.TryGetValue(name, out typeface);
        //    return typeface;
        //}

#if GDI
        internal static GdiFont TryCreateFont(string name, double size, GdiFontStyle style, out XFontSource fontSource)
        {
            fontSource = null;
            try
            {
                GdiPrivateFontCollection pfc = Singleton._privateFontCollection;
                if (pfc == null)
                    return null;
#if true
                string key = MakeKey(name, (XFontStyle)style);
                if (Singleton._fontSources.TryGetValue(key, out fontSource))
                {
                    GdiFont font = new GdiFont(name, (float)size, style, GraphicsUnit.World);
#if DEBUG_
                    Debug.Assert(StringComparer.OrdinalIgnoreCase.Compare(name, font.Name) == 0);
                    Debug.Assert(font.Bold == ((style & GdiFontStyle.Bold) != 0));
                    Debug.Assert(font.Italic == ((style & GdiFontStyle.Italic) != 0));
#endif
                    return font;
                }
                return null;
#else
                foreach (GdiFontFamily family in pfc.Families)
                {
                    if (string.Compare(family.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        GdiFont font = new GdiFont(family, (float)size, style, GraphicsUnit.World);
                        if (string.Compare(font.Name, name, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            // Style simulation is not implemented in GDI+.
                            // Use WPF build.
                        }
                        return font;
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                // Ignore exception and return null.
                Debug.WriteLine(ex.ToString());
            }
            return null;
        }
#endif
        
        static string MakeKey(string familyName, XFontStyle style)
        {
            return MakeKey(familyName, (style & XFontStyle.Bold) != 0, (style & XFontStyle.Italic) != 0);
        }

        static string MakeKey(string familyName, bool bold, bool italic)
        {
            return familyName + "#" + (bold ? "b" : "") + (italic ? "i" : "");
        }

        readonly Dictionary<string, XGlyphTypeface> _typefaces = new Dictionary<string, XGlyphTypeface>();
#if GDI
        //List<XGlyphTypeface> privateFonts = new List<XGlyphTypeface>();
        readonly Dictionary<string, XFontSource> _fontSources = new Dictionary<string, XFontSource>(StringComparer.OrdinalIgnoreCase);
#endif
    }
#endif
}
