using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrintIngredientsList
{
    public class AppSettingData
    {
        public int copyNum      = 1;      //セット枚数
        public int printStartPos = 1;       //印刷開始位置
        public string fontName = Const.defaultFontName;

        //メイン画面の商品リストのフォント名
        public string prodListFontName = Const.defaultFontName;
        public float prodListFontSize = Const.defaultProdListFontSize;
        public float editProdListFontSize = Const.defaultProdListFontSize;

        public int[] prodListColWidthAry = { 
                                              40, //CHECK
                                              50, //ID
                                              60, //種別
                                             150, //名称
                                              50, //枚数
                                             100, //賞味期限
                                              50, //内容量
                                              70, //保存方法
                                             100  //欄外
                                              };


        public void Write(string filePath)
        {
            using (var sw = new StreamWriter(filePath))
            {
                Write(sw, "COPYNUM", copyNum);
                Write(sw, "FONTNAME", fontName);
                Write(sw, "PRODLIST_FONTNAME", prodListFontName);
                Write(sw, "PRODLIST_FONTSIZEE", prodListFontSize);
                Write(sw, "EDIT_PRODLIST_FONTSIZEE", editProdListFontSize);
                Write(sw, "PRODLIST_COL_WIDTH", string.Join(",",prodListColWidthAry));

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
                        case "PRODLIST_FONTNAME":
                            prodListFontName = valueItem[1];
                            break;
                        case "PRODLIST_FONTSIZEE":
                            prodListFontSize = float.Parse(valueItem[1]);
                            break;
                        case "EDIT_PRODLIST_FONTSIZEE":
                            editProdListFontSize = float.Parse(valueItem[1]);
                            break;
                        case "PRODLIST_COL_WIDTH":
                            {
                                var ary = valueItem[1].Split(',');
                                prodListColWidthAry = ary.Select(a => int.Parse(a)).ToArray();
                            }
                            break;

                    }

                }
            }
        }
    }
}
