namespace Loader
{
    partial class ProgressForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mProgressBar = new System.Windows.Forms.ProgressBar();
            this.mProgressPanel = new DevExpress.XtraWaitForm.ProgressPanel();
            this.SuspendLayout();
            // 
            // mProgressBar
            // 
            this.mProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.mProgressBar.Location = new System.Drawing.Point(16, 65);
            this.mProgressBar.Name = "mProgressBar";
            this.mProgressBar.Size = new System.Drawing.Size(597, 16);
            this.mProgressBar.Step = 1;
            this.mProgressBar.TabIndex = 0;
            // 
            // mProgressPanel
            // 
            this.mProgressPanel.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.mProgressPanel.Appearance.Options.UseBackColor = true;
            this.mProgressPanel.AppearanceCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.mProgressPanel.AppearanceCaption.Options.UseFont = true;
            this.mProgressPanel.AppearanceDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.mProgressPanel.AppearanceDescription.Options.UseFont = true;
            this.mProgressPanel.Caption = "Пожалуйста, подождите. Идет подготовка к запуску приложения.";
            this.mProgressPanel.Description = "Копирование файлов...";
            this.mProgressPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.mProgressPanel.Location = new System.Drawing.Point(16, 16);
            this.mProgressPanel.Name = "mProgressPanel";
            this.mProgressPanel.Size = new System.Drawing.Size(597, 49);
            this.mProgressPanel.TabIndex = 2;
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(629, 95);
            this.Controls.Add(this.mProgressBar);
            this.Controls.Add(this.mProgressPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ProgressForm";
            this.Opacity = 0.95D;
            this.Padding = new System.Windows.Forms.Padding(16);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Loader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar mProgressBar;
        private DevExpress.XtraWaitForm.ProgressPanel mProgressPanel;
    }
}

