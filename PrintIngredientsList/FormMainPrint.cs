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
using NPOI.XSSF.Streaming.Values;
using static NPOI.HSSF.Util.HSSFColor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Runtime.ConstrainedExecution;
using System.Data;
using System.Windows.Controls;

namespace PrintIngredientsList
{
    public partial class FormMain : Form
    {
        enum PrintType
        {
            PREVIEW,
            PRINT
        }

        /// <summary>
        /// グリッド列インデックス
        /// </summary>
        enum LabelItemColumnIndex
        {
            COL_CHECK = 0,
            COL_NAME,
            COL_HEIGHT,
            COL_FONT,
            COL_DETAIL
        }

        enum PictureItemColumnIndex
        {
            COL_CHECK = 0,
            COL_NAME,
            COL_POSX,
            COL_POSY,
            COL_WIDTH,
            COL_HEIGHT,
            COL_DETAIL
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


        //選択されている印刷用紙
        Layout curLayout = null;

        //選択されているラベルタイプ
        LabelType curLabelType = null;

        //プレビュー画面拡大率最大／最小
        double PreviewZoomMax = 3.0;
        double PreviewZoomMin = 0.1;

        /// <summary>
        /// 印刷ドキュメント情報作成
        /// </summary>
        /// <param name="printType"></param>
        /// <returns></returns>
        private PrintDocumentEx CreatePrintDocument(PrintType printType )
        {

            PrintDocumentEx pd = new PrintDocumentEx(printType);

            var ps = new System.Drawing.Printing.PrinterSettings();


            //A4用紙
            foreach (System.Drawing.Printing.PaperSize psize in pd.PrinterSettings.PaperSizes)
            {
                if (psize.Kind == curLayout.paperKind)
                {
                    pd.DefaultPageSettings.PaperSize = psize;
                    break;
                }
            }
            pd.DefaultPageSettings.Landscape = curLayout.landscape;

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

            //印刷プレビュー画面のホイール拡大縮小処理イベント
            ppd.MouseWheel += OnPrintPreviewDialog_MouseWheel;


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


        private void OnPrintPreviewDialog_MouseWheel(object sender, MouseEventArgs e)
        {
            PrintPreviewDialog ppd = (PrintPreviewDialog)sender;

            double delta = (e.Delta / 120)/100.0 * 10;

            double nowZoom = ppd.PrintPreviewControl.Zoom;
            if (nowZoom + delta > PreviewZoomMax) return;
            if (nowZoom + delta < PreviewZoomMin) return;


            ppd.PrintPreviewControl.Zoom += delta;
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

            float A4HeightMM = curLayout.paperHeight; //mm
            float A4WidthMM  = curLayout.paperWidth; //mm

            float startX = curLayout.printGapLeft;
            float startY = curLayout.printGapTop;

            float LabelBlockWidth = curLabelType.width;
            float LabelBlockHeiht = curLabelType.height;

            // float pageWidthMM = POINT2MILLI(e.PageSettings.PaperSize.Width);

            if (pd.printType == PrintType.PREVIEW && chkTestLineDraw.Checked)
            {
                DrawUtil2 util = new DrawUtil2(e.Graphics, curLabelType, settingData.fontName, false);
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
                DrawLabel(data, e.Graphics, curLabelType, drawX, drawY);
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
        private void DrawLabel(Graphics gPreview, LabelType labelType, float gapLeft, float gapTop)
        {

            if (gridList.SelectedRows.Count <= 0) return;

            //現在選択されているグリッドのRowを取得
            var curRow = gridList.SelectedRows[0];
            EditProductData param = (EditProductData)curRow.Tag;

            DrawLabel(param, gPreview, labelType,gapTop, gapLeft,false, true);
        }

        private void DrawLabel(EditProductData param, Graphics gPreview, LabelType labelType, float gapLeft, float gapTop, bool bTestLineDraw = false, bool bDrawLabelBackground=false)
        {

            Pen pen = new Pen(Color.Black, (float)0.1);

            var commonDefStorage = commonDefInfo.GetCommonDefData(CommonDeftReader.keyStorage, param.storageMethod);
            var commonDefManifac = commonDefInfo.GetCommonDefData(CommonDeftReader.keyManifacture, param.manufacturer);

            var productData = productBaseInfo.GetProductDataByID(param.id);
            DateTime dt = Utility.GetValidDate(param.validDays);

            //以下のバーコード情報有無チェックはBARCODEブロックが１つのみ定義されていることが
            //前提です。ループの中に入れると毎回イメージ作成が走るので、手前で１回作成するようにしてます。


            DrawUtil2 util = new DrawUtil2(gPreview, labelType, settingData.fontName, gapTop, gapLeft, bDrawLabelBackground);

            for (int iBlock = 0; iBlock < labelType.lstLabelBlocks.Count; iBlock++)
            {
                LabelTypeBlock labelBlock = labelType.lstLabelBlocks[iBlock];
                util.SetTargetLabelBlock(labelBlock);

                //ラベルブロックの描画位置を設定
                util.SetDrawLabelBlockOffset(labelBlock.posX, labelBlock.posY);

                float nextY = 0;
                for (int iItem = 0; iItem < labelBlock.lstLabelTypeBlocklItems.Count; iItem++)
                {
                    var labelItemBase = labelBlock.lstLabelTypeBlocklItems[iItem];
                    switch (labelItemBase.itemType)
                    {
                        case LabelTypeBlockItemBase.LabelTypeBlockItemType.LABEL:
                            {
                                LabelItem labelItem = (LabelItem)labelItemBase;
                                if (!labelItem.Visible) continue;

                                var name = labelItem.name; //項目キー名称
                                string dispValue = GetDispGridTypeValueByKey(name, param);


                                switch (labelItem.name)
                                {
                                    case ItemName.Comment:
                                        //描画領域幅は、タイトル幅＋値幅としてTITLEで指定された文字を表示
                                        nextY = util.DrawItemComment(labelItem.Title, nextY, labelItem.FontSize, labelItem.DrawFrame);
                                        break;
                                    case ItemName.Supplementary:
                                        nextY = util.DrawItemSupplementary("", dispValue, nextY, labelItem.FontSize, labelItem.DrawFrame);
                                        break;
                                    case ItemName.NutritionalInformation:
                                        nextY = util.DrawItem(labelItem.Title, dispValue, nextY, labelItem.Height, labelItem.FontSize, false, labelItem.DrawFrame); ;
                                        break;
                                    default:
                                        nextY = util.DrawItem(labelItem.Title, dispValue, nextY, labelItem.Height, labelItem.FontSize, false, labelItem.DrawFrame);
                                        break;
                                }
                            }
                            break;
                        case LabelTypeBlockItemBase.LabelTypeBlockItemType.BARCODE:
                            {
                                PictureItem pictureItem = (PictureItem)labelItemBase;

                                string TempFolder = System.IO.Path.Combine(GetExePath(), Const.BarcodeDataFolderName);
                                if (!System.IO.Directory.Exists(TempFolder))
                                {
                                    System.IO.Directory.CreateDirectory(TempFolder);
                                }
                                string barcodeFilePath = "";
                                if (pictureItem.DispNo)
                                {
                                    barcodeFilePath = System.IO.Path.Combine(TempFolder, $"{param.id}_withNo.png");
                                }else
                                {
                                    barcodeFilePath = System.IO.Path.Combine(TempFolder, $"{param.id}.png");
                                }
                                if (!System.IO.File.Exists(barcodeFilePath))
                                {
                                    //Janコード→バーコードイメージ作成
                                    Barcode barCode = new Barcode();
                                    barCode.CreateBarcode(param.id, pictureItem.DispNo, barcodeFilePath, System.Drawing.Imaging.ImageFormat.Png);
                                }
                                if (!pictureItem.Visible) continue;
                                util.DrawImage(barcodeFilePath, pictureItem.PosX, pictureItem.PosY, pictureItem.Width, pictureItem.Height);
                            }
                            break;
                        case LabelTypeBlockItemBase.LabelTypeBlockItemType.ICON:
                            {
                                PictureItem pictureItem = (PictureItem)labelItemBase;
                                if (!pictureItem.Visible) continue;

                                string iconFilePath = System.IO.Path.Combine(SettingsFolderPath, pictureItem.Image);
                                util.DrawImage(iconFilePath, pictureItem.PosX, pictureItem.PosY, pictureItem.Width, pictureItem.Height);
                            }
                            break;
                    }

                }
            }

          

        }

        /// <summary>
        /// BLOCK().TYPE=GRIDの項目の表示値取得
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        string GetDispGridTypeValueByKey(string keyName, EditProductData param)
        {
            var commonDefStorage = commonDefInfo.GetCommonDefData(CommonDeftReader.keyStorage, param.storageMethod);
            var commonDefManifac = commonDefInfo.GetCommonDefData(CommonDeftReader.keyManifacture, param.manufacturer);
            var productData = productBaseInfo.GetProductDataByID(param.id);
            DateTime dt = Utility.GetValidDate(param.validDays);

            switch (keyName)
            {
                case ItemName.Name:
                    return productData.name;
                case ItemName.Material:
                    return productData.rawMaterials;
                case ItemName.Amount:
                    return param.amount;
                case ItemName.ValidDate:
                    return dt.ToLongDateString();
                case ItemName.Storage:
                    return commonDefStorage.printText;
                case ItemName.Manifacture:
                    return commonDefManifac.printText;
                case ItemName.Allergy:
                    return productData.allergy;
                case ItemName.Supplementary:
                    return productData.comment;

                //成分表
                case ItemName.Calorie:
                    return productData.Calorie;
                case ItemName.Protein:
                    return productData.Protein;
                case ItemName.Lipids:
                    return productData.Lipids;
                case ItemName.Carbohydrates:
                    return productData.Carbohydrates;
                case ItemName.Salt:
                    return productData.Salt;

            }
            return "";
        }


        private void UpdateRow(DataGridViewRow row, LabelTypeBlockItemBase item)
        {
            switch (item.itemType)
            {
                case LabelTypeBlockItemBase.LabelTypeBlockItemType.LABEL:
                    {
                        LabelItem labelItem = (LabelItem)item;
                        row.Tag = item;

                        int idx = 0;
                        row.Cells[idx++].Value = labelItem.Visible;
                        row.Cells[idx++].Value = labelItem.name;
                        row.Cells[idx++].Value = labelItem.Height;
                        row.Cells[idx++].Value = labelItem.FontSize;
                        row.Cells[idx++].Value = "...";

                        if (labelItem.Height == 0)
                        {
                            row.Cells[2].ReadOnly = true;
                            row.Cells[2].Value = "";
                            row.Cells[2].Style.BackColor = Color.LightGray;
                        }
                    }
                    break;
                case LabelTypeBlockItemBase.LabelTypeBlockItemType.BARCODE:
                case LabelTypeBlockItemBase.LabelTypeBlockItemType.ICON:
                    {
                        PictureItem picItem = (PictureItem)item;

                        row.Tag = item;
                        int idx = 0;
                        row.Cells[idx++].Value = picItem.Visible;
                        row.Cells[idx++].Value = picItem.name;
                        row.Cells[idx++].Value = picItem.PosX;
                        row.Cells[idx++].Value = picItem.PosY;
                        row.Cells[idx++].Value = picItem.Width;
                        row.Cells[idx++].Value = picItem.Height;
                        row.Cells[idx++].Value = "...";
                    }
                    break;
            }
        }


        /// <summary>
        /// ラベルタイププレビューウィンドウ更新
        /// </summary>
        void UpdateLabelTypePreview()
        {
            panelPrintTypePreview.Invalidate();
        }
        private void panelPrintTypePreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics gPreview = e.Graphics;

            //選択されているラベルタイプ
            var labelType = (LabelType)cmbLabelType.SelectedItem;

            Preview(gPreview, panelPrintTypePreview, labelType,(float)1);

        }



        bool IsLabelTypeBlockType(LabelTypeBlockBase.LabelTypeBlockType type)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            return labelBlock.labelTypeBlockType == type ? true : false;
        }

    }

}
