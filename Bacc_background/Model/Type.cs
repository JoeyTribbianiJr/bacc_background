
using System.Collections;

namespace Bacc_front
{
    public class BackRecord
    {
        public int SessionIndex { get; set; }
        public int RoundIndex { get; set; }
        public int DeskBanker { get; set; }
        public int DeskPlayer { get; set; }
        public int DeskTie { get; set; }
        public int Differ { get; set; }
        public int Profit { get; set; }
        public int MostPlayer { get; set; }
        public int Countdown { get; set; }
        public string JsonSession { get; set; }
        public int State { get; set; }
        public int Winner { get; set; }
        public int DeskCurScore { get; set; }
        public string JsonPlayerScores { get; set; }
    }
    public class BetScoreRecord
    {
        public int Id { get; set; }
        public int SessionIndex { get; set; }
        public int RoundIndex { get; set; }
        public int DeskBanker { get; set; }
        public int DeskPlayer { get; set; }
        public int DeskTie { get; set; }
        public int Countdown { get; set; }
        public string JsonSession { get; set; }
        public int State { get; set; }
        public int Winner { get; set; }
        public string JsonPlayerScores { get; set; }
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
        Image = 1,

        ImportFront = 2,

        ReplaceWaybill = 3,
        ReplaceWaybillOK = 4,
        ReplaceWaybillFail = 5,

        ImportBack = 6,
        ImportBackOK = 7,
        ImportBackFail = 8,

        SendFrontSetting = 9,
        SendFrontData = 10,
    }
}