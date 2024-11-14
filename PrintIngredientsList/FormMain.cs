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
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Shapes;
using static System.Windows.Forms.AxHost;
using System.Drawing.Printing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Windows.Controls;
using System.Reflection;
using NPOI.OpenXmlFormats.Shared;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;


namespace PrintIngredientsList
{
    public partial class FormMain : Form
    {

        enum PrintType
        {
            PREVIEW,
            PRINT
        }

        class PrintDocumentEx : System.Drawing.Printing.PrintDocument
        {
            public PrintDocumentEx(PrintType type)
                : base()
            {
                printType = type;
            }
            public PrintType printType;
        }

        static string exePath = "";
        ProductReader productBaseInfo = new ProductReader();
        CommonDeftReader commonDefInfo = new CommonDeftReader();

        string prevDataFilePath;

        public FormMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            exePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

            prevDataFilePath = System.IO.Path.Combine(exePath, "save.dat");



            //最小サイズをレイアウト時のサイズで固定
            this.MinimumSize = this.Size;

            //object[] items = new object[]{
            //    false,
            //    "ココナッツスティック",
            //    "3本",
            //    DateTime.Parse("2024/12/4"),
            //    100,
            //    "常温",
            //    "無し",
            //    "本社"
            //};
            //object[] items2 = new object[]{
            //    false,
            //    "AAAA",
            //    "5枚",
            //    DateTime.Parse("2024/12/8"),
            //    100,
            //    "冷凍",
            //    "無し",
            //    "本社"
            //};

            //gridList.Rows.Add(items);
            //gridList.Rows.Add(items2);

            toolStripContainer1.Dock = DockStyle.Fill;
            tabControl1.Dock = DockStyle.Fill;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer3.Dock = DockStyle.Fill;
            gridList.Dock = DockStyle.Fill;


            //フォント一覧
            //InstalledFontCollectionオブジェクトの取得
            System.Drawing.Text.InstalledFontCollection ifc =
                new System.Drawing.Text.InstalledFontCollection();
            //インストールされているすべてのフォントファミリアを取得
            FontFamily[] ffs = ifc.Families;

            foreach (FontFamily ff in ffs)
            {
                cmbFont.Items.Add(ff.Name);
            }
            cmbFont.Text = DrawUtil2.fontName;


            ReadDatabase();

            //前回保存されたデータの読み込み
            ReadSavedPath();

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

            var row = gridList.Rows[e.RowIndex];
            var data = (EditProductData)row.Tag;


            FormEditIngredients frm = new FormEditIngredients(productBaseInfo, commonDefInfo, data);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                //編集結果
                var editParam = frm.GetEditParam();

                EditParamToGridUpdate(row ,editParam);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FormEditIngredients frm = new FormEditIngredients(productBaseInfo, commonDefInfo);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                //編集結果
                var editParam = frm.GetEditParam();

                EditParamToGridAdd( editParam);
            }

        }

        private void EditParamToGridUpdate(DataGridViewRow row, EditProductData data, bool redfawPreview = true)
        {
            List<object> lstObject = new List<object>()
            {
                false //印刷チェックボックス
            };

            var paramItems = data.GetParams();

            int index = 1;
            foreach (var p in paramItems)
            {
                row.Cells[index].Value = p;
                index++;
            }
            row.Tag = data;

            if (redfawPreview)
            {
                panelPreviw.Invalidate();
            }
        }

        private void EditParamToGridAdd(EditProductData data, bool redfawPreview = true)
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

            if (redfawPreview)
            {
                panelPreviw.Invalidate();
            }
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
            float dpiX = 0;
            float dpiY = 0;

            int areaWidth = (int)DrawUtil2.MillimetersToPixels(DrawUtil2.cellWidthMM, gPreview.DpiX);
            int areaHeight = (int)DrawUtil2.MillimetersToPixels(DrawUtil2.cellHeightMM, gPreview.DpiY);

            //Bitmap bmp = new Bitmap(panelPreviw.Width, panelPreviw.Height);
            Bitmap bmp = new Bitmap(areaWidth + 5, areaHeight + 5);
            Graphics gBmp = Graphics.FromImage(bmp);

            DrawLabel(e.Graphics, 3, 3);

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
                string sValue = row.Cells[e.ColumnIndex].Value.ToString();

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

        private PrintDocumentEx CreatePrintDocument(PrintType printType)
        {
            PrintDocumentEx pd = new PrintDocumentEx(printType);

            var ps = new System.Drawing.Printing.PrinterSettings();

            //A4用紙
            foreach (System.Drawing.Printing.PaperSize psize in pd.PrinterSettings.PaperSizes)
            {
                if (psize.Kind == PaperKind.A4)
                {
                    pd.DefaultPageSettings.PaperSize = psize;
                    break;
                }
            }

            //PrintPageイベントハンドラの追加
            pd.PrintPage +=
                new System.Drawing.Printing.PrintPageEventHandler(pd_PrintPage);

            return pd;
        }

        /// <summary>
        /// プレビュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            PrintDocumentEx pd = CreatePrintDocument(PrintType.PREVIEW);

            //PrintPreviewDialogオブジェクトの作成
            PrintPreviewDialog ppd = new PrintPreviewDialog();
            //プレビューするPrintDocumentを設定
            ppd.Document = pd;

            ppd.Size = new Size(600, 480);
            //印刷プレビューダイアログを表示する
            ppd.ShowDialog();

        }
        /// <summary>
        /// 印刷
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            PrintDocumentEx pd = CreatePrintDocument(PrintType.PRINT);
            //PrintDialogクラスの作成
            System.Windows.Forms.PrintDialog pdlg = new System.Windows.Forms.PrintDialog();
            //PrintDocumentを指定
            pdlg.Document = pd;
            //印刷の選択ダイアログを表示する
            if (pdlg.ShowDialog() == DialogResult.OK)
            {
                //OKがクリックされた時は印刷する
                pd.Print();
            }

        }

        private int MILLI2POINT(float milli)
        {
            return (int)(milli / 0.352777);
        }
        private float POINT2MILLI(int point)
        {
            return (float)(point * 0.352777);
        }

        private void pd_PrintPage(object sender,
            System.Drawing.Printing.PrintPageEventArgs e)
        {

             PrintDocumentEx pd = (PrintDocumentEx)sender;
            
            //印刷データ収集
            List<EditProductData> lstPrintData = new List<EditProductData>();
            foreach (DataGridViewRow row in gridList.Rows)
            {
                if(radPrintSelect.Checked)
                {
                    if((bool)row.Cells[0].Value ==false)
                    {
                        continue;
                    }
                }
                lstPrintData.Add((EditProductData)row.Tag);
            }


            float A4HeightMM = 297; //mm
            float A4WidthMM = 210; //mm

            float startX = (float)19;
            float startY = (float)18.5;

            float LabelBlockWidth = 43;
            float LabelBlockHeiht = 65;

            // float pageWidthMM = POINT2MILLI(e.PageSettings.PaperSize.Width);

            if (pd.printType == PrintType.PREVIEW && chkTestLineDraw.Checked)
            {
                DrawUtil2 util = new DrawUtil2(e.Graphics);
                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black, (float)0.1);
                pen.DashStyle = DashStyle.Dash;

                PointF pnt1, pnt2;

                //縦ライン
                for (float x = startX; x < A4WidthMM; x += LabelBlockWidth)
                {

                    pnt1 = new PointF(x, 0);
                    pnt2 = new PointF(x, A4HeightMM);
                    util.DrawLine(pen, pnt1, pnt2);
                }
                //横ライン
                for (float y = startY; y < A4HeightMM; y += LabelBlockHeiht)
                {

                    pnt1 = new PointF(0, y);
                    pnt2 = new PointF(A4WidthMM, y);
                    util.DrawLine(pen, pnt1, pnt2);
                }


            }



            float drawX = startX;
            float drawY = startY;
            foreach ( var data in lstPrintData)
            {
                //印刷枚数
                for (int i = 0; i < data.numOfSheets; i++)
                {
                    DrawLabel(data, e.Graphics, drawX, drawY);

                    drawX += LabelBlockWidth;
                    if (drawX + LabelBlockWidth >= A4WidthMM)
                    {
                        drawY += LabelBlockHeiht;
                        drawX = startX;
                    }
                }

            }
        }
        private void DrawLabel( Graphics gPreview,  float gapLeft, float gapTop)
        {

            if (gridList.SelectedRows.Count <= 0) return;

            //現在選択されているグリッドのRowを取得
            var curRow = gridList.SelectedRows[0];
            EditProductData param = (EditProductData)curRow.Tag;

            DrawLabel(param, gPreview, gapTop, gapLeft);
        }
         
        private void DrawLabel(EditProductData param, Graphics gPreview, float gapLeft, float gapTop, bool bTestLineDraw=false)
        {

            Pen pen = new Pen(Color.Black, (float)0.1);
#if true
            //枠描画

            const float Line1Hight = 5;
            const float Line2Hight = 10;
            const float MaterialRowHight1 = 20;


            var commonDefStorage = commonDefInfo.GetCommonDefData("保存方法", param.storageMethod);
            var commonDefManifac = commonDefInfo.GetCommonDefData("製造者", param.manufacturer);

            var productData = productBaseInfo.GetProductDataByName(param.name);
            var today = DateTime.Now;
            DateTime dt = Utility.GetValidDate(productData.validDays);  

            //DrawUtil2 util = new DrawUtil2(gPreview, 2, 2);
            DrawUtil2 util = new DrawUtil2(gPreview, gapTop, gapLeft);

            //項目毎の基本フォントサイズ
            //範囲に入りきらない場合は、このフォントサイズから小さいフォントサイズに自動調整
            const float fontSizeProductTitle  = 8;
            const float fontSizMaterial       = 8;
            const float fontSizAmount         = 7;
            const float fontSizLimitDate      = 6;
            const float fontSizStorage        = 6;
            const float fontSizManifac        = 6;
            const float fontSizeComment       = 8;

            //ラベル描画処理
            float nextY = 0;
            nextY = util.DrawItem("名   称",  param.name,                 0,     Line1Hight,           fontSizeProductTitle);
            nextY = util.DrawItem("原材料名", productData.rawMaterials,   nextY, MaterialRowHight1,    fontSizMaterial);
            nextY = util.DrawItem("内 容 量", param.amount,               nextY, Line1Hight,           fontSizAmount);
            nextY = util.DrawItem("賞味期限", dt.ToLongDateString(),      nextY, Line1Hight,           fontSizLimitDate);
            nextY = util.DrawItem("保存方法", commonDefStorage.printText, nextY, Line2Hight,           fontSizStorage);
            nextY = util.DrawItem("製 造 者", commonDefManifac.printText, nextY, Line2Hight,           fontSizManifac);
            nextY = util.DrawItemComment("欄   外", productData.comment,  nextY,                       fontSizeComment, false);


          
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

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolBtnSave_Click(object sender, EventArgs e)
        {

            using (var sw = new StreamWriter(prevDataFilePath))
            {

                foreach (DataGridViewRow row in gridList.Rows)
                {
                    EditProductData data = (EditProductData)row.Tag;

                    sw.WriteLine(data.ToString());
                }
            }
        }

        private void ReadSavedPath()
        {
            if(!File.Exists(prevDataFilePath))
            {
                return;
            }
            using (var sr = new StreamReader(prevDataFilePath))
            {
                while(true)
                {
                    string s = sr.ReadLine();
                    if (s == null) break;

                    EditProductData data = new EditProductData(s);

                    EditParamToGridAdd(data, false);
                }
            }

            if( gridList.Rows.Count > 0 )
            {
                gridList.Rows[0].Selected = true;
                panelPreviw.Invalidate();


            }
        }

    }
}
