using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace PolynomialCalculator
{
    public partial class MainForm : Form
    {
        private readonly Calculator _calc;
        private readonly Logger _logger;

        // поля коэффициентов первого многочлена (текущий результат)
        private readonly TextBox[] _aBoxes = new TextBox[Polynomial.MaxInputDegree + 1];
        // поля коэффициентов второго многочлена (операнд)
        private readonly TextBox[] _bBoxes = new TextBox[Polynomial.MaxInputDegree + 1];

        private TextBox _txtResult;
        private TextBox _txtRemainder;
        private TextBox _txtPower;
        private TextBox _txtX;
        private Label _lblCurrent;

        public MainForm()
        {
            InitializeComponent();

            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "calculator.log");
            _logger = new Logger(logPath);
            _logger.LogSession("Запуск программы");
            _calc = new Calculator(_logger);

            this.Font = new Font("Segoe UI", 9.5f);

            BuildUi();
            UpdateCurrentLabel();
        }

        private void BuildUi()
        {
            // ---- блок «Текущий результат» (многочлен A) ----
            var grpA = new GroupBox();
            grpA.Text = "Текущий результат (многочлен A):  A = a0 + a1·x + a2·x² + a3·x³ + a4·x⁴";
            grpA.Location = new Point(12, 12);
            grpA.Size = new Size(870, 100);
            this.Controls.Add(grpA);

            for (int i = 0; i <= Polynomial.MaxInputDegree; i++)
            {
                var lbl = new Label();
                lbl.Text = "a" + i;
                lbl.Location = new Point(15 + i * 165, 28);
                lbl.Size = new Size(24, 22);
                lbl.TextAlign = ContentAlignment.MiddleLeft;

                var tb = new TextBox();
                tb.Location = new Point(42 + i * 165, 25);
                tb.Size = new Size(115, 22);
                tb.Text = "0";

                _aBoxes[i] = tb;
                grpA.Controls.Add(lbl);
                grpA.Controls.Add(tb);
            }

            _lblCurrent = new Label();
            _lblCurrent.Location = new Point(15, 65);
            _lblCurrent.Size = new Size(680, 25);
            _lblCurrent.Text = "A(x) = 0";
            _lblCurrent.ForeColor = Color.DarkBlue;
            _lblCurrent.Font = new Font("Consolas", 10f, FontStyle.Bold);
            _lblCurrent.AutoEllipsis = true;
            grpA.Controls.Add(_lblCurrent);

            var btnLoadA = new Button();
            btnLoadA.Text = "Загрузить A в результат";
            btnLoadA.Location = new Point(700, 62);
            btnLoadA.Size = new Size(160, 28);
            btnLoadA.Click += BtnLoadA_Click;
            grpA.Controls.Add(btnLoadA);

            // ---- блок «Операнд B» ----
            var grpB = new GroupBox();
            grpB.Text = "Операнд (многочлен B):  B = b0 + b1·x + b2·x² + b3·x³ + b4·x⁴";
            grpB.Location = new Point(12, 120);
            grpB.Size = new Size(870, 65);
            this.Controls.Add(grpB);

            for (int i = 0; i <= Polynomial.MaxInputDegree; i++)
            {
                var lbl = new Label();
                lbl.Text = "b" + i;
                lbl.Location = new Point(15 + i * 165, 28);
                lbl.Size = new Size(24, 22);
                lbl.TextAlign = ContentAlignment.MiddleLeft;

                var tb = new TextBox();
                tb.Location = new Point(42 + i * 165, 25);
                tb.Size = new Size(115, 22);
                tb.Text = "0";

                _bBoxes[i] = tb;
                grpB.Controls.Add(lbl);
                grpB.Controls.Add(tb);
            }

            // ---- блок операций ----
            var grpOps = new GroupBox();
            grpOps.Text = "Операции";
            grpOps.Location = new Point(12, 195);
            grpOps.Size = new Size(870, 165);
            this.Controls.Add(grpOps);

            AddOpButton(grpOps, "A + B", 15, 28, BtnAdd_Click);
            AddOpButton(grpOps, "A − B", 145, 28, BtnSub_Click);
            AddOpButton(grpOps, "A × B", 275, 28, BtnMul_Click);
            AddOpButton(grpOps, "A / B (с остатком)", 405, 28, BtnDiv_Click);
            AddOpButton(grpOps, "Сброс результата", 575, 28, BtnReset_Click);

            // ---- степень ----
            var lblPow = new Label();
            lblPow.Text = "Показатель n (целое ≥ 0):";
            lblPow.Location = new Point(15, 80);
            lblPow.Size = new Size(170, 22);
            lblPow.TextAlign = ContentAlignment.MiddleLeft;

            _txtPower = new TextBox();
            _txtPower.Location = new Point(190, 77);
            _txtPower.Size = new Size(60, 22);
            _txtPower.Text = "2";

            var btnPow = new Button();
            btnPow.Text = "A ^ n";
            btnPow.Location = new Point(260, 75);
            btnPow.Size = new Size(110, 27);
            btnPow.Click += BtnPow_Click;

            grpOps.Controls.Add(lblPow);
            grpOps.Controls.Add(_txtPower);
            grpOps.Controls.Add(btnPow);

            // ---- значение многочлена ----
            var lblX = new Label();
            lblX.Text = "Значение в точке x =";
            lblX.Location = new Point(400, 80);
            lblX.Size = new Size(140, 22);
            lblX.TextAlign = ContentAlignment.MiddleLeft;

            _txtX = new TextBox();
            _txtX.Location = new Point(545, 77);
            _txtX.Size = new Size(80, 22);
            _txtX.Text = "1";

            var btnEval = new Button();
            btnEval.Text = "Вычислить A(x)";
            btnEval.Location = new Point(635, 75);
            btnEval.Size = new Size(130, 27);
            btnEval.Click += BtnEval_Click;

            grpOps.Controls.Add(lblX);
            grpOps.Controls.Add(_txtX);
            grpOps.Controls.Add(btnEval);

            // ---- результат ----
            var grpRes = new GroupBox();
            grpRes.Text = "Результат";
            grpRes.Location = new Point(12, 370);
            grpRes.Size = new Size(870, 110);
            this.Controls.Add(grpRes);

            var lblRes = new Label();
            lblRes.Text = "Результат:";
            lblRes.Location = new Point(15, 28);
            lblRes.Size = new Size(90, 22);

            _txtResult = new TextBox();
            _txtResult.Location = new Point(110, 25);
            _txtResult.Size = new Size(745, 22);
            _txtResult.ReadOnly = true;
            _txtResult.Font = new Font("Consolas", 10f);

            var lblRem = new Label();
            lblRem.Text = "Остаток:";
            lblRem.Location = new Point(15, 60);
            lblRem.Size = new Size(90, 22);

            _txtRemainder = new TextBox();
            _txtRemainder.Location = new Point(110, 57);
            _txtRemainder.Size = new Size(745, 22);
            _txtRemainder.ReadOnly = true;
            _txtRemainder.Font = new Font("Consolas", 10f);

            grpRes.Controls.Add(lblRes);
            grpRes.Controls.Add(_txtResult);
            grpRes.Controls.Add(lblRem);
            grpRes.Controls.Add(_txtRemainder);

            // ---- кнопки протокола ----
            var btnViewLog = new Button();
            btnViewLog.Text = "Открыть протокол…";
            btnViewLog.Location = new Point(12, 500);
            btnViewLog.Size = new Size(170, 32);
            btnViewLog.Click += BtnViewLog_Click;
            this.Controls.Add(btnViewLog);

            var btnCopyResultToA = new Button();
            btnCopyResultToA.Text = "Скопировать результат в A";
            btnCopyResultToA.Location = new Point(190, 500);
            btnCopyResultToA.Size = new Size(200, 32);
            btnCopyResultToA.Click += BtnCopyResultToA_Click;
            this.Controls.Add(btnCopyResultToA);

            var btnExit = new Button();
            btnExit.Text = "Выход";
            btnExit.Location = new Point(782, 500);
            btnExit.Size = new Size(100, 32);
            btnExit.Click += BtnExit_Click;
            this.Controls.Add(btnExit);

            // ---- подсказка / инструкция ----
            var lblHelp = new Label();
            lblHelp.Location = new Point(12, 545);
            lblHelp.Size = new Size(870, 65);
            lblHelp.ForeColor = Color.DimGray;
            lblHelp.Text =
                "Коэффициенты: a0 — свободный член, a1 — при x, …, a4 — при x⁴. Десятичный разделитель — точка или запятая.\r\n" +
                "Кнопка «Загрузить A в результат» помещает многочлен A в накопитель калькулятора. Операции применяются\r\n" +
                "к накопителю (A := A ⊕ B). Деление возвращает частное и остаток; частное становится новым A.\r\n" +
                "Все действия и ошибки автоматически фиксируются в файл протокола calculator.log.";
            this.Controls.Add(lblHelp);
        }

        private void AddOpButton(GroupBox parent, string text, int x, int y, EventHandler handler)
        {
            var btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(130, 32);
            btn.Click += handler;
            parent.Controls.Add(btn);
        }

        // ---- обработчики событий ----

        private void BtnLoadA_Click(object sender, EventArgs e)
        {
            SafeRun(delegate
            {
                var a = ReadPolynomial(_aBoxes);
                _calc.SetCurrent(a);
                ShowResult(_calc.Current);
                UpdateCurrentLabel();
            });
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            DoBinary(BinaryOp.Add);
        }

        private void BtnSub_Click(object sender, EventArgs e)
        {
            DoBinary(BinaryOp.Subtract);
        }

        private void BtnMul_Click(object sender, EventArgs e)
        {
            DoBinary(BinaryOp.Multiply);
        }

        private void BtnDiv_Click(object sender, EventArgs e)
        {
            SafeRun(delegate
            {
                var a = ReadPolynomial(_aBoxes);
                var b = ReadPolynomial(_bBoxes);
                _calc.SetCurrent(a);
                Polynomial q, rem;
                _calc.Divide(b, out q, out rem);
                _txtResult.Text = q + "      (коэф. " + q.ToCoefficientList() + ")";
                _txtRemainder.Text = rem + "      (коэф. " + rem.ToCoefficientList() + ")";
                UpdateCurrentLabel();
            });
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _calc.Reset();
            ShowResult(_calc.Current);
            UpdateCurrentLabel();
        }

        private void BtnPow_Click(object sender, EventArgs e)
        {
            SafeRun(delegate
            {
                int n;
                if (!int.TryParse(_txtPower.Text.Trim(), NumberStyles.Integer,
                                  CultureInfo.InvariantCulture, out n))
                    throw new FormatException("Показатель степени должен быть целым числом.");
                if (n < 0) throw new FormatException("Показатель степени должен быть неотрицательным.");
                var r = _calc.Power(n);
                ShowResult(r);
                UpdateCurrentLabel();
            });
        }

        private void BtnEval_Click(object sender, EventArgs e)
        {
            SafeRun(delegate
            {
                string xStr = _txtX.Text.Trim().Replace(',', '.');
                double x;
                if (!double.TryParse(xStr, NumberStyles.Float, CultureInfo.InvariantCulture, out x))
                    throw new FormatException("Невозможно распознать x как число.");
                double v = _calc.EvaluateAt(x);
                MessageBox.Show(this,
                    "A(" + x.ToString("G", CultureInfo.InvariantCulture) + ") = " +
                    v.ToString("G", CultureInfo.InvariantCulture),
                    "Значение многочлена", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void BtnViewLog_Click(object sender, EventArgs e)
        {
            using (var form = new LogViewerForm(_logger))
            {
                form.ShowDialog(this);
            }
        }

        private void BtnCopyResultToA_Click(object sender, EventArgs e)
        {
            CopyCurrentToA();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ---- вспомогательные методы ----

        private enum BinaryOp { Add, Subtract, Multiply }

        private void DoBinary(BinaryOp op)
        {
            SafeRun(delegate
            {
                var a = ReadPolynomial(_aBoxes);
                var b = ReadPolynomial(_bBoxes);
                _calc.SetCurrent(a);
                Polynomial r;
                switch (op)
                {
                    case BinaryOp.Add:      r = _calc.Add(b); break;
                    case BinaryOp.Subtract: r = _calc.Subtract(b); break;
                    case BinaryOp.Multiply: r = _calc.Multiply(b); break;
                    default: throw new InvalidOperationException();
                }
                ShowResult(r);
                UpdateCurrentLabel();
            });
        }

        private Polynomial ReadPolynomial(TextBox[] boxes)
        {
            var arr = new double[boxes.Length];
            for (int i = 0; i < boxes.Length; i++)
            {
                string s = boxes[i].Text.Trim();
                if (s.Length == 0) { arr[i] = 0; continue; }
                s = s.Replace(',', '.');
                double v;
                if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                    throw new FormatException("Коэффициент при x^" + i +
                                              " — некорректное число: «" + boxes[i].Text + "».");
                arr[i] = v;
            }
            return Polynomial.FromInputCoefficients(arr);
        }

        private void ShowResult(Polynomial p)
        {
            _txtResult.Text = p + "      (коэф. " + p.ToCoefficientList() + ")";
            _txtRemainder.Text = string.Empty;
        }

        private void UpdateCurrentLabel()
        {
            _lblCurrent.Text = "A(x) = " + _calc.Current;
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
}
