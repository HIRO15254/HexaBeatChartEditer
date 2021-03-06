﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HexaBeatChartEditer.Core.Notes;
using HexaBeatChartEditer.UI;

namespace HexaBeatChartEditer.UI.Operations
{
    public abstract class EditShortNoteOperation : IOperation
    {
        protected TappableBase Note { get; }
        public abstract string Description { get; }

        public EditShortNoteOperation(TappableBase note)
        {
            Note = note;
        }

        public abstract void Redo();
        public abstract void Undo();
    }

    public class MoveShortNoteOperation : EditShortNoteOperation
    {
        public override string Description { get { return "ノートを移動"; } }

        protected NotePosition BeforePosition { get; }
        protected NotePosition AfterPosition { get; }

        public MoveShortNoteOperation(TappableBase note, NotePosition before, NotePosition after) : base(note)
        {
            BeforePosition = before;
            AfterPosition = after;
        }

        public override void Redo()
        {
            Note.Tick = AfterPosition.Tick;
            Note.LaneIndex = AfterPosition.LaneIndex;
        }

        public override void Undo()
        {
            Note.Tick = BeforePosition.Tick;
            Note.LaneIndex = BeforePosition.LaneIndex;
        }

        public struct NotePosition
        {
            public int Tick { get; }
            public int LaneIndex { get; }

            public NotePosition(int tick, int laneIndex)
            {
                Tick = tick;
                LaneIndex = laneIndex;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is NotePosition)) return false;
                NotePosition other = (NotePosition)obj;
                return Tick == other.Tick && LaneIndex == other.LaneIndex;
            }

            public override int GetHashCode()
            {
                return Tick ^ LaneIndex;
            }

            public static bool operator ==(NotePosition a, NotePosition b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(NotePosition a, NotePosition b)
            {
                return !a.Equals(b);
            }
        }
    }
    /*
    public class ChangeShortNoteWidthOperation : EditShortNoteOperation
    {
        public override string Description { get { return "ノート幅の変更"; } }

        protected NotePosition BeforePosition { get; }
        protected NotePosition AfterPosition { get; }

        public ChangeShortNoteWidthOperation(TappableBase note, NotePosition before, NotePosition after) : base(note)
        {
            BeforePosition = before;
            AfterPosition = after;
        }

        public override void Redo()
        {
            Note.SetPosition(AfterPosition.LaneIndex, AfterPosition.Width);
        }

        public override void Undo()
        {
            Note.SetPosition(BeforePosition.LaneIndex, BeforePosition.Width);
        }

        public struct NotePosition
        {
            public int LaneIndex { get; }
            public int Width { get; }

            public NotePosition(int laneIndex, int width)
            {
                LaneIndex = laneIndex;
                Width = width;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is NotePosition)) return false;
                NotePosition other = (NotePosition)obj;
                return LaneIndex == other.LaneIndex && Width == other.Width;
            }

            public override int GetHashCode()
            {
                return LaneIndex ^ Width;
            }

            public static bool operator ==(NotePosition a, NotePosition b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(NotePosition a, NotePosition b)
            {
                return !a.Equals(b);
            }
        }
    }
    */
    public class ChangeLongNoteDurationOperation : IOperation
    {
        public string Description { get { return "HOLD長さの変更"; } }

        protected Hold Note { get; }
        protected int BeforeDuration { get; }
        protected int AfterDuration { get; }

        public ChangeLongNoteDurationOperation(Hold note, int beforeDuration, int afterDuration)
        {
            Note = note;
            BeforeDuration = beforeDuration;
            AfterDuration = afterDuration;
        }

        public void Redo()
        {
            Note.Duration = AfterDuration;
        }

        public void Undo()
        {
            Note.Duration = BeforeDuration;
        }
    }

    public class MoveLongNoteOperation : IOperation
    {
        public string Description { get { return "HOLDの移動"; } }

        protected Hold Note { get; }
        protected NotePosition BeforePosition { get; }
        protected NotePosition AfterPosition { get; }

        public MoveLongNoteOperation(Hold note, NotePosition before, NotePosition after)
        {
            Note = note;
            BeforePosition = before;
            AfterPosition = after;
        }

        public void Redo()
        {
            Note.StartTick = AfterPosition.StartTick;
            Note.SetPosition(AfterPosition.LaneIndex, AfterPosition.Width);
        }

        public void Undo()
        {
            Note.StartTick = BeforePosition.StartTick;
            Note.SetPosition(BeforePosition.LaneIndex, BeforePosition.Width);
        }

        public struct NotePosition
        {
            public int StartTick { get; }
            public int LaneIndex { get; }
            public int Width { get; set; }

            public NotePosition(int startTick, int laneIndex, int width)
            {
                StartTick = startTick;
                LaneIndex = laneIndex;
                Width = width;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is NotePosition)) return false;
                NotePosition other = (NotePosition)obj;
                return StartTick == other.StartTick && LaneIndex == other.LaneIndex && Width == other.Width;
            }

            public override int GetHashCode()
            {
                return StartTick ^ LaneIndex ^ Width;
            }

            public static bool operator ==(NotePosition a, NotePosition b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(NotePosition a, NotePosition b)
            {
                return !a.Equals(b);
            }
        }
    }
}
