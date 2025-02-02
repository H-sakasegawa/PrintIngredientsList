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
//using System.Windows.Shapes;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
//using System.Windows.Controls;
using System.Reflection;
using NPOI.OpenXmlFormats.Shared;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static NPOI.HSSF.Util.HSSFColor;
using System.Diagnostics;
using NPOI.OpenXmlFormats.Wordprocessing;
using ZXing;
using System.Drawing.Imaging;
using NPOI.OpenXmlFormats.Dml.Chart;

namespace PrintIngredientsList
{
    public partial class FormMain : Form
    {


        static string exePath = "";
        ProductReader productBaseInfo = new ProductReader();
        CommonDeftReader commonDefInfo = new CommonDeftReader();

        PrintLayoutManager printLayoutMng = new PrintLayoutManager();

        LicenseManager licenseMng = LicenseManager.GetLicenseManager();
        public static string SettingsFolderPath = "";

        string prevDataFilePath;
        string settingDataFilePath;
        string printLayoutDataFilePath;

        string productDataBaseFile;
        string commonDataBaseFile;

        /// <summary>
        /// グリッド列インデックス
        /// </summary>
        enum ColumnIndex
        {
            COL_CHECK =0,
            COL_ID,
            COL_TYPE,
            COL_NAME,
            COL_NUM,
            COL_LIMIT_DATE,
            COL_AMOUNT,
            COL_STORGE,
            COL_MANIFAC
        }


        public AppSettingData settingData = new AppSettingData();

        public FormMain()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly().GetName();
            var ver = assembly.Version;

            // アセンブリ名 1.0.0.0
            this.Text = $"{assembly.Name} - {ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";


        }
        private void Form1_Load(object sender, EventArgs e)
        {
            exePath = Path.GetDirectoryName(Application.ExecutablePath);

            SettingsFolderPath = Path.Combine(exePath, Const.SettingFolderName);
            if (!Directory.Exists(SettingsFolderPath))
            {
                Directory.CreateDirectory(SettingsFolderPath);
            }

            prevDataFilePath        = Path.Combine(SettingsFolderPath, Const.SaveDataFileName);
            printLayoutDataFilePath = Path.Combine(SettingsFolderPath, Const.printLayoutDataFolderPath);
            settingDataFilePath     = Path.Combine(SettingsFolderPath, Const.SettingDataFineName);


            //セッティング情報の読み込み
            settingData.Read(settingDataFilePath);
            if (string.IsNullOrEmpty(settingData.dataBasePath))
            {
                settingData.dataBasePath = System.IO.Path.Combine(exePath, Const.dataBaseFolder);

            }

            //商品データベース、共通データベースのの有無チェック
            productDataBaseFile = Path.Combine(settingData.dataBasePath, Const.ProductFileName);
            commonDataBaseFile = System.IO.Path.Combine(settingData.dataBasePath, Const.CommonDefFileName);

            if (!File.Exists(productDataBaseFile))
            {
                Utility.MessageError($"{Const.ProductFileName}が見つかりません");
            }
            if (!File.Exists(commonDataBaseFile))
            {
                Utility.MessageError($"{Const.CommonDefFileName}が見つかりません");
            }



           //最小サイズをレイアウト時のサイズで固定
            this.MinimumSize = this.Size;

            toolStripContainer1.Dock = DockStyle.Fill;
            tabControl1.Dock = DockStyle.Fill;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer3.Dock = DockStyle.Fill;
            gridList.Dock = DockStyle.Fill;


            //panelPrintTypePreview.Dock = DockStyle.Fill;
            //grdLabelBlockItems.Dock = DockStyle.Fill;


            tabControl1.TabPages[0].Focus();

            gridList.Columns[(int)ColumnIndex.COL_NUM].DefaultCellStyle.BackColor = Color.LightBlue;

            //フォント一覧
            //-------------------------------------------------------
            //InstalledFontCollectionオブジェクトの取得
            System.Drawing.Text.InstalledFontCollection ifc =
                new System.Drawing.Text.InstalledFontCollection();
            //インストールされているすべてのフォントファミリアを取得
            FontFamily[] ffs = ifc.Families;

            foreach (FontFamily ff in ffs)
            {
                cmbFont.Items.Add(ff.Name);
            }
            //商品基本データファイル読み込み
            if(ReadDatabase()!=0)
            {
                Utility.MessageError("読み込みに失敗しました。");
            }

            //前回の編集データの読み込み
            ReadSavedPath();


            //印刷レイアウト一覧取得
            if (FindLayoutFiles(printLayoutDataFilePath)!=0)
            {
                Utility.MessageError($"印刷レイアウトファイルが見つかりません。\n{printLayoutDataFilePath}");
            }

            //印刷設定をUIに設定
            SettingDataToUI(settingData);


            //ユーザ固有の設定読み込み
            LoadUserSetting();

            UpdateProdListFont();

            //カラムン幅調整
            SetColumnWidth();

            gridList.MouseWheel += OnGridList_MouseWheel;

#if LICENSE
            int chkResult = CheckLicense();
            if (chkResult != 0)
            {
                SetApplicationLimit(false);

                switch (chkResult)
                {
                    case -1:
                        Utility.MessageError($"ライセンスファイルが読み込めません");
                        break;
                    case -2:
                        Utility.MessageError($"このPCで使用できないライセンスファイルが設定されています。");
                        break;
                    case -3:
                        {
                            var info = ReadLicenseFileFromSettingDir();
                            Utility.MessageError($"ライセンス期限切れです。\n現在のライセンスは{info.LimitDate.Value.ToShortDateString()}までとなっています。\n「ライセンス」メニューからライセンス申請手続きをしてください。");
                        }
                        break;
                }
            }
            mnuLicense.Visible = true;

            mnuUpdateLicense.Enabled = true;
#else
            mnuLicense.Visible=false;
            mnuUpdateLicense.Enabled = false;
#endif


            //Network.GetMacAddress();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //ユーザ固有の設定保存
            SaveUserSetting();
        }

        public int CheckLicense()
        {
            var info = ReadLicenseFileFromSettingDir();
            if (info == null)
            {
                //ライセンスファイルなしなど...
                return -1;
            }

            return LicenseManager.GetLicenseManager().CheckLicense(info);
        }

        void SetApplicationLimit(bool nFlg)
        {
            tabPage2.Enabled = nFlg;

        }


        private void SettingDataToUI(AppSettingData data)
        {
            cmbFont.Text = data.fontName;
            txtCopyNum.Text = data.copyNum.ToString();
            txtPrintStartPos.Text = data.printStartPos.ToString();

        }

        private void LaytoutToUI()
        { 
            

            //印刷レイアウト設定
            var printLayout = printLayoutMng.printLayout;
            var labelLayout = printLayoutMng.labelLayout;

            cmbPaperType.Items.Clear();
            cmbLabelType.Items.Clear();
            for (int i = 0; i < printLayout.GetLayoutCnt(); i++)
            {
                cmbPaperType.Items.Add(printLayout[i]);
            }
            if (cmbPaperType.Items.Count > 0) { cmbPaperType.SelectedIndex = 0; }

            for (int i = 0; i < labelLayout.GetLayoutCnt(); i++)
            {
                cmbLabelType.Items.Add(labelLayout[i]);
            }
            if (cmbLabelType.Items.Count > 0) { cmbLabelType.SelectedIndex = 0; }


            if (curLayout != null)
            {

                txtPrintLeftGap.Text = curLayout.PrintGapLeft.ToString("F2");
                txtPrintTopGap.Text = curLayout.PrintGapTop.ToString("F2");

                txtHeaderLeftGap.Text = curLayout.HeaderGapLeft.ToString("F2");
                txtHeaderTopGap.Text = curLayout.HeaderGapTop.ToString("F2");

                txtLabelAreaGapTop.Text = curLabelType.GapTop.ToString("F2");
                txtLabelAreaGapLeft.Text = curLabelType.GapLeft.ToString("F2");
                txtLabelAreaGapRight.Text = curLabelType.GapRight.ToString("F2");
                txtLabelAreaGapBottom.Text = curLabelType.GapBottom.ToString("F2");

            }
            if (curLabelType != null)
            {
                var labeBlock = curLabelType.GetLabelBlock("成分表");
            }

           

        }
        private void LoadUserSetting()
        {
            //setting情報
            int WinX = Properties.Settings.Default.WinLocX;
            int WinY = Properties.Settings.Default.WinLocY;

            if (WinX < 0)
            {   //小さすぎたら補正
                WinX = 0;
            }
            if (WinY < 0)
            {   //小さすぎたら補正
                WinY = 0;
            }
            this.Location = new Point(WinX, WinY);

            int SizeW = Properties.Settings.Default.WinSizeW;
            int SizeH = Properties.Settings.Default.WinSizeH;
            if (SizeW < 200)
            {   //小さすぎたら補正
                SizeW = Size.Width;
            }
            if (SizeH < 200)
            {   //小さすぎたら補正
                SizeH = Size.Height;
            }
            this.Size = new Size(SizeW, SizeH);

            int SplitDistance = Properties.Settings.Default.SplitDistance;
            splitContainer1.SplitterDistance = SplitDistance;

            //印刷設定のラベルレイアウトスプリット位置
            SplitDistance = Properties.Settings.Default.LabelLayoutPreviewSlpitDistance;
            //splitContainer4.SplitterDistance = SplitDistance;


            //印刷プレビュー画面位置とサイズ補正
            if (Properties.Settings.Default.PrintPreviewDlgLocX <0) Properties.Settings.Default.PrintPreviewDlgLocX = 0;
            if (Properties.Settings.Default.PrintPreviewDlgLocY < 0) Properties.Settings.Default.PrintPreviewDlgLocY = 0;
            if (Properties.Settings.Default.PrintPreviewDlgSizeW < Const.previewDlgBasicWidth) Properties.Settings.Default.PrintPreviewDlgSizeW = Const.previewDlgBasicWidth;
            if (Properties.Settings.Default.PrintPreviewDlgSizeH < Const.previewDlgBasicHeight) Properties.Settings.Default.PrintPreviewDlgSizeH = Const.previewDlgBasicHeight;


        }
        private void SaveUserSetting()
        {
            Properties.Settings.Default.WinLocX = this.Location.X;
            Properties.Settings.Default.WinLocY = this.Location.Y;
            Properties.Settings.Default.WinSizeW = this.Size.Width;
            Properties.Settings.Default.WinSizeH = this.Size.Height;
            Properties.Settings.Default.SplitDistance = splitContainer1.SplitterDistance;
            //Properties.Settings.Default.LabelLayoutPreviewSlpitDistance = splitContainer4.SplitterDistance;


            Properties.Settings.Default.Save();
        }
        /// <summary>
        /// 実行フォルダパス取得
        /// </summary>
        /// <returns></returns>
        public static string GetExePath() { return exePath; }


        /// <summary>
        /// 商品基本データファイル読み込み
        /// </summary>
        private int ReadDatabase()
        {
 
            //共通データベース読み込み
            commonDefInfo.ReadExcel(commonDataBaseFile);


            if(productBaseInfo.ReadExcel(productDataBaseFile) !=0)
            {
                return -1;
            }

            foreach (var info in productBaseInfo.GetProductList(Const.SelectAll))
            {
                //製造者が共通データベースに登録されているものかをチェック
                if(commonDefInfo.GetCommonDefData(CommonDeftReader.keyManifacture, info.manufacturer) == null)
                {
                    Utility.MessageError($"{Const.ProductFileName}の{CommonDeftReader.keyManifacture}「{info.manufacturer}」は\n{Const.CommonDefFileName}に登録されていません。");
                    return -1;
                }
                if (commonDefInfo.GetCommonDefData(CommonDeftReader.keyStorage, info.storageMethod) == null)
                {
                    Utility.MessageError($"{Const.ProductFileName}の{CommonDeftReader.keyStorage}「{info.storageMethod}」は\n{Const.CommonDefFileName}に登録されていません。");
                    return -1;
                }

            }
            return 0;

        }

        int ReadPrintLayoutInfo(string filePath)
        {
            return printLayoutMng.ReadLayout(filePath);
        }
        /// <summary>
        /// 商品リストのフォント設定
        /// </summary>
        private void UpdateProdListFont()
        {
            gridList.Font = new Font(settingData.prodListFontName, settingData.prodListFontSize);
            int intRowHeight = (int)(float.Parse(gridList.Font.Size.ToString()) + 12);
            for (int i = 0; i < gridList.Rows.Count; i++)
            {
                gridList.Rows[i].Height = intRowHeight;
            }
        }
        /// <summary>
        /// 商品リストのカラム幅設定
        /// </summary>
        private void SetColumnWidth()
        {
            int idx = 0;
            foreach(DataGridViewColumn column  in gridList.Columns)
            {
                if( idx >= settingData.prodListColWidthAry.Count())
                {
                    break;
                }
                column.Width = settingData.prodListColWidthAry[idx];
                idx++;
            }

        }


        //Layoutフォルダ内にあるレイアウトファイル名の一覧を取得してコンボボックスに設定する
        int FindLayoutFiles(string filePath)
        {
            if (!Directory.Exists(filePath)) 
            {
                return -1;
            }
            var PathWidlCard = Path.Combine(filePath, "*.dat");
            string [] fileNames = Directory.GetFiles(filePath, "*.dat");

            cmbLayout.Items.Clear();
            foreach ( var name in fileNames)
            {
                var path = Path.Combine(filePath, name);

                cmbLayout.Items.Add(new LaytoutFile(path));
            }
            if(cmbLayout.Items.Count>0)
            {
                cmbLayout.SelectedIndex = 0;
            }else
            {   //印刷レイアウトファイルなし
                return -1;
            }

            return 0;
        }
        private void UpdateTypeCombobox()
        {
            int selectIndex = -1;
            //現在の選択項目
            var oldSelectText = cmbKind.Text;
            cmbKind.Items.Add("全て");
            foreach (DataGridViewRow row in gridList.Rows)
            {

                EditProductData data = (EditProductData)row.Tag;

                var product = productBaseInfo.GetProductDataByID(data.id);

                if ( cmbKind.Items.IndexOf(product.kind)<0)
                {
                    cmbKind.Items.Add(product.kind);
                    if( product.kind == oldSelectText)
                    {
                        selectIndex = cmbKind.Items.Count-1;
                    }
                }
            }
            //前回選択されていた項目があれば選択状態にする。
            if(selectIndex>=0)
            {
                cmbKind.SelectedIndex = selectIndex;
            }else
            {
                cmbKind.SelectedIndex = 0;
            }

        }
        //種別フィルタ表示
        private void cmbKind_SelectedIndexChanged(object sender, EventArgs e)
        {
            var SelectKind = cmbKind.Text;

            if (SelectKind == "全て")
            {
                foreach (DataGridViewRow row in gridList.Rows)
                {
                    row.Visible = true;
                }
            } else
            {

                foreach (DataGridViewRow row in gridList.Rows)
                {
                    EditProductData data = (EditProductData)row.Tag;
                    var product = productBaseInfo.GetProductDataByID(data.id);
                    if (SelectKind == product.kind)
                    {
                        row.Visible = true;
                    }else
                    {
                        row.Visible = false;
                    }
                }
            }
        }
       /// <summary>
        /// 行の表示更新
        /// </summary>
        /// <param name="row"></param>
        /// <param name="data"></param>
        /// <param name="redfawPreview"></param>
        private void EditParamToGridUpdate(DataGridViewRow row, EditProductData data, bool redfawPreview = true)
        {
            List<object> lstObject = new List<object>()
            {
                false //印刷チェックボックス
            };

            var paramItems = data.GetParams(productBaseInfo);

            int index = 1;
            foreach (var p in paramItems)
            {
                row.Cells[index].Value = p;
                index++;
            }
            row.Tag = data;

            if (redfawPreview)
            {
                UpdatePreview();
            }
        }
        /// <summary>
        /// 行追加
        /// </summary>
        /// <param name="data"></param>
        /// <param name="redfawPreview"></param>
        private void EditParamToGridAdd(EditProductData data, bool redfawPreview = true)
        {
            List<object> lstObject = new List<object>()
            {
                false //印刷チェックボックス
            };

            var paramItems = data.GetParams(productBaseInfo);

            foreach (var p in paramItems)
            {
                lstObject.Add(p);
            }

            var rowIndex = gridList.Rows.Add(lstObject.ToArray());

            gridList.Rows[rowIndex].Tag = data;

            if (redfawPreview)
            {
                UpdatePreview();
            }
        }

        /// <summary>
        /// プレビューウィンドウ更新
        /// </summary>
        void UpdatePreview()
        {
            panelPreviw.Invalidate();
            //panelPrintTypePreview.Invalidate();
        }

        /// <summary>
        /// プレビューパネル描画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelPreviw_Paint(object sender, PaintEventArgs e)
        {
            Graphics gPreview = e.Graphics;

            //選択されているラベルタイプ
            var labelType = (LabelType)cmbLabelType.SelectedItem;

            Preview(gPreview, panelPreviw, labelType, (float)1);

        }
        private void Preview(Graphics gPreview,System.Windows.Forms.Panel panel, LabelType labelType, float rate)
        {
            if (labelType == null) return;

            int areaWidth = (int)DrawUtil2.MillimetersToPixels(labelType.Width, gPreview.DpiX);
            int areaHeight = (int)DrawUtil2.MillimetersToPixels(labelType.Height, gPreview.DpiY);

            float scaleW = (float)panel.Width / areaWidth;
            float scaleH = (float)panel.Height / areaHeight;
            float scale = Math.Min(scaleW, scaleH);
            

            scale *= rate;

            gPreview.ScaleTransform(scale, scale);

            DrawLabel(gPreview, labelType, 0, 0);

        }

        void OnGridList_MouseWheel(object sender, MouseEventArgs e)
        {

            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.Delta > 0)
                {
                    settingData.prodListFontSize -= Const.prodListFontSizeInc;
                }
                else
                {
                    settingData.prodListFontSize += Const.prodListFontSizeInc;
                }

                UpdateProdListFont();
            }
        }


        //=========================================================
        //  データの保存と読み込み
        //=========================================================
        #region データの保存と読み込み
        /// <summary>
        /// 保存ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolBtnSave_Click(object sender, EventArgs e)
        {
            //編集グリッド情報の出力
            using (var sw = new StreamWriter(prevDataFilePath))
            {

                foreach (DataGridViewRow row in gridList.Rows)
                {
                    EditProductData data = (EditProductData)row.Tag;

                    sw.WriteLine(data.ToString());
                }
            }
            //現在のカラム幅をセッティングデータに記録
            settingData.prodListColWidthAry = new int[gridList.Columns.Count];
            int index = 0;
            foreach (DataGridViewColumn column in gridList.Columns)
            {

                settingData.prodListColWidthAry[index++] = column.Width;
            }

            //セッティング情報の出力
            settingData.Write(settingDataFilePath);

            //印刷レイアウト情報の保存
#if DEBUG
            printLayoutMng.SaveLayout(printLayoutDataFilePath + "_");
#else
            printLayoutMng.SaveLayout(printLayoutDataFilePath);
#endif

        }
        /// <summary>
        /// 保存データ読み込み
        /// </summary>
        /// <returns>0..正常</returns>
        private int ReadSavedPath()
        {
            if (!File.Exists(prevDataFilePath))
            {
                return 0;
            }
            bool bExistUnknownData = false;

            using (var sr = new StreamReader(prevDataFilePath))
            {
                while (true)
                {
                    string s = sr.ReadLine();
                    if (s == null) break;

                    EditProductData data = null;
                    try
                    {
                        data = new EditProductData(s);
                    }catch
                    {
                        Utility.MessageError($"保存データが正しくありません。\n保存データの読み込みをSKIPします。");
                        return -1;
                    }

                    //前回保存されたIDに該当するものが、商品データベースにあるかをチェック
                    var productData = productBaseInfo.GetProductDataByID(data.id);
                    if (productData == null)
                    {
                        bExistUnknownData = true;
                        continue;
                    }

                    EditParamToGridAdd(data, false);
                }
            }

            if(bExistUnknownData)
            {
                Utility.MessageInfo("前回編集データに商品データベースに登録されていない商品がありました。");
            }
            //種別フィルタコンボボックス更新
            UpdateTypeCombobox();

            if (gridList.Rows.Count > 0)
            {
                gridList.Rows[0].Selected = true;
                UpdatePreview();
            }

            return 0;
        }
#endregion

        //=========================================================
        //  成分表一覧タブの各種イベント
        //=========================================================
        #region 成分表一覧タブの各種イベント

        /// <summary>
        /// ダブルクリックによる商品編集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridList_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            if (e.RowIndex < 0) return;

            var row = gridList.Rows[e.RowIndex];
            var data = (EditProductData)row.Tag;


            FormEditIngredients frm = new FormEditIngredients(this, productBaseInfo, commonDefInfo, data);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                //編集結果
                var editParam = frm.GetEditParam();

                EditParamToGridUpdate(row, editParam);
            }
        }

        /// <summary>
        /// 追加ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            FormEditIngredients frm = new FormEditIngredients(this, productBaseInfo, commonDefInfo);
            frm.Show();
            frm.TopMost = true;
            //== DialogResult.OK)
            //{
            //    //編集結果
            //    var editParam = frm.GetEditParam();

            //    EditParamToGridAdd(editParam);
            //}

        }
        public void AddProduct(EditProductData editParam )
        {
            EditParamToGridAdd(editParam);
        }
        /// <summary>
        /// 削除ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            List<int> lstCheckedRow = new List<int>();
            //チェックボックスがONの行があるか？
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
                if (Utility.MessageConfirm("選択されている項目を削除します。\nよろしいですか？", "削除") != DialogResult.OK)
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
        /// <summary>
        /// 全てチェックボックス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            UpdatePreview();
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
            if (e.ColumnIndex == (int)ColumnIndex.COL_NUM)
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
            if (e.ColumnIndex == (int)ColumnIndex.COL_NUM)
            {
                string sValue = (string)e.FormattedValue;
                int value;
                if (!CnvIntValue(sValue, out value))
                {
                    e.Cancel = true;
                }
            }
        }

        private void gridList_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            gridList.Rows[e.RowIndex].ErrorText = null;

        }


        private void button11_Click(object sender, EventArgs e)
        {
            if (Utility.MessageConfirm("編集データをリセとします。\nよろしいですか？", "リセット") != DialogResult.OK)
            {
                return;
            }

            gridList.Rows.Clear();
        }

        //商品データ再読み込み
        private void toolBtnReload_Click(object sender, EventArgs e)
        {
            if(ReadDatabase()!=0)
            {
                Utility.MessageError("読み込みに失敗しました。");
            }

            try
            {
                //印刷レイアウト情報の読み込み →選択コンボボックス更新
                FindLayoutFiles(printLayoutDataFilePath);
               
            }
            catch (Exception ex)
            {
                Utility.MessageError($"印刷レイアウト情報の読み込みで例外が発生\n{ex.Message}");
            }
            //印刷設定をUIに設定
            SettingDataToUI(settingData);


            UpdatePreview();
            Utility.MessageInfo("読み込み完了！");

        }
        /// <summary>
        /// 商品データベースを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolBtnEditDatabase_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process p =
                    System.Diagnostics.Process.Start(productDataBaseFile);
            
        }

        private void button10_Click(object sender, EventArgs e)
        {
            FormEditPrintStartPos frm = new FormEditPrintStartPos(curLabelType,settingData);
            frm.printStartPos = settingData.printStartPos;

            if (frm.ShowDialog() == DialogResult.OK)
            {
                settingData.printStartPos = frm.printStartPos;
                txtPrintStartPos.Text = settingData.printStartPos.ToString();
            }
        }

        private void menuSave_Click(object sender, EventArgs e)
        {
            toolBtnSave_Click(null, null);
        }

        private void menuReload_Click(object sender, EventArgs e)
        {
            toolBtnReload_Click(null, null);
        }
        /// <summary>
        /// ライセンス更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuUpdateLicense_Click(object sender, EventArgs e)
        {
            FormLicenseMng frm = new FormLicenseMng();
            if(frm.ShowDialog() == DialogResult.OK)
            {
                //制限解除
                SetApplicationLimit(true);
            }else
            {
                SetApplicationLimit(false);

            }
        }

        /// <summary>
        /// 有効期限について
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuLimitDate_Click(object sender, EventArgs e)
        {
            LicenseManager lm = LicenseManager.GetLicenseManager();

            var info = ReadLicenseFileFromSettingDir();

            Utility.MessageInfo($"現在取得されているライセンスの期限は、\n{info.LimitDate.Value.ToShortDateString()}\nとなっています。");
        }

        private LicenseManager.LicenseInfo ReadLicenseFileFromSettingDir()
        {
            string filePath = System.IO.Path.Combine(SettingsFolderPath, Const.LicenseFileName);

            return LicenseManager.GetLicenseManager().ReadLicenseFile(filePath);
        }
        #endregion

        //=========================================================
        //  印刷タブの各種イベント
        //=========================================================
        #region 印刷タブの各種イベント

        //--------------------------------
        //印刷レイアウト
        //--------------------------------
        #region 印刷レイアウト

        /// <summary>
        /// セット枚数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtCopyNum_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(txtCopyNum.Text, out settingData.copyNum))
            {
                ErrMsg("セット枚数");
            }

        }
        //フォント選択
        private void cmbFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            settingData.fontName = cmbFont.Text;
            UpdatePreview();
        }
        /// <summary>
        /// 用紙選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            curLayout = (Layout)cmbPaperType.SelectedItem;

            lblSize.Text = $"{curLayout.paperWidth} × {curLayout.paperHeight}";
            txtPrintLeftGap.Text = curLayout.PrintGapLeft.ToString("F2");
            txtPrintTopGap.Text  = curLayout.PrintGapTop.ToString("F2");
            UpdatePreview();

        }
        /// <summary>
        /// ラベルタイプ選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbLabelType_SelectedIndexChanged(object sender, EventArgs e)
        {
            curLabelType = (LabelType)cmbLabelType.SelectedItem;

            txtLabelAreaGapLeft.Text   = curLabelType.GapLeft.ToString("F2");
            txtLabelAreaGapTop.Text    = curLabelType.GapTop.ToString("F2");
            txtLabelAreaGapRight.Text  = curLabelType.GapRight.ToString("F2");
            txtLabelAreaGapBottom.Text = curLabelType.GapBottom.ToString("F2");

            cmbLabelBlock.Items.Clear();
            foreach (var ItemBlock in curLabelType.lstLabelBlocks)
            {
                cmbLabelBlock.Items.Add(ItemBlock);
            }
            if (cmbLabelBlock.Items.Count > 0)
            {
                cmbLabelBlock.SelectedIndex = 0;
            }
            UpdatePreview();
        }

        /// <summary>
        /// プレビュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            CreatePrintData(chkPreview1piece.Checked);

            PrintDocumentEx pd = CreatePrintDocument(PrintType.PREVIEW);
            pd.ResetPageIndex();

            FormPrintPreview frm = new FormPrintPreview(this, pd);
            frm.ShowDialog();

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
            pd.ResetPageIndex();

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
        #endregion


        //--------------------------------
        //印刷位置調整タブ
        //--------------------------------
        #region 印刷位置調整 タブ
        /// <summary>
        /// ▶ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            float value = float.Parse(txtPrintLeftGap.Text);
            value += (float)1.0;
            txtPrintLeftGap.Text = value.ToString("F1");

        }
        /// <summary>
        /// ◀　ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            float value = float.Parse(txtPrintLeftGap.Text);
            value -= (float)1.0;
            if (value < 0) value = 0;

            txtPrintLeftGap.Text = value.ToString("F1");
        }
        /// <summary>
        /// ▲ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            float value = float.Parse(txtPrintTopGap.Text);
            value -= (float)1.0;
            if (value < 0) value = 0;
            txtPrintTopGap.Text = value.ToString("F1");
        }
        /// <summary>
        /// ▼　ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            float value = float.Parse(txtPrintTopGap.Text);
            value += (float)1.0;
            txtPrintTopGap.Text = value.ToString("F1");

        }
        /// <summary>
        /// 印刷用紙左余白
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPrintLeftGap_TextChanged(object sender, EventArgs e)
        {
            float value;
            if (!float.TryParse(txtPrintLeftGap.Text, out value))
            {
                ErrMsg("印刷領域(左余白)");
                return;
            }
            curLayout.PrintGapLeft = value;
            UpdatePreview();

        }
        /// <summary>
        /// 印刷用紙上余白
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPrintTopGap_TextChanged(object sender, EventArgs e)
        {
            float value;
            if (!float.TryParse(txtPrintTopGap.Text, out value))
            {
                ErrMsg("印刷領域(上余白)");
                return;
            }
            curLayout.PrintGapTop = value;
            UpdatePreview();
        }
        /// <summary>
        /// ヘッダ領域(左余白）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtHeaderLeftGap_TextChanged(object sender, EventArgs e)
        {
            float value;
            if (!float.TryParse(txtHeaderLeftGap.Text, out value))
            {
                ErrMsg("ヘッダ領域(左余白)");
                return;
            }
            curLayout.HeaderGapLeft = value;
            UpdatePreview();

        }
        /// <summary>
        /// ヘッダ領域(上余白)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtHeaderTopGap_TextChanged(object sender, EventArgs e)
        {
            float value;
            if (!float.TryParse(txtHeaderTopGap.Text, out value))
            {
                ErrMsg("ヘッダ領域(上余白)");
                return;
            }
            curLayout.HeaderGapTop = value;
            UpdatePreview();
        }
        #endregion

        //--------------------------------
        //ラベル余白調整タブ
        //--------------------------------
        #region ラベル余白調整タブ
        /// <summary>
        /// ラベル領域の左余白
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLabelAreaGapLeft_TextChanged(object sender, EventArgs e)
        {
            float value;
            if (!float.TryParse(txtLabelAreaGapLeft.Text, out value))
            {
                ErrMsg("ラベル印刷領域(左余白)");
                return;
            }
            curLabelType.GapLeft = value;
            UpdatePreview();
        }
        /// <summary>
        /// ラベル領域の右余白
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLabelAreaGapRight_TextChanged(object sender, EventArgs e)
        {

            float value;
            if (!float.TryParse(txtLabelAreaGapRight.Text, out value))
            {
                ErrMsg("ラベル印刷領域(右余白)");
                return;
            }
            curLabelType.GapRight = value;
            UpdatePreview();
        }
        /// <summary>
        /// ラベル領域の上余白
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLabelAreaGapTop_TextChanged(object sender, EventArgs e)
        {
            float value;
            if (!float.TryParse(txtLabelAreaGapTop.Text, out value))
            {
                ErrMsg("ラベル印刷領域(上余白)");
                return;
            }
            curLabelType.GapTop = value;
            UpdatePreview();
        }
        /// <summary>
        /// ラベル領域の下余白
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLabelAreaGapBottom_TextChanged(object sender, EventArgs e)
        {
            float value;
            if (!float.TryParse(txtLabelAreaGapBottom.Text, out value))
            {
                ErrMsg("ラベル印刷領域(下余白)");
                return;
            }
            curLabelType.GapBottom = value;
            UpdatePreview();
        }
        #endregion


        //--------------------------------
        //項目の位置、高さ、フォント調整
        //--------------------------------
        #region 項目の位置、高さ、フォント調整

        //ラベルブロック選択
        private void cmbLabelBlock_SelectedIndexChanged(object sender, EventArgs e)
        {
            grdLabelBlockItems.CellValueChanged -= grdLabelBlockItems_CellValueChanged;
            {
                grdLabelBlockItems.Rows.Clear();

                LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
                if (labelBlock == null) return;

                txtPosX.Text = labelBlock.PosX.ToString();
                txtPosY.Text = labelBlock.PosY.ToString();

                txtTitleColWidth.Text = labelBlock.TitleWidth.ToString("F2");
                txtValueColWidth.Text = labelBlock.ValueWidth.ToString("F2");

                txtTitleColFontSize.Text = labelBlock.TitleFontSize.ToString("F1");

                //データグリッドビューのヘッダを更新
                grdLabelBlockItems.Columns.Clear();
                if (labelBlock.labelTypeBlockType == LabelTypeBlockBase.LabelTypeBlockType.GRID)
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

                if (row.Cells[e.ColumnIndex].Value!=null)
                {

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
                }
            }
            else
            {
                PictureItem item = (PictureItem)row.Tag;

                if (row.Cells[e.ColumnIndex].Value != null)
                {
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
                }
            }
            UpdatePreview();

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
                        if (!CnvFloatValue(sValue, out fValue))
                        {
                            e.Cancel = true;
                        }
                        break;

                }
            }
            else
            {
                switch (e.ColumnIndex)
                {
                    case (int)PictureItemColumnIndex.COL_POSX:
                    case (int)PictureItemColumnIndex.COL_POSY:
                    case (int)PictureItemColumnIndex.COL_WIDTH:
                    case (int)PictureItemColumnIndex.COL_HEIGHT:
                        string sValue = (string)e.FormattedValue;
                        if (!CnvFloatValue(sValue, out fValue))
                        {
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
        private void txtPosX_TextChanged(object sender, EventArgs e)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            if (labelBlock == null) return;
            float value;
            if (CnvFloatValue(txtPosX, out value))
            {
                labelBlock.PosX = value;
                UpdatePreview();
            }

        }

        private void txtPosY_TextChanged(object sender, EventArgs e)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            if (labelBlock == null) return;
            float value;
            if (CnvFloatValue(txtPosY, out value))
            {
                labelBlock.PosY = value;
                UpdatePreview();
            }


        }
        /// <summary>
        /// タイトル列幅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtTitleColWidth_TextChanged(object sender, EventArgs e)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            if (labelBlock == null) return;
            float value;
            if (CnvFloatValue(txtTitleColWidth, out value))
            {
                labelBlock.TitleWidth = value;
                UpdatePreview();
            }

        }
        /// <summary>
        /// タイトル列フォントサイズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtTitleColFontSize_TextChanged(object sender, EventArgs e)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            if (labelBlock == null) return;
            float value;
            if (CnvFloatValue(txtTitleColFontSize, out value))
            {
                labelBlock.TitleFontSize = value;
                UpdatePreview();
            }
        }
        /// <summary>
        /// 値列幅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtValueColWidth_TextChanged(object sender, EventArgs e)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            if (labelBlock == null) return;
            float value;
            if (CnvFloatValue(txtValueColWidth, out value))
            {
                labelBlock.ValueWidth = value;
                UpdatePreview();
            }
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

            if (IsLabelTypeBlockType(LabelTypeBlockBase.LabelTypeBlockType.GRID))
            {
                if (e.ColumnIndex == (int)LabelItemColumnIndex.COL_DETAIL) bButtonCellClicked = true;
            }
            else
            {
                if (e.ColumnIndex == (int)PictureItemColumnIndex.COL_DETAIL) bButtonCellClicked = true;
            }

            if (bButtonCellClicked)
            {
                FormLabelTypeBlockIItemDetail frm = new FormLabelTypeBlockIItemDetail(row,labeItemBlock);

                frm.onCallbackValueChanged += OnonCallbackValueChanged;
                frm.ShowDialog();
                UpdateRow(row, labeItemBlock);
                UpdatePreview();


            }
        }

        void OnonCallbackValueChanged(DataGridViewRow row, LabelTypeBlockItemBase blockItem)
        {
            UpdateRow(row, blockItem);
            UpdatePreview();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        #endregion

        /// <summary>
        /// TextBoxの値→float変換
        /// </summary>
        /// <param name="txtBox"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool CnvFloatValue(TextBox txtBox, out float value)
        {
            return CnvFloatValue(txtBox.Text, out value);
        }
        /// <summary>
        /// 数値文字列→float変換
        /// </summary>
        /// <param name="sValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool CnvFloatValue(string sValue, out float value)
        {
            if (!float.TryParse(sValue, out value))
            {
                Utility.MessageError("不正な値です");
                return false;
            }
            return true;

        }
        bool CnvIntValue(string sValue, out int value)
        {
            if (!int.TryParse(sValue, out value))
            {
                Utility.MessageError("不正な値です");
                return false;
            }
            return true;

        }

        void ErrMsg(string itemName)
        {
            Utility.MessageError($"{itemName}に不正な文字が入力されました");

        }

        /// <summary>
        /// レイアウトコンボボックス選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbLayout_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            LaytoutFile layoutFile = (LaytoutFile)cmbLayout.SelectedItem;
            if (layoutFile == null) return;

            try
            {
                //印刷レイアウト情報の読み込み
                ReadPrintLayoutInfo(layoutFile.filePath);
            }
            catch (Exception ex)
            {
                Utility.MessageError($"印刷レイアウト情報の読み込みで例外が発生\n{ex.Message}");
            }
            LaytoutToUI();
        }



        #endregion

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void mnuSetting_Click(object sender, EventArgs e)
        {
            FromSetting frm = new FromSetting(settingData);
            if( frm.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(settingData.dataBasePath))
                {
                    Utility.MessageConfirm("データベースのフォルダが未設定です。\nオプション画面からデータベースのフォルダを設定してください。", "データベースパス");
                    return;
                }


                //商品データベース、共通データベースファイルパス更新
                productDataBaseFile = Path.Combine(settingData.dataBasePath, Const.ProductFileName);
                commonDataBaseFile = System.IO.Path.Combine(settingData.dataBasePath, Const.CommonDefFileName);

                UpdateProdListFont();

            }
        }

    }

}