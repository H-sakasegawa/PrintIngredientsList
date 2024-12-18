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
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

namespace PrintIngredientsList
{
    internal class DrawUtil2
    {

        PrintSettingData settingData;
        Graphics graphics = null;

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
        float titleAreWidthMM          = 0; //mm
        float contentAreWidthMM        = 0;  //mm

        float CellBoxGapSumHeight = 0;
        float CellBoxGapSumWidth = 0;

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


        ////mm → 座
        //float dpiX = 0;
        //float dpiY = 0;
        //private const float MillimetersPerInch = 25.4f;
        public DrawUtil2(Graphics g, PrintSettingData settingData)
        {
            this.graphics = g;
            this.settingData = settingData;
            fontSizeItemTitle = settingData.fontSizeTitle;
            this.graphics.PageUnit = GraphicsUnit.Millimeter;

            Init();
        }

        public DrawUtil2(Graphics g, PrintSettingData settingData, float PageGapTopMM, float PageGapLeftMM)
        {
            this.graphics = g;
            this.settingData = settingData;
            fontSizeItemTitle = settingData.fontSizeTitle;
            this.graphics.PageUnit = GraphicsUnit.Millimeter;
            this.PageGapTopMM = PageGapTopMM;
            this.PageGapLeftMM = PageGapLeftMM;

            Init();

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

        private void Init()
        {
            labelDrawAreaWidthMM = settingData.LabelDrawArealWidthMM;
            labelDrawAreaHeightMM = settingData.LabelDrawAreaHeightMM;

            titleAreWidthMM     = settingData.TitleAreWidthMM;

            contentAreWidthMM   = settingData.ContentAreWidthMM;

            CellBoxGapSumHeight = settingData.CellBoxGapSumHeight;
            CellBoxGapSumWidth  = settingData.CellBoxGapSumWidth;


        }

        public float DrawItemComment(string title, string content, float startY, float baseFontSize, ref bool result ,bool bDrawFrame = true)
        {

            //Font fntTitle = GetFontCalcedByWidth(baseFontSize, title, titleAreWidthMM);
            //備考欄の高さは、全エリアの高さから直前の描画開始位置を引いた残りから、セルギャップを差し引いた高さ
            float contentsLimitHightMM = labelDrawAreaHeightMM - startY - CellBoxGapSumHeight;
            float contentsHight = 0;
            float drawAreaWidth = labelDrawAreaWidthMM - CellBoxGapSumWidth;

            Font fntContents = GetFontCalcedBydHeightAndWidth(baseFontSize, content, drawAreaWidth, contentsLimitHightMM, ref contentsHight);

            float cellHeight = contentsLimitHightMM;
            cellHeight = contentsHight + (int)Math.Ceiling(settingData.CellBoxGapSumHeight);

            result = DrawItem(content, startY,  fntContents, drawAreaWidth, contentsHight, cellHeight, bDrawFrame);

            return startY + contentsHight;
        }

        public float DrawItem(string title, string content, float startY, float limitHight, float baseFontSize,bool bResizeHeight=false, bool bDrawFrame = true)
        {

            //タイトル領域は、横１列として表示可能なフォントサイズのフォントを取得
            Font fntTitle = new Font(settingData.fontName, fontSizeItemTitle, FontStyle.Regular);
            //Font fntTitle = new Font(GetFontCalcedByWidth(baseFontSize, title, titleAreWidthMM - CellBoxGapSumWidth, limitHight-CellBoxGapSumHeight );
            float contentsHight = 0;
            Font fntContents = GetFontCalcedBydHeight(baseFontSize, content, limitHight - CellBoxGapSumHeight, ref contentsHight);

            float cellHeight = limitHight;
            if (bResizeHeight)
            {
                cellHeight = contentsHight + (int)Math.Ceiling(settingData.CellBoxGapSumHeight);
            }


            DrawItem( title,  content,  startY, fntTitle, fntContents, contentsHight, cellHeight,  bDrawFrame);

            return startY + cellHeight;

        }

        private void DrawItem(string title, string content, float startY, Font fntTitle, Font fntContens, float contentsHight, float gridHeight, bool bDrawFrame)
        {
            //グリッドセルのTop/Leftの余白
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            PointF point = new PointF(_X(settingData.CellBoxGapLeft), _Y(startY + settingData.CellBoxGapTop));
            graphics.DrawString(title, fntTitle, brs, point);
            point.X = titleAreWidthMM + settingData.CellBoxGapLeft;
            graphics.DrawString(content, fntContens, brs, new RectangleF(_X(point.X), _Y(startY + settingData.CellBoxGapTop), contentAreWidthMM, contentsHight));

            if (bDrawFrame)
            {
                //ライン描画
                Pen pen = new Pen(Color.Black, (float)0.1);
                graphics.DrawRectangle(pen,_X(0), _Y(startY), labelDrawAreaWidthMM, gridHeight);
                graphics.DrawLine(pen, new PointF(_X(titleAreWidthMM), _Y(startY)), new PointF(_X(titleAreWidthMM), _Y(startY + gridHeight)));
            }

        }
        private bool DrawItem( string content, float startY,  Font fntContens, float drawAreaWidth,float contentsHight, float gridHeight, bool bDrawFrame)
        {
            bool bDrawHeightResult = true;
            //グリッドセルのTop/Leftの余白
            //graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            PointF point = new PointF(_X(settingData.CellBoxGapLeft), _Y(startY + settingData.CellBoxGapTop));
            graphics.DrawString(content, fntContens, brs, new RectangleF(point.X, point.Y, drawAreaWidth, gridHeight));

            //---------------------------------------------------------
            // 描画した文字列の高さが グリッド高さをこえていないか？
            SizeF size = graphics.MeasureString(content, fntContens, (int)drawAreaWidth );

            if( size.Height > gridHeight)
            {
                bDrawHeightResult = false;
            }

            if (bDrawFrame)
            {
                //ライン描画
                Pen pen = new Pen(Color.Black, (float)0.1);
                graphics.DrawRectangle(pen,_X(0), _Y(startY), labelDrawAreaWidthMM, gridHeight);
            }

            return bDrawHeightResult;

        }

        float _X(float x) { return PageGapLeftMM + settingData.LabelAreaGapLeft + x; }
        float _Y(float y) { return PageGapTopMM + settingData.LabelAreaGapTop + y; }


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
                Font font = new Font(settingData.fontName, fntSize, FontStyle.Regular);
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
                Font font = new Font(settingData.fontName, fntSize, FontStyle.Regular);
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
                Font font = new Font(settingData.fontName, fntSize, FontStyle.Regular);
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
            Font font = new Font(settingData.fontName, fontSizeItemHeader, FontStyle.Regular);
            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            PointF point = new PointF(startX, startY);

            //改行コードを削除
            name = name.Replace("\n", "");
            name = name.Replace("\r", "");
            name = name + $" ({num}枚)";

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
            Font font = new Font(settingData.fontName, fontSizeItemHeader, FontStyle.Regular);
            Brush brs = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            string pageStr = $"{curPageNo} / {pageNum}";
            SizeF size = graphics.MeasureString(pageStr, font);


            float x = (pageWidth - size.Width)/ 2;
            float y = pageHeight - 10;
            graphics.DrawString(pageStr, font, brs, x, y);
            return 0;
        }

    }
}
