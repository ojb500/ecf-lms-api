namespace Ojb500.EcfLms
{
    public class Pairing
    {
        public Pairing(int board, bool firstPlayerWhite, Player firstPlayer, Player secondPlayer, GameResult result)
        {
            Board = board;
            FirstPlayerWhite = firstPlayerWhite;
            FirstPlayer = firstPlayer;
            SecondPlayer = secondPlayer;
            Result = result;
        }
        public int Board { get; }
        public bool FirstPlayerWhite { get; }

        public Player FirstPlayer { get; }

        public Player SecondPlayer { get; }

        public GameResult Result { get; }

        public override string ToString()
        {
            return $"[{Board}] {FirstPlayer} {(FirstPlayerWhite ? "(w)" : "(b)")} {Result} {SecondPlayer}";
        }
    }
}