namespace PolynomialCalculator
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 620);
            this.MinimumSize = new System.Drawing.Size(916, 659);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Name = "MainForm";
            this.Text = "Калькулятор многочленов (до 4-й степени) — вариант 11";
        }
    }
}
