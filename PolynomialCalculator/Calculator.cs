using System;
using System.Globalization;

namespace PolynomialCalculator;

/// <summary>
/// Калькулятор многочленов: хранит «текущий результат» (как обычный калькулятор)
/// и выполняет операции с другим многочленом. Все действия пишутся в Logger.
/// </summary>
public sealed class Calculator
{
    private readonly Logger _logger;
    public Polynomial Current { get; private set; } = new Polynomial(0.0);

    public Calculator(Logger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Reset()
    {
        Current = new Polynomial(0.0);
        _logger.Log("СБРОС результата. Текущий результат = 0.");
    }

    public void SetCurrent(Polynomial p)
    {
        Current = p ?? throw new ArgumentNullException(nameof(p));
        _logger.Log($"ЗАГРУЗКА в результат: {p}  (коэф. {p.ToCoefficientList()})");
    }

    public Polynomial Add(Polynomial other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        var r = Current + other;
        _logger.Log($"СЛОЖЕНИЕ: ({Current}) + ({other}) = {r}");
        Current = r;
        return r;
    }

    public Polynomial Subtract(Polynomial other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        var r = Current - other;
        _logger.Log($"ВЫЧИТАНИЕ: ({Current}) - ({other}) = {r}");
        Current = r;
        return r;
    }

    public Polynomial Multiply(Polynomial other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        var r = Current * other;
        _logger.Log($"УМНОЖЕНИЕ: ({Current}) * ({other}) = {r}");
        Current = r;
        return r;
    }

    public (Polynomial quotient, Polynomial remainder) Divide(Polynomial other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        var (q, r) = Polynomial.DivMod(Current, other);
        _logger.Log($"ДЕЛЕНИЕ: ({Current}) / ({other}) = ({q}); остаток = ({r})");
        Current = q;
        return (q, r);
    }

    public Polynomial Power(int n)
    {
        var r = Current.Power(n);
        _logger.Log($"СТЕПЕНЬ: ({Current})^{n} = {r}");
        Current = r;
        return r;
    }

    public double EvaluateAt(double x)
    {
        double v = Current.Evaluate(x);
        _logger.Log(
            $"ЗНАЧЕНИЕ: P({x.ToString("G", CultureInfo.InvariantCulture)}) = " +
            v.ToString("G", CultureInfo.InvariantCulture) +
            $", где P(x) = {Current}");
        return v;
    }

    public void LogError(string message) => _logger.LogError(message);
    public Logger Logger => _logger;
}
