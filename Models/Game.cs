namespace TicTacToe.Models
{
    public class Game
    {
        // *** ИСПРАВЛЕНИЕ 1: Board теперь string[] для совместимости с Razor и логикой !!! ***
        public string[] Board { get; set; } = new string[9] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        public char Player { get; } = 'X';
        public char Computer { get; } = 'O';
        public bool IsPlayerTurn { get; set; } = true;

        // !!! ДОБАВЛЕНО: Свойство для вывода статуса в Index.cshtml !!!
        public string StatusMessage { get; set; } = "Ваш ход (X)";

        public void PlayerMove(int position)
        {
            Board[position - 1] = Player.ToString(); // Преобразуем char в string
        }

        public void ComputerMove()
        {
            Random random = new Random();
            int move;
            do
            {
                move = random.Next(1, 10);
                // Сравнение теперь идет со строкой, так как Board - string[]
            } while (Board[move - 1] == Player.ToString() || Board[move - 1] == Computer.ToString());

            Board[move - 1] = Computer.ToString();
            IsPlayerTurn = true;
            StatusMessage = "Ваш ход (X)"; // Сбрасываем статус после хода компьютера
        }

        public bool CheckForWinner(char marker)
        {
            string markerStr = marker.ToString(); // Преобразуем для сравнения с Board

            return (Board[0] == markerStr && Board[1] == markerStr && Board[2] == markerStr) ||
                   (Board[3] == markerStr && Board[4] == markerStr && Board[5] == markerStr) ||
                   (Board[6] == markerStr && Board[7] == markerStr && Board[8] == markerStr) ||
                   (Board[0] == markerStr && Board[3] == markerStr && Board[6] == markerStr) ||
                   (Board[1] == markerStr && Board[4] == markerStr && Board[7] == markerStr) ||
                   (Board[2] == markerStr && Board[5] == markerStr && Board[8] == markerStr) ||
                   (Board[0] == markerStr && Board[4] == markerStr && Board[8] == markerStr) ||
                   (Board[2] == markerStr && Board[4] == markerStr && Board[6] == markerStr);
        }

        public bool IsBoardFull()
        {
            // Проверяем, остались ли еще цифры ('1'...'9')
            return Board.All(x => x == Player.ToString() || x == Computer.ToString());
        }

        public string GeneratePromoCode()
        {
            Random random = new Random();
            return "WIN" + random.Next(10000, 99999).ToString();
        }
    }
}