using DemoLibrary.Entities;

namespace DemoLibrary.Repositories;

/// <summary>
///     Репозиторій для роботи з об'єктами Book в файлах CSV.
/// </summary>
public class BookRepository
{
    private readonly string filePath = Path.Combine("Data", "books.csv");

    /// <summary>
    ///     Отримує книгу за ISBN.
    /// </summary>
    /// <param name="isbn">ISBN книги.</param>
    /// <returns>Книга з відповідним ISBN.</returns>
    public Book Get(string isbn)
    {
        List<Book> books = this.GetAll();
        foreach (Book book in books)
        {
            if (book.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase))
            {
                return book;
            }
        }

        throw new KeyNotFoundException("Book with the specified ISBN not found.");
    }

    /// <summary>
    ///     Отримує усі книги.
    /// </summary>
    /// <returns>Список усіх книг.</returns>
    public List<Book> GetAll()
    {
        List<Book> books = new();

        if (!File.Exists(this.filePath))
        {
            return books;
        }

        string[] lines = File.ReadAllLines(this.filePath);

        // Перевіряємо, чи є заголовки у файлі
        if (lines.Length > 0 && !lines[0].Equals(Book.Header))
        {
            throw new InvalidOperationException("CSV file format is incorrect, missing headers.");
        }

        foreach (string line in lines.Skip(1)) // Пропускаємо перший рядок (заголовок)
        {
            try
            {
                Book book = Book.Deserialize(line);
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
    ///     Додає нову книгу в CSV файл.
    /// </summary>
    /// <param name="book">Книга для додавання.</param>
    public void Add(Book book)
    {
        List<Book> books = this.GetAll();
        foreach (Book existingBook in books)
        {
            if (existingBook.ISBN.Equals(book.ISBN, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Book with the same ISBN already exists.");
            }
        }

        // Перевіряємо, чи є заголовки у файлі, якщо їх немає — додаємо
        if (new FileInfo(this.filePath).Length == 0)
        {
            File.AppendAllLines(this.filePath, new[] { Book.Header });
        }

        string line = book.Serialize();
        File.AppendAllLines(this.filePath, new[] { line });
    }

    /// <summary>
    ///     Оновлює інформацію про книгу в CSV файлі.
    /// </summary>
    /// <param name="book">Оновлена книга.</param>
    public void Update(Book book)
    {
        List<Book> books = this.GetAll();
        bool found = false;

        for (int i = 0; i < books.Count; i++)
        {
            if (books[i].ISBN.Equals(book.ISBN, StringComparison.OrdinalIgnoreCase))
            {
                books[i] = book;
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Book not found.");
        }

        this.SaveAll(books);
    }

    /// <summary>
    ///     Видаляє книгу з CSV файлу.
    /// </summary>
    /// <param name="isbn">ISBN книги, яку потрібно видалити.</param>
    public void Delete(string isbn)
    {
        List<Book> books = this.GetAll();
        bool found = false;

        for (int i = 0; i < books.Count; i++)
        {
            if (books[i].ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase))
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

        this.SaveAll(books);
    }

    /// <summary>
    ///     Зберігає усі книги в CSV файл.
    /// </summary>
    /// <param name="books">Список книг для збереження.</param>
    private void SaveAll(List<Book> books)
    {
        List<string> lines = new() { Book.Header }; // Додаємо заголовок при збереженні

        foreach (Book book in books)
        {
            lines.Add(book.Serialize());
        }

        File.WriteAllLines(this.filePath, lines);
    }
}