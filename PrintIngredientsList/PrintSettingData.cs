using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrintIngredientsList
{
    internal class PrintSettingData
    {
        public string fontName = Const.defaultFontName; //フォント
        public int copyNum      = 1;      //セット枚数

        public float PrintTopGap = (float)18.5;
        public float PrintLeftGap = (float)19.0;



        //１つのラベルシールの領域サイズ
        public float LabelAreaWidth = 43;
        public float LabelAreaHeight = 65;


        //１つのラベル印刷領域における余白
        public float LabelAreaGapTop    = (float)2.0;
        public float LabelAreaGapBottom = (float)2.0;
        public float LabelAreaGapLeft   = (float)2.0;
        public float LabelAreaGapRight  = (float)2.0;

        //ラベル領域内の各セル（矩形）内の文字描画範囲余白サイズ
        public float CellBoxGapTop      = (float)0.4;
        public float CellBoxGapBottom   = (float)0.2;
        public float CellBoxGapLeft     = (float)0.2;
        public float CellBoxGapRight    = (float)0.2;

        public float TitleAreWidthMM = (float)10;

        //項目毎の基本フォントサイズ
        //範囲に入りきらない場合は、このフォントサイズから小さいフォントサイズに自動調整
        public float fontSizeProductTitle = 6;
        public float fontSizeMaterial = 5;
        public float fontSizeAmount = 7;
        public float fontSizeLimitDate = 7;
        public float fontSizeStorage = 7;
        public float fontSizeManifac = 6;
        public float fontSizeComment = 4;

        //行高さ
        public float hightProductTitle = 4;
        public float hightMaterial = 20;
        public float hightAmount = 4;
        public float hightLimitDate = 4;
        public float hightStorage = 6;
        public float hightManifac = 6;

        /// <summary>
        /// １つのラベルシールの描画可能領域幅(mm) 左右の余白を差し引いた値
        /// </summary>
        public float LabelDrawArealWidthMM
        {
            get
            {
                return LabelAreaWidth - (LabelAreaGapLeft + LabelAreaGapRight); //mm
            }
        }
        /// <summary>
        /// １つのラベルシールの描画可能領域高さ(mm) 上下の余白を差し引いた値
        /// </summary>
        public float LabelDrawAreaHeightMM
        {
            get
            {
                return LabelAreaHeight - (LabelAreaGapTop + LabelAreaGapBottom); //mm
            }
        }
        /// <summary>
        /// 右側セルの幅(mm)
        /// </summary>
        public float ContentAreWidthMM
        {
            get
            {
                return LabelDrawArealWidthMM - TitleAreWidthMM;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public float CellBoxGapSumHeight
        {
            get
            {
                return CellBoxGapTop + CellBoxGapBottom;
            }
        }
        public float CellBoxGapSumWidth
        {
            get
            {
                return CellBoxGapLeft + CellBoxGapRight;
            }
        }

        public void Write(string filePath)
        {
            using (var sw = new StreamWriter(filePath))
            {
                Write(sw, "FONTNAME", fontName);
                Write(sw, "PRINTTOPGAP", PrintTopGap);
                Write(sw, "PRINTLEFTGAP", PrintLeftGap);
                Write(sw, "LABELAREAGAPTOP", LabelAreaGapTop);
                Write(sw, "LABELAREAGAPBOTTOM", LabelAreaGapBottom);
                Write(sw, "LABELAREAGAPLEFT", LabelAreaGapLeft);
                Write(sw, "LABELAREAGAPRIGHT", LabelAreaGapRight);
                Write(sw, "CELLAREAGAPTOP", CellBoxGapTop);
                Write(sw, "CELLAREAGAPBOTTOM", CellBoxGapBottom);
                Write(sw, "CELLAREAGAPLEFT", CellBoxGapLeft);
                Write(sw, "CELLAREAGAPRIGHT", CellBoxGapRight);
                Write(sw, "COPYNUM", copyNum);

                Write(sw, "FONT_PRODUCT", fontSizeProductTitle);
                Write(sw, "FONT_MATERIAL", fontSizeMaterial);
                Write(sw, "FONT_AMOUNT", fontSizeAmount);
                Write(sw, "FONT_LIMITDATE", fontSizeLimitDate);
                Write(sw, "FONT_STORAGE", fontSizeStorage);
                Write(sw, "FONT_MANIFAC", fontSizeManifac);
                Write(sw, "FONT_COMMNETT", fontSizeComment);

                Write(sw, "HIGHT_PRODUCT", hightProductTitle);
                Write(sw, "HIGHT_MATERIAL", hightMaterial);
                Write(sw, "HIGHT_AMOUNT", hightAmount);
                Write(sw, "HIGHT_LIMITDATE", hightLimitDate);
                Write(sw, "HIGHT_STORAGE", hightStorage);
                Write(sw, "HIGHT_MANIFAC", hightManifac);

            }
        }

        private void Write(StreamWriter sw, string key, object value)
        {
            sw.WriteLine($"{key}:{value}");
        }
        public void Read(string filePath)
        {
            if (!File.Exists(filePath)) return;
            using (var sr = new StreamReader(filePath))
            {
                while (true)
                {
                    string s = sr.ReadLine();
                    if (s == null) return;

                    var valueItem = s.Split(':');
                    switch(valueItem[0])
                    {
                        case "FONTNAME":            fontName            = valueItem[1]; break;
                        case "COPYNUM":             copyNum             = int.Parse(valueItem[1]); break;
                        case "PRINTTOPGAP":         PrintTopGap         = float.Parse(valueItem[1]); break;
                        case "PRINTLEFTGAP":        PrintLeftGap        = float.Parse(valueItem[1]); break;
                        case "LABELAREAGAPTOP":     LabelAreaGapTop     = float.Parse(valueItem[1]); break;
                        case "LABELAREAGAPBOTTOM":  LabelAreaGapBottom  = float.Parse(valueItem[1]); break;
                        case "LABELAREAGAPLEFT":    LabelAreaGapLeft    = float.Parse(valueItem[1]); break;
                        case "LABELAREAGAPRIGHT":   LabelAreaGapRight   = float.Parse(valueItem[1]); break;
                        case "CELLAREAGAPTOP":      CellBoxGapTop       = float.Parse(valueItem[1]); break;
                        case "CELLAREAGAPBOTTOM":   CellBoxGapBottom    = float.Parse(valueItem[1]); break;
                        case "CELLAREAGAPLEFT":     CellBoxGapLeft      = float.Parse(valueItem[1]); break;
                        case "CELLAREAGAPRIGHT":    CellBoxGapRight     = float.Parse(valueItem[1]); break;


                        case "FONT_PRODUCT":        fontSizeProductTitle = float.Parse(valueItem[1]); break;
                        case "FONT_MATERIAL":       fontSizeMaterial      = float.Parse(valueItem[1]); break;
                        case "FONT_AMOUNT":         fontSizeAmount        = float.Parse(valueItem[1]); break;
                        case "FONT_LIMITDATE":      fontSizeLimitDate     = float.Parse(valueItem[1]); break;
                        case "FONT_STORAGE":        fontSizeStorage       = float.Parse(valueItem[1]); break;
                        case "FONT_MANIFAC":        fontSizeManifac       = float.Parse(valueItem[1]); break;
                        case "FONT_COMMNETT":       fontSizeComment       = float.Parse(valueItem[1]); break;


                        case "HIGHT_PRODUCT":      hightProductTitle      = float.Parse(valueItem[1]); break;
                        case "HIGHT_MATERIAL":     hightMaterial          = float.Parse(valueItem[1]); break;
                        case "HIGHT_AMOUNT":       hightAmount            = float.Parse(valueItem[1]); break;
                        case "HIGHT_LIMITDATE":    hightLimitDate         = float.Parse(valueItem[1]); break;
                        case "HIGHT_STORAGE":      hightStorage           = float.Parse(valueItem[1]); break;
                        case "HIGHT_MANIFAC":      hightManifac           = float.Parse(valueItem[1]); break;

                    }

                }
            }
        }
    }
}
