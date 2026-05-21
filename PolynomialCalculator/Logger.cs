using System;
using System.IO;
using System.Text;

namespace PolynomialCalculator;

/// <summary>
/// Протокол работы калькулятора — пишет события в текстовый файл.
/// </summary>
public sealed class Logger
{
    private readonly object _lock = new();

    public string FilePath { get; }

    public Logger(string filePath)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath,
                "=== Протокол работы калькулятора многочленов ===" + Environment.NewLine,
                Encoding.UTF8);
        }
    }

    public void Log(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
        lock (_lock)
        {
            File.AppendAllText(FilePath, line, Encoding.UTF8);
        }
    }

    public void LogError(string message) => Log("ОШИБКА: " + message);

    public void LogSession(string title)
    {
        Log(new string('-', 60));
        Log(title);
    }

    public string ReadAll()
    {
        lock (_lock)
        {
            return File.Exists(FilePath) ? File.ReadAllText(FilePath, Encoding.UTF8) : string.Empty;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            File.WriteAllText(FilePath,
                "=== Протокол работы калькулятора многочленов ===" + Environment.NewLine,
                Encoding.UTF8);
        }
    }
}
