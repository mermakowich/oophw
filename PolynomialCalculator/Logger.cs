using System;
using System.IO;
using System.Text;

namespace PolynomialCalculator
{
    /// <summary>
    /// Протокол работы калькулятора — пишет события в текстовый файл.
    /// </summary>
    public sealed class Logger
    {
        private readonly object _lock = new object();
        private readonly string _filePath;

        public string FilePath { get { return _filePath; } }

        public Logger(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            _filePath = filePath;

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
            string line = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " +
                          message + Environment.NewLine;
            lock (_lock)
            {
                File.AppendAllText(_filePath, line, Encoding.UTF8);
            }
        }

        public void LogError(string message)
        {
            Log("ОШИБКА: " + message);
        }

        public void LogSession(string title)
        {
            Log(new string('-', 60));
            Log(title);
        }

        public string ReadAll()
        {
            lock (_lock)
            {
                return File.Exists(_filePath) ? File.ReadAllText(_filePath, Encoding.UTF8) : string.Empty;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                File.WriteAllText(_filePath,
                    "=== Протокол работы калькулятора многочленов ===" + Environment.NewLine,
                    Encoding.UTF8);
            }
        }
    }
}
