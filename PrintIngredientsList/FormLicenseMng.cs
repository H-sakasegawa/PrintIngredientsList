using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PrintIngredientsList
{
    public partial class FormLicenseMng : Form
    {
        public FormLicenseMng()
        {
            InitializeComponent();
            tabControl1.Visible = false;
            panel1.Parent = this;
            panel2.Parent = this;
            panel2.Visible = false;

        }

         private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 申請ファイル作成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            //MACアドレス取得
            string macAddr = Network.GetMacAddress();

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "ライセンス申請ファイル.txt";
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using( var sw = new StreamWriter(dlg.FileName))
            {
                sw.WriteLine($"申請日：{DateTime.Now.ToString()}");
                sw.WriteLine($"MACアドレス：{macAddr}");
            }

            Utility.MessageInfo($"以下のライセンス申請ファイルを作成しました。\n  {dlg.FileName}");

        }

        private void button5_Click(object sender, EventArgs e)
        {
            //指定されたライセンスファイルを読み込んで、現在のMACアドレスとの整合性をチェック
            LicenseManager lm = LicenseManager.GetLicenseManager();
            LicenseManager.LicenseInfo info = lm.ReadLicenseFile(txtLicenseFile.Text); 

            if( info == null)
            {
                Utility.MessageError("不正なライセンスファイルです。");
                return;
            }

            //ライセンスファイルをExeのフォルダにコピー
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(path, LicenseManager.licenseFileName);
            File.Copy(txtLicenseFile.Text, filePath, true);

            //ライセンス解除
            DialogResult = DialogResult.OK;
            Close();

        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            panel2.Visible = true;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtLicenseFile.Text = dlg.FileName;
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
