// <copyright file="ValidationHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DemoLibrary.Utils;

/// <summary>
///     Клас для обробки помилок валідації.
///     Дозволяє додавати, видаляти та перевіряти наявність помилок у різних полях.
/// </summary>
public class ValidationHandler
{
    /// <summary>
    ///     Словник, що містить помилки валідації для кожного поля.
    ///     Ключ - назва поля, значення - список повідомлень про помилки.
    /// </summary>
    private readonly Dictionary<string, List<string>> errors = new();

    /// <summary>
    ///     Отримує словник помилок, доступний лише для читання.
    /// </summary>
    public IReadOnlyDictionary<string, List<string>> Errors => this.errors;

    /// <summary>
    ///     Перевіряє, чи є помилки у словнику.
    /// </summary>
    public bool HasErrors => this.errors.Count > 0;

    /// <summary>
    ///     Додає нову помилку для вказаного поля.
    ///     Якщо поле ще не має помилок, створюється новий запис у словнику.
    ///     Якщо помилка вже існує, вона не буде додана повторно.
    /// </summary>
    /// <param name="fieldName">Назва поля, для якого додається помилка.</param>
    /// <param name="error">Повідомлення про помилку.</param>
    public void AddError(string fieldName, string error)
    {
        if (!this.errors.ContainsKey(fieldName))
        {
            this.errors[fieldName] = new List<string>();
        }

        if (!this.errors[fieldName].Contains(error))
        {
            this.errors[fieldName].Add(error);
        }
    }

    /// <summary>
    ///     Видаляє всі помилки для вказаного поля.
    ///     Якщо поле відсутнє у словнику, метод нічого не робить.
    /// </summary>
    /// <param name="fieldName">Назва поля, помилки якого потрібно видалити.</param>
    public void RemoveError(string fieldName)
    {
        if (this.errors.ContainsKey(fieldName))
        {
            this.errors.Remove(fieldName);
        }
    }
}