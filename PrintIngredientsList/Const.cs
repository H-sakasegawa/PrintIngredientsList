using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintIngredientsList
{
    internal class Const
    {
        /// <summary>
        /// 商品データベース格納フォルダ（Exeファイルパス配下）
        /// </summary>
        public const string dataBaseFolder = "Database";
        /// <summary>
        /// 商品基本データベースファイル
        /// </summary>
        public const string ProductFileName = "商品.xlsx";
        /// <summary>
        /// 共通データベースファイル
        /// </summary>
        public const string CommonDefFileName = "共通.xlsx";

        public const string SelectAll = "全て";

        public const float GapTop = (float)18.5;
        public const float GapLeft = (float)19.0;


    }
}
