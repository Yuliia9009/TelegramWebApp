using TelegramWebAPI.Data;
using TelegramWebAPI.Models;

public class CountrySeeder
{
    private readonly ApplicationDbContext _context;

    public CountrySeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public void SeedCountries()
    {
        var rawData = File.ReadAllText("countries_raw.txt"); // 👉 сюда вставь весь список
        var lines = rawData.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var countries = new List<Country>();

        foreach (var line in lines)
        {
            var parts = line.Split('\t'); // предполагаем, что столбцы разделены табуляцией

            if (parts.Length < 3)
                continue;

            var name = parts[0].Trim();
            var code = parts[1].Trim();
            var isoRaw = parts[2].Trim();
            var iso = isoRaw.Split('/').FirstOrDefault()?.Trim();

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(iso))
            {
                countries.Add(new Country
                {
                    Id = code,
                    Name = name,
                    Iso = iso,
                    Code = code
                });
            }
        }

        _context.Countries.AddRange(countries);
        _context.SaveChanges();
    }
}