﻿using NPOI.SS.Formula.Functions;
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
using System.Windows.Media.TextFormatting;
//using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

namespace PrintIngredientsList
{
    internal class DrawUtil2
    {

        LabelType labelType;
        LabelTypeBlock labelBlock;
        Graphics graphics = null;

        string fontName = Const.defaultFontName;
        float fontSizeItemTitle = 6;
        float fontSizeItemHeader = 6;

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
        float contentAreaWidthMM        = 0;  //mm

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
        public DrawUtil2(Graphics g, LabelType labelType, string fontName, bool bDrawLabelBackground=false)
        {
            this.graphics = g;
            this.graphics.PageUnit = GraphicsUnit.Millimeter;
            this.fontName = fontName;
            this.labelType = labelType;

            Init(bDrawLabelBackground);
        }

        public DrawUtil2(Graphics g, LabelType labelType, string fontName,  float PageGapTopMM, float PageGapLeftMM, bool bDrawLabelBackground)
        {

            this.graphics = g;
            this.graphics.PageUnit = GraphicsUnit.Millimeter;
            this.fontName = fontName;

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
                graphics.FillRectangle(brs, new RectangleF(0, 0, labelType.Width, labelType.Height));
            }
        }
        public  void SetTargetLabelBlock(LabelTypeBlock labelBlock)
        {
            this.labelBlock = labelBlock;

            fontSizeItemTitle = labelBlock.TitleFontSize;

            labelBlockWidthMM = labelBlock.LabelBlockWidth;
            titleAreWidthMM = labelBlock.TitleWidth;
            contentAreaWidthMM = labelBlock.ValueWidth;

            fontSizeItemTitle = labelBlock.TitleFontSize;
        }

        /// <summary>
        /// コメント描画
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="startY"></param>
        /// <param name="baseFontSize"></param>
        /// <param name="bDrawFrame"></param>
        /// <returns></returns>
        public float DrawItemComment(string comment, float startY, float baseFontSize, bool bDrawFrame = true)
        {
           // Font fntTitle = new Font(fontName, fontSizeItemTitle, FontStyle.Regular);

            //Font fntTitle = GetFontCalcedByWidth(baseFontSize, title, titleAreWidthMM);
            //備考欄の高さは、全エリアの高さから直前の描画開始位置を引いた残りから、セルギャップを差し引いた高さ
            float contentsLimitHightMM = labelDrawAreaHeightMM - (startY + CellBoxGapSumHeight);
            float contentsHight = 0;

            Font fntContents = GetFontCalcedBydHeightAndWidth(baseFontSize, comment, labelBlockWidthMM, contentsLimitHightMM, ref contentsHight);

            DrawItemOther(comment, startY,  fntContents, contentsHight, (float)(contentsHight + Math.Ceiling(labelType.CelGapBottom)), labelBlockWidthMM, bDrawFrame);


            return startY + contentsHight;
        }

        //=============================================
        //欄外
        //=============================================
        /// <summary>
        /// 欄外
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="startY"></param>
        /// <param name="baseFontSize"></param>
        /// <param name="bDrawFrame"></param>
        /// <returns></returns>
        public float DrawItemSupplementary( string title,
                                            string content, 
                                            float startY, 
                                            float baseFontSize, 
                                            float width,
                                            ref bool result,
                                            bool bDrawFrame = true
            )
        {

            //Font fntTitle = GetFontCalcedByWidth(baseFontSize, title, titleAreWidthMM);
            //備考欄の高さは、全エリアの高さから直前の描画開始位置を引いた残りから、セルギャップを差し引いた高さ
            float contentsLimitHightMM = labelDrawAreaHeightMM - (startY + CellBoxGapSumHeight);
            float contentsHight = 0;

            float drawAreaW = labelBlockWidthMM - CellBoxGapSumWidth;

            Font fntContents;
            if (width > 0)
            {   //欄外幅指定あり
                drawAreaW = width;
            }
            fntContents = GetFontCalcedBydHeightAndWidth(baseFontSize, content, drawAreaW, contentsLimitHightMM, ref contentsHight);
            result = DrawItemOther( content, startY,  fntContents, contentsHight, (float)( contentsHight + Math.Ceiling(labelType.CelGapBottom)), drawAreaW, bDrawFrame);

            return startY + contentsHight;
        }
        private bool DrawItemOther(string content, 
                                           float startY,
                                           Font fntContens, 
                                           float contentsHight, 
                                           float gridHeight, 
                                           float drawAreaW,
                                           bool bDrawFrame
            )
        {
            bool bDrawHeightResult = true;

            //グリッドセルのTop/Leftの余白
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            PointF point = new PointF(_X(labelType.CelGapLeft), _Y(startY + labelType.CelGapTop));
            graphics.DrawString(content, fntContens, brs, new RectangleF(point.X, point.Y, drawAreaW, contentsHight));

            //---------------------------------------------------------
            // 描画した文字列の高さが グリッド高さをこえていないか？
            SizeF size = graphics.MeasureString(content, fntContens, (int)drawAreaW);

            if (size.Height > gridHeight)
            {
                bDrawHeightResult = false;
            }

            if (bDrawFrame)
            {
                //ライン描画
                Pen pen = new Pen(Color.Black, (float)0.1);
                graphics.DrawRectangle(pen, _X(0), _Y(startY), labelBlockWidthMM, gridHeight);
            }
            return bDrawHeightResult;

        }

        //=============================================
        //ラベル項目
        //=============================================
        public float DrawItem(float startY,
                              string content,
                              LabelItem labelItem)
        {
            string title        = labelItem.Title;
            float titleX        = 0;
            float titleY        = startY;
            float titleWidth    = labelItem.parent.TitleWidth;
            float titleHeight   = labelItem.Height;
            bool bDrawFrame     = labelItem.DrawFrame;

            float valueX        = titleAreWidthMM;
            float valueY        = startY;
            float valueWidth    = labelItem.parent.ValueWidth;
            float valueHeight   = labelItem.Height;
            float valueFontSize = labelItem.FontSize;

            //Itemブロック固有設定
            if( labelItem.ValueWidth>0)
            {
                valueWidth = labelItem.ValueWidth;
            }

            if ( labelItem.bCustomize)
            {
                //カスタマイズラベル
                LabelItemCustomize custmizeItem = (LabelItemCustomize)labelItem;
                titleX = custmizeItem.TitlePosX;
                titleY = custmizeItem.TitlePosY;
                titleWidth = custmizeItem.TitleWidth;
                titleHeight = custmizeItem.TitleHeight;
                valueX = custmizeItem.ValuePosX;
                valueY = custmizeItem.ValuePosY; ;
                valueWidth = custmizeItem.ValueWidth;;
                valueHeight = custmizeItem.ValueHeight;

            }

            //項目タイトル描画
            DrawItemTitle(title, titleX, titleY, titleWidth, titleHeight, bDrawFrame);

            //項目値描画
            float nextY = DrawItemValue(content, valueX, valueY, valueWidth, valueHeight, valueFontSize, bDrawFrame);

            return nextY;

        }
        private void DrawItemTitle(string title, float x, float y, float width, float height, bool bDrawFrame)
        {
            //タイトル領域は、横１列として表示可能なフォントサイズのフォントを取得
            Font fntTitle = new Font(fontName, fontSizeItemTitle, FontStyle.Regular);

            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            PointF point = new PointF(_X(x), _Y(y));
            graphics.DrawString(title, fntTitle, brs, point);

            if (bDrawFrame)
            {
                Pen pen = new Pen(Color.Black, (float)0.1);
                graphics.DrawRectangle(pen, _X(x), _Y(y), width, height);
            }

        }

        private float DrawItemValue(string value, float x, float y, float width, float height, float baseFontSize, bool bDrawFrame)
        {
            float contentsHight = 0;

            float areaWidth = width - CellBoxGapSumWidth;
            float areaHeight = height - CellBoxGapSumHeight;

            Font fntContents = GetFontCalcedBydHeightAndWidth(baseFontSize, value, areaWidth, areaHeight, ref contentsHight);

            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            PointF point = new PointF(_X(x + labelType.CelGapLeft), _Y(y + labelType.CelGapTop));
            graphics.DrawString(value, fntContents, brs, new RectangleF(point.X, point.Y, areaWidth, areaHeight));

            if (bDrawFrame)
            {
                Pen pen = new Pen(Color.Black, (float)0.1);
                graphics.DrawRectangle(pen, _X(x), _Y(y), width, height);
            }
            return y + height;

        }



        public float DrawItem(  string title, 
                            string content, 
                            float startY,
                            float limitHight,
                            float baseFontSize,
                            bool bResizeHeight=false, 
                            bool bDrawFrame = true
        )
        {

            //タイトル領域は、横１列として表示可能なフォントサイズのフォントを取得
            Font fntTitle = new Font(fontName, fontSizeItemTitle, FontStyle.Regular);
            float contentsHight = 0;
            Font fntContents = GetFontCalcedByHeight(baseFontSize, content, limitHight - CellBoxGapSumHeight, ref contentsHight);

            float cellHeight = limitHight;
            if (bResizeHeight)
            {
                cellHeight = contentsHight + (int)Math.Ceiling(labelType.CellBoxGapSumHeight);
            }


            DrawItemContents( title,  content,  startY, fntTitle, fntContents, contentsHight, cellHeight,  bDrawFrame);

            return startY + cellHeight;

        }

        private void DrawItemContents(string title, 
                                      string content, 
                                      float startY, 
                                      Font fntTitle, 
                                      Font fntContens, 
                                      float contentsHight,
                                      float gridHeight,
                                      bool bDrawFrame
            )
        {
            //グリッドセルのTop/Leftの余白
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            PointF point = new PointF(_X(labelType.CelGapLeft), _Y(startY + labelType.CelGapTop));
            graphics.DrawString(title, fntTitle, brs, point);
            point.X = titleAreWidthMM + labelType.CelGapLeft;
            graphics.DrawString(content, fntContens, brs, new RectangleF(_X(point.X), _Y(startY + labelType.CelGapTop), contentAreaWidthMM, contentsHight));

            if (bDrawFrame)
            {
                //ライン描画
                Pen pen = new Pen(Color.Black, (float)0.1);
                graphics.DrawRectangle(pen,_X(0), _Y(startY), labelBlockWidthMM, gridHeight);
                graphics.DrawLine(pen, new PointF(_X(titleAreWidthMM), _Y(startY)), new PointF(_X(titleAreWidthMM), _Y(startY + gridHeight)));
            }

        }

        float _X(float x) { return PageGapLeftMM + LabelBlockOfsX +  labelType.GapLeft + x; }
        float _Y(float y) { return PageGapTopMM + LabelBlockOfsY +  labelType.GapTop + y; }


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
        Font GetFontCalcedByHeight(float startFontSize, string s, float limitHeght, ref float height)
        {
            float fntSize = startFontSize;
            while (fntSize > 0)
            {
                Font font = new Font(fontName, fntSize, FontStyle.Regular);
                SizeF size = graphics.MeasureString(s, font, (int)contentAreaWidthMM);

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
        public int DrawHeader(string name, int num, float startX, float startY, float maxY, ref float nextY, ref float width)
        {
            Font font = new Font(fontName, fontSizeItemHeader, FontStyle.Regular);
            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            PointF point = new PointF(startX, startY);

            //改行コードを削除
            name = Utility.RemoveCRLF(name) + $" ({num}枚)";

            SizeF size = graphics.MeasureString(name, font);
            if (startY + size.Height > maxY)
            {
                //ヘッダ領域オーバー
                return -1;
            }
            width = size.Width;

            graphics.DrawString(name, font, brs, startX, startY);

            nextY = startY + size.Height;

            return 0;
        }

        public int DrawFooter(int curPageNo, int pageNum, float pageWidth, float pageHeight)
        {
            Font font = new Font(fontName, fontSizeItemHeader, FontStyle.Regular);
            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            string pageStr = $"{curPageNo} / {pageNum}";
            SizeF size = graphics.MeasureString(pageStr, font);


            float x = (pageWidth - size.Width)/ 2;
            float y = pageHeight - 10;
            graphics.DrawString(pageStr, font, brs, x, y);
            return 0;
        }

        public void DrawImage(string imageFilePath, float x, float y, float width, float height )
        {
            Image img = System.Drawing.Image.FromFile(imageFilePath);

            graphics.DrawImage(img, _X(x), _Y(y),width, height);

            img.Dispose();
        }

    }
}
