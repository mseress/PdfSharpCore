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
using System.Drawing;
using System.Drawing.Drawing2D;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
using PdfSharp.Drawing;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Default platform specific font resolving.
    /// </summary>
    public static class PlatformFontResolver //: IFontResolver
    {
        /// <summary>
        /// Resolves the typeface by generating a font resolver info.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="isBold">Indicates whether a bold font is requested.</param>
        /// <param name="isItalic">Indicates whether an italic font is requested.</param>
        public static FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            FontResolvingOptions fontResolvingOptions = new FontResolvingOptions(FontHelper.CreateStyle(isBold, isItalic));
            return ResolveTypeface(familyName, fontResolvingOptions, XGlyphTypeface.ComputeKey(familyName, fontResolvingOptions));
        }

        /// <summary>
        /// Internal implementation.
        /// </summary>
        internal static FontResolverInfo ResolveTypeface(string familyName, FontResolvingOptions fontResolvingOptions, string typefaceKey)
        {
            // Internally we often have the typeface key already.
            if (string.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeKey(familyName, fontResolvingOptions);

            // The user may call ResolveTypeface anytime from anywhere, so check cache in FontFactory in the first place.
            FontResolverInfo fontResolverInfo;
            if (FontFactory.TryGetFontResolverInfoByTypefaceKey(typefaceKey, out fontResolverInfo))
                return fontResolverInfo;

            // Let the platform create the requested font source and save both PlattformResolverInfo
            // and XFontSource in FontFactory cache.
            // It is possible that we already have the correct font source. E.g. we already have the regular typeface in cache
            // and looking now for the italic typeface, but no such font exists. In this case we get the regular font source
            // and cache again it with the italic typeface key. Furthermore in glyph typeface style simulation for italic is set.

            GdiFont gdiFont;
            XFontSource fontSource = CreateFontSource(familyName, fontResolvingOptions, out gdiFont, typefaceKey);

            // If no such font exists return null. PDFsharp will fail.
            if (fontSource == null)
                return null;

            //#if (CORE || GDI) && !WPF
            //            // TODO: Support style simulation for GDI+ platform fonts.
            //            fontResolverInfo = new PlatformFontResolverInfo(typefaceKey, false, false, gdiFont);
            //#endif
            if (fontResolvingOptions.OverrideStyleSimulations)
            {
                // TODO: Support style simulation for GDI+ platform fonts.
                fontResolverInfo = new PlatformFontResolverInfo(typefaceKey, fontResolvingOptions.MustSimulateBold, fontResolvingOptions.MustSimulateItalic, gdiFont);
            }
            else
            {
                bool mustSimulateBold = gdiFont.Bold && !fontSource.Fontface.os2.IsBold;
                bool mustSimulateItalic = gdiFont.Italic && !fontSource.Fontface.os2.IsItalic;
                fontResolverInfo = new PlatformFontResolverInfo(typefaceKey, mustSimulateBold, mustSimulateItalic, gdiFont);
            }

            FontFactory.CacheFontResolverInfo(typefaceKey, fontResolverInfo);

            // Register font data under the platform specific face name.
            // Already done in CreateFontSource.
            // FontFactory.CacheNewFontSource(typefaceKey, fontSource);

            return fontResolverInfo;
        }

        /// <summary>
        /// Create a GDI+ font and use its handle to retrieve font data using native calls.
        /// </summary>
        internal static XFontSource CreateFontSource(string familyName, FontResolvingOptions fontResolvingOptions, out GdiFont font, string typefaceKey)
        {
            if (string.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeKey(familyName, fontResolvingOptions);

            GdiFontStyle gdiStyle = (GdiFontStyle)(fontResolvingOptions.FontStyle & XFontStyle.BoldItalic);

            XFontSource fontSource = null;
            if (!FontFactory.TryGetFontSourceByTypefaceKey(typefaceKey, out fontSource))
            {
                font = FontHelper.CreateFont(familyName, 10, gdiStyle, out fontSource);
                Debug.Assert(font != null);
                FontFactory.CacheExistingFontSourceWithNewTypefaceKey(typefaceKey, fontSource);
                FontFactory.CacheExistingFontWithNewTypefaceKey(typefaceKey, font);
            }
            else
            {
                FontFactory.TryGetFontByTypefaceKey(typefaceKey, out font);
            }
            return fontSource;
        }

    }
}
