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

        LabelType labelType;
        LabelTypeBlock labelBlock;
        Graphics graphics = null;

        float fontSizeItemTitle = 6;

        //印刷ページの左上の余白（コンストラクタで確定）
        float PageGapTopMM = 0;
        float PageGapLeftMM = 0;

        //--------------------------------------------------
        // Init()関数で設定
        //--------------------------------------------------
        public float labelDrawAreaWidthMM = 0; //mm
        public float labelDrawAreaHeightMM = 0; //mm
        float labelBlockWidthMM = 0;
        float titleAreWidthMM          = 0; //mm
        float contentAreWidthMM        = 0;  //mm

        float CellBoxGapSumHeight = 0;
        float CellBoxGapSumWidth = 0;

        float LabelBlockOfsX = 0;
        float LabelBlockOfsY = 0;

        const float fontSizeDec =(float) 0.2;

        // |<--- labelDrawAreaWidthMM -->|
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
        // |        |                    |   labelDrawAreaHeightMM
        // |--------+--------------------|
        // |        |                    |    ｜
        // |--------+--------------------|    ｜
        // | 欄外                        |    ｜
        // |                             |    ｜
        // |                             |    ｜
        // |                             |    ↓
        //  ----------------------------- <-----

        public void SetDrawLabelBlockOffset(float x, float y)
        {
            LabelBlockOfsX = x;
            LabelBlockOfsY = y;
        }

        ////mm → 座
        //float dpiX = 0;
        //float dpiY = 0;
        //private const float MillimetersPerInch = 25.4f;
        public DrawUtil2(Graphics g, LabelType labelType,  bool bDrawLabelBackground)
        {
            this.graphics = g;
            fontSizeItemTitle = settingData.fontSizeTitle;
            this.graphics.PageUnit = GraphicsUnit.Millimeter;

            this.labelType = labelType;

            Init(bDrawLabelBackground);
        }

        public DrawUtil2(Graphics g, LabelType labelType,  float PageGapTopMM, float PageGapLeftMM, bool bDrawLabelBackground)
        {

            this.graphics = g;
            fontSizeItemTitle = settingData.fontSizeTitle;
            this.graphics.PageUnit = GraphicsUnit.Millimeter;

            this.labelType = labelType;
            this.PageGapTopMM = PageGapTopMM;
            this.PageGapLeftMM = PageGapLeftMM;

            Init(bDrawLabelBackground);

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

        private void Init(bool bDrawLabelBackground)
        {
            labelDrawAreaWidthMM = labelType.LabelDrawArealWidthMM;
            labelDrawAreaHeightMM = labelType.LabelDrawAreaHeightMM;

            CellBoxGapSumHeight = labelType.CellBoxGapSumHeight;
            CellBoxGapSumWidth = labelType.CellBoxGapSumWidth;
            if (bDrawLabelBackground)
            {
                //背景描画
                graphics.Clear(Color.Gray);
                //ラベル背景描画
                SolidBrush brs = new SolidBrush(Color.White);
                graphics.FillRectangle(brs, new RectangleF(0, 0, labelType.width, labelType.height));
            }
        }
        public  void SetTargetLabelBlock(LabelTypeBlock labelBlock)
        {
            this.labelBlock = labelBlock;

            labelBlockWidthMM = labelBlock.LabelBlockWidth;
            titleAreWidthMM = labelBlock.TitleAreWidthMM;
            contentAreWidthMM = labelBlock.ContentAreWidthMM;

            fontSizeItemTitle = labelBlock.titleFontSize;
        }

        public float DrawItemComment(string title, string content, float startY, float baseFontSize, bool bDrawFrame = true)
        {
            Font fntTitle = new Font(labelType.fontName, fontSizeItemTitle, FontStyle.Regular);

            //Font fntTitle = GetFontCalcedByWidth(baseFontSize, title, titleAreWidthMM);
            //備考欄の高さは、全エリアの高さから直前の描画開始位置を引いた残りから、セルギャップを差し引いた高さ
            float contentsLimitHightMM = labelDrawAreaHeightMM - (startY + CellBoxGapSumHeight);
            float contentsHight = 0;

            Font fntContents = GetFontCalcedBydHeightAndWidth(baseFontSize, content, labelBlockWidthMM, contentsLimitHightMM, ref contentsHight);

            DrawItem( content, startY, fntTitle, fntContents, contentsHight, (float)( contentsHight + Math.Ceiling(labelType.celGapBottom)),bDrawFrame);

            return startY + contentsHight;
        }

        public float DrawItem(string title, string content, float startY, float limitHight, float baseFontSize,bool bResizeHeight=false, bool bDrawFrame = true)
        {

            //タイトル領域は、横１列として表示可能なフォントサイズのフォントを取得
            Font fntTitle = new Font(labelType.fontName, fontSizeItemTitle, FontStyle.Regular);
            //Font fntTitle = new Font(GetFontCalcedByWidth(baseFontSize, title, titleAreWidthMM - CellBoxGapSumWidth, limitHight-CellBoxGapSumHeight );
            float contentsHight = 0;
            Font fntContents = GetFontCalcedBydHeight(baseFontSize, content, limitHight - CellBoxGapSumHeight, ref contentsHight);

            float cellHeight = limitHight;
            if (bResizeHeight)
            {
                cellHeight = contentsHight + (int)Math.Ceiling(labelType.CellBoxGapSumHeight);
            }


            DrawItem( title,  content,  startY, fntTitle, fntContents, contentsHight, cellHeight,  bDrawFrame);

            return startY + cellHeight;

        }

        private void DrawItem(string title, string content, float startY, Font fntTitle, Font fntContens, float contentsHight, float gridHeight, bool bDrawFrame)
        {
            //グリッドセルのTop/Leftの余白
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            PointF point = new PointF(_X(labelType.celGapLeft), _Y(startY + labelType.celGapTop));
            graphics.DrawString(title, fntTitle, brs, point);
            point.X = titleAreWidthMM + labelType.celGapLeft;
            graphics.DrawString(content, fntContens, brs, new RectangleF(_X(point.X), _Y(startY + labelType.celGapTop), contentAreWidthMM, contentsHight));

            if (bDrawFrame)
            {
                //ライン描画
                Pen pen = new Pen(Color.Black, (float)0.1);
                graphics.DrawRectangle(pen,_X(0), _Y(startY), labelBlockWidthMM, gridHeight);
                graphics.DrawLine(pen, new PointF(_X(titleAreWidthMM), _Y(startY)), new PointF(_X(titleAreWidthMM), _Y(startY + gridHeight)));
            }

        }
        private void DrawItem( string content, float startY, Font fntTitle, Font fntContens, float contentsHight, float gridHeight, bool bDrawFrame)
        {
            //グリッドセルのTop/Leftの余白
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            PointF point = new PointF(_X(labelType.celGapLeft), _Y(startY + labelType.celGapTop));
            graphics.DrawString(content, fntContens, brs, new RectangleF(point.X, point.Y, labelBlockWidthMM - CellBoxGapSumWidth, contentsHight));

            if (bDrawFrame)
            {
                //ライン描画
                Pen pen = new Pen(Color.Black, (float)0.1);
                graphics.DrawRectangle(pen,_X(0), _Y(startY), labelBlockWidthMM, gridHeight);
            }

        }

        float _X(float x) { return PageGapLeftMM + LabelBlockOfsX +  labelType.gapLeft + x; }
        float _Y(float y) { return PageGapTopMM + LabelBlockOfsY +  labelType.gapTop + y; }


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
                Font font = new Font(labelType.fontName, fntSize, FontStyle.Regular);
                SizeF size = graphics.MeasureString(s, font);

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
                Font font = new Font(labelType.fontName, fntSize, FontStyle.Regular);
                SizeF size = graphics.MeasureString(s, font, (int)contentAreWidthMM);

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
                Font font = new Font(labelType.fontName, fntSize, FontStyle.Regular);
                SizeF size = graphics.MeasureString(s, font, (int)limitWidth);

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
            graphics.DrawLine(pen, p1, p2);
        }

        public void DrawImage(string imageFilePath, float x, float y, float width, float height )
        {
            Image img = System.Drawing.Image.FromFile(imageFilePath);

            graphics.DrawImage(img, _X(x), _Y(y),width, height);

            img.Dispose();
        }

    }
}
