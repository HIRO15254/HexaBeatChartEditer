﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Ched.Core;
using Ched.Core.Notes;
using Ched.Localization;

namespace Ched.Plugins
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
            sb.AppendLine(string.Format("DTAP: {0}", combo.Tap));
            sb.AppendLine(string.Format("HOLD: {0}", combo.Hold));
            sb.AppendLine(string.Format("DHOLD: {0}", combo.DHold));

            MessageBox.Show(sb.ToString(), DisplayName);
        }

        protected ComboDetails CalculateCombo(Score score)
        {
            var combo = new ComboDetails();
            combo.Tap += new int[]
            {
                score.Notes.Taps.Count,
                score.Notes.Damages.Count,
                score.Notes.Holds.Count
            }.Sum();

            combo.DTap += new int[]
            {
                score.Notes.DTaps.Count,
                score.Notes.DHolds.Count
            }.Sum();


            int barTick = 4 * score.TicksPerBeat;
            var bpmEvents = score.Events.BPMChangeEvents.OrderBy(p => p.Tick).ToList();
            Func<int, decimal> getHeadBpmAt = tick => (bpmEvents.LastOrDefault(p => p.Tick <= tick) ?? bpmEvents[0]).BPM;
            Func<int, decimal> getTailBpmAt = tick => (bpmEvents.LastOrDefault(p => p.Tick < tick) ?? bpmEvents[0]).BPM;
            Func<decimal, int> comboDivider = bpm => bpm < 120 ? 16 : (bpm < 240 ? 8 : 4);

            // コンボとしてカウントされるstartTickからのオフセットを求める
            Func<int, IEnumerable<int>, List<int>> calcComboTicks = (startTick, stepTicks) =>
            {
                var tickList = new List<int>();
                var sortedStepTicks = stepTicks.OrderBy(p => p).ToList();
                int duration = sortedStepTicks[sortedStepTicks.Count - 1];
                int head = 0;
                int bpmIndex = 0;
                int stepIndex = 0;

                while (head < duration)
                {
                    while (bpmIndex + 1 < bpmEvents.Count && startTick + head >= bpmEvents[bpmIndex + 1].Tick) bpmIndex++;
                    int interval = barTick / comboDivider(bpmEvents[bpmIndex].BPM);
                    int diff = Math.Min(interval, sortedStepTicks[stepIndex] - head);
                    head += diff;
                    tickList.Add(head);
                    if (head == sortedStepTicks[stepIndex]) stepIndex++;
                }

                return tickList;
            };
            Func<IEnumerable<int>, int, int, IEnumerable<int>> removeLostTicks = (ticks, startTick, duration) =>
            {
                int interval = barTick / comboDivider(getTailBpmAt(startTick + duration));
                return ticks.Where(p => p <= duration - interval).ToList();
            };

            foreach (var hold in score.Notes.Holds)
            {
                var tickList = new HashSet<int>(calcComboTicks(hold.StartTick, new int[] { hold.Duration }));
                combo.Hold += tickList.Count;
            }

            foreach (var hold in score.Notes.DHolds)
            {
                var tickList = new HashSet<int>(calcComboTicks(hold.StartTick, new int[] { hold.Duration }));
                combo.DHold += tickList.Count;
            }

            return combo;
        }

        public struct ComboDetails
        {
            public int Total => Tap +  DTap + Hold + DHold;
            public int Tap { get; set; }
            public int DTap { get; set; }
            public int Hold { get; set; }
            public int DHold { get; set; }
        }
    }
}
