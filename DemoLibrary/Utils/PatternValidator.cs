// <copyright file="PatternValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DemoLibrary.Utils;

public static class PatternValidator
{
    public static bool IsMatch(string input, string pattern)
    {
        if (input.Length != pattern.Length)
        {
            return false;
        }

        for (int i = 0; i < input.Length; i++)
        {
            char inputChar = input[i];
            char patternChar = pattern[i];

            if (patternChar == '#')
            {
                // Очікуємо цифру
                //if (!char.IsDigit(inputChar))
                if (inputChar < 48 || inputChar > 57)
                {
                    return false;
                }
            }
            else if (patternChar == '@')
            {
                // Очікуємо букву
                //if (!(inputChar < 48 || inputChar > 57))
                if (!char.IsLetter(inputChar))
                {
                    return false;
                }
            }
            else if (patternChar == '_')
            {
                //if (!char.IsWhiteSpace(inputChar))
                if (inputChar != 20)
                {
                    return false;
                }
            }
            else if (patternChar == '?')
            {
            }
            else
            {
                // Очікуємо точний збіг символу
                if (inputChar != patternChar)
                {
                    return false;
                }
            }
        }

        return true;
    }
}