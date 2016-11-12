using For_Seller;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace For_Seller
{
    class Project_DB
    {
        static string conStr = "Data Source=DESKTOP-GILSLLQ;Initial Catalog=Product_DB;Integrated Security=True";
        SqlConnection scon = new SqlConnection(conStr);

        #region DB Singleton
        static Project_DB pd;
        private Project_DB() { }                                                                                                  
        public static Project_DB GetInstance()
        {
            if (pd == null)
            {
                pd = new Project_DB();
            }
            return pd;
        }
        #endregion

        #region SaveImage(string name, byte[] imge) - 이미지 업로드
        public bool SaveImage(string name, byte[] image)
        {
            string sql = "SELECT * FROM Texture";
            using (SqlCommand scom = new SqlCommand(sql, scon))
            {
                scom.Connection.Open();
                using (SqlDataReader reader = scom.ExecuteReader())
                {
                    if (reader == null)
                    {
                        scom.Connection.Close();
                        return false;
                    }
                    while (reader.Read())
                    {
                        if (reader["TextureName"].Equals(name))
                        {
                            scom.Connection.Close();
                            return false;
                        }
                    }
                }
            }
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Texture", scon);
            SqlCommandBuilder cb = new SqlCommandBuilder(da);
            DataSet ds = new DataSet("Texture");
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(ds, "Texture");

            DataRow myRow;
            myRow = ds.Tables["Texture"].NewRow();

            myRow["TextureName"] = name;
            myRow["TextureImage"] = image;
            ds.Tables["Texture"].Rows.Add(myRow);
            da.Update(ds, "Texture");
            scon.Close();

            return true;
        }
        #endregion

        #region SaveProductList
        public bool SaveProductList(List<Product> ProductList)
        {
            string sql = null;
            if (ProductList[0] == null)
            {
                return false;
            }
            if (ProductList[0].ModelType == 1)
            {
                sql = "SELECT * FROM Rectangular_lumber";
            }
            else if (ProductList[0].ModelType == 2)
            {
                sql = "SELECT * FROM Board_lumber";
            }
            else if (ProductList[0].ModelType == 3)
            {
                sql = "SELECT * FROM Cylinder_lumber";
            }
            else
            {
                return false;
            }

            SqlDataAdapter da = new SqlDataAdapter(sql, scon);
            SqlCommandBuilder cb = new SqlCommandBuilder(da);
            DataSet ds = new DataSet("Temp_Table");
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(ds, "Temp_Table");

            foreach (var p in ProductList)
            {
                DataRow myRow;
                myRow = ds.Tables["Temp_Table"].NewRow();
                myRow["Name"] = p.Name;
                myRow["MaxLength"] = p.MaxLength;
                myRow["Price"] = p.Price;
                myRow["Texture"] = p.Texture;
                if (p.ModelType == 1)
                {
                    myRow["Width"] = p.Width;
                    myRow["Height"] = p.Height;
                }
                else if (p.ModelType == 2)
                {
                    myRow["Height"] = p.Height;
                    myRow["MinLength"] = p.MinLength;
                    myRow["MaxWidth"] = p.MaxWidth;
                    myRow["MinWidth"] = p.MinWidth;
                }
                else
                {
                    myRow["Diameter"] = p.Diameter;
                }

                ds.Tables["Temp_Table"].Rows.Add(myRow);
            }

            da.Update(ds, "Temp_Table");
            scon.Close();
            return true;
        }
        #endregion
        #region LoadList();
        public List<string> LoadList()
        {
            List<string> list = new List<string>();
            string sql = "SELECT TextureName FROM Texture";
            using (SqlCommand scom = new SqlCommand(sql, scon))
            {
                scom.Connection.Open();
                using (SqlDataReader reader = scom.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader["TextureName"].ToString());
                    }
                }
                scom.Connection.Close();
            }
            return list;
        }
        #endregion

        #region RemoveProductList
        public bool ReomveProductList(List<Product> ProductList)
        {
            string sql = null;
            if (ProductList == null)
            {
                return false ;
            }

            foreach(var p in ProductList)
            {
                if (p.ModelType == 1)
                {
                    
                    sql = string.Format("DELETE FROM Rectangular_lumber WHERE Name = '{0}' AND Texture = '{1}' AND Price = '{2}' AND MaxLength = '{3}'", p.Name, p.Texture, p.Price, p.MaxLength);
                }
                else if (p.ModelType == 2)
                {
                    sql = string.Format("DELETE FROM Board_lumber WHERE Name = '{0}' AND Texture = '{1}' AND Price = '{2}'", p.Name, p.Texture, p.Price);
                }
                else if (p.ModelType == 3)
                {
                    sql = string.Format("DELETE FROM Cylinder_lumber WHERE Name = '{0}' AND Texture = '{1}' AND Price = '{2}' AND diameter = '{3}'", p.Name, p.Texture,p.Price, p.Diameter);
                }
                SqlCommand scom = new SqlCommand(sql, scon);
                scom.Connection.Open();
                scom.ExecuteNonQuery();
                scom.Connection.Close();
            }
            return true;

        }
        #endregion

        #region LoadImage - 이미지 불러오기
        public byte[] LoadImage(string TextureName)
        {
            string sql = string.Format("SELECT TextureImage FROM Texture WHERE TextureName = '{0}'", TextureName);
            byte[] ImageSouece;
            using (SqlCommand scom = new SqlCommand(sql, scon))
            {
                scom.Connection.Open();
                ImageSouece = (byte[])scom.ExecuteScalar();
                scom.Connection.Close();
            }
            return ImageSouece;
        }
        #endregion

        #region RemoveImage
        public void RemoveImage(string TextureName)
        {
            string sql = string.Format("DELETE FROM Texture WHERE TextureName = '{0}'", TextureName);
            SqlCommand scom = new SqlCommand(sql, scon);
            scom.Connection.Open();
            scom.ExecuteNonQuery();
            scom.Connection.Close();
        }

        #endregion

        #region Rectagular_lumber
        public List<Product> Rectangular_lumber()
        {
            List<Product> temp_list = new List<Product>();
            string sql = string.Format("SELECT * FROM Rectangular_lumber");
            using (SqlCommand scom = new SqlCommand(sql, scon))
            {
                scom.Connection.Open();
                using (SqlDataReader reader = scom.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Product temp = new Product();
                        temp.Name = reader["Name"].ToString();
                        temp.Texture = reader["Texture"].ToString();
                        temp.Width = (double)reader["Width"];
                        temp.Height = (double)reader["Height"];
                        temp.MaxLength = (int)reader["MaxLength"];
                        temp.Price = (double)reader["Price"];
                        temp.ModelType = 2;
                        temp_list.Add(temp);
                    }
                }
                scom.Connection.Close();
            }
            return temp_list;
        }

        #endregion

        #region Cylinder_lumber
        public List<Product> Cylinder_lumber()
        {
            List<Product> temp_list = new List<Product>();
            string sql = string.Format("SELECT * FROM Cylinder_lumber");
            using (SqlCommand scom = new SqlCommand(sql, scon))
            {
                scom.Connection.Open();
                using (SqlDataReader reader = scom.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Product temp = new Product();
                        temp.Name = reader["Name"].ToString();
                        temp.Texture = reader["Texture"].ToString();
                        temp.Diameter = (double)reader["diameter"];
                        temp.MaxLength = (int)reader["MaxLength"];
                        temp.Price = (double)reader["Price"];
                        temp.ModelType = 3;
                        temp_list.Add(temp);
                    }
                }
                scom.Connection.Close();
            }
            return temp_list;
        }

        #endregion

        #region Board_lumber
        public List<Product> Board_lumber()
        {
            List<Product> temp_list = new List<Product>();
            string sql = string.Format("SELECT * FROM Board_lumber");
            using (SqlCommand scom = new SqlCommand(sql, scon))
            {
                scom.Connection.Open();
                using (SqlDataReader reader = scom.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Product temp = new Product();
                        temp.Name = reader["Name"].ToString();
                        temp.MaxLength = (int)reader["MaxLength"];
                        temp.MinLength = (int)reader["MinLength"];
                        temp.MaxWidth = (int)reader["MaxWidth"];
                        temp.MinWidth = (int)reader["Minwidth"];
                        temp.Texture = reader["Texture"].ToString();
                        temp.Price = (double)reader["Price"];
                        temp.Height = (double)reader["Height"];
                        temp.ModelType = 1;
                        temp_list.Add(temp);
                    }
                }
                scom.Connection.Close();
            }
            return temp_list;
        }

        #endregion

        #region ImageSource
        public Dictionary<string, byte[]> GetImageFile()
        {
            Dictionary<string, byte[]> reDic = new Dictionary<string, byte[]>();

            string sql = "SELECT * FROM Texture";
            using (SqlCommand scom = new SqlCommand(sql, scon))
            {
                scom.Connection.Open();
                using (SqlDataReader reader = scom.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["TextureName"].ToString();
                        byte[] imageSource = (byte[])reader["TextureImage"];
                        reDic[name] = imageSource;
                    }
                }
                scom.Connection.Close();
            }
            return reDic;
        }
        #endregion
    }
}
