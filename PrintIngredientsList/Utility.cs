using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
