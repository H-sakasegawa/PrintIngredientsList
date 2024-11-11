using ExcelReaderUtility;
using NPOI.HPSF;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.Streaming.Values;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Shapes;
using static System.Windows.Forms.AxHost;

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
            exePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

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
            object[] items2 = new object[]{
                false,
                "AAAA",
                "5枚",
                DateTime.Parse("2024/12/8"),
                100,
                "冷凍",
                "無し",
                "本社"
            };

            //gridList.Rows.Add(items);
            //gridList.Rows.Add(items2);

            gridList.Dock = DockStyle.Fill;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer3.Dock = DockStyle.Fill;

            ReadDatabase();

        }

        public static string GetExePath() { return exePath; }


        private void ReadDatabase()
        {
            string dirPath = System.IO.Path.Combine(exePath, Const.dataBaseFolder);

            string path = System.IO.Path.Combine(dirPath, Const.ProductFileName);
            productBaseInfo.ReadExcel(path);

            path = System.IO.Path.Combine(dirPath, Const.CommonDefFileName);
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
            if (frm.ShowDialog() == DialogResult.OK)
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

            panelPreviw.Invalidate();



        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<int> lstCheckedRow = new List<int>();
            //チェックボックスがONの行があるか？
            bool bCheckON = false;
            var rows = gridList.Rows;
            for (int iRow = 0; iRow < rows.Count; iRow++)
            {
                if ((bool)(rows[iRow].Cells[0].Value) == true)
                {
                    lstCheckedRow.Add(iRow);
                }
            }
            if (lstCheckedRow.Count > 0)
            {
                if (MessageBox.Show("選択されている項目を削除します。\nよろしいですか？", "削除", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                {
                    return;
                }
                for (int iRow = lstCheckedRow.Count - 1; iRow >= 0; iRow--)
                {
                    rows.RemoveAt(lstCheckedRow[iRow]);
                }
            }
            else
            {
                //選択行を削除
                foreach (DataGridViewRow row in gridList.SelectedRows)
                {
                    rows.Remove(row);
                }
            }
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in gridList.Rows)
            {
                row.Cells[0].Value = chkAll.Checked;

            }
        }

        /// <summary>
        /// 行選択イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridList_SelectionChanged(object sender, EventArgs e)
        {
            panelPreviw.Invalidate();
        }
        /// <summary>
        /// プレビューパネル描画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelPreviw_Paint(object sender, PaintEventArgs e)
        {
            Graphics gPreview = e.Graphics;

            if (gridList.SelectedRows.Count <= 0) return;

            //現在選択されているグリッドのRowを取得
            var curRow = gridList.SelectedRows[0];
            EditProductData param = (EditProductData)curRow.Tag;

            Pen pen = new Pen(Color.Black, (float)0.1);
#if true
            //枠描画

            const int Line1Hight = 6;
            const int Line2Hight = 12;
            const int MaterialRowHight1 = 20;


            var commonDefStorage = commonDefInfo.GetCommonDefData("保存方法", param.storageMethod);
            var commonDefManifac = commonDefInfo.GetCommonDefData("製造者", param.manufacturer);


            float dpiX = 0;
            float dpiY = 0;

            int areaWidth = (int)DrawUtil2.MillimetersToPixels(DrawUtil2.areaWidthMM, gPreview.DpiX);
            int areaHeight = (int)DrawUtil2.MillimetersToPixels(DrawUtil2.areaHeightMM, gPreview.DpiY);

            //Bitmap bmp = new Bitmap(panelPreviw.Width, panelPreviw.Height);
            Bitmap bmp = new Bitmap(areaWidth + 5, areaHeight + 5);
            Graphics gBmp = Graphics.FromImage(bmp);


            //DrawUtil2 util = new DrawUtil2(gPreview, 2, 2);
            DrawUtil2 util = new DrawUtil2(gBmp, 2, 2);

            //名称
            int nextY = 0;
            nextY = util.DrawItem("名   称", param.name, 0, Line1Hight);
            nextY = util.DrawItem("原材料名", param.rawMaterials, nextY, MaterialRowHight1);
            nextY = util.DrawItem("内 容 量", param.amount, nextY, Line1Hight);
            nextY = util.DrawItem("賞味期限", param.dtExpirationDate.ToLongDateString(), nextY, Line1Hight);
            nextY = util.DrawItem("保存方法", commonDefStorage.printText, nextY, Line2Hight);
            nextY = util.DrawItem("製 造 者", commonDefManifac.printText, nextY, Line2Hight);
            nextY = util.DrawItemComment("欄   外", param.comment, nextY, false);


            //現在のPreviewパネルサイズ
            float rate = (float)1.0;
            int stretchWidth = (int)(areaWidth * rate);
            int stretchHeight = (int)(areaHeight * rate);
            if (panelPreviw.Width < panelPreviw.Height)
            {
                stretchWidth = panelPreviw.Width;
                stretchHeight = (int)(areaHeight * (stretchWidth / (float)areaWidth));
            }
            else
            {
                stretchHeight = panelPreviw.Height;
                stretchWidth = (int)(areaWidth * (stretchHeight / (float)areaHeight));
            }

            gPreview.InterpolationMode = InterpolationMode.HighQualityBilinear;
            gPreview.DrawImage(bmp, 0, 0, stretchWidth, stretchHeight);
            //gPreview.DrawImage(bmp, 0, 0);

            //bmp.Save("D:\\Temp\\aaaa.bmp");
#else
            DrawUtil util = new DrawUtil(g, 2, 2);

            //名称
            int nextY = util.DrawItem("名   称", param.name, 0);
            nextY = util.DrawItem("原材料名", param.rawMaterials, nextY);
            nextY = util.DrawItem("内 容 量", param.amount, nextY);
            nextY = util.DrawItem("賞味期限", param.dtExpirationDate.ToLongDateString(), nextY);
            nextY = util.DrawItem("保存方法", param.storageMethod, nextY);
            nextY = util.DrawItem("製 造 者", param.manufacturer, nextY);
            nextY = util.DrawItemComment("欄   外", param.comment, nextY, false);
#endif

        }
        float _X(float x)
        {
            return Const.GapLeft + x;
        }
        float _Y(float y)
        {
            return Const.GapTop + y;
        }

        /// <summary>
        /// グリッドセルの編集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = gridList.Rows[e.RowIndex];
            //枚数
            if (e.ColumnIndex == 2)
            {
                var editParam = (EditProductData)row.Tag;
                string sValue = (string)row.Cells[e.ColumnIndex].Value;

                int.TryParse(sValue, out editParam.numOfSheets);
            }
        }

        private void gridList_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (!gridList.IsCurrentCellDirty)
                return;

            var row = gridList.Rows[e.RowIndex];
            //枚数
            if (e.ColumnIndex == 2)
            {
                string sValue = (string)e.FormattedValue;
                int value;
                if (!int.TryParse(sValue, out value))
                {

                    MessageBox.Show("不正な値です","エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
            }
        }

        private void gridList_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            gridList.Rows[e.RowIndex].ErrorText = null;

        }
    }
}
