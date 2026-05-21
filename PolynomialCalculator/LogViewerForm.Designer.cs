namespace PolynomialCalculator
{
    partial class LogViewerForm
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
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.MinimumSize = new System.Drawing.Size(516, 339);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "LogViewerForm";
            this.Text = "Протокол работы калькулятора";
        }
    }
}
