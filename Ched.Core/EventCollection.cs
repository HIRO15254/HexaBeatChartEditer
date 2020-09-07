using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HexaBeatChartEditer.Core.Events;

namespace HexaBeatChartEditer.Core
{
    /// <summary>
    /// イベントを格納するコレクションを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class EventCollection
    {
        [Newtonsoft.Json.JsonProperty]
        private List<BPMChangeEvent> bpmChangeEvents = new List<BPMChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<TimeSignatureChangeEvent> timeSignatureChangeEvents = new List<TimeSignatureChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<HighSpeedChangeEvent> highSpeedChangeEvents = new List<HighSpeedChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<SplitLaneEvent> splitLaneEvents = new List<SplitLaneEvent>();

        public List<BPMChangeEvent> BPMChangeEvents
        {
            get { return bpmChangeEvents; }
            set { bpmChangeEvents = value; }
        }

        public List<TimeSignatureChangeEvent> TimeSignatureChangeEvents
        {
            get { return timeSignatureChangeEvents; }
            set { timeSignatureChangeEvents = value; }
        }

        public List<HighSpeedChangeEvent> HighSpeedChangeEvents
        {
            get { return highSpeedChangeEvents; }
            set { highSpeedChangeEvents = value; }
        }

        public List<SplitLaneEvent> SplitLaneEvents
        {
            get { return splitLaneEvents; }
            set { splitLaneEvents = value; }
        }

        public void UpdateTicksPerBeat(double factor)
        {
            var events = BPMChangeEvents.Cast<EventBase>()
                 .Concat(TimeSignatureChangeEvents)
                 .Concat(HighSpeedChangeEvents)
                 .Concat(SplitLaneEvents);
            foreach (var e in events)
                e.Tick = (int)(e.Tick * factor);
        }
    }
}
