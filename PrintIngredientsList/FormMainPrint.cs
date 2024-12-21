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
        public enum PrintType
        {
            PREVIEW,
            PRINT
        }

        /// <summary>
        /// グリッド列インデックス
        /// </summary>
        public enum LabelItemColumnIndex
        {
            COL_CHECK = 0,
            COL_NAME,
            COL_HEIGHT,
            COL_FONT,
            COL_DETAIL
        }

        public enum PictureItemColumnIndex
        {
            COL_CHECK = 0,
            COL_NAME,
            COL_POSX,
            COL_POSY,
            COL_WIDTH,
            COL_HEIGHT,
            COL_DETAIL
        }
        /// <summary>
        /// 印刷ドキュメント拡張クラス
        /// </summary>
        public class PrintDocumentEx : System.Drawing.Printing.PrintDocument
        {
            public PrintDocumentEx(PrintType type)
                : base()
            {
                printType = type;
            }
            public PrintType printType;
            public int printDataIndex;
            public　int curPrintPageNo = 0;

            public void ResetPageIndex() 
            { 
                printDataIndex = 0;
                curPrintPageNo = 0;
            }

        }

        /// <summary>
        /// ヘッダに表示する商品と印刷ラベル数情報管理クラス
        /// </summary>
        class PrintedProducts
        {
            public PrintedProducts(string name)
            {
                this.name = name;
            }
            public string name;
            public int printNum;
        }

        List<EditProductData> lstPrintData = new List<EditProductData>();
        //int printDataIndex = 0;
        bool bPrintStartPositionning = false;


        //選択されている印刷用紙
        Layout curLayout = null;

        //選択されているラベルタイプ
        LabelType curLabelType = null;

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



        
        private void CreatePrintData(bool b1peace=false)
        {
            lstPrintData.Clear();
            //printDataIndex = 0; //印刷ラベルIndex初期化

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

                    int num = data.numOfSheets;
                    if(b1peace)
                    {
                        num = 1;//強制１枚印刷
                    }

                    for (int i = 0; i < num; i++)
                    {
                        lstPrintData.Add(data);
                    }
                }
            }
        }



        private void pd_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            PrintDocumentEx pd = (PrintDocumentEx)sender;
            pd.curPrintPageNo++;

            float A4HeightMM = curLayout.paperHeight; //mm
            float A4WidthMM  = curLayout.paperWidth; //mm

            float startX = curLayout.PrintGapLeft;
            float startY = curLayout.PrintGapTop;

            float LabelBlockWidth = curLabelType.Width;
            float LabelBlockHeiht = curLabelType.Height;

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
                while (pd.printDataIndex < lstPrintData.Count)
                {
                    EditProductData data = lstPrintData[pd.printDataIndex];
                    var productData = productBaseInfo.GetProductDataByID(data.id);

                    if (curPrintProduct == null || string.Compare(curPrintProduct.name, productData.name, true) != 0)
                    {
                        curPrintProduct = new PrintedProducts(productData.name);
                        printHeaderProductNames.Add(curPrintProduct);
                    }
                    //印刷される製品の切り替わり位置ライン描画フラグ
                    bool bDrawProductSepLine = false;
                    if(curPrintProduct.printNum==1)
                    {
                       bDrawProductSepLine = true;
                    }


                    if (pd.printDataIndex < lstPrintData.Count)
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
                            DrawLabel(data, e.Graphics, bDrawProductSepLine, curLabelType, drawX, drawY);
                            pd.printDataIndex++;//次のラベル
                            //現在の製品のの印刷ラベル枚数カウント
                            curPrintProduct.printNum++;

                            drawX += LabelBlockWidth;
                        }
                    }
                }

                e.HasMorePages = false;
            }finally
            {
                //ヘッダにこのページに印刷されている商品名とラベル枚数を表示
                DrawHeader(printHeaderProductNames, e.Graphics, curLayout.PrintGapLeft, curLayout.PrintGapTop);
                DrawFooter(pd.curPrintPageNo, pageNum, e.Graphics, A4WidthMM, A4HeightMM);
            }
        }
        /// <summary>
        /// 印刷ページ数カウント
        /// </summary>
        /// <returns></returns>
        public int GetPageNum()
        {
            float A4HeightMM = curLayout.paperHeight; //mm
            float A4WidthMM = curLayout.paperWidth; //mm

            float startX = curLayout.PrintGapLeft;
            float startY = curLayout.PrintGapTop;

            float LabelBlockWidth = curLabelType.Width;
            float LabelBlockHeiht = curLabelType.Height;

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
        /// <summary>
        /// プレビュー表示用
        /// </summary>
        /// <param name="gPreview"></param>
        /// <param name="labelType"></param>
        /// <param name="gapLeft"></param>
        /// <param name="gapTop"></param>
        private void DrawLabel(Graphics gPreview, LabelType labelType, float gapLeft, float gapTop)
        {

            if (gridList.SelectedRows.Count <= 0) return;

            //現在選択されているグリッドのRowを取得
            var curRow = gridList.SelectedRows[0];
            EditProductData param = (EditProductData)curRow.Tag;

            DrawLabel(param, gPreview, false,labelType, gapTop, gapLeft, false, true);
        }
        //印刷、印刷プレビュー用
        private void DrawLabel(EditProductData param, Graphics gPreview, bool bDrawProductSepLine, LabelType labelType, float gapLeft, float gapTop, bool bTestLineDraw = false, bool bDrawLabelBackground = false)
        {

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
                util.SetDrawLabelBlockOffset(labelBlock.PosX, labelBlock.PosY);

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
                                    case ItemName.Comment: //コメント
                                        //描画領域幅は、タイトル幅＋値幅としてTITLEで指定された文字を表示
                                        nextY = util.DrawItemComment(labelItem.Title, nextY, labelItem.FontSize, labelItem.DrawFrame);
                                        break;
                                    case ItemName.Supplementary://欄外
                                        {
                                            bool bResult = true;
                                            nextY = util.DrawItemSupplementary("", dispValue, nextY, labelItem.FontSize, labelItem.ValueWidth, ref bResult, labelItem.DrawFrame);
                                            if (bResult == false)
                                            {
                                                Utility.MessageError($"{productData.name}の欄外表示がはみ出している可能性があります。");
                                            }
                                        }
                                        break;
                                    case ItemName.NutritionalInformation: //栄養成分表示
                                        nextY = util.DrawItem(labelItem.Title, dispValue, nextY, labelItem.Height, labelItem.FontSize, false, labelItem.DrawFrame);
                                        break;
                                    default:

                                        //nextY = util.DrawItem(labelItem.Title, dispValue, nextY, labelItem.Height, labelItem.FontSize, false, labelItem.DrawFrame);
                                        nextY = util.DrawItem(nextY, dispValue, labelItem);
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

            if(bDrawProductSepLine)
            {
                float x = gapLeft;
                Pen pen = new Pen(Color.Black, (float)0.3);
                PointF pnt1 = new PointF(x, gapTop);
                PointF pnt2 = new PointF(x, gapTop +1);
                util.DrawLine(pen, pnt1, pnt2);
            }


        }
        void DrawHeader(List<PrintedProducts> printHeaderProductNames, Graphics gPreview, float gapLeft, float gapTop)
        {
            DrawUtil2 util = new DrawUtil2(gPreview, curLabelType, settingData.fontName);

            float maxWidth = 0;
            float nextY = 0;
            float x = curLayout.HeaderGapLeft;
            float y = curLayout.HeaderGapTop;
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
                        y = curLayout.HeaderGapTop;
                    }
                } while (rc != 0);

                maxWidth = Math.Max(maxWidth, width);
                y = nextY;
            }
        }

        void DrawFooter(int curPageNo, int pageNum, Graphics gPreview, float pageWidth, float pageHeight)
        {
            DrawUtil2 util = new DrawUtil2(gPreview, curLabelType, settingData.fontName);
            util.DrawFooter(curPageNo, pageNum, pageWidth, pageHeight);
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

        //private void panelPrintTypePreview_Paint(object sender, PaintEventArgs e)
        //{
        //    Graphics gPreview = e.Graphics;

        //    //選択されているラベルタイプ
        //    var labelType = (LabelType)cmbLabelType.SelectedItem;

        //    //Preview(gPreview, panelPrintTypePreview, labelType,(float)1);

        //}
        bool IsLabelTypeBlockType(LabelTypeBlockBase.LabelTypeBlockType type)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            return labelBlock.labelTypeBlockType == type ? true : false;
        }

    }

}
