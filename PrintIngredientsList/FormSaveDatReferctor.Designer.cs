namespace PrintIngredientsList
{
    partial class FormSaveDatReferctor
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
            this.lstProductName = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.radReplace = new System.Windows.Forms.RadioButton();
            this.radDelete = new System.Windows.Forms.RadioButton();
            this.chkApplyAll = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstProductName
            // 
            this.lstProductName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstProductName.Enabled = false;
            this.lstProductName.FormattingEnabled = true;
            this.lstProductName.ItemHeight = 12;
            this.lstProductName.Location = new System.Drawing.Point(44, 102);
            this.lstProductName.Name = "lstProductName";
            this.lstProductName.Size = new System.Drawing.Size(232, 100);
            this.lstProductName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(15, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(263, 52);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            // 
            // radReplace
            // 
            this.radReplace.AutoSize = true;
            this.radReplace.Location = new System.Drawing.Point(17, 80);
            this.radReplace.Name = "radReplace";
            this.radReplace.Size = new System.Drawing.Size(141, 16);
            this.radReplace.TabIndex = 2;
            this.radReplace.Text = "以下の商品に置き換える";
            this.radReplace.UseVisualStyleBackColor = true;
            this.radReplace.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // radDelete
            // 
            this.radDelete.AutoSize = true;
            this.radDelete.Checked = true;
            this.radDelete.Location = new System.Drawing.Point(17, 58);
            this.radDelete.Name = "radDelete";
            this.radDelete.Size = new System.Drawing.Size(99, 16);
            this.radDelete.TabIndex = 3;
            this.radDelete.TabStop = true;
            this.radDelete.Text = "項目を削除する";
            this.radDelete.UseVisualStyleBackColor = true;
            this.radDelete.CheckedChanged += new System.EventHandler(this.radDelete_CheckedChanged);
            // 
            // chkApplyAll
            // 
            this.chkApplyAll.AutoSize = true;
            this.chkApplyAll.Location = new System.Drawing.Point(122, 58);
            this.chkApplyAll.Name = "chkApplyAll";
            this.chkApplyAll.Size = new System.Drawing.Size(112, 16);
            this.chkApplyAll.TabIndex = 4;
            this.chkApplyAll.Text = "他の項目にも適用";
            this.chkApplyAll.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(200, 208);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(76, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormSaveDatReferctor
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(290, 237);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.chkApplyAll);
            this.Controls.Add(this.radDelete);
            this.Controls.Add(this.radReplace);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstProductName);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSaveDatReferctor";
            this.Text = "不明な商品";
            this.Load += new System.EventHandler(this.FormSaveDatReferctor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstProductName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radReplace;
        private System.Windows.Forms.RadioButton radDelete;
        private System.Windows.Forms.CheckBox chkApplyAll;
        private System.Windows.Forms.Button button1;
    }
}