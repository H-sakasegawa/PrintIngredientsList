
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;

using NPOI.XSSF.UserModel;

using PrintIngredientsList;
using NPOI.SS.UserModel;

namespace ExcelReaderUtility
{
    /// <summary>
    /// 共通定義Excel ファイルクラス
    /// </summary>
    public class CommonDeftReader
    {

        public class CommonDefDataItem
        {

            /// <summary>
            /// 選択項目
            /// </summary>
            public string selectName;
            /// <summary>
            /// 印刷文字列
            /// </summary>
            public string printText;
        }
        public class CommonDefData
        {
            public CommonDefData(CommonDefDataItem item )
            {
                lstCommonDataItems.Add(item);
            }
            public List<CommonDefDataItem> lstCommonDataItems = new List<CommonDefDataItem>();
        }

        /// <summary>
        /// 商品基本データ
        /// </summary>
        private Dictionary<string, CommonDefData> dicCommonDef = new Dictionary<string, CommonDefData>();

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
                    }
                    else
                    {
                        CommonDefDataItem data = new CommonDefDataItem();

                        string name = lstRowData[0].Text;
                        data.selectName = lstRowData[1].Text;
                        data.printText = lstRowData[2].Text;

                        if (dicCommonDef.ContainsKey(name))
                        {
                            dicCommonDef[name].lstCommonDataItems.Add(data);
                        }else
                        {
                            dicCommonDef.Add(name, new CommonDefData(data));
                        }
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

        public string[] GetSelectText(string keyName)
        {
            if (!dicCommonDef.ContainsKey(keyName)) return null;

            return dicCommonDef[keyName].lstCommonDataItems.Select(x => x.selectName).ToArray();
        }

        public CommonDefDataItem GetCommonDefData(string keyName, string selectName)
        {
            if (!dicCommonDef.ContainsKey(keyName)) return null;
            return dicCommonDef[keyName].lstCommonDataItems.FirstOrDefault(x => x.selectName == selectName);

        }
    }
}
