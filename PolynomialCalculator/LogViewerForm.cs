using System;
using System.Drawing;
using System.Windows.Forms;

namespace PolynomialCalculator;

public sealed class LogViewerForm : Form
{
    private readonly Logger _logger;
    private readonly TextBox _txt;

    public LogViewerForm(Logger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Text = "Протокол работы калькулятора";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(800, 500);
        Font = new Font("Segoe UI", 9.5f);
        MinimumSize = new Size(500, 300);

        _txt = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            WordWrap = false,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10f)
        };
        Controls.Add(_txt);

        var panel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 45
        };
        var btnRefresh = new Button
        {
            Text = "Обновить",
            Location = new Point(10, 8),
            Size = new Size(110, 30)
        };
        btnRefresh.Click += (_, _) => LoadLog();
        var btnClear = new Button
        {
            Text = "Очистить протокол",
            Location = new Point(130, 8),
            Size = new Size(160, 30)
        };
        btnClear.Click += (_, _) =>
        {
            var dr = MessageBox.Show(this,
                "Очистить файл протокола?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                _logger.Clear();
                _logger.Log("Протокол очищен пользователем.");
                LoadLog();
            }
        };
        var btnClose = new Button
        {
            Text = "Закрыть",
            Location = new Point(680, 8),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnClose.Click += (_, _) => Close();

        var lblPath = new Label
        {
            Text = "Файл: " + _logger.FilePath,
            Location = new Point(300, 14),
            Size = new Size(370, 22),
            ForeColor = Color.DimGray
        };

        panel.Controls.Add(btnRefresh);
        panel.Controls.Add(btnClear);
        panel.Controls.Add(lblPath);
        panel.Controls.Add(btnClose);
        Controls.Add(panel);

        LoadLog();
    }

    private void LoadLog()
    {
        _txt.Text = _logger.ReadAll();
        _txt.SelectionStart = _txt.Text.Length;
        _txt.ScrollToCaret();
    }
}
