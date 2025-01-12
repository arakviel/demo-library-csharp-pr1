// <copyright file="Book.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text;
using DemoLibrary.Exceptions;
using DemoLibrary.Repositories;
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
    ///     Заголовок для CSV, що містить імена всіх властивостей книги.
    /// </summary>
    public static string Header =>
        $"{nameof(ISBN)}{CsvUtil.Separator}{nameof(Title)}{CsvUtil.Separator}{nameof(Author)}{CsvUtil.Separator}{nameof(Description)}{CsvUtil.Separator}{nameof(Pages)}{CsvUtil.Separator}{nameof(PublicationDate)}";

    public Book(
        string isbn,
        string title,
        Author author,
        string? description = null,
        int? pages = null,
        DateTime? publicationDate = null)
    {
        this.ISBN = isbn;
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

    public string ISBN
    {
        get => this.isbn;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !PatternValidator.IsMatch(value, "###-#-##-######-#"))
            {
                this.errorHandler.AddError(nameof(this.ISBN), "ISBN must follow the format ###-#-##-######-#.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.ISBN));
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

    public string? Description { get; set; }

    public int? Pages
    {
        get => this.pages;
        set
        {
            if (value < 1)
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
            if (value > DateTime.Now)
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

    // Метод для серіалізації в CSV рядок
    public string Serialize()
    {
        StringBuilder sb = new();
        sb.Append(CsvUtil.Escape(this.ISBN)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Title)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Author.Name)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Description ?? string.Empty)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Pages?.ToString() ?? string.Empty)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.PublicationDate?.ToString("yyyy-MM-dd") ?? string.Empty));
        return sb.ToString();
    }

    // Метод для десеріалізації з CSV рядка
    public static Book Deserialize(string csvLine)
    {
        string[] fields = CsvUtil.ParseCsvLine(csvLine);
        if (fields.Length != 6)
        {
            throw new FormatException("Each CSV row must have exactly 6 fields.");
        }

        // Замість створення нового об'єкта Author вручну, шукаємо автора через репозиторій
        AuthorRepository authorRepository = new();
        Author author = authorRepository.Get(fields[2]);

        return new Book(
            fields[0], // ISBN
            fields[1], // Title
            author, // Author
            fields[3] == string.Empty ? null : fields[3], // Description
            string.IsNullOrEmpty(fields[4]) ? null : int.Parse(fields[4]), // Pages
            string.IsNullOrEmpty(fields[5]) ? null : DateTime.Parse(fields[5]) // PublicationDate
        );
    }
}