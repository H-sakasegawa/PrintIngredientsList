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


        private int MILLI2POINT(float milli)
        {
            return (int)(milli / 0.352777);
        }
        private float POINT2MILLI(int point)
        {
            return (float)(point * 0.352777);
        }

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

            return pd;
        }


        List<EditProductData> lstPrintData = new List<EditProductData>();
        int printDataIndex = 0;

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
                pen.DashStyle =  DashStyle.Dash;

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
            while(printDataIndex< lstPrintData.Count)
            {
                EditProductData data = lstPrintData[printDataIndex];

                //印刷枚数
                DrawLabel(data, e.Graphics, drawX, drawY);
                printDataIndex++;//次のラベル

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
#if true
            //枠描画

            const float Line1Hight = 4;
            const float Line2Hight = 6;
            const float MaterialRowHight1 = 20;


            var commonDefStorage = commonDefInfo.GetCommonDefData("保存方法", param.storageMethod);
            var commonDefManifac = commonDefInfo.GetCommonDefData("製造者", param.manufacturer);

            var productData = productBaseInfo.GetProductDataByName(param.name);
            DateTime dt = Utility.GetValidDate(param.validDays);

            //DrawUtil2 util = new DrawUtil2(gPreview, 2, 2);
            DrawUtil2 util = new DrawUtil2(gPreview, settingData, gapTop, gapLeft);


            //ラベル描画処理
            float nextY = 0;
            nextY = util.DrawItem("名    称", param.name,                 0,     Line1Hight,        settingData.fontSizeProductTitle);
            nextY = util.DrawItem("原材料名", productData.rawMaterials,   nextY, MaterialRowHight1, settingData.fontSizMaterial, true);
            nextY = util.DrawItem("内 容 量", param.amount,               nextY, Line1Hight,        settingData.fontSizAmount);
            nextY = util.DrawItem("賞味期限", dt.ToLongDateString(),      nextY, Line1Hight,        settingData.fontSizLimitDate);
            nextY = util.DrawItem("保存方法", commonDefStorage.printText, nextY, Line2Hight,        settingData.fontSizStorage);
            nextY = util.DrawItem("製 造 者", commonDefManifac.printText, nextY, Line2Hight,        settingData.fontSizManifac);
            nextY = util.DrawItemComment("", productData.comment,         nextY,                    settingData.fontSizeComment, false);


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
            if (!float.TryParse(txtFontMaterial.Text, out settingData.fontSizMaterial))
            {
                ErrMsg("原材料フォントサイズ");
            }
        }

        private void txtFontAmount_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontAmount.Text, out settingData.fontSizAmount))
            {
                ErrMsg("内容量フォントサイズ");
            }
        }

        private void txtFontValidDays_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontValidDays.Text, out settingData.fontSizLimitDate))
            {
                ErrMsg("賞味期限フォントサイズ");
            }
        }

        private void txtFontSotrage_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontSotrage.Text, out settingData.fontSizStorage))
            {
                ErrMsg("保存方法フォントサイズ");
            }
        }

        private void txtFontManifucture_TextAlignChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtFontManifucture.Text, out settingData.fontSizManifac))
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
    }

}
