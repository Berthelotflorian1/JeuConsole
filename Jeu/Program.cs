using System;
using System.Security.Cryptography.X509Certificates;

namespace TicTacToe

{
    // Liste des joueurs. présence de "Aucun" lors d'égalité.
    public enum Player
    {
        Aucun,
        X,
        O,
    }

    // Les différentes manières de gagner
    public enum WinType
    {
        Ligne,
        Colonne,
        Diagonale,
        AntiDiagonale,
    }

    // Décrire la manière de gagner pour expliquer comment un joueur a gagné
    public class WinInfo
    {
        public WinType WinType { get; set; }
        public int Number { get; set; }
        public WinType Type { get; internal set; }
    }

    // Pour indiquer le résultat de la partie
    public class GameResult
    {
        public Player Winner { get; set; }
        public WinInfo WinInfo { get; set; }
    }

    // Pour définir l'état du jeu : qui joue, qui a joué, est-ce que la partie est terminée ? Est-ce qu'il y a un gagnant ?
    public class GameState
    {
        public Player[,] GameGrid { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public int TurnsPassed { get; private set; }
        public bool GameOver { get; private set; }

        public event Action<int, int> MoveMade;
        public event Action<GameResult> GameEnded;
        public event Action GameRestarted;
        public GameState()
        {
            GameGrid = new Player[3, 3];
            CurrentPlayer = Player.X; TurnsPassed = 0; GameOver = false;

        }

        private bool CanMakeMove(int l, int c)
        {
            return !GameOver && GameGrid[l, c] == Player.Aucun;
        }

        private bool IsGridFull()
        {
            return TurnsPassed == 9;
        }

        private void SwitchPlayer()
        {
            if (CurrentPlayer == Player.X)
            {
                CurrentPlayer = Player.O;
            }
            else
            {
                CurrentPlayer = Player.X;
            }
        }
        // Savoir si une case est déjà utilisée
        private bool AreSquaresMarked((int, int)[] squares, Player player)
        {
            foreach ((int l, int c) in squares)
            {
                if (GameGrid[l, c] != player)
                {
                    return false;
                }

            }
            return true;

        }
        // Est-ce que le joueur vient de gagner ? et comment ? (ligne, colonne, diagonale ou antidiagonale?)
        private bool DidMoveWin(int l, int c, out WinInfo winInfo)
        {
            (int, int)[] Ligne = new[] { (l, 0), (l, 1), (l, 2) };
            (int, int)[] Colonne = new[] { (0, c), (1, c), (2, c) };
            (int, int)[] Diagonale = new[] { (0, 0), (1, 1), (2, 2) };
            (int, int)[] AntiDiagonale = new[] { (0, 2), (1, 1), (2, 0) };

            if (AreSquaresMarked(Ligne, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Ligne, Number = l };
                return true;
            }
            if (AreSquaresMarked(Colonne, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Colonne, Number = c };
                return true;
            }
            if (AreSquaresMarked(Diagonale, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Diagonale };
                return true;
            }

            if (AreSquaresMarked(AntiDiagonale, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.AntiDiagonale };
                return true;
            }

            winInfo = null;
            return false;
        }
        // Si joueur a gagné ou si match nul (quand le jeu est rempli)
        private bool DidMoveEndGame(int l, int c, out GameResult gameResult)
        {
            if (DidMoveWin(l, c, out WinInfo winInfo))
            {
                gameResult = new GameResult { Winner = CurrentPlayer, WinInfo = winInfo };
                return true;
            }

            if (IsGridFull())
            {
                gameResult = new GameResult { Winner = Player.Aucun };
                return true;
            }

            gameResult = null;
            return false;
        }

        public void MakeMove(int l, int c)
        {
            if (!CanMakeMove(l, c))
            {
                return;
            }


            GameGrid[l, c] = CurrentPlayer;
            TurnsPassed++;

            if (DidMoveEndGame(l, c, out GameResult gameResult))
            {
                GameOver = true;

                if (MoveMade != null)
                {
                    MoveMade(l, c);
                    GameEnded?.Invoke(gameResult);
                }
                else
                {
                    SwitchPlayer();
                    MoveMade?.Invoke(l, c);
                }
            }
        }
        // Redémarre le jeu, clear le jeu et recommence au joueur 1
        public void Reset()
            {
                GameGrid = new Player[3, 3];
                CurrentPlayer = Player.X;
                TurnsPassed = 0;
                GameOver = false;
                GameRestarted?.Invoke();
            }
    }
}
