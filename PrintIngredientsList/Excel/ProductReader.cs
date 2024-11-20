using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

using NPOI.XSSF.UserModel;

using PrintIngredientsList;
using NPOI.SS.UserModel;
using System.Xml.Linq;

namespace ExcelReaderUtility
{
    /// <summary>
    /// 説明用Excelファイルから画像データを管理提供するクラス
    /// </summary>
    public class ProductReader
    {

        public class ProductData
        {
  
            /// <summary>
            /// 分類
            /// </summary>
            public string kind;
            /// <summary>
            /// 名称
            /// </summary>
            public string name;
            /// <summary>
            /// 原材料
            /// </summary>
            public string rawMaterials;
            /// <summary>
            /// 内容量
            /// </summary>
            public string amount;
            /// <summary>
            /// 賞味期限（日数）
            /// </summary>
            public int validDays;
            /// <summary>
            /// 保存方法
            /// </summary>
            public string storageMethod;
            /// <summary>
            /// アレルギー
            /// </summary>
            public string allergy;
            /// <summary>
            /// 製造者
            /// </summary>
            public string manufacturer;
            /// <summary>
            /// 欄外
            /// </summary>
            public string comment;
        }

        const string kind = "分類";
        const string name = "名称";
        const string rawMaterials = "原材料";
        const string amount = "内容量";
        const string validDays = "賞味期限";
        const string storageMethod = "保存方法";
        const string allergy = "アレルギー";
        const string manufacturer = "製造者";
        const string comment = "欄外";

        //Exel 列データフォーマット
        string[] colDataNames = new string[]
        {
            kind,name,rawMaterials,amount,validDays,storageMethod,allergy,manufacturer,comment
        };

        private Dictionary<string, int> dicColmunIndex = new Dictionary<string, int>();

        /// <summary>
        /// 商品基本データ
        /// </summary>
        private List<ProductData> lstProduct = new List<ProductData>();

        public int ReadExcel( string excelFilePath)
        {
            if ( !File.Exists(excelFilePath))
            {
                return -1;
            }


            string exePath = FormMain.GetExePath();

            var workbook = ExcelReader.GetWorkbook(excelFilePath, "xlsx");
            if( workbook==null)
            {
                MessageBox.Show($"{excelFilePath}\nを開けません");
                return -1;
            }

            //データクリア
            lstProduct.Clear();
    

            XSSFSheet sheet = (XSSFSheet)((XSSFWorkbook)workbook).GetSheetAt(0);

            bool bReadTitle = false;
            int iRow = 0;
            while (true)
            {
                List<ExcelReader.TextInfo> lstRowData;

                int rc =  ReadRowData( sheet,  iRow, out lstRowData);
                if( rc == -1)
                {
                    //読み込み終了
                    break;
                }
                if (lstRowData != null)
                {
                    if (!bReadTitle)
                    {
                        //最初の行をタイトル行とする。
                        bReadTitle = true;
                        dicColmunIndex.Clear();
                        //カラムインデックス ディクショナリを作成
                        foreach ( var name in colDataNames)
                        {
                            int index = lstRowData.FindIndex(x => string.Compare(x.Text, name, true) == 0);
                            if( index<0)
                            {
                                MessageBox.Show($"商品データベースに「{name}」項目が見つかりません。");
                                return -1;
                            }

                            dicColmunIndex[name] = index;
                        }


                    }
                    else
                    {
                        //取り合えず、順番に決め打ちでデータ作成


                        ProductData data = new ProductData();

                        data.kind           = lstRowData[dicColmunIndex[kind]].Text;
                        data.name           = lstRowData[dicColmunIndex[name]].Text;
                        data.rawMaterials   = lstRowData[dicColmunIndex[rawMaterials]].Text;
                        data.amount         = lstRowData[dicColmunIndex[amount]].Text;
                        data.validDays      = int.Parse(lstRowData[dicColmunIndex[validDays]].Text);
                        data.storageMethod  = lstRowData[dicColmunIndex[storageMethod]].Text;
                        data.allergy        = lstRowData[dicColmunIndex[allergy]].Text;
                        data.manufacturer   = lstRowData[dicColmunIndex[manufacturer]].Text;
                        data.comment        = lstRowData[dicColmunIndex[comment]].Text;

                        lstProduct.Add(data);

                    }
                }

                iRow++;
            }

            return 0;

        }

 
        private int ReadRowData(XSSFSheet sheet, int iRow, out List<ExcelReader.TextInfo> lstRowITems)
        {
            lstRowITems = null;

            int cellNum = ExcelReader.GetCellCount(sheet, iRow);
            if (cellNum < 0) return -1; //読み込み終了


            if (string.IsNullOrEmpty(ExcelReader.CellValue(sheet, iRow, 0)))
            {
                return 0;
            }
            lstRowITems = new List<ExcelReader.TextInfo>();

            for (int iCol = 0; iCol < cellNum; iCol++)
            {
                string text = ExcelReader.CellValue(sheet, iRow, iCol);

                ExcelReader.TextInfo textInfo = new ExcelReader.TextInfo();
                textInfo.col = iCol;
                textInfo.row = iRow;
                textInfo.textData = text.Replace("\n", "\r\n");
                lstRowITems.Add(textInfo);
            }

            return 0;

        }

        /// <summary>
        /// 種別一覧取得
        /// </summary>
        /// <returns></returns>
        public string[] GetKindList()
        {
            return lstProduct.Select(x => x.kind).Distinct().ToArray();
        }
        /// <summary>
        /// 商品名一覧取得
        /// </summary>
        /// <returns></returns>
        public string[] GetProductList(string kind)
        {
            List<ProductData> lst;
            if (kind == Const.SelectAll)
            {
                lst = lstProduct;
            }
            else
            {
                lst = lstProduct.FindAll(x => x.kind == kind);
            }
            return lst.Select(x => x.name).Distinct().ToArray();
        }
        /// <summary>
        /// 商品名称からその登録データを取得
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ProductData GetProductDataByName( string name)
        {
            if( string.IsNullOrEmpty(name)) return null;    
            return lstProduct.FirstOrDefault(x => x.name == name);
        }

    }
}
