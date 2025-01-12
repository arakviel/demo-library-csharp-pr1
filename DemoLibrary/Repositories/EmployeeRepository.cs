using DemoLibrary.Entities;

namespace DemoLibrary.Repositories;

/// <summary>
///     Репозиторій для роботи з об'єктами Employee в файлах CSV.
/// </summary>
public class EmployeeRepository
{
    private readonly string filePath = Path.Combine("Data", "employees.csv");

    /// <summary>
    ///     Отримує співробітника за номером телефону.
    /// </summary>
    /// <param name="phone">Номер телефону співробітника.</param>
    /// <returns>Співробітник з відповідним номером телефону.</returns>
    public Employee Get(string phone)
    {
        List<Employee> employees = this.GetAll();
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
    ///     Отримує усіх співробітників.
    /// </summary>
    /// <returns>Список усіх співробітників.</returns>
    public List<Employee> GetAll()
    {
        List<Employee> employees = new();

        if (!File.Exists(this.filePath))
        {
            return employees;
        }

        string[] lines = File.ReadAllLines(this.filePath);

        // Перевіряємо, чи є заголовки у файлі
        if (lines.Length > 0 && !lines[0].Equals(Employee.Header))
        {
            throw new InvalidOperationException("CSV file format is incorrect, missing headers.");
        }

        foreach (string line in lines.Skip(1)) // Пропускаємо перший рядок (заголовок)
        {
            try
            {
                Employee employee = Employee.Deserialize(line);
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
    ///     Отримує усіх співробітників, що мають певну посаду.
    /// </summary>
    /// <param name="position">Посада співробітника.</param>
    /// <returns>Список співробітників з вказаною посадою.</returns>
    public List<Employee> GetAllByPosition(Employee.PositionType position)
    {
        List<Employee> employees = this.GetAll();
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

    /// <summary>
    ///     Додає нового співробітника в CSV файл.
    /// </summary>
    /// <param name="employee">Співробітник для додавання.</param>
    public void Add(Employee employee)
    {
        List<Employee> employees = this.GetAll();
        foreach (Employee emp in employees)
        {
            if (emp.Phone == employee.Phone)
            {
                throw new InvalidOperationException("Employee with the same phone already exists.");
            }
        }

        // Перевіряємо, чи є заголовки у файлі, якщо їх немає — додаємо
        if (new FileInfo(this.filePath).Length == 0)
        {
            File.AppendAllLines(this.filePath, new[] { Employee.Header });
        }

        string line = employee.Serialize();
        File.AppendAllLines(this.filePath, new[] { line });
    }

    /// <summary>
    ///     Оновлює інформацію про співробітника в CSV файлі.
    /// </summary>
    /// <param name="employee">Оновлений співробітник.</param>
    public void Update(Employee employee)
    {
        List<Employee> employees = this.GetAll();
        bool found = false;

        for (int i = 0; i < employees.Count; i++)
        {
            if (employees[i].Phone == employee.Phone)
            {
                employees[i] = employee;
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Employee not found.");
        }

        this.SaveAll(employees);
    }

    /// <summary>
    ///     Видаляє співробітника з CSV файлу.
    /// </summary>
    /// <param name="phone">Номер телефону співробітника, якого потрібно видалити.</param>
    public void Delete(string phone)
    {
        List<Employee> employees = this.GetAll();
        bool found = false;

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

        this.SaveAll(employees);
    }

    /// <summary>
    ///     Зберігає усіх співробітників в CSV файл.
    /// </summary>
    /// <param name="employees">Список співробітників для збереження.</param>
    private void SaveAll(List<Employee> employees)
    {
        List<string> lines = new() { Employee.Header }; // Додаємо заголовок при збереженні

        foreach (Employee emp in employees)
        {
            lines.Add(emp.Serialize());
        }

        File.WriteAllLines(this.filePath, lines);
    }
}