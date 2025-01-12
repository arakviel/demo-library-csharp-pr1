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
    ///     Шлях до CSV файлу, де зберігаються дані працівників.
    /// </summary>
    private static readonly string FilePath = Path.Combine("Data", "employees.csv");

    /// <summary>
    ///     Заголовок для CSV файлу, що містить імена всіх властивостей працівника.
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

    public PositionType Position { get; set; }

    public string Password
    {
        get => this.password;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                this.errorHandler.AddError(nameof(this.Password), "Password cannot be empty.");
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
            if (value < 18 || value > 100)
            {
                this.errorHandler.AddError(nameof(this.Age), "Age must be between 18 and 100.");
            }
            else
            {
                this.errorHandler.RemoveError(nameof(this.Age));
            }

            this.age = value;
        }
    }

    /// <summary>
    ///     Серіалізує поточний об'єкт працівника в CSV рядок.
    /// </summary>
    public string Serialize()
    {
        return $"{CsvUtil.Escape(this.Phone)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Name)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Surname)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Position.ToString())}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Password)}{CsvUtil.Separator}" +
               $"{CsvUtil.Escape(this.Age?.ToString() ?? string.Empty)}";
    }

    /// <summary>
    ///     Десеріалізує рядок CSV в об'єкт працівника.
    /// </summary>
    public static Employee Deserialize(string csvLine)
    {
        string[] fields = CsvUtil.ParseCsvLine(csvLine);
        if (fields.Length != 6)
        {
            throw new FormatException("Each CSV row must have exactly 6 fields.");
        }

        return new Employee(
            fields[0], // Phone
            fields[1], // Name
            fields[2], // Surname
            Enum.Parse<PositionType>(fields[3], true), // Position
            fields[4], // Password
            string.IsNullOrEmpty(fields[5]) ? null : int.Parse(fields[5]) // Age
        );
    }

    /// <summary>
    ///     Зберігає поточний екземпляр працівника в файл, або оновлює існуючий запис, якщо працівник вже існує.
    /// </summary>
    public void Save()
    {
        List<Employee> employees = GetAll();

        bool exists = false;
        foreach (Employee employee in employees)
        {
            if (employee.Phone == this.Phone)
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
    ///     Додає нового працівника до CSV файлу.
    /// </summary>
    private void Add()
    {
        List<Employee> employees = GetAll();

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
    ///     Оновлює існуючого працівника в CSV файлі.
    /// </summary>
    private void Update()
    {
        List<Employee> employees = GetAll();
        bool found = false;

        // Шукаємо працівника в списку та оновлюємо його дані
        for (int i = 0; i < employees.Count; i++)
        {
            if (employees[i].Phone == this.Phone)
            {
                employees[i] = this;
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Employee not found.");
        }

        SaveAll(employees);
    }

    /// <summary>
    ///     Отримує всіх працівників з CSV файлу.
    /// </summary>
    public static List<Employee> GetAll()
    {
        List<Employee> employees = new();

        // Якщо файл не існує, повертаємо порожній список
        if (!File.Exists(FilePath))
        {
            return employees;
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
                Employee employee = Deserialize(line);
                employees.Add(employee);
            }
            catch (Exception)
            {
                // Пропускаємо некоректні записи
            }
        }

        return employees;
    }

    /// <summary>
    ///     Видаляє працівника з CSV файлу за його номером телефону.
    /// </summary>
    public static void Delete(string phone)
    {
        List<Employee> employees = GetAll();
        bool found = false;

        // Шукаємо працівника для видалення
        for (int i = 0; i < employees.Count; i++)
        {
            if (employees[i].Phone == phone)
            {
                employees.RemoveAt(i);
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Employee not found.");
        }

        SaveAll(employees);
    }

    /// <summary>
    ///     Зберігає всіх працівників до CSV файлу.
    /// </summary>
    private static void SaveAll(List<Employee> employees)
    {
        List<string> lines = new() { Header }; // Додаємо заголовок при збереженні

        foreach (Employee employee in employees)
        {
            lines.Add(employee.Serialize());
        }

        File.WriteAllLines(FilePath, lines);
    }

    /// <summary>
    ///     Отримує співробітника за номером телефону.
    /// </summary>
    /// <param name="phone">Номер телефону співробітника.</param>
    /// <returns>Співробітник з відповідним номером телефону.</returns>
    public static Employee Get(string phone)
    {
        List<Employee> employees = GetAll();
        foreach (Employee emp in employees)
        {
            if (emp.Phone == phone)
            {
                return emp;
            }
        }

        throw new KeyNotFoundException("Employee with the specified phone not found.");
    }

    /// <summary>
    ///     Отримує усіх співробітників, що мають певну посаду.
    /// </summary>
    /// <param name="position">Посада співробітника.</param>
    /// <returns>Список співробітників з вказаною посадою.</returns>
    public static List<Employee> GetAllByPosition(PositionType position)
    {
        List<Employee> employees = GetAll();
        List<Employee> result = new();

        foreach (Employee emp in employees)
        {
            if (emp.Position == position)
            {
                result.Add(emp);
            }
        }

        return result;
    }
}