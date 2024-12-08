using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using NPOI.SS.Formula.Functions;
using ExcelReaderUtility;

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

        List<EditProductData> lstPrintData = new List<EditProductData>();
        int printDataIndex = 0;
        bool bPrintStartPositionning = false;


        /// <summary>
        /// 印刷ドキュメント情報作成
        /// </summary>
        /// <param name="printType"></param>
        /// <returns></returns>
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

            //印刷開始位置調整済みフラグをリセット
            bPrintStartPositionning = false;
            return pd;
        }

        /// <summary>
        /// プレビュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {

            CreatePrintData();

            PrintDocumentEx pd = CreatePrintDocument(PrintType.PREVIEW);

            //PrintPreviewDialogオブジェクトの作成
            PrintPreviewDialog ppd = new PrintPreviewDialog();
            //プレビューするPrintDocumentを設定

            int x = Properties.Settings.Default.PrintPreviewDlgLocX;
            int y = Properties.Settings.Default.PrintPreviewDlgLocY;
            int w = Properties.Settings.Default.PrintPreviewDlgSizeW;
            int h = Properties.Settings.Default.PrintPreviewDlgSizeH;
            ppd.SetBounds( x, y, w, h);

            ppd.PrintPreviewControl.Zoom = Properties.Settings.Default.PrintPreviewDlgZoom;

            ppd.Document = pd;
            //印刷プレビューダイアログを表示する
            ppd.ShowDialog();

            //現在位置とサイズを記録
            Properties.Settings.Default.PrintPreviewDlgLocX = ppd.Bounds.Left;
            Properties.Settings.Default.PrintPreviewDlgLocY = ppd.Bounds.Top;
            Properties.Settings.Default.PrintPreviewDlgSizeW = ppd.Bounds.Width;
            Properties.Settings.Default.PrintPreviewDlgSizeH = ppd.Bounds.Height;
            Properties.Settings.Default.PrintPreviewDlgZoom = ppd.PrintPreviewControl.Zoom;

        }
        /// <summary>
        /// 印刷
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            CreatePrintData();

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
        private void CreatePrintData()
        {
            lstPrintData.Clear();
            printDataIndex = 0; //印刷ラベルIndex初期化

            //セット枚数
            for (int iCopy = 0; iCopy < settingData.copyNum; iCopy++)
            {
                //印刷データ収集
                foreach (DataGridViewRow row in gridList.Rows)
                {
                    if (radPrintSelect.Checked)
                    {
                        if ((bool)row.Cells[0].Value == false)
                        {
                            continue;
                        }
                    }
                    EditProductData data = (EditProductData)row.Tag;

                    for (int i = 0; i < data.numOfSheets; i++)
                    {
                        lstPrintData.Add(data);
                    }
                }
            }
        }



        private void pd_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            PrintDocumentEx pd = (PrintDocumentEx)sender;

            float A4HeightMM = 297; //mm
            float A4WidthMM = 210; //mm

            float startX = settingData.PrintLeftGap;
            float startY = settingData.PrintTopGap;

            float LabelBlockWidth = settingData.LabelAreaWidth;
            float LabelBlockHeiht = settingData.LabelAreaHeight;

            // float pageWidthMM = POINT2MILLI(e.PageSettings.PaperSize.Width);

            if (pd.printType == PrintType.PREVIEW && chkTestLineDraw.Checked)
            {
                DrawUtil2 util = new DrawUtil2(e.Graphics, settingData);
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

            if (bPrintStartPositionning == false)
            {
                //印刷開始位置の1つ手前まで座標を進める（１ページ目のみ）
                for (int iPos = 0; iPos < settingData.printStartPos - 1; iPos++)
                {
                    drawX += LabelBlockWidth;
                    if (drawX + LabelBlockWidth >= A4WidthMM)
                    {
                        drawY += LabelBlockHeiht;
                        drawX = startX;
                    }

                }
                //印刷開始位置調整済みフラグセット
                bPrintStartPositionning = true;
            }
            while (printDataIndex< lstPrintData.Count)
            {
                EditProductData data = lstPrintData[printDataIndex];

                //印刷枚数
                DrawLabel(data, e.Graphics, drawX, drawY);
                printDataIndex++;//次のラベル

                if( printDataIndex< lstPrintData.Count)
                {
                    
                    drawX += LabelBlockWidth;
                    if (drawX + LabelBlockWidth >= A4WidthMM)
                    {
                        drawY += LabelBlockHeiht;
                        drawX = startX;

                        if( drawY + LabelBlockHeiht > A4HeightMM)
                        {
                            //改ページ
                            e.HasMorePages = true;
                            return;
                        }
                    }
                }
            }
            e.HasMorePages = false;
        }
        private void DrawLabel(Graphics gPreview, float gapLeft, float gapTop)
        {

            if (gridList.SelectedRows.Count <= 0) return;

            //現在選択されているグリッドのRowを取得
            var curRow = gridList.SelectedRows[0];
            EditProductData param = (EditProductData)curRow.Tag;

            DrawLabel(param, gPreview, gapTop, gapLeft);
        }

        private void DrawLabel(EditProductData param, Graphics gPreview, float gapLeft, float gapTop, bool bTestLineDraw = false)
        {

            Pen pen = new Pen(Color.Black, (float)0.1);

            var commonDefStorage = commonDefInfo.GetCommonDefData(CommonDeftReader.keyStorage, param.storageMethod);
            var commonDefManifac = commonDefInfo.GetCommonDefData(CommonDeftReader.keyManifacture, param.manufacturer);

            var productData = productBaseInfo.GetProductDataByID(param.id);
            DateTime dt = Utility.GetValidDate(param.validDays);

            //DrawUtil2 util = new DrawUtil2(gPreview, 2, 2);
            DrawUtil2 util = new DrawUtil2(gPreview, settingData, gapTop, gapLeft);


            //ラベル描画処理
            float nextY = 0;
            nextY = util.DrawItem("名    称", productData.name,           0,     settingData.hightProductTitle,  settingData.fontSizeProductTitle);
            nextY = util.DrawItem("原材料名", productData.rawMaterials,   nextY, settingData.hightMaterial,      settingData.fontSizeMaterial, true);
            nextY = util.DrawItem("内 容 量", param.amount,               nextY, settingData.hightAmount,        settingData.fontSizeAmount);
            nextY = util.DrawItem("賞味期限", dt.ToLongDateString(),      nextY, settingData.hightAmount,        settingData.fontSizeLimitDate);
            nextY = util.DrawItem("保存方法", commonDefStorage.printText, nextY, settingData.hightStorage,       settingData.fontSizeStorage);
            nextY = util.DrawItem("製 造 者", commonDefManifac.printText, nextY, settingData.hightManifac,       settingData.fontSizeManifac);
            nextY = util.DrawItemComment("", productData.comment,         nextY,                                 settingData.fontSizeComment, false);

        }

        //フォント選択
        private void cmbFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            settingData.fontName = cmbFont.Text;
            panelPreviw.Invalidate();
        }

        private void txtFontProductTitle_TextChanged(object sender, EventArgs e)
        {
            if( !float.TryParse(txtFontProductTitle.Text, out settingData.fontSizeProductTitle))
            {
                ErrMsg("名称フォントサイズ");
            }
        }

        private void txtFontMaterial_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontMaterial.Text, out settingData.fontSizeMaterial))
            {
                ErrMsg("原材料フォントサイズ");
            }
        }

        private void txtFontAmount_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontAmount.Text, out settingData.fontSizeAmount))
            {
                ErrMsg("内容量フォントサイズ");
            }
        }

        private void txtFontValidDays_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontValidDays.Text, out settingData.fontSizeLimitDate))
            {
                ErrMsg("賞味期限フォントサイズ");
            }
        }

        private void txtFontSotrage_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontSotrage.Text, out settingData.fontSizeStorage))
            {
                ErrMsg("保存方法フォントサイズ");
            }
        }

        private void txtFontManifucture_TextAlignChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontManifucture.Text, out settingData.fontSizeManifac))
            {
                ErrMsg("製造者フォントサイズ");
            }
        }


        private void txtFontComment_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontComment.Text, out settingData.fontSizeComment))
            {
                ErrMsg("欄外フォントサイズ");
            }
        }
        private void txtFontTitle_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontTitle.Text, out settingData.fontSizeTitle))
            {
                ErrMsg("タイトル列フォントサイズ");
            }
        }

        private void txtHightProductTitle_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtHightProductTitle.Text, out settingData.hightProductTitle))
            {
                ErrMsg("名称 高さ");
            }
        }

        private void txtHightMaterial_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtHightMaterial.Text, out settingData.hightMaterial))
            {
                ErrMsg("原材料 高さ");
            }
        }

        private void txtHightAmount_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtHightAmount.Text, out settingData.hightAmount))
            {
                ErrMsg("内容量 高さ");
            }
        }

        private void txtHightValidDays_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtHightValidDays.Text, out settingData.hightLimitDate))
            {
                ErrMsg("賞味期限 高さ");
            }
        }

        private void txtHightSotrage_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtHightSotrage.Text, out settingData.hightStorage))
            {
                ErrMsg("製造者 高さ");
            }
        }

        private void txtHightManifucture_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtHightManifucture.Text, out settingData.hightManifac))
            {
                ErrMsg("欄外 高さ");
            }
        }
        private void txtWidthTitle_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtWidthTitle.Text, out settingData.TitleAreWidthMM))
            {
                ErrMsg("タイトル列 幅");
            }
        }

    }

}
