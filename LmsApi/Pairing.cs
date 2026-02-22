namespace Ojb500.EcfLms
{
    public class Pairing
    {
        public Pairing()
        {

        }
        internal Pairing(int board, bool firstPlayerWhite, Player firstPlayer, Player secondPlayer, GameResult result)
        {
            Board = board;
            FirstPlayerWhite = firstPlayerWhite;
            FirstPlayer = firstPlayer;
            SecondPlayer = secondPlayer;
            Result = result;
        }
        public int Board { get; set; }
        public bool FirstPlayerWhite { get; set; }

        public Player FirstPlayer { get; set; }

        public Player SecondPlayer { get; set; }

        public GameResult Result { get; set; }

        public override string ToString()
        {
            return $"[{Board}] {FirstPlayer} {(FirstPlayerWhite ? "(w)" : "(b)")} {Result} {SecondPlayer}";
        }
    }
}