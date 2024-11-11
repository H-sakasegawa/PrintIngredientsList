namespace PrintIngredientsList
{
    partial class FormEditIngredients
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditIngredients));
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMaterial = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.txtAmount = new System.Windows.Forms.TextBox();
            this.timePicker = new System.Windows.Forms.DateTimePicker();
            this.label11 = new System.Windows.Forms.Label();
            this.cmbStorage = new System.Windows.Forms.ComboBox();
            this.txtAllergy = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbKind = new System.Windows.Forms.ComboBox();
            this.cmbProduct = new System.Windows.Forms.ComboBox();
            this.txtStoragePrintText = new System.Windows.Forms.TextBox();
            this.cmbManufacture = new System.Windows.Forms.ComboBox();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.txtValidDays = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.txtNumOfSheets = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.txtValidDays)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "名称：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 237);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "賞味期限：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 158);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "内容量：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 181);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "保存方法：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 260);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "アレルギー：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 319);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 12);
            this.label7.TabIndex = 2;
            this.label7.Text = "製造者：";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(16, 66);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(47, 12);
            this.label9.TabIndex = 2;
            this.label9.Text = "原材料：";
            // 
            // txtMaterial
            // 
            this.txtMaterial.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMaterial.Location = new System.Drawing.Point(69, 63);
            this.txtMaterial.Multiline = true;
            this.txtMaterial.Name = "txtMaterial";
            this.txtMaterial.Size = new System.Drawing.Size(317, 82);
            this.txtMaterial.TabIndex = 5;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(23, 384);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 12);
            this.label10.TabIndex = 2;
            this.label10.Text = "欄外：";
            // 
            // txtComment
            // 
            this.txtComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtComment.Location = new System.Drawing.Point(63, 384);
            this.txtComment.Multiline = true;
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(317, 71);
            this.txtComment.TabIndex = 14;
            // 
            // txtAmount
            // 
            this.txtAmount.Location = new System.Drawing.Point(69, 155);
            this.txtAmount.Name = "txtAmount";
            this.txtAmount.Size = new System.Drawing.Size(55, 19);
            this.txtAmount.TabIndex = 6;
            // 
            // timePicker
            // 
            this.timePicker.Location = new System.Drawing.Point(158, 232);
            this.timePicker.Name = "timePicker";
            this.timePicker.Size = new System.Drawing.Size(125, 19);
            this.timePicker.TabIndex = 10;
            this.timePicker.ValueChanged += new System.EventHandler(this.timePicker_ValueChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(118, 236);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(17, 12);
            this.label11.TabIndex = 6;
            this.label11.Text = "日";
            // 
            // cmbStorage
            // 
            this.cmbStorage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStorage.FormattingEnabled = true;
            this.cmbStorage.Location = new System.Drawing.Point(69, 180);
            this.cmbStorage.Name = "cmbStorage";
            this.cmbStorage.Size = new System.Drawing.Size(83, 20);
            this.cmbStorage.TabIndex = 7;
            this.cmbStorage.SelectedIndexChanged += new System.EventHandler(this.cmbStorage_SelectedIndexChanged);
            // 
            // txtAllergy
            // 
            this.txtAllergy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAllergy.Location = new System.Drawing.Point(69, 257);
            this.txtAllergy.Multiline = true;
            this.txtAllergy.Name = "txtAllergy";
            this.txtAllergy.Size = new System.Drawing.Size(311, 48);
            this.txtAllergy.TabIndex = 11;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(221, 461);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(77, 26);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(304, 461);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(77, 26);
            this.button2.TabIndex = 2;
            this.button2.Text = "キャンセル";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "分類：";
            // 
            // cmbKind
            // 
            this.cmbKind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKind.FormattingEnabled = true;
            this.cmbKind.Location = new System.Drawing.Point(70, 6);
            this.cmbKind.Name = "cmbKind";
            this.cmbKind.Size = new System.Drawing.Size(117, 20);
            this.cmbKind.TabIndex = 3;
            this.cmbKind.SelectedIndexChanged += new System.EventHandler(this.cmbKind_SelectedIndexChanged);
            // 
            // cmbProduct
            // 
            this.cmbProduct.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbProduct.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProduct.FormattingEnabled = true;
            this.cmbProduct.Location = new System.Drawing.Point(70, 35);
            this.cmbProduct.Name = "cmbProduct";
            this.cmbProduct.Size = new System.Drawing.Size(309, 20);
            this.cmbProduct.TabIndex = 4;
            this.cmbProduct.SelectedIndexChanged += new System.EventHandler(this.cmbProduct_SelectedIndexChanged);
            // 
            // txtStoragePrintText
            // 
            this.txtStoragePrintText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStoragePrintText.Location = new System.Drawing.Point(158, 178);
            this.txtStoragePrintText.Multiline = true;
            this.txtStoragePrintText.Name = "txtStoragePrintText";
            this.txtStoragePrintText.ReadOnly = true;
            this.txtStoragePrintText.Size = new System.Drawing.Size(221, 48);
            this.txtStoragePrintText.TabIndex = 8;
            // 
            // cmbManufacture
            // 
            this.cmbManufacture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbManufacture.FormattingEnabled = true;
            this.cmbManufacture.Location = new System.Drawing.Point(70, 316);
            this.cmbManufacture.Name = "cmbManufacture";
            this.cmbManufacture.Size = new System.Drawing.Size(82, 20);
            this.cmbManufacture.TabIndex = 12;
            this.cmbManufacture.SelectedIndexChanged += new System.EventHandler(this.cmbManufacture_SelectedIndexChanged);
            // 
            // txtAddress
            // 
            this.txtAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAddress.Location = new System.Drawing.Point(158, 316);
            this.txtAddress.Multiline = true;
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.ReadOnly = true;
            this.txtAddress.Size = new System.Drawing.Size(221, 62);
            this.txtAddress.TabIndex = 13;
            // 
            // txtValidDays
            // 
            this.txtValidDays.Location = new System.Drawing.Point(63, 232);
            this.txtValidDays.Name = "txtValidDays";
            this.txtValidDays.Size = new System.Drawing.Size(50, 19);
            this.txtValidDays.TabIndex = 9;
            this.txtValidDays.ValueChanged += new System.EventHandler(this.txtValidDays_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(250, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 12);
            this.label8.TabIndex = 15;
            this.label8.Text = "印刷枚数：";
            // 
            // txtNumOfSheets
            // 
            this.txtNumOfSheets.Location = new System.Drawing.Point(306, 6);
            this.txtNumOfSheets.Name = "txtNumOfSheets";
            this.txtNumOfSheets.Size = new System.Drawing.Size(55, 19);
            this.txtNumOfSheets.TabIndex = 16;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(364, 9);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(17, 12);
            this.label12.TabIndex = 17;
            this.label12.Text = "枚";
            // 
            // FormEditIngredients
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 499);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtNumOfSheets);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtValidDays);
            this.Controls.Add(this.cmbManufacture);
            this.Controls.Add(this.txtAddress);
            this.Controls.Add(this.txtStoragePrintText);
            this.Controls.Add(this.cmbProduct);
            this.Controls.Add(this.cmbKind);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtAllergy);
            this.Controls.Add(this.cmbStorage);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.timePicker);
            this.Controls.Add(this.txtAmount);
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.txtMaterial);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormEditIngredients";
            this.Text = "成分情報編集";
            this.Load += new System.EventHandler(this.FormEditIngredients_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtValidDays)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtMaterial;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.TextBox txtAmount;
        private System.Windows.Forms.DateTimePicker timePicker;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmbStorage;
        private System.Windows.Forms.TextBox txtAllergy;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbKind;
        private System.Windows.Forms.ComboBox cmbProduct;
        private System.Windows.Forms.TextBox txtStoragePrintText;
        private System.Windows.Forms.ComboBox cmbManufacture;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.NumericUpDown txtValidDays;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtNumOfSheets;
        private System.Windows.Forms.Label label12;
    }
}