using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.AxHost;

namespace PrintIngredientsList
{
    internal class DrawUtil2
    {
        //印刷ページの左上の余白（コンストラクタで確定）
        float PageGapTopMM = 0;
        float PageGapLeftMM = 0;

        //１つのラベル印刷領域における余白
        const float LabelAreaGapTop = (float)2.0;
        const float LabelAreaGapBottom = (float)2.0;
        const float LabelAreaGapLeft = (float)2.0;
        const float LabelAreaGapRight = (float)2.0;


        //ラベル領域内の各セル（矩形）内の文字描画範囲余白サイズ
        const float CellBoxGapTop = (float)0.2;
        const float CellBoxGapBottom = (float)0.2;
        const float CellBoxGapLeft = (float)0.2;
        const float CellBoxGapRight = (float)0.2;

        
        const float CellBoxGapSumHeight = (CellBoxGapTop + CellBoxGapBottom);
        const float CellBoxGapSumWidth = (CellBoxGapLeft + CellBoxGapRight);

        Graphics g = null;

        public const string fontName = "Meiryo UI";


        public const float cellWidthMM       = 43 - (LabelAreaGapLeft + LabelAreaGapRight); //mm
        public const float cellHeightMM      = 65 - (LabelAreaGapTop  + LabelAreaGapBottom); //mm
        const float titleAreWidthMM   = 10; //mm
        const float contentAreWidthMM = cellWidthMM - titleAreWidthMM;  //mm

        const float fontSizeDec =(float) 0.2;

        // |<---     cellWidthMM      -->|
        // |                             |
        // | titleAreWidthMM             |
        // |   ↓     contentAreWidthMM  |
        // |<------>|<------------------>|
        //  ----------------------------- <----
        // |        |                    |    ↑
        // |--------+--------------------|    ｜
        // |        |                    |    ｜
        // |--------+--------------------|    ｜
        // |        |                    |    ｜
        // |--------+--------------------|
        // |        |                    |    cellHeightMM 
        // |--------+--------------------|
        // |        |                    |    ｜
        // |--------+--------------------|    ｜
        // | 欄外                        |    ｜
        // |                             |    ｜
        // |                             |    ｜
        // |                             |    ↓
        //  ----------------------------- <-----


        ////mm → 座
        //float dpiX = 0;
        //float dpiY = 0;
        //private const float MillimetersPerInch = 25.4f;
        public DrawUtil2(Graphics g)
        {

            g.PageUnit = GraphicsUnit.Millimeter;
            this.g = g;
        }

        public DrawUtil2(Graphics g, float PageGapTopMM, float PageGapLeftMM)
        {
            g.PageUnit = GraphicsUnit.Millimeter;
            this.g = g;
            this.PageGapTopMM = PageGapTopMM;
            this.PageGapLeftMM = PageGapLeftMM;


            //  GapLeftMM
            // |<->|
            //  ----------------------------------------
            // |                                    ↕   GapTopMM
            // |   -----------------------------  ---- 
            // |   |        |                    |
            // |   |--------+--------------------|
            // |   |        |                    |
            // |   |--------+--------------------|
            // |   |        |                    |

        }

        public float DrawItemComment(string title, string content, float startY, float baseFontSize, bool bDrawFrame = true)
        {
            Font fntTitle = GetFontCalcedByWidth(baseFontSize, title, titleAreWidthMM);
            //備考欄の高さは、全エリアの高さから直前の描画開始位置を引いた残りから、セルギャップを差し引いた高さ
            float contentsLimitHightMM = cellHeightMM - startY - CellBoxGapSumHeight;
            float contentsHight = 0;

            Font fntContents = GetFontCalcedBydHeightAndWidth(baseFontSize, content, cellWidthMM, contentsLimitHightMM, ref contentsHight);

            DrawItem( content, startY, fntTitle, fntContents, contentsHight, (float)( contentsHight + Math.Ceiling(CellBoxGapBottom)),bDrawFrame);

            return startY + contentsHight;
        }
        public float DrawItem(string title, string content, float startY, float limitHight, float baseFontSize, bool bDrawFrame = true)
        {

            //タイトル領域は、横１列として表示可能なフォントサイズのフォントを取得
            Font fntTitle = GetFontCalcedByWidth(baseFontSize, title, titleAreWidthMM - CellBoxGapSumWidth, limitHight-CellBoxGapSumHeight );
            float contentsHight = 0;
            Font fntContents = GetFontCalcedBydHeight(baseFontSize, content, limitHight - CellBoxGapSumHeight, ref contentsHight);

            float cellHeight = contentsHight + (int)Math.Ceiling(CellBoxGapBottom);

            DrawItem( title,  content,  startY, fntTitle, fntContents, contentsHight, cellHeight,  bDrawFrame);

            return startY + cellHeight;

        }

        private void DrawItem(string title, string content, float startY, Font fntTitle, Font fntContens, float contentsHight, float gridHeight, bool bDrawFrame)
        {
            //グリッドセルのTop/Leftの余白
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            PointF point = new PointF(_X(CellBoxGapLeft), _Y(startY + CellBoxGapTop));
            g.DrawString(title, fntTitle, brs, point);
            point.X = titleAreWidthMM + CellBoxGapLeft;
            g.DrawString(content, fntContens, brs, new RectangleF(_X(point.X), _Y(startY), contentAreWidthMM, contentsHight));

            if (bDrawFrame)
            {
                //ライン描画
                Pen pen = new Pen(Color.Black, (float)0.1);
                g.DrawRectangle(pen,_X(0), _Y(startY), cellWidthMM, gridHeight);
                g.DrawLine(pen, new PointF(_X(titleAreWidthMM), _Y(startY)), new PointF(_X(titleAreWidthMM), _Y(startY + gridHeight)));
            }

        }
        private void DrawItem( string content, float startY, Font fntTitle, Font fntContens, float contentsHight, float gridHeight, bool bDrawFrame)
        {
            //グリッドセルのTop/Leftの余白
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            PointF point = new PointF(_X(CellBoxGapLeft), _Y(startY + CellBoxGapTop));
            g.DrawString(content, fntContens, brs, new RectangleF(point.X, point.Y, cellWidthMM - CellBoxGapSumWidth, contentsHight));

            if (bDrawFrame)
            {
                //ライン描画
                Pen pen = new Pen(Color.Black, (float)0.1);
                g.DrawRectangle(pen,_X(0), _Y(startY), cellWidthMM, gridHeight);
            }

        }

        float _X(float x) { return PageGapLeftMM + LabelAreaGapTop + x; }
        float _Y(float y) { return PageGapTopMM + LabelAreaGapLeft + y; }


        /// <summary>
        /// 領域の横サイズに入りきるフォントを取得
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        Font GetFontCalcedByWidth(float startFontSize ,string s, float limitWidth, float limitHight=0)
        {
            float fntSize = startFontSize;
            while (fntSize > 0)
            {
                Font font = new Font(fontName, fntSize, FontStyle.Regular);
                SizeF size = g.MeasureString(s, font);

                if( limitHight!=0 && size.Height > limitHight)
                {
                    fntSize-= fontSizeDec;
                    continue;

                }
                if (size.Width > limitWidth)
                {
                    fntSize-= fontSizeDec;
                    continue;
                }
                return font;
            }
            return null;
        }

        /// <summary>
        /// 領域の高さサイズに入りきるフォントを取得
        /// </summary>
        /// <param name="s"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Font GetFontCalcedBydHeight(float startFontSize, string s, float limitHeght, ref float height)
        {
            float fntSize = startFontSize;
            while (fntSize > 0)
            {
                Font font = new Font(fontName, fntSize, FontStyle.Regular);
                SizeF size = g.MeasureString(s, font, (int)contentAreWidthMM);

                height = size.Height;

                if (height < limitHeght)
                {
                    return font;
                }
                fntSize -= fontSizeDec;
            }
            return null;
        }
        /// <summary>
        /// 領域の高さサイズに入りきるフォントを取得
        /// </summary>
        /// <param name="s"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Font GetFontCalcedBydHeightAndWidth(float startFontSize, string s, float limitWidth,float limitHeght, ref float height)
        {
            float fntSize = startFontSize;
            while (fntSize > 0)
            {
                Font font = new Font(fontName, fntSize, FontStyle.Regular);
                SizeF size = g.MeasureString(s, font, (int)limitWidth);

                height = size.Height;

                if (height < limitHeght)
                {
                    return font;
                }
                fntSize -= fontSizeDec;
            }
            return null;
        }


        public static float MillimetersToPixels(float millimeters, float dpi)
        {
            float MillimetersPerInch = 25.4f;

            return (millimeters / MillimetersPerInch) * dpi;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="p1">ライン座標</param>
        /// <param name="p2">ライン座標</param>
        public void DrawLine(Pen pen, PointF p1, PointF p2)
        {
            g.DrawLine(pen, p1, p2);
        }

    }
}
