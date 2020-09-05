using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HexaBeatChartEditer.Core;
using HexaBeatChartEditer.Core.Notes;
using HexaBeatChartEditer.Localization;

namespace HexaBeatChartEditer.Plugins
{
    public class ComboCalculator : IScorePlugin
    {
        public string DisplayName => PluginStrings.ComboCalculator;

        public void Run(IScorePluginArgs args)
        {
            var score = args.GetCurrentScore();
            var combo = CalculateCombo(score);
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("総コンボ数: {0}", combo.Total));
            sb.AppendLine(string.Format("TAP: {0}", combo.Tap));
            sb.AppendLine(string.Format("DTAP: {0}", combo.DTap));
            sb.AppendLine(string.Format("HTAP: {0}", combo.HTap));
            sb.AppendLine(string.Format("LTAP: {0}", combo.LTap));
            sb.AppendLine(string.Format("TRACE: {0}", combo.Trace));
            sb.AppendLine(string.Format("DTRACE: {0}", combo.DTrace));
            sb.AppendLine(string.Format("HTRACE: {0}", combo.HTrace));
            sb.AppendLine(string.Format("LTRACE: {0}", combo.LTrace));
            sb.AppendLine(string.Format("HOLD: {0}", combo.Hold));
            sb.AppendLine(string.Format("DHOLD: {0}", combo.DHold));
            sb.AppendLine(string.Format("HHOLD: {0}", combo.HHold));
            sb.AppendLine(string.Format("LHOLD: {0}", combo.LHold));

            MessageBox.Show(sb.ToString(), DisplayName);
        }

        protected ComboDetails CalculateCombo(Score score)
        {
            var combo = new ComboDetails();

            combo.Tap = score.Notes.Taps.Count / 2;
            combo.DTap = score.Notes.DTaps.Count;
            combo.HTap = score.Notes.HTaps.Count;
            combo.LTap = score.Notes.LTaps.Count;

            combo.Trace = score.Notes.Traces.Count;
            combo.DTrace = score.Notes.DTraces.Count;
            combo.HTrace = score.Notes.HTraces.Count;
            combo.LTrace = score.Notes.LTraces.Count;

            combo.Hold = score.Notes.Holds.Count;
            combo.DHold = score.Notes.DHolds.Count;
            combo.HHold = score.Notes.HHolds.Count;
            combo.LHold = score.Notes.LHolds.Count;

            return combo;
        }
       

        public struct ComboDetails
        {
            public int Total => TapTotal + TraceTotal + HoldTotal;

            public int TapTotal => Tap + DTap + HTap + LTap;
            public int HoldTotal => Hold + DHold + HHold + LHold;
            public int TraceTotal => Trace +  DTrace + HTrace + LTrace;

            public int Tap { get; set; }
            public int DTap { get; set; }
            public int HTap { get; set; }
            public int LTap { get; set; }
            public int Trace { get; set; }
            public int DTrace { get; set; }
            public int HTrace { get; set; }
            public int LTrace { get; set; }
            public int Hold { get; set; }
            public int DHold { get; set; }
            public int HHold { get; set; }
            public int LHold { get; set; }
        }
    }
}
