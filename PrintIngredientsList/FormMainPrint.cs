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
using NPOI.Util;
using static ExcelReaderUtility.ProductReader;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;
using System.Reflection;
using System.Runtime.InteropServices;

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
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        public const int WM_HSCROLL = 0x00000114;
        public const int WM_VSCROLL = 0x00000115;
        private const int SB_LINEUP = 0;          //上矢印を押した
        private const int SB_LINEDOWN = 1;          //下矢印を押した

        List<EditProductData> lstPrintData = new List<EditProductData>();
        int printDataIndex = 0;
        bool bPrintStartPositionning = false;

        //プレビュー画面拡大率最大／最小
        double PreviewZoomMax = 3.0;
        double PreviewZoomMin = 0.1;


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

        protected void OnMouseWheel(Object sender, MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            PrintPreviewControl ppc = (PrintPreviewControl)sender;

            //if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                //スクロールでズーム値変更する
                //増減量・方向はお好みで
                if (e.Delta > 0)
                {
                    ppc.Zoom -= ppc.Zoom * 0.1;
                }
                else
                {
                    ppc.Zoom += ppc.Zoom * 0.1;
                }
            }
            //else
            //{
            //    SendMessage(ppc.Parent.Handle, WM_VSCROLL, new IntPtr(SB_LINEDOWN), new IntPtr(100));
            //    var pos = ppc.AutoScrollOffset;
            //}

            //外部通知用にイベントを呼ぶ
            // this.ZoomChanged?.Invoke(this, EventArgs.Empty);
        }
        protected void OnMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            //ドラッグ中だった場合
            if (e.Button == MouseButtons.Left)
            {
            }
            else
            {
            }

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


            //印刷プレビュー画面のホイール拡大縮小処理イベント
            var ppc = PrintPreviewDlg.PrintPreviewControl;

            ppc.MouseWheel += OnMouseWheel;
            ppc.MouseMove += OnMouseMove;

            //プレビューするPrintDocumentを設定
            int x = Properties.Settings.Default.PrintPreviewDlgLocX;
            int y = Properties.Settings.Default.PrintPreviewDlgLocY;
            int w = Properties.Settings.Default.PrintPreviewDlgSizeW;
            int h = Properties.Settings.Default.PrintPreviewDlgSizeH;
            PrintPreviewDlg.SetBounds( x, y, w, h);

            PrintPreviewDlg.PrintPreviewControl.Zoom = Properties.Settings.Default.PrintPreviewDlgZoom;

            PrintPreviewDlg.Document = pd;
            //印刷プレビューダイアログを表示する
            PrintPreviewDlg.ShowDialog();

            //現在位置とサイズを記録
            Properties.Settings.Default.PrintPreviewDlgLocX = PrintPreviewDlg.Bounds.Left;
            Properties.Settings.Default.PrintPreviewDlgLocY = PrintPreviewDlg.Bounds.Top;
            Properties.Settings.Default.PrintPreviewDlgSizeW = PrintPreviewDlg.Bounds.Width;
            Properties.Settings.Default.PrintPreviewDlgSizeH = PrintPreviewDlg.Bounds.Height;
            Properties.Settings.Default.PrintPreviewDlgZoom = PrintPreviewDlg.PrintPreviewControl.Zoom;

        }
        /// <summary>
        /// ;ホイール操作(拡大、スクロール）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPrintPreviewDialog_MouseWheel(object sender, MouseEventArgs e)
        {
            PrintPreviewDialog ppd = (PrintPreviewDialog)sender;

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                double delta = (e.Delta / 120) / 100.0 * 10;

                double nowZoom = ppd.PrintPreviewControl.Zoom;
                if (nowZoom + delta > PreviewZoomMax) return;
                if (nowZoom + delta < PreviewZoomMin) return;


                ppd.PrintPreviewControl.Zoom += delta;
            }else
            {
                var pos = ppd.AutoScrollPosition;

                var vs = ppd.VerticalScroll;

                vs.Value += (-e.Delta / 120) * 10;
            }
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

        int curPrintPageNo = 0;
        
        private void CreatePrintData()
        {
            lstPrintData.Clear();
            printDataIndex = 0; //印刷ラベルIndex初期化
            curPrintPageNo = 0;

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

        class PrintedProducts
        {
            public PrintedProducts(string name)
            {
                this.name = name;
            }
            public string name;
            public int printNum;
        }


        private void pd_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            curPrintPageNo++;

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

            int pageNum = GetPageNum();

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

            List<PrintedProducts> printHeaderProductNames = new List<PrintedProducts>();
            try
            {
                PrintedProducts curPrintProduct = null;
                while (printDataIndex < lstPrintData.Count)
                {
                    EditProductData data = lstPrintData[printDataIndex];
                    var productData = productBaseInfo.GetProductDataByID(data.id);

                    if (curPrintProduct == null || string.Compare(curPrintProduct.name, productData.name, true) != 0)
                    {
                        curPrintProduct = new PrintedProducts(productData.name);
                        printHeaderProductNames.Add(curPrintProduct);
                    }
                    //現在の製品のの印刷ラベル枚数カウント
                    curPrintProduct.printNum++;
                    //印刷される製品の切り替わり位置ライン描画フラグ
                    bool bDrawProductSepLine = false;
                    if(curPrintProduct.printNum==1)
                    {
                       bDrawProductSepLine = true;
                    }

                    if (printDataIndex < lstPrintData.Count)
                    {

                        if (drawX + LabelBlockWidth >= A4WidthMM)
                        {
                            drawY += LabelBlockHeiht;
                            drawX = startX;

                            if (drawY + LabelBlockHeiht > A4HeightMM)
                            {
                                //改ページ
                                e.HasMorePages = true;
                                return;
                            }
                        }
                        else
                        {
                            //印刷枚数
                            DrawLabel(data, e.Graphics, bDrawProductSepLine, drawX, drawY);
                            printDataIndex++;//次のラベル

                            drawX += LabelBlockWidth;
                        }
                    }
                }

                e.HasMorePages = false;
            }finally
            {
                //ヘッダにこのページに印刷されている商品名とラベル枚数を表示
                DrawHeader(printHeaderProductNames, e.Graphics, settingData.PrintLeftGap, settingData.PrintTopGap);
                DrawFooter(curPrintPageNo, pageNum, e.Graphics, A4WidthMM, A4HeightMM);
            }
        }
        /// <summary>
        /// 印刷ページ数カウント
        /// </summary>
        /// <returns></returns>
        private int GetPageNum()
        {
            float A4HeightMM = 297; //mm
            float A4WidthMM = 210; //mm

            float startX = settingData.PrintLeftGap;
            float startY = settingData.PrintTopGap;

            float LabelBlockWidth = settingData.LabelAreaWidth;
            float LabelBlockHeiht = settingData.LabelAreaHeight;

            float drawX = startX;
            float drawY = startY;


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


            int pageCount = 1;
            int index = 0;
            while (index < lstPrintData.Count)
            {
                if (drawX + LabelBlockWidth >= A4WidthMM)
                {
                    drawY += LabelBlockHeiht;
                    drawX = startX;

                    if (drawY + LabelBlockHeiht > A4HeightMM)
                    {
                        //改ページ
                        pageCount++;
                        drawX = startX;
                        drawY = startY;
                    }
                    continue;
                }
                drawX += LabelBlockWidth;
                index++;
            }
            return pageCount;

        }

        private void DrawLabel(Graphics gPreview, bool bDrawProductSepLine,float gapLeft, float gapTop)
        {

            if (gridList.SelectedRows.Count <= 0) return;

            //現在選択されているグリッドのRowを取得
            var curRow = gridList.SelectedRows[0];
            EditProductData param = (EditProductData)curRow.Tag;

            DrawLabel(param, gPreview, false, gapTop, gapLeft);
        }

        private void DrawLabel(EditProductData param, Graphics gPreview, bool bDrawProductSepLine, float gapLeft, float gapTop, bool bTestLineDraw = false)
        {

            var commonDefStorage = commonDefInfo.GetCommonDefData(CommonDeftReader.keyStorage, param.storageMethod);
            var commonDefManifac = commonDefInfo.GetCommonDefData(CommonDeftReader.keyManifacture, param.manufacturer);

            var productData = productBaseInfo.GetProductDataByID(param.id);
            DateTime dt = Utility.GetValidDate(param.validDays);

            //DrawUtil2 util = new DrawUtil2(gPreview, 2, 2);
            DrawUtil2 util = new DrawUtil2(gPreview, settingData, gapTop, gapLeft);


            bool bResult = true;
            //ラベル描画処理
            float nextY = 0;
            nextY = util.DrawItem("名    称", productData.name,           0,     settingData.hightProductTitle,  settingData.fontSizeProductTitle);
            nextY = util.DrawItem("原材料名", productData.rawMaterials,   nextY, settingData.hightMaterial,      settingData.fontSizeMaterial, true);
            nextY = util.DrawItem("内 容 量", param.amount,               nextY, settingData.hightAmount,        settingData.fontSizeAmount);
            nextY = util.DrawItem("賞味期限", dt.ToLongDateString(),      nextY, settingData.hightLimitDate,        settingData.fontSizeLimitDate);
            nextY = util.DrawItem("保存方法", commonDefStorage.printText, nextY, settingData.hightStorage,       settingData.fontSizeStorage);
            nextY = util.DrawItem("製 造 者", commonDefManifac.printText, nextY, settingData.hightManifac,       settingData.fontSizeManifac);
            nextY = util.DrawItemComment("", productData.comment,         nextY,                                 settingData.fontSizeComment, ref bResult,false);

            if(bDrawProductSepLine)
            {
                float x = gapLeft;
                Pen pen = new Pen(Color.Black, (float)0.3);
                PointF pnt1 = new PointF(x, gapTop);
                PointF pnt2 = new PointF(x, gapTop +1);
                util.DrawLine(pen, pnt1, pnt2);
            }

            if(bResult==false)
            {
                Utility.MessageError($"{productData.name}の欄外表示がはみ出している可能性があります。");
            }
        }

        void DrawHeader(List<PrintedProducts> printHeaderProductNames, Graphics gPreview, float gapLeft, float gapTop)
        {
            DrawUtil2 util = new DrawUtil2(gPreview, settingData, gapTop, gapLeft);

            float maxWidth = 0;
            float nextY = 0;
            float x = settingData.HeaderPrintLeftGap;
            float y = settingData.HeaderPrintTopGap;
            float width = 0;

            foreach (var prd in printHeaderProductNames)
            {
                int rc = 0;
                do
                {
                    rc = util.DrawHeader(prd.name, prd.printNum, x, y, gapTop, ref nextY, ref width);
                    if (rc != 0)
                    {
                        //ヘッダ領域オーバー
                        x += maxWidth+2;
                        y = settingData.HeaderPrintTopGap;
                    }
                } while (rc != 0);

                maxWidth = Math.Max(maxWidth, width);
                y = nextY;
            }
        }

        void DrawFooter(int curPageNo, int pageNum, Graphics gPreview, float pageWidth, float pageHeight)
        {
            DrawUtil2 util = new DrawUtil2(gPreview, settingData, 0, 0);
            util.DrawFooter(curPageNo, pageNum, pageWidth, pageHeight);
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
