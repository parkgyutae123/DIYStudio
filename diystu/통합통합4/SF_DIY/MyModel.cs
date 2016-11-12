using SF_DIY.ServiceReference1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SF_DIY
{
    public delegate void SelectedProductEventHandler(Product p);

    class MyModel
    {
        static int StartCnt = 0;
        Service1Client client = new Service1Client();

        Product[] Rectangle_List;
        Product[] Cylinder_List;
        Product[] Board_List;

        Product[] rectangle_ListForPreView;
        Product[] cylinder_ListForPreView;
        Product[] board_ListForPreView;

        Dictionary<string, byte[]> Temp_Image;
        Dictionary<string, BitmapImage> Image;
        List<Product> AddProducts;

        public static event SelectedProductEventHandler Selected = null;

        public Product[] Rectangle_ListForPreView
        {
            get { return rectangle_ListForPreView; }
        }

        public Product[] Cylinder_ListForPreView
        {
            get { return cylinder_ListForPreView; }
        }

        public Product[] Board_ListForPreView
        {
            get { return board_ListForPreView; }
        }

        #region Singleton
        static MyModel mymodel;
        private MyModel()
        {
            Temp_Image = new Dictionary<string, byte[]>();
            Image = new Dictionary<string, BitmapImage>();
            AddProducts = new List<Product>();
            try
            {
                Temp_Image = client.GetImageFile();
                Rectangle_List = client.GetRectangular_lumber();
                Cylinder_List = client.GetCylinder_lumber();
                Board_List = client.GetBoard_lumber();

                rectangle_ListForPreView = client.GetRectangular_lumber();
                cylinder_ListForPreView = client.GetCylinder_lumber();
                board_ListForPreView = client.GetBoard_lumber();

                ImageBitToImageSource();
            }
            catch
            {
                MessageBoxResult result =  MessageBox.Show("네트워크가 연결되지 않았습니다. 오프라인 모드로 실행하시겠습니까? (오프라인 모드는 프로그램이 원활히 동작되지 않을 수 있습니다.", "오류", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Rectangle_List = new Product[] { new Product() };
                    Cylinder_List = new Product[] { new Product() };
                    Board_List = new Product[] { new Product() };

                    rectangle_ListForPreView = new Product[] { new Product() };
                    cylinder_ListForPreView = new Product[] { new Product() };
                    board_ListForPreView = new Product[] { new Product() };
                    return;
                }
                
            }
        }
        

        #region Init - 프로그램 처음 시작시 실행.
        public void Init()
        {
            if (StartCnt == 0)
            {
                GetInstance();
                StartCnt++;
            }
        }
        #endregion



        public static MyModel GetInstance()
        {
            if (mymodel == null)
            {
                mymodel = new MyModel();
            }
            return mymodel;
        }
        #endregion

        #region ByteToImageSource
        private void ImageBitToImageSource()
        {
            foreach (KeyValuePair<string, byte[]> iter in Temp_Image)
            {
                byte[] tempImage = iter.Value;
                BitmapImage bitImg = new BitmapImage();
                MemoryStream ms = new MemoryStream(tempImage);
                bitImg.BeginInit();
                bitImg.StreamSource = ms;
                bitImg.EndInit();
                Image[iter.Key] = bitImg;

                //이미지 생성
                try
                {
                    string path = @"..\..\Texture\" + iter.Key + ".png";
                    var image = bitImg;
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(image));
                        encoder.Save(fileStream);
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show("이미지 생성 실패 " + e.Message);
                }
            }
        }
        #endregion

        #region Get_RectangleList
        public Product[] Get_RectangleList()
        {
            return Rectangle_List;
        }
        #endregion

        #region Get_CylinderList
        public Product[] Get_CylinderList()
        {
            return Cylinder_List;
        }
        #endregion

        #region Get_BoardList
        public Product[] Get_BoardList()
        {
            return Board_List;
        }
        #endregion

        #region GetImage(string ImageName) 
        public BitmapImage GetImage(string ImageName)
        {
            return Image[ImageName];
        }
        #endregion

        #region GetImageList
        public Dictionary<string, BitmapImage> GetImageList()
        {
            return Image;
        }
        #endregion

        #region GetRectangle_Info
        public Product GetRectangle_Info(string name)
        {
            Product re = new Product();
            foreach (Product iter in Rectangle_List)
            {
                if (iter.Name == name)
                {
                    re = iter;
                    break;
                }
            }
            return re;
        }
        #endregion

        #region GetCylinder_Info
        public Product GetCylinder_Info(string name)
        {
            Product re = new Product();
            foreach (Product iter in Cylinder_List)
            {
                if (iter.Name == name)
                {
                    re = iter;
                    break;
                }
            }
            return re;
        }
        #endregion

        #region SelectedProduct
        private Product _SelectedProduct;
        public Product SelectedProduct
        {
            get
            {
                return _SelectedProduct;
            }
            set
            {
                _SelectedProduct = value;
                Selected(_SelectedProduct);
            }
        }
        #endregion
    }
}
