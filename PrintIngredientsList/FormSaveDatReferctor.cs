using ExcelReaderUtility;
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
    public partial class FormSaveDatReferctor : Form
    {
        public int result = 0;
        public bool bApplyAll = false;
        public string selectName = "";
        EditProductData data;
        ProductReader productBaseInfo;
        public FormSaveDatReferctor(ProductReader productBaseInfo,EditProductData data)
        {
            InitializeComponent();
            this.data = data;
            this.productBaseInfo = productBaseInfo;
        }

        private void FormSaveDatReferctor_Load(object sender, EventArgs e)
        {

            label1.Text = $"保存データの\n   「{data.name}」\nがデータベースから見つかりませんでした。";

            var names = productBaseInfo.GetProductList(data.kind);

            foreach( var s in names)
            {
                lstProductName.Items.Add(s);
            }
            if (lstProductName.Items.Count > 0) lstProductName.SelectedIndex = 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            bApplyAll = chkApplyAll.Checked;
            if (lstProductName.SelectedIndex >= 0)
            {
                selectName = (string)lstProductName.Items[lstProductName.SelectedIndex];
            }

            if(radReplace.Checked && string.IsNullOrEmpty(selectName))
            {
                Utility.MessageError("商品名が選択されていません。");
                return;
            }
            if (radDelete.Checked) result = 0;
            else result = 1;

            Close();
        }

        private void radDelete_CheckedChanged(object sender, EventArgs e)
        {
            chkApplyAll.Enabled = true;
            lstProductName.Enabled = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            chkApplyAll.Enabled = false;
            lstProductName.Enabled = true;
        }
    }
}
