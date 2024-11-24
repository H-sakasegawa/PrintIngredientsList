using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using PrintIngredientsList;

namespace LicenseManager
{
    public partial class Form1 : Form
    {

        class CmbItem
        {
            public CmbItem(string title, int month)
            {
                this.title = title;
                this.month = month;
            }

            public override string ToString()
            {
                return title;
            }
            public string title;
            public int month;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddComboItem(1);
            AddComboItem(3);
            AddComboItem(6);
            AddComboItem(12);
            AddComboItem(-1);

            cmbSelectRange.SelectedIndex = 0;

            txtUserName.Text = "TestUser";
            txtMacAddr.Text = "30-85-A9-EE-1B-B0";

        }



        void AddComboItem( int month)
        {
            if (month >= 0)
            {
                cmbSelectRange.Items.Add(new CmbItem($"{month}ヵ月", month));
            }else
            {
                cmbSelectRange.Items.Add(new CmbItem($"制限なし", month));
            }
        }

        private void cmbSelectRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            CmbItem item = (CmbItem)cmbSelectRange.SelectedItem;

            if (item.month > 0)
            {
                DateTime today = DateTime.Today;
                DateTime limitDay = today.AddMonths(item.month);

                dateTimePicker1.Value = limitDay;
            }
            else
            {
                dateTimePicker1.Value = dateTimePicker1.MaxDate;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = PrintIngredientsList.LicenseManager.licenseFileName;
            if ( dlg.ShowDialog()!= DialogResult.OK)
            {
                return;
            }

            string maxAddr = txtMacAddr.Text.Replace("-", "");

            PrintIngredientsList.LicenseManager lm = PrintIngredientsList.LicenseManager.GetLicenseManager();

            if(lm.WriteLicenseFile(dlg.FileName, txtUserName.Text, maxAddr, dateTimePicker1.Value)!=0)
            {
                MessageBox.Show("ライセンスファイル作成失敗");
            }else
            {
                MessageBox.Show($"ライセンスファイルを作成しました。\n{dlg.FileName}");

            }

        }
    }
}
