using System.IO;
using System.Collections.Generic;
using System;

class Program
{
    static void Main(string[] args)
    {
        var terminal = new Terminal();
        using (StreamReader reader = File.OpenText(args[0]))
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            if (null == line)
                continue;

            foreach (var c in line)
            {
                terminal.KeyPress(c.ToString());
            }
        }

        terminal.PrintTerminal();
    }
}

public class Terminal
{
    string [,] term = new string[10,10];
    string previousInput = string.Empty;
    Tuple<int,int> currentPosition = new Tuple<int, int>(0, 0);
    Mode currentMode = Mode.Overwrite;

    enum Mode
    {
        Insert,
        Overwrite
    }

    public Terminal()
    {
        InitializeTerm();
    }

    private void InitializeTerm()
    {
        for (int i = 0; i <= 9; i++)
        {
            for (int j = 0; j <= 9; j++)
            {
                term[i, j] = " ";
            }
        }
    }

    public void KeyPress(string input)
    {
        try
        {
            if (previousInput == "^")
            {
                ProcessControlSequence(input);
            }
            else 
            {
                int previousDigit;
                if (int.TryParse(previousInput, out previousDigit))
                {
                    int digit;
                    if (int.TryParse(input, out digit))
                    {
                        if (digit >= 0 && digit <= 9 && previousDigit >= 0 && previousDigit <= 9)
                        {
                            currentPosition = new Tuple<int, int>(previousDigit, digit);
                        }
                    }
                }
                else
                {
                    if (previousInput != "^" && input != "^") WriteCharacter(input);
                }
            }
        }
        finally
        {
            if ((input == "^" || previousInput == "^") && !(input == "^" && previousInput == "^"))
            {
                previousInput = input;
            }
            else
            {
                previousInput = string.Empty;
            }
        }
    }

    private void WriteCharacter(string input)
    {
        if (currentMode == Mode.Overwrite)
        {
            term[currentPosition.Item1, currentPosition.Item2] = input;
        }
        else
        {
            int row = currentPosition.Item1;
            //Shift everything over to the right
            for (int i = 9; i >= currentPosition.Item2; i--)
            {
                if (i > 0) term[row, i] = term[row, i--];
            }
            term[row, currentPosition.Item2] = input;
        }
        
        MoveCursor(currentPosition.Item1, currentPosition.Item2 + 1);
    }

    private void ProcessControlSequence(string input)
    {
        int row = currentPosition.Item1;
        int col = currentPosition.Item2;

        switch (input)
        {
            case "c": //Clear
                InitializeTerm();
                break;
            case "h": //Home
                MoveCursor(0, 0);
                break;
            case "b": //Beginning of row
                MoveCursor(currentPosition.Item1, 0);
                break;
            case "d": //Down 1 row
                MoveCursor(row + 1, col);
                break;
            case "u": //Up 1 row
                MoveCursor(row - 1, col);
                break;
            case "l": //Left 1 col
                MoveCursor(row, col - 1);
                break;
            case "r": //Right 1 col
                MoveCursor(row, col + 1);
                break;
            case "e": //Erase characters to the right
                for (int i = col; i <= 9 ; i++)
                {
                    term[row, i] = string.Empty;
                }
                break;
            case "i":
                currentMode = Mode.Insert;
                break;
            case "o":
                currentMode = Mode.Overwrite;
                break;
            case "^":
                WriteCharacter(input);
                break;
            default:
                break;
        }
    }

    private void MoveCursor(int row, int col)
    {
        int newRow = row;
        int newCol = col;

        if (newRow > 9) newRow = 9;
        if (newRow < 0) newRow = 0;
        if (newCol > 9) newCol = 9;
        if (newCol < 0) newCol = 0;

        currentPosition = new Tuple<int, int>(newRow, newCol);
    }

    public void PrintTerminal()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Console.Write(term[i, j]);
            }
            Console.WriteLine();
        }
    }
}