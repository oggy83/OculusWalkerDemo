﻿namespace Oggy
{
	partial class MyForm
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
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
			this.m_renderPanel.Size = new System.Drawing.Size(284, 261);
			this.m_renderPanel.TabIndex = 0;
			// 
			// MyForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.m_renderPanel);
			this.Name = "MyForm";
			this.Text = "Form1";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this._OnFormClosed);
			this.Load += new System.EventHandler(this._OnLoad);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel m_renderPanel;
	}
}

