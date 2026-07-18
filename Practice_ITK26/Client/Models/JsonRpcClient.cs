using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client.Models.Services
{
    /// <summary>
    /// Простой JSON‑RPC 2.0 клиент поверх HTTP.
    /// Отправляет запросы на /jsonrpc и десериализует ответы.
    /// </summary>
    public class JsonRpcClient
    {
        private readonly HttpClient _httpClient;
        private int _nextId = 1; // инкрементируемый идентификатор запроса

        public JsonRpcClient(string baseUrl)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        /// <summary>
        /// Отправляет JSON‑RPC запрос с указанным методом и параметрами.
        /// </summary>
        
        public async Task<T> SendAsync<T>(string method, object? parameters = null)
        {
            // Формируем объект запроса по спецификации JSON-RPC 2.0
            var request = new
            {
                jsonrpc = "2.0",
                id = _nextId++,
                method,
                @params = parameters
            };

            // Отправляем POST на /jsonrpc
            var response = await _httpClient.PostAsJsonAsync("/jsonrpc", request);
            response.EnsureSuccessStatusCode(); // бросает исключение, если HTTP статус не 200

            // Десериализуем ответ в обёртку с результатом
            var responseJson = await response.Content.ReadFromJsonAsync<JsonRpcResponse<T>>();
            if (responseJson == null)
                throw new InvalidOperationException("Пустой ответ от сервера");

            // Если есть поле error – выбрасываем исключение
            if (responseJson.Error != null)
                throw new Exception($"JSON‑RPC ошибка {responseJson.Error.Code}: {responseJson.Error.Message}");

            return responseJson.Result!;
        }

        // Вспомогательные классы для десериализации ответа
        private class JsonRpcResponse<TResult>
        {
            public string Jsonrpc { get; set; } = "";
            public TResult? Result { get; set; }
            public JsonRpcError? Error { get; set; }
            public int Id { get; set; }
        }

        private class JsonRpcError
        {
            public int Code { get; set; }
            public string Message { get; set; } = "";
        }
    }
}