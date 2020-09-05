using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexaBeatChartEditer.Drawing
{
    public static class GraphicsExtensions
    {
        // ref: http://csharphelper.com/blog/2016/01/draw-rounded-rectangles-in-c/
        internal static GraphicsPath ToRoundedPath(this RectangleF rect, float radius)
        {
            if (radius * 2 > Math.Min(rect.Width, rect.Height))
            {
                radius = Math.Min(rect.Width, rect.Height) / 2;
            }

            var path = new GraphicsPath();

            path.AddLine(rect.Left + radius * 0.86f, rect.Top, rect.Left, rect.Top + radius);
            path.AddLine(rect.Left, rect.Bottom - radius, rect.Left + radius * 0.86f, rect.Bottom);
            path.AddLine(rect.Right - radius * 0.86f, rect.Bottom, rect.Right, rect.Bottom - radius);
            path.AddLine(rect.Right, rect.Top + radius, rect.Right - radius * 0.86f, rect.Top);

            return path;
        }

        public static RectangleF Expand(this RectangleF rect, float size)
        {
            return rect.Expand(size, size);
        }

        public static RectangleF Expand(this RectangleF rect, float dx, float dy)
        {
            return new RectangleF(rect.Left - dx, rect.Top - dy, rect.Width + dx * 2, rect.Height + dy * 2);
        }
    }
}
