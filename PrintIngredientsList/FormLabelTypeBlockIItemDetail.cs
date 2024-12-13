using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace PrintIngredientsList
{
    public partial class FormLabelTypeBlockIItemDetail : Form
    {
        LabelTypeBlockItemBase labelTypeBlockItem;

        public FormLabelTypeBlockIItemDetail(LabelTypeBlockItemBase labelTypeBlockItem)
        {
            InitializeComponent();

            this.labelTypeBlockItem = labelTypeBlockItem;
        }

        private void FormDetail_Load(object sender, EventArgs e)
        {

            bool bImageEnable = false;
            bool bDispNoEnable = false;
            Type targetType = typeof(LabelItem);

            switch(labelTypeBlockItem.itemType)
            {
                case LabelTypeBlockItemBase.LabelTypeBlockItemType.LABEL:
                    targetType = new LabelItem().GetType();
                    break;
                case LabelTypeBlockItemBase.LabelTypeBlockItemType.BARCODE:
                    targetType = new PictureItem().GetType();
                    bDispNoEnable = true; //DISP_NO項目表示
                    break;
                case LabelTypeBlockItemBase.LabelTypeBlockItemType.ICON:
                    targetType = new PictureItem().GetType();
                    bImageEnable = true; //IMAGE項目表示
                    break;

            }
            var properties = targetType.GetProperties();
            gridItem.CellValueChanged -= gridItem_CellValueChanged;
            {

                foreach (var p in properties)
                {
                    var index = gridItem.Rows.Add(p.Name);
                    var row = gridItem.Rows[index];

                    row.Tag = p;

                    if (p.PropertyType == typeof(bool))
                    {
                        DataGridViewCheckBoxCell checkCell = new DataGridViewCheckBoxCell();
                        row.Cells[1] = checkCell;
                    }
                    if (string.Compare(p.Name, "DispNo", true) == 0 && !bDispNoEnable)
                    {
                        row.Cells[1].ReadOnly = true;
                        row.Cells[1].Style.BackColor = Color.LightGray;
                    }

                    if ( string.Compare(p.Name,"Image", true)==0 && !bImageEnable)
                    {
                        row.Cells[1].ReadOnly = true;
                        row.Cells[1].Style.BackColor = Color.LightGray;
                    }
                    row.Cells[1].Value = p.GetValue(labelTypeBlockItem);
                }
            }
            gridItem.CellValueChanged += gridItem_CellValueChanged;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }


        bool IsLabelTypeBlockItem()
        {
            return labelTypeBlockItem.itemType == LabelTypeBlockItemBase.LabelTypeBlockItemType.LABEL ? true : false;
        }

        private void gridItem_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = gridItem.Rows[e.RowIndex];

            var value = row.Cells[e.ColumnIndex].Value;

            PropertyInfo info = (PropertyInfo)row.Tag;

            Type targetType = typeof(LabelItem);
            var properties = targetType.GetProperties();

            var t = value.GetType();

            switch (info.PropertyType.Name)
            {
                case "Single":
                    info.SetValue(labelTypeBlockItem, float.Parse((string)value));
                    break;
                case "Boolean":
                    info.SetValue(labelTypeBlockItem, value);
                    break;
                case "String":
                    info.SetValue(labelTypeBlockItem,(string)value);
                    break;

            }
        }

        private void gridItem_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (gridItem.CurrentCellAddress.X == 1 &&
                   gridItem.IsCurrentCellDirty)
            {
                //コミットする
                gridItem.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }

        }
    }
}
