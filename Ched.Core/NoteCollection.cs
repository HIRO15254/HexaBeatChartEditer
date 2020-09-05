using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HexaBeatChartEditer.Core.Notes;

namespace HexaBeatChartEditer.Core
{
    /// <summary>
    /// ノーツを格納するコレクションを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class NoteCollection
    {
        //タップノーツ並べる
        [Newtonsoft.Json.JsonProperty]
        private List<Tap> taps;
        [Newtonsoft.Json.JsonProperty]
        private List<DTap> dTaps;
        [Newtonsoft.Json.JsonProperty]
        private List<HTap> hTaps;
        [Newtonsoft.Json.JsonProperty]
        private List<LTap> lTaps;
        //ホールドノーツ並べる
        [Newtonsoft.Json.JsonProperty]
        private List<Hold> holds;
        [Newtonsoft.Json.JsonProperty]
        private List<DHold> dHolds;
        [Newtonsoft.Json.JsonProperty]
        private List<HHold> hHolds;
        [Newtonsoft.Json.JsonProperty]
        private List<LHold> lHolds;
        //トレースノーツ並べる
        [Newtonsoft.Json.JsonProperty]
        private List<Trace> traces;
        [Newtonsoft.Json.JsonProperty]
        private List<DTrace> dTraces;
        [Newtonsoft.Json.JsonProperty]
        private List<HTrace> hTraces;
        [Newtonsoft.Json.JsonProperty]
        private List<LTrace> lTraces;
        [Newtonsoft.Json.JsonProperty]
        //こんなノーツないです
        private List<Flick> flicks;
        [Newtonsoft.Json.JsonProperty]
        private List<Damage> damages;
        [Newtonsoft.Json.JsonProperty]

        public List<Tap> Taps
        {
            get { return taps; }
            set { taps = value; }
        }

        public List<DTap> DTaps
        {
            get { return dTaps; }
            set { dTaps = value; }
        }

        public List<HTap> HTaps
        {
            get { return hTaps; }
            set { hTaps = value; }
        }

        public List<LTap> LTaps
        {
            get { return lTaps; }
            set { lTaps = value; }
        }

        public List<Hold> Holds
        {
            get { return holds; }
            set { holds = value; }
        }
        public List<DHold> DHolds
        {
            get { return dHolds; }
            set { dHolds = value; }
        }
        public List<HHold> HHolds
        {
            get { return hHolds; }
            set { hHolds = value; }
        }
        public List<LHold> LHolds
        {
            get { return lHolds; }
            set { lHolds = value; }
        }
        public List<Trace> Traces
        {
            get { return traces; }
            set { traces = value; }
        }
        public List<DTrace> DTraces
        {
            get { return dTraces; }
            set { dTraces = value; }
        }
        public List<HTrace> HTraces
        {
            get { return hTraces; }
            set { hTraces = value; }
        }
        public List<LTrace> LTraces
        {
            get { return lTraces; }
            set { lTraces = value; }
        }
        public List<Flick> Flicks
        {
            get { return flicks; }
            set { flicks = value; }
        }

        public List<Damage> Damages
        {
            get { return damages; }
            set { damages = value; }
        }

        public NoteCollection()
        {
            Taps = new List<Tap>();
            DTaps = new List<DTap>();
            HTaps = new List<HTap>();
            LTaps = new List<LTap>();

            Holds = new List<Hold>();
            DHolds = new List<DHold>();
            HHolds = new List<HHold>();
            LHolds = new List<LHold>();

            Traces = new List<Trace>();
            DTraces = new List<DTrace>();
            HTraces = new List<HTrace>();
            LTraces = new List<LTrace>();

            Flicks = new List<Flick>();
            Damages = new List<Damage>();
        }

        public NoteCollection(NoteCollection collection)
        {
            Taps = collection.Taps.ToList();
            DTaps = collection.DTaps.ToList();
            HTaps = collection.HTaps.ToList();
            LTaps = collection.LTaps.ToList();

            Holds = collection.Holds.ToList();
            DHolds = collection.DHolds.ToList();
            HHolds = collection.HHolds.ToList();
            LHolds = collection.LHolds.ToList();

            Traces = collection.Traces.ToList();
            DTraces = collection.DTraces.ToList();
            HTraces = collection.HTraces.ToList();
            LTraces = collection.LTraces.ToList();

            Flicks = collection.Flicks.ToList();
            Damages = collection.Damages.ToList();
        }

        public IEnumerable<TappableBase> GetShortNotes()
        {
            return Taps.Cast<TappableBase>().Concat(DTaps).Concat(HTaps).Concat(LTaps).Concat(Traces).Concat(DTraces).Concat(HTraces).Concat(LTraces);
        }
        public IEnumerable<Hold> GetLongNotes()
        {
            return Holds.Cast<Hold>().Concat(DHolds).Concat(HHolds).Concat(LHolds);
        }

        public void UpdateTicksPerBeat(double factor)
        {
            foreach (var note in GetShortNotes())
                note.Tick = (int)(note.Tick * factor);

            foreach (var hold in GetLongNotes())
            {
                hold.StartTick = (int)(hold.StartTick * factor);
                hold.Duration = (int)(hold.Duration * factor);
            }
        }
    }
}
