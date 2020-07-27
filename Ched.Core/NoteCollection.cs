using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Notes;

namespace Ched.Core
{
    /// <summary>
    /// ノーツを格納するコレクションを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class NoteCollection
    {
        [Newtonsoft.Json.JsonProperty]
        private List<Tap> taps;
        [Newtonsoft.Json.JsonProperty]
        private List<DTap> dTaps;
        [Newtonsoft.Json.JsonProperty]
        private List<Hold> holds;
        [Newtonsoft.Json.JsonProperty]
        private List<DHold> dHolds;
        [Newtonsoft.Json.JsonProperty]
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
            Holds = new List<Hold>();
            DHolds = new List<DHold>();
            Flicks = new List<Flick>();
            Damages = new List<Damage>();
        }

        public NoteCollection(NoteCollection collection)
        {
            Taps = collection.Taps.ToList();
            DTaps = collection.DTaps.ToList();
            Holds = collection.Holds.ToList();
            DHolds = collection.DHolds.ToList();
            Flicks = collection.Flicks.ToList();
            Damages = collection.Damages.ToList();
        }

        public IEnumerable<TappableBase> GetShortNotes()
        {
            return Taps.Cast<TappableBase>().Concat(DTaps).Concat(Flicks).Concat(Damages);
        }
        public IEnumerable<Hold> GetLongNotes()
        {
            return Taps.Cast<Hold>().Concat(DHolds);
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
