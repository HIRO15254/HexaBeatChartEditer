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
            dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.DTapColor, dc.ColorProfile.BorderColor);
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
            RectangleF rect2 = new RectangleF(rect.X + rect.Width * 0.1f, rect.Y, rect.Width * 0.8f, rect.Height);

            var prevMode = dc.Graphics.SmoothingMode;
            dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(rect2, BackgroundEdgeColor, BackgroundMiddleColor, LinearGradientMode.Horizontal))
            {
                var blend = new ColorBlend(3)
                {
                    Colors = new Color[] { BackgroundEdgeColor, BackgroundMiddleColor, BackgroundEdgeColor },
                    Positions = new float[] { 0.0f, 0.5f, 1.0f }
                };
                brush.InterpolationColors = blend;
                dc.Graphics.FillRectangle(brush, rect2);
            }
            dc.Graphics.SmoothingMode = prevMode;
        }
        public static void DrawDHoldBegin(this DrawingContext dc, RectangleF rect)
        {
            dc.DrawDHoldEnd(rect);
            dc.Graphics.DrawTapSymbol(rect);
        }

        public static void DrawDHoldEnd(this DrawingContext dc, RectangleF rect)
        {
            dc.Graphics.DrawNote(rect, dc.ColorProfile.DHoldColor, dc.ColorProfile.BorderColor);
        }

        public static void DrawDHoldBackground(this DrawingContext dc, RectangleF rect)
        {
            Color BackgroundEdgeColor = dc.ColorProfile.DHoldBackgroundColor.DarkColor;
            Color BackgroundMiddleColor = dc.ColorProfile.DHoldBackgroundColor.LightColor;
            RectangleF rect2 = new RectangleF(rect.X + rect.Width * 0.1f, rect.Y, rect.Width * 0.8f, rect.Height);
            var prevMode = dc.Graphics.SmoothingMode;
            dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(rect2, BackgroundEdgeColor, BackgroundMiddleColor, LinearGradientMode.Horizontal))
            {
                var blend = new ColorBlend(5)
                {
                    Colors = new Color[] { BackgroundEdgeColor, BackgroundMiddleColor, BackgroundEdgeColor, BackgroundMiddleColor, BackgroundEdgeColor },
                    Positions = new float[] { 0.0f,0.3f, 0.5f,0.7f, 1.0f }
                };
                brush.InterpolationColors = blend;
                dc.Graphics.FillRectangle(brush, rect2);
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
