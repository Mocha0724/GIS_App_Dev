using System;
using System.Data;  // 导入数据表模块，以引入DataTable类。
using System.IO;
using Npgsql;

namespace GIS_APP_DEV
{
    public class ConnectPostgreSql  // 类：连接数据库
    {
        // Main
        public static void Main()
        {
            OutputDatabaseQuery();
        }

        // 连接数据库的用户主机端口账号密码等信息
        private static string connString = "Host=localhost;Port=5432;Username=postgres;Password=tangjintong625;Database=postgres";

        // 方法：查询，返回DataTable。输入参数：SQL语句。
        public static DataTable Query(string sqrStr)
        {
            DataTable dt = new DataTable();  // 初始化一个DataTable。
            NpgsqlConnection connection = new NpgsqlConnection(connString); // 建立连接
            NpgsqlDataAdapter sqlAdapter = new NpgsqlDataAdapter(sqrStr, connection); // 建立一个sqlAdapter，按SQL语句读取表中内容
            connection.Open();  // 打开连接
            sqlAdapter.Fill(dt);  // sqlAdapter返回的信息填充到dt里，生成要输出的表。
            MyMapObjects.Layer layer = new MyMapObjects.Layer();  // 生成层对象，但是似乎我应该知道怎样往里面填我的数据，而这个还没有写好。
            connection.Close();  // 关闭连接
            return dt;
            // 待实现：读取的数据变成layer类。

        }

        // 方法：增删改。不返回。输入参数：SQL语句。
        public static void NonExecution(string sqrStr)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))  // 建立连接
            {
                connection.Open();  // 打开连接
                using NpgsqlCommand command = new NpgsqlCommand(sqrStr);  // 执行sql指令
                connection.Close();  // 关闭连接
            }
        }

        // 待实现：方法：LayerToPostGIS，从Layer类写入PostGIS。利用NonExecution函数。

        // 方法：输出数据表。
        public static void OutputDatabaseQuery()
        {
            string fileName = @"E:\sqlText.txt";
            string sqlText = File.ReadAllText(fileName);
            DataTable dt = Query(sqlText);
            if (dt.Rows.Count > 0)
            {
                string columnName = string.Empty;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    columnName += dt.Columns[i].ColumnName + "|";
                }
                Console.WriteLine(columnName);
                foreach (DataRow row in dt.Rows)
                {
                    string columnValue = string.Empty;
                    foreach (DataColumn c in dt.Columns)
                    {
                        columnValue += row[c] + "|";
                    }
                    Console.WriteLine(columnValue);
                }
            }
        }

    }
}
