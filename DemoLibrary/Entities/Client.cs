using DemoLibrary.Exceptions;
using DemoLibrary.Utils;

namespace DemoLibrary.Entities;

public struct Client
{
    private readonly ValidationHandler errorHandler = new();
    private string phone;
    private string password;
    private string? name;

    private static readonly string FilePath = Path.Combine("Data", "clients.csv");

    /// <summary>
    ///     Заголовок для CSV, що містить імена всіх властивостей класу Client.
    /// </summary>
    public static string Header =>
        $"{nameof(Phone)}{CsvUtil.Separator}{nameof(Password)}{CsvUtil.Separator}{nameof(Name)}{CsvUtil.Separator}{nameof(Address)}{CsvUtil.Separator}{nameof(RegistrationDate)}";

    /// <summary>
    ///     Ініціалізує новий екземпляр клієнта з вказаними властивостями.
    /// </summary>
    public Client(
        string phone,
        string password,
        string? name = null,
        string? address = null,
        DateTime? registrationDate = null)
    {
        this.Phone = phone;
        this.Password = password;
        this.Name = name;
        this.Address = address;
        this.RegistrationDate = registrationDate;

        if (this.errorHandler.HasErrors)
        {
            throw new ValidationException(this.errorHandler.Errors);
        }
    }

    public string Phone
    {
        get => this.phone;
        set
        {
            if (!PatternValidator.IsMatch(value, "##########"))
            {
                this.errorHandler.AddError(nameof(this.Phone), "Phone must be a 10-digit number.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Phone));
            }

            this.phone = value;
        }
    }

    public string Password
    {
        get => this.password;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 8)
            {
                this.errorHandler.AddError(nameof(this.Password), "Password must be at least 8 characters long.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Password));
            }

            this.password = value;
        }
    }

    public string? Name
    {
        get => this.name;
        set
        {
            if (value != null && value.Length < 2)
            {
                this.errorHandler.AddError(nameof(this.Name), "Name must be at least 2 characters long if provided.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Name));
            }

            this.name = value;
        }
    }

    public string? Address { get; set; }

    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    ///     Серіалізує поточний об'єкт клієнта в CSV рядок.
    /// </summary>
    public string Serialize()
    {
        return $"{CsvUtil.Escape(this.Phone)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Password)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Name ?? string.Empty)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Address ?? string.Empty)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.RegistrationDate?.ToString("yyyy-MM-dd") ?? string.Empty)}";
    }

    /// <summary>
    ///     Десеріалізує рядок CSV в об'єкт клієнта.
    /// </summary>
    public static Client Deserialize(string csvLine)
    {
        string[] fields = CsvUtil.ParseCsvLine(csvLine);
        if (fields.Length != 5)
        {
            throw new FormatException("Each CSV row must have exactly 5 fields.");
        }

        return new Client(
            fields[0], // Phone
            fields[1], // Password
            fields[2] == string.Empty ? null : fields[2], // Name
            fields[3] == string.Empty ? null : fields[3], // Address
            string.IsNullOrEmpty(fields[4]) ? null : DateTime.Parse(fields[4]) // RegistrationDate
        );
    }

    /// <summary>
    ///     Зберігає поточний екземпляр клієнта в файл, або оновлює існуючий запис, якщо клієнт уже існує.
    /// </summary>
    public void Save()
    {
        List<Client> clients = GetAll();

        bool exists = false;
        foreach (Client client in clients)
        {
            if (client.Phone.Equals(this.Phone, StringComparison.OrdinalIgnoreCase))
            {
                exists = true;
                break;
            }
        }

        if (exists)
        {
            this.Update();
        }
        else
        {
            this.Add();
        }
    }

    /// <summary>
    ///     Додає нового клієнта до CSV файлу.
    /// </summary>
    private void Add()
    {
        List<Client> clients = GetAll();

        // Перевірка на наявність заголовків у файлі
        string directoryPath = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath); // Створюємо директорію, якщо вона не існує
        }

        // Перевірка на наявність файлу, якщо його немає — створюємо
        if (!File.Exists(FilePath))
        {
            File.Create(FilePath).Close(); // Створюємо файл, якщо він не існує
        }

        // Якщо файл порожній, додаємо заголовок
        if (new FileInfo(FilePath).Length == 0)
        {
            File.AppendAllLines(FilePath, new[] { Header });
        }

        string line = this.Serialize();
        File.AppendAllLines(FilePath, new[] { line });
    }

    /// <summary>
    ///     Оновлює існуючого клієнта в CSV файлі.
    /// </summary>
    private void Update()
    {
        List<Client> clients = GetAll();
        bool found = false;

        // Шукаємо клієнта в списку та оновлюємо його дані
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].Phone.Equals(this.Phone, StringComparison.OrdinalIgnoreCase))
            {
                clients[i] = this;
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Client not found.");
        }

        SaveAll(clients);
    }

    /// <summary>
    ///     Отримує клієнта за його номером телефону.
    /// </summary>
    public static Client Get(string phone)
    {
        List<Client> clients = GetAll();
        foreach (Client client in clients)
        {
            if (client.Phone.Equals(phone, StringComparison.OrdinalIgnoreCase))
            {
                return client;
            }
        }

        throw new KeyNotFoundException("Client with the specified phone number not found.");
    }

    /// <summary>
    ///     Отримує всіх клієнтів з CSV файлу.
    /// </summary>
    public static List<Client> GetAll()
    {
        List<Client> clients = new();

        if (!File.Exists(FilePath))
        {
            return clients;
        }

        string[] lines = File.ReadAllLines(FilePath);

        if (lines.Length > 0 && !lines[0].Equals(Header))
        {
            throw new InvalidOperationException("CSV file format is incorrect, missing headers.");
        }

        foreach (string line in lines.Skip(1)) // Пропускаємо перший рядок (заголовок)
        {
            try
            {
                Client client = Deserialize(line);
                clients.Add(client);
            }
            catch (Exception)
            {
                // Пропускаємо некоректні записи
            }
        }

        return clients;
    }

    /// <summary>
    ///     Видаляє клієнта з CSV файлу за його номером телефону.
    /// </summary>
    public static void Delete(string phone)
    {
        List<Client> clients = GetAll();
        bool found = false;

        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].Phone.Equals(phone, StringComparison.OrdinalIgnoreCase))
            {
                clients.RemoveAt(i);
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Client not found.");
        }

        SaveAll(clients);
    }

    /// <summary>
    ///     Зберігає всіх клієнтів до CSV файлу.
    /// </summary>
    private static void SaveAll(List<Client> clients)
    {
        List<string> lines = new() { Header };

        foreach (Client client in clients)
        {
            lines.Add(client.Serialize());
        }

        File.WriteAllLines(FilePath, lines);
    }
}