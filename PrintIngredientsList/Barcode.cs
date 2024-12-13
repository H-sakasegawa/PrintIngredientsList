using NPOI.OpenXmlFormats.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace PrintIngredientsList
{
    internal class Barcode
    {

        public int Height { get; set; } = 40;
        public int Width { get; set; } = 150;
        public int Gap { get; set; } = 5;
        public BarcodeFormat BarcodeFormat { get; set; } = BarcodeFormat.CODE_128;


        public int CreateBarcode(string codeValue, bool bDispNo, string barcodeImageFilePath, ImageFormat imgFormat)
        {
            if (imgFormat is null)
            {
                throw new ArgumentNullException(nameof(imgFormat));
            }

            var bacodeWriter = new BarcodeWriter();

            // バーコードの種類
            bacodeWriter.Format = BarcodeFormat;

            // サイズ
            bacodeWriter.Options.Height = Height;
            bacodeWriter.Options.Width = Width;

            // バーコード左右の余白
            bacodeWriter.Options.Margin = Gap;

            // バーコードのみ表示するか
            // falseにするとテキストも表示する
            bacodeWriter.Options.PureBarcode = !bDispNo;

            // バーコードBitmapを作成
            using (var bitmap = bacodeWriter.Write(codeValue))
            {
                System.IO.File.Delete(barcodeImageFilePath);
                // 画像として保存
                bitmap.Save(barcodeImageFilePath, imgFormat); // ImageFormat.Png);
            }

            return 0;

        }

    }
}
