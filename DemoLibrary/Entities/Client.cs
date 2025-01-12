// <copyright file="Client.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text;
using DemoLibrary.Exceptions;
using DemoLibrary.Utils;

namespace DemoLibrary.Entities;

public struct Client
{
    private readonly ValidationHandler errorHandler = new();
    private string phone;
    private string password;
    private string? name;
    private string? address;
    private DateTime? registrationDate;

    /// <summary>
    ///     Заголовок для CSV, що містить імена всіх властивостей класу Client.
    /// </summary>
    public static string Header =>
        $"{nameof(Phone)}{CsvUtil.Separator}{nameof(Password)}{CsvUtil.Separator}{nameof(Name)}{CsvUtil.Separator}{nameof(Address)}{CsvUtil.Separator}{nameof(RegistrationDate)}";

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

    public string? Address
    {
        get => this.address;
        set
        {
            if (value != null && value.Length < 5)
            {
                this.errorHandler.AddError(
                    nameof(this.Address),
                    "Address must be at least 5 characters long if provided.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Address));
            }

            this.address = value;
        }
    }

    public DateTime? RegistrationDate
    {
        get => this.registrationDate;
        set
        {
            if (value != null && value > DateTime.Now)
            {
                this.errorHandler.AddError(nameof(this.RegistrationDate), "Registration date cannot be in the future.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.RegistrationDate));
            }

            this.registrationDate = value;
        }
    }

    // Метод для серіалізації в CSV рядок
    public string Serialize()
    {
        StringBuilder sb = new();
        sb.Append(CsvUtil.Escape(this.Phone)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Password)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Name ?? string.Empty)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Address ?? string.Empty)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.RegistrationDate?.ToString("yyyy-MM-dd") ?? string.Empty));
        return sb.ToString();
    }

    // Метод для десеріалізації з CSV рядка
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
}