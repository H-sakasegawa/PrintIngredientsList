using Org.BouncyCastle.Asn1.Mozilla;
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

        public const string defaultFontName = "Meiryo UI";
        public const float defaultProdListFontSize = 9;

        /// <summary>
        /// 商品リストのホイール操作によるフォントサイズ変更値
        /// </summary>
        public const float prodListFontSizeInc = 0.5f;

        public const float defaultFontSize = 6;
        public const float defaultItemHeight = 4;


        public const int previewDlgBasicWidth = 400;
        public const int previewDlgBasicHeight = 600;

        public const string SettingFolderName = "Settings";
        public const string BarcodeDataFolderName = "Barcode";
        public const string SaveDataFileName = "save.dat";
        public const string SettingDataFineName = "setting.dat";

        public const string printLayoutDataFolderName = "Layout";


        public const string LicenseFileName = "license.dat";

    }
    internal class ItemName
    {
        public const string ID = "ID";
        public const string Kind = "分類";
        public const string Name = "名称";
        public const string Material = "原材料";
        public const string Amount = "内容量";
        public const string ValidDate = "賞味期限";
        public const string Storage = "保存方法";
        public const string Manifacture = "製造者";
        public const string Allergy = "アレルギー";
        public const string Supplementary = "欄外";

        public const string NutritionalInformation = "栄養成分表示";
        public const string Calorie = "熱量";
        public const string Protein = "たんぱく質";
        public const string Lipids = "脂質";
        public const string Carbohydrates = "炭水化物";
        public const string Salt = "食塩相当量";

        public const string Comment = "コメント";


        public const string Barcode = "BARCODE";
        public const string Icon = "ICON";

    }
}