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


        PrintSettingData settingData = new PrintSettingData();

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

            exePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

            SettingsFolderPath = System.IO.Path.Combine(exePath, Const.SettingFolderName);
            if( !Directory.Exists(SettingsFolderPath))
            {
                Directory.CreateDirectory(SettingsFolderPath);
            }

            prevDataFilePath = System.IO.Path.Combine(SettingsFolderPath, Const.SaveDataFileName);
            settingDataFilePath = System.IO.Path.Combine(SettingsFolderPath, Const.SettingDataFineName);
            printLayoutDataFilePath = System.IO.Path.Combine(SettingsFolderPath, Const.printLayoutDataFineName);

            //最小サイズをレイアウト時のサイズで固定
            this.MinimumSize = this.Size;

            toolStripContainer1.Dock = DockStyle.Fill;
            tabControl1.Dock = DockStyle.Fill;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer3.Dock = DockStyle.Fill;
            gridList.Dock = DockStyle.Fill;


            panelPrintTypePreview.Dock = DockStyle.Fill;
            grdLabelBlockItems.Dock = DockStyle.Fill;


            tabControl1.TabPages[0].Focus();


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

            //セッティング情報の読み込み
            settingData.Read(settingDataFilePath);

            try
            {
                //印刷レイアウト情報の読み込み
                ReadPrintLayoutInfo(printLayoutDataFilePath);
            }catch(Exception ex)
            {
                Utility.MessageError("印刷レイアウト情報の読み込みで例外が発生");
            }
            //印刷設定をUIに設定
            SettingDataToUI(settingData);


            //ユーザ固有の設定読み込み
            LoadUserSetting();

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


        private void SettingDataToUI( PrintSettingData data)
        {

            //印刷レイアウト設定
            var printLayout = printLayoutMng.printLayout;
            var labelLayout = printLayoutMng.labelLayout;

            cmbLayout.Items.Clear();
            cmbLabelType.Items.Clear();
            for (int i = 0; i < printLayout.GetLayoutCnt(); i++)
            {
                cmbLayout.Items.Add(printLayout[i]);
            }
            if (cmbLayout.Items.Count > 0) { cmbLayout.SelectedIndex = 0; }

            for (int i = 0; i < labelLayout.GetLayoutCnt(); i++)
            {
                cmbLabelType.Items.Add(labelLayout[i]);
            }
            if (cmbLabelType.Items.Count > 0) { cmbLabelType.SelectedIndex = 0; }


            cmbFont.Text = data.fontName;
            if (curLayout != null)
            {

                txtCopyNum.Text = data.copyNum.ToString();
                txtPrintLeftGap.Text = curLayout.printGapLeft.ToString("F2");
                txtPrintTopGap.Text = curLayout.printGapTop.ToString("F2");

                txtHeaderLeftGap.Text = curLayout.headerGapLeft.ToString("F2");
                txtHeaderTopGap.Text = curLayout.headerGapTop.ToString("F2");

                txtLabelAreaGapTop.Text = curLabelType.gapTop.ToString("F2");
                txtLabelAreaGapLeft.Text = curLabelType.gapLeft.ToString("F2");
                txtLabelAreaGapRight.Text = curLabelType.gapRight.ToString("F2");
                txtLabelAreaGapBottom.Text = curLabelType.gapBottom.ToString("F2");

            }
            if (curLabelType != null)
            {
                var labeBlock = curLabelType.GetLabelBlock("成分表");
            }

            txtPrintStartPos.Text       = data.printStartPos.ToString();
           

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
            splitContainer4.SplitterDistance = SplitDistance;


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
            Properties.Settings.Default.LabelLayoutPreviewSlpitDistance = splitContainer4.SplitterDistance;


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
            string dirPath = System.IO.Path.Combine(exePath, Const.dataBaseFolder);
            string path = System.IO.Path.Combine(dirPath, Const.CommonDefFileName);
            
            //共通データベース読み込み
            commonDefInfo.ReadExcel(path);


            path = System.IO.Path.Combine(dirPath, Const.ProductFileName);
            if(productBaseInfo.ReadExcel(path)!=0)
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

            int areaWidth = (int)DrawUtil2.MillimetersToPixels(labelType.width, gPreview.DpiX);
            int areaHeight = (int)DrawUtil2.MillimetersToPixels(labelType.height, gPreview.DpiY);

            float scaleW = (float)panel.Width / areaWidth;
            float scaleH = (float)panel.Height / areaHeight;
            float scale = Math.Min(scaleW, scaleH);
            

            scale *= rate;

            gPreview.ScaleTransform(scale, scale);

            DrawLabel(gPreview, labelType, 0, 0);

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
                //印刷レイアウト情報の読み込み
                ReadPrintLayoutInfo(printLayoutDataFilePath);
            }
            catch (Exception ex)
            {
                Utility.MessageError("印刷レイアウト情報の読み込みで例外が発生");
            }
            //印刷設定をUIに設定
            SettingDataToUI(settingData);


            UpdatePreview();
            Utility.MessageInfo("読み込み完了！");

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
            UpdateLabelTypePreview();
        }
        /// <summary>
        /// 用紙選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            curLayout = (Layout)cmbLayout.SelectedItem;

            lblSize.Text = $"{curLayout.paperWidth} × {curLayout.paperHeight}";
            txtPrintLeftGap.Text = curLayout.printGapLeft.ToString("F2");
            txtPrintTopGap.Text = curLayout.printGapTop.ToString("F2");

        }
        /// <summary>
        /// ラベルタイプ選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbLabelType_SelectedIndexChanged(object sender, EventArgs e)
        {
            curLabelType = (LabelType)cmbLabelType.SelectedItem;

            txtLabelAreaGapLeft.Text = curLabelType.gapLeft.ToString("F2");
            txtLabelAreaGapTop.Text = curLabelType.gapTop.ToString("F2");
            txtLabelAreaGapRight.Text = curLabelType.gapRight.ToString("F2");
            txtLabelAreaGapBottom.Text = curLabelType.gapBottom.ToString("F2");

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

        /// <summary>
        /// プレビュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            CreatePrintData();

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
            if (!float.TryParse(txtPrintLeftGap.Text, out curLayout.printGapLeft))
            {
                ErrMsg("印刷領域(左余白)");
                return;
            }
            UpdatePreview();

        }
        /// <summary>
        /// 印刷用紙上余白
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPrintTopGap_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtPrintTopGap.Text, out curLayout.printGapTop))
            {
                ErrMsg("印刷領域(上余白)");
                return;
            }
            UpdatePreview();
        }
        /// <summary>
        /// ヘッダ領域(左余白）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtHeaderLeftGap_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtHeaderLeftGap.Text, out curLayout.headerGapLeft))
            {
                ErrMsg("ヘッダ領域(左余白)");
                return;
            }
            UpdatePreview();

        }
        /// <summary>
        /// ヘッダ領域(上余白)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtHeaderTopGap_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtHeaderTopGap.Text, out curLayout.headerGapTop))
            {
                ErrMsg("ヘッダ領域(上余白)");
                return;
            }
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
            if (!float.TryParse(txtLabelAreaGapLeft.Text, out curLabelType.gapLeft))
            {
                ErrMsg("ラベル印刷領域(左余白)");
                return;
            }
            UpdatePreview();
        }
        /// <summary>
        /// ラベル領域の右余白
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLabelAreaGapRight_TextChanged(object sender, EventArgs e)
        {

            if (!float.TryParse(txtLabelAreaGapRight.Text, out curLabelType.gapRight))
            {
                ErrMsg("ラベル印刷領域(右余白)");
                return;
            }
            UpdatePreview();
        }
        /// <summary>
        /// ラベル領域の上余白
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLabelAreaGapTop_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtLabelAreaGapTop.Text, out curLabelType.gapTop))
            {
                ErrMsg("ラベル印刷領域(上余白)");
                return;
            }
            UpdatePreview();
        }
        /// <summary>
        /// ラベル領域の下余白
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLabelAreaGapBottom_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtLabelAreaGapBottom.Text, out curLabelType.gapBottom))
            {
                ErrMsg("ラベル印刷領域(下余白)");
                return;
            }
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

                txtPosX.Text = labelBlock.posX.ToString();
                txtPosY.Text = labelBlock.posY.ToString();

                txtTitleColWidth.Text = labelBlock.titleWidth.ToString("F2");
                txtValueColWidth.Text = labelBlock.valueWidth.ToString("F2");

                txtTitleColFontSize.Text = labelBlock.titleFontSize.ToString("F1");

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
            UpdateLabelTypePreview();

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
                labelBlock.posX = value;
                UpdateLabelTypePreview();
            }

        }

        private void txtPosY_TextChanged(object sender, EventArgs e)
        {
            LabelTypeBlock labelBlock = (LabelTypeBlock)cmbLabelBlock.SelectedItem;
            if (labelBlock == null) return;
            float value;
            if (CnvFloatValue(txtPosY, out value))
            {
                labelBlock.posY = value;
                UpdateLabelTypePreview();
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
                labelBlock.titleWidth = value;
                UpdateLabelTypePreview();
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
                labelBlock.titleFontSize = value;
                UpdateLabelTypePreview();
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
                labelBlock.valueWidth = value;
                UpdateLabelTypePreview();
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
                FormLabelTypeBlockIItemDetail frm = new FormLabelTypeBlockIItemDetail(labeItemBlock);
                frm.ShowDialog();
                UpdateRow(row, labeItemBlock);
                UpdateLabelTypePreview();


            }
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



        #endregion

 

    }

}