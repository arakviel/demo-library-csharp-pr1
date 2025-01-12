namespace DemoLibrary.Entities;

public struct BookIssueRecord
{
    // Шлях до файлу
    private static readonly string FilePath = Path.Combine("Data", "book_issues.csv");

    // Властивості для об'єктів
    public Guid Id { get; set; }

    public Book Book { get; set; }

    public Client Client { get; set; }

    public Employee Employee { get; set; }

    // Властивості для збереження ідентифікаторів
    private string BookIsbn => this.Book.Isbn;

    private string ClientPhone => this.Client.Phone;

    private string EmployeePhone => this.Employee.Phone;

    // Дата видачі та повернення
    public DateTime IssueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    // Статичний метод для отримання запису по GUID
    public static BookIssueRecord Get(Guid id)
    {
        List<BookIssueRecord> records = GetAll();
        foreach (BookIssueRecord record in records)
        {
            if (record.Id == id)
            {
                return record;
            }
        }

        throw new KeyNotFoundException($"No record found with ID {id}");
    }

    // Статичний метод для отримання записів за ISBN книги
    public static List<BookIssueRecord> GetByBookIsbn(string isbn)
    {
        List<BookIssueRecord> records = GetAll();
        List<BookIssueRecord> result = new();

        foreach (BookIssueRecord record in records)
        {
            if (record.BookIsbn == isbn)
            {
                result.Add(record);
            }
        }

        if (result.Count == 0)
        {
            throw new KeyNotFoundException($"No records found for book with ISBN {isbn}");
        }

        return result;
    }

    // Статичний метод для отримання записів за номером телефону клієнта
    public static List<BookIssueRecord> GetByClientPhone(string phone)
    {
        List<BookIssueRecord> records = GetAll();
        List<BookIssueRecord> result = new();

        foreach (BookIssueRecord record in records)
        {
            if (record.ClientPhone == phone)
            {
                result.Add(record);
            }
        }

        if (result.Count == 0)
        {
            throw new KeyNotFoundException($"No records found for client with phone {phone}");
        }

        return result;
    }

    // Статичний метод для отримання записів за номером телефону працівника
    public static List<BookIssueRecord> GetByEmployeePhone(string phone)
    {
        List<BookIssueRecord> records = GetAll();
        List<BookIssueRecord> result = new();

        foreach (BookIssueRecord record in records)
        {
            if (record.EmployeePhone == phone)
            {
                result.Add(record);
            }
        }

        if (result.Count == 0)
        {
            throw new KeyNotFoundException($"No records found for employee with phone {phone}");
        }

        return result;
    }

    // Заголовок для CSV файлу
    public static string Header =>
        $"{nameof(BookIsbn)}{CsvUtil.Separator}{nameof(ClientPhone)}{CsvUtil.Separator}{nameof(EmployeePhone)}{CsvUtil.Separator}{nameof(IssueDate)}{CsvUtil.Separator}{nameof(ReturnDate)}{CsvUtil.Separator}{nameof(Id)}";

    // Серіалізація до CSV
    public string Serialize()
    {
        return $"{CsvUtil.Escape(this.BookIsbn)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.ClientPhone)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.EmployeePhone)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.IssueDate.ToString("yyyy-MM-dd"))}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.ReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Id.ToString())}";
    }

    // Десеріалізація з CSV
    public static BookIssueRecord Deserialize(string csvLine)
    {
        string[] fields = CsvUtil.ParseCsvLine(csvLine);
        if (fields.Length != 6)
        {
            throw new FormatException("Each CSV row must have exactly 6 fields.");
        }

        return new BookIssueRecord
        {
            Book = Book.Get(fields[0]),
            Client = Client.Get(fields[1]),
            Employee = Employee.Get(fields[2]),
            IssueDate = DateTime.Parse(fields[3]),
            ReturnDate = string.IsNullOrEmpty(fields[4]) ? null : DateTime.Parse(fields[4]),
            Id = Guid.Parse(fields[5])
        };
    }

    // Зберігає новий запис або оновлює що існує
    public void Save()
    {
        List<BookIssueRecord> records = GetAll();
        bool exists = false;
        foreach (BookIssueRecord record in records)
        {
            if (record.BookIsbn == this.BookIsbn && record.ClientPhone == this.ClientPhone &&
                record.EmployeePhone == this.EmployeePhone)
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

    // Додає новий запис до CSV
    private void Add()
    {
        List<BookIssueRecord> records = GetAll();

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

    // Оновлює існуючий запис у файлі
    private void Update()
    {
        List<BookIssueRecord> records = GetAll();
        bool found = false;

        for (int i = 0; i < records.Count; i++)
        {
            if (records[i].BookIsbn == this.BookIsbn && records[i].ClientPhone == this.ClientPhone &&
                records[i].EmployeePhone == this.EmployeePhone)
            {
                records[i] = this;
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Record not found.");
        }

        SaveAll(records);
    }

    // Отримує всі записи з CSV файлу
    public static List<BookIssueRecord> GetAll()
    {
        List<BookIssueRecord> records = new();

        // Якщо файл не існує, повертаємо порожній список
        if (!File.Exists(FilePath))
        {
            return records;
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
                BookIssueRecord record = Deserialize(line);
                records.Add(record);
            }
            catch (Exception)
            {
                // Пропускаємо некоректні записи
            }
        }

        return records;
    }

    // Зберігає всі записи до CSV файлу
    private static void SaveAll(List<BookIssueRecord> records)
    {
        List<string> lines = new() { Header };

        foreach (BookIssueRecord record in records)
        {
            lines.Add(record.Serialize());
        }

        File.WriteAllLines(FilePath, lines);
    }
}