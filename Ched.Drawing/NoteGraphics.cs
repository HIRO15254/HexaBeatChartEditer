using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

using HexaBeatChartEditer.Core.Notes;

namespace HexaBeatChartEditer.Drawing
{
    public static class NoteGraphics
    {
        public static void DrawTap(this DrawingContext dc, RectangleF rect)
        {
            dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.NColor, dc.ColorProfile.BorderColor);
        }
        public static void DrawDTap(this DrawingContext dc, RectangleF rect)
        {
            dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.DColor, dc.ColorProfile.BorderColor);
        }
        public static void DrawHTap(this DrawingContext dc, RectangleF rect)
        {
            dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.HColor, dc.ColorProfile.BorderColor);
        }
        public static void DrawLTap(this DrawingContext dc, RectangleF rect)
        {
            dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.LColor, dc.ColorProfile.BorderColor);
        }

        public static void DrawTrace(this DrawingContext dc, RectangleF rect)
        {
            rect.Y += rect.Height * 0.2f;
            rect.Height *= 0.6f;
            dc.Graphics.DrawTraceNote(rect, dc.ColorProfile.NColor, dc.ColorProfile.BorderColor);
        }
        public static void DrawDTrace(this DrawingContext dc, RectangleF rect)
        {
            rect.Y += rect.Height * 0.2f;
            rect.Height *= 0.6f;
            dc.Graphics.DrawTraceNote(rect, dc.ColorProfile.DColor, dc.ColorProfile.BorderColor);
        }
        public static void DrawHTrace(this DrawingContext dc, RectangleF rect)
        {
            rect.Y += rect.Height * 0.2f;
            rect.Height *= 0.6f;
            dc.Graphics.DrawTraceNote(rect, dc.ColorProfile.HColor, dc.ColorProfile.BorderColor);
        }
        public static void DrawLTrace(this DrawingContext dc, RectangleF rect)
        {
            rect.Y += rect.Height * 0.2f;
            rect.Height *= 0.6f;
            dc.Graphics.DrawTraceNote(rect, dc.ColorProfile.LColor, dc.ColorProfile.BorderColor);
        }



        public static void DrawHoldBackground(this DrawingContext dc, RectangleF rect)
        {
            Color BackgroundEdgeColor = dc.ColorProfile.NColor.DarkColor;
            Color BackgroundMiddleColor = dc.ColorProfile.NColor.LightColor;
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

        public static void DrawDHoldBackground(this DrawingContext dc, RectangleF rect)
        {
            Color BackgroundEdgeColor = dc.ColorProfile.DColor.DarkColor;
            Color BackgroundMiddleColor = dc.ColorProfile.DColor.LightColor;
            RectangleF rect2 = new RectangleF(rect.X + rect.Width * 0.1f, rect.Y, rect.Width * 0.8f, rect.Height);
            var prevMode = dc.Graphics.SmoothingMode;
            dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(rect2, BackgroundEdgeColor, BackgroundMiddleColor, LinearGradientMode.Horizontal))
            {
                var blend = new ColorBlend(3)
                {
                    Colors = new Color[] { BackgroundEdgeColor, BackgroundMiddleColor, BackgroundEdgeColor},
                    Positions = new float[] { 0.0f , 0.5f, 1.0f }
                };
                brush.InterpolationColors = blend;
                dc.Graphics.FillRectangle(brush, rect2);
            }
            dc.Graphics.SmoothingMode = prevMode;
        }

        public static void DrawHHoldBackground(this DrawingContext dc, RectangleF rect)
        {
            Color BackgroundEdgeColor = dc.ColorProfile.HColor.DarkColor;
            Color BackgroundMiddleColor = dc.ColorProfile.HColor.LightColor;
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

        public static void DrawLHoldBackground(this DrawingContext dc, RectangleF rect)
        {
            Color BackgroundEdgeColor = dc.ColorProfile.LColor.DarkColor;
            Color BackgroundMiddleColor = dc.ColorProfile.LColor.LightColor;
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
    }
}
