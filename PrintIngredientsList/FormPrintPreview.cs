using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrintIngredientsList
{
    public partial class FormPrintPreview : Form
    {
        FormMain.PrintDocumentEx pd;
        FormMain parent = null;

        int pageNum = 0;
        public FormPrintPreview(FormMain parent, FormMain.PrintDocumentEx pd)
        {
            InitializeComponent();

            this.parent = parent;
            this.pd = pd;
            printPreviewControl1.Document = pd;

            printPreviewControl1.Dock = DockStyle.Fill;
            splitContainer1.Dock = DockStyle.Fill;

            pageNum = parent.GetPageNum();
        }

        private void FormPrintPreview_Load(object sender, EventArgs e)
        {
            int x = Properties.Settings.Default.PrintPreviewDlgLocX;
            int y = Properties.Settings.Default.PrintPreviewDlgLocY;
            int w = Properties.Settings.Default.PrintPreviewDlgSizeW;
            int h = Properties.Settings.Default.PrintPreviewDlgSizeH;
            this.SetBounds(x, y, w, h);

            printPreviewControl1.Zoom = Properties.Settings.Default.PrintPreviewDlgZoom;

            txtPage.Text = "1";
            lblPageNum.Text = pageNum.ToString();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //PrintDialogクラスの作成
            System.Windows.Forms.PrintDialog pdlg = new System.Windows.Forms.PrintDialog();
            pd.ResetPageIndex();
            //PrintDocumentを指定
            pdlg.Document = pd;
            //印刷の選択ダイアログを表示する
            if (pdlg.ShowDialog() == DialogResult.OK)
            {
                //OKがクリックされた時は印刷する
                pd.Print();
            }

        }

        private void FormPrintPreview_FormClosing(object sender, FormClosingEventArgs e)
        {
            //現在位置とサイズを記録
            Properties.Settings.Default.PrintPreviewDlgLocX = Bounds.Left;
            Properties.Settings.Default.PrintPreviewDlgLocY = Bounds.Top;
            Properties.Settings.Default.PrintPreviewDlgSizeW = Bounds.Width;
            Properties.Settings.Default.PrintPreviewDlgSizeH = Bounds.Height;
            Properties.Settings.Default.PrintPreviewDlgZoom = printPreviewControl1.Zoom;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            int value = int.Parse(txtPage.Text);
            value -= 1;
            if (value <= 0) return;
            txtPage.Text = value.ToString();

            printPreviewControl1.StartPage = value-1;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            int value = int.Parse(txtPage.Text);
            value += 1;
            if (value > pageNum) return;
            txtPage.Text = value.ToString();
            printPreviewControl1.StartPage = value-1;
        }

        private void txtPage_TextChanged(object sender, EventArgs e)
        {
            int value = 0;
            if(int.TryParse(txtPage.Text, out value))
            {
                if (value <= 0 || value > pageNum) return;
                printPreviewControl1.StartPage = value - 1;

            }

        }
    }



    public class PrintPreviewControlEx : PrintPreviewControl
    {
        private Point oldPosition;//前回マウスポジ
        private Point currentPosition;//現在の位置
        private FieldInfo finfo_Position;//現在位置の取得用
        private MethodInfo minfo_SetPositionNoInvalidate;//魔法のスクロール処理

        public PrintPreviewControlEx()
        {
            //リフレクションを利用する為にTypeを取得する
            var _type = typeof(PrintPreviewControl);
            //隠蔽可された位置フィールド
            finfo_Position = _type.GetField("position", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.ExactBinding);
            //隠蔽可されたスクロールメソッド
            minfo_SetPositionNoInvalidate = _type.GetMethod("SetPositionNoInvalidate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.ExactBinding);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            //ドラッグ中だった場合
            if (e.Button == MouseButtons.Left)
            {
                //スクロール処理してみる 
                minfo_SetPositionNoInvalidate?.Invoke(this, new object[] { currentPosition + ((Size)(oldPosition - ((Size)e.Location))) });
            }
            else
            {
                //位置などを取っておく
                oldPosition = e.Location;
                currentPosition = (Point)finfo_Position?.GetValue(this);
            }

        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            //OnMouseWheelを検知する為に選択状態にする
            this.Select();
        }

        //こういった操作感が欲しい人は、もちろんマウスホイールでズーム率変えたいよね！
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                //スクロールでズーム値変更する
                //増減量・方向はお好みで
                if (e.Delta > 0)
                {
                    this.Zoom -= this.Zoom * 0.1;
                }
                else
                {
                    this.Zoom += this.Zoom * 0.1;
                }
            }
            else
            {
                currentPosition = (Point)finfo_Position?.GetValue(this);
                if (e.Delta > 0)
                {
                    currentPosition.Y -= 10;
                }else
                {
                    currentPosition.Y += 10;
                }
                //スクロール処理してみる 
                minfo_SetPositionNoInvalidate?.Invoke(this, new object[] { currentPosition });

            }

            //外部通知用にイベントを呼ぶ
            //this.ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        //外部通知用のイベントハンドラを用意しておく。使わないなら上記と共に消して構わん。
        //public event EventHandler<EventArgs>? ZoomChanged;
    }
}
