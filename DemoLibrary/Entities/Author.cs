// <copyright file="Author.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text;
using DemoLibrary.Exceptions;
using DemoLibrary.Utils;

namespace DemoLibrary.Entities;

public struct Author
{
    private readonly ValidationHandler errorHandler = new();
    private DateTime? birthDate;
    private DateTime? deathDate;

    /// <summary>
    ///     Заголовок для CSV, що містить імена всіх властивостей.
    /// </summary>
    public static string Header =>
        $"{nameof(Name)}{CsvUtil.Separator}{nameof(Biography)}{CsvUtil.Separator}{nameof(BirthDate)}{CsvUtil.Separator}{nameof(DeathDate)}";

    private string name;

    public Author(string name, string? biography = null, DateTime? birthDate = null, DateTime? deathDate = null)
    {
        this.Name = name;
        this.Biography = biography;
        this.BirthDate = birthDate;
        this.DeathDate = deathDate;

        if (this.errorHandler.HasErrors)
        {
            throw new ValidationException(this.errorHandler.Errors);
        }
    }

    public string Name
    {
        get => this.name;
        set
        {
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

    public string? Biography { get; set; }

    public DateTime? BirthDate
    {
        get => this.birthDate;
        set
        {
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

    public DateTime? DeathDate
    {
        get => this.deathDate;
        set
        {
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

    // Метод для серіалізації в CSV рядок
    public string Serialize()
    {
        StringBuilder sb = new();
        sb.Append(CsvUtil.Escape(this.Name)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Biography ?? string.Empty)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.BirthDate?.ToString("yyyy-MM-dd") ?? string.Empty)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.DeathDate?.ToString("yyyy-MM-dd") ?? string.Empty));
        return sb.ToString();
    }

    // Метод для десеріалізації з CSV рядка
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
}