using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace For_Insert_Image
{
    class Program
    {
        static void Main(string[] args)
        {
            //conStr 만 바꿔 니 컴퓨터 sql서버 문자열로.
            string conStr = "Data Source=DESKTOP-GILSLLQ;Initial Catalog=Product_DB;Integrated Security=True";

            SqlConnection scon = new SqlConnection(conStr);

            string dirPath = string.Format(Environment.CurrentDirectory + "\\Image");

            if (Directory.Exists(dirPath))
            {
                DirectoryInfo di = new DirectoryInfo(dirPath);
                foreach (var item in di.GetFiles())
                {
                    string temp = item.Name;
                    string path = string.Format(item.Directory + "\\" + item.Name);
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);

                    byte[] image = br.ReadBytes((int)fs.Length);
                    string image_name = Path.GetFileNameWithoutExtension(item.Name);

                    scon.Open();

                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Texture", scon);
                    SqlCommandBuilder cb = new SqlCommandBuilder(da);
                    DataSet ds = new DataSet("Texture");
                    da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                    da.Fill(ds, "Texture");

                    DataRow myRow;
                    myRow = ds.Tables["Texture"].NewRow();

                    myRow["TextureName"] = image_name;
                    myRow["TextureImage"] = image;
                    ds.Tables["Texture"].Rows.Add(myRow);
                    da.Update(ds, "Texture");

                    scon.Close();
                }
            }

            string TextPath = string.Format(Environment.CurrentDirectory + "\\Text");

            if (Directory.Exists(TextPath))
            {
                DirectoryInfo di = new DirectoryInfo(TextPath);
                using (SqlConnection sscon = new SqlConnection(conStr))
                {
                    sscon.Open();

                    foreach (var item in di.GetFiles())
                    {
                        string path = string.Format(item.Directory + "\\" + item.Name);
                        string[] textline = File.ReadAllLines(path, Encoding.Default);

                        if (textline.Length > 0)
                        {
                            foreach (var query in textline)
                            {
                                SqlCommand cmd = new SqlCommand();
                                cmd.Connection = sscon;
                                cmd.CommandText = query;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                }
            }
        }
    }
}
