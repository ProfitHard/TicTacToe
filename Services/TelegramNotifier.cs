using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class TelegramNotifier
{
    private readonly HttpClient _client;
    private readonly ILogger<TelegramNotifier> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Конструктор для инициализации объекта TelegramNotifier.
    /// </summary>
    /// <param name="client">Клиент HTTP для отправки запросов.</param>
    /// <param name="logger">Объект журнала для регистрации активности.</param>
    /// <param name="configuration">Конфигурация приложения для получения токена и Chat ID.</param>
    public TelegramNotifier(HttpClient client, ILogger<TelegramNotifier> logger, IConfiguration configuration)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Асинхронно отправляет сообщение в Telegram.
    /// </summary>
    /// <param name="text">Текст сообщения.</param>
    /// <returns>True, если сообщение отправлено успешно; False в противном случае.</returns>
    public async Task<bool> SendAsync(string text)
    {
        var token = GetConfiguredToken();
        var chatId = GetConfiguredChatId();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(chatId))
        {
            _logger.LogError("Токен или Chat ID не найдены в конфигурации.");
            return false;
        }

        var endpointUrl = $"https://api.telegram.org/bot{token}/sendMessage";

        var payload = new Dictionary<string, object>()
        {
            ["chat_id"] = chatId,
            ["text"] = text
        };

        var serializedPayload = SerializePayload(payload);

        using var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
        {
            Content = new StringContent(serializedPayload, Encoding.UTF8, "application/json")
        };

        try
        {
            using var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await DeserializeError(response);
                _logger.LogError($"Ошибка отправки сообщения в Telegram: {errorDetails}. Статус-код: {response.StatusCode}");
                return false;
            }

            _logger.LogInformation("Сообщение успешно отправлено в Telegram.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при попытке отправки сообщения в Telegram.");
            return false;
        }
    }

    #region Вспомогательные методы

    private string GetConfiguredToken()
    {
        return _configuration.GetValue<string>("Telegram:BotToken");
    }

    private string GetConfiguredChatId()
    {
        return _configuration.GetValue<string>("Telegram:ChatId");
    }

    private string SerializePayload(object payload)
    {
        return JsonConvert.SerializeObject(payload);
    }

    private async Task<string> DeserializeError(HttpResponseMessage response)
    {
        var errorStream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(errorStream);
        return await reader.ReadToEndAsync();
    }

    #endregion
}