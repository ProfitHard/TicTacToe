using Microsoft.AspNetCore.Mvc;
using TicTacToe.Models;
using System.Linq;

namespace TicTacToe.Controllers
{
    public class GameController : Controller
    {
        // ВАЖНО: Для AJAX, лучше НЕ использовать статику, а использовать сессии.
        // Но для демонстрации сохраняем статику.
        private static Game _currentGame = new Game();

        // GET: /Game/Index (Возвращает HTML)
        public IActionResult Index()
        {
            if (_currentGame.Board.All(c => char.IsDigit(c.ToCharArray()[0])))
            {
                _currentGame.StatusMessage = "Ваш ход (X)";
            }
            return View(_currentGame);
        }

        // POST: /Game/PlayerMove (Обрабатывает AJAX-запросы)
        [HttpPost]
        public IActionResult PlayerMove(int position)
        {
            // Если это AJAX-запрос, мы возвращаем JSON
            return ProcessMove(position);
        }

        // POST: /Game/HandleMove (Обрабатывает POST-запросы от кнопок, если JS отключен, но лучше использовать JSON)
        // Мы заставим кнопки в Index.cshtml использовать AJAX, но этот метод может остаться как запасной.
        // В этом примере мы будем полагаться только на AJAX, поэтому можем оставить его как дубликат,
        // или удалить, если вы уверены, что JS всегда включен.
        [HttpPost]
        public IActionResult HandleMove(int position)
        {
            return ProcessMove(position);
        }


        private IActionResult ProcessMove(int position)
        {
            // --- ЛОГИКА ИГРЫ ---

            if (position < 1 || position > 9)
            {
                return Json(new { success = false, message = "Некорректная позиция хода." });
            }

            string currentPlayerMarker = _currentGame.Player.ToString();
            string computerMarker = _currentGame.Computer.ToString();

            if (_currentGame.Board[position - 1] == currentPlayerMarker || _currentGame.Board[position - 1] == computerMarker)
            {
                return Json(new { success = false, board = _currentGame.Board, status = "Эта клетка уже занята. Выберите другую.", isGameOver = false });
            }

            _currentGame.PlayerMove(position);
            string statusMessage = "Ход компьютера...";

            if (_currentGame.CheckForWinner(_currentGame.Player))
            {
                var promoCode = _currentGame.GeneratePromoCode();
                statusMessage = $"Вы победили! Ваш промокод: {promoCode}";
                return Json(new { success = true, board = _currentGame.Board, status = statusMessage, isGameOver = true, winner = "Player", promo = promoCode });
            }

            if (_currentGame.IsBoardFull())
            {
                statusMessage = "Ничья!";
                return Json(new { success = true, board = _currentGame.Board, status = statusMessage, isGameOver = true, winner = "Draw" });
            }

            _currentGame.ComputerMove();

            if (_currentGame.CheckForWinner(_currentGame.Computer))
            {
                statusMessage = "Компьютер победил! Попробуйте снова.";
                return Json(new { success = true, board = _currentGame.Board, status = statusMessage, isGameOver = true, winner = "Computer" });
            }

            statusMessage = "Ваш ход (X)";

            // Возвращаем текущее состояние, если игра продолжается
            return Json(new { success = true, board = _currentGame.Board, status = statusMessage, isGameOver = false });
        }

        // POST: /Game/Restart (Возвращает JSON)
        [HttpPost]
        public IActionResult Restart()
        {
            _currentGame = new Game();
            // Возвращаем JSON с флагом и новым статусом
            return Json(new { success = true, message = "Игра сброшена", board = _currentGame.Board, status = "Ваш ход (X)" });
        }
    }
}