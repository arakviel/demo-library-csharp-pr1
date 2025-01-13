using DemoLibrary.Exceptions;
using DemoLibrary.Utils;

namespace DemoLibrary.Entities;

public struct Book
{
    private readonly ValidationHandler errorHandler = new();
    private Author author;

    private string isbn;
    private int? pages;
    private DateTime? publicationDate;
    private string title;

    /// <summary>
    ///     Шлях до CSV файлу, де зберігаються дані книг.
    /// </summary>
    private static readonly string FilePath = Path.Combine("Data", "books.csv");

    /// <summary>
    ///     Заголовок для CSV, що містить імена всіх властивостей книги.
    /// </summary>
    public static string Header =>
        $"{nameof(Isbn)}{CsvUtil.Separator}{nameof(Title)}{CsvUtil.Separator}{nameof(Author)}{CsvUtil.Separator}{nameof(Description)}{CsvUtil.Separator}{nameof(Pages)}{CsvUtil.Separator}{nameof(PublicationDate)}";

    public Book(
        string isbn,
        string title,
        Author author,
        string? description = null,
        int? pages = null,
        DateTime? publicationDate = null)
    {
        this.Isbn = isbn;
        this.Title = title;
        this.Author = author;
        this.Description = description;
        this.Pages = pages;
        this.PublicationDate = publicationDate;

        if (this.errorHandler.HasErrors)
        {
            throw new ValidationException(this.errorHandler.Errors);
        }
    }

    public string Isbn
    {
        get => this.isbn;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !PatternValidator.IsMatch(value, "###-#-##-######-#"))
            {
                this.errorHandler.AddError(nameof(this.Isbn), "ISBN must follow the format ###-#-##-######-#.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Isbn));
            }

            this.isbn = value;
        }
    }

    public string Title
    {
        get => this.title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                this.errorHandler.AddError(nameof(this.Title), "Title cannot be empty.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Title));
            }

            this.title = value;
        }
    }

    public Author Author
    {
        get => this.author;
        set
        {
            if (value.Equals(default(Author)))
            {
                this.errorHandler.AddError(nameof(this.Author), "Author cannot be null.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Author));
            }

            this.author = value;
        }
    }

    public string? Description { get; set; }

    public int? Pages
    {
        get => this.pages;
        set
        {
            if (value.HasValue && value <= 0)
            {
                this.errorHandler.AddError(nameof(this.Pages), "Pages must be greater than zero.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Pages));
            }

            this.pages = value;
        }
    }

    public DateTime? PublicationDate
    {
        get => this.publicationDate;
        set
        {
            if (value.HasValue && value > DateTime.Now)
            {
                this.errorHandler.AddError(nameof(this.PublicationDate), "Publication date cannot be in the future.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.PublicationDate));
            }

            this.publicationDate = value;
        }
    }

    public string Serialize()
    {
        return $"{CsvUtil.Escape(this.Isbn)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Title)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Author.Name)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Description ?? string.Empty)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Pages?.ToString() ?? string.Empty)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.PublicationDate?.ToString("yyyy-MM-dd") ?? string.Empty)}";
    }

    /// <summary>
    ///     Десеріалізує рядок CSV в об'єкт книги.
    /// </summary>
    /// <param name="csvLine">Рядок CSV, що містить інформацію про книгу.</param>
    /// <returns>Екземпляр книги.</returns>
    /// <exception cref="FormatException">Викидається, якщо рядок CSV не містить всіх полів.</exception>
    public static Book Deserialize(string csvLine)
    {
        string[] fields = CsvUtil.ParseCsvLine(csvLine);
        if (fields.Length != 6)
        {
            throw new FormatException("Each CSV row must have exactly 6 fields.");
        }

        return new Book(
            fields[0], // ISBN
            fields[1], // Title
            Author.Get(fields[2]), // Author
            fields[3] == string.Empty ? null : fields[3], // Description
            string.IsNullOrEmpty(fields[4]) ? null : int.Parse(fields[4]), // Pages
            string.IsNullOrEmpty(fields[5]) ? null : DateTime.Parse(fields[5]) // PublicationDate
        );
    }

    /// <summary>
    ///     Зберігає поточний екземпляр книги в файл, або оновлює існуючий запис, якщо книга вже існує.
    /// </summary>
    public void Save()
    {
        List<Book> books = GetAll();

        bool exists = false;
        foreach (Book book in books)
        {
            if (book.Isbn.Equals(this.Isbn, StringComparison.OrdinalIgnoreCase))
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
    ///     Додає нову книгу до CSV файлу.
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
    ///     Оновлює існуючу книгу в CSV файлі.
    /// </summary>
    private void Update()
    {
        List<Book> books = GetAll();
        bool found = false;

        // Шукаємо книгу в списку та оновлюємо її дані
        for (int i = 0; i < books.Count; i++)
        {
            if (books[i].Isbn.Equals(this.Isbn, StringComparison.OrdinalIgnoreCase))
            {
                books[i] = this;
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Book not found.");
        }

        SaveAll(books);
    }

    /// <summary>
    ///     Отримує книгу за її ISBN.
    /// </summary>
    /// <param name="isbn">ISBN книги.</param>
    /// <returns>Книга з вказаним ISBN.</returns>
    /// <exception cref="KeyNotFoundException">Викидається, якщо книга з таким ISBN не знайдена.</exception>
    public static Book Get(string isbn)
    {
        List<Book> books = GetAll();
        foreach (Book book in books)
        {
            if (book.Isbn.Equals(isbn, StringComparison.OrdinalIgnoreCase))
            {
                return book;
            }
        }

        throw new KeyNotFoundException("Book with the specified ISBN not found.");
    }

    /// <summary>
    ///     Отримує всі книги з CSV файлу.
    /// </summary>
    /// <returns>Список книг.</returns>
    public static List<Book> GetAll()
    {
        List<Book> books = new();

        // Якщо файл не існує, повертаємо порожній список
        if (!File.Exists(FilePath))
        {
            return books;
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
                Book book = Deserialize(line);
                books.Add(book);
            }
            catch (Exception)
            {
                // Пропускаємо некоректні записи
            }
        }

        return books;
    }

    /// <summary>
    ///     Видаляє книгу з CSV файлу за її ISBN.
    /// </summary>
    /// <param name="isbn">ISBN книги, яку потрібно видалити.</param>
    /// <exception cref="KeyNotFoundException">Викидається, якщо книга з таким ISBN не знайдена.</exception>
    public static void Delete(string isbn)
    {
        List<Book> books = GetAll();
        bool found = false;

        // Шукаємо книгу за ISBN
        for (int i = 0; i < books.Count; i++)
        {
            if (books[i].Isbn.Equals(isbn, StringComparison.OrdinalIgnoreCase))
            {
                books.RemoveAt(i);
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Book not found.");
        }

        SaveAll(books);
    }

    /// <summary>
    ///     Зберігає всі книги в CSV файлі.
    /// </summary>
    private static void SaveAll(List<Book> books)
    {
        List<string> serializedBooks = new();
        serializedBooks.Add(Header);

        foreach (Book book in books)
        {
            serializedBooks.Add(book.Serialize());
        }

        File.WriteAllLines(FilePath, serializedBooks);
    }
}