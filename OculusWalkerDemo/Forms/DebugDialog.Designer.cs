namespace Oggy
{
    partial class DebugDialog
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
            this.m_menuStrip = new System.Windows.Forms.MenuStrip();
            this.m_systemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_mainTab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.m_menuStrip.SuspendLayout();
            this.m_mainTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_menuStrip
            // 
            this.m_menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_systemToolStripMenuItem});
            this.m_menuStrip.Location = new System.Drawing.Point(0, 0);
            this.m_menuStrip.Name = "m_menuStrip";
            this.m_menuStrip.Size = new System.Drawing.Size(252, 24);
            this.m_menuStrip.TabIndex = 1;
            this.m_menuStrip.Text = "menuStrip1";
            // 
            // m_systemToolStripMenuItem
            // 
            this.m_systemToolStripMenuItem.Name = "m_systemToolStripMenuItem";
            this.m_systemToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.m_systemToolStripMenuItem.Text = "System";
            // 
            // m_mainTab
            // 
            this.m_mainTab.Controls.Add(this.tabPage1);
            this.m_mainTab.Controls.Add(this.tabPage2);
            this.m_mainTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_mainTab.Location = new System.Drawing.Point(0, 24);
            this.m_mainTab.Multiline = true;
            this.m_mainTab.Name = "m_mainTab";
            this.m_mainTab.SelectedIndex = 0;
            this.m_mainTab.Size = new System.Drawing.Size(252, 461);
            this.m_mainTab.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(244, 435);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(209, 73);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // DebugDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(252, 485);
            this.ControlBox = false;
            this.Controls.Add(this.m_mainTab);
            this.Controls.Add(this.m_menuStrip);
            this.MaximizeBox = false;
            this.Name = "DebugDialog";
            this.ShowIcon = false;
            this.Text = "DebugDialog";
            this.m_menuStrip.ResumeLayout(false);
            this.m_menuStrip.PerformLayout();
            this.m_mainTab.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip m_menuStrip;
        private System.Windows.Forms.ToolStripMenuItem m_systemToolStripMenuItem;
        private System.Windows.Forms.TabControl m_mainTab;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}