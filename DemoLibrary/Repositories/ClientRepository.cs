using DemoLibrary.Entities;

namespace DemoLibrary.Repositories;

/// <summary>
///     Репозиторій для роботи з об'єктами Client в файлах CSV.
/// </summary>
public class ClientRepository
{
    private readonly string filePath = Path.Combine("Data", "clients.csv");

    /// <summary>
    ///     Отримує клієнта за номером телефону.
    /// </summary>
    /// <param name="phone">Номер телефону клієнта.</param>
    /// <returns>Клієнт з відповідним номером телефону.</returns>
    public Client Get(string phone)
    {
        List<Client> clients = this.GetAll();
        foreach (Client client in clients)
        {
            if (client.Phone == phone)
            {
                return client;
            }
        }

        throw new KeyNotFoundException("Client with the specified phone not found.");
    }

    /// <summary>
    ///     Отримує усіх клієнтів.
    /// </summary>
    /// <returns>Список усіх клієнтів.</returns>
    public List<Client> GetAll()
    {
        List<Client> clients = new();

        if (!File.Exists(this.filePath))
        {
            return clients;
        }

        string[] lines = File.ReadAllLines(this.filePath);

        // Перевіряємо, чи є заголовки у файлі
        if (lines.Length > 0 && !lines[0].Equals(Client.Header))
        {
            throw new InvalidOperationException("CSV file format is incorrect, missing headers.");
        }

        foreach (string line in lines.Skip(1)) // Пропускаємо перший рядок (заголовок)
        {
            try
            {
                Client client = Client.Deserialize(line);
                clients.Add(client);
            }
            catch (Exception)
            {
                // Пропускаємо некоректні записи
            }
        }

        return clients;
    }

    /// <summary>
    ///     Отримує усіх клієнтів, що зареєстровані після певної дати.
    /// </summary>
    /// <param name="date">Дата реєстрації.</param>
    /// <returns>Список клієнтів, що зареєстровані після вказаної дати.</returns>
    public List<Client> GetAllByRegistrationDate(DateTime date)
    {
        List<Client> clients = this.GetAll();
        List<Client> result = new();

        foreach (Client client in clients)
        {
            if (client.RegistrationDate.HasValue && client.RegistrationDate.Value > date)
            {
                result.Add(client);
            }
        }

        return result;
    }

    /// <summary>
    ///     Додає нового клієнта в CSV файл.
    /// </summary>
    /// <param name="client">Клієнт для додавання.</param>
    public void Add(Client client)
    {
        List<Client> clients = this.GetAll();
        foreach (Client c in clients)
        {
            if (c.Phone == client.Phone)
            {
                throw new InvalidOperationException("Client with the same phone already exists.");
            }
        }

        // Перевіряємо, чи є заголовки у файлі, якщо їх немає — додаємо
        if (new FileInfo(this.filePath).Length == 0)
        {
            File.AppendAllLines(this.filePath, new[] { Client.Header });
        }

        string line = client.Serialize();
        File.AppendAllLines(this.filePath, new[] { line });
    }

    /// <summary>
    ///     Оновлює інформацію про клієнта в CSV файлі.
    /// </summary>
    /// <param name="client">Оновлений клієнт.</param>
    public void Update(Client client)
    {
        List<Client> clients = this.GetAll();
        bool found = false;

        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].Phone == client.Phone)
            {
                clients[i] = client;
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Client not found.");
        }

        this.SaveAll(clients);
    }

    /// <summary>
    ///     Видаляє клієнта з CSV файлу.
    /// </summary>
    /// <param name="phone">Номер телефону клієнта, якого потрібно видалити.</param>
    public void Delete(string phone)
    {
        List<Client> clients = this.GetAll();
        bool found = false;

        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].Phone == phone)
            {
                clients.RemoveAt(i);
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new KeyNotFoundException("Client not found.");
        }

        this.SaveAll(clients);
    }

    /// <summary>
    ///     Зберігає усіх клієнтів в CSV файл.
    /// </summary>
    /// <param name="clients">Список клієнтів для збереження.</param>
    private void SaveAll(List<Client> clients)
    {
        List<string> lines = new() { Client.Header }; // Додаємо заголовок при збереженні

        foreach (Client client in clients)
        {
            lines.Add(client.Serialize());
        }

        File.WriteAllLines(this.filePath, lines);
    }
}