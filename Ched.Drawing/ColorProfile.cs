using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexaBeatChartEditer.Drawing
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
        public GradientColor NColor { get; set; }
        public GradientColor DColor { get; set; }
        public GradientColor HColor { get; set; }
        public GradientColor LColor { get; set; }
    }
}
