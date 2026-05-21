using System;
using System.Globalization;
using System.Text;

namespace PolynomialCalculator;

/// <summary>
/// Многочлен с вещественными коэффициентами.
/// Коэффициенты хранятся в массиве, где индекс соответствует степени:
/// coefficients[0] — свободный член, coefficients[i] — коэффициент при x^i.
/// </summary>
public sealed class Polynomial : IEquatable<Polynomial>
{
    public const int MaxInputDegree = 4;
    private const double Epsilon = 1e-12;

    private readonly double[] _coeffs;

    public Polynomial(params double[] coefficients)
    {
        if (coefficients == null || coefficients.Length == 0)
        {
            _coeffs = new double[] { 0.0 };
            return;
        }

        int last = coefficients.Length - 1;
        while (last > 0 && Math.Abs(coefficients[last]) < Epsilon)
            last--;

        _coeffs = new double[last + 1];
        Array.Copy(coefficients, _coeffs, last + 1);
    }

    /// <summary>Степень многочлена. Для нулевого многочлена — 0.</summary>
    public int Degree => _coeffs.Length - 1;

    /// <summary>Признак нулевого многочлена.</summary>
    public bool IsZero => Degree == 0 && Math.Abs(_coeffs[0]) < Epsilon;

    /// <summary>Получить коэффициент при заданной степени.</summary>
    public double this[int power]
    {
        get
        {
            if (power < 0)
                throw new ArgumentOutOfRangeException(nameof(power), "Степень не может быть отрицательной.");
            return power < _coeffs.Length ? _coeffs[power] : 0.0;
        }
    }

    /// <summary>Получить копию массива коэффициентов.</summary>
    public double[] ToArray() => (double[])_coeffs.Clone();

    // ----- арифметические операции -----

    public static Polynomial operator +(Polynomial a, Polynomial b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));

        int len = Math.Max(a._coeffs.Length, b._coeffs.Length);
        var result = new double[len];
        for (int i = 0; i < len; i++)
            result[i] = a[i] + b[i];
        return new Polynomial(result);
    }

    public static Polynomial operator -(Polynomial a, Polynomial b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));

        int len = Math.Max(a._coeffs.Length, b._coeffs.Length);
        var result = new double[len];
        for (int i = 0; i < len; i++)
            result[i] = a[i] - b[i];
        return new Polynomial(result);
    }

    public static Polynomial operator -(Polynomial a)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        var result = new double[a._coeffs.Length];
        for (int i = 0; i < a._coeffs.Length; i++)
            result[i] = -a._coeffs[i];
        return new Polynomial(result);
    }

    public static Polynomial operator *(Polynomial a, Polynomial b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));

        var result = new double[a._coeffs.Length + b._coeffs.Length - 1];
        for (int i = 0; i < a._coeffs.Length; i++)
            for (int j = 0; j < b._coeffs.Length; j++)
                result[i + j] += a._coeffs[i] * b._coeffs[j];
        return new Polynomial(result);
    }

    /// <summary>
    /// Деление многочленов «уголком». Возвращает частное и остаток.
    /// </summary>
    public static (Polynomial quotient, Polynomial remainder) DivMod(Polynomial dividend, Polynomial divisor)
    {
        if (dividend is null) throw new ArgumentNullException(nameof(dividend));
        if (divisor is null) throw new ArgumentNullException(nameof(divisor));
        if (divisor.IsZero)
            throw new DivideByZeroException("Деление на нулевой многочлен невозможно.");

        if (dividend.Degree < divisor.Degree)
            return (new Polynomial(0.0), new Polynomial(dividend._coeffs));

        var rem = (double[])dividend._coeffs.Clone();
        int divDeg = divisor.Degree;
        double divLead = divisor._coeffs[divDeg];

        int quotLen = dividend.Degree - divDeg + 1;
        var quot = new double[quotLen];

        for (int i = dividend.Degree; i >= divDeg; i--)
        {
            double factor = rem[i] / divLead;
            quot[i - divDeg] = factor;
            for (int j = 0; j <= divDeg; j++)
                rem[i - divDeg + j] -= factor * divisor._coeffs[j];
        }

        var remArr = new double[Math.Max(divDeg, 1)];
        for (int i = 0; i < divDeg; i++)
            remArr[i] = rem[i];

        return (new Polynomial(quot), new Polynomial(remArr));
    }

    public static Polynomial operator /(Polynomial a, Polynomial b)
    {
        var (q, _) = DivMod(a, b);
        return q;
    }

    public static Polynomial operator %(Polynomial a, Polynomial b)
    {
        var (_, r) = DivMod(a, b);
        return r;
    }

    /// <summary>Возведение многочлена в целую неотрицательную степень.</summary>
    public Polynomial Power(int n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "Показатель степени должен быть неотрицательным целым.");

        var result = new Polynomial(1.0);
        var baseP = this;
        int exp = n;
        while (exp > 0)
        {
            if ((exp & 1) == 1)
                result = result * baseP;
            exp >>= 1;
            if (exp > 0)
                baseP = baseP * baseP;
        }
        return result;
    }

    /// <summary>Вычисление значения многочлена в точке x (схема Горнера).</summary>
    public double Evaluate(double x)
    {
        double result = 0.0;
        for (int i = _coeffs.Length - 1; i >= 0; i--)
            result = result * x + _coeffs[i];
        return result;
    }

    // ----- разбор строки коэффициентов -----

    /// <summary>
    /// Разобрать строку коэффициентов, разделённых пробелами/запятыми/точками с запятой.
    /// Порядок: a0 a1 a2 a3 a4 (свободный член — первый).
    /// </summary>
    public static Polynomial Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new FormatException("Строка коэффициентов пуста.");

        var parts = text.Replace(';', ' ').Replace('\t', ' ')
            .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length > MaxInputDegree + 1)
            throw new FormatException(
                $"Слишком много коэффициентов: {parts.Length}. Допускается не более {MaxInputDegree + 1} (степень до {MaxInputDegree}).");

        var coeffs = new double[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            string normalized = parts[i].Replace(',', '.');
            if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                throw new FormatException($"Невозможно распознать коэффициент «{parts[i]}» как число.");
            coeffs[i] = v;
        }
        return new Polynomial(coeffs);
    }

    /// <summary>Сформировать многочлен из массива коэффициентов длиной до MaxInputDegree+1.</summary>
    public static Polynomial FromInputCoefficients(double[] coefficients)
    {
        if (coefficients == null)
            throw new ArgumentNullException(nameof(coefficients));
        if (coefficients.Length > MaxInputDegree + 1)
            throw new ArgumentException(
                $"Слишком много коэффициентов: {coefficients.Length}. Допускается не более {MaxInputDegree + 1}.");
        return new Polynomial(coefficients);
    }

    // ----- форматирование -----

    public override string ToString()
    {
        if (IsZero) return "0";

        var sb = new StringBuilder();
        bool first = true;
        for (int i = _coeffs.Length - 1; i >= 0; i--)
        {
            double c = _coeffs[i];
            if (Math.Abs(c) < Epsilon) continue;

            string sign;
            double abs = Math.Abs(c);
            if (first)
            {
                sign = c < 0 ? "-" : "";
                first = false;
            }
            else
            {
                sign = c < 0 ? " - " : " + ";
            }

            string coeffStr;
            if (i == 0 || Math.Abs(abs - 1.0) > Epsilon)
                coeffStr = abs.ToString("G", CultureInfo.InvariantCulture);
            else
                coeffStr = "";

            string xPart = i switch
            {
                0 => "",
                1 => "x",
                _ => $"x^{i}"
            };

            string sep = (coeffStr.Length > 0 && xPart.Length > 0) ? "*" : "";
            sb.Append(sign).Append(coeffStr).Append(sep).Append(xPart);
        }
        return sb.ToString();
    }

    /// <summary>Строка вида "[a0, a1, a2, a3, a4]" — удобно для протокола.</summary>
    public string ToCoefficientList()
    {
        var sb = new StringBuilder("[");
        for (int i = 0; i < _coeffs.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(_coeffs[i].ToString("G", CultureInfo.InvariantCulture));
        }
        sb.Append(']');
        return sb.ToString();
    }

    // ----- сравнение -----

    public bool Equals(Polynomial? other)
    {
        if (other is null) return false;
        if (Degree != other.Degree) return false;
        for (int i = 0; i <= Degree; i++)
            if (Math.Abs(_coeffs[i] - other._coeffs[i]) > Epsilon) return false;
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as Polynomial);

    public override int GetHashCode()
    {
        var hc = new HashCode();
        foreach (var c in _coeffs) hc.Add(Math.Round(c, 9));
        return hc.ToHashCode();
    }

    public static bool operator ==(Polynomial? a, Polynomial? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(Polynomial? a, Polynomial? b) => !(a == b);
}
