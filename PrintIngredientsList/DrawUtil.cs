using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.AxHost;

namespace PrintIngredientsList
{
    internal class DrawUtil
    {
        float GapTopMM = 0; 
        float GapLeftMM = 0;
        SizeF sizeGap;
        Graphics g = null;

        string fontName = "Meiryo UI";
        int fontSize = 10;
        Font font = new Font("Meiryo UI", 10, FontStyle.Regular);


        float areaWidthMM = 43; //mm → 162.51dpi

        int titleAreWidth = 60; //dpi
        int contentAreWidth = 0;

        //行の最大高さ
        int maxRowHeight = 150; //dip


        int areaWidth = 0; 
        int areaWidthPts = 0;
        float dpiX = 0;
        float dpiY = 0;

        private const float MillimetersPerInch = 25.4f;

        public DrawUtil(Graphics g, float GapTopMM, float GapLeftMM)
        {
            this.g = g;
            this.GapTopMM = GapTopMM;
            this.GapLeftMM = GapLeftMM;

            dpiX = g.DpiX;
            dpiY = g.DpiY;

            sizeGap = new SizeF(
                MillimetersToPixels(GapTopMM, dpiY),
                MillimetersToPixels(GapLeftMM, dpiX)
                );

            areaWidthPts = (int)MillimetersToPixels(areaWidthMM, dpiX);

            contentAreWidth = (int)(areaWidthPts - titleAreWidth);


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="p1">ライン座標</param>
        /// <param name="p2">ライン座標</param>
        public void DrawLine(Pen pen, PointF p1, PointF p2)
        {
            p1 = PointF.Add(p1, sizeGap);
            p2 = PointF.Add(p2, sizeGap);

            g.DrawLine(pen, p1, p2);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="p1">ボックス座標１点目</param>
        /// <param name="p2">ボックス座標２点目</param>
        public void DrawBox(Pen pen, PointF p1, PointF p2)
        {
            p1 = PointF.Add(p1, sizeGap);
            p2 = PointF.Add(p2, sizeGap);

            PointF[] pnt = new PointF[]
            {
               new PointF(p1.X, p1.Y ),
               new PointF(p1.X, p2.Y ),
               new PointF(p2.X, p2.Y ),
               new PointF(p2.X, p1.Y ),
            };
            for(int i=0; i<pnt.Length-1; i++)
            {
                g.DrawLine(pen, pnt[i], pnt[i+1]);
            }
            g.DrawLine(pen, pnt[pnt.Length - 1], pnt[0]);
        }

        public void SetFont(Font font)
        {
            this.font = font;
        }

        public int DrawItem(string title, string content, int startY, bool bDrawFrame=true)
        {
            PointF point = new PointF(0, startY);
            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            //描画横幅を指定して、実際に文字列が描画される領域サイズを計算
            SizeF szTitle = g.MeasureString(title, font, titleAreWidth);
            //SizeF szContents = g.MeasureString(content, font, contentAreWidth);

            //StringFormat stringFormat = new StringFormat();
            //stringFormat.Alignment = StringAlignment.

            //タイトル領域は、横１列として表示可能なフォントサイズのフォントを取得
            Font fnt = GetFontCalcedByWidth(title);
            g.DrawString(title, fnt, brs, point);

            int contentsHight = 0;
            fnt = GetFontCalcedBydHeight(content, ref contentsHight);

            point.X += titleAreWidth;
            g.DrawString(content, fnt, brs, new RectangleF(point.X, startY, contentAreWidth, contentsHight) );

            if (bDrawFrame)
            {
                //ライン描画
                Pen pen = new Pen(Color.Blue);
                g.DrawRectangle(pen, new Rectangle(0, startY, areaWidthPts, (int)contentsHight));
                g.DrawLine(pen, new PointF(titleAreWidth, startY), new PointF(titleAreWidth, startY + contentsHight));
            }
            return startY + (int)contentsHight;

        }

        private float MillimetersToPixels(float millimeters, float dpi) 
        {

            return (millimeters / MillimetersPerInch) * dpi;
        }
        
        /// <summary>
        /// 領域の横サイズに入りきるフォントを取得
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        Font GetFontCalcedByWidth(string s)
        {
            int fntSize = fontSize;
            while (fntSize > 0)
            {
                Font font = new Font(fontName, fntSize, FontStyle.Regular);
                SizeF size = g.MeasureString(s, font);

                if(size.Width < titleAreWidth)
                {
                    return font;
                }
                fntSize--;
            }
            return null;
        }

        /// <summary>
        /// 領域の高さサイズに入りきるフォントを取得
        /// </summary>
        /// <param name="s"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Font GetFontCalcedBydHeight(string s, ref int height)
        {
            int fntSize = fontSize;
            while (fntSize > 0)
            {
                Font font = new Font(fontName, fntSize, FontStyle.Regular);
                SizeF size = g.MeasureString(s, font, contentAreWidth);

                height = (int)size.Height;

                if (height < maxRowHeight)
                {
                    return font;
                }
                fntSize--;
            }
            return null;
        }

    }
}
