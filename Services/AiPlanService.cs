using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using yeniWeb.Models.ViewModels;

namespace yeniWeb.Services
{
    public class GeminiPlanService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiPlanService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GeneratePlanAsync(PlanRequestVm model, CancellationToken ct = default)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var geminiModel = _configuration["Gemini:Model"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Gemini:ApiKey boş geliyor.");

            if (string.IsNullOrWhiteSpace(geminiModel))
                geminiModel = "models/gemini-2.0-flash";

            if (!geminiModel.StartsWith("models/"))
                geminiModel = "models/" + geminiModel;

            var notesText = string.IsNullOrWhiteSpace(model.Notes) ? "Belirtilmedi" : model.Notes;
            var equipmentText = string.IsNullOrWhiteSpace(model.Equipment) ? "Belirtilmedi" : model.Equipment;

            var jsonSchema = """
{
  "summary": "string",
  "photo_insights": ["string"],
  "training": [
    {
      "day": "string",
      "focus": "string",
      "exercises": [
        {
          "name": "string",
          "sets": "string",
          "reps": "string",
          "rest": "string"
        }
      ]
    }
  ],
  "progression_rules": {
    "week2": ["string"],
    "week3": ["string"],
    "week4": ["string"]
  },
  "nutrition": {
    "calories": "number",
    "macros": {
      "protein_g": "number",
      "carb_g": "number",
      "fat_g": "number"
    },
    "sample_day": ["string"]
  },
  "warnings": ["string"]
}
""";

            var prompt = $"""
Sen bir fitness koçusun. Kullanıcı bilgilerine göre 1 haftalık egzersiz + beslenme planı üret ve ayrıca 2–4. hafta için ilerleme kurallarını ver.
Eğer fotoğraf verilmişse, fotoğraftan genel gözlemler çıkar (ör. duruş/proporsiyon gibi) ve önerilere yansıt.

ÇIKTI KURALLARI:
- SADECE geçerli JSON döndür. JSON dışında hiçbir metin yazma.
- JSON parse edilebilir olmalı, çift tırnak kullan, trailing virgül kullanma.
- Değerler Türkçe olsun.
- Tıbbi teşhis koyma, ilaç/tedavi önerme. Risk varsa sadece warnings alanında doktora yönlendir.

JSON ŞEMASI:
{jsonSchema}

İÇERİK:
- training sadece 1 haftayı kapsasın ve haftada {model.DaysPerWeek} gün antrenman yaz.
- Hedef: {model.Goal}
- Ekipman: {equipmentText}
- Notlar: {notesText}

KULLANICI:
- Yaş: {model.Age}
- Cinsiyet: {model.Sex}
- Boy: {model.HeightCm} cm
- Kilo: {model.WeightKg} kg

Şimdi SADECE JSON döndür.
""";

            string? base64 = null;
            string? mime = null;

            if (model.Photo != null && model.Photo.Length > 0)
            {
                if (!model.Photo.ContentType.StartsWith("image/"))
                    throw new Exception("Lütfen sadece görsel dosyası yükleyin.");

                if (model.Photo.Length > 5 * 1024 * 1024)
                    throw new Exception("Fotoğraf çok büyük. 5MB altı yükleyin.");

                using var ms = new MemoryStream();
                await model.Photo.CopyToAsync(ms, ct);
                base64 = Convert.ToBase64String(ms.ToArray());
                mime = model.Photo.ContentType;
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/{geminiModel}:generateContent";

            object[] parts = base64 == null
                ? new object[] { new { text = prompt } }
                : new object[]
                {
                    new { text = prompt },
                    new { inlineData = new { mimeType = mime, data = base64 } }
                };

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = parts
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 2000,
                    temperature = 0.3
                }
            };

            var json = JsonSerializer.Serialize(body);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

            const int maxRetries = 3;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await _httpClient.PostAsync(url, content, ct);

                if ((int)resp.StatusCode == 429 || (int)resp.StatusCode == 503)
                {
                    if (attempt == maxRetries)
                        throw new Exception("Model şu anda yoğun veya limit aşıldı. Biraz sonra tekrar deneyin.");

                    await Task.Delay(TimeSpan.FromSeconds(5 * (attempt + 1)), ct);
                    continue;
                }

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync(ct);
                    throw new Exception(err);
                }

                var respJson = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(respJson);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "{}";
            }

            return "{}";
        }
    }
}
