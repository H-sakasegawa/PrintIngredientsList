using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ExcelReaderUtility.ProductReader;

namespace PrintIngredientsList
{
    internal class Utility
    {
        /// <summary>
        /// 今日から指定された日数のDateTimeを返す
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static DateTime GetValidDate( int days)
        {
            var today = DateTime.Now;
            
            return today.Add(TimeSpan.FromDays(days - 1));

        }

        public static void MessageError(string msg)
        {
            MessageBox.Show(msg, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void MessageInfo(string msg, string title="情報")
        {
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static DialogResult MessageConfirm(string msg, string title)
        {
            return MessageBox.Show(msg, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        }
        public static int MILLI2POINT(float milli)
        {
            return (int)(milli / 0.352777);
        }
        public static float POINT2MILLI(int point)
        {
            return (float)(point * 0.352777);
        }

        public static float ToFloat(string value)
        {
            return float.Parse(value);
        }
        public static int ToInt(string value)
        {
            return int.Parse(value);
        }
        public static bool ToBoolean(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                int tmpValue = 0;
                int.TryParse(value, out tmpValue);
                return tmpValue == 0 ? false : true;
            }
            return false;
        }
    }
}
