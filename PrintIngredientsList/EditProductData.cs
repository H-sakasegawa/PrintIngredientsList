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
        public string allergy;
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
            DateTime dt = Utility.GetValidDate(validDays);
            return new object[]
            {
                name,
                numOfSheets,
                dt.ToShortDateString(),
                amount,
                storageMethod,
                allergy,
                manufacturer
            };


        }


        public override string ToString()
        {
            return $"kind:{kind}" +
                   $",name:{name}" +
                   $",numOfSheets:{numOfSheets}" +
                   //$"rawMaterials:{rawMaterials}," +
                   $",amount:{amount}" +
                   //$"dtExpirationDate:{dtExpirationDate.ToShortDateString()}," +
                   $",validDays:{validDays}" +
                   $",storageMethod:{storageMethod}" +
                   $",allergy:{allergy}" +
                   $",manufacturer:{manufacturer}" 
                   //$"comment:{comment}"
                   ;

        }
        public void Parse(string s)
        {
            var ary = s.Split(',');

            kind = GetValue(ary, "kind", "");
            name = GetValue(ary, "name", "");
            numOfSheets = int.Parse(GetValue(ary, "numOfSheets", "1"));
            //rawMaterials = GetValue(ary, "rawMaterials", "");
            amount = GetValue(ary, "amount", "");
            //dtExpirationDate = DateTime.Parse(GetValue(ary, "dtExpirationDate", ""));
            validDays =int.Parse( GetValue(ary, "validDays", "0"));
            storageMethod = GetValue(ary, "storageMethod", "");
            allergy = GetValue(ary, "allergy", "");
            manufacturer = GetValue(ary, "manufacturer", "");
            //comment = GetValue(ary, "comment", "");


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
