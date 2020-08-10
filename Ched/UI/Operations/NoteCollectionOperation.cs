using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Notes;

namespace Ched.UI.Operations
{
    public abstract class NoteCollectionOperation<T> : IOperation
    {
        protected T Note { get; }
        protected NoteView.NoteCollection Collection { get; }
        public abstract string Description { get; }

        public NoteCollectionOperation(NoteView.NoteCollection collection, T note)
        {
            Collection = collection;
            Note = note;
        }

        public abstract void Undo();
        public abstract void Redo();
    }

    public class InsertTapOperation : NoteCollectionOperation<Tap>
    {
        public override string Description { get { return "TAPの追加"; } }

        public InsertTapOperation(NoteView.NoteCollection collection, Tap note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveTapOperation : NoteCollectionOperation<Tap>
    {
        public override string Description { get { return "TAPの削除"; } }

        public RemoveTapOperation(NoteView.NoteCollection collection, Tap note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }

    public class InsertDTapOperation : NoteCollectionOperation<DTap>
    {
        public override string Description { get { return "DTAPの追加"; } }

        public InsertDTapOperation(NoteView.NoteCollection collection, DTap note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveDTapOperation : NoteCollectionOperation<DTap>
    {
        public override string Description { get { return "DTAPの削除"; } }

        public RemoveDTapOperation(NoteView.NoteCollection collection, DTap note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }

    public class InsertHoldOperation : NoteCollectionOperation<Hold>
    {
        public override string Description { get { return "HOLDの追加"; } }

        public InsertHoldOperation(NoteView.NoteCollection collection, Hold note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveHoldOperation : NoteCollectionOperation<Hold>
    {
        public override string Description { get { return "HOLDの削除"; } }

        public RemoveHoldOperation(NoteView.NoteCollection collection, Hold note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }

    public class InsertDHoldOperation : NoteCollectionOperation<DHold>
    {
        public override string Description { get { return "DHOLDの追加"; } }

        public InsertDHoldOperation(NoteView.NoteCollection collection, DHold note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveDHoldOperation : NoteCollectionOperation<DHold>
    {
        public override string Description { get { return "DHOLDの削除"; } }

        public RemoveDHoldOperation(NoteView.NoteCollection collection, DHold note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }

    public class InsertFlickOperation : NoteCollectionOperation<Flick>
    {
        public override string Description { get { return "FLICKの追加"; } }

        public InsertFlickOperation(NoteView.NoteCollection collection, Flick note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveFlickOperation : NoteCollectionOperation<Flick>
    {
        public override string Description { get { return "FLICKの削除"; } }

        public RemoveFlickOperation(NoteView.NoteCollection collection, Flick note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }


    public class InsertDamageOperation : NoteCollectionOperation<Damage>
    {
        public override string Description { get { return "ダメージノーツの追加"; } }

        public InsertDamageOperation(NoteView.NoteCollection collection, Damage note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveDamageOperation : NoteCollectionOperation<Damage>
    {
        public override string Description { get { return "ダメージノーツの削除"; } }

        public RemoveDamageOperation(NoteView.NoteCollection collection, Damage note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }
}
