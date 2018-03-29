using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ip.Text) && !string.IsNullOrWhiteSpace(userid.Text) && !string.IsNullOrWhiteSpace(password.Text))
            {
                try
                {
                    var list = await Task.Run(() => GetAllDataBase(ip.Text, userid.Text, password.Text));
                    comboBox1.DataSource = list;
                    MessageBox.Show("连接成功!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("error,请确认是否为空!");
            }

        }

        /// <summary>
        /// 获取指定IP地址的数据库所有数据库实例名。
        /// </summary>
        /// <param name="ip">指定的 IP 地址。</param>
        /// <param name="username">登录数据库的用户名。</param>
        /// <param name="password">登陆数据库的密码。</param>
        /// <returns>返回包含数据实例名的列表。</returns>
        private ArrayList GetAllDataBase(string ip, string username, string password)
        {
            ArrayList DBNameList = new ArrayList();
            SqlConnection Connection = new SqlConnection(
                String.Format("Data Source={0};DataBase = master;User ID = {1};PWD = {2}", ip, username, password));
            DataTable DBNameTable = new DataTable();
            SqlDataAdapter Adapter = new SqlDataAdapter("select name from master..sysdatabases", Connection);
            lock (Adapter)
            {
                Adapter.Fill(DBNameTable);
            }

            foreach (DataRow row in DBNameTable.Rows)
            {
                DBNameList.Add(row["name"]);
            }
            Connection.Close();
            return DBNameList;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            this.path.Text = path.SelectedPath;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(comboBox1.SelectedValue.ToString()))
            {
                if (!string.IsNullOrWhiteSpace(path.Text))
                {
                    SqlConnection Connection = new SqlConnection($"Server={ip.Text};DataBase={comboBox1.SelectedValue.ToString()};Uid={userid.Text};Pwd={password.Text}");
                    SqlCommand cmd = Connection.CreateCommand();
                    cmd.CommandText = $"select * from {comboBox1.SelectedValue.ToString()}.information_schema.tables";
                    Connection.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            Writeclass(dr["TABLE_NAME"].ToString());
                        }
                    }
                    Connection.Close();

                }
                else
                {
                    MessageBox.Show("未选择保存路径!");
                }
            }
            else
            {
                MessageBox.Show("未连接服务器!");
            }


        }

        private void Writeclass(string database)
        {
            #region sql语句
            string sql = $@"SELECT  CASE WHEN col.colorder = 1 THEN obj.name  
                  ELSE ''  
             END AS 表名,  
        col.colorder AS 序号 ,  
        col.name AS 列名 ,  
        ISNULL(ep.[value], '') AS 列说明 ,  
        t.name AS 数据类型 ,  
        col.length AS 长度 ,  
        ISNULL(COLUMNPROPERTY(col.id, col.name, 'Scale'), 0) AS 小数位数 ,  
        CASE WHEN COLUMNPROPERTY(col.id, col.name, 'IsIdentity') = 1 THEN '√'  
             ELSE ''  
        END AS 标识 ,  
        CASE WHEN EXISTS ( SELECT   1  
                           FROM     dbo.sysindexes si  
                                    INNER JOIN dbo.sysindexkeys sik ON si.id = sik.id  
                                                              AND si.indid = sik.indid  
                                    INNER JOIN dbo.syscolumns sc ON sc.id = sik.id  
                                                              AND sc.colid = sik.colid  
                                    INNER JOIN dbo.sysobjects so ON so.name = si.name  
                                                              AND so.xtype = 'PK'  
                           WHERE    sc.id = col.id  
                                    AND sc.colid = col.colid ) THEN '√'  
             ELSE ''  
        END AS 主键 ,  
        CASE WHEN col.isnullable = 1 THEN '√'  
             ELSE ''  
        END AS 允许空 ,  
        ISNULL(comm.text, '') AS 默认值  
FROM    dbo.syscolumns col  
        LEFT  JOIN dbo.systypes t ON col.xtype = t.xusertype  
        inner JOIN dbo.sysobjects obj ON col.id = obj.id  
                                         AND obj.xtype = 'U'  
                                         AND obj.status >= 0  
        LEFT  JOIN dbo.syscomments comm ON col.cdefault = comm.id  
        LEFT  JOIN sys.extended_properties ep ON col.id = ep.major_id  
                                                      AND col.colid = ep.minor_id  
                                                      AND ep.name = 'MS_Description'  
        LEFT  JOIN sys.extended_properties epTwo ON obj.id = epTwo.major_id  
                                                         AND epTwo.minor_id = 0  
                                                         AND epTwo.name = 'MS_Description'  
WHERE   obj.name = '{database}'
ORDER BY col.colorder ;  ";
            #endregion
            SqlConnection Connection = new SqlConnection($"Server={ip.Text};DataBase={comboBox1.SelectedValue.ToString()};Uid={userid.Text};Pwd={password.Text}");
            SqlCommand cmd = Connection.CreateCommand();
            cmd.CommandText = sql;
            Connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    MessageBox.Show(dr[2].ToString());
                }
            }
            Connection.Close();
        }


    }
}
