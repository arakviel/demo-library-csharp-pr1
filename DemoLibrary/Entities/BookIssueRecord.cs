// <copyright file="BookIssueRecord.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text;
using DemoLibrary.Repositories;

namespace DemoLibrary.Entities;

public struct BookIssueRecord
{
    public Book Book { get; }

    public Client Client { get; }

    public Employee Employee { get; }

    public DateTime IssueDate { get; }

    public DateTime? ReturnDate { get; }

    public BookIssueRecord(Book book, Client client, Employee employee, DateTime issueDate, DateTime? returnDate = null)
    {
        if (book.Equals(default(Book)))
        {
            throw new ArgumentException("Book cannot be uninitialized.", nameof(book));
        }

        if (client.Equals(default(Client)))
        {
            throw new ArgumentException("Client cannot be uninitialized.", nameof(client));
        }

        if (employee.Equals(default(Employee)))
        {
            throw new ArgumentException("Employee cannot be uninitialized.", nameof(employee));
        }

        if (issueDate > DateTime.Now)
        {
            if (issueDate > DateTime.Now)
            {
                throw new ArgumentException("Issue date cannot be in the future.", nameof(issueDate));
            }
        }

        this.Book = book;
        this.Client = client;
        this.Employee = employee;
        this.IssueDate = issueDate;
        this.ReturnDate = returnDate;
    }

    public bool IsReturned => this.ReturnDate.HasValue;

    // Метод для серіалізації в CSV рядок
    public string Serialize()
    {
        StringBuilder sb = new();
        sb.Append(CsvUtil.Escape(this.Book.ISBN)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Client.Phone)).Append(CsvUtil.Separator)
            .Append(CsvUtil.Escape(this.Employee.Phone)).Append(CsvUtil.Separator)
            .Append(this.IssueDate.ToString("yyyy-MM-dd")).Append(CsvUtil.Separator)
            .Append(this.ReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty);
        return sb.ToString();
    }

    // Метод для десеріалізації з CSV рядка
    public static BookIssueRecord Deserialize(string csvLine)
    {
        string[] fields = CsvUtil.ParseCsvLine(csvLine);
        if (fields.Length != 5)
        {
            throw new FormatException("Each CSV row must have exactly 5 fields.");
        }

        BookRepository bookRepository = new();
        ClientRepository clientRepository = new();
        EmployeeRepository employeeRepository = new();

        Book book = bookRepository.Get(fields[0]);
        Client client = clientRepository.Get(fields[1]);
        Employee employee = employeeRepository.Get(fields[2]);

        return new BookIssueRecord(
            book,
            client,
            employee,
            DateTime.Parse(fields[3]),
            string.IsNullOrEmpty(fields[4]) ? null : DateTime.Parse(fields[4]));
    }
}