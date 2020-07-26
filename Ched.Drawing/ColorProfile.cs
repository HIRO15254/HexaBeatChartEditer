using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Drawing
{
    public class GradientColor
    {
        public Color DarkColor { get; set; }
        public Color LightColor { get; set; }

        public GradientColor(Color darkColor, Color lightColor)
        {
            DarkColor = darkColor;
            LightColor = lightColor;
        }
    }

    public class ColorProfile
    {
        public GradientColor BorderColor { get; set; }
        public GradientColor TapColor { get; set; }
        public GradientColor ExTapColor { get; set; }

        /// <summary>
        /// フリックを描画する際の<see cref="GradientColor"/>を指定します.
        /// Item1には背景色, Item2には前景色を指定します.
        /// </summary>
        public Tuple<GradientColor, GradientColor> FlickColor { get; set; }
        public GradientColor DamageColor { get; set; }
        public GradientColor HoldBackgroundColor { get; set; }
        public GradientColor HoldColor { get; set; }
    }
}
