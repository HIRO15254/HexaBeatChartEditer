using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core.Notes
{
    public interface IAirable
    {
        /// <summary>
        /// ノートの位置を表すTickを設定します。
        /// </summary>
        int Tick { get; }

        /// <summary>
        /// ノートの配置されるレーン番号を取得します。。
        /// </summary>
        int LaneIndex { get; }

        /// <summary>
        /// ノートのレーン幅を取得します。
        /// </summary>
        int Width { get; }
    }
    public abstract class NoteBase
    {
    }
}
