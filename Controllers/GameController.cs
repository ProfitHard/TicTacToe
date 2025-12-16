using Microsoft.AspNetCore.Mvc;
using TicTacToe.Models;
using System.Diagnostics;

namespace TicTacToe.Controllers
{
    public class GameController : Controller
    {
        private static Game _currentGame = new Game();
        private readonly TelegramNotifier _tg;

        public GameController(TelegramNotifier tg)
        {
            _tg = tg ?? throw new ArgumentNullException(nameof(tg));
        }

        // GET: /Game/Index (возвращает HTML)
        public IActionResult Index()
        {
            if (_currentGame.Board.All(c => char.IsDigit(c.ToCharArray()[0])))
            {
                _currentGame.StatusMessage = "Ваш ход (X)";
            }
            return View(_currentGame);
        }

        // POST: /Game/PlayerMove (обрабатывает AJAX-запросы)
        [HttpPost]
        public async Task<IActionResult> PlayerMove(int position)
        {
            return await ProcessMove(position);
        }

        // POST: /Game/HandleMove (резервный метод, если JS отключен)
        [HttpPost]
        public async Task<IActionResult> HandleMove(int position)
        {
            return await ProcessMove(position);
        }

        private async Task<IActionResult> ProcessMove(int position)
        {
            if (position < 1 || position > 9)
                return Json(new { success = false, message = "Некорректная позиция хода." });

            string currentPlayerMarker = _currentGame.Player.ToString();
            string computerMarker = _currentGame.Computer.ToString();

            if (_currentGame.Board[position - 1] == currentPlayerMarker || _currentGame.Board[position - 1] == computerMarker)
            {
                return Json(new
                {
                    success = false,
                    board = _currentGame.Board,
                    status = "Эта клетка уже занята. Выберите другую.",
                    isGameOver = false
                });
            }

            _currentGame.PlayerMove(position);
            string statusMessage = "Ход компьютера...";

            if (_currentGame.CheckForWinner(_currentGame.Player))
            {
                var promoCode = _currentGame.GeneratePromoCode();
                statusMessage = $"Вы победили! Ваш промокод: {promoCode}";

                try
                {
                    await _tg.SendAsync($"Победа игрока 🎉 Промокод: {promoCode}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка отправки сообщения в Telegram: {ex.Message}");
                }

                return Json(new
                {
                    success = true,
                    board = _currentGame.Board,
                    status = statusMessage,
                    isGameOver = true,
                    winner = "Player",
                    promo = promoCode
                });
            }

            if (_currentGame.IsBoardFull())
            {
                statusMessage = "Ничья!";

                try
                {
                    await _tg.SendAsync("Ничья в игре крестики-нолики.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка отправки сообщения в Telegram: {ex.Message}");
                }

                return Json(new { success = true, board = _currentGame.Board, status = statusMessage, isGameOver = true, winner = "Draw" });
            }

            _currentGame.ComputerMove();

            if (_currentGame.CheckForWinner(_currentGame.Computer))
            {
                statusMessage = "Компьютер победил! Попробуйте снова.";

                try
                {
                    await _tg.SendAsync("Победа компьютера. Игрок проиграл.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка отправки сообщения в Telegram: {ex.Message}");
                }

                return Json(new { success = true, board = _currentGame.Board, status = statusMessage, isGameOver = true, winner = "Computer" });
            }

            statusMessage = "Ваш ход (X)";
            return Json(new { success = true, board = _currentGame.Board, status = statusMessage, isGameOver = false });
        }

        // POST: /Game/Restart (сбрасывает игру)
        [HttpPost]
        public async Task<IActionResult> Restart()
        {
            _currentGame = new Game();

            try
            {
                await _tg.SendAsync("Игра сброшена (Restart).");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отправки сообщения в Telegram: {ex.Message}");
            }

            return Json(new { success = true, message = "Игра сброшена", board = _currentGame.Board, status = "Ваш ход (X)" });
        }
    }
}