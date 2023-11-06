using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;  // 导入数据表模块，以引入DataTable类。
using System.IO;
using Npgsql;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Windows.Forms;

namespace MyMapObjects
{
    public class PostGISConnect
    {
        #region 字段
        private static string connString ;
        private NpgsqlConnection connection;//数据库连接
        private NpgsqlCommand command;//数据库command字段
        public MyMapObjects.moMapLayer templayer;//用于存储查询到的图层
        private List<MyMapObjects.moGeometry> sGeometries= new List<MyMapObjects.moGeometry>();//存储几何图形
        private List<MyMapObjects.moAttributes> sAttributes=new List<MyMapObjects.moAttributes>();//存储属性集合
        private MyMapObjects.moFields sFields=new MyMapObjects.moFields();//存储字段名字
        private MyMapObjects.moFeatures sFeatures = new MyMapObjects.moFeatures();//存储要素集合
        private MyMapObjects.moGeometryTypeConstant sGeometryType;
        public string layer_name;


        #endregion

        #region 构造函数
        public PostGISConnect() 
        {
            connString = "Host=localhost;Port=5432;Username=postgres;Password=020228chenxi;Database=postgres";
        }
        public PostGISConnect(string constr)
        {
            connString = constr;
            connection = new NpgsqlConnection(connString);
            connection.Open();  // 打开连接
            if (connection.State == System.Data.ConnectionState.Open)
            {
                MessageBox.Show("连接成功！");
            }
            else
            {
                MessageBox.Show("连接失败！");
            }
            sGeometryType= MyMapObjects.moGeometryTypeConstant.MultiPolygon;//目前只做了这一种类型的读取
        }

        #endregion

        #region 公有方法
        public MyMapObjects.moMapLayer Query(string sqrStr)
        {
            DataTable dt = new DataTable();  // 初始化一个DataTable。
            NpgsqlCommand command = new NpgsqlCommand(sqrStr, connection); // 建立一个command，按SQL语句读取表中内容'
            NpgsqlDataReader reader = command.ExecuteReader();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                MessageBox.Show("数据库尚未连接！");
                return null;// 待实现：读取的数据变成layer类。
            }
            else 
            {
                // 创建一个 WKTReader 对象，用于将文本数据解析为几何对象
                while (reader.Read())
                {

                    // 获取字段的数量
                    int fieldCount = reader.FieldCount;
                    //
                    MyMapObjects.moAttributes tempAttr = new MyMapObjects.moAttributes();
                    for (int i = 0; i < fieldCount; i++)
                    {
                        string fieldName = reader.GetName(i);
                        object fieldValue = reader[i];
                        if (fieldName == "geom".ToString())//如果刚好到了记录几何图形的位置
                        {
                            //一条就是一个多边形
                            string geomText = fieldValue.ToString();// 现在 geomText 包含转换为文本的几何数据
                            MyMapObjects.moPoints temp = DealGeotext(geomText);//生成一组点集
                            MyMapObjects.moParts tempparts = new MyMapObjects.moParts();                                                   
                            tempparts.Add(temp);//将多点添加到parts
                            MyMapObjects.moMultiPolygon temppolygon = new MyMapObjects.moMultiPolygon(tempparts);//创建一个多边形
                            temppolygon.UpdateExtent();
                            sGeometries.Add(temppolygon);//将这个多边形添加到几何图形集合里面
                        }
                        else//如果不是，就是记录属性值的
                        {
                            if(sFields.Count< fieldCount-1)
                            {
                                //记录字段
                                MyMapObjects.moField tempfield = new MyMapObjects.moField(fieldName,MyMapObjects.moValueTypeConstant.dText);
                                sFields.Append(tempfield);
                            }
                            tempAttr.Append(fieldValue);//添加一个属性进去
                        }
                        //Console.WriteLine($"Field Name: {fieldName}, Field Value: {fieldValue}");
                    }
                    sAttributes.Add(tempAttr);//将这一个要素的属性添加到大的属性集合里
                }//现在实现了几何图形，要素，字段的添加            
                MyMapObjects.moMapLayer layer = new MyMapObjects.moMapLayer(layer_name, sGeometryType, sFields);
                for (Int32 i = 0; i < sGeometries.Count; ++i)
                {
                    MyMapObjects.moFeature sFeature = new MyMapObjects.moFeature(sGeometryType, sGeometries[i], sAttributes[i]);
                    sFeatures.Add(sFeature);
                }
                layer.Features = sFeatures;
                return layer;
            }
        }

        public bool changeorder(string sqrStr)
        {
            NpgsqlCommand command = new NpgsqlCommand(sqrStr, connection); // 建立一个command
            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                MessageBox.Show("受影响的行数为".ToString() + rowsAffected.ToString()+"。请重新从数据库中载入图层查看修改后的结果");
                return true;
            }
            catch(NpgsqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public bool OpenOK()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 私有方法
        private MyMapObjects.moPoint Get_points(string xy)
        {
            //输入一对"x y"这样的字符串，返回一个点要素
            string[] parts = xy.Split(' ');
            MyMapObjects.moPoint temp;
            if (parts.Length == 2)
            {
                if (double.TryParse(parts[0], out double x) && double.TryParse(parts[1], out double y))
                {
                    // 现在 x 和 y 包含了成功转换的两个小数值
                    temp = new MyMapObjects.moPoint(x, y);
                    return temp;
                }
            }
            return null;//不行就返回一个null
        }

        private MyMapObjects.moPoints DealGeotext(string text)
        {
            MyMapObjects.moPoints temppoints = new MyMapObjects.moPoints();
            //"MULTIPOLYGON(((1334848.0705366437 5619971.878145997,1335088.668073245 5620156.191957159)))"
            string[] polygons = text.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

            // 进一步处理分割后的子字符串
            List<string> coordinates = new List<string>();

            foreach (string polygon in polygons)
            {
                string trimmedPolygon = polygon.Trim(); // 去除首尾的空格
                if (trimmedPolygon[0] == 'M')
                    continue;
                temppoints.Add(Get_points(trimmedPolygon));
            }
            return temppoints;
        }

        #endregion
        //SELECT gid,name,code,type,id,area,ST_AsText(geom) AS geom FROM public.province
        //UPDATE public.province SET name = '浙江' WHERE name = '浙江省'
    }
}
