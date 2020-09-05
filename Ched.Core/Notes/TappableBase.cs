using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexaBeatChartEditer.Core.Notes
{
    public abstract class TapBase : ShortNoteBase
    {
    }

    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public abstract class TappableBase : TapBase, IAirable
    {
        [Newtonsoft.Json.JsonProperty]
        private int tick;
        [Newtonsoft.Json.JsonProperty]
        private int laneIndex;
        [Newtonsoft.Json.JsonProperty]
        private int width = 1;

        public override string ToString() => $"{tick}:{laneIndex}";

        public override bool Equals(object obj)
        {
            var p = obj as TappableBase;
            if (p == null)
                return false;
            return (this.tick == p.tick && this.laneIndex == p.laneIndex);
        }

        public override int GetHashCode() => tick.GetHashCode() ^ laneIndex.GetHashCode();
        /// <summary>
        /// ノートの位置を表すTickを設定します。
        /// </summary>
        public int Tick
        {
            get { return tick; }
            set
            {
                if (tick == value) return;
                if (tick < 0) throw new ArgumentOutOfRangeException("value", "value must not be negative.");
                tick = value;
            }
        }

        /// <summary>
        /// ノートの配置されるレーン番号を設定します。。
        /// </summary>
        public int LaneIndex
        {
            get { return laneIndex; }
            set
            {
                CheckPosition(value, Width);
                laneIndex = value;
            }
        }

        /// <summary>
        /// ノートのレーン幅を設定します。
        /// </summary>
        public int Width
        {
            get { return width; }
            set
            {
                CheckPosition(LaneIndex, value);
                width = value;
            }
        }

        protected void CheckPosition(int laneIndex, int width)
        {
            if (width < 1 || width > Constants.LanesCount)
                throw new ArgumentOutOfRangeException("width", "Invalid width.");
            if (laneIndex < 0 || laneIndex + width > Constants.LanesCount)
                throw new ArgumentOutOfRangeException("laneIndex", "Invalid lane index.");
        }

        public void SetPosition(int laneIndex, int width)
        {
            CheckPosition(laneIndex, width);
            this.laneIndex = laneIndex;
            this.width = width;
        }
    }
}
