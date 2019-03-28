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
#if GDI
using System.Drawing;
#endif

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Defines a single color object used to fill shapes and draw text.
    /// </summary>
    public sealed class XSolidBrush : XBrush
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XSolidBrush"/> class.
        /// </summary>
        public XSolidBrush()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XSolidBrush"/> class.
        /// </summary>
        public XSolidBrush(XColor color)
            : this(color, false)
        { }

        internal XSolidBrush(XColor color, bool immutable)
        {
            _color = color;
            _immutable = immutable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XSolidBrush"/> class.
        /// </summary>
        public XSolidBrush(XSolidBrush brush)
        {
            _color = brush.Color;
        }

        /// <summary>
        /// Gets or sets the color of this brush.
        /// </summary>
        public XColor Color
        {
            get { return _color; }
            set
            {
                if (_immutable)
                    throw new ArgumentException(PSSR.CannotChangeImmutableObject("XSolidBrush"));
#if GDI
                _gdiDirty = _gdiDirty || _color != value;
#endif
                _color = value;
            }
        }
        internal XColor _color;

        /// <summary>
        /// Gets or sets a value indicating whether the brush enables overprint when used in a PDF document.
        /// Experimental, takes effect only on CMYK color mode.
        /// </summary>
        public bool Overprint
        {
            get { return _overprint; }
            set
            {
                if (_immutable)
                    throw new ArgumentException(PSSR.CannotChangeImmutableObject("XSolidBrush"));
                _overprint = value;
            }
        }
        internal bool _overprint;

#if GDI
        internal override System.Drawing.Brush RealizeGdiBrush()
        {
            if (_gdiDirty)
            {
                if (_gdiBrush == null)
                    _gdiBrush = new SolidBrush(_color.ToGdiColor());
                else
                    _gdiBrush.Color = _color.ToGdiColor();
                _gdiDirty = false;
            }

#if DEBUG
            System.Drawing.Color clr = _color.ToGdiColor();
            SolidBrush brush1 = new SolidBrush(clr);
            Debug.Assert(_gdiBrush.Color == brush1.Color);
#endif
            return _gdiBrush;
        }
#endif


#if GDI
        bool _gdiDirty = true;
        SolidBrush _gdiBrush;
#endif
        readonly bool _immutable;
    }
}
