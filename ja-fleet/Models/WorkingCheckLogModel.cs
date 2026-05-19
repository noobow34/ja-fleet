namespace jafleet.Models
{
    /// <summary>
    /// 1機体分の稼働チェックログ詳細行
    /// </summary>
    public class WorkingCheckLogEntry
    {
        /// <summary>レジ番号（例: JA123A）</summary>
        public string? RegistrationNumber { get; set; }

        /// <summary>型式名（例: B737-800）</summary>
        public string? TypeName { get; set; }

        /// <summary>特記マーク（◎=特別塗装, ☆=整備通知）</summary>
        public string? NotifyMark { get; set; }

        /// <summary>最終フライト日時</summary>
        public string? FlightDate { get; set; }

        /// <summary>出発空港（生文字列 "Tokyo (NRT)" 形式）</summary>
        public string? FromAp { get; set; }

        /// <summary>到着空港（生文字列 "Tokyo (NRT)" 形式）</summary>
        public string? ToAp { get; set; }

        /// <summary>出発空港コード（例: NRT）</summary>
        public string? FromApCode { get; set; }

        /// <summary>出発空港名（例: Tokyo）</summary>
        public string? FromApName { get; set; }

        /// <summary>到着空港コード（例: HND）</summary>
        public string? ToApCode { get; set; }

        /// <summary>到着空港名（例: Tokyo）</summary>
        public string? ToApName { get; set; }

        /// <summary>便名</summary>
        public string? FlightNumber { get; set; }

        /// <summary>フライト状態</summary>
        public string? Status { get; set; }

        /// <summary>前回フライト日時（稼働変化時のみ）</summary>
        public string? PreviousDate { get; set; }

        /// <summary>FR24ページ存在</summary>
        public bool? ExistPage { get; set; }

        /// <summary>写真存在</summary>
        public bool? ExistPhoto { get; set; }

        /// <summary>運航情報存在</summary>
        public bool? ExistOperation { get; set; }

        /// <summary>待機時間（ミリ秒）</summary>
        public string? WaitMs { get; set; }

        /// <summary>パースできなかった生の行テキスト</summary>
        public string? RawLine { get; set; }
    }

    /// <summary>
    /// セクション（--------見出し--------）単位のグループ
    /// </summary>
    public class WorkingCheckLogSection
    {
        public string? SectionTitle { get; set; }
        public List<WorkingCheckLogEntry> Entries { get; set; } = new();
    }

    /// <summary>
    /// 1バッチ実行分のWorkingCheckLogまとまり
    /// </summary>
    public class WorkingCheckLogBatch
    {
        /// <summary>バッチ終了日時（"RefreshWorkingStatus正常終了:" 行から取得）</summary>
        public string? FinishedAt { get; set; }

        /// <summary>セクション一覧（概要行を除いた詳細）</summary>
        public List<WorkingCheckLogSection> Sections { get; set; } = new();

        /// <summary>全エントリをフラットに返す（フィルタ用）</summary>
        public IEnumerable<WorkingCheckLogEntry> AllEntries => Sections.SelectMany(s => s.Entries);
    }

    public class WorkingCheckLogModel : BaseModel
    {
        public DateTime SearchDate { get; set; }
        public List<WorkingCheckLogBatch> Batches { get; set; } = new();

        /// <summary>全エントリ数（ページ・写真・運航情報チェック行を合算）</summary>
        public int TotalEntryCount => Batches.Sum(b => b.AllEntries.Count());

        /// <summary>セクションに含まれるエントリの種別サマリ（ページ上部の統計バッジ用）</summary>
        public int WorkingChangedCount => Batches.Sum(b =>
            b.Sections.Where(s => s.SectionTitle != null && s.SectionTitle.Contains("稼働"))
                      .Sum(s => s.Entries.Count));
    }
}
