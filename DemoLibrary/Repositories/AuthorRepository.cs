using DemoLibrary.Entities;

namespace DemoLibrary.Repositories;

/// <summary>
///     Репозиторій для роботи з об'єктами Author в файлах CSV.
/// </summary>
public class AuthorRepository
{
    private readonly string filePath = Path.Combine("Data", "authors.csv");

    /// <summary>
    ///     Отримує автора за ім'ям.
    /// </summary>
    /// <param name="name">Ім'я автора.</param>
    /// <returns>Автор з відповідним ім'ям.</returns>
    public Author Get(string name)
    {
        List<Author> authors = this.GetAll();
        foreach (Author author in authors)
        {
            if (author.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return author;
            }
        }

        throw new KeyNotFoundException("Author with the specified name not found.");
    }

    /// <summary>
    ///     Отримує усіх авторів.
    /// </summary>
    /// <returns>Список усіх авторів.</returns>
    public List<Author> GetAll()
    {
        List<Author> authors = new();

        if (!File.Exists(this.filePath))
        {
            return authors;
        }

        string[] lines = File.ReadAllLines(this.filePath);

        // Перевіряємо, чи є заголовки у файлі
        if (lines.Length > 0 && !lines[0].Equals(Author.Header))
        {
            throw new InvalidOperationException("CSV file format is incorrect, missing headers.");
        }

        foreach (string line in lines.Skip(1)) // Пропускаємо перший рядок (заголовок)
        {
            try
            {
                Author author = Author.Deserialize(line);
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
    ///     Додає нового автора в CSV файл.
    /// </summary>
    /// <param name="author">Автор для додавання.</param>
    public void Add(Author author)
    {
        List<Author> authors = this.GetAll();
        foreach (Author existingAuthor in authors)
        {
            if (existingAuthor.Name.Equals(author.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Author with the same name already exists.");
            }
        }

        // Перевіряємо, чи є заголовки у файлі, якщо їх немає — додаємо
        if (new FileInfo(this.filePath).Length == 0)
        {
            File.AppendAllLines(this.filePath, new[] { Author.Header });
        }

        string line = author.Serialize();
        File.AppendAllLines(this.filePath, new[] { line });
    }

    /// <summary>
    ///     Оновлює інформацію про автора в CSV файлі.
    /// </summary>
    /// <param name="author">Оновлений автор.</param>
    public void Update(Author author)
    {
        List<Author> authors = this.GetAll();
        bool found = false;

        for (int i = 0; i < authors.Count; i++)
        {
            if (authors[i].Name.Equals(author.Name, StringComparison.OrdinalIgnoreCase))
            {
                authors[i] = author;
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Author not found.");
        }

        this.SaveAll(authors);
    }

    /// <summary>
    ///     Видаляє автора з CSV файлу.
    /// </summary>
    /// <param name="name">Ім'я автора, якого потрібно видалити.</param>
    public void Delete(string name)
    {
        List<Author> authors = this.GetAll();
        bool found = false;

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

        this.SaveAll(authors);
    }

    /// <summary>
    ///     Зберігає усіх авторів в CSV файл.
    /// </summary>
    /// <param name="authors">Список авторів для збереження.</param>
    private void SaveAll(List<Author> authors)
    {
        List<string> lines = new() { Author.Header }; // Додаємо заголовок при збереженні

        foreach (Author author in authors)
        {
            lines.Add(author.Serialize());
        }

        File.WriteAllLines(this.filePath, lines);
    }
}