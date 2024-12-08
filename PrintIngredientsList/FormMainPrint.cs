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

            float A4HeightMM = curLayout.paperHeight; //mm
            float A4WidthMM  = curLayout.paperWidth; //mm

            float startX = curLayout.printGapLeft;
            float startY = curLayout.printGapTop;

            float LabelBlockWidth = curLabelType.width;
            float LabelBlockHeiht = curLabelType.height;

            // float pageWidthMM = POINT2MILLI(e.PageSettings.PaperSize.Width);

            if (pd.printType == PrintType.PREVIEW && chkTestLineDraw.Checked)
            {
                DrawUtil2 util = new DrawUtil2(e.Graphics, curLabelType, false);
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

            string barcodeFilePath = "";
            if (labelType.IsExistImageBlock())
            {
                //Janコード→バーコードイメージ作成
                Barcode barCode = new Barcode();

                string TempFolder = System.IO.Path.Combine(GetExePath(), "Temp");
                if (!System.IO.Directory.Exists(TempFolder))
                {
                    System.IO.Directory.CreateDirectory(TempFolder);
                }
                barcodeFilePath = System.IO.Path.Combine(TempFolder, "barcode.png");
                barCode.CreateBarcode(param.id, barcodeFilePath, System.Drawing.Imaging.ImageFormat.Png);
            }


            DrawUtil2 util = new DrawUtil2(gPreview, labelType, gapTop, gapLeft, bDrawLabelBackground);

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
                                        nextY = util.DrawItemComment("", dispValue, nextY, labelItem.FontSize, labelItem.DrawFrame);
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
                    return param.name;
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
                case ItemName.Comment:
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



        //フォント選択
        private void cmbFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            curLabelType.fontName = cmbFont.Text;
            panelPreviw.Invalidate();
        }
        /// <summary>
        /// 用紙選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            curLayout = (Layout)cmbLayout.SelectedItem;

            lblSize.Text            = $"{curLayout.paperWidth} × {curLayout.paperHeight}";
            txtPrintLeftGap.Text    = curLayout.printGapLeft.ToString("F2");
            txtPrintTopGap.Text     = curLayout.printGapTop.ToString("F2");

        }

        /// <summary>
        /// ラベルタイプ選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbLabelType_SelectedIndexChanged(object sender, EventArgs e)
        {
            curLabelType = (LabelType)cmbLabelType.SelectedItem;

            txtLabelAreaGapLeft.Text     = curLabelType.gapLeft.ToString("F2");
            txtLabelAreaGapTop.Text     = curLabelType.gapTop.ToString("F2");
            txtLabelAreaGapRight.Text   = curLabelType.gapRight.ToString("F2");
            txtLabelAreaGapBottom.Text  = curLabelType.gapBottom.ToString("F2");

            cmbLabelBlock.Items.Clear();
            foreach (var ItemBlock in curLabelType.lstLabelBlocks)
            {
                cmbLabelBlock.Items.Add(ItemBlock);
            }
            if (cmbLabelBlock.Items.Count > 0)
            {
                cmbLabelBlock.SelectedIndex = 0;
            }
            UpdateLabelTypePreview();
        }

        //ラベルブロック選択
        private void cmbLabelBlock_SelectedIndexChanged(object sender, EventArgs e)
        {



            grdLabelBlockItems.CellValueChanged -= grdLabelBlockItems_CellValueChanged;
            {
                grdLabelBlockItems.Rows.Clear();

                LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
                if (labelBlock == null) return;

                txtPosX.Text = labelBlock.posX.ToString();
                txtPosY.Text = labelBlock.posY.ToString();

                //データグリッドビューのヘッダを更新
                grdLabelBlockItems.Columns.Clear();
                if ( labelBlock.labelTypeBlockType == LabelTypeBlockBase.LabelTypeBlockType.GRID)
                {
                    DataGridViewCheckBoxColumn chkColumn = new DataGridViewCheckBoxColumn();
                    chkColumn.HeaderText = "表示";
                    var colIndex = grdLabelBlockItems.Columns.Add(chkColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 37;

                    DataGridViewTextBoxColumn txtColumn = new DataGridViewTextBoxColumn();
                    txtColumn.HeaderText = "項目名";
                    colIndex = grdLabelBlockItems.Columns.Add(txtColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 70;

                    txtColumn = new DataGridViewTextBoxColumn();
                    txtColumn.HeaderText = "高さ";
                    colIndex = grdLabelBlockItems.Columns.Add(txtColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 40;

                    txtColumn = new DataGridViewTextBoxColumn();
                    txtColumn.HeaderText = "フォント";
                    colIndex = grdLabelBlockItems.Columns.Add(txtColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 50;

                    DataGridViewButtonColumn btnColumn = new DataGridViewButtonColumn();
                    btnColumn.HeaderText = "詳細";
                    colIndex = grdLabelBlockItems.Columns.Add(btnColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 40;
                }
                else
                {
                    DataGridViewCheckBoxColumn chkColumn = new DataGridViewCheckBoxColumn();
                    chkColumn.HeaderText = "表示";
                    var colIndex = grdLabelBlockItems.Columns.Add(chkColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 37;

                    DataGridViewTextBoxColumn txtColumn = new DataGridViewTextBoxColumn();
                    txtColumn.HeaderText = "項目名";
                    colIndex = grdLabelBlockItems.Columns.Add(txtColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 70;

                    txtColumn = new DataGridViewTextBoxColumn();
                    txtColumn.HeaderText = "X";
                    colIndex = grdLabelBlockItems.Columns.Add(txtColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 35;

                    txtColumn = new DataGridViewTextBoxColumn();
                    txtColumn.HeaderText = "Y";
                    colIndex = grdLabelBlockItems.Columns.Add(txtColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 35;

                    txtColumn = new DataGridViewTextBoxColumn();
                    txtColumn.HeaderText = "Width";
                    colIndex = grdLabelBlockItems.Columns.Add(txtColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 40;

                    txtColumn = new DataGridViewTextBoxColumn();
                    txtColumn.HeaderText = "Height";
                    colIndex = grdLabelBlockItems.Columns.Add(txtColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 40;

                    DataGridViewButtonColumn btnColumn = new DataGridViewButtonColumn();
                    btnColumn.HeaderText = "詳細";
                    colIndex = grdLabelBlockItems.Columns.Add(btnColumn);
                    grdLabelBlockItems.Columns[colIndex].Width = 40;

                }

                foreach (var item in labelBlock.lstLabelTypeBlocklItems)
                {
                    int index = grdLabelBlockItems.Rows.Add();
                    var row = grdLabelBlockItems.Rows[index];

                    UpdateRow(row, item);
                }
            }
            grdLabelBlockItems.CellValueChanged += grdLabelBlockItems_CellValueChanged;

           // UpdateLabelTypePreview();

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

        private void grdLabelBlockItems_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {

            if (grdLabelBlockItems.CurrentCellAddress.X == 0 &&
                   grdLabelBlockItems.IsCurrentCellDirty)
            {
                //コミットする
                grdLabelBlockItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void grdLabelBlockItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = grdLabelBlockItems.Rows[e.RowIndex];

            if (IsLabelTypeBlockType(LabelTypeBlockBase.LabelTypeBlockType.GRID))
            {
                LabelItem item = (LabelItem)row.Tag;

                switch (e.ColumnIndex)
                {
                    case (int)LabelItemColumnIndex.COL_CHECK:
                        item.Visible = (bool)row.Cells[e.ColumnIndex].Value;
                        break;
                    case (int)LabelItemColumnIndex.COL_HEIGHT:
                        item.Height = float.Parse(row.Cells[e.ColumnIndex].Value.ToString());
                        break;
                    case (int)LabelItemColumnIndex.COL_FONT:
                        item.FontSize = float.Parse(row.Cells[e.ColumnIndex].Value.ToString());
                        break;

                }
            }else
            {
                PictureItem item = (PictureItem)row.Tag;

                switch (e.ColumnIndex)
                {
                    case (int)PictureItemColumnIndex.COL_CHECK:
                        item.Visible = (bool)row.Cells[e.ColumnIndex].Value;
                        break;
                    case (int)PictureItemColumnIndex.COL_POSX:
                        item.PosX = float.Parse(row.Cells[e.ColumnIndex].Value.ToString());
                        break;
                    case (int)PictureItemColumnIndex.COL_POSY:
                        item.PosY = float.Parse(row.Cells[e.ColumnIndex].Value.ToString());
                        break;
                    case (int)PictureItemColumnIndex.COL_WIDTH:
                        item.Width = float.Parse(row.Cells[e.ColumnIndex].Value.ToString());
                        break;
                    case (int)PictureItemColumnIndex.COL_HEIGHT:
                        item.Height = float.Parse(row.Cells[e.ColumnIndex].Value.ToString());
                        break;

                }
                UpdateLabelTypePreview();
            }

        }
        private void grdLabelBlockItems_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (!grdLabelBlockItems.IsCurrentCellDirty)
                return;

            float fValue;
            var row = grdLabelBlockItems.Rows[e.RowIndex];

            if (IsLabelTypeBlockType(LabelTypeBlockBase.LabelTypeBlockType.GRID))
            {
                switch (e.ColumnIndex)
                {
                    case (int)LabelItemColumnIndex.COL_HEIGHT:
                    case (int)LabelItemColumnIndex.COL_FONT:
                        string sValue = (string)e.FormattedValue;
                        if (!float.TryParse(sValue, out fValue))
                        {
                            Utility.MessageError("不正な値です");
                            e.Cancel = true;
                        }
                        break;

                }
            }else
            {
                switch (e.ColumnIndex)
                {
                    case (int)PictureItemColumnIndex.COL_POSX:
                    case (int)PictureItemColumnIndex.COL_POSY:
                    case (int)PictureItemColumnIndex.COL_WIDTH:
                    case (int)PictureItemColumnIndex.COL_HEIGHT:
                        string sValue = (string)e.FormattedValue;
                        if (!float.TryParse(sValue, out fValue))
                        {
                            Utility.MessageError("不正な値です");
                            e.Cancel = true;
                        }
                        break;

                }

            }
        }
        private void grdLabelBlockItems_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
           // grdLabelBlockItems.Rows[e.RowIndex].ErrorText = null;

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

        private void txtPosX_TextChanged(object sender, EventArgs e)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            if (labelBlock == null) return;
            float value;
            if(! float.TryParse(txtPosX.Text, out value))
            {
                Utility.MessageError("不正な値です");
                return;
            }
            labelBlock.posX = value;
            UpdateLabelTypePreview();

        }

        private void txtPosY_TextChanged(object sender, EventArgs e)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            if (labelBlock == null) return;
            float value;
            if (!float.TryParse(txtPosY.Text, out value))
            {
                Utility.MessageError("不正な値です");
                return;
            }
            labelBlock.posY = value;
            UpdateLabelTypePreview();

        }
        /// <summary>
        /// データグリッドセルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdLabelBlockItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = grdLabelBlockItems.Rows[e.RowIndex];

            var labeItemBlock = (LabelTypeBlockItemBase)row.Tag;

            DataGridView dgv = (DataGridView)sender;
            //"Button"列ならば、ボタンがクリックされた
            bool bButtonCellClicked = false;

            if (IsLabelTypeBlockType( LabelTypeBlockBase.LabelTypeBlockType.GRID))
            {
                if (e.ColumnIndex == (int)LabelItemColumnIndex.COL_DETAIL) bButtonCellClicked = true;
            }else
            {
                if (e.ColumnIndex == (int)PictureItemColumnIndex.COL_DETAIL) bButtonCellClicked = true;
            }

            if (bButtonCellClicked)
            {
                FormLabelTypeBlockIItemDetail frm = new FormLabelTypeBlockIItemDetail(labeItemBlock);
                frm.ShowDialog();
                UpdateRow(row, labeItemBlock);


            }
        }

        bool IsLabelTypeBlockType(LabelTypeBlockBase.LabelTypeBlockType type)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            return labelBlock.labelTypeBlockType == type ? true : false;
        }

    }

}
