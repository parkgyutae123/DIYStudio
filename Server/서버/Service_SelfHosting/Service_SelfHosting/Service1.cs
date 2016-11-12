using DB_Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using For_Seller;

namespace Service_SelfHosting
{
    // 참고: "리팩터링" 메뉴에서 "이름 바꾸기" 명령을 사용하여 코드 및 config 파일에서 클래스 이름 "Service1"을 변경할 수 있습니다.
    public class Service1 : IService1
    {
        Project_DB db = Project_DB.GetInstance();
      
          public void RemoveImage(string name)
        {
            db.RemoveImage(name);
        }

        public bool SaveImage(string name, byte[] imahe)
        {
            bool re = db.SaveImage(name, imahe);
            return re;
        }

        public byte[] LoadImage(string imageNmae)
        {
            byte[] re = db.LoadImage(imageNmae);
            return re;
        }

        public List<string> LoadList()
        {
            List<string> reList = db.LoadList();
            return reList;
        }

        public List<Product> GetRectangular_lumber()
        {
            List<Product> reList = db.Rectangular_lumber();
            return reList;

        }

        public List<Product> GetBoard_lumber()
        {
            List<Product> reList = db.Board_lumber();
            return reList;
        }

        public List<Product> GetCylinder_lumber()
        {
            List<Product> reList = db.Cylinder_lumber();
            return reList;
        }

        public Dictionary<string, byte[]> GetImageFile()
        {
            Dictionary<string, byte[]> reDic = db.GetImageFile();
            return reDic;
        }

        public bool SaveProduct(List<Product> ProductList)
        {
            return db.SaveProductList(ProductList);
        }

        public bool RemoveProduct(List<Product> ProductList)
        {
            return db.ReomveProductList(ProductList);
        }
    }
}
