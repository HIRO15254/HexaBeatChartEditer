﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

using HexaBeatChartEditer.Core;
using HexaBeatChartEditer.Core.Notes;
using HexaBeatChartEditer.Drawing;
using HexaBeatChartEditer.UI.Operations;

namespace HexaBeatChartEditer.UI
{
    public partial class NoteView : Control
    {
        public event EventHandler HeadTickChanged;
        public event EventHandler EditModeChanged;
        public event EventHandler SelectedRangeChanged;
        public event EventHandler NewNoteTypeChanged;
        public event EventHandler DragScroll;

        private Color barLineColor = Color.FromArgb(160, 160, 160);
        private Color beatLineColor = Color.FromArgb(80, 80, 80);
        private Color laneBorderLightColor = Color.FromArgb(60, 60, 60);
        private Color laneBorderDarkColor = Color.FromArgb(30, 30, 30);
        private ColorProfile colorProfile;
        private int unitLaneWidth = 12;
        private int shortNoteHeight = 5;
        private int unitBeatTick = 480;
        private float unitBeatHeight = 120;

        private int headTick = 0;
        private bool editable = true;
        private EditMode editMode = EditMode.Edit;
        private int currentTick = 0;
        private SelectionRange selectedRange = SelectionRange.Empty;
        private NoteType newNoteType = NoteType.Tap;



        /// <summary>
        /// 小節の区切り線の色を設定します。
        /// </summary>
        public Color BarLineColor
        {
            get { return barLineColor; }
            set
            {
                barLineColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 1拍のガイド線の色を設定します。
        /// </summary>
        public Color BeatLineColor
        {
            get { return beatLineColor; }
            set
            {
                beatLineColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// レーンのガイド線のメインカラーを設定します。
        /// </summary>
        public Color LaneBorderLightColor
        {
            get { return laneBorderLightColor; }
            set
            {
                laneBorderLightColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// レーンのガイド線のサブカラーを設定します。
        /// </summary>
        public Color LaneBorderDarkColor
        {
            get { return laneBorderDarkColor; }
            set
            {
                laneBorderDarkColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// ノーツの描画に利用する<see cref="HexaBeatChartEditer.Drawing.ColorProfile"/>を取得します。
        /// </summary>
        public ColorProfile ColorProfile
        {
            get { return colorProfile; }
        }

        /// <summary>
        /// 1レーンあたりの表示幅を設定します。
        /// </summary>
        public int UnitLaneWidth
        {
            get { return unitLaneWidth; }
            set
            {
                unitLaneWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        /// レーンの表示幅を取得します。
        /// </summary>
        public int LaneWidth
        {
            get { return UnitLaneWidth * Constants.LanesCount + BorderThickness * (Constants.LanesCount - 1); }
        }

        /// <summary>
        /// レーンのガイド線の幅を取得します。
        /// </summary>
        public int BorderThickness => UnitLaneWidth < 5 ? 0 : 1;

        /// <summary>
        /// ショートノーツの表示高さを設定します。
        /// </summary>
        public int ShortNoteHeight
        {
            get { return shortNoteHeight; }
            set
            {
                shortNoteHeight = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 1拍あたりのTick数を設定します。
        /// </summary>
        public int UnitBeatTick
        {
            get { return unitBeatTick; }
            set
            {
                unitBeatTick = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 1拍あたりの表示高さを設定します。
        /// </summary>
        public float UnitBeatHeight
        {
            get { return unitBeatHeight; }
            set
            {
                // 6の倍数でいい感じに描画してくれる
                unitBeatHeight = value;
                Invalidate();
            }
        }

        /// <summary>
        /// クォンタイズを行うTick数を指定します。
        /// </summary>
        public double QuantizeTick { get; set; }

        /// <summary>
        /// 表示始端のTickを設定します。
        /// </summary>
        public int HeadTick
        {
            get { return headTick; }
            set
            {
                if (headTick == value) return;
                headTick = value;
                HeadTickChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        /// <summary>
        /// 表示終端のTickを取得します。
        /// </summary>
        public int TailTick
        {
            get { return HeadTick + (int)(ClientSize.Height * UnitBeatTick / UnitBeatHeight); }
        }

        /// <summary>
        /// 譜面始端の表示余白に充てるTickを取得します。
        /// </summary>
        public int PaddingHeadTick
        {
            get { return UnitBeatTick / 8; }
        }

        /// <summary>
        /// ノーツが編集可能かどうかを示す値を設定します。
        /// </summary>
        public bool Editable
        {
            get { return editable; }
            set
            {
                editable = value;
                Cursor = value ? Cursors.Default : Cursors.No;
            }
        }

        /// <summary>
        /// 編集モードを設定します。
        /// </summary>
        public EditMode EditMode
        {
            get { return editMode; }
            set
            {
                editMode = value;
                EditModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 現在のTickを設定します。
        /// </summary>
        public int CurrentTick
        {
            get { return currentTick; }
            set
            {
                currentTick = value;
                if (currentTick < HeadTick || currentTick > TailTick)
                {
                    HeadTick = currentTick;
                    DragScroll?.Invoke(this, EventArgs.Empty);
                }
                Invalidate();
            }
        }

        /// <summary>
        /// 現在の選択範囲を設定します。
        /// </summary>
        public SelectionRange SelectedRange
        {
            get { return selectedRange; }
            set
            {
                selectedRange = value;
                SelectedRangeChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        /// <summary>
        /// 追加するノート種別を設定します。
        /// </summary>
        public NoteType NewNoteType
        {
            get { return newNoteType; }
            set
            {
                int bits = (int)value;
                bool isSingle = bits != 0 && (bits & (bits - 1)) == 0;
                if (!isSingle) throw new ArgumentException("value", "value must be single bit.");
                newNoteType = value;
                NewNoteTypeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// ノート端の当たり判定幅の下限を取得します。
        /// </summary>

        protected int LastWidth { get; set; } = 4;

        public bool CanUndo { get { return OperationManager.CanUndo; } }

        public bool CanRedo { get { return OperationManager.CanRedo; } }

        public NoteCollection Notes { get; private set; } = new NoteCollection(new Core.NoteCollection());

        public EventCollection ScoreEvents { get; set; } = new EventCollection();

        protected OperationManager OperationManager { get; }

        protected CompositeDisposable Subscriptions { get; } = new CompositeDisposable();

        private Dictionary<Score, NoteCollection> NoteCollectionCache { get; } = new Dictionary<Score, NoteCollection>();

        public NoteView(OperationManager manager)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Opaque, true);

            OperationManager = manager;

            QuantizeTick = UnitBeatTick;

            colorProfile = new ColorProfile()
            {
                BorderColor = new GradientColor(Color.FromArgb(160, 160, 160), Color.FromArgb(208, 208, 208)),
                NColor = new GradientColor(Color.FromArgb(200, 58, 255, 25), Color.FromArgb(200, 255 ,255,255)),
                DColor = new GradientColor(Color.FromArgb(200, 206, 25, 255),Color.FromArgb(200, 255, 255, 255)),
                HColor = new GradientColor(Color.FromArgb(200, 255, 25, 25), Color.FromArgb(200, 255, 255, 255)),
                LColor = new GradientColor(Color.FromArgb(200, 41, 25, 255), Color.FromArgb(200, 255, 255, 255))
            };

            var mouseDown = this.MouseDownAsObservable();
            var mouseMove = this.MouseMoveAsObservable();
            var mouseUp = this.MouseUpAsObservable();

            // マウスをクリックしているとき以外
            var mouseMoveSubscription = mouseMove.TakeUntil(mouseDown).Concat(mouseMove.SkipUntil(mouseUp).TakeUntil(mouseDown).Repeat())
                .Where(p => EditMode == EditMode.Edit && Editable)
                .Do(p =>
                {
                    var pos = GetDrawingMatrix(new Matrix()).GetInvertedMatrix().TransformPoint(p.Location);
                    int tailTick = TailTick;
                    Func<int, bool> visibleTick = t => t >= HeadTick && t <= tailTick;

                    var shortNotes = Enumerable.Empty<TappableBase>()
                        .Concat(Notes.Taps.Reverse())
                        .Concat(Notes.DTaps.Reverse())
                        .Concat(Notes.HTaps.Reverse())
                        .Concat(Notes.LTaps.Reverse())
                        .Concat(Notes.Traces.Reverse())
                        .Concat(Notes.DTraces.Reverse())
                        .Concat(Notes.HTraces.Reverse())
                        .Concat(Notes.LTraces.Reverse())
                        .Concat(Notes.Damages.Reverse())
                        .Concat(Notes.Flicks.Reverse())
                        .Where(q => visibleTick(q.Tick))
                        .Select(q => GetClickableRectFromNotePosition(q.Tick, q.LaneIndex, q.Width));

                    foreach (RectangleF rect in shortNotes)
                    {
                        if (!rect.Contains(pos)) continue;
                        Cursor = Cursors.SizeAll;
                        return;
                    }
                    foreach (RectangleF rect in shortNotes)
                    {
                        if (!rect.Contains(pos)) continue;
                        Cursor = Cursors.SizeAll;
                        return;
                    }

                    foreach (var hold in Notes.Holds.Concat(Notes.DHolds).Concat(Notes.HHolds).Concat(Notes.LHolds).Reverse())
                    {
                        if (GetClickableRectFromNotePosition(hold.EndNote.Tick, hold.LaneIndex, hold.Width).Contains(pos))
                        {
                            Cursor = Cursors.SizeNS;
                            return;
                        }

                        RectangleF rect = GetClickableRectFromNotePosition(hold.StartTick, hold.LaneIndex, hold.Width);
                        if (!rect.Contains(pos)) continue;
                        Cursor = Cursors.SizeAll;
                        return;
                    }

                    Cursor = Cursors.Default;
                })
                .Subscribe();

            var dragSubscription = mouseDown
                .SelectMany(p => mouseMove.TakeUntil(mouseUp).TakeUntil(mouseUp)
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(200)).TakeUntil(mouseUp), (q, r) => q)
                    .Sample(TimeSpan.FromMilliseconds(200), new ControlScheduler(this))
                    .Do(q =>
                    {
                        // コントロール端にドラッグされたらスクロールする
                        if (q.Y <= ClientSize.Height * 0.1)
                        {
                            HeadTick += UnitBeatTick;
                            DragScroll?.Invoke(this, EventArgs.Empty);
                        }
                        else if (q.Y >= ClientSize.Height * 0.9)
                        {
                            HeadTick -= HeadTick + PaddingHeadTick < UnitBeatTick ? HeadTick + PaddingHeadTick : UnitBeatTick;
                            DragScroll?.Invoke(this, EventArgs.Empty);
                        }
                    })).Subscribe();

            var editSubscription = mouseDown
                .Where(p => Editable)
                .Where(p => p.Button == MouseButtons.Left && EditMode == EditMode.Edit)
                .SelectMany(p =>
                {
                    int tailTick = TailTick;
                    var from = p.Location;
                    Matrix matrix = GetDrawingMatrix(new Matrix());
                    matrix.Invert();
                    PointF scorePos = matrix.TransformPoint(p.Location);

                    // そもそも描画領域外であれば何もしない
                    RectangleF scoreRect = new RectangleF(0, GetYPositionFromTick(HeadTick), LaneWidth, GetYPositionFromTick(TailTick) - GetYPositionFromTick(HeadTick));
                    if (!scoreRect.Contains(scorePos)) return Observable.Empty<MouseEventArgs>();

                    Func<TappableBase, IObservable<MouseEventArgs>> moveTappableNoteHandler = note =>
                    {
                        int beforeLaneIndex = note.LaneIndex;
                        return mouseMove
                            .TakeUntil(mouseUp)
                            .Do(q =>
                            {
                                var currentScorePos = GetDrawingMatrix(new Matrix()).GetInvertedMatrix().TransformPoint(q.Location);
                                note.Tick = Math.Max(GetQuantizedTick(GetTickFromYPosition(currentScorePos.Y)), 0);
                                int xdiff = (int)((currentScorePos.X - scorePos.X) / (UnitLaneWidth + BorderThickness));
                                int laneIndex = beforeLaneIndex + xdiff;
                                note.LaneIndex = Math.Min(Constants.LanesCount - note.Width, Math.Max(0, laneIndex));
                                Cursor.Current = Cursors.SizeAll;
                            })
                            .Finally(() => Cursor.Current = Cursors.Default);
                    };



                    Func<TappableBase, IObservable<MouseEventArgs>> shortNoteHandler = note =>
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(note.Tick, note.LaneIndex, note.Width);

                        // ノート本体
                        if (rect.Contains(scorePos))
                        {
                            var beforePos = new MoveShortNoteOperation.NotePosition(note.Tick, note.LaneIndex);
                            return moveTappableNoteHandler(note)
                                .Finally(() =>
                                {
                                    var afterPos = new MoveShortNoteOperation.NotePosition(note.Tick, note.LaneIndex);
                                    if (beforePos == afterPos) return;
                                    OperationManager.Push(new MoveShortNoteOperation(note, beforePos, afterPos));
                                });
                        }

                        return null;
                    };

                    Func<Hold, IObservable<MouseEventArgs>> holdDurationHandler = hold =>
                    {
                        return mouseMove.TakeUntil(mouseUp)
                            .Do(q =>
                            {
                                var currentScorePos = GetDrawingMatrix(new Matrix()).GetInvertedMatrix().TransformPoint(q.Location);
                                hold.Duration = (int)Math.Max(QuantizeTick, GetQuantizedTick(GetTickFromYPosition(currentScorePos.Y)) - hold.StartTick);
                                Cursor.Current = Cursors.SizeNS;
                            })
                            .Finally(() => Cursor.Current = Cursors.Default);
                    };
                   

                    Func<Hold, IObservable<MouseEventArgs>> holdHandler = hold =>
                    {
                        // HOLD長さ変更
                        if (GetClickableRectFromNotePosition(hold.EndNote.Tick, hold.LaneIndex, hold.Width).Contains(scorePos))
                        {
                            int beforeDuration = hold.Duration;
                            return holdDurationHandler(hold)
                                .Finally(() =>
                                {
                                    if (beforeDuration == hold.Duration) return;
                                    OperationManager.Push(new ChangeLongNoteDurationOperation(hold, beforeDuration, hold.Duration));
                                });
                        }

                        RectangleF startRect = GetClickableRectFromNotePosition(hold.StartTick, hold.LaneIndex, hold.Width);

                        var beforePos = new MoveLongNoteOperation.NotePosition(hold.StartTick, hold.LaneIndex, hold.Width);

                        if (startRect.Contains(scorePos))
                        {
                            return mouseMove
                                .TakeUntil(mouseUp)
                                .Do(q =>
                                {
                                    var currentScorePos = GetDrawingMatrix(new Matrix()).GetInvertedMatrix().TransformPoint(q.Location);
                                    hold.StartTick = Math.Max(GetQuantizedTick(GetTickFromYPosition(currentScorePos.Y)), 0);
                                    int xdiff = (int)((currentScorePos.X - scorePos.X) / (UnitLaneWidth + BorderThickness));
                                    int laneIndex = beforePos.LaneIndex + xdiff;
                                    hold.LaneIndex = Math.Min(Constants.LanesCount - hold.Width, Math.Max(0, laneIndex));
                                    Cursor.Current = Cursors.SizeAll;
                                })
                                .Finally(() =>
                                {
                                    Cursor.Current = Cursors.Default;
                                    LastWidth = hold.Width;
                                    var afterPos = new MoveLongNoteOperation.NotePosition(hold.StartTick, hold.LaneIndex, hold.Width);
                                    if (beforePos == afterPos) return;
                                    OperationManager.Push(new MoveLongNoteOperation(hold, beforePos, afterPos));
                                });
                        }

                        return null;
                    };

                    Func<IObservable<MouseEventArgs>> surfaceNotesHandler = () =>
                    {
                        foreach (var note in Notes.Taps.Reverse().Where(q => q.Tick >= HeadTick && q.Tick <= tailTick))
                        {
                            var subscription = shortNoteHandler(note);
                            if (subscription != null) return subscription;
                        }
                        foreach (var note in Notes.DTaps.Reverse().Where(q => q.Tick >= HeadTick && q.Tick <= tailTick))
                        {
                            var subscription = shortNoteHandler(note);
                            if (subscription != null) return subscription;
                        }
                        foreach (var note in Notes.HTaps.Reverse().Where(q => q.Tick >= HeadTick && q.Tick <= tailTick))
                        {
                            var subscription = shortNoteHandler(note);
                            if (subscription != null) return subscription;
                        }
                        foreach (var note in Notes.LTaps.Reverse().Where(q => q.Tick >= HeadTick && q.Tick <= tailTick))
                        {
                            var subscription = shortNoteHandler(note);
                            if (subscription != null) return subscription;
                        }

                        foreach (var note in Notes.Traces.Reverse().Where(q => q.Tick >= HeadTick && q.Tick <= tailTick))
                        {
                            var subscription = shortNoteHandler(note);
                            if (subscription != null) return subscription;
                        }
                        foreach (var note in Notes.DTraces.Reverse().Where(q => q.Tick >= HeadTick && q.Tick <= tailTick))
                        {
                            var subscription = shortNoteHandler(note);
                            if (subscription != null) return subscription;
                        }
                        foreach (var note in Notes.HTraces.Reverse().Where(q => q.Tick >= HeadTick && q.Tick <= tailTick))
                        {
                            var subscription = shortNoteHandler(note);
                            if (subscription != null) return subscription;
                        }
                        foreach (var note in Notes.LTraces.Reverse().Where(q => q.Tick >= HeadTick && q.Tick <= tailTick))
                        {
                            var subscription = shortNoteHandler(note);
                            if (subscription != null) return subscription;
                        }

                        foreach (var note in Notes.Holds.Reverse().Where(q => q.StartTick <= tailTick && q.StartTick + q.GetDuration() >= HeadTick))
                        {
                            var subscription = holdHandler(note);
                            if (subscription != null) return subscription;
                        }
                        foreach (var note in Notes.DHolds.Reverse().Where(q => q.StartTick <= tailTick && q.StartTick + q.GetDuration() >= HeadTick))
                        {
                            var subscription = holdHandler(note);
                            if (subscription != null) return subscription;
                        }
                        foreach (var note in Notes.HHolds.Reverse().Where(q => q.StartTick <= tailTick && q.StartTick + q.GetDuration() >= HeadTick))
                        {
                            var subscription = holdHandler(note);
                            if (subscription != null) return subscription;
                        }
                        foreach (var note in Notes.LHolds.Reverse().Where(q => q.StartTick <= tailTick && q.StartTick + q.GetDuration() >= HeadTick))
                        {
                            var subscription = holdHandler(note);
                            if (subscription != null) return subscription;
                        }

                        return null;
                    };

                    var subscription2 = surfaceNotesHandler();
                    if (subscription2 != null) return subscription2;

                    // なんもねえなら追加だァ！
                    if ((NoteType.Tap | NoteType.DTap | NoteType.HTap | NoteType.LTap | NoteType.Trace | NoteType.DTrace | NoteType.HTrace | NoteType.LTrace).HasFlag(NewNoteType))
                    {
                        TappableBase newNote = null;
                        IOperation op = null;
                        switch (NewNoteType)
                        {
                            case NoteType.Tap:
                                var tap = new Tap();
                                Notes.Add(tap);
                                newNote = tap;
                                op = new InsertTapOperation(Notes, tap);
                                break;

                            case NoteType.DTap:
                                var dtap = new DTap();
                                Notes.Add(dtap);
                                newNote = dtap;
                                op = new InsertDTapOperation(Notes, dtap);
                                break;

                            case NoteType.HTap:
                                var htap = new HTap();
                                Notes.Add(htap);
                                newNote = htap;
                                op = new InsertHTapOperation(Notes, htap);
                                break;

                            case NoteType.LTap:
                                var ltap = new LTap();
                                Notes.Add(ltap);
                                newNote = ltap;
                                op = new InsertLTapOperation(Notes, ltap);
                                break;

                            case NoteType.Trace:
                                var trace = new Trace();
                                Notes.Add(trace);
                                newNote = trace;
                                op = new InsertTraceOperation(Notes, trace);
                                break;

                            case NoteType.DTrace:
                                var dtrace = new DTrace();
                                Notes.Add(dtrace);
                                newNote = dtrace;
                                op = new InsertDTraceOperation(Notes, dtrace);
                                break;

                            case NoteType.HTrace:
                                var htrace = new HTrace();
                                Notes.Add(htrace);
                                newNote = htrace;
                                op = new InsertHTraceOperation(Notes, htrace);
                                break;

                            case NoteType.LTrace:
                                var ltrace = new LTrace();
                                Notes.Add(ltrace);
                                newNote = ltrace;
                                op = new InsertLTraceOperation(Notes, ltrace);
                                break;
                        }

                        newNote.Width = 1;
                        newNote.Tick = Math.Max(GetQuantizedTick(GetTickFromYPosition(scorePos.Y)), 0);
                        int newNoteLaneIndex = (int)(scorePos.X / (UnitLaneWidth + BorderThickness)) - newNote.Width / 2;
                        newNoteLaneIndex = Math.Min(Constants.LanesCount - newNote.Width, Math.Max(0, newNoteLaneIndex));
                        newNote.LaneIndex = newNoteLaneIndex;
                        Invalidate();
                        return moveTappableNoteHandler(newNote)
                            .Finally(() => OperationManager.Push(op));
                    }
                    else
                    {
                        int newNoteLaneIndex;

                        switch (NewNoteType)
                        {
                            case NoteType.Hold:
                                var hold = new Hold
                                {
                                    StartTick = Math.Max(GetQuantizedTick(GetTickFromYPosition(scorePos.Y)), 0),
                                    Width = 1,
                                    Duration = (int)QuantizeTick
                                };
                                newNoteLaneIndex = (int)(scorePos.X / (UnitLaneWidth + BorderThickness)) - hold.Width / 2;
                                hold.LaneIndex = Math.Min(Constants.LanesCount - hold.Width, Math.Max(0, newNoteLaneIndex));
                                Notes.Add(hold);
                                Invalidate();
                                return holdDurationHandler(hold)
                                    .Finally(() => OperationManager.Push(new InsertHoldOperation(Notes, hold)));

                            case NoteType.DHold:
                                var dhold = new DHold
                                {
                                    StartTick = Math.Max(GetQuantizedTick(GetTickFromYPosition(scorePos.Y)), 0),
                                    Width = 1,
                                    Duration = (int)QuantizeTick
                                };
                                newNoteLaneIndex = (int)(scorePos.X / (UnitLaneWidth + BorderThickness)) - dhold.Width / 2;
                                dhold.LaneIndex = Math.Min(Constants.LanesCount - dhold.Width, Math.Max(0, newNoteLaneIndex));
                                Notes.Add(dhold);
                                Invalidate();
                                return holdDurationHandler(dhold)
                                    .Finally(() => OperationManager.Push(new InsertDHoldOperation(Notes, dhold)));

                            case NoteType.HHold:
                                var hhold = new HHold
                                {
                                    StartTick = Math.Max(GetQuantizedTick(GetTickFromYPosition(scorePos.Y)), 0),
                                    Width = 1,
                                    Duration = (int)QuantizeTick
                                };
                                newNoteLaneIndex = (int)(scorePos.X / (UnitLaneWidth + BorderThickness)) - hhold.Width / 2;
                                hhold.LaneIndex = Math.Min(Constants.LanesCount - hhold.Width, Math.Max(0, newNoteLaneIndex));
                                Notes.Add(hhold);
                                Invalidate();
                                return holdDurationHandler(hhold)
                                    .Finally(() => OperationManager.Push(new InsertHHoldOperation(Notes, hhold)));

                            case NoteType.LHold:
                                var lhold = new LHold
                                {
                                    StartTick = Math.Max(GetQuantizedTick(GetTickFromYPosition(scorePos.Y)), 0),
                                    Width = 1,
                                    Duration = (int)QuantizeTick
                                };
                                newNoteLaneIndex = (int)(scorePos.X / (UnitLaneWidth + BorderThickness)) - lhold.Width / 2;
                                lhold.LaneIndex = Math.Min(Constants.LanesCount - lhold.Width, Math.Max(0, newNoteLaneIndex));
                                Notes.Add(lhold);
                                Invalidate();
                                return holdDurationHandler(lhold)
                                    .Finally(() => OperationManager.Push(new InsertLHoldOperation(Notes, lhold)));
                        }
                    }
                    return Observable.Empty<MouseEventArgs>();
                }).Subscribe(p => Invalidate());

            Func<PointF, IObservable<MouseEventArgs>> rangeSelection = startPos =>
            {
                SelectedRange = new SelectionRange()
                {
                    StartTick = Math.Max(GetQuantizedTick(GetTickFromYPosition(startPos.Y)), 0),
                    Duration = 0,
                    StartLaneIndex = 0,
                    SelectedLanesCount = 0
                };

                return mouseMove.TakeUntil(mouseUp)
                    .Do(q =>
                    {
                        Matrix currentMatrix = GetDrawingMatrix(new Matrix());
                        currentMatrix.Invert();
                        var scorePos = currentMatrix.TransformPoint(q.Location);

                        int startLaneIndex = Math.Min(Math.Max((int)startPos.X / (UnitLaneWidth + BorderThickness), 0), Constants.LanesCount - 1);
                        int endLaneIndex = Math.Min(Math.Max((int)scorePos.X / (UnitLaneWidth + BorderThickness), 0), Constants.LanesCount - 1);
                        int endTick = GetQuantizedTick(GetTickFromYPosition(scorePos.Y));

                        SelectedRange = new SelectionRange()
                        {
                            StartTick = SelectedRange.StartTick,
                            Duration = endTick - SelectedRange.StartTick,
                            StartLaneIndex = Math.Min(startLaneIndex, endLaneIndex),
                            SelectedLanesCount = Math.Abs(endLaneIndex - startLaneIndex) + 1
                        };
                    });
            };

            var eraseSubscription = mouseDown
                .Where(p => Editable)
                .Where(p => p.Button == MouseButtons.Left && EditMode == EditMode.Erase)
                .SelectMany(p =>
                {
                    Matrix startMatrix = GetDrawingMatrix(new Matrix());
                    startMatrix.Invert();
                    PointF startScorePos = startMatrix.TransformPoint(p.Location);
                    return rangeSelection(startScorePos)
                        .Count()
                        .Zip(mouseUp, (q, r) => new { Pos = r.Location, Count = q });
                })
                .Do(p =>
                {
                    if (p.Count > 0) // ドラッグで範囲選択された
                    {
                        RemoveSelectedNotes();
                        SelectedRange = SelectionRange.Empty;
                        return;
                    }

                    Matrix matrix = GetDrawingMatrix(new Matrix());
                    matrix.Invert();
                    PointF scorePos = matrix.TransformPoint(p.Pos);

                    foreach (var note in Notes.Taps.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(note.Tick, note.LaneIndex, note.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveTapOperation(Notes, note);
                            Notes.Remove(note);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var note in Notes.DTaps.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(note.Tick, note.LaneIndex, note.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveDTapOperation(Notes, note);
                            Notes.Remove(note);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var note in Notes.HTaps.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(note.Tick, note.LaneIndex, note.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveHTapOperation(Notes, note);
                            Notes.Remove(note);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var note in Notes.LTaps.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(note.Tick, note.LaneIndex, note.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveLTapOperation(Notes, note);
                            Notes.Remove(note);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var note in Notes.Traces.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(note.Tick, note.LaneIndex, note.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveTraceOperation(Notes, note);
                            Notes.Remove(note);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var note in Notes.DTraces.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(note.Tick, note.LaneIndex, note.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveDTraceOperation(Notes, note);
                            Notes.Remove(note);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var note in Notes.HTraces.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(note.Tick, note.LaneIndex, note.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveHTraceOperation(Notes, note);
                            Notes.Remove(note);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var note in Notes.LTraces.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(note.Tick, note.LaneIndex, note.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveLTraceOperation(Notes, note);
                            Notes.Remove(note);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var hold in Notes.Holds.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(hold.StartTick, hold.LaneIndex, hold.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveHoldOperation(Notes, hold);
                            Notes.Remove(hold);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var hold in Notes.DHolds.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(hold.StartTick, hold.LaneIndex, hold.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveDHoldOperation(Notes, hold);
                            Notes.Remove(hold);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var hold in Notes.HHolds.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(hold.StartTick, hold.LaneIndex, hold.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveHHoldOperation(Notes, hold);
                            Notes.Remove(hold);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                    foreach (var hold in Notes.LHolds.Reverse())
                    {
                        RectangleF rect = GetClickableRectFromNotePosition(hold.StartTick, hold.LaneIndex, hold.Width);
                        if (rect.Contains(scorePos))
                        {
                            var op = new RemoveLHoldOperation(Notes, hold);
                            Notes.Remove(hold);
                            OperationManager.Push(op);
                            return;
                        }
                    }

                })
                .Subscribe(p => Invalidate());

            var selectSubscription = mouseDown
                .Where(p => Editable)
                .Where(p => p.Button == MouseButtons.Left && EditMode == EditMode.Select)
                .SelectMany(p =>
                {
                    Matrix startMatrix = GetDrawingMatrix(new Matrix());
                    startMatrix.Invert();
                    PointF startScorePos = startMatrix.TransformPoint(p.Location);

                    if (GetSelectionRect().Contains(Point.Ceiling(startScorePos)))
                    {
                        int minTick = SelectedRange.StartTick + (SelectedRange.Duration < 0 ? SelectedRange.Duration : 0);
                        int maxTick = SelectedRange.StartTick + (SelectedRange.Duration < 0 ? 0 : SelectedRange.Duration);
                        int startTick = SelectedRange.StartTick;
                        int startLaneIndex = SelectedRange.StartLaneIndex;
                        int endLaneIndex = SelectedRange.StartLaneIndex + SelectedRange.SelectedLanesCount;

                        var selectedNotes = GetSelectedNotes();
                        var dicShortNotes = selectedNotes.GetShortNotes().ToDictionary(q => q, q => new MoveShortNoteOperation.NotePosition(q.Tick, q.LaneIndex));
                        var dicHolds = selectedNotes.GetLongNotes().ToDictionary(q => q, q => new MoveLongNoteOperation.NotePosition(q.StartTick, q.LaneIndex, q.Width));

                        // 選択範囲移動
                        return mouseMove.TakeUntil(mouseUp).Do(q =>
                        {
                            Matrix currentMatrix = GetDrawingMatrix(new Matrix());
                            currentMatrix.Invert();
                            var scorePos = currentMatrix.TransformPoint(q.Location);

                            int xdiff = (int)((scorePos.X - startScorePos.X) / (UnitLaneWidth + BorderThickness));
                            int laneIndex = startLaneIndex + xdiff;

                            SelectedRange = new SelectionRange()
                            {
                                StartTick = startTick + Math.Max(GetQuantizedTick(GetTickFromYPosition(scorePos.Y) - GetTickFromYPosition(startScorePos.Y)), -startTick - (SelectedRange.Duration < 0 ? SelectedRange.Duration : 0)),
                                Duration = SelectedRange.Duration,
                                StartLaneIndex = Math.Min(Math.Max(laneIndex, 0), Constants.LanesCount - SelectedRange.SelectedLanesCount),
                                SelectedLanesCount = SelectedRange.SelectedLanesCount
                            };

                            foreach (var item in dicShortNotes)
                            {
                                item.Key.Tick = item.Value.Tick + (SelectedRange.StartTick - startTick);
                                item.Key.LaneIndex = item.Value.LaneIndex + (SelectedRange.StartLaneIndex - startLaneIndex);
                            }

                            // ロングノーツは全体が範囲内に含まれているもののみを対象にするので範囲外移動は考えてない
                            foreach (var item in dicHolds)
                            {
                                item.Key.StartTick = item.Value.StartTick + (SelectedRange.StartTick - startTick);
                                item.Key.LaneIndex = item.Value.LaneIndex + (SelectedRange.StartLaneIndex - startLaneIndex);
                            }

                            // AIR-ACTIONはOffsetの管理面倒で実装できませんでした。許せ

                            Invalidate();
                        })
                        .Finally(() =>
                        {
                            var opShortNotes = dicShortNotes.Select(q =>
                            {
                                var after = new MoveShortNoteOperation.NotePosition(q.Key.Tick, q.Key.LaneIndex);
                                return new MoveShortNoteOperation(q.Key, q.Value, after);
                            });

                            var opHolds = dicHolds.Select(q =>
                            {
                                var after = new MoveLongNoteOperation.NotePosition(q.Key.StartTick, q.Key.LaneIndex, q.Key.Width);
                                return new MoveLongNoteOperation(q.Key, q.Value, after);
                            });


                            // 同じ位置に戻ってきたら操作扱いにしない
                            if (startTick == SelectedRange.StartTick && startLaneIndex == SelectedRange.StartLaneIndex) return;
                            OperationManager.Push(new CompositeOperation("ノーツの移動", opShortNotes.Cast<IOperation>().Concat(opHolds).ToList()));
                        });
                    }
                    else
                    {
                        // 範囲選択
                        CurrentTick = Math.Max(GetQuantizedTick(GetTickFromYPosition(startScorePos.Y)), 0);
                        return rangeSelection(startScorePos);
                    }
                }).Subscribe();

            Subscriptions.Add(mouseMoveSubscription);
            Subscriptions.Add(dragSubscription);
            Subscriptions.Add(editSubscription);
            Subscriptions.Add(eraseSubscription);
            Subscriptions.Add(selectSubscription);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Matrix matrix = GetDrawingMatrix(new Matrix());
            matrix.Invert();

            if (EditMode == EditMode.Select && Editable)
            {
                var scorePos = matrix.TransformPoint(e.Location);
                Cursor = GetSelectionRect().Contains(scorePos) ? Cursors.SizeAll : Cursors.Default;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Right)
            {
                EditMode = EditMode == EditMode.Edit ? EditMode.Select : EditMode.Edit;
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            // Y軸の正方向をTick増加方向として描画 (y = 0 はコントロール下端)
            // コントロールの中心に描画したいなら後でTranslateしといてね
            var prevMatrix = pe.Graphics.Transform;
            pe.Graphics.Transform = GetDrawingMatrix(prevMatrix);

            var dc = new DrawingContext(pe.Graphics, ColorProfile);

            float laneWidth = LaneWidth;
            int tailTick = HeadTick + (int)(ClientSize.Height * UnitBeatTick / UnitBeatHeight);

            // レーン分割線描画
            using (var lightPen = new Pen(LaneBorderLightColor, BorderThickness))
            using (var darkPen = new Pen(LaneBorderDarkColor, BorderThickness))
            {
                for (int i = 0; i <= Constants.LanesCount; i++)
                {
                    float x = i * (UnitLaneWidth + BorderThickness);
                    pe.Graphics.DrawLine(lightPen, x, GetYPositionFromTick(HeadTick), x, GetYPositionFromTick(tailTick));
                }
            }


            // 時間ガイドの描画
            // そのイベントが含まれる小節(ただし[小節開始Tick, 小節開始Tick + 小節Tick)の範囲)からその拍子を適用
            var sigs = ScoreEvents.TimeSignatureChangeEvents.OrderBy(p => p.Tick).ToList();

            using (var beatPen = new Pen(BeatLineColor, BorderThickness))
            using (var barPen = new Pen(BarLineColor, BorderThickness))
            {
                // 最初の拍子
                int firstBarLength = UnitBeatTick * 4 * sigs[0].Numerator / sigs[0].Denominator;
                int barTick = UnitBeatTick * 4;

                for (int i = HeadTick / (barTick / sigs[0].Denominator); sigs.Count < 2 || i * barTick / sigs[0].Denominator < sigs[1].Tick / firstBarLength * firstBarLength; i++)
                {
                    int tick = i * barTick / sigs[0].Denominator;
                    float y = GetYPositionFromTick(tick);
                    pe.Graphics.DrawLine(i % sigs[0].Numerator == 0 ? barPen : beatPen, 0, y, laneWidth, y);
                    if (tick > tailTick) break;
                }

                // その後の拍子
                int pos = 0;
                for (int j = 1; j < sigs.Count; j++)
                {
                    int prevBarLength = barTick * sigs[j - 1].Numerator / sigs[j - 1].Denominator;
                    int currentBarLength = barTick * sigs[j].Numerator / sigs[j].Denominator;
                    pos += (sigs[j].Tick - pos) / prevBarLength * prevBarLength;
                    if (pos > tailTick) break;
                    for (int i = HeadTick - pos < 0 ? 0 : (HeadTick - pos) / (barTick / sigs[j].Denominator); pos + i * (barTick / sigs[j].Denominator) < tailTick; i++)
                    {
                        if (j < sigs.Count - 1 && i * barTick / sigs[j].Denominator >= (sigs[j + 1].Tick - pos) / currentBarLength * currentBarLength) break;
                        float y = GetYPositionFromTick(pos + i * barTick / sigs[j].Denominator);
                        pe.Graphics.DrawLine(i % sigs[j].Numerator == 0 ? barPen : beatPen, 0, y, laneWidth, y);
                    }
                }
            }

            using (var posPen = new Pen(Color.FromArgb(196, 0, 0)))
            {
                float y = GetYPositionFromTick(CurrentTick);
                pe.Graphics.DrawLine(posPen, -UnitLaneWidth * 2, y, laneWidth, y);
            }

            // ノート描画
            var holds = Notes.Holds.Where(p => p.StartTick <= tailTick && p.StartTick + p.GetDuration() >= HeadTick).ToList();
            var dholds = Notes.DHolds.Where(p => p.StartTick <= tailTick && p.StartTick + p.GetDuration() >= HeadTick).ToList();
            var hholds = Notes.HHolds.Where(p => p.StartTick <= tailTick && p.StartTick + p.GetDuration() >= HeadTick).ToList();
            var lholds = Notes.LHolds.Where(p => p.StartTick <= tailTick && p.StartTick + p.GetDuration() >= HeadTick).ToList();
            // ロングノーツ背景
            // HOLD
            foreach (var hold in holds)
            {
                dc.DrawHoldBackground(new RectangleF(
                    (UnitLaneWidth + BorderThickness) * hold.LaneIndex + BorderThickness,
                    GetYPositionFromTick(hold.StartTick),
                    (UnitLaneWidth + BorderThickness) * hold.Width - BorderThickness,
                    GetYPositionFromTick(hold.Duration)
                    ));
            }
            // DHOLD
            foreach (var hold in dholds)
            {
                dc.DrawDHoldBackground(new RectangleF(
                    (UnitLaneWidth + BorderThickness) * hold.LaneIndex + BorderThickness,
                    GetYPositionFromTick(hold.StartTick),
                    (UnitLaneWidth + BorderThickness) * hold.Width - BorderThickness,
                    GetYPositionFromTick(hold.Duration)
                    ));
            }
            // HHOLD
            foreach (var hold in hholds)
            {
                dc.DrawHHoldBackground(new RectangleF(
                    (UnitLaneWidth + BorderThickness) * hold.LaneIndex + BorderThickness,
                    GetYPositionFromTick(hold.StartTick),
                    (UnitLaneWidth + BorderThickness) * hold.Width - BorderThickness,
                    GetYPositionFromTick(hold.Duration)
                    ));
            }
            // LHOLD
            foreach (var hold in lholds)
            {
                dc.DrawLHoldBackground(new RectangleF(
                    (UnitLaneWidth + BorderThickness) * hold.LaneIndex + BorderThickness,
                    GetYPositionFromTick(hold.StartTick),
                    (UnitLaneWidth + BorderThickness) * hold.Width - BorderThickness,
                    GetYPositionFromTick(hold.Duration)
                    ));
            }

            // TAP, DTAP, FLICK, DAMAGE
            foreach (var note in Notes.Taps.Where(p => p.Tick >= HeadTick && p.Tick <= tailTick)){dc.DrawTap(GetRectFromNotePosition(note.Tick, note.LaneIndex, note.Width));}
            foreach (var note in Notes.DTaps.Where(p => p.Tick >= HeadTick && p.Tick <= tailTick)){dc.DrawDTap(GetRectFromNotePosition(note.Tick, note.LaneIndex, note.Width));}
            foreach (var note in Notes.HTaps.Where(p => p.Tick >= HeadTick && p.Tick <= tailTick)) { dc.DrawHTap(GetRectFromNotePosition(note.Tick, note.LaneIndex, note.Width)); }
            foreach (var note in Notes.LTaps.Where(p => p.Tick >= HeadTick && p.Tick <= tailTick)) { dc.DrawLTap(GetRectFromNotePosition(note.Tick, note.LaneIndex, note.Width)); }
            foreach (var note in Notes.Traces.Where(p => p.Tick >= HeadTick && p.Tick <= tailTick)) { dc.DrawTrace(GetRectFromNotePosition(note.Tick, note.LaneIndex, note.Width)); }
            foreach (var note in Notes.DTraces.Where(p => p.Tick >= HeadTick && p.Tick <= tailTick)) { dc.DrawDTrace(GetRectFromNotePosition(note.Tick, note.LaneIndex, note.Width)); }
            foreach (var note in Notes.HTraces.Where(p => p.Tick >= HeadTick && p.Tick <= tailTick)) { dc.DrawHTrace(GetRectFromNotePosition(note.Tick, note.LaneIndex, note.Width)); }
            foreach (var note in Notes.LTraces.Where(p => p.Tick >= HeadTick && p.Tick <= tailTick)) { dc.DrawLTrace(GetRectFromNotePosition(note.Tick, note.LaneIndex, note.Width)); }

            // 選択範囲描画
            if (Editable) DrawSelectionRange(pe.Graphics);

            // Y軸反転させずにTick = 0をY軸原点とする座標系へ
            pe.Graphics.Transform = GetDrawingMatrix(prevMatrix, false);

            using (var font = new Font("MS Gothic", 8))
            {
                SizeF strSize = pe.Graphics.MeasureString("000", font);

                // 小節番号描画
                int barTick = UnitBeatTick * 4;
                int barCount = 0;
                int pos = 0;

                for (int j = 0; j < sigs.Count; j++)
                {
                    if (pos > tailTick) break;
                    int currentBarLength = (UnitBeatTick * 4) * sigs[j].Numerator / sigs[j].Denominator;
                    for (int i = 0; pos + i * currentBarLength < tailTick; i++)
                    {
                        if (j < sigs.Count - 1 && i * currentBarLength >= (sigs[j + 1].Tick - pos) / currentBarLength * currentBarLength) break;

                        int tick = pos + i * currentBarLength;
                        barCount++;
                        if (tick < HeadTick) continue;
                        var point = new PointF(-strSize.Width, -GetYPositionFromTick(tick) - strSize.Height);
                        pe.Graphics.DrawString(string.Format("{0:000}", barCount), font, Brushes.White, point);
                    }

                    if (j < sigs.Count - 1)
                        pos += (sigs[j + 1].Tick - pos) / currentBarLength * currentBarLength;
                }

                float rightBase = (UnitLaneWidth + BorderThickness) * Constants.LanesCount + strSize.Width / 3;

                // BPM描画
                using (var bpmBrush = new SolidBrush(Color.FromArgb(0, 192, 0)))
                {
                    foreach (var item in ScoreEvents.BPMChangeEvents.Where(p => p.Tick >= HeadTick && p.Tick < tailTick))
                    {
                        var point = new PointF(rightBase, -GetYPositionFromTick(item.Tick) - strSize.Height);
                        pe.Graphics.DrawString(Regex.Replace(item.BPM.ToString(), @"\.0$", "").PadLeft(3), font, Brushes.Lime, point);
                    }
                }

                // 拍子記号描画
                using (var sigBrush = new SolidBrush(Color.FromArgb(216, 116, 0)))
                {
                    foreach (var item in sigs.Where(p => p.Tick >= HeadTick && p.Tick < tailTick))
                    {
                        var point = new PointF(rightBase + strSize.Width, -GetYPositionFromTick(item.Tick) - strSize.Height);
                        pe.Graphics.DrawString(string.Format("{0}/{1}", item.Numerator, item.Denominator), font, sigBrush, point);
                    }
                }

                // ハイスピ描画
                using (var highSpeedBrush = new SolidBrush(Color.FromArgb(216, 0, 64)))
                {
                    foreach (var item in ScoreEvents.HighSpeedChangeEvents.Where(p => p.Tick >= HeadTick && p.Tick < tailTick))
                    {
                        var point = new PointF(rightBase + strSize.Width * 2, -GetYPositionFromTick(item.Tick) - strSize.Height);
                        pe.Graphics.DrawString(string.Format("x{0: 0.00;-0.00}", item.SpeedRatio), font, highSpeedBrush, point);
                    }
                }

                // レーン分裂描画
                using (var splitLaneBrush = new Pen(Color.FromArgb(216, 0, 216),1))
                {
                    bool[] vs = new bool[6];
                    float[] starts = new float[6];
                    ScoreEvents.SplitLaneEvents.Sort((a,b) => a.Tick - b.Tick);
                    foreach (var item in ScoreEvents.SplitLaneEvents)
                    {
                        pe.Graphics.DrawLine(splitLaneBrush, 0, -GetYPositionFromTick(item.Tick), (UnitLaneWidth + BorderThickness)*6, -GetYPositionFromTick(item.Tick));
                        for (int i = 0; i < 6; i++)
                        {
                            if(vs[i] != item.Lane[i])
                            {
                                if (item.Lane[i])
                                    starts[i] = -GetYPositionFromTick(item.Tick);
                                else
                                    pe.Graphics.DrawLine(splitLaneBrush, (UnitLaneWidth + BorderThickness) * (i + 0.5f), starts[i], (UnitLaneWidth + BorderThickness) * (i + 0.5f), -GetYPositionFromTick(item.Tick));
                            }
                            vs[i] = item.Lane[i];
                        }
                    }
                }

            }

            pe.Graphics.Transform = prevMatrix;
        }

        private Matrix GetDrawingMatrix(Matrix baseMatrix)
        {
            return GetDrawingMatrix(baseMatrix, true);
        }

        private Matrix GetDrawingMatrix(Matrix baseMatrix, bool flipY)
        {
            Matrix matrix = baseMatrix.Clone();
            if (flipY)
            {
                // 反転してY軸増加方向を時間軸に
                matrix.Scale(1, -1);
            }
            // ずれたコントロール高さ分を補正
            matrix.Translate(0, ClientSize.Height - 1, MatrixOrder.Append);
            // さらにずらして下端とHeadTickを合わせる
            matrix.Translate(0, HeadTick * UnitBeatHeight / UnitBeatTick, MatrixOrder.Append);
            // 水平方向に対して中央に寄せる
            matrix.Translate((ClientSize.Width - LaneWidth) / 2, 0);

            return matrix;
        }

        private float GetYPositionFromTick(int tick)
        {
            return tick * UnitBeatHeight / UnitBeatTick;
        }

        protected int GetTickFromYPosition(float y)
        {
            return (int)(y * UnitBeatTick / UnitBeatHeight);
        }

        protected int GetQuantizedTick(int tick)
        {
            var sigs = ScoreEvents.TimeSignatureChangeEvents.OrderBy(p => p.Tick).ToList();

            int head = 0;
            for (int i = 0; i < sigs.Count; i++)
            {
                int barTick = UnitBeatTick * 4 * sigs[i].Numerator / sigs[i].Denominator;

                if (i < sigs.Count - 1)
                {
                    int nextHead = head + (sigs[i + 1].Tick - head) / barTick * barTick;
                    if (tick >= nextHead)
                    {
                        head = nextHead;
                        continue;
                    }
                }

                int headBarTick = head + (tick - head) / barTick * barTick;
                int offsetCount = (int)Math.Round((float)(tick - headBarTick) / QuantizeTick);
                int maxOffsetCount = (int)(barTick / QuantizeTick);
                int remnantTick = barTick - (int)(maxOffsetCount * QuantizeTick);
                return headBarTick + ((tick - headBarTick >= barTick - remnantTick / 2) ? barTick : (int)(offsetCount * QuantizeTick));
            }

            throw new InvalidOperationException();
        }

        private RectangleF GetRectFromNotePosition(int tick, int laneIndex, int width)
        {
            return new RectangleF(
                (UnitLaneWidth + BorderThickness) * laneIndex + BorderThickness,
                GetYPositionFromTick(tick) - ShortNoteHeight / 2,
                (UnitLaneWidth + BorderThickness) * width - BorderThickness,
                ShortNoteHeight
                );
        }

        private RectangleF GetClickableRectFromNotePosition(int tick, int laneIndex, int width)
        {
            return GetRectFromNotePosition(tick, laneIndex, width).Expand(1);
        }

        private Rectangle GetSelectionRect()
        {
            int minTick = SelectedRange.Duration < 0 ? SelectedRange.StartTick + SelectedRange.Duration : SelectedRange.StartTick;
            int maxTick = SelectedRange.Duration < 0 ? SelectedRange.StartTick : SelectedRange.StartTick + SelectedRange.Duration;
            var start = new Point(SelectedRange.StartLaneIndex * (UnitLaneWidth + BorderThickness), (int)GetYPositionFromTick(minTick) - ShortNoteHeight);
            var end = new Point((SelectedRange.StartLaneIndex + SelectedRange.SelectedLanesCount) * (UnitLaneWidth + BorderThickness), (int)GetYPositionFromTick(maxTick) + ShortNoteHeight);
            return new Rectangle(start.X, start.Y, end.X - start.X, end.Y - start.Y);
        }

        protected void DrawSelectionRange(Graphics g)
        {
            Rectangle selectedRect = GetSelectionRect();
            g.DrawXorRectangle(PenStyles.Dot, g.Transform.TransformPoint(selectedRect.Location), g.Transform.TransformPoint(selectedRect.Location + selectedRect.Size));
        }

        public Core.NoteCollection GetSelectedNotes()
        {
            int minTick = SelectedRange.StartTick + (SelectedRange.Duration < 0 ? SelectedRange.Duration : 0);
            int maxTick = SelectedRange.StartTick + (SelectedRange.Duration < 0 ? 0 : SelectedRange.Duration);
            int startLaneIndex = SelectedRange.StartLaneIndex;
            int endLaneIndex = SelectedRange.StartLaneIndex + SelectedRange.SelectedLanesCount;

            var c = new Core.NoteCollection();

            Func<IAirable, bool> contained = p => p.Tick >= minTick && p.Tick <= maxTick & p.LaneIndex >= startLaneIndex && p.LaneIndex + p.Width <= endLaneIndex;
            c.Taps.AddRange(Notes.Taps.Where(p => contained(p)));
            c.DTaps.AddRange(Notes.DTaps.Where(p => contained(p)));
            c.HTaps.AddRange(Notes.HTaps.Where(p => contained(p)));
            c.LTaps.AddRange(Notes.LTaps.Where(p => contained(p)));
            c.Traces.AddRange(Notes.Traces.Where(p => contained(p)));
            c.DTraces.AddRange(Notes.DTraces.Where(p => contained(p)));
            c.HTraces.AddRange(Notes.HTraces.Where(p => contained(p)));
            c.LTraces.AddRange(Notes.LTraces.Where(p => contained(p)));
            c.Holds.AddRange(Notes.Holds.Where(p => p.StartTick >= minTick && p.StartTick + p.Duration <= maxTick && p.LaneIndex >= startLaneIndex && p.LaneIndex + p.Width <= endLaneIndex));
            c.DHolds.AddRange(Notes.DHolds.Where(p => p.StartTick >= minTick && p.StartTick + p.Duration <= maxTick && p.LaneIndex >= startLaneIndex && p.LaneIndex + p.Width <= endLaneIndex));
            c.HHolds.AddRange(Notes.HHolds.Where(p => p.StartTick >= minTick && p.StartTick + p.Duration <= maxTick && p.LaneIndex >= startLaneIndex && p.LaneIndex + p.Width <= endLaneIndex));
            c.LHolds.AddRange(Notes.LHolds.Where(p => p.StartTick >= minTick && p.StartTick + p.Duration <= maxTick && p.LaneIndex >= startLaneIndex && p.LaneIndex + p.Width <= endLaneIndex));
            return c;
        }

        public void CutSelectedNotes()
        {
            CopySelectedNotes();
            RemoveSelectedNotes();
        }

        public void CopySelectedNotes()
        {
            
            var data = new SelectionData(SelectedRange.StartTick + Math.Min(SelectedRange.Duration, 0), UnitBeatTick, GetSelectedNotes());
            Clipboard.SetDataObject(data, true);
        }

        public void PasteNotes()
        {
            var op = PasteNotes(p => { });
            if (op == null) return;
            OperationManager.Push(op);
            Invalidate();
        }

        public void PasteFlippedNotes()
        {
            var op = PasteNotes(p => FlipNotes(p.SelectedNotes));
            if (op == null) return;
            OperationManager.Push(op);
            Invalidate();
        }

        /// <summary>
        /// クリップボードにコピーされたノーツをペーストしてその操作を表す<see cref="IOperation"/>を返します。
        /// ペーストするノーツがない場合はnullを返します。
        /// </summary>
        /// <param name="action">選択データに対して適用するアクション</param>
        /// <returns>ペースト操作を表す<see cref="IOperation"/></returns>
        protected IOperation PasteNotes(Action<SelectionData> action)
        {
            var obj = Clipboard.GetDataObject();
            if (obj == null || !obj.GetDataPresent(typeof(SelectionData))) return null;

            var data = obj.GetData(typeof(SelectionData)) as SelectionData;
            if (data.IsEmpty) return null;

            double tickFactor = UnitBeatTick / (double)data.TicksPerBeat;
            int originTick = (int)(data.StartTick * tickFactor);
            if (data.TicksPerBeat != UnitBeatTick)
                data.SelectedNotes.UpdateTicksPerBeat(tickFactor);

            data.SelectedNotes.Taps = data.SelectedNotes.Taps.Distinct().ToList();
            foreach (var note in data.SelectedNotes.GetShortNotes())
            {
                note.Tick = note.Tick - originTick + CurrentTick;
            }

            foreach (var hold in data.SelectedNotes.GetLongNotes())
            {
                hold.StartTick = hold.StartTick - originTick + CurrentTick;
            }

            action(data);


            var op = data.SelectedNotes.Taps.Select(p => new InsertTapOperation(Notes, p)).Cast<IOperation>()
                .Concat(data.SelectedNotes.DTaps.Select(p => new InsertDTapOperation(Notes, p)))
                .Concat(data.SelectedNotes.HTaps.Select(p => new InsertHTapOperation(Notes, p)))
                .Concat(data.SelectedNotes.LTaps.Select(p => new InsertLTapOperation(Notes, p)))
                .Concat(data.SelectedNotes.Traces.Select(p => new InsertTraceOperation(Notes, p)))
                .Concat(data.SelectedNotes.DTraces.Select(p => new InsertDTraceOperation(Notes, p)))
                .Concat(data.SelectedNotes.HTraces.Select(p => new InsertHTraceOperation(Notes, p)))
                .Concat(data.SelectedNotes.LTraces.Select(p => new InsertLTraceOperation(Notes, p)))
                .Concat(data.SelectedNotes.Holds.Select(p => new InsertHoldOperation(Notes, p)))
                .Concat(data.SelectedNotes.DHolds.Select(p => new InsertDHoldOperation(Notes, p)))
                .Concat(data.SelectedNotes.HHolds.Select(p => new InsertHHoldOperation(Notes, p)))
                .Concat(data.SelectedNotes.LHolds.Select(p => new InsertLHoldOperation(Notes, p)))
                .Concat(data.SelectedNotes.Flicks.Select(p => new InsertFlickOperation(Notes, p)))
                .Concat(data.SelectedNotes.Damages.Select(p => new InsertDamageOperation(Notes, p)));
            var composite = new CompositeOperation("クリップボードからペースト", op.ToList());
            composite.Redo(); // 追加書くの面倒になったので許せ

            return composite;
        }

        public void RemoveSelectedNotes()
        {
            var selected = GetSelectedNotes();

            var taps = selected.Taps.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveTapOperation(Notes, p);
            });
            var dtaps = selected.DTaps.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveDTapOperation(Notes, p);
            });
            var htaps = selected.HTaps.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveHTapOperation(Notes, p);
            });
            var ltaps = selected.LTaps.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveLTapOperation(Notes, p);
            });

            var traces = selected.Traces.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveTraceOperation(Notes, p);
            });
            var dtraces = selected.DTraces.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveDTraceOperation(Notes, p);
            });
            var htraces = selected.HTraces.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveHTraceOperation(Notes, p);
            });
            var ltraces = selected.LTraces.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveLTraceOperation(Notes, p);
            });

            var holds = selected.Holds.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveHoldOperation(Notes, p);
            });
            var dholds = selected.DHolds.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveDHoldOperation(Notes, p);
            });
            var hholds = selected.HHolds.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveHHoldOperation(Notes, p);
            });
            var lholds = selected.LHolds.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveLHoldOperation(Notes, p);
            });

            var flicks = selected.Flicks.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveFlickOperation(Notes, p);
            });
            var damages = selected.Damages.Select(p =>
            {
                Notes.Remove(p);
                return new RemoveDamageOperation(Notes, p);
            });

            var opList = taps.Cast<IOperation>().Concat(dtaps).Concat(htaps).Concat(ltaps).Concat(traces).Concat(dtraces).Concat(htraces).Concat(ltraces)
                .Concat(holds).Concat(dholds).Concat(hholds).Concat(lholds)
                .Concat(flicks).Concat(damages)
                .ToList();

            if (opList.Count() == 0) return;
            OperationManager.Push(new CompositeOperation("選択範囲内ノーツ削除", opList));
            Invalidate();
        }

        public void FlipSelectedNotes()
        {
            var op = FlipNotes(GetSelectedNotes());
            if (op == null) return;
            OperationManager.Push(op);
            Invalidate();
        }

        /// <summary>
        /// 指定のコレクション内のノーツを反転してその操作を表す<see cref="IOperation"/>を返します。
        /// 反転するノーツがない場合はnullを返します。
        /// </summary>
        /// <param name="notes">反転対象となるノーツを含む<see cref="Core.NoteCollection"/></param>
        /// <returns>反転操作を表す<see cref="IOperation"/></returns>
        protected IOperation FlipNotes(Core.NoteCollection notes)
        {
            var dicShortNotes = notes.GetShortNotes().ToDictionary(q => q, q => new MoveShortNoteOperation.NotePosition(q.Tick, q.LaneIndex));
            var dicHolds = notes.GetLongNotes().ToDictionary(q => q, q => new MoveLongNoteOperation.NotePosition(q.StartTick, q.LaneIndex, q.Width));
            var referenced = new NoteCollection(notes);

            var opShortNotes = dicShortNotes.Select(p =>
            {
                p.Key.LaneIndex = Constants.LanesCount - p.Key.LaneIndex - p.Key.Width;
                var after = new MoveShortNoteOperation.NotePosition(p.Key.Tick, p.Key.LaneIndex);
                return new MoveShortNoteOperation(p.Key, p.Value, after);
            });

            var opHolds = dicHolds.Select(p =>
            {
                p.Key.LaneIndex = Constants.LanesCount - p.Key.LaneIndex - p.Key.Width;
                var after = new MoveLongNoteOperation.NotePosition(p.Key.StartTick, p.Key.LaneIndex, p.Key.Width);
                return new MoveLongNoteOperation(p.Key, p.Value, after);
            });

            var opList = opShortNotes.Cast<IOperation>().Concat(opHolds).ToList();
            return opList.Count == 0 ? null : new CompositeOperation("ノーツの反転", opList);
        }

        public void Undo()
        {
            if (!OperationManager.CanUndo) return;
            OperationManager.Undo();
            Invalidate();
        }

        public void Redo()
        {
            if (!OperationManager.CanRedo) return;
            OperationManager.Redo();
            Invalidate();
        }


        public void Initialize()
        {
            SelectedRange = SelectionRange.Empty;
            CurrentTick = SelectedRange.StartTick;
            Invalidate();
        }

        public void Initialize(Score score)
        {
            Initialize();
            UpdateScore(score);
        }

        public void UpdateScore(Score score)
        {
            UnitBeatTick = score.TicksPerBeat;
            if (NoteCollectionCache.ContainsKey(score))
            {
                Notes = NoteCollectionCache[score];
            }
            else
            {
                Notes = new NoteCollection(score.Notes);
                NoteCollectionCache.Add(score, Notes);
            }
            ScoreEvents = score.Events;
            Invalidate();
        }

        public class NoteCollection
        {
            public event EventHandler NoteChanged;

            private Core.NoteCollection source = new Core.NoteCollection();

            public IReadOnlyCollection<Tap> Taps { get { return source.Taps; } }
            public IReadOnlyCollection<DTap> DTaps { get { return source.DTaps; } }
            public IReadOnlyCollection<HTap> HTaps { get { return source.HTaps; } }
            public IReadOnlyCollection<LTap> LTaps { get { return source.LTaps; } }

            public IReadOnlyCollection<Trace> Traces { get { return source.Traces; } }
            public IReadOnlyCollection<DTrace> DTraces { get { return source.DTraces; } }
            public IReadOnlyCollection<HTrace> HTraces { get { return source.HTraces; } }
            public IReadOnlyCollection<LTrace> LTraces { get { return source.LTraces; } }

            public IReadOnlyCollection<Hold> Holds { get { return source.Holds; } }
            public IReadOnlyCollection<DHold> DHolds { get { return source.DHolds; } }
            public IReadOnlyCollection<HHold> HHolds { get { return source.HHolds; } }
            public IReadOnlyCollection<LHold> LHolds { get { return source.LHolds; } }

            public IReadOnlyCollection<Flick> Flicks { get { return source.Flicks; } }
            public IReadOnlyCollection<Damage> Damages { get { return source.Damages; } }

            public NoteCollection(Core.NoteCollection src)
            {
                Load(src);
            }

            public void Add(Tap note)
            {
                source.Taps.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Add(DTap note)
            {
                source.DTaps.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Add(HTap note)
            {
                source.HTaps.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Add(LTap note)
            {
                source.LTaps.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Add(Trace note)
            {
                source.Traces.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Add(DTrace note)
            {
                source.DTraces.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Add(HTrace note)
            {
                source.HTraces.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Add(LTrace note)
            {
                source.LTraces.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Add(Hold note)
            {
                source.Holds.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Add(DHold note)
            {
                source.DHolds.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Add(HHold note)
            {
                source.HHolds.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Add(LHold note)
            {
                source.LHolds.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Add(Flick note)
            {
                source.Flicks.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Add(Damage note)
            {
                source.Damages.Add(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }


            public void Remove(Tap note)
            {
                source.Taps.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Remove(DTap note)
            {
                source.DTaps.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Remove(HTap note)
            {
                source.HTaps.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Remove(LTap note)
            {
                source.LTaps.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Remove(Trace note)
            {
                source.Traces.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Remove(DTrace note)
            {
                source.DTraces.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Remove(HTrace note)
            {
                source.HTraces.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Remove(LTrace note)
            {
                source.LTraces.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Remove(Hold note)
            {
                source.Holds.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Remove(DHold note)
            {
                source.DHolds.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Remove(HHold note)
            {
                source.HHolds.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }
            public void Remove(LHold note)
            {
                source.LHolds.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Remove(Flick note)
            {
                source.Flicks.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Remove(Damage note)
            {
                source.Damages.Remove(note);
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }

            public int GetLastTick()
            {
                var shortNotes = Taps.Cast<TappableBase>().Concat(DTaps).Concat(HTaps).Concat(LTaps).Concat(Traces).Concat(DTraces).Concat(HTraces).Concat(LTraces).ToList();
                var longNotes = Holds.Cast<ILongNote>().Concat(DHolds).Concat(HHolds).Concat(LHolds).ToList();
                int lastShortNoteTick = shortNotes.Count == 0 ? 0 : shortNotes.Max(p => p.Tick);
                int lastLongNoteTick = longNotes.Count == 0 ? 0 : longNotes.Max(p => p.StartTick + p.GetDuration());
                return Math.Max(lastShortNoteTick, lastLongNoteTick);
            }


            public void Load(Core.NoteCollection collection)
            {
                Clear();

                foreach (var note in collection.Taps) Add(note);
                foreach (var note in collection.DTaps) Add(note);
                foreach (var note in collection.HTaps) Add(note);
                foreach (var note in collection.LTaps) Add(note);
                foreach (var note in collection.Traces) Add(note);
                foreach (var note in collection.DTraces) Add(note);
                foreach (var note in collection.HTraces) Add(note);
                foreach (var note in collection.LTraces) Add(note);
                foreach (var note in collection.Holds) Add(note);
                foreach (var note in collection.DHolds) Add(note);
                foreach (var note in collection.HHolds) Add(note);
                foreach (var note in collection.LHolds) Add(note);
                foreach (var note in collection.Flicks) Add(note);
                foreach (var note in collection.Damages) Add(note);
            }

            public void Clear()
            {
                source = new Core.NoteCollection();
                NoteChanged?.Invoke(this, EventArgs.Empty);
            }

            public void UpdateTicksPerBeat(double factor)
            {
                source.UpdateTicksPerBeat(factor);
            }
        }
    }

    public enum EditMode
    {
        Select,
        Edit,
        Erase
    }

    [Flags]
    public enum NoteType
    {
        Tap = 1,
        DTap = 1 << 1,
        HTap = 1 << 2,
        LTap = 1 << 3,
        Trace = 1 << 4,
        DTrace = 1 << 5,
        HTrace = 1 << 6,
        LTrace = 1 << 7,
        Hold = 1 << 8,
        DHold = 1 << 9,
        HHold = 1 << 10,
        LHold = 1 << 11,
        Flick = 1 << 12,
        Damage = 1 << 13
    }

    [Serializable]
    public class SelectionData
    {
        private string serializedText = null;

        [NonSerialized]
        private InnerData Data;

        public int StartTick
        {
            get
            {
                CheckRestored();
                return Data.StartTick;
            }
        }

        public Core.NoteCollection SelectedNotes
        {
            get
            {
                CheckRestored();
                return Data.SelectedNotes;
            }
        }

        public bool IsEmpty
        {
            get
            {
                CheckRestored();
                return SelectedNotes.GetShortNotes().Count() == 0 && SelectedNotes.GetLongNotes().Count() == 0;
            }
        }

        public int TicksPerBeat
        {
            get
            {
                CheckRestored();
                return Data.TicksPerBeat;
            }
        }

        public SelectionData()
        {
        }

        public SelectionData(int startTick, int ticksPerBeat, NoteCollection notes)
        {
            Data = new InnerData(startTick, ticksPerBeat, notes);
            serializedText = Newtonsoft.Json.JsonConvert.SerializeObject(Data, SerializerSettings);
        }

        protected void CheckRestored()
        {
            if (Data == null) Restore();
        }

        protected void Restore()
        {
            Data = Newtonsoft.Json.JsonConvert.DeserializeObject<InnerData>(serializedText, SerializerSettings);
        }

        protected static Newtonsoft.Json.JsonSerializerSettings SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver() { IgnoreSerializableAttribute = true }
        };

        [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
        protected class InnerData
        {
            [Newtonsoft.Json.JsonProperty]
            private int startTick;

            [Newtonsoft.Json.JsonProperty]
            private int ticksPerBeat;

            [Newtonsoft.Json.JsonProperty]
            private NoteCollection selectedNotes;

            public int StartTick => startTick;
            public int TicksPerBeat => ticksPerBeat;
            public NoteCollection SelectedNotes => selectedNotes;

            public InnerData(int startTick, int ticksPerBeat, NoteCollection notes)
            {
                this.startTick = startTick;
                this.ticksPerBeat = ticksPerBeat;
                selectedNotes = notes;
            }
        }
    }

    internal static class UIExtensions
    {
        public static Core.NoteCollection Reposit(this NoteView.NoteCollection collection)
        {
            var res = new NoteCollection();
            res.Taps = collection.Taps.ToList();
            res.DTaps = collection.DTaps.ToList();
            res.HTaps = collection.HTaps.ToList();
            res.LTaps = collection.LTaps.ToList();
            res.Traces = collection.Traces.ToList();
            res.DTraces = collection.DTraces.ToList();
            res.HTraces = collection.HTraces.ToList();
            res.LTraces = collection.LTraces.ToList();
            res.Holds = collection.Holds.ToList();
            res.DHolds = collection.DHolds.ToList();
            res.HHolds = collection.HHolds.ToList();
            res.LHolds = collection.LHolds.ToList();
            res.Flicks = collection.Flicks.ToList();
            res.Damages = collection.Damages.ToList();
            return res;
        }
    }
}
