using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;

namespace PrintIngredientsList
{
    public class EditProductData
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
        /// 印刷枚数
        /// </summary>
        public int numOfSheets=1;
        /// <summary>
        /// 原材料
        /// </summary>
       // public string rawMaterials;
        /// <summary>
        /// 内容量
        /// </summary>
        public string amount;
        /// <summary>
        /// 賞味期限（日数）
        /// </summary>
        public int validDays;
        /// <summary>
        /// 賞味期限（日付）
        /// </summary>
        //public DateTime dtExpirationDate;
        /// <summary>
        /// 保存方法
        /// </summary>
        public string storageMethod;
        /// <summary>
        /// アレルギー
        /// </summary>
        //public string allergy;
        /// <summary>
        /// 製造者
        /// </summary>
        public string manufacturer;
        /// <summary>
        /// 欄外
        /// </summary>
        //public string comment;


        public EditProductData()
        {

        }
        public EditProductData(string parameter)
        {
            Parse(parameter);
        }


        public object[] GetParams()
        {
            //グリッドに表示する文字の一覧
            DateTime dt = Utility.GetValidDate(validDays);
            return new object[]
            {
                kind,
                name,
                numOfSheets,
                dt.ToShortDateString(), //日数ではなく、日付文字列
                amount,
                storageMethod,
                manufacturer
            };


        }


        public override string ToString()
        {
            return $"kind:{kind}" +
                   $",name:{name}" +
                   $",numOfSheets:{numOfSheets}" +
                   $",amount:{amount}" +
                   $",validDays:{validDays}" +
                   $",storageMethod:{storageMethod}" +
                   $",manufacturer:{manufacturer}" 
                   ;

        }
        public void Parse(string s)
        {
            var ary = s.Split(',');

            kind = GetValue(ary, "kind", "");
            name = GetValue(ary, "name", "");
            numOfSheets = int.Parse(GetValue(ary, "numOfSheets", "1"));
            amount = GetValue(ary, "amount", "");
            validDays =int.Parse( GetValue(ary, "validDays", "0"));
            storageMethod = GetValue(ary, "storageMethod", "");
            manufacturer = GetValue(ary, "manufacturer", "");


        }

        private string GetValue(string[] ary, string key, string sDefault=null)
        {
            var param = ary.FirstOrDefault(x => x.StartsWith($"{key}:"));
            if (param == null) return sDefault;

            var items = param.Split(':');

            return items[1];

        }

    }
}
