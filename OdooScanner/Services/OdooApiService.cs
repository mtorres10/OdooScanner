using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OdooScanner.Models;

namespace OdooScanner.Services
{
    public class OdooApiService
    {
        private readonly HttpClient _httpClient;
        private string? _sessionId;
        private int? _userId;
        private OdooCredentials? _credentials;

        public OdooApiService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<OdooAuthResponse> AuthenticateAsync(OdooCredentials credentials)
        {
            try
            {
                _credentials = credentials;
                var url = $"{credentials.Url}/web/session/authenticate";

                var payload = new
                {
                    jsonrpc = "2.0",
                    method = "call",
                    @params = new
                    {
                        db = credentials.Database,
                        login = credentials.Username,
                        password = credentials.Password
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new OdooAuthResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"HTTP Error: {response.StatusCode}"
                    };
                }

                var jsonResponse = JObject.Parse(responseBody);
                
                if (jsonResponse["error"] != null)
                {
                    var errorMessage = jsonResponse["error"]?["data"]?["message"]?.ToString() 
                                     ?? jsonResponse["error"]?["message"]?.ToString() 
                                     ?? "Authentication failed";
                    
                    return new OdooAuthResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = errorMessage
                    };
                }

                var result = jsonResponse["result"];
                if (result != null)
                {
                    _userId = result["uid"]?.Value<int>();
                    _sessionId = result["session_id"]?.ToString();

                    if (_userId.HasValue && _userId.Value > 0)
                    {
                        return new OdooAuthResponse
                        {
                            IsSuccess = true,
                            UserId = _userId,
                            SessionId = _sessionId
                        };
                    }
                }

                return new OdooAuthResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid credentials"
                };
            }
            catch (Exception ex)
            {
                return new OdooAuthResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<List<StockPicking>> GetStockPickingsAsync(string state = "assigned")
        {
            if (_credentials == null || !_userId.HasValue)
            {
                return new List<StockPicking>();
            }

            try
            {
                var url = $"{_credentials.Url}/web/dataset/search_read";

                var payload = new
                {
                    jsonrpc = "2.0",
                    method = "call",
                    @params = new
                    {
                        model = "stock.picking",
                        fields = new[] { "id", "name", "state", "picking_type_id", "scheduled_date", "origin" },
                        domain = new object[] { new object[] { "state", "=", state } },
                        limit = 50
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new List<StockPicking>();
                }

                var jsonResponse = JObject.Parse(responseBody);
                var records = jsonResponse["result"]?["records"] as JArray;

                if (records == null)
                {
                    return new List<StockPicking>();
                }

                var stockPickings = new List<StockPicking>();
                foreach (var record in records)
                {
                    stockPickings.Add(new StockPicking
                    {
                        Id = record["id"]?.Value<int>() ?? 0,
                        Name = record["name"]?.ToString() ?? string.Empty,
                        State = record["state"]?.ToString() ?? string.Empty,
                        PickingTypeId = record["picking_type_id"]?[1]?.ToString() ?? string.Empty,
                        ScheduledDate = record["scheduled_date"]?.Value<DateTime>() ?? DateTime.Now,
                        Origin = record["origin"]?.ToString() ?? string.Empty
                    });
                }

                return stockPickings;
            }
            catch (Exception)
            {
                return new List<StockPicking>();
            }
        }

        public async Task<bool> ValidateProductBarcodeAsync(string barcode)
        {
            if (_credentials == null || !_userId.HasValue)
            {
                return false;
            }

            try
            {
                var url = $"{_credentials.Url}/web/dataset/search_read";

                var payload = new
                {
                    jsonrpc = "2.0",
                    method = "call",
                    @params = new
                    {
                        model = "product.product",
                        fields = new[] { "id", "name", "barcode" },
                        domain = new object[] { new object[] { "barcode", "=", barcode } },
                        limit = 1
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var jsonResponse = JObject.Parse(responseBody);
                var records = jsonResponse["result"]?["records"] as JArray;

                return records != null && records.Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateStockMoveQuantityAsync(int moveId, double quantity)
        {
            if (_credentials == null || !_userId.HasValue)
            {
                return false;
            }

            try
            {
                var url = $"{_credentials.Url}/web/dataset/call_kw";

                var payload = new
                {
                    jsonrpc = "2.0",
                    method = "call",
                    @params = new
                    {
                        model = "stock.move",
                        method = "write",
                        args = new object[]
                        {
                            new[] { moveId },
                            new { quantity_done = quantity }
                        },
                        kwargs = new { }
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var jsonResponse = JObject.Parse(responseBody);
                return jsonResponse["result"]?.Value<bool>() ?? false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsAuthenticated => _userId.HasValue && _userId.Value > 0;
    }
}
