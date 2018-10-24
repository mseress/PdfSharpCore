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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Internal;
using PdfSharp.Pdf;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Defines an object used to draw text.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed class XFont
    {
        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="emSize">The em size.</param>
        public XFont(string familyName, double emSize)
            : this(familyName, emSize, XFontStyle.Regular, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        public XFont(string familyName, double emSize, XFontStyle style)
            : this(familyName, emSize, style, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        /// <param name="pdfOptions">Additional PDF options.</param>
        public XFont(string familyName, double emSize, XFontStyle style, XPdfFontOptions pdfOptions)
        {
            FamilyName = familyName;
            Size = emSize;
            Style = style;
            _pdfOptions = pdfOptions;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class with enforced style simulation.
        /// Only for testing PDFsharp.
        /// </summary>
        public XFont(string familyName, double emSize, XFontStyle style, XPdfFontOptions pdfOptions, XStyleSimulations styleSimulations)
        {
            FamilyName = familyName;
            Size = emSize;
            Style = style;
            _pdfOptions = pdfOptions;
            OverrideStyleSimulations = true;
            StyleSimulations = styleSimulations;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.FontFamily.
        /// </summary>
        /// <param name="fontFamily">The System.Drawing.FontFamily.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        public XFont(GdiFontFamily fontFamily, double emSize, XFontStyle style)
            : this(fontFamily, emSize, style, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.FontFamily.
        /// </summary>
        /// <param name="fontFamily">The System.Drawing.FontFamily.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        /// <param name="pdfOptions">Additional PDF options.</param>
        public XFont(GdiFontFamily fontFamily, double emSize, XFontStyle style, XPdfFontOptions pdfOptions)
        {
            FamilyName = fontFamily.Name;
            _gdiFontFamily = fontFamily;
            Size = emSize;
            Style = style;
            _pdfOptions = pdfOptions;
            InitializeFromGdi();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.Font.
        /// </summary>
        /// <param name="font">The System.Drawing.Font.</param>
        public XFont(GdiFont font)
            : this(font, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.Font.
        /// </summary>
        /// <param name="font">The System.Drawing.Font.</param>
        /// <param name="pdfOptions">Additional PDF options.</param>
        public XFont(GdiFont font, XPdfFontOptions pdfOptions)
        {
            if (font.Unit != GraphicsUnit.World)
                throw new ArgumentException("Font must use GraphicsUnit.World.");
            _gdiFont = font;
            Debug.Assert(font.Name == font.FontFamily.Name);
            FamilyName = font.Name;
            Size = font.Size;
            Style = FontStyleFrom(font);
            _pdfOptions = pdfOptions;
            InitializeFromGdi();
        }
        #endregion


        /// <summary>
        /// Initializes this instance by computing the glyph typeface, font family, font source and TrueType fontface.
        /// (PDFsharp currently only deals with TrueType fonts.)
        /// </summary>
        void Initialize()
        {
            var fontResolvingOptions = OverrideStyleSimulations
                ? new FontResolvingOptions(Style, StyleSimulations)
                : new FontResolvingOptions(Style);

            // HACK: 'PlatformDefault' is used in unit test code.
            if (StringComparer.OrdinalIgnoreCase.Compare(FamilyName, GlobalFontSettings.DefaultFontName) == 0)
            {
                FamilyName = "Calibri";
            }

            // In principle an XFont is an XGlyphTypeface plus an em-size.
            GlyphTypeface = XGlyphTypeface.GetOrCreateFrom(FamilyName, fontResolvingOptions);
#if GDI  // TODO: In CORE build it is not necessary to create a GDI font at all
            // Create font by using font family.
            XFontSource fontSource;  // Not needed here.
            _gdiFont = FontHelper.CreateFont(_familyName, (float)Size, (GdiFontStyle)(_style & XFontStyle.BoldItalic), out fontSource);
#endif
            CreateDescriptorAndInitializeFontMetrics();
        }

        /// <summary>
        /// A GDI+ font object is used to setup the internal font objects.
        /// </summary>
        void InitializeFromGdi()
        {
            try
            {
                Lock.EnterFontFactory();
                if (_gdiFontFamily != null)
                {
                    // Create font based on its family.
                    _gdiFont = new Font(_gdiFontFamily, (float)Size, (GdiFontStyle)Style, GraphicsUnit.World);
                }

                if (_gdiFont != null)
                {
#if DEBUG_
                    string name1 = _gdiFont.Name;
                    string name2 = _gdiFont.OriginalFontName;
                    string name3 = _gdiFont.SystemFontName;
#endif
                    FamilyName = _gdiFont.FontFamily.Name;
                    // TODO: _glyphTypeface = XGlyphTypeface.GetOrCreateFrom(_gdiFont);
                }
                else
                {
                    Debug.Assert(false);
                }

                if (GlyphTypeface == null)
                    GlyphTypeface = XGlyphTypeface.GetOrCreateFromGdi(_gdiFont);

                CreateDescriptorAndInitializeFontMetrics();
            }
            finally { Lock.ExitFontFactory(); }
        }


        /// <summary>
        /// Code separated from Metric getter to make code easier to debug.
        /// (Setup properties in their getters caused side effects during debugging because Visual Studio calls a getter
        /// to early to show its value in a debugger window.)
        /// </summary>
        void CreateDescriptorAndInitializeFontMetrics()  // TODO: refactor
        {
            Debug.Assert(_fontMetrics == null, "InitializeFontMetrics() was already called.");
            Descriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptorFor(this); //_familyName, _style, _glyphTypeface.Fontface);
            _fontMetrics = new XFontMetrics(Descriptor.FontName, Descriptor.UnitsPerEm, Descriptor.Ascender, Descriptor.Descender,
                Descriptor.Leading, Descriptor.LineSpacing, Descriptor.CapHeight, Descriptor.XHeight, Descriptor.StemV, 0, 0, 0,
                Descriptor.UnderlinePosition, Descriptor.UnderlineThickness, Descriptor.StrikeoutPosition, Descriptor.StrikeoutSize);

            XFontMetrics fm = Metrics;

            // Already done in CreateDescriptorAndInitializeFontMetrics.
            //if (Descriptor == null)
            //    Descriptor = (OpenTypeDescriptor)FontDescriptorStock.Global.CreateDescriptor(this);  //(Name, (XGdiFontStyle)Font.Style);

            UnitsPerEm = Descriptor.UnitsPerEm;
            CellAscent = Descriptor.Ascender;
            CellDescent = Descriptor.Descender;
            CellSpace = Descriptor.LineSpacing;

            Debug.Assert(fm.UnitsPerEm == Descriptor.UnitsPerEm);
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Gets the XFontFamily object associated with this XFont object.
        /// </summary>
        [Browsable(false)]
        public XFontFamily FontFamily
        {
            get { return GlyphTypeface.FontFamily; }
        }

        /// <summary>
        /// WRONG: Gets the face name of this Font object.
        /// Indeed it returns the font family name.
        /// </summary>
        // [Obsolete("This function returns the font family name, not the face name. Use xxx.FontFamily.Name or xxx.FaceName")]
        public string Name
        {
            get { return GlyphTypeface.FontFamily.Name; }
        }

        internal string FaceName
        {
            get { return GlyphTypeface.FaceName; }
        }

        /// <summary>
        /// Gets the em-size of this font measured in the unit of this font object.
        /// </summary>
        public double Size { get; }

        /// <summary>
        /// Gets style information for this Font object.
        /// </summary>
        [Browsable(false)]
        public XFontStyle Style { get; }

        /// <summary>
        /// Indicates whether this XFont object is bold.
        /// </summary>
        public bool Bold
        {
            get { return (Style & XFontStyle.Bold) == XFontStyle.Bold; }
        }

        /// <summary>
        /// Indicates whether this XFont object is italic.
        /// </summary>
        public bool Italic
        {
            get { return (Style & XFontStyle.Italic) == XFontStyle.Italic; }
        }

        /// <summary>
        /// Indicates whether this XFont object is stroke out.
        /// </summary>
        public bool Strikeout
        {
            get { return (Style & XFontStyle.Strikeout) == XFontStyle.Strikeout; }
        }

        /// <summary>
        /// Indicates whether this XFont object is underlined.
        /// </summary>
        public bool Underline
        {
            get { return (Style & XFontStyle.Underline) == XFontStyle.Underline; }
        }

        /// <summary>
        /// Temporary HACK for XPS to PDF converter.
        /// </summary>
        internal bool IsVertical { get; set; }


        /// <summary>
        /// Gets the PDF options of the font.
        /// </summary>
        public XPdfFontOptions PdfOptions
        {
            get { return _pdfOptions ?? (_pdfOptions = new XPdfFontOptions()); }
        }
        XPdfFontOptions _pdfOptions;

        /// <summary>
        /// Indicates whether this XFont is encoded as Unicode.
        /// </summary>
        internal bool Unicode
        {
            get { return _pdfOptions != null && _pdfOptions.FontEncoding == PdfFontEncoding.Unicode; }
        }

        /// <summary>
        /// Gets the cell space for the font. The CellSpace is the line spacing, the sum of CellAscent and CellDescent and optionally some extra space.
        /// </summary>
        public int CellSpace { get; internal set; }

        /// <summary>
        /// Gets the cell ascent, the area above the base line that is used by the font.
        /// </summary>
        public int CellAscent { get; internal set; }

        /// <summary>
        /// Gets the cell descent, the area below the base line that is used by the font.
        /// </summary>
        public int CellDescent { get; internal set; }

        /// <summary>
        /// Gets the font metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public XFontMetrics Metrics
        {
            get
            {
                // Code moved to InitializeFontMetrics().
                //if (_fontMetrics == null)
                //{
                //    FontDescriptor descriptor = FontDescriptorStock.Global.CreateDescriptor(this);
                //    _fontMetrics = new XFontMetrics(descriptor.FontName, descriptor.UnitsPerEm, descriptor.Ascender, descriptor.Descender,
                //        descriptor.Leading, descriptor.LineSpacing, descriptor.CapHeight, descriptor.XHeight, descriptor.StemV, 0, 0, 0);
                //}
                Debug.Assert(_fontMetrics != null, "InitializeFontMetrics() not yet called.");
                return _fontMetrics;
            }
        }
        XFontMetrics _fontMetrics;

        /// <summary>
        /// Returns the line spacing, in pixels, of this font. The line spacing is the vertical distance
        /// between the base lines of two consecutive lines of text. Thus, the line spacing includes the
        /// blank space between lines along with the height of the character itself.
        /// </summary>
        public double GetHeight()
        {
            double value = CellSpace * Size / UnitsPerEm;
            return value;
        }

        /// <summary>
        /// Gets the line spacing of this font.
        /// </summary>
        [Browsable(false)]
        public int Height
        {
            // Implementation from System.Drawing.Font.cs
            get { return (int)Math.Ceiling(GetHeight()); }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public XGlyphTypeface GlyphTypeface { get; private set;}


        public OpenTypeDescriptor Descriptor { get; private set;}


        public string FamilyName { get; private set;}


        public int UnitsPerEm { get; private set;}

        /// <summary>
        /// Override style simulations by using the value of StyleSimulations.
        /// </summary>
        internal bool OverrideStyleSimulations;

        /// <summary>
        /// Used to enforce style simulations by renderer. For development purposes only.
        /// </summary>
        internal XStyleSimulations StyleSimulations;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the GDI family.
        /// </summary>
        /// <value>The GDI family.</value>
        public GdiFontFamily GdiFontFamily
        {
            get { return _gdiFontFamily; }
        }
        readonly GdiFontFamily _gdiFontFamily;

        internal GdiFont GdiFont
        {
            get { return _gdiFont; }
        }
        Font _gdiFont;

        internal static XFontStyle FontStyleFrom(GdiFont font)
        {
            return
              (font.Bold ? XFontStyle.Bold : 0) |
              (font.Italic ? XFontStyle.Italic : 0) |
              (font.Strikeout ? XFontStyle.Strikeout : 0) |
              (font.Underline ? XFontStyle.Underline : 0);
        }

        /// <summary>
        /// Implicit conversion form Font to XFont
        /// </summary>
        public static implicit operator XFont(GdiFont font)
        {
            return new XFont(font);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Cache PdfFontTable.FontSelector to speed up finding the right PdfFont
        /// if this font is used more than once.
        /// </summary>
        internal string Selector { get; set; }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
        {
            get { return String.Format(CultureInfo.InvariantCulture, "font=('{0}' {1:0.##})", Name, Size); }
        }
    }
}
