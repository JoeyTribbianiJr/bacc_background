
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections;

namespace Bacc_front
{
    public class LocalSession
    {
        public int Id { get; set; }
        public int SessionIndex { get; set; }
        public string JsonSession { get; set; }
    }
    /// <summary>
    /// 后台累积押分记录
    /// </summary>
    public class BackBetRecord
    {
        public string PlayerId { get; set; }
        public int BetScore { get; set; }
        public int Profit { get; set; }
        public int DingFen { get; set; }
        public int ZhongFen { get; set; }
    }
    /// 后台实时盈利
    /// </summary>
    public class BackLiveData
    {
        public int SessionIndex { get; set; }
        public int RoundIndex { get; set; }
        public int DeskBanker { get; set; }
        public int DeskPlayer { get; set; }
        public int DeskTie { get; set; }
        public int DeskCurScore { get; set; }
        public int Differ { get; set; }
        public int Profit { get; set; }
        public int MostPlayer { get; set; }
        public int Countdown { get; set; }
        public int State { get; set; }

        public int Winner { get; set; }
        public string JsonPlayerScores { get; set; }
    }
    /// <summary>
    /// 押分记录,也是路单记录
    /// </summary>
    public class BetScoreRecord
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public int SessionIndex { get; set; }
        public int RoundIndex { get; set; }
        public int DeskBanker { get; set; }
        public int DeskPlayer { get; set; }
        public int DeskTie { get; set; }
        public int Winner { get; set; }
        public string JsonPlayerScores { get; set; }
    }
    /// <summary>
    /// 前台账目
    /// </summary>
    public class FrontAccount
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsClear { get; set; }
        public DateTime ClearTime { get; set; }
        public string JsonScoreRecord { get; set; }
    }
    public class AddSubScoreRecord
    {
        public string PlayerId { get; set; }
        public int TotalAddScore { get; set; }
        public int TotalSubScore { get; set; }
        public int TotalAccount { get; set; }
    }
    public enum WinnerEnum
    {
        none = -1,
        banker = 0,
        tie = 1,
        player = 2
    }
    public enum BetSide
    {
        banker = 0,
        tie = 1,
        player = 2
    }
    public enum GameState
    {
        Preparing = -1,
        Shuffling = 0,
        Betting,
        Dealing,
        DealOver = 3
    }
    /// <summary>
    /// 每次押注面额
    /// </summary>
    public enum BetDenomination
    {
        big = 0,
        mini = 1
    }
    [PropertyChanged.ImplementPropertyChanged]
    public class WhoWin
    {
        public int Winner { get; set; }
        public WhoWin()
        {
            Winner = -1;
        }
    }

    public enum Suits
    {
        Club,
        Diamond,
        Heart,
        Spade,
    }
    public enum Weight
    {
        One = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
    }
    public enum RemoteCommand
    {
        //Image = 1,
        Login = 1,
        /// <summary>
        /// 将后台之前传送到前台路单发送给后台
        /// </summary>
        ImportFrontLocalSessions,
        /// <summary>
        /// 前台导入后台传送来的全部路单
        /// </summary>
        ImportBack,
        ImportBackOK,
        ImportBackFail,
        /// <summary>
        /// 前台导入后台传送来的下一局路单
        /// </summary>
        ImportBackNextSession,
        ImportBackNextSessionOnCurrentSession,
        ImportBackNextSessionOnNextSession,
        ImportBackNextSessionFail,

        /// <summary>
        /// 将前台正在运行的局传送到后台
        /// </summary>
        SendFrontCurSession,

        SendFrontPassword,
        ModifyFrontPassword,
        ModifyFrontPasswordOK,
        SendFrontSetting,
        ModifyFrontSetting,
        ModifyFrontSettingOK,
        SendFrontLiveData,
        SendFrontSummationBetRecord,
        SendFrontAccount,
        ClearFrontAccount,
        /// <summary>
        /// 将前台保存的押分记录的局数发送到后台
        /// </summary>
        SendFrontBetRecordIdList,
        /// <summary>
        /// 将某局的押分记录传送至后台
        /// </summary>
        SendFrontBetRecord,
        //SaveFrontBetRecord,

        ClearFrontLocalSessions,

        SetWinner,
        KillBig,
        ExtraWaybill,

        LockFront,
        LockFrontOK,
        UnlockFront,
        UnlockFrontOK,
        ShutdownFront,
        BreakdownFront,
    }
}