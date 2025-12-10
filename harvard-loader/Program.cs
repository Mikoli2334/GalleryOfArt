
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Npgsql;

class Program
{
    static async Task Main()
    {
        // 1. Загружаем конфиг из appsettings.Development.json + переменные окружения
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var apiKey   = config["Harvard:ApiKey"]!;
        var baseUrl  = config["Harvard:BaseUrl"]!;
        var pageSize = int.TryParse(config["Harvard:PageSize"], out var ps) ? ps : 50;
        var connStr  = config["Database:ConnectionString"]!;

        Console.WriteLine("Harvard API loader is starting…");

        // 2. HttpClient
        using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        http.Timeout = TimeSpan.FromSeconds(30);

        // 3. Подключение к PostgreSQL
        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();

        var sql = @"
            INSERT INTO staging.harvard_objects
            (id, title, dated, classification, technique, medium, culture, period, department,
            people, primaryimageurl, url, raw, fetched_at)
            VALUES (@id, @title, @dated, @classification, @technique, @medium, @culture, @period, @department,
                    @people::jsonb, @primaryimageurl, @url, @raw::jsonb, now())
            ON CONFLICT (id) DO UPDATE SET
                title           = EXCLUDED.title,
                dated           = EXCLUDED.dated,
                classification  = EXCLUDED.classification,
                technique       = EXCLUDED.technique,
                medium          = EXCLUDED.medium,
                culture         = EXCLUDED.culture,
                period          = EXCLUDED.period,
                department      = EXCLUDED.department,
                people          = EXCLUDED.people,
                primaryimageurl = EXCLUDED.primaryimageurl,
                url             = EXCLUDED.url,
                raw             = EXCLUDED.raw,
                fetched_at      = now();
        ";

        int page = 1;
        int totalInsertedOrUpdated = 0;

        while (true)
        {
            var url = $"/object?apikey={apiKey}&classification=Paintings&hasimage=1&size={pageSize}&page={page}";
            JsonElement root;

            // --- HTTP с ретраем ---
            try
            {
                root = await HttpExt.FetchJsonAsync(http, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP error on page {page}: {ex.Message}");
                await Task.Delay(1000);

                try
                {
                    root = await HttpExt.FetchJsonAsync(http, url);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Repeated HTTP error on page {page}: {ex2.Message}");
                    break;
                }
            }

            // Нет records → выходим
            if (root.ValueKind == JsonValueKind.Undefined ||
                !root.TryGetProperty("records", out var records) ||
                records.GetArrayLength() == 0)
            {
                Console.WriteLine("No more records or empty page.");
                break;
            }

            // Транзакция на страницу
            await using var tx = await conn.BeginTransactionAsync();
            int pageCount = 0;

            foreach (var rec in records.EnumerateArray())
            {
                long     id             = rec.Get<long>("id");
                string?  title          = rec.Get<string>("title");
                string?  dated          = rec.Get<string>("dated");
                string?  classification = rec.Get<string>("classification");
                string?  technique      = rec.Get<string>("technique");
                string?  medium         = rec.Get<string>("medium");
                string?  culture        = rec.Get<string>("culture");
                string?  period         = rec.Get<string>("period");
                string?  department     = rec.Get<string>("department");
                string?  primaryImage   = rec.Get<string>("primaryimageurl");
                string?  urlPage        = rec.Get<string>("url");
                string?  peopleJson     = rec.TryGetProperty("people", out var ppl) ? ppl.GetRawText() : null;

                await using var cmd = new NpgsqlCommand(sql, conn, tx);
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("title",           (object?)title          ?? DBNull.Value);
                cmd.Parameters.AddWithValue("dated",           (object?)dated          ?? DBNull.Value);
                cmd.Parameters.AddWithValue("classification",  (object?)classification ?? DBNull.Value);
                cmd.Parameters.AddWithValue("technique",       (object?)technique      ?? DBNull.Value);
                cmd.Parameters.AddWithValue("medium",          (object?)medium         ?? DBNull.Value);
                cmd.Parameters.AddWithValue("culture",         (object?)culture        ?? DBNull.Value);
                cmd.Parameters.AddWithValue("period",          (object?)period         ?? DBNull.Value);
                cmd.Parameters.AddWithValue("department",      (object?)department     ?? DBNull.Value);
                cmd.Parameters.AddWithValue("people",          (object?)peopleJson     ?? DBNull.Value);
                cmd.Parameters.AddWithValue("primaryimageurl", (object?)primaryImage   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("url",             (object?)urlPage        ?? DBNull.Value);
                cmd.Parameters.AddWithValue("raw",             rec.GetRawText());

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    pageCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ DB error for id={id}: {ex.Message}");
                }
            }

            await tx.CommitAsync();

            totalInsertedOrUpdated += pageCount;
            Console.WriteLine($"Page {page}: upserted {pageCount} rows (total: {totalInsertedOrUpdated})");

            // Проверяем, не последняя ли это страница
            if (!root.TryGetProperty("info", out var info) ||
                !info.TryGetProperty("pages", out var pagesEl))
            {
                break;
            }

            var totalPages = pagesEl.GetInt32();
            if (page >= totalPages) break;

            page++;
            await Task.Delay(200); // маленькая пауза, чтобы не душить API
        }

        Console.WriteLine($"Done. Total upserted: {totalInsertedOrUpdated}");
    }
}

static class HttpExt
{
    public static async Task<JsonElement> FetchJsonAsync(HttpClient http, string url)
    {
        var json = await http.GetStringAsync(url);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}

static class JsonExt
{
    public static T Get<T>(this JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p))
            return default!;

        try
        {
            return JsonSerializer.Deserialize<T>(p.GetRawText())!;
        }
        catch
        {
            if (typeof(T) == typeof(string))
                return (T)(object)(p.ValueKind == JsonValueKind.Null ? null : p.ToString());

            if (typeof(T) == typeof(long) &&
                p.ValueKind == JsonValueKind.Number &&
                p.TryGetInt64(out var l))
                return (T)(object)l;

            if (typeof(T) == typeof(int) &&
                p.ValueKind == JsonValueKind.Number &&
                p.TryGetInt32(out var i))
                return (T)(object)i;
        }

        return default!;
    }
}
