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
        private List<ExTap> exTaps;
        [Newtonsoft.Json.JsonProperty]
        private List<Hold> holds;
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

        public List<ExTap> ExTaps
        {
            get { return exTaps; }
            set { exTaps = value; }
        }

        public List<Hold> Holds
        {
            get { return holds; }
            set { holds = value; }
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
            ExTaps = new List<ExTap>();
            Holds = new List<Hold>();
            Flicks = new List<Flick>();
            Damages = new List<Damage>();
        }

        public NoteCollection(NoteCollection collection)
        {
            Taps = collection.Taps.ToList();
            ExTaps = collection.ExTaps.ToList();
            Holds = collection.Holds.ToList();
            Flicks = collection.Flicks.ToList();
            Damages = collection.Damages.ToList();
        }

        public IEnumerable<TappableBase> GetShortNotes()
        {
            return Taps.Cast<TappableBase>().Concat(ExTaps).Concat(Flicks).Concat(Damages);
        }

        public void UpdateTicksPerBeat(double factor)
        {
            foreach (var note in GetShortNotes())
                note.Tick = (int)(note.Tick * factor);

            foreach (var hold in Holds)
            {
                hold.StartTick = (int)(hold.StartTick * factor);
                hold.Duration = (int)(hold.Duration * factor);
            }
        }
    }
}
