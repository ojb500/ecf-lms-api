namespace Ojb500.EcfLms
{
    /// <summary>A single board pairing within a <see cref="MatchCard"/>.</summary>
    public class Pairing
    {
        internal Pairing(int board, bool firstPlayerWhite, Player firstPlayer, Player secondPlayer, GameResult result)
        {
            Board = board;
            FirstPlayerWhite = firstPlayerWhite;
            FirstPlayer = firstPlayer;
            SecondPlayer = secondPlayer;
            Result = result;
        }

        /// <summary>Board number (1 = top board).</summary>
        public int Board { get; }

        /// <summary>Whether the home (first) player had the white pieces.</summary>
        public bool FirstPlayerWhite { get; }

        /// <summary>The home team's player on this board.</summary>
        public Player FirstPlayer { get; }

        /// <summary>The away team's player on this board.</summary>
        public Player SecondPlayer { get; }

        /// <summary>The result of this individual game.</summary>
        public GameResult Result { get; }

        public override string ToString()
        {
            return $"[{Board}] {FirstPlayer} {(FirstPlayerWhite ? "(w)" : "(b)")} {Result} {SecondPlayer}";
        }
    }
}
