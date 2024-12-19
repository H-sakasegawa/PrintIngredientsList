using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrintIngredientsList
{
    public partial class FromSetting : Form
    {
        AppSettingData setting;

        public FromSetting(AppSettingData setting)
        {
            InitializeComponent();

            this.setting = setting;
        }

        private void FromSetting_Load(object sender, EventArgs e)
        {
            //フォント一覧
            //-------------------------------------------------------
            //InstalledFontCollectionオブジェクトの取得
            System.Drawing.Text.InstalledFontCollection ifc =
                new System.Drawing.Text.InstalledFontCollection();
            //インストールされているすべてのフォントファミリアを取得
            FontFamily[] ffs = ifc.Families;

            foreach (FontFamily ff in ffs)
            {
                cmbFont.Items.Add(ff.Name);
            }

            cmbFont.Text = setting.prodListFontName;
            txtFontSize.Text = setting.prodListFontSize.ToString("F1");



        }
        public AppSettingData GetSettingData()
        {
            return setting;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cmbFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            setting.prodListFontName = cmbFont.Text;
        }

        private void txtFontSize_TextChanged(object sender, EventArgs e)
        {
            if(! float.TryParse(txtFontSize.Text, out setting.prodListFontSize))
            {
                Utility.MessageError("正しいフォントサイズを入力してください");
                return;
            }

        }
    }
}
