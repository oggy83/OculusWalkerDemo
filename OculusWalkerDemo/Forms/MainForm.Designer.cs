namespace Oggy
{
    partial class MainForm
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
            this.m_renderPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // m_renderPanel
            // 
            this.m_renderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_renderPanel.Location = new System.Drawing.Point(0, 0);
            this.m_renderPanel.Name = "m_renderPanel";
            this.m_renderPanel.Size = new System.Drawing.Size(858, 481);
            this.m_renderPanel.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(858, 481);
            this.Controls.Add(this.m_renderPanel);
            this.Name = "MainForm";
            this.Text = "OculusWalkerDemo";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this._OnFormClosed);
            this.Load += new System.EventHandler(this._OnLoad);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this._OnKeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel m_renderPanel;
    }
}