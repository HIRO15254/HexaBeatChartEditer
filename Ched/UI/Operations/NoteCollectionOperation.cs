using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HexaBeatChartEditer.Core.Notes;

namespace HexaBeatChartEditer.UI.Operations
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

        public InsertTapOperation(NoteView.NoteCollection collection, Tap note) : base(collection, note){}

        public override void Redo(){Collection.Add(Note);}

        public override void Undo(){Collection.Remove(Note);}
    }

    public class RemoveTapOperation : NoteCollectionOperation<Tap>
    {
        public override string Description { get { return "TAPの削除"; } }

        public RemoveTapOperation(NoteView.NoteCollection collection, Tap note) : base(collection, note){}

        public override void Redo(){Collection.Remove(Note);}

        public override void Undo(){Collection.Add(Note);}
    }

    public class InsertDTapOperation : NoteCollectionOperation<DTap>
    {
        public override string Description { get { return "DTAPの追加"; } }

        public InsertDTapOperation(NoteView.NoteCollection collection, DTap note) : base(collection, note){}

        public override void Redo(){Collection.Add(Note);}

        public override void Undo(){Collection.Remove(Note);}
    }

    public class RemoveDTapOperation : NoteCollectionOperation<DTap>
    {
        public override string Description { get { return "DTAPの削除"; } }

        public RemoveDTapOperation(NoteView.NoteCollection collection, DTap note) : base(collection, note){}

        public override void Redo(){Collection.Remove(Note);}

        public override void Undo(){Collection.Add(Note);}
    }

    public class InsertHTapOperation : NoteCollectionOperation<HTap>
    {
        public override string Description { get { return "HTAPの追加"; } }

        public InsertHTapOperation(NoteView.NoteCollection collection, HTap note) : base(collection, note){}

        public override void Redo(){Collection.Add(Note);}

        public override void Undo(){Collection.Remove(Note);}
    }

    public class RemoveHTapOperation : NoteCollectionOperation<HTap>
    {
        public override string Description { get { return "HTAPの削除"; } }

        public RemoveHTapOperation(NoteView.NoteCollection collection, HTap note) : base(collection, note){}

        public override void Redo(){Collection.Remove(Note);}

        public override void Undo(){Collection.Add(Note);}
    }

    public class InsertLTapOperation : NoteCollectionOperation<LTap>
    {
        public override string Description { get { return "LTAPの追加"; } }

        public InsertLTapOperation(NoteView.NoteCollection collection, LTap note) : base(collection, note){}

        public override void Redo(){Collection.Add(Note);}

        public override void Undo(){Collection.Remove(Note);}
    }

    public class RemoveLTapOperation : NoteCollectionOperation<LTap>
    {
        public override string Description { get { return "LTAPの削除"; } }

        public RemoveLTapOperation(NoteView.NoteCollection collection, LTap note) : base(collection, note){}

        public override void Redo(){Collection.Remove(Note);}

        public override void Undo(){Collection.Add(Note);}
    }

    public class InsertTraceOperation : NoteCollectionOperation<Trace>
    {
        public override string Description { get { return "TARCEの追加"; } }

        public InsertTraceOperation(NoteView.NoteCollection collection, Trace note) : base(collection, note){}

        public override void Redo(){Collection.Add(Note);}

        public override void Undo(){Collection.Remove(Note);}
    }

    public class RemoveTraceOperation : NoteCollectionOperation<Trace>
    {
        public override string Description { get { return "TRACEの削除"; } }

        public RemoveTraceOperation(NoteView.NoteCollection collection, Trace note) : base(collection, note){}

        public override void Redo(){Collection.Remove(Note);}

        public override void Undo(){Collection.Add(Note);}
    }
    public class InsertDTraceOperation : NoteCollectionOperation<DTrace>
    {
        public override string Description { get { return "DTARCEの追加"; } }

        public InsertDTraceOperation(NoteView.NoteCollection collection, DTrace note) : base(collection, note){}

        public override void Redo(){Collection.Add(Note);}

        public override void Undo(){Collection.Remove(Note);}
    }

    public class RemoveDTraceOperation : NoteCollectionOperation<DTrace>
    {
        public override string Description { get { return "DTRACEの削除"; } }

        public RemoveDTraceOperation(NoteView.NoteCollection collection, DTrace note) : base(collection, note){ }

        public override void Redo(){Collection.Remove(Note);}

        public override void Undo(){Collection.Add(Note);}
    }

    public class InsertHTraceOperation : NoteCollectionOperation<HTrace>
    {
        public override string Description { get { return "HTARCEの追加"; } }

        public InsertHTraceOperation(NoteView.NoteCollection collection, HTrace note) : base(collection, note){}

        public override void Redo(){Collection.Add(Note);}

        public override void Undo(){Collection.Remove(Note);}
    }

    public class RemoveHTraceOperation : NoteCollectionOperation<HTrace>
    {
        public override string Description { get { return "HTRACEの削除"; } }

        public RemoveHTraceOperation(NoteView.NoteCollection collection, HTrace note) : base(collection, note){}

        public override void Redo(){Collection.Remove(Note);}

        public override void Undo(){Collection.Add(Note);}
    }
    public class InsertLTraceOperation : NoteCollectionOperation<LTrace>
    {
        public override string Description { get { return "LTARCEの追加"; } }

        public InsertLTraceOperation(NoteView.NoteCollection collection, LTrace note) : base(collection, note) { }

        public override void Redo() { Collection.Add(Note); }

        public override void Undo() { Collection.Remove(Note); }
    }

    public class RemoveLTraceOperation : NoteCollectionOperation<LTrace>
    {
        public override string Description { get { return "LTRACEの削除"; } }

        public RemoveLTraceOperation(NoteView.NoteCollection collection, LTrace note) : base(collection, note) { }

        public override void Redo() { Collection.Remove(Note); }

        public override void Undo() { Collection.Add(Note); }
    }

    public class InsertHoldOperation : NoteCollectionOperation<Hold>
    {
        public override string Description { get { return "HOLDの追加"; } }

        public InsertHoldOperation(NoteView.NoteCollection collection, Hold note) : base(collection, note){}

        public override void Redo(){Collection.Add(Note);}

        public override void Undo(){Collection.Remove(Note);}
    }

    public class RemoveHoldOperation : NoteCollectionOperation<Hold>
    {
        public override string Description { get { return "HOLDの削除"; } }

        public RemoveHoldOperation(NoteView.NoteCollection collection, Hold note) : base(collection, note){}

        public override void Redo(){Collection.Remove(Note);}

        public override void Undo(){Collection.Add(Note);}
    }

    public class InsertDHoldOperation : NoteCollectionOperation<DHold>
    {
        public override string Description { get { return "DHOLDの追加"; } }

        public InsertDHoldOperation(NoteView.NoteCollection collection, DHold note) : base(collection, note){}

        public override void Redo(){Collection.Add(Note);}

        public override void Undo(){Collection.Remove(Note);}
    }

    public class RemoveDHoldOperation : NoteCollectionOperation<DHold>
    {
        public override string Description { get { return "DHOLDの削除"; } }

        public RemoveDHoldOperation(NoteView.NoteCollection collection, DHold note) : base(collection, note){}

        public override void Redo(){Collection.Remove(Note);}

        public override void Undo(){Collection.Add(Note);}
    }

    public class InsertHHoldOperation : NoteCollectionOperation<HHold>
    {
        public override string Description { get { return "HHOLDの追加"; } }

        public InsertHHoldOperation(NoteView.NoteCollection collection, HHold note) : base(collection, note) { }

        public override void Redo() { Collection.Add(Note); }

        public override void Undo() { Collection.Remove(Note); }
    }

    public class RemoveHHoldOperation : NoteCollectionOperation<HHold>
    {
        public override string Description { get { return "HHOLDの削除"; } }

        public RemoveHHoldOperation(NoteView.NoteCollection collection, HHold note) : base(collection, note) { }

        public override void Redo() { Collection.Remove(Note); }

        public override void Undo() { Collection.Add(Note); }
    }
    public class InsertLHoldOperation : NoteCollectionOperation<LHold>
    {
        public override string Description { get { return "LHOLDの追加"; } }

        public InsertLHoldOperation(NoteView.NoteCollection collection, LHold note) : base(collection, note) { }

        public override void Redo() { Collection.Add(Note); }

        public override void Undo() { Collection.Remove(Note); }
    }

    public class RemoveLHoldOperation : NoteCollectionOperation<LHold>
    {
        public override string Description { get { return "LHOLDの削除"; } }

        public RemoveLHoldOperation(NoteView.NoteCollection collection, LHold note) : base(collection, note) { }

        public override void Redo() { Collection.Remove(Note); }

        public override void Undo() { Collection.Add(Note); }
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
