﻿using ExcelReaderUtility;
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
        /// ID
        /// </summary>
        public string id;
        /// <summary>
        /// 印刷枚数
        /// </summary>
        public int numOfSheets=1;
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
        /// 製造者
        /// </summary>
        public string manufacturer;


        public EditProductData()
        {

        }
        public EditProductData(string parameter)
        {
            if(Parse(parameter)!=0)
            {
                throw new Exception();
            }
        }

        public object[] GetParams(ProductReader productReader)
        {

            var product = productReader.GetProductDataByID(id);

            //グリッドに表示する文字の一覧
            DateTime dt = Utility.GetValidDate(validDays);
            return new object[]
            {
                id,
                product.kind,
                product.name,
                numOfSheets,
                dt.ToShortDateString(), //日数ではなく、日付文字列
                amount,
                storageMethod,
                manufacturer
            };


        }


        public override string ToString()
        {
            return $"id:{id}" +
                   $",numOfSheets:{numOfSheets}" +
                   $",amount:{amount}" +
                   $",validDays:{validDays}" +
                   $",storageMethod:{storageMethod}" +
                   $",manufacturer:{manufacturer}" 
                   ;

        }
        public int Parse(string s)
        {
            var ary = s.Split(',');

            id = GetValue(ary, "id", "");
            if(string.IsNullOrEmpty(id))
            {
                return -1;
            }
            numOfSheets = int.Parse(GetValue(ary, "numOfSheets", "1"));
            amount = GetValue(ary, "amount", "");
            validDays =int.Parse( GetValue(ary, "validDays", "0"));
            storageMethod = GetValue(ary, "storageMethod", "");
            manufacturer = GetValue(ary, "manufacturer", "");

            return 0;
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
