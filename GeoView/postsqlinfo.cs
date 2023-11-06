using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeoView
{
    public partial class postsqlinfo : Form
    {
        #region 字段

        Main father_form = new Main();//表示父窗口
        private DataTable dataTable;//数据表

        #endregion


        #region 构造函数
        public postsqlinfo()
        {
            InitializeComponent();
        }

        public postsqlinfo(Main temp)
        {
            InitializeComponent();
            father_form = temp;//连通父窗口       
            //string order = "SELECT ST_AsText(geom) AS geom FROM public.province";
            //DataTable dt = moMap.mPostgisConnect.Query(order);
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {        
            //按下确认之后，将几个文本框的内容输出为一个字符串
            string Port = textBox1.Text;
            string Userid = textBox4.Text;
            string password = textBox2.Text;
            string database = textBox3.Text;
            string constr = "Host=localhost;Port=".ToString() + Port +
                ";Username=".ToString() + Userid + 
                ";Password=".ToString() + password + 
                ";Database=".ToString() + database;
            //"Host=localhost;Port=5432;Username=postgres;Password=020228chenxi;Database=postgres";
            if(Port.Length==0)
                constr= "Host=localhost;Port=5432;Username=postgres;Password=020228chenxi;Database=postgres";
            father_form.moMap.mPostgisConnect = new MyMapObjects.PostGISConnect(constr);//实现这个类
            this.Close();
        }
    }
}
