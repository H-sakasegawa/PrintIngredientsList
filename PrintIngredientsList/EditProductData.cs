using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string rawMaterials;
        /// <summary>
        /// 内容量
        /// </summary>
        public string amount;
        /// <summary>
        /// 賞味期限（日数）
        /// </summary>
        //public int validDays;
        /// <summary>
        /// 賞味期限（日付）
        /// </summary>
        public DateTime dtExpirationDate;
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

        public object[] GetParams()
        {
            return new object[]
            {
                name,
                numOfSheets,
                dtExpirationDate,
                amount,
                storageMethod,
                allergy,
                manufacturer
            };


        }
    }
}
