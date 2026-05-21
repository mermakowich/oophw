using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace PolynomialCalculator;

public sealed class MainForm : Form
{
    private readonly Calculator _calc;
    private readonly Logger _logger;

    // поля коэффициентов первого многочлена (текущий результат)
    private readonly TextBox[] _aBoxes = new TextBox[Polynomial.MaxInputDegree + 1];
    // поля коэффициентов второго многочлена (операнд)
    private readonly TextBox[] _bBoxes = new TextBox[Polynomial.MaxInputDegree + 1];

    private TextBox _txtResult = null!;
    private TextBox _txtRemainder = null!;
    private TextBox _txtPower = null!;
    private TextBox _txtX = null!;
    private Label _lblCurrent = null!;

    public MainForm()
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "calculator.log");
        _logger = new Logger(logPath);
        _logger.LogSession("Запуск программы");
        _calc = new Calculator(_logger);

        Text = "Калькулятор многочленов (до 4-й степени) — вариант 11";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(900, 620);
        Font = new Font("Segoe UI", 9.5f);
        MinimumSize = new Size(900, 620);

        BuildUi();
        UpdateCurrentLabel();
    }

    private void BuildUi()
    {
        // ---- блок «Текущий результат» (многочлен A) ----
        var grpA = new GroupBox
        {
            Text = "Текущий результат (многочлен A):  A = a0 + a1·x + a2·x² + a3·x³ + a4·x⁴",
            Location = new Point(12, 12),
            Size = new Size(870, 100)
        };
        Controls.Add(grpA);

        for (int i = 0; i <= Polynomial.MaxInputDegree; i++)
        {
            var lbl = new Label
            {
                Text = $"a{i}",
                Location = new Point(15 + i * 165, 28),
                Size = new Size(24, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var tb = new TextBox
            {
                Location = new Point(42 + i * 165, 25),
                Size = new Size(115, 22),
                Text = "0"
            };
            _aBoxes[i] = tb;
            grpA.Controls.Add(lbl);
            grpA.Controls.Add(tb);
        }

        _lblCurrent = new Label
        {
            Location = new Point(15, 65),
            Size = new Size(680, 25),
            Text = "A(x) = 0",
            ForeColor = Color.DarkBlue,
            Font = new Font("Consolas", 10f, FontStyle.Bold),
            AutoEllipsis = true
        };
        grpA.Controls.Add(_lblCurrent);

        var btnLoadA = new Button
        {
            Text = "Загрузить A в результат",
            Location = new Point(700, 62),
            Size = new Size(160, 28)
        };
        btnLoadA.Click += (_, _) => SafeRun(() =>
        {
            var a = ReadPolynomial(_aBoxes);
            _calc.SetCurrent(a);
            ShowResult(_calc.Current);
            UpdateCurrentLabel();
        });
        grpA.Controls.Add(btnLoadA);

        // ---- блок «Операнд B» ----
        var grpB = new GroupBox
        {
            Text = "Операнд (многочлен B):  B = b0 + b1·x + b2·x² + b3·x³ + b4·x⁴",
            Location = new Point(12, 120),
            Size = new Size(870, 65)
        };
        Controls.Add(grpB);

        for (int i = 0; i <= Polynomial.MaxInputDegree; i++)
        {
            var lbl = new Label
            {
                Text = $"b{i}",
                Location = new Point(15 + i * 165, 28),
                Size = new Size(24, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var tb = new TextBox
            {
                Location = new Point(42 + i * 165, 25),
                Size = new Size(115, 22),
                Text = "0"
            };
            _bBoxes[i] = tb;
            grpB.Controls.Add(lbl);
            grpB.Controls.Add(tb);
        }

        // ---- блок операций ----
        var grpOps = new GroupBox
        {
            Text = "Операции",
            Location = new Point(12, 195),
            Size = new Size(870, 165)
        };
        Controls.Add(grpOps);

        AddOpButton(grpOps, "A + B", 15, 28, (s, e) => DoBinary((a, b) => _calc.Add(b)));
        AddOpButton(grpOps, "A − B", 145, 28, (s, e) => DoBinary((a, b) => _calc.Subtract(b)));
        AddOpButton(grpOps, "A × B", 275, 28, (s, e) => DoBinary((a, b) => _calc.Multiply(b)));
        AddOpButton(grpOps, "A / B (с остатком)", 405, 28, (s, e) => DoDivide());
        AddOpButton(grpOps, "Сброс результата", 575, 28, (s, e) =>
        {
            _calc.Reset();
            ShowResult(_calc.Current);
            UpdateCurrentLabel();
        });

        // ---- степень ----
        var lblPow = new Label { Text = "Показатель n (целое ≥ 0):", Location = new Point(15, 80), Size = new Size(170, 22), TextAlign = ContentAlignment.MiddleLeft };
        _txtPower = new TextBox { Location = new Point(190, 77), Size = new Size(60, 22), Text = "2" };
        var btnPow = new Button { Text = "A ^ n", Location = new Point(260, 75), Size = new Size(110, 27) };
        btnPow.Click += (s, e) => SafeRun(() =>
        {
            if (!int.TryParse(_txtPower.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n))
                throw new FormatException("Показатель степени должен быть целым числом.");
            if (n < 0) throw new FormatException("Показатель степени должен быть неотрицательным.");
            var r = _calc.Power(n);
            ShowResult(r);
            UpdateCurrentLabel();
        });
        grpOps.Controls.Add(lblPow);
        grpOps.Controls.Add(_txtPower);
        grpOps.Controls.Add(btnPow);

        // ---- значение многочлена ----
        var lblX = new Label { Text = "Значение в точке x =", Location = new Point(400, 80), Size = new Size(140, 22), TextAlign = ContentAlignment.MiddleLeft };
        _txtX = new TextBox { Location = new Point(545, 77), Size = new Size(80, 22), Text = "1" };
        var btnEval = new Button { Text = "Вычислить A(x)", Location = new Point(635, 75), Size = new Size(130, 27) };
        btnEval.Click += (_, _) => SafeRun(() =>
        {
            string xStr = _txtX.Text.Trim().Replace(',', '.');
            if (!double.TryParse(xStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double x))
                throw new FormatException("Невозможно распознать x как число.");
            double v = _calc.EvaluateAt(x);
            MessageBox.Show(this,
                $"A({x.ToString("G", CultureInfo.InvariantCulture)}) = {v.ToString("G", CultureInfo.InvariantCulture)}",
                "Значение многочлена", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
        grpOps.Controls.Add(lblX);
        grpOps.Controls.Add(_txtX);
        grpOps.Controls.Add(btnEval);

        // ---- результат ----
        var grpRes = new GroupBox
        {
            Text = "Результат",
            Location = new Point(12, 370),
            Size = new Size(870, 110)
        };
        Controls.Add(grpRes);

        var lblRes = new Label { Text = "Результат:", Location = new Point(15, 28), Size = new Size(90, 22) };
        _txtResult = new TextBox
        {
            Location = new Point(110, 25),
            Size = new Size(745, 22),
            ReadOnly = true,
            Font = new Font("Consolas", 10f)
        };
        var lblRem = new Label { Text = "Остаток:", Location = new Point(15, 60), Size = new Size(90, 22) };
        _txtRemainder = new TextBox
        {
            Location = new Point(110, 57),
            Size = new Size(745, 22),
            ReadOnly = true,
            Font = new Font("Consolas", 10f)
        };
        grpRes.Controls.Add(lblRes);
        grpRes.Controls.Add(_txtResult);
        grpRes.Controls.Add(lblRem);
        grpRes.Controls.Add(_txtRemainder);

        // ---- кнопки протокола ----
        var btnViewLog = new Button
        {
            Text = "Открыть протокол…",
            Location = new Point(12, 500),
            Size = new Size(170, 32)
        };
        btnViewLog.Click += (s, e) =>
        {
            using var form = new LogViewerForm(_logger);
            form.ShowDialog(this);
        };
        Controls.Add(btnViewLog);

        var btnCopyResultToA = new Button
        {
            Text = "Скопировать результат в A",
            Location = new Point(190, 500),
            Size = new Size(200, 32)
        };
        btnCopyResultToA.Click += (s, e) => CopyCurrentToA();
        Controls.Add(btnCopyResultToA);

        var btnExit = new Button
        {
            Text = "Выход",
            Location = new Point(782, 500),
            Size = new Size(100, 32)
        };
        btnExit.Click += (s, e) => Close();
        Controls.Add(btnExit);

        // ---- подсказка / инструкция ----
        var lblHelp = new Label
        {
            Location = new Point(12, 545),
            Size = new Size(870, 65),
            ForeColor = Color.DimGray,
            Text =
                "Коэффициенты: a0 — свободный член, a1 — при x, …, a4 — при x⁴. Десятичный разделитель — точка или запятая.\r\n" +
                "Кнопка «Загрузить A в результат» помещает многочлен A в накопитель калькулятора. Операции применяются\r\n" +
                "к накопителю (A := A ⊕ B). Деление возвращает частное и остаток; частное становится новым A.\r\n" +
                "Все действия и ошибки автоматически фиксируются в файл протокола calculator.log."
        };
        Controls.Add(lblHelp);
    }

    private void AddOpButton(GroupBox parent, string text, int x, int y, EventHandler handler)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(130, 32)
        };
        btn.Click += handler;
        parent.Controls.Add(btn);
    }

    private Polynomial ReadPolynomial(TextBox[] boxes)
    {
        var arr = new double[boxes.Length];
        for (int i = 0; i < boxes.Length; i++)
        {
            string s = boxes[i].Text.Trim();
            if (s.Length == 0) { arr[i] = 0; continue; }
            s = s.Replace(',', '.');
            if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                throw new FormatException($"Коэффициент при x^{i} — некорректное число: «{boxes[i].Text}».");
            arr[i] = v;
        }
        return Polynomial.FromInputCoefficients(arr);
    }

    private void ShowResult(Polynomial p)
    {
        _txtResult.Text = p.ToString() + "      (коэф. " + p.ToCoefficientList() + ")";
        _txtRemainder.Text = string.Empty;
    }

    private void UpdateCurrentLabel()
    {
        _lblCurrent.Text = "A(x) = " + _calc.Current.ToString();
    }

    private void CopyCurrentToA()
    {
        var coeffs = _calc.Current.ToArray();
        for (int i = 0; i <= Polynomial.MaxInputDegree; i++)
        {
            double v = i < coeffs.Length ? coeffs[i] : 0.0;
            _aBoxes[i].Text = v.ToString("G", CultureInfo.InvariantCulture);
        }
    }

    private void DoBinary(Func<Polynomial, Polynomial, Polynomial> op)
    {
        SafeRun(() =>
        {
            var a = ReadPolynomial(_aBoxes);
            var b = ReadPolynomial(_bBoxes);
            _calc.SetCurrent(a);
            var r = op(a, b);
            ShowResult(r);
            UpdateCurrentLabel();
        });
    }

    private void DoDivide()
    {
        SafeRun(() =>
        {
            var a = ReadPolynomial(_aBoxes);
            var b = ReadPolynomial(_bBoxes);
            _calc.SetCurrent(a);
            var (q, rem) = _calc.Divide(b);
            _txtResult.Text = q.ToString() + "      (коэф. " + q.ToCoefficientList() + ")";
            _txtRemainder.Text = rem.ToString() + "      (коэф. " + rem.ToCoefficientList() + ")";
            UpdateCurrentLabel();
        });
    }

    private void SafeRun(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            _calc.LogError(ex.Message);
            MessageBox.Show(this, ex.Message, "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
