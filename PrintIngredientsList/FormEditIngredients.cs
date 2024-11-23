﻿using ExcelReaderUtility;
using ExtendedNumerics.Helpers;
using MathNet.Numerics.Distributions;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        public FormEditIngredients(ProductReader productBaseInfo, CommonDeftReader commonDefData, EditProductData editData = null)
        {
            InitializeComponent();

            this.productBaseInfo = productBaseInfo;
            this.commonDefData = commonDefData;

            this.editData = editData;
        }

        private void FormEditIngredients_Load(object sender, EventArgs e)
        {
            this.MinimumSize = this.Size;

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
                var productData = productBaseInfo.GetProductDataByID(editData.id);


                cmbProduct.SelectedItem = editData.name;
                txtMaterial.Text = productData.rawMaterials;
                txtAmount.Text = editData.amount;
                txtValidDays.Value = editData.validDays;
                cmbStorage.Text = editData.storageMethod;
                txtAllergy.Text = productData.allergy;
                cmbManufacture.Text = editData.manufacturer;
                txtComment.Text = productData.comment;

                txtNumOfSheets.Text = editData.numOfSheets.ToString();


            }


        }

        /// <summary>
        /// 種別選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbKind_SelectedIndexChanged(object sender, EventArgs e)
        {
            //選択された種別の商品名をコンボボックスに設定
            cmbProduct.Items.Clear();
            //商品名コンボボックス
            var lstProduct = productBaseInfo.GetProductList(cmbKind.Text);
            foreach (var product in lstProduct)
            {
                cmbProduct.Items.Add(product);
            }
            if (cmbProduct.Items.Count > 0) cmbProduct.SelectedIndex = 0;

        }
        /// <summary>
        /// 商品選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedIndex < 0) return;

            var productData = (ProductData)cmbProduct.Items[cmbProduct.SelectedIndex];

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
            int days = (int)(timePicker.Value - DateTime.Now).TotalDays;
            if (days < 0) days = 0;

            txtValidDays.ValueChanged -= txtValidDays_ValueChanged;
            txtValidDays.Value = days;
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

                this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        public EditProductData GetEditParam()
        {

            var productData = (ProductData)cmbProduct.Items[cmbProduct.SelectedIndex];

            EditProductData editParam = new EditProductData();

            editParam.id = productData.id;
            editParam.kind = productData.kind;
            editParam.name = productData.name;
            editParam.amount = txtAmount.Text;
            editParam.validDays = (int)txtValidDays.Value;
            editParam.storageMethod = cmbStorage.Text;
            editParam.manufacturer = cmbManufacture.Text;

            editParam.numOfSheets = int.Parse(txtNumOfSheets.Text);

            
            return editParam;
        }


    }
}
