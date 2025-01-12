// <copyright file="CsvUtil.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text;

namespace DemoLibrary.Entities;

public static class CsvUtil
{
    public static char Separator => ',';

    public static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(",") || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    public static string[] ParseCsvLine(string line)
    {
        List<string> values = new();
        StringBuilder currentField = new();
        bool insideQuotes = false;

        foreach (char ch in line)
        {
            if (ch == '"' && insideQuotes)
            {
                if (currentField.Length > 0 && currentField[^1] == '"')
                {
                    currentField.Remove(currentField.Length - 1, 1); // Заміна подвоєних лапок на одну
                    currentField.Append(ch);
                }
                else
                {
                    insideQuotes = false;
                }
            }
            else if (ch == '"' && !insideQuotes)
            {
                insideQuotes = true;
            }
            else if (ch == ',' && !insideQuotes)
            {
                values.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(ch);
            }
        }

        values.Add(currentField.ToString());
        return values.ToArray();
    }
}