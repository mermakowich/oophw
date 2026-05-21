using System;
using System.Globalization;

namespace PolynomialCalculator
{
    /// <summary>
    /// Калькулятор многочленов: хранит «текущий результат» (как обычный калькулятор)
    /// и выполняет операции с другим многочленом. Все действия пишутся в Logger.
    /// </summary>
    public sealed class Calculator
    {
        private readonly Logger _logger;
        private Polynomial _current = new Polynomial(0.0);

        public Polynomial Current { get { return _current; } }
        public Logger Logger { get { return _logger; } }

        public Calculator(Logger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
        }

        public void Reset()
        {
            _current = new Polynomial(0.0);
            _logger.Log("СБРОС результата. Текущий результат = 0.");
        }

        public void SetCurrent(Polynomial p)
        {
            if (p == null) throw new ArgumentNullException("p");
            _current = p;
            _logger.Log("ЗАГРУЗКА в результат: " + p + "  (коэф. " + p.ToCoefficientList() + ")");
        }

        public Polynomial Add(Polynomial other)
        {
            if (other == null) throw new ArgumentNullException("other");
            var r = _current + other;
            _logger.Log("СЛОЖЕНИЕ: (" + _current + ") + (" + other + ") = " + r);
            _current = r;
            return r;
        }

        public Polynomial Subtract(Polynomial other)
        {
            if (other == null) throw new ArgumentNullException("other");
            var r = _current - other;
            _logger.Log("ВЫЧИТАНИЕ: (" + _current + ") - (" + other + ") = " + r);
            _current = r;
            return r;
        }

        public Polynomial Multiply(Polynomial other)
        {
            if (other == null) throw new ArgumentNullException("other");
            var r = _current * other;
            _logger.Log("УМНОЖЕНИЕ: (" + _current + ") * (" + other + ") = " + r);
            _current = r;
            return r;
        }

        public void Divide(Polynomial other, out Polynomial quotient, out Polynomial remainder)
        {
            if (other == null) throw new ArgumentNullException("other");
            Polynomial q, rem;
            Polynomial.DivMod(_current, other, out q, out rem);
            _logger.Log("ДЕЛЕНИЕ: (" + _current + ") / (" + other + ") = (" + q +
                        "); остаток = (" + rem + ")");
            _current = q;
            quotient = q;
            remainder = rem;
        }

        public Polynomial Power(int n)
        {
            var r = _current.Power(n);
            _logger.Log("СТЕПЕНЬ: (" + _current + ")^" + n + " = " + r);
            _current = r;
            return r;
        }

        public double EvaluateAt(double x)
        {
            double v = _current.Evaluate(x);
            _logger.Log("ЗНАЧЕНИЕ: P(" + x.ToString("G", CultureInfo.InvariantCulture) + ") = " +
                        v.ToString("G", CultureInfo.InvariantCulture) +
                        ", где P(x) = " + _current);
            return v;
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
        }
    }
}
