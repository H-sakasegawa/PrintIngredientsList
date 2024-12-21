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
            /// ID(JANコード）
            /// </summary>
            public string id;
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
            /// <summary>
            /// 熱量
            /// </summary>
            public string Calorie;
            /// <summary>
            /// たんぱく質
            /// </summary>
            public string Protein;
            /// <summary>
            /// 脂質
            /// </summary>
            public string Lipids;
            /// <summary>
            /// 炭水化物
            /// </summary>
            public string Carbohydrates;
            /// <summary>
            /// 食塩相当量
            /// </summary>
            public string Salt;



            public override string ToString()
            {
                return $"({id}) {name}";
            }
        }

        //Exel 列データフォーマット
        string[] colDataNames = new string[]
        {
            ItemName.ID,
            //ItemName.Kind,
            ItemName.Name,
            ItemName.Material,
            ItemName.Amount,
            ItemName.ValidDate,
            ItemName.Storage,
            ItemName.Allergy,
            ItemName.Manifacture,
            ItemName.Supplementary,
            //-----------------------------
            ItemName.Calorie,
            ItemName.Protein,
            ItemName.Lipids,
            ItemName.Carbohydrates,
            ItemName.Salt,

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
                Utility.MessageError($"{excelFilePath}\nを開けません");
                return -1;
            }

            //データクリア
            lstProduct.Clear();
            int sheetNum = workbook.NumberOfSheets;

            for (int iSheet = 0; iSheet < sheetNum; iSheet++)
            {
                XSSFSheet sheet = (XSSFSheet)((XSSFWorkbook)workbook).GetSheetAt(iSheet);

                string sheetName = sheet.SheetName;

                bool bReadTitle = false;
                int iRow = 0;
                while (true)
                {
                    List<ExcelReader.TextInfo> lstRowData;

                    int rc = ReadRowData(sheet, iRow, out lstRowData);
                    if (rc == -1)
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
                            foreach (var name in colDataNames)
                            {
                                int index = lstRowData.FindIndex(x => string.Compare(x.Text, name, true) == 0);
                                if (index < 0)
                                {
                                    dicColmunIndex[name] = -1;
                                    continue;
                                    //Utility.MessageError($"商品データベースに「{name}」項目が見つかりません。");
                                    //return -1;
                                }

                                dicColmunIndex[name] = index;
                            }


                        }
                        else
                        {
                            //最低でもSupplementaryまでのデータが入力されていなければ読み込み対象外とする
                            if (lstRowData.Count > dicColmunIndex[ItemName.Supplementary])
                            {
                                //取り合えず、順番に決め打ちでデータ作成
                                ProductData data = new ProductData();
                                data.kind = sheetName;
                                GetExcelRowData(lstRowData, ItemName.ID, ref data.id);
                                //GetExcelRowData(lstRowData, ItemName.Kind, ref data.kind);
                                GetExcelRowData(lstRowData, ItemName.Name, ref data.name);
                                GetExcelRowData(lstRowData, ItemName.Material, ref data.rawMaterials);
                                GetExcelRowData(lstRowData, ItemName.Amount, ref data.amount);
                                GetExcelRowData(lstRowData, ItemName.ValidDate, ref data.validDays);
                                GetExcelRowData(lstRowData, ItemName.Storage, ref data.storageMethod);
                                GetExcelRowData(lstRowData, ItemName.Allergy, ref data.allergy);
                                GetExcelRowData(lstRowData, ItemName.Manifacture, ref data.manufacturer);
                                GetExcelRowData(lstRowData, ItemName.Supplementary, ref data.comment);

                                GetExcelRowData(lstRowData, ItemName.Calorie, ref data.Calorie);
                                GetExcelRowData(lstRowData, ItemName.Protein, ref data.Protein);
                                GetExcelRowData(lstRowData, ItemName.Lipids, ref data.Lipids);
                                GetExcelRowData(lstRowData, ItemName.Carbohydrates, ref data.Carbohydrates);
                                GetExcelRowData(lstRowData, ItemName.Salt, ref data.Salt);

                                var wk = lstProduct.Find(x => x.id == data.id);

                                if (wk != null)
                                {
                                    //IDの重複
                                    Utility.MessageError($"商品データベースに重複したIDがあります。\n({wk.id}) {wk.name}\n({data.id}) {data.name}\n\n重複しないIDを設定してください");
                                    return -1;
                                }
                                lstProduct.Add(data);
                            }

                        }
                    }

                    iRow++;
                }
            }

            return 0;

        }

        private int GetExcelRowData(List<ExcelReader.TextInfo> rowData, string itemKeyName, ref string value)
        {
            value = "";
            if (dicColmunIndex[itemKeyName] >= 0)
            {
                if (dicColmunIndex[itemKeyName] < rowData.Count)
                {
                    value = rowData[dicColmunIndex[itemKeyName]].Text;
                }
            }
            return 0;
        }

        private int GetExcelRowData(List<ExcelReader.TextInfo> rowData, string itemKeyName, ref int value)
        {
            value = 0;
            if (dicColmunIndex[itemKeyName] >= 0)
            {
                if (dicColmunIndex[itemKeyName] + 1 < rowData.Count)
                {
                    if (int.TryParse(rowData[dicColmunIndex[itemKeyName]].Text, out value) == false)
                    {
                        return -1;
                    }
                }
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
        public List<ProductData> GetProductList(string kind)
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
            return lst;
        }
        /// <summary>
        /// 商品IDからその登録データを取得
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ProductData GetProductDataByID( string id)
        {
            if( string.IsNullOrEmpty(id)) return null;    
            return lstProduct.FirstOrDefault(x => x.id == id);
        }

    }
}
