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
using System.Windows.Controls;
using System.Reflection;
using NPOI.OpenXmlFormats.Shared;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static NPOI.HSSF.Util.HSSFColor;
using System.Diagnostics;
using NPOI.OpenXmlFormats.Wordprocessing;


namespace PrintIngredientsList
{
    public partial class FormMain : Form
    {


        static string exePath = "";
        ProductReader productBaseInfo = new ProductReader();
        CommonDeftReader commonDefInfo = new CommonDeftReader();

        LicenseManager licenseMng = LicenseManager.GetLicenseManager();
        public static string SettingsFolderPath = "";

        string prevDataFilePath;
        string settingDataFilePath;

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
            this.Text =  $"{assembly.Name} - {ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
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


            //最小サイズをレイアウト時のサイズで固定
            this.MinimumSize = this.Size;

            toolStripContainer1.Dock = DockStyle.Fill;
            tabControl1.Dock = DockStyle.Fill;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer3.Dock = DockStyle.Fill;
            gridList.Dock = DockStyle.Fill;


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


            Network.GetMacAddress();
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

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //ユーザ固有の設定保存
            SaveUserSetting();
        }

        private void SettingDataToUI( PrintSettingData data)
        {
            cmbFont.Text = data.fontName;

            txtCopyNum.Text            = data.copyNum.ToString();
            txtPrintLeftGap.Text       = data.PrintTopGap.ToString("F2");
            txtPrintTopGap.Text        = data.PrintTopGap.ToString("F2");

            txtLabelAreaGapTop.Text    = data.LabelAreaGapTop.ToString("F2");
            txtLabelAreaGapLeft.Text   = data.LabelAreaGapLeft.ToString("F2");
            txtLabelAreaGapRight.Text  = data.LabelAreaGapRight.ToString("F2");
            txtLabelAreaGapBottom.Text = data.LabelAreaGapBottom.ToString("F2");


            txtFontProductTitle.Text    = data.fontSizeProductTitle.ToString("F0");
            txtFontMaterial.Text        = data.fontSizeMaterial.ToString("F0");
            txtFontAmount.Text          = data.fontSizeAmount.ToString("F0");
            txtFontValidDays.Text       = data.fontSizeLimitDate.ToString("F0");
            txtFontSotrage.Text         = data.fontSizeStorage.ToString("F0");
            txtFontManifucture.Text     = data.fontSizeManifac.ToString("F0");
            txtFontComment.Text         = data.fontSizeComment.ToString("F0");


            txtHightProductTitle.Text   = data.hightProductTitle.ToString("F0");
            txtHightMaterial.Text       = data.hightMaterial.ToString("F0");
            txtHightAmount.Text         = data.hightAmount.ToString("F0");
            txtHightValidDays.Text      = data.hightLimitDate.ToString("F0");
            txtHightSotrage.Text        = data.hightStorage.ToString("F0");
            txtHightManifucture.Text    = data.hightManifac.ToString("F0");

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


            FormEditIngredients frm = new FormEditIngredients(productBaseInfo, commonDefInfo, data);
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
            FormEditIngredients frm = new FormEditIngredients(productBaseInfo, commonDefInfo);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                //編集結果
                var editParam = frm.GetEditParam();

                EditParamToGridAdd(editParam);
            }

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
        /// プレビューパネル描画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelPreviw_Paint(object sender, PaintEventArgs e)
        {
            Graphics gPreview = e.Graphics;

            int areaWidth = (int)DrawUtil2.MillimetersToPixels(settingData.LabelDrawArealWidthMM, gPreview.DpiX);
            int areaHeight = (int)DrawUtil2.MillimetersToPixels(settingData.LabelDrawAreaHeightMM, gPreview.DpiY);

            float scale = 1;
            if (panelPreviw.Width < panelPreviw.Height)
            {
                scale = (float) panelPreviw.Width / areaWidth;
            }
            else
            {
                scale = (float) panelPreviw.Height / areaHeight;
            }
            scale *= (float)0.9;

            gPreview.ScaleTransform(scale, scale);

            DrawLabel(gPreview, 0, 0);
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
                if (!int.TryParse(sValue, out value))
                {

                    Utility.MessageError("不正な値です");
                    e.Cancel = true;
                }
            }
        }

        private void gridList_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            gridList.Rows[e.RowIndex].ErrorText = null;

        }


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
                        //if (!bApplyAll)
                        //{
                        //    FormSaveDatReferctor frm = new FormSaveDatReferctor(productBaseInfo, data);
                        //    frm.ShowDialog();
                        //    if (frm.result == 0)
                        //    {
                        //        bApplyAll = frm.bApplyAll;
                        //        continue;
                        //    }
                        //    data.name = frm.selectName;
                        //    productData = productBaseInfo.GetProductDataByID(frm.selectName);
                        //}
                        bExistUnknownData = true;
                        continue;
                    }
                    //商品名はデータベースの内容で更新
                    data.name = productData.name;
                    //種別名はデータベースの内容で更新
                    data.kind = productData.kind;

                    EditParamToGridAdd(data, false);
                }
            }

            if(bExistUnknownData)
            {
                Utility.MessageInfo("前回編集データに商品データベースに登録されていない商品がありました。");
            }

            if (gridList.Rows.Count > 0)
            {
                gridList.Rows[0].Selected = true;
                UpdatePreview();
            }

            return 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            float value = float.Parse(txtPrintLeftGap.Text);
            value += (float)1.0;
            txtPrintLeftGap.Text = value.ToString("F1");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            float value = float.Parse(txtPrintLeftGap.Text);
            value -= (float)1.0;
            if (value < 0) value = 0;

            txtPrintLeftGap.Text = value.ToString("F1");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            float value = float.Parse(txtPrintTopGap.Text);
            value -= (float)1.0;
            if (value < 0) value = 0;
            txtPrintTopGap.Text = value.ToString("F1");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            float value = float.Parse(txtPrintTopGap.Text);
            value += (float)1.0;
            txtPrintTopGap.Text = value.ToString("F1");

        }

        private void txtPrintLeftGap_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtPrintLeftGap.Text, out settingData.PrintLeftGap))
            {
                ErrMsg("印刷領域(左余白)");
                return;
            }
            UpdatePreview();

        }

        private void txtPrintTopGap_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtPrintTopGap.Text, out settingData.PrintTopGap))
            {
                ErrMsg("印刷領域(上余白)");
                return;
            }
            UpdatePreview();
        }

        private void txtLabelAreaGapLeft_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtLabelAreaGapLeft.Text, out settingData.LabelAreaGapLeft))
            {
                ErrMsg("ラベル印刷領域(左余白)");
                return;
            }
            UpdatePreview();
        }

        private void txtLabelAreaGapRight_TextChanged(object sender, EventArgs e)
        {

            if (!float.TryParse(txtLabelAreaGapRight.Text, out settingData.LabelAreaGapRight))
            {
                ErrMsg("ラベル印刷領域(右余白)");
                return;
            }
            UpdatePreview();
        }

        private void txtLabelAreaGapTop_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtLabelAreaGapTop.Text, out settingData.LabelAreaGapTop))
            {
                ErrMsg("ラベル印刷領域(上余白)");
                return;
            }
            UpdatePreview();
        }

        private void txtLabelAreaGapBottom_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(txtLabelAreaGapBottom.Text, out settingData.LabelAreaGapBottom))
            {
                ErrMsg("ラベル印刷領域(下余白)");
                return;
            }
            UpdatePreview();
        }
        private void txtCopyNum_TextChanged(object sender, EventArgs e)
        {
            if(!int.TryParse(txtCopyNum.Text, out settingData.copyNum))
            {
                ErrMsg("セット枚数");
            }

        }
        void ErrMsg(string itemName)
        {
            Utility.MessageError($"{itemName}に不正な文字が入力されました");

        }

        /// <summary>
        /// リセット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            settingData = new PrintSettingData();
            //印刷設定をUIに設定
            SettingDataToUI(settingData);
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
            }else
            {
                Utility.MessageInfo("商品データを再読み込みしました。");
            }

            UpdatePreview();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            FormEditPrintStartPos frm = new FormEditPrintStartPos(settingData);
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
    }

}