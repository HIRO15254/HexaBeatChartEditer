using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

using Ched.Core.Notes;

namespace Ched.Drawing
{
    public static class NoteGraphics
    {
        public static void DrawTap(this DrawingContext dc, RectangleF rect)
        {
            dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.TapColor, dc.ColorProfile.BorderColor);
        }

        public static void DrawExTap(this DrawingContext dc, RectangleF rect)
        {
            dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.ExTapColor, dc.ColorProfile.BorderColor);
        }

        public static void DrawFlick(this DrawingContext dc, RectangleF rect)
        {
            var foregroundRect = new RectangleF(rect.Left + rect.Width / 4, rect.Top, rect.Width / 2, rect.Height);
            dc.Graphics.DrawNoteBase(rect, dc.ColorProfile.FlickColor.Item1);
            dc.Graphics.DrawNoteBase(foregroundRect, dc.ColorProfile.FlickColor.Item2);
            dc.Graphics.DrawBorder(rect, dc.ColorProfile.BorderColor);
            dc.Graphics.DrawTapSymbol(foregroundRect);
        }

        public static void DrawDamage(this DrawingContext dc, RectangleF rect)
        {
            dc.Graphics.DrawSquarishNote(rect, dc.ColorProfile.DamageColor, dc.ColorProfile.BorderColor);
        }

        public static void DrawHoldBegin(this DrawingContext dc, RectangleF rect)
        {
            dc.DrawHoldEnd(rect);
            dc.Graphics.DrawTapSymbol(rect);
        }

        public static void DrawHoldEnd(this DrawingContext dc, RectangleF rect)
        {
            dc.Graphics.DrawNote(rect, dc.ColorProfile.HoldColor, dc.ColorProfile.BorderColor);
        }

        public static void DrawHoldBackground(this DrawingContext dc, RectangleF rect)
        {
            Color BackgroundEdgeColor = dc.ColorProfile.HoldBackgroundColor.DarkColor;
            Color BackgroundMiddleColor = dc.ColorProfile.HoldBackgroundColor.LightColor;

            var prevMode = dc.Graphics.SmoothingMode;
            dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(rect, BackgroundEdgeColor, BackgroundMiddleColor, LinearGradientMode.Vertical))
            {
                var blend = new ColorBlend(4)
                {
                    Colors = new Color[] { BackgroundEdgeColor, BackgroundMiddleColor, BackgroundMiddleColor, BackgroundEdgeColor },
                    Positions = new float[] { 0.0f, 0.3f, 0.7f, 1.0f }
                };
                brush.InterpolationColors = blend;
                dc.Graphics.FillRectangle(brush, rect);
            }
            dc.Graphics.SmoothingMode = prevMode;
        }
    }
    public class SlideStepElement
    {
        public PointF Point { get; set; }
        public float Width { get; set; }
    }
}
