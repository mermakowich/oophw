using System;
using System.Drawing;
using System.Windows.Forms;

namespace PolynomialCalculator
{
    public partial class LogViewerForm : Form
    {
        private readonly Logger _logger;
        private TextBox _txt;

        public LogViewerForm(Logger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;

            InitializeComponent();
            this.Font = new Font("Segoe UI", 9.5f);

            BuildUi();
            LoadLog();
        }

        private void BuildUi()
        {
            _txt = new TextBox();
            _txt.Multiline = true;
            _txt.ScrollBars = ScrollBars.Vertical;
            _txt.ReadOnly = true;
            _txt.WordWrap = false;
            _txt.Dock = DockStyle.Fill;
            _txt.Font = new Font("Consolas", 10f);
            this.Controls.Add(_txt);

            var panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 45;

            var btnRefresh = new Button();
            btnRefresh.Text = "Обновить";
            btnRefresh.Location = new Point(10, 8);
            btnRefresh.Size = new Size(110, 30);
            btnRefresh.Click += BtnRefresh_Click;

            var btnClear = new Button();
            btnClear.Text = "Очистить протокол";
            btnClear.Location = new Point(130, 8);
            btnClear.Size = new Size(160, 30);
            btnClear.Click += BtnClear_Click;

            var btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Location = new Point(680, 8);
            btnClose.Size = new Size(100, 30);
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += BtnClose_Click;

            var lblPath = new Label();
            lblPath.Text = "Файл: " + _logger.FilePath;
            lblPath.Location = new Point(300, 14);
            lblPath.Size = new Size(370, 22);
            lblPath.ForeColor = Color.DimGray;

            panel.Controls.Add(btnRefresh);
            panel.Controls.Add(btnClear);
            panel.Controls.Add(lblPath);
            panel.Controls.Add(btnClose);
            this.Controls.Add(panel);
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadLog();
        }

        private void BtnClear_Click(object sender, EventArgs e)
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
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadLog()
        {
            _txt.Text = _logger.ReadAll();
            _txt.SelectionStart = _txt.Text.Length;
            _txt.ScrollToCaret();
        }
    }
}
