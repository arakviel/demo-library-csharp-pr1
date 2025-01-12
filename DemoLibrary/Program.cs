using DemoLibrary.Entities;
using DemoLibrary.Exceptions;

namespace DemoLibrary;

internal class Program
{
    private static void Main()
    {
        //TestAuthors();
        //TestClients();
        //TestEmployees();
        //TestBooks();
    }

    private static void TestClients()
    {
        // Створення 10 клієнтів
        List<Client> clients = new()
        {
            new Client("1234567890", "password123", "John Doe", "123 Main St", DateTime.Now),
            new Client("0987654321", "securepassword", "Jane Smith", "456 Oak St", DateTime.Now),
            new Client("1122334455", "mypassword1", "Alice Johnson", "789 Pine St", DateTime.Now),
            new Client("2233445566", "password2", "Bob Brown", "101 Maple St", DateTime.Now),
            new Client("3344556677", "password1234", "Charlie White", "202 Birch St", DateTime.Now),
            new Client("4455667788", "newpassword1", "David Black", "303 Cedar St", DateTime.Now),
            new Client("5566778899", "password2021", "Eve Green", "404 Elm St", DateTime.Now),
            new Client("6677889900", "strongpassword", "Frank Blue", "505 Redwood St", DateTime.Now),
            new Client("7788990011", "securepass1", "Grace Yellow", "606 Willow St", DateTime.Now),
            new Client("8899001122", "mypassword2", "Hank Purple", "707 Cedar St", DateTime.Now)
        };

        // Збереження кожного клієнта в CSV
        foreach (Client client in clients)
        {
            client.Save();
        }

        // Отримання всіх клієнтів з CSV
        List<Client> allClients = Client.GetAll();

        // Виведення всіх клієнтів
        Console.WriteLine("List of all clients:");
        foreach (Client client in allClients)
        {
            Console.WriteLine($"{client.Name} - {client.Phone}");
        }

        // Оновлення інформації для конкретного клієнта
        Client clientToUpdate = allClients.First();
        clientToUpdate.Name = "Updated Name";
        clientToUpdate.Save();

        // Видалення клієнта за номером телефону
        string phoneToDelete = allClients.Last().Phone;
        Client.Delete(phoneToDelete);

        // Виведення оновленої інформації
        Console.WriteLine("\nUpdated list of clients after deletion:");
        allClients = Client.GetAll();
        foreach (Client client in allClients)
        {
            Console.WriteLine($"{client.Name} - {client.Phone}");
        }
    }

    private static void TestAuthors()
    {
        // Створення авторів
        Author[] authors =
        {
            new(
                "Mark Twain",
                "Author of The Adventures of Tom Sawyer",
                new DateTime(1835, 11, 30),
                new DateTime(1910, 4, 21)),
            new("Jane Austen", "Author of Pride and Prejudice", new DateTime(1775, 12, 16), new DateTime(1817, 7, 18)),
            new(
                "Charles Dickens",
                "Author of A Tale of Two Cities",
                new DateTime(1812, 2, 7),
                new DateTime(1870, 6, 9)),
            new(
                "George Orwell",
                "Author of 1984 and Animal Farm",
                new DateTime(1903, 6, 25),
                new DateTime(1950, 1, 21)),
            new("J.K. Rowling", "Author of the Harry Potter series", new DateTime(1965, 7, 31)),
            new("Hemingway", "Author of The Old Man and the Sea", new DateTime(1899, 7, 21), new DateTime(1961, 7, 2)),
            new("Leo Tolstoy", "Author of War and Peace", new DateTime(1828, 9, 9), new DateTime(1910, 11, 20)),
            new(
                "F. Scott Fitzgerald",
                "Author of The Great Gatsby",
                new DateTime(1896, 9, 24),
                new DateTime(1940, 12, 21)),
            new("Virginia Woolf", "Author of Mrs. Dalloway", new DateTime(1882, 1, 25), new DateTime(1941, 3, 28)),
            new(
                "Agatha Christie",
                "Author of Murder on the Orient Express",
                new DateTime(1890, 9, 15),
                new DateTime(1976, 1, 12))
        };

        // Збереження авторів до файлу
        foreach (Author author in authors)
        {
            author.Save();
        }

        // Отримання всіх авторів
        List<Author> allAuthors = Author.GetAll();

        // Виведення всіх авторів
        Console.WriteLine("All Authors:");
        foreach (Author author in allAuthors)
        {
            Console.WriteLine(
                $"{author.Name} ({author.BirthDate?.ToString("yyyy-MM-dd")} - {author.DeathDate?.ToString("yyyy-MM-dd") ?? "Present"})");
        }

        // Отримання конкретного автора
        try
        {
            Author specificAuthor = Author.Get("Jane Austen");
            Console.WriteLine($"\nFound author: {specificAuthor.Name}, Biography: {specificAuthor.Biography}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        // Оновлення даних автора
        try
        {
            Author authorToUpdate = Author.Get("Mark Twain");
            authorToUpdate.Biography = "Updated biography for Mark Twain.";
            authorToUpdate.Save();
            Console.WriteLine("\nUpdated Mark Twain's biography.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        // Видалення автора
        try
        {
            Author.Delete("Agatha Christie");
            Console.WriteLine("\nDeleted Agatha Christie.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void TestEmployees()
    {
        Console.WriteLine("=== Створення працівників ===");

        List<Employee> employees = new();

        // Створюємо 20 працівників
        for (int i = 1; i <= 20; i++)
        {
            try
            {
                Employee employee = new(
                    $"0981234{i:D3}",
                    $"Name{i}",
                    $"Surname{i}",
                    (Employee.PositionType)(i % 5),
                    $"password{i}",
                    i + 20);
                employees.Add(employee);
                Console.WriteLine(
                    $"Працівник {employee.Name} {employee.Surname} (Телефон: {employee.Phone}) успішно створений.");
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Помилка при створенні працівника: {ex.Message}");
            }
        }

        Console.WriteLine("\n=== Збереження працівників у файл ===");

        foreach (Employee employee in employees)
        {
            try
            {
                employee.Save();
                Console.WriteLine($"Працівник {employee.Name} {employee.Surname} збережений.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при збереженні: {ex.Message}");
            }
        }

        Console.WriteLine("\n=== Отримання всіх працівників ===");

        List<Employee> allEmployees = Employee.GetAll();
        Console.WriteLine($"Отримано {allEmployees.Count} працівників.");

        Console.WriteLine("\n=== Оновлення інформації про працівника ===");

        try
        {
            Employee employeeToUpdate = allEmployees[0];
            employeeToUpdate.Name = "UpdatedName";
            employeeToUpdate.Save();
            Console.WriteLine($"Працівник {employeeToUpdate.Phone} оновлений.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка при оновленні: {ex.Message}");
        }

        Console.WriteLine("\n=== Видалення працівника ===");

        try
        {
            Employee.Delete("0981234001");
            Console.WriteLine("Працівник з телефоном 0981234001 видалений.");
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"Помилка при видаленні: {ex.Message}");
        }

        Console.WriteLine("\n=== Отримання працівника за телефоном ===");

        try
        {
            Employee foundEmployee = Employee.Get("0981234002");
            Console.WriteLine(
                $"Знайдений працівник: {foundEmployee.Name} {foundEmployee.Surname}, {foundEmployee.Phone}");
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"Помилка при пошуку працівника: {ex.Message}");
        }

        Console.WriteLine("\n=== Отримання всіх працівників з певною посадою ===");

        List<Employee> librarians = Employee.GetAllByPosition(Employee.PositionType.Librarian);
        Console.WriteLine($"Знайдено {librarians.Count} бібліотекарів.");
        foreach (Employee librarian in librarians)
        {
            Console.WriteLine($"Бібліотекар: {librarian.Name} {librarian.Surname}");
        }
    }

    public static void TestBooks()
    {
        // Додаємо 20 книг
        List<Author> authors = new()
        {
            new Author(
                "Mark Twain",
                "Updated biography for Mark Twain.",
                new DateTime(1835, 11, 30),
                new DateTime(1910, 4, 21)),
            new Author(
                "Jane Austen",
                "Author of Pride and Prejudice",
                new DateTime(1775, 12, 16),
                new DateTime(1817, 7, 18)),
            new Author(
                "Charles Dickens",
                "Author of A Tale of Two Cities",
                new DateTime(1812, 2, 7),
                new DateTime(1870, 6, 9)),
            new Author(
                "George Orwell",
                "Author of 1984 and Animal Farm",
                new DateTime(1903, 6, 25),
                new DateTime(1950, 1, 21)),
            new Author("J.K. Rowling", "Author of the Harry Potter series", new DateTime(1965, 7, 31)),
            new Author(
                "Hemingway",
                "Author of The Old Man and the Sea",
                new DateTime(1899, 7, 21),
                new DateTime(1961, 7, 2)),
            new Author("Leo Tolstoy", "Author of War and Peace", new DateTime(1828, 9, 9), new DateTime(1910, 11, 20)),
            new Author(
                "F. Scott Fitzgerald",
                "Author of The Great Gatsby",
                new DateTime(1896, 9, 24),
                new DateTime(1940, 12, 21)),
            new Author(
                "Virginia Woolf",
                "Author of Mrs. Dalloway",
                new DateTime(1882, 1, 25),
                new DateTime(1941, 3, 28))
        };

        List<Book> books = new()
        {
            // ###-#-##-######-#
            new Book(
                "978-3-16-148410-0",
                "The Adventures of Huckleberry Finn",
                authors[0],
                "A story about a boy and his journey.",
                366,
                new DateTime(1884, 1, 1)),
            new Book(
                "978-1-56-619909-4",
                "Pride and Prejudice",
                authors[1],
                "A classic love story.",
                432,
                new DateTime(1813, 1, 28)),
            new Book(
                "978-0-14-143960-0",
                "A Tale of Two Cities",
                authors[2],
                "A novel set during the French Revolution.",
                489,
                new DateTime(1859, 4, 30)),
            new Book(
                "978-0-42-228423-4",
                "1984",
                authors[3],
                "A dystopian social science fiction novel.",
                328,
                new DateTime(1949, 6, 8)),
            new Book(
                "978-0-54-501022-1",
                "Harry Potter and the Sorcerer's Stone",
                authors[4],
                "The first book in the Harry Potter series.",
                309,
                new DateTime(1997, 6, 26)),
            new Book(
                "978-0-74-327356-5",
                "The Old Man and the Sea",
                authors[5],
                "A short novel about an old man’s struggle with a giant fish.",
                127,
                new DateTime(1952, 9, 1)),
            new Book(
                "978-0-14-303999-0",
                "War and Peace",
                authors[6],
                "A historical novel about the Napoleonic Wars.",
                1225,
                new DateTime(1869, 1, 1)),
            new Book(
                "978-0-74-327356-5",
                "The Great Gatsby",
                authors[7],
                "A novel about the American Dream.",
                180,
                new DateTime(1925, 4, 10)),
            new Book(
                "978-0-15-101026-4",
                "Mrs. Dalloway",
                authors[8],
                "A novel about a woman’s inner thoughts and perceptions.",
                194,
                new DateTime(1925, 5, 14))
        };

        // Зберігаємо всі книги
        foreach (Book book in books)
        {
            book.Save();
        }

        Console.WriteLine("All books saved to CSV.");

        // Оновлюємо книгу
        Book bookToUpdate = Book.Get("978-3-16-148410-0");
        bookToUpdate.Title = "The Adventures of Tom Sawyer";
        bookToUpdate.Save();
        Console.WriteLine($"Book with ISBN {bookToUpdate.Isbn} updated.");

        // Отримуємо всі книги
        List<Book> allBooks = Book.GetAll();
        Console.WriteLine("All books from the CSV:");
        foreach (Book book in allBooks)
        {
            Console.WriteLine($"{book.Title} by {book.Author.Name}");
        }

        // Видаляємо книгу
        Book.Delete("978-1-56619-909-4");
        Console.WriteLine("Book with ISBN 978-1-56619-909-4 deleted.");

        // Спробуємо отримати видалену книгу
        try
        {
            Book.Get("978-1-56-619909-4");
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine(ex.Message); // Виводиться повідомлення про те, що книга не знайдена
        }
    }
}