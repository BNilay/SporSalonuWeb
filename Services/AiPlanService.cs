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
            var geminiModel = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Gemini:ApiKey boş geliyor.");

            var notesText = string.IsNullOrWhiteSpace(model.Notes) ? "Belirtilmedi" : model.Notes;
            var equipmentText = string.IsNullOrWhiteSpace(model.Equipment) ? "Belirtilmedi" : model.Equipment;

            var jsonSchema = """
{
  "summary": "Bu program, 22 yaşında, 172 cm boyunda ve 60 kg ağırlığında, evde ekipmansız çalışarak kilo kaybı hedefleyen bir kadın için hazırlanmıştır. Haftada 3 gün tüm vücut antrenmanı ve sürdürülebilir bir beslenme yaklaşımı içermektedir. Amaç, 55 kg hedefine sağlıklı ve dengeli şekilde ulaşmaktır.",
  "training": [
    {
      "day": "Pazartesi",
      "focus": "Tüm Vücut – Kuvvet ve Stabilite",
      "exercises": [
        {
          "name": "Squat",
          "sets": "3",
          "reps": "15–20",
          "rest": "60 saniye"
        },
        {
          "name": "Şınav (dizler yerde veya tam)",
          "sets": "3",
          "reps": "8–12 veya maksimum",
          "rest": "60 saniye"
        },
        {
          "name": "Lunge (her bacak)",
          "sets": "3",
          "reps": "12–15",
          "rest": "60 saniye"
        },
        {
          "name": "Plank",
          "sets": "3",
          "reps": "30–60 saniye",
          "rest": "60 saniye"
        },
        {
          "name": "Glute Bridge",
          "sets": "3",
          "reps": "15–20",
          "rest": "60 saniye"
        }
      ]
    },
    {
      "day": "Çarşamba",
      "focus": "Tüm Vücut – Kardiyo ve Core",
      "exercises": [
        {
          "name": "Jumping Jacks",
          "sets": "3",
          "reps": "20–25",
          "rest": "45 saniye"
        },
        {
          "name": "Mountain Climbers",
          "sets": "3",
          "reps": "20–25 (her bacak)",
          "rest": "45 saniye"
        },
        {
          "name": "Bicycle Crunch",
          "sets": "3",
          "reps": "15–20",
          "rest": "45 saniye"
        },
        {
          "name": "Superman Hold",
          "sets": "3",
          "reps": "20–30 saniye",
          "rest": "45 saniye"
        }
      ]
    },
    {
      "day": "Cuma",
      "focus": "Tüm Vücut – Dayanıklılık",
      "exercises": [
        {
          "name": "Bodyweight Squat Pulse",
          "sets": "3",
          "reps": "20",
          "rest": "60 saniye"
        },
        {
          "name": "Reverse Lunge",
          "sets": "3",
          "reps": "12–15 (her bacak)",
          "rest": "60 saniye"
        },
        {
          "name": "Side Plank",
          "sets": "2",
          "reps": "20–30 saniye (her taraf)",
          "rest": "45 saniye"
        },
        {
          "name": "High Knees",
          "sets": "3",
          "reps": "30 saniye",
          "rest": "45 saniye"
        }
      ]
    }
  ],
  "progression_rules": {
    "week2": [
      "Her egzersizde 1 set ekle veya tekrar sayısını %10 artır.",
      "Dinlenme sürelerini 10–15 saniye azalt."
    ],
    "week3": [
      "Tempo kontrolü ekle (inişleri 3 saniyede yap).",
      "Plank ve core egzersizlerinde süreyi uzat."
    ],
    "week4": [
      "Yoğunluğu %20 azaltarak toparlanma haftası uygula.",
      "Hareket formuna ve esneme çalışmalarına odaklan."
    ]
  },
  "nutrition": {
    "calories": 1600,
    "macros": {
      "protein_g": 95,
      "carb_g": 180,
      "fat_g": 50
    },
    "sample_day": [
      "Kahvaltı: Yulaf, yoğurt ve meyve",
      "Ara öğün: 1 avuç badem",
      "Öğle: Izgara tavuk, bulgur ve salata",
      "Ara öğün: Yoğurt veya kefir",
      "Akşam: Sebze yemeği ve yoğurt"
    ]
  },
  "warnings": [
    "Egzersiz sırasında baş dönmesi veya ağrı hissedilirse antrenman durdurulmalıdır.",
    "Kronik rahatsızlık durumunda bir sağlık uzmanına danışılması önerilir."
  ]
}

""";

            var prompt = $"""
Sen bir fitness koçusun. Kullanıcı bilgilerine göre 1 haftalık egzersiz + beslenme planı üret ve ayrıca 2–4. hafta için ilerleme kurallarını ver.

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

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
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

            const int maxRetries = 2;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await _httpClient.PostAsync(url, content, ct);

                if ((int)resp.StatusCode == 429)
                {
                    if (attempt == maxRetries)
                        throw new Exception("429: Gemini limit/kota aşıldı.");

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
