using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrintIngredientsList
{
    public class PrintSettingData
    {
        public int copyNum      = 1;      //セット枚数
        public int printStartPos = 1;       //印刷開始位置
        public string fontName = Const.defaultFontName;

        //項目毎の基本フォントサイズ
        //範囲に入りきらない場合は、このフォントサイズから小さいフォントサイズに自動調整
        //public float fontSizeProductTitle = 6;
        //public float fontSizeMaterial = 5;
        //public float fontSizeAmount = 7;
        //public float fontSizeLimitDate = 7;
        //public float fontSizeStorage = 7;
        //public float fontSizeManifac = 6;
        //public float fontSizeComment = 4;

        ////行高さ
        //public float hightProductTitle = 4;
        //public float hightMaterial = 20;
        //public float hightAmount = 4;
        //public float hightLimitDate = 4;
        //public float hightStorage = 6;
        //public float hightManifac = 6;


        public void Write(string filePath)
        {
            using (var sw = new StreamWriter(filePath))
            {
                Write(sw, "COPYNUM", copyNum);
                Write(sw, "FONTNAME", fontName);

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
                        case "COPYNUM":
                            copyNum = int.Parse(valueItem[1]);
                            break;
                        case "FONTNAME":
                            fontName = valueItem[1];
                            break;

                    }

                }
            }
        }
    }
}
