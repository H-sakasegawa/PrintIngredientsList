using ExcelReaderUtility;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PrintIngredientsList
{
    public partial class FormMain : Form
    {
        static string exePath = "";
        ProductReader productBaseInfo = new ProductReader();
        CommonDeftReader commonDefInfo = new CommonDeftReader();

        public FormMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            exePath = Path.GetDirectoryName(Application.ExecutablePath);

            //最小サイズをレイアウト時のサイズで固定
            this.MinimumSize = this.Size;

            object[] items = new object[]{
                false,
                "ココナッツスティック",
                "3本",
                DateTime.Parse("2024/12/4"),
                100,
                "常温",
                "無し",
                "本社"
            };

            var row = gridList.Rows.Add(items);

            gridList.Dock = DockStyle.Fill;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer3.Dock = DockStyle.Fill;

            ReadDatabase();

        }

        public static string GetExePath() { return exePath; }


        private void ReadDatabase()
        {
            string dirPath = Path.Combine(exePath, Const.dataBaseFolder);

            string path = Path.Combine(dirPath, Const.ProductFileName);
            productBaseInfo.ReadExcel(path);

            path = Path.Combine(dirPath, Const.CommonDefFileName);
            commonDefInfo.ReadExcel(path);
            
        }


        private void gridList_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            if (e.RowIndex < 0) return;

            var data = (EditProductData)gridList.Rows[e.RowIndex].Tag;


            FormEditIngredients frm = new FormEditIngredients(productBaseInfo, commonDefInfo, data);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                //編集結果
                var editParam = frm.GetEditParam();

                EditParamToGrid(editParam);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FormEditIngredients frm = new FormEditIngredients(productBaseInfo, commonDefInfo);
            if(frm.ShowDialog() == DialogResult.OK)
            {
                //編集結果
                var editParam = frm.GetEditParam();

                EditParamToGrid(editParam);
            }

        }

        private void EditParamToGrid(EditProductData data)
        {
            List<object> lstObject = new List<object>()
            {
                false //印刷チェックボックス
            };

            var paramItems = data.GetParams();

            foreach (var p in paramItems)
            {
                lstObject.Add(p);
            }

            var rowIndex = gridList.Rows.Add(lstObject.ToArray());

            gridList.Rows[rowIndex].Tag = data;



        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<int> lstCheckedRow = new List<int>();
            //チェックボックスがONの行があるか？
            bool bCheckON = false;
            var rows = gridList.Rows;
            for(int iRow=0; iRow < rows.Count; iRow++)
            {
                if( (bool)(rows[iRow].Cells[0].Value)==true)
                {
                    lstCheckedRow.Add(iRow);
                }
            }
            if(lstCheckedRow.Count>0)
            {
                if(MessageBox.Show("選択されている項目を削除します。\nよろしいですか？","削除",MessageBoxButtons.OKCancel, MessageBoxIcon.Question)!= DialogResult.OK)
                {
                    return;
                }
                for(int iRow= lstCheckedRow.Count-1; iRow>=0; iRow--)
                {
                    rows.RemoveAt(lstCheckedRow[iRow]);
                }
            }else
            {
                //選択行を削除
                foreach(DataGridViewRow row in  gridList.SelectedRows)
                {
                    rows.Remove(row);
                }
            }
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach(DataGridViewRow row in gridList.Rows)
            {
                row.Cells[0].Value = chkAll.Checked;

            }
        }
    }
}
