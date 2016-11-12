using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Test
{
    class DB
    {
        static string conStr = "Data Source=DESKTOP-GILSLLQ;Initial Catalog=Product_DB;Integrated Security=True";
        SqlConnection scon = new SqlConnection(conStr);

        static DB dbm;
        private DB ()
        {

        }
        public static DB GetInstance()
        {
            if(dbm == null)
            {
                dbm = new DB();
            }
            return dbm;
        }

        #region SaveImage(string name, byte[] image) - 이미지 업로드
        public bool SaveImange(string name, byte[] image)
        {
            string sql = "SELECT * FROM DB_Image";
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
                        if (reader["ImageName"].Equals(name))
                        {
                            scom.Connection.Close();
                            return false;
                        }
                    }
                }
            }
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM DB_Image", scon);
            SqlCommandBuilder CB = new SqlCommandBuilder(da);
            DataSet ds = new DataSet("DB_Image");
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(ds, "DB_Image");

            DataRow myRow;
            myRow = ds.Tables["DB_Image"].NewRow();

            myRow["ImageName"] = name;
            myRow["ImageFile"] = image;
            ds.Tables["DB_Image"].Rows.Add(myRow);
            da.Update(ds, "DB_Image");
            scon.Close();

            return true;
        }
        #endregion


        #region LoadList() - DB 목록 불러오기
        public List<string> LoadList()
        {
            List<string> list = new List<string>();
            string sql = "SELECT ImageName FROM DB_Image";
            using (SqlCommand scom = new SqlCommand(sql, scon))
            {
                scom.Connection.Open();
                using (SqlDataReader reader = scom.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader["ImageName"].ToString());
                    }
                }
                scom.Connection.Close();
            }
            return list;
        }

        #endregion


        #region LoadImage - 이미지 불러오기
        public byte[] LoadImage(string imageName)
        {
            string sql = string.Format("SELECT ImageFile FROM DB_Image WHERE ImageName = '{0}'", imageName);
            byte[] ImageSource;
            using (SqlCommand scom = new SqlCommand(sql, scon))
            {
                scom.Connection.Open();
                ImageSource = (byte[])scom.ExecuteScalar();
                scom.Connection.Close();
            }
            return ImageSource;
        }
        #endregion
    }
}
