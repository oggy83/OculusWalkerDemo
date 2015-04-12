namespace Oggy
{
    partial class ConfigForm
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
            this.m_startButton = new System.Windows.Forms.Button();
            this.m_monoralMode = new System.Windows.Forms.RadioButton();
            this.m_stereoMode = new System.Windows.Forms.RadioButton();
            this.m_detectedHmdLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_startButton
            // 
            this.m_startButton.BackColor = System.Drawing.Color.AntiqueWhite;
            this.m_startButton.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.m_startButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.m_startButton.Location = new System.Drawing.Point(135, 147);
            this.m_startButton.Name = "m_startButton";
            this.m_startButton.Size = new System.Drawing.Size(140, 23);
            this.m_startButton.TabIndex = 0;
            this.m_startButton.Text = "START";
            this.m_startButton.UseVisualStyleBackColor = false;
            this.m_startButton.Click += new System.EventHandler(this._OnClickStartButton);
            // 
            // m_monoralMode
            // 
            this.m_monoralMode.AutoSize = true;
            this.m_monoralMode.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.m_monoralMode.Location = new System.Drawing.Point(39, 23);
            this.m_monoralMode.Name = "m_monoralMode";
            this.m_monoralMode.Size = new System.Drawing.Size(109, 22);
            this.m_monoralMode.TabIndex = 1;
            this.m_monoralMode.TabStop = true;
            this.m_monoralMode.Text = "No Hmd Mode";
            this.m_monoralMode.UseVisualStyleBackColor = true;
            // 
            // m_stereoMode
            // 
            this.m_stereoMode.AutoSize = true;
            this.m_stereoMode.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.m_stereoMode.Location = new System.Drawing.Point(39, 61);
            this.m_stereoMode.Name = "m_stereoMode";
            this.m_stereoMode.Size = new System.Drawing.Size(89, 22);
            this.m_stereoMode.TabIndex = 2;
            this.m_stereoMode.TabStop = true;
            this.m_stereoMode.Text = "Hmd Mode";
            this.m_stereoMode.UseVisualStyleBackColor = true;
            // 
            // m_detectedHmdLabel
            // 
            this.m_detectedHmdLabel.AutoSize = true;
            this.m_detectedHmdLabel.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.m_detectedHmdLabel.Location = new System.Drawing.Point(70, 86);
            this.m_detectedHmdLabel.Name = "m_detectedHmdLabel";
            this.m_detectedHmdLabel.Size = new System.Drawing.Size(107, 18);
            this.m_detectedHmdLabel.TabIndex = 3;
            this.m_detectedHmdLabel.Text = "Detected device:";
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.ClientSize = new System.Drawing.Size(414, 192);
            this.Controls.Add(this.m_detectedHmdLabel);
            this.Controls.Add(this.m_stereoMode);
            this.Controls.Add(this.m_monoralMode);
            this.Controls.Add(this.m_startButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigForm";
            this.Text = "Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_startButton;
        private System.Windows.Forms.RadioButton m_monoralMode;
        private System.Windows.Forms.RadioButton m_stereoMode;
        private System.Windows.Forms.Label m_detectedHmdLabel;
    }
}