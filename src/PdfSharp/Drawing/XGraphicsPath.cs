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
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Represents a series of connected lines and curves.
    /// </summary>
    public sealed class XGraphicsPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XGraphicsPath"/> class.
        /// </summary>
        public XGraphicsPath()
        {
#if CORE
            _corePath = new CoreGraphicsPath();
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath = new GraphicsPath();
            }
            finally { Lock.ExitGdiPlus(); }
#endif

        }

#if GDI
        /// <summary>
        /// Initializes a new instance of the <see cref="XGraphicsPath"/> class.
        /// </summary>
        public XGraphicsPath(PointF[] points, byte[] types, XFillMode fillMode)
        {
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath = new GraphicsPath(points, types, (FillMode)fillMode);
            }
            finally { Lock.ExitGdiPlus(); }
#endif

        }
#endif

        /// <summary>
        /// Clones this instance.
        /// </summary>
        public XGraphicsPath Clone()
        {
            XGraphicsPath path = (XGraphicsPath)MemberwiseClone();
#if CORE
            _corePath = new CoreGraphicsPath(_corePath);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                path._gdipPath = _gdipPath.Clone() as GraphicsPath;
            }
            finally { Lock.ExitGdiPlus(); }
#endif
            return path;
        }

        // ----- AddLine ------------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a line segment to current figure.
        /// </summary>
        public void AddLine(System.Drawing.Point pt1, System.Drawing.Point pt2)
        {
            AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }
#endif

#if GDI
        /// <summary>
        /// Adds  a line segment to current figure.
        /// </summary>
        public void AddLine(PointF pt1, PointF pt2)
        {
            AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }
#endif

        /// <summary>
        /// Adds  a line segment to current figure.
        /// </summary>
        public void AddLine(XPoint pt1, XPoint pt2)
        {
            AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        /// <summary>
        /// Adds  a line segment to current figure.
        /// </summary>
        public void AddLine(double x1, double y1, double x2, double y2)
        {
#if CORE
            _corePath.MoveOrLineTo(x1, y1);
            _corePath.LineTo(x2, y2, false);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddLine((float)x1, (float)y1, (float)x2, (float)y2);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // ----- AddLines -----------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a series of connected line segments to current figure.
        /// </summary>
        public void AddLines(System.Drawing.Point[] points)
        {
            AddLines(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

#if GDI
        /// <summary>
        /// Adds a series of connected line segments to current figure.
        /// </summary>
        public void AddLines(PointF[] points)
        {
            AddLines(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

        /// <summary>
        /// Adds a series of connected line segments to current figure.
        /// </summary>
        public void AddLines(XPoint[] points)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            int count = points.Length;
            if (count == 0)
                return;
#if CORE
            _corePath.MoveOrLineTo(points[0].X, points[0].Y);
            for (int idx = 1; idx < count; idx++)
                _corePath.LineTo(points[idx].X, points[idx].Y, false);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddLines(XGraphics.MakePointFArray(points));
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // ----- AddBezier ----------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(System.Drawing.Point pt1, System.Drawing.Point pt2, System.Drawing.Point pt3, System.Drawing.Point pt4)
        {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }
#endif

#if WPF
        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(SysPoint pt1, SysPoint pt2, SysPoint pt3, SysPoint pt4)
        {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4)
        {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }
#endif

        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(XPoint pt1, XPoint pt2, XPoint pt3, XPoint pt4)
        {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }

        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
#if CORE
            _corePath.MoveOrLineTo(x1, y1);
            _corePath.BezierTo(x2, y2, x3, y3, x4, y4, false);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddBezier((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3, (float)x4, (float)y4);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // ----- AddBeziers ---------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a sequence of connected cubic Bézier curves to the current figure.
        /// </summary>
        public void AddBeziers(System.Drawing.Point[] points)
        {
            AddBeziers(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

#if GDI
        /// <summary>
        /// Adds a sequence of connected cubic Bézier curves to the current figure.
        /// </summary>
        public void AddBeziers(PointF[] points)
        {
            AddBeziers(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

        /// <summary>
        /// Adds a sequence of connected cubic Bézier curves to the current figure.
        /// </summary>
        public void AddBeziers(XPoint[] points)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            int count = points.Length;
            if (count < 4)
                throw new ArgumentException("At least four points required for bezier curve.", "points");

            if ((count - 1) % 3 != 0)
                throw new ArgumentException("Invalid number of points for bezier curve. Number must fulfil 4+3n.",
                    "points");

#if CORE
            _corePath.MoveOrLineTo(points[0].X, points[0].Y);
            for (int idx = 1; idx < count; idx += 3)
            {
                _corePath.BezierTo(points[idx].X, points[idx].Y, points[idx + 1].X, points[idx + 1].Y,
                    points[idx + 2].X, points[idx + 2].Y, false);
            }
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddBeziers(XGraphics.MakePointFArray(points));
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // ----- AddCurve -----------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(System.Drawing.Point[] points)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(PointF[] points)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length));
        }
#endif

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(XPoint[] points)
        {
            AddCurve(points, 0.5);
        }

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(System.Drawing.Point[] points, double tension)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(PointF[] points, double tension)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }
#endif

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(XPoint[] points, double tension)
        {
            int count = points.Length;
            if (count < 2)
                throw new ArgumentException("AddCurve requires two or more points.", "points");
#if CORE
            _corePath.AddCurve(points, tension);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddCurve(XGraphics.MakePointFArray(points), (float)tension);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(System.Drawing.Point[] points, int offset, int numberOfSegments, float tension)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), offset, numberOfSegments, tension);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(PointF[] points, int offset, int numberOfSegments, float tension)
        {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), offset, numberOfSegments, tension);
        }
#endif

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(XPoint[] points, int offset, int numberOfSegments, double tension)
        {
#if CORE
            throw new NotImplementedException("AddCurve not yet implemented.");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddCurve(XGraphics.MakePointFArray(points), offset, numberOfSegments, (float)tension);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // ----- AddArc -------------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(Rectangle rect, double startAngle, double sweepAngle)
        {
            AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }
#endif

#if GDI
        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(RectangleF rect, double startAngle, double sweepAngle)
        {
            AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }
#endif

        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(XRect rect, double startAngle, double sweepAngle)
        {
            AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(double x, double y, double width, double height, double startAngle, double sweepAngle)
        {
#if CORE
            _corePath.AddArc(x, y, width, height, startAngle, sweepAngle);
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddArc((float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        /// <summary>
        /// Adds an elliptical arc to the current figure. The arc is specified WPF like.
        /// </summary>
        public void AddArc(XPoint point1, XPoint point2, XSize size, double rotationAngle, bool isLargeArg, XSweepDirection sweepDirection)
        {
#if CORE
            _corePath.AddArc(point1, point2, size, rotationAngle, isLargeArg, sweepDirection);
#endif
#if GDI
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddArc");
#endif
        }

        // ----- AddRectangle -------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(Rectangle rect)
        {
            AddRectangle(new XRect(rect));
        }
#endif

#if GDI
        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(RectangleF rect)
        {
            AddRectangle(new XRect(rect));
        }
#endif

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(XRect rect)
        {
#if CORE
            _corePath.MoveTo(rect.X, rect.Y);
            _corePath.LineTo(rect.X + rect.Width, rect.Y, false);
            _corePath.LineTo(rect.X + rect.Width, rect.Y + rect.Height, false);
            _corePath.LineTo(rect.X, rect.Y + rect.Height, true);
            _corePath.CloseSubpath();
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                // If rect is empty GDI+ removes the rect from the path.
                // This is not intended if the path is used for clipping.
                // See http://forum.pdfsharp.net/viewtopic.php?p=9433#p9433
                // _gdipPath.AddRectangle(rect.ToRectangleF());

                // Draw the rectangle manually.
                _gdipPath.StartFigure();
                _gdipPath.AddLines(new PointF[] { rect.TopLeft.ToPointF(), rect.TopRight.ToPointF(), rect.BottomRight.ToPointF(), rect.BottomLeft.ToPointF() });
                _gdipPath.CloseFigure();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(double x, double y, double width, double height)
        {
            AddRectangle(new XRect(x, y, width, height));
        }

        // ----- AddRectangles ------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        public void AddRectangles(Rectangle[] rects)
        {
            int count = rects.Length;
            for (int idx = 0; idx < count; idx++)
                AddRectangle(rects[idx]);

            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddRectangles(rects);
            }
            finally { Lock.ExitGdiPlus(); }
        }
#endif

#if GDI
        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        public void AddRectangles(RectangleF[] rects)
        {
            int count = rects.Length;
            for (int idx = 0; idx < count; idx++)
                AddRectangle(rects[idx]);

            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddRectangles(rects);
            }
            finally { Lock.ExitGdiPlus(); }
        }
#endif

        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        public void AddRectangles(XRect[] rects)
        {
            int count = rects.Length;
            for (int idx = 0; idx < count; idx++)
            {
#if CORE
                AddRectangle(rects[idx]);
#endif
#if GDI
                try
                {
                    Lock.EnterGdiPlus();
                    _gdipPath.AddRectangle(rects[idx].ToRectangleF());
                }
                finally { Lock.ExitGdiPlus(); }
#endif
#if WPF
                StartFigure();
                PathFigure figure = CurrentPathFigure;
                XRect rect = rects[idx];
                figure.StartPoint = new SysPoint(rect.X, rect.Y);

                // figure.Segments.Add(new LineSegment(new SysPoint(rect.x + rect.width, rect.y), true));
                // figure.Segments.Add(new LineSegment(new SysPoint(rect.x + rect.width, rect.y + rect.height), true));
                // figure.Segments.Add(new LineSegment(new SysPoint(rect.x, rect.y + rect.height), true));
#if !SILVERLIGHT
                LineSegment lineSegment1 = new LineSegment(new SysPoint(rect.X + rect.Width, rect.Y), true);
                LineSegment lineSegment2 = new LineSegment(new SysPoint(rect.X + rect.Width, rect.Y + rect.Height), true);
                LineSegment lineSegment3 = new LineSegment(new SysPoint(rect.X, rect.Y + rect.Height), true);
#else
                LineSegment lineSegment1 = new LineSegment();
                lineSegment1.Point = new Point(rect.X + rect.Width, rect.Y);
                LineSegment lineSegment2 = new LineSegment();
                lineSegment2.Point = new Point(rect.X + rect.Width, rect.Y + rect.Height);
                LineSegment lineSegment3 = new LineSegment();
                lineSegment3.Point = new Point(rect.X, rect.Y + rect.Height);
#endif
                figure.Segments.Add(lineSegment1);
                figure.Segments.Add(lineSegment2);
                figure.Segments.Add(lineSegment3);
                CloseFigure();
#endif
            }
        }

        // ----- AddRoundedRectangle ------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(Rectangle rect, System.Drawing.Size ellipseSize)
        {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }
#endif

#if WPF || NETFX_CORE
        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(SysRect rect, SysSize ellipseSize)
        {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(RectangleF rect, SizeF ellipseSize)
        {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(XRect rect, SizeF ellipseSize)
        {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }
#endif

        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(double x, double y, double width, double height, double ellipseWidth, double ellipseHeight)
        {
#if CORE
#if true
            double arcWidth = ellipseWidth / 2;
            double arcHeight = ellipseHeight / 2;
#if true  // Clockwise
            _corePath.MoveTo(x + width - arcWidth, y);
            _corePath.QuadrantArcTo(x + width - arcWidth, y + arcHeight, arcWidth, arcHeight, 1, true);

            _corePath.LineTo(x + width, y + height - arcHeight, false);
            _corePath.QuadrantArcTo(x + width - arcWidth, y + height - arcHeight, arcWidth, arcHeight, 4, true);

            _corePath.LineTo(x + arcWidth, y + height, false);
            _corePath.QuadrantArcTo(x + arcWidth, y + height - arcHeight, arcWidth, arcHeight, 3, true);

            _corePath.LineTo(x, y + arcHeight, false);
            _corePath.QuadrantArcTo(x + arcWidth, y + arcHeight, arcWidth, arcHeight, 2, true);

            _corePath.CloseSubpath();
#else  // Counterclockwise
            _corePath.MoveTo(x + arcWidth, y);
            _corePath.QuadrantArcTo(x + arcWidth, y + arcHeight, arcWidth, arcHeight, 2, false);

            _corePath.LineTo(x, y + height - arcHeight, false);
            _corePath.QuadrantArcTo(x + arcWidth, y + height - arcHeight, arcWidth, arcHeight, 3, false);

            _corePath.LineTo(x + width - arcWidth, y + height, false);
            _corePath.QuadrantArcTo(x + width - arcWidth, y + height - arcHeight, arcWidth, arcHeight, 4, false);

            _corePath.LineTo(x + width, y + arcHeight, false);
            _corePath.QuadrantArcTo(x + width - arcWidth, y + arcHeight, arcWidth, arcHeight, 1, false);

            _corePath.CloseSubpath();
#endif
#else
            // AddArc not yet implemented
            AddArc((float)(x + width - ellipseWidth), (float)y, (float)ellipseWidth, (float)ellipseHeight, -90, 90);
            AddArc((float)(x + width - ellipseWidth), (float)(y + height - ellipseHeight), (float)ellipseWidth,
                (float)ellipseHeight, 0, 90);
            AddArc((float)x, (float)(y + height - ellipseHeight), (float)ellipseWidth, (float)ellipseHeight, 90, 90);
            AddArc((float)x, (float)y, (float)ellipseWidth, (float)ellipseHeight, 180, 90);
            CloseFigure();
#endif
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.StartFigure();
                _gdipPath.AddArc((float)(x + width - ellipseWidth), (float)y, (float)ellipseWidth, (float)ellipseHeight, -90, 90);
                _gdipPath.AddArc((float)(x + width - ellipseWidth), (float)(y + height - ellipseHeight), (float)ellipseWidth, (float)ellipseHeight, 0, 90);
                _gdipPath.AddArc((float)x, (float)(y + height - ellipseHeight), (float)ellipseWidth, (float)ellipseHeight, 90, 90);
                _gdipPath.AddArc((float)x, (float)y, (float)ellipseWidth, (float)ellipseHeight, 180, 90);
                _gdipPath.CloseFigure();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || NETFX_CORE
            double ex = ellipseWidth / 2;
            double ey = ellipseHeight / 2;
            StartFigure();
            PathFigure figure = CurrentPathFigure;
            figure.StartPoint = new SysPoint(x + ex, y);

            //#if !SILVERLIGHT
            //      figure.Segments.Add(new LineSegment(new SysPoint(x + width - ex, y), true));
            //      // TODOWPF XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx
            //      figure.Segments.Add(new ArcSegment(new SysPoint(x + width, y + ey), new SysSize(ex, ey), 0, false, SweepDirection.Clockwise, true));
            //      //figure.Segments.Add(new LineSegment(new SysPoint(x + width, y + ey), true));

            //      figure.Segments.Add(new LineSegment(new SysPoint(x + width, y + height - ey), true));
            //      // TODOWPF
            //      figure.Segments.Add(new ArcSegment(new SysPoint(x + width - ex, y + height), new SysSize(ex, ey), 0, false, SweepDirection.Clockwise, true));
            //      //figure.Segments.Add(new LineSegment(new SysPoint(x + width - ex, y + height), true));

            //      figure.Segments.Add(new LineSegment(new SysPoint(x + ex, y + height), true));
            //      // TODOWPF
            //      figure.Segments.Add(new ArcSegment(new SysPoint(x, y + height - ey), new SysSize(ex, ey), 0, false, SweepDirection.Clockwise, true));
            //      //figure.Segments.Add(new LineSegment(new SysPoint(x, y + height - ey), true));

            //      figure.Segments.Add(new LineSegment(new SysPoint(x, y + ey), true));
            //      // TODOWPF
            //      figure.Segments.Add(new ArcSegment(new SysPoint(x + ex, y), new SysSize(ex, ey), 0, false, SweepDirection.Clockwise, true));
            //      //figure.Segments.Add(new LineSegment(new SysPoint(x + ex, y), true));
            //#else

#if !SILVERLIGHT && !NETFX_CORE
            figure.Segments.Add(new LineSegment(new SysPoint(x + width - ex, y), true));
#else
            figure.Segments.Add(new LineSegment { Point = new SysPoint(x + width - ex, y) });
#endif

            // TODOWPF XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx
#if !SILVERLIGHT && !NETFX_CORE
            figure.Segments.Add(new ArcSegment(new SysPoint(x + width, y + ey), new SysSize(ex, ey), 0, false, SweepDirection.Clockwise, true));
            //figure.Segments.Add(new LineSegment(new SysPoint(x + width, y + ey), true));
#else
            figure.Segments.Add(new ArcSegment
            {
                Point = new SysPoint(x + width, y + ey),
                Size = new SysSize(ex, ey),
                //RotationAngle = 0,
                //IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            });
#endif

#if !SILVERLIGHT && !NETFX_CORE
            figure.Segments.Add(new LineSegment(new SysPoint(x + width, y + height - ey), true));
#else
            figure.Segments.Add(new LineSegment { Point = new SysPoint(x + width, y + height - ey) });
#endif

            // TODOWPF
#if !SILVERLIGHT && !NETFX_CORE
            figure.Segments.Add(new ArcSegment(new SysPoint(x + width - ex, y + height), new SysSize(ex, ey), 0, false, SweepDirection.Clockwise, true));
            //figure.Segments.Add(new LineSegment(new SysPoint(x + width - ex, y + height), true));
#else
            figure.Segments.Add(new ArcSegment
            {
                Point = new SysPoint(x + width - ex, y + height),
                Size = new SysSize(ex, ey),
                //RotationAngle = 0,
                //IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            });
#endif

#if !SILVERLIGHT && !NETFX_CORE
            figure.Segments.Add(new LineSegment(new SysPoint(x + ex, y + height), true));
#else
            figure.Segments.Add(new LineSegment { Point = new SysPoint(x + ex, y + height) });
#endif

            // TODOWPF
#if !SILVERLIGHT && !NETFX_CORE
            figure.Segments.Add(new ArcSegment(new SysPoint(x, y + height - ey), new SysSize(ex, ey), 0, false, SweepDirection.Clockwise, true));
            //figure.Segments.Add(new LineSegment(new SysPoint(x, y + height - ey), true));
#else
            figure.Segments.Add(new ArcSegment
            {
                Point = new SysPoint(x, y + height - ey),
                Size = new SysSize(ex, ey),
                //RotationAngle = 0,
                //IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            });
#endif

#if !SILVERLIGHT && !NETFX_CORE
            figure.Segments.Add(new LineSegment(new SysPoint(x, y + ey), true));
#else
            figure.Segments.Add(new LineSegment { Point = new SysPoint(x, y + ey) });
#endif

            // TODOWPF
#if !SILVERLIGHT && !NETFX_CORE
            figure.Segments.Add(new ArcSegment(new SysPoint(x + ex, y), new SysSize(ex, ey), 0, false, SweepDirection.Clockwise, true));
            //figure.Segments.Add(new LineSegment(new SysPoint(x + ex, y), true));
#else
            figure.Segments.Add(new ArcSegment
            {
                Point = new SysPoint(x + ex, y),
                Size = new SysSize(ex, ey),
                //RotationAngle = 0,
                //IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            });
#endif
            CloseFigure();
#endif
        }

        // ----- AddEllipse ---------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(Rectangle rect)
        {
            AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }
#endif

#if GDI
        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(RectangleF rect)
        {
            AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }
#endif

        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(XRect rect)
        {
            AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(double x, double y, double width, double height)
        {
#if CORE
            double w = width / 2;
            double h = height / 2;
            double xc = x + w;
            double yc = y + h;
            _corePath.MoveTo(x + w, y);
            _corePath.QuadrantArcTo(xc, yc, w, h, 1, true);
            _corePath.QuadrantArcTo(xc, yc, w, h, 4, true);
            _corePath.QuadrantArcTo(xc, yc, w, h, 3, true);
            _corePath.QuadrantArcTo(xc, yc, w, h, 2, true);
            _corePath.CloseSubpath();
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddEllipse((float)x, (float)y, (float)width, (float)height);
            }
            finally { Lock.ExitGdiPlus(); }
#endif

        }

        // ----- AddPolygon ---------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        public void AddPolygon(System.Drawing.Point[] points)
        {
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddPolygon(points);
            }
            finally { Lock.ExitGdiPlus(); }
        }
#endif



#if GDI
        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        public void AddPolygon(PointF[] points)
        {
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddPolygon(points);
            }
            finally { Lock.ExitGdiPlus(); }
        }
#endif

        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        public void AddPolygon(XPoint[] points)
        {
#if CORE
            int count = points.Length;
            if (count == 0)
                return;

            _corePath.MoveTo(points[0].X, points[0].Y);
            for (int idx = 0; idx < count - 1; idx++)
                _corePath.LineTo(points[idx].X, points[idx].Y, false);
            _corePath.LineTo(points[count - 1].X, points[count - 1].Y, true);
            _corePath.CloseSubpath();
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddPolygon(XGraphics.MakePointFArray(points));
            }
            finally { Lock.ExitGdiPlus(); }
#endif

        }

        // ----- AddPie -------------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(Rectangle rect, double startAngle, double sweepAngle)
        {
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddPie(rect, (float)startAngle, (float)sweepAngle);
            }
            finally { Lock.ExitGdiPlus(); }
        }
#endif

#if GDI
        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(RectangleF rect, double startAngle, double sweepAngle)
        {
            AddPie(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }
#endif

        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(XRect rect, double startAngle, double sweepAngle)
        {
            AddPie(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(double x, double y, double width, double height, double startAngle, double sweepAngle)
        {
#if CORE
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddPie");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddPie((float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // ----- AddClosedCurve ------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(System.Drawing.Point[] points)
        {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), 0.5);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(PointF[] points)
        {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), 0.5);
        }
#endif

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(XPoint[] points)
        {
            AddClosedCurve(points, 0.5);
        }

#if GDI
        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(System.Drawing.Point[] points, double tension)
        {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(PointF[] points, double tension)
        {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }
#endif

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(XPoint[] points, double tension)
        {
            if (points == null)
                throw new ArgumentNullException("points");
            int count = points.Length;
            if (count == 0)
                return;
            if (count < 2)
                throw new ArgumentException("Not enough points.", "points");

#if CORE
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddClosedCurve");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddClosedCurve(XGraphics.MakePointFArray(points), (float)tension);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // ----- AddPath ------------------------------------------------------------------------------

        /// <summary>
        /// Adds the specified path to this path.
        /// </summary>
        public void AddPath(XGraphicsPath path, bool connect)
        {
#if CORE
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddPath");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddPath(path._gdipPath, connect);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // ----- AddString ----------------------------------------------------------------------------

#if GDI
        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, System.Drawing.Point origin, XStringFormat format)
        {
            AddString(s, family, style, emSize, new XRect(origin.X, origin.Y, 0, 0), format);
        }
#endif

#if GDI
        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, PointF origin, XStringFormat format)
        {
            AddString(s, family, style, emSize, new XRect(origin.X, origin.Y, 0, 0), format);
        }
#endif

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, XPoint origin,
            XStringFormat format)
        {
            try
            {
#if CORE
                DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddString");
#endif
#if GDI
                if (family.GdiFamily == null)
                    throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));

                PointF p = origin.ToPointF();
                p.Y += SimulateBaselineOffset(family, style, emSize, format);

                try
                {
                    Lock.EnterGdiPlus();
                    _gdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, p, format.RealizeGdiStringFormat());
                }
                finally { Lock.ExitGdiPlus(); }
#endif
            }
            catch
            {
                throw;
            }
        }

#if GDI
        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, Rectangle layoutRect, XStringFormat format)
        {
            if (family.GdiFamily == null)
                throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));

            Rectangle rect = new Rectangle(layoutRect.X, layoutRect.Y, layoutRect.Width, layoutRect.Height);
            rect.Offset(new System.Drawing.Point(0, (int)SimulateBaselineOffset(family, style, emSize, format)));

            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, rect, format.RealizeGdiStringFormat());
            }
            finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, RectangleF layoutRect, XStringFormat format)
        {
            if (family.GdiFamily == null)
                throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));

            RectangleF rect = new RectangleF(layoutRect.X, layoutRect.Y, layoutRect.Width, layoutRect.Height);
            rect.Offset(new PointF(0, SimulateBaselineOffset(family, style, emSize, format)));

            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, layoutRect, format.RealizeGdiStringFormat());
            }
            finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Calculates the offset for BaseLine positioning simulation:
        /// In GDI we have only Near, Center and Far as LineAlignment and no BaseLine. For XLineAlignment.BaseLine StringAlignment.Near is returned.
        /// We now return the negative drawed ascender height.
        /// This has to be added to the LayoutRect/Origin before each _gdipPath.AddString().
        /// </summary>
        /// <param name="family"></param>
        /// <param name="style"></param>
        /// <param name="emSize"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private float SimulateBaselineOffset(XFontFamily family, XFontStyle style, double emSize, XStringFormat format)
        {
            XFont font = new XFont(family.Name, emSize, style);

            if (format.LineAlignment == XLineAlignment.BaseLine)
            {
                double lineSpace = font.GetHeight();
                int cellSpace = font.FontFamily.GetLineSpacing(font.Style);
                int cellAscent = font.FontFamily.GetCellAscent(font.Style);
                int cellDescent = font.FontFamily.GetCellDescent(font.Style);
                double cyAscent = lineSpace * cellAscent / cellSpace;
                cyAscent = lineSpace * font.CellAscent / font.CellSpace;
                return (float)-cyAscent;
            }
            return 0;
        }

#endif

#if WPF
        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, Rect rect, XStringFormat format)
        {
            //gdip Path.AddString(s, family.gdiFamily, (int)style, (float)emSize, layoutRect, format.RealizeGdiStringFormat());
            AddString(s, family, style, emSize, new XRect(rect), format);
        }
#endif

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, XRect layoutRect,
            XStringFormat format)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            if (family == null)
                throw new ArgumentNullException("family");

            if (format == null)
                format = XStringFormats.Default;

            if (format.LineAlignment == XLineAlignment.BaseLine && layoutRect.Height != 0)
                throw new InvalidOperationException(
                    "DrawString: With XLineAlignment.BaseLine the height of the layout rectangle must be 0.");

            if (s.Length == 0)
                return;

            XFont font = new XFont(family.Name, emSize, style);
#if CORE
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddString");
#endif
#if (GDI || CORE_) && !WPF
            //Gfx.DrawString(text, font.Realize_GdiFont(), brush.RealizeGdiBrush(), rect,
            //  format != null ? format.RealizeGdiStringFormat() : null);

            if (family.GdiFamily == null)
                throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));

            RectangleF rect = layoutRect.ToRectangleF();
            rect.Offset(new PointF(0, SimulateBaselineOffset(family, style, emSize, format)));

            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, rect, format.RealizeGdiStringFormat());
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Closes the current figure and starts a new figure.
        /// </summary>
        public void CloseFigure()
        {
#if CORE
            _corePath.CloseSubpath();
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.CloseFigure();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
#if WPF || NETFX_CORE
            PathFigure figure = PeekCurrentFigure;
            if (figure != null && figure.Segments.Count != 0)
                figure.IsClosed = true;
#endif
        }

        /// <summary>
        /// Starts a new figure without closing the current figure.
        /// </summary>
        public void StartFigure()
        {
#if CORE
            // TODO: ???
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.StartFigure();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets an XFillMode that determines how the interiors of shapes are filled.
        /// </summary>
        public XFillMode FillMode
        {
            get { return _fillMode; }
            set
            {
                _fillMode = value;
#if CORE
                // Nothing to do.
#endif
#if GDI
                try
                {
                    Lock.EnterGdiPlus();
                    _gdipPath.FillMode = (FillMode)value;
                }
                finally { Lock.ExitGdiPlus(); }
#endif
            }
        }

        private XFillMode _fillMode;

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
        /// </summary>
        public void Flatten()
        {
#if CORE
            // Just do nothing.
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.Flatten();
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        /// <summary>
        /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
        /// </summary>
        public void Flatten(XMatrix matrix)
        {
#if CORE
            // Just do nothing.
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.Flatten(matrix.ToGdiMatrix());
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        /// <summary>
        /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
        /// </summary>
        public void Flatten(XMatrix matrix, double flatness)
        {
#if CORE
            // Just do nothing.
#endif
#if CORE___
            throw new NotImplementedException("XGraphicsPath.Flatten");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.Flatten(matrix.ToGdiMatrix(), (float)flatness);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
        /// by the specified pen.
        /// </summary>
        public void Widen(XPen pen)
        {
#if CORE
            // Just do nothing.
#endif
#if CORE___
            throw new NotImplementedException("XGraphicsPath.Widen");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.Widen(pen.RealizeGdiPen());
            }
            finally { Lock.ExitGdiPlus(); }
#endif

        }

        /// <summary>
        /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
        /// by the specified pen.
        /// </summary>
        public void Widen(XPen pen, XMatrix matrix)
        {
#if CORE
            // Just do nothing.
#endif
#if CORE
            throw new NotImplementedException("XGraphicsPath.Widen");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.Widen(pen.RealizeGdiPen(), matrix.ToGdiMatrix());
            }
            finally { Lock.ExitGdiPlus(); }
#endif

        }

        /// <summary>
        /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
        /// by the specified pen.
        /// </summary>
        public void Widen(XPen pen, XMatrix matrix, double flatness)
        {
#if CORE
            // Just do nothing.
#endif
#if CORE__
            throw new NotImplementedException("XGraphicsPath.Widen");
#endif
#if GDI
            try
            {
                Lock.EnterGdiPlus();
                _gdipPath.Widen(pen.RealizeGdiPen(), matrix.ToGdiMatrix(), (float)flatness);
            }
            finally { Lock.ExitGdiPlus(); }
#endif
        }

        /// <summary>
        /// Grants access to internal objects of this class.
        /// </summary>
        public XGraphicsPathInternals Internals
        {
            get { return new XGraphicsPathInternals(this); }
        }

#if CORE
        /// <summary>
        /// Gets access to underlying Core graphics path.
        /// </summary>
        internal CoreGraphicsPath _corePath;
#endif

#if GDI
        /// <summary>
        /// Gets access to underlying GDI+ graphics path.
        /// </summary>
        internal GraphicsPath _gdipPath;
#endif
    }
}