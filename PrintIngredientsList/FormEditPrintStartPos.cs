using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrintIngredientsList
{
    public partial class FormEditPrintStartPos : Form
    {

        List<Button> lstPictureBoxes = new List<Button>();

        public int printStartPos = 1;

        private PrintSettingData settingData;
        private LabelType labelType;

        public FormEditPrintStartPos(LabelType labelType, PrintSettingData settingData)
        {
            InitializeComponent();

            this.settingData = settingData;
            this.labelType = labelType;
        }

        private void FormEditPrintStartPos_Load(object sender, EventArgs e)
        {

            int colNum = 4;
            int rowNum = 4;

            int btnWidth  = (int)(Utility.MILLI2POINT(labelType.Width) * 0.4);
            int btnHeight = (int)(Utility.MILLI2POINT(labelType.Height) * 0.4);

            int gapTop = 10;
            int gapLeft =10;

            int DiaogWidth = gapLeft * 2 + colNum * btnWidth + 16;
            int DiaogHight = gapTop * 2 + rowNum * btnHeight + button1.Height+ SystemInformation.CaptionHeight+ 25 ;

            this.Size = new Size(DiaogWidth, DiaogHight);

            int cnt = 0;
            Color setColor = Color.Gray;
            int y = gapTop;
            for(int iRow=0; iRow<rowNum; iRow++)
            {
                int x = gapLeft;
                for (int iCol = 0; iCol < colNum; iCol++)
                {
                    Button btn = new Button();
                    btn.Width = btnWidth;
                    btn.Height = btnHeight;

                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = Color.YellowGreen;
                    btn.Location = new Point(x, y);
                    btn.Parent = this;
                    btn.Text = (cnt + 1).ToString();
                    btn.Click += OnPictureBoxClick;
                    lstPictureBoxes.Add(btn);
                    x += btnWidth;

                    cnt++;

                }
                y += btnHeight;
            }

            SetButtonColor(printStartPos);

        }


        void OnPictureBoxClick(object sender, EventArgs e)
        {
            Button clickBtn = (Button)sender;

            int index = lstPictureBoxes.FindIndex(x => x == clickBtn);
            printStartPos = index + 1;
            SetButtonColor(printStartPos);

        }

        void SetButtonColor(int printStartPos)
        {
            int cnt = 1;
            Color setColor = Color.Gray;
            foreach (var pic in lstPictureBoxes)
            {
                if (cnt == printStartPos)
                {
                    setColor = Color.YellowGreen;
                }
                pic.BackColor = setColor;
                cnt++;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
