using MVVMBase.ViewModel;
using SF_DIY.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SF_DIY
{
    class AddTextureComboBox : ViewModelBase
    {
        AddTextureComboBox[] TextureImages;

        AddTextureComboBox[] BoardList;
        AddTextureComboBox[] RectangleList;
        AddTextureComboBox[] CylinderList;

        Dictionary<string, BitmapImage> ImgeDic;
        static int index = 0;
        #region ImageSource
        private ImageSource _ImgSource;
        public ImageSource ImgSource
        {
            get
            {
                return _ImgSource;
            }
            set
            {
                _ImgSource = value;
            }
        }
        #endregion
        #region     ImageName

        private string _ImgName;
        public string ImgName
        {
            get
            {
                return _ImgName;
            }
            set
            {
                _ImgName = value;
            }
        }
        #endregion
        #region TextureIndex
        private int _TextureIndex;
        public int TextureIndex
        {
            get
            {
                return _TextureIndex;
            }
            set
            {
                _TextureIndex = value;
            }
        }
        #endregion

        #region 생성자
        public AddTextureComboBox()
        {
            ImgeDic = MyModel.GetInstance().GetImageList();
            TextureImages = new AddTextureComboBox[ImgeDic.Count];
            int cnt = 0;
            foreach (KeyValuePair<string, BitmapImage> iter in ImgeDic)
            {
                ImageSource temp_Source = iter.Value as ImageSource;
                TextureImages[cnt] = new AddTextureComboBox(iter.Key, temp_Source);
                cnt++;
            }

        }
        #endregion


        public void InitTextureRectangl0e()
        {
            int cnt = 0;
            Product[] temp = MyModel.GetInstance().Get_RectangleList();
            List<string> TextureNames = new List<string>();
            foreach (var t in temp)
            {
                if (!TextureNames.Contains(t.Texture))
                {
                    TextureNames.Add(t.Texture);
                }
            }
            RectangleList = new AddTextureComboBox[TextureNames.Count];
            cnt = 0;

            for (short a = 0; a < TextureNames.Count; a++)
            {
                foreach (KeyValuePair<string, BitmapImage> iter in ImgeDic)
                {
                    if (TextureNames[a] == iter.Key)
                    {
                        ImageSource temp_Source = iter.Value as ImageSource;
                        RectangleList[a] = new AddTextureComboBox(iter.Key, temp_Source);
                        break;
                    }
                }
            }
        }

        public void InitTextureCylinder()
        {
            int cnt = 0;
            Product[] temp = MyModel.GetInstance().Get_CylinderList();
            List<string> TextureNames = new List<string>();
            foreach (var t in temp)
            {
                if (!TextureNames.Contains(t.Texture))
                {
                    TextureNames.Add(t.Texture);
                }
            }
            CylinderList = new AddTextureComboBox[TextureNames.Count];
            cnt = 0;

            for (short a = 0; a < TextureNames.Count; a++)
            {
                foreach (KeyValuePair<string, BitmapImage> iter in ImgeDic)
                {
                    if (TextureNames[a] == iter.Key)
                    {
                        ImageSource temp_Source = iter.Value as ImageSource;
                        CylinderList[a] = new AddTextureComboBox(iter.Key, temp_Source);
                        break;
                    }
                }
            }
        }

        public void InitTextureBoard()
        {
            int cnt = 0;
            Product[] temp = MyModel.GetInstance().Get_BoardList();
            List<string> TextureNames = new List<string>();
            foreach (var t in temp)
            {
                if (!TextureNames.Contains(t.Texture))
                {
                    TextureNames.Add(t.Texture);
                }
            }
            BoardList = new AddTextureComboBox[TextureNames.Count];
            cnt = 0;

            for (short a = 0; a < TextureNames.Count; a++)
            {
                foreach (KeyValuePair<string, BitmapImage> iter in ImgeDic)
                {
                    if (TextureNames[a] == iter.Key)
                    {
                        ImageSource temp_Source = iter.Value as ImageSource;
                        BoardList[a] = new AddTextureComboBox(iter.Key, temp_Source);
                        break;
                    }
                }
            }
        }
        public AddTextureComboBox[] RectangleCombo
        {
            get
            {
                return RectangleList;
            }
        }

        public AddTextureComboBox[] BoardCombo
        {
            get
            {
                return BoardList;
            }
        }

        public AddTextureComboBox[] CylinderCombo
        {
            get
            {
                return CylinderList;
            }
        }
        public AddTextureComboBox[] All
        {
            get
            {
                return TextureImages;
            }
            set
            {
                TextureImages = value;
            }
        }

        public AddTextureComboBox(string imgName, ImageSource imgSource)
        {
            ImgName = imgName;
            ImgSource = imgSource;
            TextureIndex = index;
            index++;
        }
    }
}
