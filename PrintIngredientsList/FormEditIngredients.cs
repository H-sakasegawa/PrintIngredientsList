using ExcelReaderUtility;
using ExtendedNumerics.Helpers;
using MathNet.Numerics.Distributions;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.Streaming.Values;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static ExcelReaderUtility.ProductReader;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace PrintIngredientsList
{
    public partial class FormEditIngredients : Form
    {
        ProductReader productBaseInfo = null;
        CommonDeftReader commonDefData = null;
        EditProductData editData = null;

        FormMain frmMain = null;

        public FormEditIngredients(FormMain frmMain, ProductReader productBaseInfo, CommonDeftReader commonDefData, EditProductData editData = null)
        {
            InitializeComponent();

            this.productBaseInfo = productBaseInfo;
            this.commonDefData = commonDefData;

            this.editData = editData;
            this.frmMain = frmMain;
        }

        private void FormEditIngredients_Load(object sender, EventArgs e)
        {
            MinimumSize = this.Size;


            var lstKind = productBaseInfo.GetKindList();

            var lstStorage = commonDefData.GetSelectText(CommonDeftReader.keyStorage);
            var lstManufacuture = commonDefData.GetSelectText(CommonDeftReader.keyManifacture);

            //-------------------------------------------
            // コンボボックスアイテム設定
            //-------------------------------------------
            //種別コンボボックス
            cmbKind.Items.Add(Const.SelectAll);
            foreach (var s in lstKind)
            {
                cmbKind.Items.Add(s);
            }

            //保存方法コンボボックス
            foreach (var s in lstStorage)
            {
                cmbStorage.Items.Add(s);
            }
            //製造者コンボボックス
            foreach (var s in lstManufacuture)
            {
                cmbManufacture.Items.Add(s);
            }


            //-------------------------------------------

            if (cmbKind.Items.Count > 0)
            {
                cmbKind.SelectedIndex = 0;
            }


            txtNumOfSheets.Text = "1";

            if (editData != null)
            {
                lstProductNames.SelectedIndexChanged -= lstProductNames_SelectedIndexChanged;
                {
                    var productData = productBaseInfo.GetProductDataByID(editData.id);


                    txtProductName.Text = Utility.RemoveCRLF(productData.name);
                    txtAmount.Text = editData.amount;
                    txtValidDays.Value = editData.validDays;
                    cmbStorage.Text = editData.storageMethod;
                    cmbManufacture.Text = editData.manufacturer;

                    txtNumOfSheets.Text = editData.numOfSheets.ToString();

                    for(int i=0; i< lstProductNames.Items.Count; i++)
                    {
                        ProductData product = (ProductData)lstProductNames.Items[i];
                        if (product.id == editData.id)
                        {
                            lstProductNames.SelectedIndex = i;
                        }
                    }

                }
                lstProductNames.SelectedIndexChanged += lstProductNames_SelectedIndexChanged;


                button1.Text = "OK";
                button2.Text = "キャンセル";
            }
            else
            {
                button1.Text = "追加";
                button2.Text = "閉じる";
            }

            LoadUserSetting();
        }


        /// <summary>
        /// 種別選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbKind_SelectedIndexChanged(object sender, EventArgs e)
        {
            //選択された種別の商品名をコンボボックスに設定
            lstProductNames.Items.Clear();
            //商品名コンボボックス
            var lstProduct = productBaseInfo.GetProductList(cmbKind.Text);
            foreach (var product in lstProduct)
            {
                int index =lstProductNames.Items.Add(product);
            }
            if (lstProductNames.Items.Count > 0) lstProductNames.SelectedIndex = 0;

        }
        /// <summary>
        /// 商品選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstProductNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstProductNames.SelectedIndex < 0) return;

            var productData = (ProductData)lstProductNames.Items[lstProductNames.SelectedIndex];

            //商品名
            txtProductName.Text = Utility.RemoveCRLF( productData.name);
            //成分
            txtMaterial.Text = productData.rawMaterials;
            //数量
            txtAmount.Text = productData.amount;

            //保存方法
            cmbStorage.Text = productData.storageMethod;

            //賞味期限日数
            txtValidDays.Text = productData.validDays.ToString();
            //アレルギー
            txtAllergy.Text = productData.allergy;
            //製造者
            cmbManufacture.Text = productData.manufacturer;
            //欄外
            txtComment.Text = productData.comment;

            //栄養成分
            lvNutritional.Items.Clear();
            AddLvNutritional(ItemName.Calorie, productData.Calorie);
            AddLvNutritional(ItemName.Protein, productData.Protein);
            AddLvNutritional(ItemName.Lipids, productData.Lipids);
            AddLvNutritional(ItemName.Carbohydrates, productData.Carbohydrates);
            AddLvNutritional(ItemName.Salt, productData.Salt);
        }



        void AddLvNutritional(string title, string value)
        {
            var lvItem = lvNutritional.Items.Add(title);
            lvItem.SubItems.Add(value);
        }

        /// <summary>
        /// 保存方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbStorage_SelectedIndexChanged(object sender, EventArgs e)
        {

            var data = commonDefData.GetCommonDefData(CommonDeftReader.keyStorage, cmbStorage.Text);
            txtStoragePrintText.Text = data.printText;
        }
        /// <summary>
        /// 製造者
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbManufacture_SelectedIndexChanged(object sender, EventArgs e)
        {
            var data = commonDefData.GetCommonDefData(CommonDeftReader.keyManifacture, cmbManufacture.Text);
            txtAddress.Text = data.printText;

        }



        private void txtValidDays_ValueChanged(object sender, EventArgs e)
        {
            int date = (int)txtValidDays.Value;

            var today = DateTime.Now;

            timePicker.ValueChanged -= timePicker_ValueChanged;
            timePicker.Value = Utility.GetValidDate(date);
            timePicker.ValueChanged += timePicker_ValueChanged;

        }
        private void timePicker_ValueChanged(object sender, EventArgs e)
        {
            //指定された日付から今日までの日数
            var daysSpan = (timePicker.Value.Date - DateTime.Now.Date).Days + 1;
            if (daysSpan < 0) daysSpan = 0;

            txtValidDays.ValueChanged -= txtValidDays_ValueChanged;
            txtValidDays.Value = daysSpan;
            txtValidDays.ValueChanged += txtValidDays_ValueChanged;
        }
        /// <summary>
        /// OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            int value = 0;
            if (int.TryParse(txtNumOfSheets.Text, out value) == false)
            {
                Utility.MessageError("枚数の値が不正な値です");
                return;
            }

            if (editData != null)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }else
            {
                frmMain.AddProduct(GetEditParam());
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        public EditProductData GetEditParam()
        {

            var productData = (ProductData)lstProductNames.Items[lstProductNames.SelectedIndex];

            EditProductData editParam = new EditProductData();

            editParam.id = productData.id;
            //editParam.kind = productData.kind;
            //editParam.name = productData.name;
            editParam.amount = txtAmount.Text;
            editParam.validDays = (int)txtValidDays.Value;
            editParam.storageMethod = cmbStorage.Text;
            editParam.manufacturer = cmbManufacture.Text;

            editParam.numOfSheets = int.Parse(txtNumOfSheets.Text);

            
            return editParam;
        }

        private void FormEditIngredients_SizeChanged(object sender, EventArgs e)
        {
        }

        private void FormEditIngredients_Resize(object sender, EventArgs e)
        {
            if(this.Height> MinimumSize.Height)
            {
                Height = MinimumSize.Height;
            }
        }

        private void FormEditIngredients_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveUserSetting();
        }
        private void LoadUserSetting()
        {
            //setting情報
            int WinX = Properties.Settings.Default.EdtFrmWinLocX;
            int WinY = Properties.Settings.Default.EdtFrmWinLocY;

            if (WinX < 0)
            {   //小さすぎたら補正
                WinX = 0;
            }
            if (WinY < 0)
            {   //小さすぎたら補正
                WinY = 0;
            }
            this.Location = new Point(WinX, WinY);

            int SizeW = Properties.Settings.Default.EdtFrmWinSizeW;
            int SizeH = Properties.Settings.Default.EdtFrmWinSizeH;
            if (SizeW < 200)
            {   //小さすぎたら補正
                SizeW = Size.Width;
            }
            if (SizeH < 200)
            {   //小さすぎたら補正
                SizeH = Size.Height;
            }
            this.Size = new Size(SizeW, SizeH);

            int SplitDistance = Properties.Settings.Default.EdtFrmSplitDistance;
            if (SplitDistance < 100) SplitDistance = 100;
            splitContainer1.SplitterDistance = SplitDistance;
        }
        private void SaveUserSetting()
        {
            Properties.Settings.Default.EdtFrmWinLocX = this.Location.X;
            Properties.Settings.Default.EdtFrmWinLocY = this.Location.Y;
            Properties.Settings.Default.EdtFrmWinSizeW = this.Size.Width;
            Properties.Settings.Default.EdtFrmWinSizeH = this.Size.Height;
            Properties.Settings.Default.EdtFrmSplitDistance = splitContainer1.SplitterDistance;


            Properties.Settings.Default.Save();
        }

        private void lstProductNames_DoubleClick(object sender, EventArgs e)
        {
            if (editData != null) return;
            frmMain.AddProduct(GetEditParam());

        }
    }
}
