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
                    }

                }
            }
        }
    }
}
