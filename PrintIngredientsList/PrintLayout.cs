using ConfigReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using static iTextSharp.text.pdf.PRTokeniser;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using static PrintIngredientsList.LabelTypeBlockItemBase;
using static PrintIngredientsList.LabelTypeBlockBase;
using System.ComponentModel;
using NPOI.OpenXmlFormats.Wordprocessing;

namespace PrintIngredientsList
{
    public class PrintLayoutBase
    {
        public PrintLayoutBase(CConfigReaderIFWrapper readerIF = null)
        {
            if (readerIF == null)
            {
                this.readerIF = new CConfigReaderIFWrapper();
            }
            else
            {
                this.readerIF = readerIF;
            }
        }

        protected CConfigReaderIFWrapper readerIF;
    }

    public class ConfigBlockBase
    {
        public string GetBlockItemVale(CConfigBlockWrapper block, string keyName, string defaultValue = "")
        {
            this.blockITEM = block;

            var item = block.GetItem(keyName);
            if (item == null) return defaultValue;
            return item.GetValue();
        }
        protected CConfigBlockWrapper blockITEM;

    }

    public class LabelTypeBlockBase : ConfigBlockBase
    {

        public enum LabelTypeBlockType
        {
            NONE,
            GRID,
            PICTURE,
        }

        public LabelTypeBlockBase()
        {

        }

        public LabelTypeBlockBase(CConfigBlockWrapper block)
        {

            var value = GetBlockItemVale(block, "TYPE", "");
            Enum.TryParse(value, true, out labelTypeBlockType);
        }


        public LabelTypeBlockType labelTypeBlockType = LabelTypeBlockType.NONE;
    }

    public class LabelTypeBlockItemBase : ConfigBlockBase
    {
        public enum LabelTypeBlockItemType
        {
            LABEL,
            BARCODE,
            ICON
        }
        public LabelTypeBlockItemBase() { }
        public LabelTypeBlockItemBase(LabelTypeBlock parent,CConfigBlockWrapper blockITEM)
        {
            this.parent = parent;
            this.blockITEM = blockITEM;
            name = blockITEM.GetValue();

            if (!string.IsNullOrEmpty(name))
            {
                Enum.TryParse(name, true, out itemType);
            }

            var item = blockITEM.GetItem("VISIBLE");
            if (item != null)
            {
                int value = 0;
                int.TryParse(item.GetValue(), out value);
                visible = value == 0 ? false : true;
            }
            item = blockITEM.GetItem("TYPE");
            if (item != null)
            {
                if (item.GetValue() == "CUSTOMIZE")
                {
                    bCustomize = true;
                }
            }

        }


        public bool bCustomize = false;
        public string name;

        protected bool visible = true;
        public bool Visible
        {
            get { return visible; }
            set
            {
                visible = value;
                blockITEM.SetValue("VISIBLE", visible ? "1" : "0");
            }
        }

        public LabelTypeBlock parent = null;
        public LabelTypeBlockItemType itemType = LabelTypeBlockItemType.LABEL;

    }


    public class PrintLayoutManager : PrintLayoutBase
    {
        public PrintLayoutManager()
        {

            printLayout = new PrintLayout(readerIF);
            labelLayout = new LabelLayout(readerIF);

        }

        public int ReadLayout(string filePath)
        {
            try
            {
                readerIF.ReadConfigFile(filePath);

                labelLayout.ReadLayout();
                printLayout.ReadLayout();
            }
            catch(Exception ex)
            {
                Utility.MessageError(ex.ToString());
            }
            return 0;

        }

        public int SaveLayout(string filePath)
        {
            return readerIF.WriteConfigFile(filePath);

        }


        public PrintLayout printLayout;
        public LabelLayout labelLayout;


    }
    //=======================================================

    public class PrintLayout : PrintLayoutBase
    {
        public PrintLayout(CConfigReaderIFWrapper readerIF) : base(readerIF)
        {
        }
        public int ReadLayout()
        {
            try
            {
                var blkPRINT_LAYOUT = readerIF.GetBlock("PRINT_LAYOUT");
                if (blkPRINT_LAYOUT == null) return -1;

                lstLayout.Clear();
                int num = blkPRINT_LAYOUT.GetBlockCount("LAYOUT");
                for (int i = 0; i < num; i++)
                {
                    var blkLAYOUT = blkPRINT_LAYOUT.GetBlock(i);
                    Layout layout = new Layout(blkLAYOUT);

                    lstLayout.Add(layout);
                }
            }catch(Exception ex)
            {
                Utility.MessageError(ex.ToString());
                return -1;
            }

            return 0;

        }

        public Layout this[int index]
        {
            get
            {
                if (index >= lstLayout.Count) return null;
                return lstLayout[index];
            }
        }

        public int GetLayoutCnt() { return lstLayout.Count; }
        public List<Layout> lstLayout = new List<Layout>();

    }

    public class Layout: ConfigBlockBase
    {
        public Layout(CConfigBlockWrapper blockLayout)
        {

            layoutName = blockLayout.GetValue(); //"A4縦"
            paperType = GetBlockItemVale(blockLayout, "PAPER_TYPE", "A4");
            paperDirc = GetBlockItemVale(blockLayout, "PAPER_DIRC", "縦");
            paperWidth = Utility.ToFloat(GetBlockItemVale(blockLayout,"PAPER_WIDTH", "0") );
            paperHeight = Utility.ToFloat(GetBlockItemVale(blockLayout, "PAPER_HEIGHT", "0"));
            printGapTop = Utility.ToFloat(GetBlockItemVale(blockLayout, "PRINT_GAP_TOP", "18.5"));
            printGapLeft = Utility.ToFloat(GetBlockItemVale(blockLayout, "PRINT_GAP_LEFT", "19.0"));
            headerGapTop = Utility.ToFloat(GetBlockItemVale(blockLayout, "HEADER_GAP_TOP", "3"));
            headerGapLeft = Utility.ToFloat(GetBlockItemVale(blockLayout, "HEADER_GAP_LEFT", "3"));

            if (paperType == "A4")
            {
                paperKind = System.Drawing.Printing.PaperKind.A4;
                if (paperDirc == "横")
                {
                    landscape = true;
                }
            }
            //else if (layoutName.IndexOf("A3") > 0)
            //{
            //    if (layoutName.IndexOf("縦") > 0)
            //    {
            //        paperKind = System.Drawing.Printing.PaperKind.A3;
            //    }else
            //    {
            //        paperKind = System.Drawing.Printing.PaperKind.A3Rotated;
            //    }
            //}
            //else if (layoutName.IndexOf("B4") > 0)
            //{
            //    if (layoutName.IndexOf("縦") > 0)
            //    {
            //        paperKind = System.Drawing.Printing.PaperKind.B4;
            //    }else
            //    {
            //        paperKind = System.Drawing.Printing.PaperKind.B4JisRotated;
            //    }
            //}

        }
        public override string ToString()
        {
            return layoutName;
        }

        public float paperWidth;
        public float paperHeight;
        public string paperType;
        public string paperDirc;

        protected float printGapTop;
        public float PrintGapTop
        {
            get { return printGapTop; }
            set {
                printGapTop = value;
                blockITEM.SetValue("PRINT_GAP_TOP", printGapTop.ToString("F1"));
            }
        }

        protected float printGapLeft;
        public float PrintGapLeft
        {
            get { return printGapLeft; }
            set
            {
                printGapLeft = value;
                blockITEM.SetValue("PRINT_GAP_LEFT", printGapLeft.ToString("F1"));
            }
        }

        protected float headerGapTop;
        public float HeaderGapTop
        {
            get { return headerGapTop; }
            set
            {
                headerGapTop = value;
                blockITEM.SetValue("HEADER_GAP_TOP", headerGapTop.ToString("F1"));
            }
        }
        protected float headerGapLeft;
        public float HeaderGapLeft
        {
            get { return headerGapLeft; }
            set
            {
                headerGapLeft = value;
                blockITEM.SetValue("HEADER_GAP_LEFT", headerGapLeft.ToString("F1"));
            }
        }



        public string layoutName;
        public System.Drawing.Printing.PaperKind paperKind;
        public bool landscape = false;

    }

    //=======================================================
    public class LabelLayout : PrintLayoutBase
    {
        public LabelLayout(CConfigReaderIFWrapper readerIF) : base(readerIF)
        {
        }
        public int ReadLayout()
        {
            var blkLABEL = readerIF.GetBlock("LABEL");
            if (blkLABEL == null) return -1;

            lstLabelTypes.Clear();
            int num = blkLABEL.GetBlockCount("TYPE");
            for(int i=0; i<num; i++)
            {
                var blkTYPE = blkLABEL.GetBlock(i);

                LabelType labelType = new LabelType(blkTYPE);
                lstLabelTypes.Add(labelType);
            }


            return 0;
        }
        public int GetLayoutCnt() { return lstLabelTypes.Count; }
        public LabelType this[int index]
        {
            get
            {
                if (index >= lstLabelTypes.Count) return null;
                return lstLabelTypes[index];
            }
        }


        public List<LabelType> lstLabelTypes = new List<LabelType>();

    }

    public class LabelType : ConfigBlockBase
    {
        public LabelType(CConfigBlockWrapper blockTYPE)
        {
            Name = GetBlockItemVale(blockTYPE, "TITLE", "UnKnown");
            width = Utility.ToFloat(GetBlockItemVale(blockTYPE, "WIDTH", "43"));
            height = Utility.ToFloat(GetBlockItemVale(blockTYPE, "HEIGHT", "65"));
            gapTop = Utility.ToFloat(GetBlockItemVale(blockTYPE, "GAP_TOP", "2"));
            gapLeft = Utility.ToFloat(GetBlockItemVale(blockTYPE, "GAP_LEFT", "2"));
            gapRight = Utility.ToFloat(GetBlockItemVale(blockTYPE, "GAP_RIGHT", "2"));
            gapBottom = Utility.ToFloat(GetBlockItemVale(blockTYPE, "GAP_BOTTOM", "2"));
            celGapTop = Utility.ToFloat(GetBlockItemVale(blockTYPE, "CEL_GAP_TOP", "0.2"));
            celGapLeft = Utility.ToFloat(GetBlockItemVale(blockTYPE, "CEL_GAP_LEFT", "0.2"));
            celGapRight = Utility.ToFloat(GetBlockItemVale(blockTYPE, "CEL_GAP_RIGHT", "0.2"));
            celGapBottom = Utility.ToFloat(GetBlockItemVale(blockTYPE, "CEL_GAP_BOTTOM", "0.2"));

            lstLabelBlocks.Clear();
            int num = blockTYPE.GetBlockCount("BLOCK");
            for(int i=0; i<num; i++)
            {
                var blockBLOCK = blockTYPE.GetBlock(i);
                LabelTypeBlock labelBlock = new LabelTypeBlock(blockBLOCK, this);


                lstLabelBlocks.Add(labelBlock);
            }

        }
        public override string ToString()
        {
            return Name;
        }

        public LabelTypeBlock GetLabelBlock(string name)
        {
            return lstLabelBlocks.Find(x => x.name == name);

        }

        /// <summary>
        /// アイコン描画ブロックがるかを判定
        /// </summary>
        /// <returns></returns>
        public bool IsExistImageBlock()
        {
            for (int iBlock = 0; iBlock < lstLabelBlocks.Count; iBlock++)
            {
                LabelTypeBlock labelBlock = lstLabelBlocks[iBlock];
                for (int iItem = 0; iItem < labelBlock.lstLabelTypeBlocklItems.Count; iItem++)
                {
                    var labelItemBase = labelBlock.lstLabelTypeBlocklItems[iItem];
                    if (labelItemBase.itemType == LabelTypeBlockItemType.BARCODE ||
                       labelItemBase.itemType == LabelTypeBlockItemType.ICON)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// BARCODEアイテムブロックの有無チェック
        /// </summary>
        /// <returns></returns>
        public bool IsExistBarcodeBlock()
        {
            var item = GetBarcodeItem();
            if (item != null) return true;

            return false;
        }

        public LabelTypeBlockItemBase GetBarcodeItem()
        {
            for (int iBlock = 0; iBlock < lstLabelBlocks.Count; iBlock++)
            {
                LabelTypeBlock labelBlock = lstLabelBlocks[iBlock];
                for (int iItem = 0; iItem < labelBlock.lstLabelTypeBlocklItems.Count; iItem++)
                {
                    var labelItemBase = labelBlock.lstLabelTypeBlocklItems[iItem];
                    if (labelItemBase.itemType == LabelTypeBlockItemType.BARCODE)
                    {
                        return labelItemBase;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// １つのラベルシールの描画可能領域幅(mm) 左右の余白を差し引いた値
        /// </summary>
        public float LabelDrawArealWidthMM
        {
            get
            {
                return width - LabelGapSumWidth; //mm
            }
        }
        /// <summary>
        /// １つのラベルシールの描画可能領域高さ(mm) 上下の余白を差し引いた値
        /// </summary>
        public float LabelDrawAreaHeightMM
        {
            get
            {
                return height - LabelGapSumHeight; //mm
            }
        }
        /// <summary>
        /// ラベルの縦余白合計
        /// </summary>
        public float LabelGapSumHeight
        {
            get
            {
                return gapTop + gapBottom;
            }
        }
        /// <summary>
        /// ラベル内の横余白合計
        /// </summary>
        public float LabelGapSumWidth
        {
            get
            {
                return gapLeft + gapRight;
            }
        }
        /// <summary>
        /// セルボックス内の縦余白合計
        /// </summary>
        public float CellBoxGapSumHeight
        {
            get
            {
                return celGapTop + celGapBottom;
            }
        }
        /// <summary>
        /// セルボックス内の横余白合計
        /// </summary>
        public float CellBoxGapSumWidth
        {
            get
            {
                return celGapLeft + celGapRight;
            }
        }


        public string Name;

        protected float width;
        public float Width
        {
            get { return width; }
            set
            {
                width = value;
                blockITEM.SetValue("WIDTH", width.ToString("F1"));
            }
        }

        protected float height;
        public float Height
        {
            get { return height; }
            set
            {
                height = value;
                blockITEM.SetValue("HEIGHT", height.ToString("F1"));
            }
        }
        ////ラベル領域内のギャップ
        protected float gapTop;
        public float GapTop
        {
            get { return gapTop; }
            set
            {
                gapTop = value;
                blockITEM.SetValue("GAP_TOP", gapTop.ToString("F1"));
            }
        }
        protected float gapLeft;
        public float GapLeft
        {
            get { return gapLeft; }
            set
            {
                gapLeft = value;
                blockITEM.SetValue("GAP_LEFT", gapLeft.ToString("F1"));
            }
        }
        protected float gapRight;
        public float GapRight
        {
            get { return gapRight; }
            set
            {
                gapRight = value;
                blockITEM.SetValue("GAP_RIGHT", gapRight.ToString("F1"));
            }
        }
        protected float gapBottom;
        public float GapBottom
        {
            get { return gapBottom; }
            set
            {
                gapBottom = value;
                blockITEM.SetValue("GAP_BOTTOM", gapBottom.ToString("F1"));
            }
        }
        //ラベル領域内の各セル（矩形）内の文字描画範囲余白サイズ
        protected float celGapTop;
        public float CelGapTop
        {
            get { return celGapTop; }
            set
            {
                celGapTop = value;
                blockITEM.SetValue("CEL_GAP_TOP", celGapTop.ToString("F1"));
            }
        }
        protected float celGapLeft;
        public float CelGapLeft
        {
            get { return celGapLeft; }
            set
            {
                celGapLeft = value;
                blockITEM.SetValue("CEL_GAP_LEFT", celGapLeft.ToString("F1"));
            }
        }
        protected float celGapRight;
        public float CelGapRight
        {
            get { return celGapRight; }
            set
            {
                celGapRight = value;
                blockITEM.SetValue("CEL_GAP_RIGHT", celGapRight.ToString("F1"));
            }
        }
        protected float celGapBottom;
        public float CelGapBottom
        {
            get { return celGapBottom; }
            set
            {
                celGapBottom = value;
                blockITEM.SetValue("CEL_GAP_BOTTOM", celGapBottom.ToString("F1"));
            }
        }



        public List<LabelTypeBlock> lstLabelBlocks = new List<LabelTypeBlock>();

    }

    public class LabelTypeBlock : LabelTypeBlockBase
    {
        public LabelTypeBlock() { }
        public LabelTypeBlock(CConfigBlockWrapper blockBLOCK, LabelType labelType):base(blockBLOCK)
        {
            this.labelType = labelType;

            name = GetBlockItemVale(blockBLOCK, "TITLE", "UnKnown");
            string pos = GetBlockItemVale(blockBLOCK, "POSITION", "0,0");
            var items = pos.Split(',');
            posX = Utility.ToFloat(items[0]);
            posY = Utility.ToFloat(items[1]);

            titleWidth = Utility.ToFloat(GetBlockItemVale(blockBLOCK, "TITLE_WIDTH", "10"));
            valueWidth = Utility.ToFloat(GetBlockItemVale(blockBLOCK,"VALUE_WIDTH", "0"));
            titleFontSize = Utility.ToFloat(GetBlockItemVale(blockBLOCK,"TITLE_FONT_SIZE", "6"));

            lstLabelTypeBlocklItems.Clear();
            int num = blockBLOCK.GetBlockCount();
            for(int i=0; i<num; i++)
            {
                var blockItem = blockBLOCK.GetBlock(i);

                LabelTypeBlockItemBase item = null;
                switch (labelTypeBlockType)
                {
                    case LabelTypeBlockType.GRID:
                        {
                            var itemType = blockItem.GetItem("TYPE");
                            if (itemType != null && itemType.GetValue() == "CUSTOMIZE")
                            {
                                item = new LabelItemCustomize(this, blockItem);
                            }
                            else
                            {
                                item = new LabelItem(this, blockItem);
                            }
                        }
                        break;
                    case LabelTypeBlockType.PICTURE:
                        item = new PictureItem(this, blockItem);
                        break;
                }

                lstLabelTypeBlocklItems.Add(item);
            }

        }

        public override string ToString()
        {
            return name;
        }

        public float LabelBlockWidth
        {
            get
            {
                return titleWidth + valueWidth;
            }
        }


        public LabelItem GetLabelItem(string name)
        {
            return (LabelItem)lstLabelTypeBlocklItems.Find(x => x.name == name);

        }

        //public float GetFontSize(string name)
        //{
        //    var item = GetLabelItem(name);
        //    if (item != null)
        //    {
        //        return item.FontSize;
        //    }
        //    return Const.defaultFontSize;

        //}
        //public void SetFontSize(string name, float value)
        //{
        //    var item = GetLabelItem(name);
        //    if (item != null)
        //    {
        //        item.FontSize = value;
        //    }
        //}

        //public float GeItemHeight(string name)
        //{
        //    var item = GetLabelItem(name);
        //    if (item != null)
        //    {
        //        return item.Height;
        //    }
        //    return Const.defaultItemHeight;

        //}
        //public void SeItemHeight(string name, float value)
        //{
        //    var item = GetLabelItem(name);
        //    if (item != null)
        //    {
        //        item.Height = value;
        //    }

        //}


        /// <summary>
        /// 所属する親のラベルタイプオブジェクト
        /// </summary>
        private LabelType labelType;

        public string name { get; set; }

        protected float posX = 0;
        public float PosX
        {
            get { return posX; }
            set
            {
                posX = value;
                blockITEM.SetValue("POSITION", $"{posX:F1},{posY:F1}");
            }
        }
        protected float posY = 0;
        public float PosY
        {
            get { return posY; }
            set
            {
                posY = value;
                blockITEM.SetValue("POSITION", $"{posX:F1},{posY:F1}");
            }
        }

        protected float titleWidth = 10;
        public float TitleWidth
        {
            get { return titleWidth; }
            set
            {
                titleWidth = value;
                blockITEM.SetValue("TITLE_WIDTH", titleWidth.ToString("F1"));
            }

        }
        protected float valueWidth;
        public float ValueWidth
        {
            get{ return valueWidth;}
            set
            {
                valueWidth = value;
                blockITEM.SetValue("VALUE_WIDTH", valueWidth.ToString("F1"));
            }
        }
        protected float titleFontSize = 6;
        public float TitleFontSize
        {
            get { return titleFontSize; }
            set
            {
                titleFontSize = value;
                blockITEM.SetValue("TITLE_FONT_SIZE", titleFontSize.ToString("F1"));
            }
        }

        public List<LabelTypeBlockItemBase> lstLabelTypeBlocklItems { get; set; } = new List<LabelTypeBlockItemBase>();
        
    }

    //====================================================
    // ITEM
    //====================================================
    /// <summary>
    /// BLOCK(成分表),
    /// BLOCK(栄養成分表) 内のITEM
    /// </summary>
    public class LabelItem : LabelTypeBlockItemBase
    {
        public LabelItem() { }

        public LabelItem(LabelTypeBlock parent, CConfigBlockWrapper blockITEM):base(parent,blockITEM)
        {
            this.blockITEM = blockITEM;

            var item = blockITEM.GetItem("TITLE");
            if (item != null)
            {
                Title = item.GetValue();
            }

            item = blockITEM.GetItem("HIGHT");
            if (item != null)
            {
                height = Utility.ToFloat(item.GetValue());
            }
            item = blockITEM.GetItem("VALUE_WIDTH");
            if (item != null)
            {
                valueWidth = Utility.ToFloat(item.GetValue());
            }
            item = blockITEM.GetItem("FONT_SIZE");
            if (item != null)
            {
                fontSize = Utility.ToFloat(item.GetValue());
            }
            item = blockITEM.GetItem("FRAME");
            if (item != null)
            {
                int value = 0;
                int.TryParse(item.GetValue(), out value);
                DrawFrame = value == 0 ? false : true;
            }



        }

        //------------------------------------
        // プロパティ
        //------------------------------------

        public string Title { get; set; }

        protected float height= -1;
        public float Height
        {
            get { return height; }
            set
            {
                height = value;
                blockITEM.SetValue("HIGHT", height.ToString("F1"));
            }
        }
        protected float valueWidth = -1;
        public float ValueWidth
        {
            get { return valueWidth; }
            set
            {
                valueWidth = value;
                blockITEM.SetValue("VALUE_WIDTH", valueWidth.ToString("F1"));
            }
        }
        protected float fontSize;
        public float FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                blockITEM.SetValue("FONT_SIZE", fontSize.ToString("F1"));
            }
        }
        public bool drawFrame = true;
        public bool DrawFrame
        {
            get { return drawFrame; }
            set
            {
                drawFrame = value;
                blockITEM.SetValue("FRAME", drawFrame ? "1" : "0");
            }
        }


    }

    public class LabelItemCustomize : LabelItem
    {
        public LabelItemCustomize() { }

        public LabelItemCustomize(LabelTypeBlock parent, CConfigBlockWrapper blockITEM) : base(parent, blockITEM)
        {


            var item = blockITEM.GetItem("TITLE_POSITION");
            if (item != null)
            {
                string pos = item.GetValue();
                var items = pos.Split(',');
                titlePosX = Utility.ToFloat(items[0]);
                titlePosY = Utility.ToFloat(items[1]);
            }
            item = blockITEM.GetItem("VALUE_POSITION");
            if (item != null)
            {
                string pos = item.GetValue();
                var items = pos.Split(',');
                valuePosX = Utility.ToFloat(items[0]);
                valuePosY = Utility.ToFloat(items[1]);
            }

            item = blockITEM.GetItem("TITLE_HIGHT");
            if (item != null)
            {
                titleHeight = Utility.ToFloat(item.GetValue());
            }
            item = blockITEM.GetItem("TITLE_WIDTH");
            if (item != null)
            {
                titleWidth = Utility.ToFloat(item.GetValue());
            }
            item = blockITEM.GetItem("VALUE_HIGHT");
            if (item != null)
            {
                valueHeight = Utility.ToFloat(item.GetValue());
            }
            item = blockITEM.GetItem("VALUE_WIDTH");
            if (item != null)
            {
                valueWidth = Utility.ToFloat(item.GetValue());
            }

        }

        //------------------------------------
        // プロパティ
        //------------------------------------

        //拡張プロパティ
        protected float titlePosX;
        public float TitlePosX
        {
            get { return titlePosX; }
            set
            {
                titlePosX = value;
                blockITEM.SetValue("TITLE_POSITION", $"{titlePosX:F1},{titlePosY:F1}");
            }
        }
        protected float titlePosY;
        public float TitlePosY
        {
            get { return titlePosY; }
            set
            {
                titlePosY = value;
                blockITEM.SetValue("TITLE_POSITION", $"{titlePosX:F1},{titlePosY:F1}");
            }
        }
        protected float valuePosX;
        public float ValuePosX
        {
            get { return valuePosX; }
            set
            {
                valuePosX = value;
                blockITEM.SetValue("TITLE_POSITION", $"{valuePosX:F1},{valuePosY:F1}");
            }
        }
        protected float valuePosY;
        public float ValuePosY
        {
            get { return valuePosY; }
            set
            {
                valuePosY = value;
                blockITEM.SetValue("TITLE_POSITION", $"{valuePosX:F1},{valuePosY:F1}");
            }
        }
        protected float titleHeight;
        public float TitleHeight
        {
            get { return titleHeight; }
            set
            {
                titleHeight = value;
                blockITEM.SetValue("TITLE_HIGHT", titleHeight.ToString("F1"));
            }
        }
        protected float valueHeight;
        public float ValueHeight
        {
            get { return valueHeight; }
            set
            {
                valueHeight = value;
                blockITEM.SetValue("VALUE_HIGHT", valueHeight.ToString("F1"));
            }
        }
        protected float titleWidth;
        public float TitleWidth
        {
            get { return titleWidth; }
            set
            {
                titleWidth = value;
                blockITEM.SetValue("TITLE_WIDTH", titleWidth.ToString("F1"));
            }
        }
        //派生もとにあるので一旦コメント
        //protected float valueWidth;
        //public float ValueWidth
        //{
        //    get { return valueWidth; }
        //    set
        //    {
        //        valueWidth = value;
        //        blockITEM.SetValue("VALUE_WIDTH", valueWidth.ToString("F1"));
        //    }
        //}


    }

    /// <summary>
    /// BLOCK(画像)内のITEM
    /// </summary>
    public class PictureItem : LabelTypeBlockItemBase
    {
        public PictureItem() { }
        public PictureItem(LabelTypeBlock parent, CConfigBlockWrapper blockITEM) : base(parent, blockITEM)
        {


            string pos = GetBlockItemVale(blockITEM, "POSITION", "0,0");
            var items = pos.Split(',');
            PosX = Utility.ToFloat(items[0]);
            PosY = Utility.ToFloat(items[1]);

            Image = GetBlockItemVale(blockITEM, "SOURCE", "");
            Width = Utility.ToFloat(GetBlockItemVale(blockITEM, "WIDTH", "0"));
            Height = Utility.ToFloat(GetBlockItemVale(blockITEM, "HEIGHT", "0"));
            DispNo = Utility.ToBoolean(GetBlockItemVale(blockITEM, "DISP_NO", "1"));
        }
        protected float posX = 0;
        public float PosX
        {
            get { return posX; }
            set
            {
                posX = value;
                blockITEM.SetValue("POSITION", $"{posX:F1},{posY:F1}");
            }
        }
        public float posY = 0;
        public float PosY
        {
            get { return posY; }
            set
            {
                posY = value;
                blockITEM.SetValue("POSITION", $"{posX:F1},{posY:F1}");
            }
        }
        public float width = 0;
        public float Width
        {
            get { return width; }
            set
            {
                width = value;
                blockITEM.SetValue("WIDTH", width.ToString("F1"));
            }
        }
        public float height = 0;
        public float Height
        {
            get { return height; }
            set
            {
                height = value;
                blockITEM.SetValue("HEIGHT", height.ToString("F1"));
            }
        }

        public bool dispNo = true;
        public bool DispNo
        {
            get { return dispNo; }
            set
            {
                dispNo = value;
                blockITEM.SetValue("DISP_NO", value ? "1" : "0");
            }
        }

        public string Image { get; set; }
    }

}
