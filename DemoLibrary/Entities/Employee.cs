// <copyright file="User.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text;
using DemoLibrary.Exceptions;
using DemoLibrary.Utils;

namespace DemoLibrary.Entities;

public struct Employee
{
    /// <summary>
    ///     Перелік посад у бібліотеці.
    /// </summary>
    public enum PositionType
    {
        Librarian, // Бібліотекар
        Archivist, // Архіваріус
        Administrator, // Адміністратор
        Cataloguer, // Каталогізатор
        ResearchAssistant // Науковий співробітник
    }

    private readonly ValidationHandler errorHandler = new();

    private int? age;
    private string name;
    private string password;
    private string phone;
    private PositionType position;
    private string surname;

    /// <summary>
    ///     Заголовок для CSV, що містить імена всіх властивостей класу Employee.
    /// </summary>
    public static string Header =>
        $"{nameof(Phone)}{CsvUtil.Separator}{nameof(Name)}{CsvUtil.Separator}{nameof(Surname)}{CsvUtil.Separator}{nameof(Position)}{CsvUtil.Separator}{nameof(Password)}{CsvUtil.Separator}{nameof(Age)}";

    public Employee(
        string phone,
        string name,
        string surname,
        PositionType position,
        string password,
        int? age = null)
    {
        this.Phone = phone;
        this.Name = name;
        this.Surname = surname;
        this.Position = position;
        this.Password = password;
        this.Age = age;

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
            if (!PatternValidator.IsMatch(value, "\\d{10}"))
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

    public string Surname
    {
        get => this.surname;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                this.errorHandler.AddError(nameof(this.Surname), "Surname cannot be empty.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Surname));
            }

            this.surname = value;
        }
    }

    public PositionType Position
    {
        get => this.position;
        set
        {
            if (!Enum.IsDefined(typeof(PositionType), value))
            {
                this.errorHandler.AddError(nameof(this.Position), "Invalid position.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Position));
            }

            this.position = value;
        }
    }

    public string Password
    {
        get => this.password;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 6)
            {
                this.errorHandler.AddError(nameof(this.Password), "Password must be at least 6 characters long.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Password));
            }

            this.password = value;
        }
    }

    public int? Age
    {
        get => this.age;
        set
        {
            if (value is < 18 or > 65)
            {
                this.errorHandler.AddError(nameof(this.Age), "Age must be between 18 and 65.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Age));
            }

            this.age = value;
        }
    }

    // Метод для серіалізації в CSV рядок
    public string Serialize()
    {
        StringBuilder sb = new();
        sb.Append(CsvUtil.Escape(this.Phone)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Name)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Surname)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Position.ToString())).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Password));
        return sb.ToString();
    }

    // Метод для десеріалізації з CSV рядка
    public static Employee Deserialize(string csvLine)
    {
        string[] fields = CsvUtil.ParseCsvLine(csvLine);
        if (fields.Length != 5)
        {
            throw new FormatException("Each CSV row must have exactly 5 fields.");
        }

        return new Employee(
            fields[0],
            fields[1],
            fields[2],
            Enum.Parse<PositionType>(fields[3]),
            fields[4]);
    }
}