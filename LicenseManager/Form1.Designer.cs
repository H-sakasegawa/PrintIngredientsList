namespace LicenseManager
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txtMacAddr = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbSelectRange = new System.Windows.Forms.ComboBox();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.chkMacAddr = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtMacAddr
            // 
            this.txtMacAddr.Enabled = false;
            this.txtMacAddr.Location = new System.Drawing.Point(127, 9);
            this.txtMacAddr.Name = "txtMacAddr";
            this.txtMacAddr.Size = new System.Drawing.Size(206, 19);
            this.txtMacAddr.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "ライセンス有効期限";
            // 
            // cmbSelectRange
            // 
            this.cmbSelectRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSelectRange.FormattingEnabled = true;
            this.cmbSelectRange.Location = new System.Drawing.Point(127, 83);
            this.cmbSelectRange.Name = "cmbSelectRange";
            this.cmbSelectRange.Size = new System.Drawing.Size(113, 20);
            this.cmbSelectRange.TabIndex = 4;
            this.cmbSelectRange.SelectedIndexChanged += new System.EventHandler(this.cmbSelectRange_SelectedIndexChanged);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(246, 83);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(137, 19);
            this.dateTimePicker1.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(127, 118);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(180, 38);
            this.button1.TabIndex = 8;
            this.button1.Text = "ライセンスファイルを発行";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // chkMacAddr
            // 
            this.chkMacAddr.AutoSize = true;
            this.chkMacAddr.Location = new System.Drawing.Point(12, 12);
            this.chkMacAddr.Name = "chkMacAddr";
            this.chkMacAddr.Size = new System.Drawing.Size(109, 16);
            this.chkMacAddr.TabIndex = 9;
            this.chkMacAddr.Text = "登録MACアドレス";
            this.chkMacAddr.UseVisualStyleBackColor = true;
            this.chkMacAddr.CheckedChanged += new System.EventHandler(this.chkMacAddr_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(167, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(146, 16);
            this.label1.TabIndex = 3;
            this.label1.Text = "XX-XX-XX-XX-XX-XX";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(167, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 16);
            this.label3.TabIndex = 10;
            this.label3.Text = "XXXXXXXXXXXX";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(136, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(146, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "以下のどちらの形式でも入力可です。";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 168);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkMacAddr);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.cmbSelectRange);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtMacAddr);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "ライセンス発行";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtMacAddr;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbSelectRange;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox chkMacAddr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}

