﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HexaBeatChartEditer.Core
{
    /// <summary>
    /// 譜面ファイルを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class ScoreBook
    {
        internal static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        [Newtonsoft.Json.JsonProperty]
        private Version version = typeof(ScoreBook).Assembly.GetName().Version;
        [Newtonsoft.Json.JsonProperty]
        private string title = "";
        [Newtonsoft.Json.JsonProperty]
        private string artistName = "";
        [Newtonsoft.Json.JsonProperty]
        private string notesDesignerName = "";
        [Newtonsoft.Json.JsonProperty]
        private Score score = new Score();
        [Newtonsoft.Json.JsonProperty]
        private Dictionary<string, object> exporterArgs = new Dictionary<string, object>();

        public string Path { get; set; }

        /// <summary>
        /// ファイルを作成したアプリケーションのバージョンを設定します。
        /// </summary>
        public Version Version
        {
            get { return version; }
            set { version = value; }
        }

        /// <summary>
        /// 楽曲のタイトルを設定します。
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// 楽曲のアーティストを設定します。
        /// </summary>
        public string ArtistName
        {
            get { return artistName; }
            set { artistName = value; }
        }

        /// <summary>
        /// 譜面制作者名を設定します。
        /// </summary>
        public string NotesDesignerName
        {
            get { return notesDesignerName; }
            set { notesDesignerName = value; }
        }

        /// <summary>
        /// 譜面データを設定します。
        /// </summary>
        public Score Score
        {
            get { return score; }
            set { score = value; }
        }

        /// <summary>
        /// エクスポート用の設定を格納します。
        /// </summary>
        public Dictionary<string, object> ExporterArgs
        {
            get { return exporterArgs; }
            set { exporterArgs = value; }
        }

        public void Save(string path)
        {
            Path = path;
            Save();
        }

        public void Save()
        {
            string data = JsonConvert.SerializeObject(this, SerializerSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using (var stream = new MemoryStream(bytes))
            {
                using (var file = new FileStream(Path, FileMode.Create))
                using (var gz = new GZipStream(file, CompressionMode.Compress))
                {
                    stream.CopyTo(gz);
                }
            }
        }

        /// <summary>
        /// 指定のファイルから<see cref="ScoreBook"/>のインスタンスを生成します。
        /// 古いバージョンのファイルは現在のバージョン用に変換されます。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ScoreBook LoadFile(string path)
        {
            string data = GetDecompressedData(path);
            var doc = JObject.Parse(data);
            var fileVersion = GetFileVersion(path);

            doc["version"] = JObject.FromObject(typeof(ScoreBook).Assembly.GetName().Version);

            //var res = doc.ToObject<ScoreBook>(JsonSerializer.Create(SerializerSettings));
            var res = doc.ToObject<ScoreBook>();
            // 循環参照は復元できないねん……

            if (res.Score.Events.TimeSignatureChangeEvents.Count == 0)
            {
                res.Score.Events.TimeSignatureChangeEvents.Add(new Events.TimeSignatureChangeEvent() { Tick = 0, Numerator = 4, DenominatorExponent = 2 });
            }

            res.score.Notes.Taps = res.score.Notes.Taps.Distinct().ToList();
            res.score.Notes.DTaps = res.score.Notes.DTaps.Distinct().ToList();
            res.score.Notes.HTaps = res.score.Notes.HTaps.Distinct().ToList();
            res.score.Notes.LTaps = res.score.Notes.LTaps.Distinct().ToList();
            res.score.Notes.Traces = res.score.Notes.Traces.Distinct().ToList();
            res.score.Notes.DTraces = res.score.Notes.DTraces.Distinct().ToList();
            res.score.Notes.HTraces = res.score.Notes.HTraces.Distinct().ToList();
            res.score.Notes.LTraces = res.score.Notes.LTraces.Distinct().ToList();
            res.score.Notes.Holds = res.score.Notes.Holds.Distinct().ToList();
            res.score.Notes.DHolds = res.score.Notes.DHolds.Distinct().ToList();
            res.score.Notes.HHolds = res.score.Notes.HHolds.Distinct().ToList();
            res.score.Notes.LHolds = res.score.Notes.LHolds.Distinct().ToList();

            res.Path = path;
            return res;
        }

        /// <summary>
        /// 指定のファイルが現在のバージョンと互換性があるかどうか調べます。
        /// </summary>
        /// <param name="path">ファイルへのパス</param>
        /// <returns>互換性があればtrue, 互換性がなければfalse</returns>
        public static bool IsCompatible(string path)
        {
            Version current = typeof(ScoreBook).Assembly.GetName().Version;
            return GetFileVersion(path).Major <= current.Major;
        }

        /// <summary>
        /// 指定のファイルに対してバージョンアップが必要かどうか調べます。
        /// </summary>
        /// <param name="path">ファイルへのパス</param>
        /// <returns>読み込み可能であればtrue, 不可能であればfalse</returns>
        public static bool IsUpgradeNeeded(string path)
        {
            Version current = typeof(ScoreBook).Assembly.GetName().Version;
            return GetFileVersion(path).Major == current.Major;
        }

        private static string GetDecompressedData(string path)
        {
            using (var gz = new GZipStream(new FileStream(path, FileMode.Open), CompressionMode.Decompress))
            using (var stream = new MemoryStream())
            {
                gz.CopyTo(stream);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// 指定のファイルに保存されたデータのバージョンを取得します。
        /// </summary>
        /// <param name="path">ファイルへのパス</param>
        /// <returns>ファイルが生成されたバージョン</returns>
        private static Version GetFileVersion(string path)
        {
            var doc = JObject.Parse(GetDecompressedData(path));
            Version current = typeof(ScoreBook).Assembly.GetName().Version;
            return doc["version"].ToObject<Version>();
        }
    }
}
