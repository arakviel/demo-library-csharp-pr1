using DemoLibrary.Entities;
using DemoLibrary.Exceptions;
using DemoLibrary.Utils;

/// <summary>
///     Представляє автора з біографією, датами народження та смерті, а також надає можливість
///     серіалізувати/десеріалізувати з CSV формату,
///     зберігати, оновлювати та видаляти записи в CSV файлі.
/// </summary>
public struct Author
{
    // Обробник помилок валідації
    private readonly ValidationHandler errorHandler = new();

    // Дати народження та смерті автора
    private DateTime? birthDate;
    private DateTime? deathDate;

    // Ім'я автора
    private string name;

    /// <summary>
    ///     Шлях до CSV файлу, де зберігаються дані авторів.
    /// </summary>
    private static readonly string FilePath = Path.Combine("Data", "authors.csv");

    /// <summary>
    ///     Заголовок для CSV файлу, що містить імена всіх властивостей автора.
    /// </summary>
    public static string Header =>
        $"{nameof(Name)}{CsvUtil.Separator}{nameof(Biography)}{CsvUtil.Separator}{nameof(BirthDate)}{CsvUtil.Separator}{nameof(DeathDate)}";

    /// <summary>
    ///     Ініціалізує новий екземпляр автора з вказаними властивостями.
    /// </summary>
    /// <param name="name">Ім'я автора.</param>
    /// <param name="biography">Біографія автора.</param>
    /// <param name="birthDate">Дата народження автора.</param>
    /// <param name="deathDate">Дата смерті автора.</param>
    /// <exception cref="ValidationException">Викидається, якщо під час ініціалізації виникають помилки валідації.</exception>
    public Author(string name, string? biography = null, DateTime? birthDate = null, DateTime? deathDate = null)
    {
        this.Name = name;
        this.Biography = biography;
        this.BirthDate = birthDate;
        this.DeathDate = deathDate;

        // Перевірка на наявність помилок валідації
        if (this.errorHandler.HasErrors)
        {
            throw new ValidationException(this.errorHandler.Errors);
        }
    }

    /// <summary>
    ///     Отримує або встановлює ім'я автора.
    /// </summary>
    public string Name
    {
        get => this.name;
        set
        {
            // Валідація на наявність порожнього значення
            if (string.IsNullOrWhiteSpace(value))
            {
                this.errorHandler.AddError(nameof(this.Name), "Name cannot be empty.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Name));
            }

            this.name = value;
        }
    }

    /// <summary>
    ///     Отримує або встановлює біографію автора.
    /// </summary>
    public string? Biography { get; set; }

    /// <summary>
    ///     Отримує або встановлює дату народження автора.
    /// </summary>
    public DateTime? BirthDate
    {
        get => this.birthDate;
        set
        {
            // Валідація на наявність дати, що вказує на майбутнє
            if (value > DateTime.Now)
            {
                this.errorHandler.AddError(nameof(this.BirthDate), "Birth date cannot be in the future.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.BirthDate));
            }

            this.birthDate = value;
        }
    }

    /// <summary>
    ///     Отримує або встановлює дату смерті автора.
    /// </summary>
    public DateTime? DeathDate
    {
        get => this.deathDate;
        set
        {
            // Валідація на наявність дати смерті, яка не може бути раніше дати народження
            if (this.BirthDate.HasValue && value.HasValue && value < this.BirthDate)
            {
                this.errorHandler.AddError(nameof(this.DeathDate), "Death date cannot be earlier than birth date.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.DeathDate));
            }

            this.deathDate = value;
        }
    }

    /// <summary>
    ///     Серіалізує поточний об'єкт автора в CSV рядок.
    /// </summary>
    /// <returns>CSV рядок, що містить інформацію про автора.</returns>
    public string Serialize()
    {
        return $"{CsvUtil.Escape(this.Name)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Biography ?? string.Empty)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.BirthDate?.ToString("yyyy-MM-dd") ?? string.Empty)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.DeathDate?.ToString("yyyy-MM-dd") ?? string.Empty)}";
    }

    /// <summary>
    ///     Десеріалізує рядок CSV в об'єкт автора.
    /// </summary>
    /// <param name="csvLine">Рядок CSV, що містить інформацію про автора.</param>
    /// <returns>Екземпляр автора.</returns>
    /// <exception cref="FormatException">Викидається, якщо рядок CSV не містить чотирьох полів.</exception>
    public static Author Deserialize(string csvLine)
    {
        string[] fields = CsvUtil.ParseCsvLine(csvLine);
        if (fields.Length != 4)
        {
            throw new FormatException("Each CSV row must have exactly 4 fields.");
        }

        return new Author(
            fields[0], // Name
            fields[1] == string.Empty ? null : fields[1], // Biography
            string.IsNullOrEmpty(fields[2]) ? null : DateTime.Parse(fields[2]), // BirthDate
            string.IsNullOrEmpty(fields[3]) ? null : DateTime.Parse(fields[3]) // DeathDate
        );
    }

    /// <summary>
    ///     Зберігає поточний екземпляр автора в файл, або оновлює існуючий запис, якщо автор уже існує.
    /// </summary>
    public void Save()
    {
        List<Author> authors = GetAll();

        bool exists = false;
        foreach (Author author in authors)
        {
            if (author.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase))
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
    ///     Додає нового автора до CSV файлу.
    /// </summary>
    private void Add()
    {
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
    ///     Оновлює існуючого автора в CSV файлі.
    /// </summary>
    private void Update()
    {
        List<Author> authors = GetAll();
        bool found = false;

        // Шукаємо автора в списку та оновлюємо його дані
        for (int i = 0; i < authors.Count; i++)
        {
            if (authors[i].Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase))
            {
                authors[i] = this;
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Author not found.");
        }

        SaveAll(authors);
    }

    /// <summary>
    ///     Отримує автора за його ім'ям.
    /// </summary>
    /// <param name="name">Ім'я автора.</param>
    /// <returns>Автор, що має зазначене ім'я.</returns>
    /// <exception cref="KeyNotFoundException">Викидається, якщо автор з таким ім'ям не знайдений.</exception>
    public static Author Get(string name)
    {
        List<Author> authors = GetAll();
        foreach (Author author in authors)
        {
            if (author.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return author;
            }
        }

        throw new KeyNotFoundException("Author with the specified name not found.");
    }

    public static List<Author> GetByBirthdateFromTo(DateTime start, DateTime end)
    {
        List<Author> authors = GetAll();
        List<Author> result = new();

        foreach (Author author in authors)
        {
            if (author.BirthDate.HasValue && author.BirthDate.Value >= start && author.BirthDate.Value <= end)
            {
                result.Add(author);
            }
        }

        return result;
    }

    public static List<Author> GetByDeathdateFromTo(DateTime start, DateTime end)
    {
        List<Author> authors = GetAll();
        List<Author> result = new();

        foreach (Author author in authors)
        {
            if (author.DeathDate.HasValue && author.DeathDate.Value >= start && author.DeathDate.Value <= end)
            {
                result.Add(author);
            }
        }

        return result;
    }

    /// <summary>
    ///     Отримує всіх авторів з CSV файлу.
    /// </summary>
    /// <returns>Список авторів.</returns>
    public static List<Author> GetAll()
    {
        List<Author> authors = new();

        // Якщо файл не існує, повертаємо порожній список
        if (!File.Exists(FilePath))
        {
            return authors;
        }

        string[] lines = File.ReadAllLines(FilePath);

        // Перевірка на заголовок у файлі
        if (lines.Length > 0 && !lines[0].Equals(Header))
        {
            throw new InvalidOperationException("CSV file format is incorrect, missing headers.");
        }

        foreach (string line in lines.Skip(1)) // Пропускаємо перший рядок (заголовок)
        {
            try
            {
                Author author = Deserialize(line);
                authors.Add(author);
            }
            catch (Exception)
            {
                // Пропускаємо некоректні записи
            }
        }

        return authors;
    }

    /// <summary>
    ///     Видаляє автора з CSV файлу за його ім'ям.
    /// </summary>
    /// <param name="name">Ім'я автора, якого потрібно видалити.</param>
    /// <exception cref="KeyNotFoundException">Викидається, якщо автор з таким ім'ям не знайдений.</exception>
    public static void Delete(string name)
    {
        List<Author> authors = GetAll();
        bool found = false;

        // Шукаємо автора для видалення
        for (int i = 0; i < authors.Count; i++)
        {
            if (authors[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                authors.RemoveAt(i);
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Author not found.");
        }

        SaveAll(authors);
    }

    /// <summary>
    ///     Зберігає всіх авторів до CSV файлу.
    /// </summary>
    private static void SaveAll(List<Author> authors)
    {
        List<string> lines = new() { Header }; // Додаємо заголовок при збереженні

        foreach (Author author in authors)
        {
            lines.Add(author.Serialize());
        }

        File.WriteAllLines(FilePath, lines);
    }
}