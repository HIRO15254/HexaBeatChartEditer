﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using ConcurrentPriorityQueue;
using HexaBeatChartEditer.Core;
using HexaBeatChartEditer.Core.Notes;
using HexaBeatChartEditer.Core.Events;

namespace HexaBeatChartEditer.Components.Exporter
{
    public class SusExporter : IExtendedExpoerter<SusArgs>
    {
        public string FormatName
        {
            get { return "Hexabeat Chart File(hcf形式)"; }
        }

        public SusArgs CustomArgs { get; set; }

        public void Export(string path, ScoreBook book)
        {
            SusArgs args = CustomArgs;
            var notes = book.Score.Notes;

            notes.Taps = notes.Taps.Distinct().ToList();
            notes.DTaps = notes.DTaps.Distinct().ToList();
            notes.HTaps = notes.HTaps.Distinct().ToList();
            notes.LTaps = notes.LTaps.Distinct().ToList();
            notes.Traces = notes.Traces.Distinct().ToList();
            notes.DTraces = notes.DTraces.Distinct().ToList();
            notes.HTraces = notes.HTraces.Distinct().ToList();
            notes.LTraces = notes.LTraces.Distinct().ToList();
            notes.Holds = notes.Holds.Distinct().ToList();
            notes.DHolds = notes.DHolds.Distinct().ToList();
            notes.HHolds = notes.HHolds.Distinct().ToList();
            notes.LHolds = notes.LHolds.Distinct().ToList();

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("#TITLE \"{0}\"", book.Title);
                writer.WriteLine("#ARTIST \"{0}\"", book.ArtistName);
                writer.WriteLine("#DESIGNER \"{0}\"", book.NotesDesignerName);
                writer.WriteLine("#DIFFICULTY {0}", (int)args.PlayDifficulty + (string.IsNullOrEmpty(args.ExtendedDifficulty) ? "" : ":" + args.ExtendedDifficulty));
                writer.WriteLine("#PLAYLEVEL {0}", args.PlayLevel);
                writer.WriteLine("#WAVE \"{0}\"", args.SoundFileName);
                writer.WriteLine("#WAVEOFFSET {0}", args.SoundOffset);

                writer.WriteLine();

                int barTick = book.Score.TicksPerBeat * 4;
                var barIndexCalculator = new BarIndexCalculator(barTick, book.Score.Events.TimeSignatureChangeEvents, args.HasPaddingBar);

                foreach (var item in barIndexCalculator.TimeSignatures)
                {
                    writer.WriteLine("#{0:000}02: {1}", item.StartBarIndex + (args.HasPaddingBar && item.StartBarIndex == 1 ? -1 : 0), 4f * item.TimeSignature.Numerator / item.TimeSignature.Denominator);
                }

                writer.WriteLine();

                var bpmlist = book.Score.Events.BPMChangeEvents
                    .GroupBy(p => p.BPM)
                    .SelectMany((p, i) => p.Select(q => new { Index = i, Value = q, BarPosition = barIndexCalculator.GetBarPositionFromTick(q.Tick) }))
                    .ToList();

                if (bpmlist.Count >= 36 * 36) throw new ArgumentException("BPM定義数が上限を超えました。");

                var bpmIdentifiers = EnumerateIdentifiers(2).Skip(1).Take(bpmlist.Count).ToList();
                foreach (var item in bpmlist.GroupBy(p => p.Index).Select(p => p.First()))
                {
                    writer.WriteLine("#BPM{0}: {1}", bpmIdentifiers[item.Index], item.Value.BPM);
                }

                if (args.HasPaddingBar)
                    writer.WriteLine("#{0:000}08: {1:x2}", 0, bpmIdentifiers[bpmlist.OrderBy(p => p.Value.Tick).First().Index]);

                foreach (var eventInBar in bpmlist.GroupBy(p => p.BarPosition.BarIndex))
                {
                    var sig = barIndexCalculator.GetTimeSignatureFromBarIndex(eventInBar.Key);
                    int barLength = barTick * sig.Numerator / sig.Denominator;
                    var dic = eventInBar.ToDictionary(p => p.BarPosition.TickOffset, p => p);
                    int gcd = eventInBar.Select(p => p.BarPosition.TickOffset).Aggregate(barLength, (p, q) => GetGcd(p, q));
                    writer.Write("#{0:000}08: ", eventInBar.Key);
                    for (int i = 0; i * gcd < barLength; i++)
                    {
                        int tickOffset = i * gcd;
                        writer.Write(dic.ContainsKey(tickOffset) ? bpmIdentifiers[dic[tickOffset].Index] : "00");
                    }
                    writer.WriteLine();
                }

                writer.WriteLine();

                foreach (var EventVar in book.Score.Events.HighSpeedChangeEvents)
                {
                    var barPos = barIndexCalculator.GetBarPositionFromTick(EventVar.Tick);
                    writer.Write("#{0:000}07: ", barPos.BarIndex);
                    writer.WriteLine(EventVar.SpeedRatio);
                }

                foreach (var EventVar in book.Score.Events.SplitLaneEvents)
                {
                    var barPos = barIndexCalculator.GetBarPositionFromTick(EventVar.Tick);
                    writer.Write("#{0:000}05: ", barPos.BarIndex);
                    writer.WriteLine(EventVar.ToString().ToCharArray());
                }

                writer.WriteLine();

                var shortNotes = notes.Taps.Cast<TappableBase>().Select(p => new { Type = '1', Note = p })
                    .Concat(notes.DTaps.Cast<TappableBase>().Select(p => new { Type = '2', Note = p }))
                    .Concat(notes.HTaps.Cast<TappableBase>().Select(p => new { Type = '3', Note = p }))
                    .Concat(notes.LTaps.Cast<TappableBase>().Select(p => new { Type = '4', Note = p }))
                    .Concat(notes.Traces.Cast<TappableBase>().Select(p => new { Type = '5', Note = p }))
                    .Concat(notes.DTraces.Cast<TappableBase>().Select(p => new { Type = '6', Note = p }))
                    .Concat(notes.HTraces.Cast<TappableBase>().Select(p => new { Type = '7', Note = p }))
                    .Concat(notes.LTraces.Cast<TappableBase>().Select(p => new { Type = '8', Note = p }))
                    .Concat(notes.Flicks.Cast<TappableBase>().Select(p => new { Type = '9', Note = p }))
                    .Concat(notes.Damages.Cast<TappableBase>().Select(p => new { Type = 'A', Note = p }))
                    .Select(p => new
                    {
                        BarPosition = barIndexCalculator.GetBarPositionFromTick(p.Note.Tick),
                        LaneIndex = p.Note.LaneIndex,
                        Width = p.Note.Width,
                        Type = p.Type
                    });

                foreach (var notesInBar in shortNotes.GroupBy(p => p.BarPosition.BarIndex))
                {
                    foreach (var notesInLane in notesInBar.GroupBy(p => p.LaneIndex))
                    {
                        var sig = barIndexCalculator.GetTimeSignatureFromBarIndex(notesInBar.Key);
                        int barLength = barTick * sig.Numerator / sig.Denominator;

                        var offsetList = notesInLane.GroupBy(p => p.BarPosition.TickOffset).Select(p => p.ToList());
                        var separatedNotes = Enumerable.Range(0, offsetList.Max(p => p.Count)).Select(p => offsetList.Where(q => q.Count >= p + 1).Select(q => q[p]));

                        foreach (var dic in separatedNotes.Select(p => p.ToDictionary(q => q.BarPosition.TickOffset, q => q)))
                        {
                            int gcd = dic.Values.Select(p => p.BarPosition.TickOffset).Aggregate(barLength, (p, q) => GetGcd(p, q));
                            writer.Write("#{0:000}1{1}:", notesInBar.Key, notesInLane.Key.ToString("x"));
                            for (int i = 0; i * gcd < barLength; i++)
                            {
                                int tickOffset = i * gcd;
                                writer.Write(dic.ContainsKey(tickOffset) ? dic[tickOffset].Type + ToLaneWidthString(dic[tickOffset].Width) : "00");
                            }
                            writer.WriteLine();
                        }
                    }
                }

                var identifier = new IdentifierAllocationManager();

                var holds = book.Score.Notes.Holds
                    .OrderBy(p => p.StartTick)
                    .Select(p => new
                    {
                        Identifier = identifier.Allocate(p.StartTick, p.Duration),
                        StartTick = p.StartTick,
                        EndTick = p.StartTick + p.Duration,
                        Width = p.Width,
                        LaneIndex = p.LaneIndex
                    });

                foreach (var hold in holds)
                {
                    var startBarPosition = barIndexCalculator.GetBarPositionFromTick(hold.StartTick);
                    var endBarPosition = barIndexCalculator.GetBarPositionFromTick(hold.EndTick);
                    if (startBarPosition.BarIndex == endBarPosition.BarIndex)
                    {
                        var sig = barIndexCalculator.GetTimeSignatureFromBarIndex(startBarPosition.BarIndex);
                        int barLength = barTick * sig.Numerator / sig.Denominator;
                        writer.Write("#{0:000}2{1}{2}:", startBarPosition.BarIndex, hold.LaneIndex.ToString("x"), hold.Identifier);
                        int gcd = GetGcd(GetGcd(startBarPosition.TickOffset, endBarPosition.TickOffset), barLength);
                        for (int i = 0; i * gcd < barLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (startBarPosition.TickOffset == tickOffset) writer.Write("1" + ToLaneWidthString(hold.Width));
                            else if (endBarPosition.TickOffset == tickOffset) writer.Write("2" + ToLaneWidthString(hold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();
                    }
                    else
                    {
                        var startSig = barIndexCalculator.GetTimeSignatureFromBarIndex(startBarPosition.BarIndex);
                        int startBarLength = barTick * startSig.Numerator / startSig.Denominator;
                        writer.Write("#{0:000}2{1}{2}:", startBarPosition.BarIndex, hold.LaneIndex.ToString("x"), hold.Identifier);
                        int gcd = GetGcd(startBarPosition.TickOffset, startBarLength);
                        for (int i = 0; i * gcd < startBarLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (startBarPosition.TickOffset == tickOffset) writer.Write("1" + ToLaneWidthString(hold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();

                        var endSig = barIndexCalculator.GetTimeSignatureFromBarIndex(endBarPosition.BarIndex);
                        int endBarLength = barTick * endSig.Numerator / endSig.Denominator;
                        writer.Write("#{0:000}2{1}{2}:", endBarPosition.BarIndex, hold.LaneIndex.ToString("x"), hold.Identifier);
                        gcd = GetGcd(endBarPosition.TickOffset, endBarLength);
                        for (int i = 0; i * gcd < endBarLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (endBarPosition.TickOffset == tickOffset) writer.Write("2" + ToLaneWidthString(hold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();
                    }
                }

                var identifier2 = new IdentifierAllocationManager();

                var dholds = book.Score.Notes.DHolds
                    .OrderBy(p => p.StartTick)
                    .Select(p => new
                    {
                        Identifier2 = identifier2.Allocate(p.StartTick, p.Duration),
                        StartTick = p.StartTick,
                        EndTick = p.StartTick + p.Duration,
                        Width = p.Width,
                        LaneIndex = p.LaneIndex
                    });
                foreach (var dhold in dholds)
                {
                    var startBarPosition = barIndexCalculator.GetBarPositionFromTick(dhold.StartTick);
                    var endBarPosition = barIndexCalculator.GetBarPositionFromTick(dhold.EndTick);
                    if (startBarPosition.BarIndex == endBarPosition.BarIndex)
                    {
                        var sig = barIndexCalculator.GetTimeSignatureFromBarIndex(startBarPosition.BarIndex);
                        int barLength = barTick * sig.Numerator / sig.Denominator;
                        writer.Write("#{0:000}3{1}{2}:", startBarPosition.BarIndex, dhold.LaneIndex.ToString("x"), dhold.Identifier2);
                        int gcd = GetGcd(GetGcd(startBarPosition.TickOffset, endBarPosition.TickOffset), barLength);
                        for (int i = 0; i * gcd < barLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (startBarPosition.TickOffset == tickOffset) writer.Write("1" + ToLaneWidthString(dhold.Width));
                            else if (endBarPosition.TickOffset == tickOffset) writer.Write("2" + ToLaneWidthString(dhold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();
                    }
                    else
                    {
                        var startSig = barIndexCalculator.GetTimeSignatureFromBarIndex(startBarPosition.BarIndex);
                        int startBarLength = barTick * startSig.Numerator / startSig.Denominator;
                        writer.Write("#{0:000}3{1}{2}:", startBarPosition.BarIndex, dhold.LaneIndex.ToString("x"), dhold.Identifier2);
                        int gcd = GetGcd(startBarPosition.TickOffset, startBarLength);
                        for (int i = 0; i * gcd < startBarLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (startBarPosition.TickOffset == tickOffset) writer.Write("1" + ToLaneWidthString(dhold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();

                        var endSig = barIndexCalculator.GetTimeSignatureFromBarIndex(endBarPosition.BarIndex);
                        int endBarLength = barTick * endSig.Numerator / endSig.Denominator;
                        writer.Write("#{0:000}3{1}{2}:", endBarPosition.BarIndex, dhold.LaneIndex.ToString("x"), dhold.Identifier2);
                        gcd = GetGcd(endBarPosition.TickOffset, endBarLength);
                        for (int i = 0; i * gcd < endBarLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (endBarPosition.TickOffset == tickOffset) writer.Write("2" + ToLaneWidthString(dhold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();
                    }

                }

                var identifier3 = new IdentifierAllocationManager();

                var hholds = book.Score.Notes.DHolds
                    .OrderBy(p => p.StartTick)
                    .Select(p => new
                    {
                        Identifier3 = identifier3.Allocate(p.StartTick, p.Duration),
                        StartTick = p.StartTick,
                        EndTick = p.StartTick + p.Duration,
                        Width = p.Width,
                        LaneIndex = p.LaneIndex
                    });
                foreach (var hhold in hholds)
                {
                    var startBarPosition = barIndexCalculator.GetBarPositionFromTick(hhold.StartTick);
                    var endBarPosition = barIndexCalculator.GetBarPositionFromTick(hhold.EndTick);
                    if (startBarPosition.BarIndex == endBarPosition.BarIndex)
                    {
                        var sig = barIndexCalculator.GetTimeSignatureFromBarIndex(startBarPosition.BarIndex);
                        int barLength = barTick * sig.Numerator / sig.Denominator;
                        writer.Write("#{0:000}4{1}{2}:", startBarPosition.BarIndex, hhold.LaneIndex.ToString("x"), hhold.Identifier3);
                        int gcd = GetGcd(GetGcd(startBarPosition.TickOffset, endBarPosition.TickOffset), barLength);
                        for (int i = 0; i * gcd < barLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (startBarPosition.TickOffset == tickOffset) writer.Write("1" + ToLaneWidthString(hhold.Width));
                            else if (endBarPosition.TickOffset == tickOffset) writer.Write("2" + ToLaneWidthString(hhold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();
                    }
                    else
                    {
                        var startSig = barIndexCalculator.GetTimeSignatureFromBarIndex(startBarPosition.BarIndex);
                        int startBarLength = barTick * startSig.Numerator / startSig.Denominator;
                        writer.Write("#{0:000}4{1}{2}:", startBarPosition.BarIndex, hhold.LaneIndex.ToString("x"), hhold.Identifier3);
                        int gcd = GetGcd(startBarPosition.TickOffset, startBarLength);
                        for (int i = 0; i * gcd < startBarLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (startBarPosition.TickOffset == tickOffset) writer.Write("1" + ToLaneWidthString(hhold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();

                        var endSig = barIndexCalculator.GetTimeSignatureFromBarIndex(endBarPosition.BarIndex);
                        int endBarLength = barTick * endSig.Numerator / endSig.Denominator;
                        writer.Write("#{0:000}4{1}{2}:", endBarPosition.BarIndex, hhold.LaneIndex.ToString("x"), hhold.Identifier3);
                        gcd = GetGcd(endBarPosition.TickOffset, endBarLength);
                        for (int i = 0; i * gcd < endBarLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (endBarPosition.TickOffset == tickOffset) writer.Write("2" + ToLaneWidthString(hhold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();
                    }

                    identifier.Clear();
                }

                var identifier4 = new IdentifierAllocationManager();

                var lholds = book.Score.Notes.DHolds
                    .OrderBy(p => p.StartTick)
                    .Select(p => new
                    {
                        Identifier4 = identifier4.Allocate(p.StartTick, p.Duration),
                        StartTick = p.StartTick,
                        EndTick = p.StartTick + p.Duration,
                        Width = p.Width,
                        LaneIndex = p.LaneIndex
                    });
                foreach (var lhold in lholds)
                {
                    var startBarPosition = barIndexCalculator.GetBarPositionFromTick(lhold.StartTick);
                    var endBarPosition = barIndexCalculator.GetBarPositionFromTick(lhold.EndTick);
                    if (startBarPosition.BarIndex == endBarPosition.BarIndex)
                    {
                        var sig = barIndexCalculator.GetTimeSignatureFromBarIndex(startBarPosition.BarIndex);
                        int barLength = barTick * sig.Numerator / sig.Denominator;
                        writer.Write("#{0:000}5{1}{2}:", startBarPosition.BarIndex, lhold.LaneIndex.ToString("x"), lhold.Identifier4);
                        int gcd = GetGcd(GetGcd(startBarPosition.TickOffset, endBarPosition.TickOffset), barLength);
                        for (int i = 0; i * gcd < barLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (startBarPosition.TickOffset == tickOffset) writer.Write("1" + ToLaneWidthString(lhold.Width));
                            else if (endBarPosition.TickOffset == tickOffset) writer.Write("2" + ToLaneWidthString(lhold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();
                    }
                    else
                    {
                        var startSig = barIndexCalculator.GetTimeSignatureFromBarIndex(startBarPosition.BarIndex);
                        int startBarLength = barTick * startSig.Numerator / startSig.Denominator;
                        writer.Write("#{0:000}5{1}{2}:", startBarPosition.BarIndex, lhold.LaneIndex.ToString("x"), lhold.Identifier4);
                        int gcd = GetGcd(startBarPosition.TickOffset, startBarLength);
                        for (int i = 0; i * gcd < startBarLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (startBarPosition.TickOffset == tickOffset) writer.Write("1" + ToLaneWidthString(lhold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();

                        var endSig = barIndexCalculator.GetTimeSignatureFromBarIndex(endBarPosition.BarIndex);
                        int endBarLength = barTick * endSig.Numerator / endSig.Denominator;
                        writer.Write("#{0:000}5{1}{2}:", endBarPosition.BarIndex, lhold.LaneIndex.ToString("x"), lhold.Identifier4);
                        gcd = GetGcd(endBarPosition.TickOffset, endBarLength);
                        for (int i = 0; i * gcd < endBarLength; i++)
                        {
                            int tickOffset = i * gcd;
                            if (endBarPosition.TickOffset == tickOffset) writer.Write("2" + ToLaneWidthString(lhold.Width));
                            else writer.Write("00");
                        }
                        writer.WriteLine();
                    }
                }
                identifier.Clear(); 
                identifier2.Clear(); 
                identifier3.Clear(); 
                identifier4.Clear();
            }
        }

        public static int GetGcd(int a, int b)
        {
            if (a < b) return GetGcd(b, a);
            if (b == 0) return a;
            return GetGcd(b, a % b);
        }

        public static string ToLaneWidthString(int width)
        {
            return width == 16 ? "g" : width.ToString("x");
        }

        public static IEnumerable<string> EnumerateIdentifiers(int digits)
        {
            var num = Enumerable.Range(0, 10).Select(p => (char)('0' + p));
            var alpha = Enumerable.Range(0, 26).Select(p => (char)('A' + p));
            var seq = num.Concat(alpha).Select(p => p.ToString()).ToList();

            return EnumerateIdentifiers(digits, seq);
        }

        private static IEnumerable<string> EnumerateIdentifiers(int digits, List<string> seq)
        {
            if (digits < 1) throw new ArgumentOutOfRangeException("digits");
            if (digits == 1) return seq;
            return EnumerateIdentifiers(digits - 1, seq).SelectMany(p => seq.Select(q => p + q));
        }

        public class IdentifierAllocationManager
        {
            private int lastStartTick;
            private Stack<char> IdentifierStack;
            private ConcurrentPriorityQueue<Tuple<int, char>, int> UsedIdentifiers;

            public IdentifierAllocationManager()
            {
                Clear();
            }

            public void Clear()
            {
                lastStartTick = 0;
                IdentifierStack = new Stack<char>(EnumerateIdentifiers(1).Select(p => p.Single()).Reverse());
                UsedIdentifiers = new ConcurrentPriorityQueue<Tuple<int, char>, int>();
            }

            public char Allocate(int startTick, int duration)
            {
                if (startTick < lastStartTick) throw new InvalidOperationException("startTick must not be less than last called value.");
                while (UsedIdentifiers.Count > 0 && UsedIdentifiers.Peek().Item1 < startTick)
                {
                    IdentifierStack.Push(UsedIdentifiers.Dequeue().Item2);
                }
                char c = IdentifierStack.Pop();
                int endTick = startTick + duration;
                UsedIdentifiers.Enqueue(Tuple.Create(endTick, c), -endTick);
                lastStartTick = startTick;
                return c;
            }
        }

        public class BarIndexCalculator
        {
            private bool hasPaddingBar;
            private int barTick;
            private SortedDictionary<int, TimeSignatureItem> timeSignatures;

            /// <summary>
            /// 時間順にソートされた有効な拍子変更イベントのコレクションを取得します。
            /// </summary>
            public IEnumerable<TimeSignatureItem> TimeSignatures
            {
                get { return timeSignatures.Select(p => p.Value).Reverse(); }
            }

            public BarIndexCalculator(int barTick, IEnumerable<TimeSignatureChangeEvent> events, bool hasPaddingBar)
            {
                this.hasPaddingBar = hasPaddingBar;
                this.barTick = barTick;
                var ordered = events.OrderBy(p => p.Tick).ToList();
                var dic = new SortedDictionary<int, TimeSignatureItem>();
                int pos = 0;
                int barIndex = hasPaddingBar ? 1 : 0;
                for (int i = 0; i < ordered.Count; i++)
                {
                    var item = new TimeSignatureItem()
                    {
                        StartTick = pos,
                        StartBarIndex = barIndex,
                        TimeSignature = ordered[i]
                    };

                    // 時間逆順で追加
                    if (dic.ContainsKey(-pos)) dic[-pos] = item;
                    else dic.Add(-pos, item);

                    if (i < ordered.Count - 1)
                    {
                        int barLength = barTick * ordered[i].Numerator / ordered[i].Denominator;
                        int duration = ordered[i + 1].Tick - pos;
                        pos += duration / barLength * barLength;
                        barIndex += duration / barLength;
                    }
                }

                timeSignatures = dic;
            }

            public BarPosition GetBarPositionFromTick(int tick)
            {
                foreach (var item in timeSignatures)
                {
                    if (tick < item.Value.StartTick) continue;
                    var sig = item.Value.TimeSignature;
                    int barLength = barTick * sig.Numerator / sig.Denominator;
                    int tickOffset = tick - item.Value.StartTick;
                    int barOffset = tickOffset / barLength;
                    return new BarPosition()
                    {
                        BarIndex = item.Value.StartBarIndex + barOffset,
                        TickOffset = tickOffset - barOffset * barLength,
                        TimeSignature = item.Value.TimeSignature
                    };
                }

                throw new InvalidOperationException();
            }

            public TimeSignatureChangeEvent GetTimeSignatureFromBarIndex(int barIndex)
            {
                foreach (var item in timeSignatures)
                {
                    if (barIndex < item.Value.StartBarIndex) continue;
                    return item.Value.TimeSignature;
                }

                throw new InvalidOperationException();
            }

            public struct BarPosition
            {
                public int BarIndex { get; set; }
                public int TickOffset { get; set; }
                public TimeSignatureChangeEvent TimeSignature { get; set; }
            }

            public class TimeSignatureItem
            {
                public int StartTick { get; set; }
                public int StartBarIndex { get; set; }
                public TimeSignatureChangeEvent TimeSignature { get; set; }
            }
        }
    }

    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class SusArgs
    {
        [Newtonsoft.Json.JsonProperty]
        private string playLevel;
        [Newtonsoft.Json.JsonProperty]
        private Difficulty playDificulty;
        [Newtonsoft.Json.JsonProperty]
        private string extendedDifficulty;
        [Newtonsoft.Json.JsonProperty]
        private string songId;
        [Newtonsoft.Json.JsonProperty]
        private string soundFileName;
        [Newtonsoft.Json.JsonProperty]
        private decimal soundOffset;
        [Newtonsoft.Json.JsonProperty]
        private string jacketFilePath;
        [Newtonsoft.Json.JsonProperty]
        private bool hasPaddingBar;

        public string PlayLevel
        {
            get { return playLevel; }
            set { playLevel = value; }
        }

        public Difficulty PlayDifficulty
        {
            get { return playDificulty; }
            set { playDificulty = value; }
        }

        public string ExtendedDifficulty
        {
            get { return extendedDifficulty; }
            set { extendedDifficulty = value; }
        }

        public string SongId
        {
            get { return songId; }
            set { songId = value; }
        }

        public string SoundFileName
        {
            get { return soundFileName; }
            set { soundFileName = value; }
        }

        public decimal SoundOffset
        {
            get { return soundOffset; }
            set { soundOffset = value; }
        }

        public string JacketFilePath
        {
            get { return jacketFilePath; }
            set { jacketFilePath = value; }
        }

        public bool HasPaddingBar
        {
            get { return hasPaddingBar; }
            set { hasPaddingBar = value; }
        }

        public enum Difficulty
        {
            Basic,
            Advanced,
            Expert,
            Master,
            WorldsEnd
        }
    }
}
