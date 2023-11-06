using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GeoView
{
    public partial class Postgischangeorder : Form
    {
        Main father_form = new Main();//表示父窗口
        public Postgischangeorder()
        {
            InitializeComponent();
        }
        public Postgischangeorder(Main temp)
        {
            InitializeComponent();
            father_form = temp;//连通父窗口       
            //string order = "SELECT ST_AsText(geom) AS geom FROM public.province";
            //DataTable dt = moMap.mPostgisConnect.Query(order);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //按下确认之后，就开始执行文本框的内容
            string sqlorder = textBox1.Text;
            if (father_form.moMap.mPostgisConnect != null)
            {
                bool judge = father_form.moMap.mPostgisConnect.changeorder(sqlorder);//进行查询的执行
                this.Close();
            }
            else
            {
                MessageBox.Show("数据库尚未连接！");
                this.Close();
            }
        }
    }
}
