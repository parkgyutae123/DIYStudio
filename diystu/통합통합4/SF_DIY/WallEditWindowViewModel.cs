using HelixToolkit.Wpf;
using MVVMBase.Command;
using MVVMBase.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace SF_DIY
{
    class WallEditWindowViewModel : ViewModelBase
    {
        private Wall wall;
        private WallEditWindow This;
        private WallEditWindowViewModel[] TextureImages;
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

        #region SelectedTextureItem
        private WallEditWindowViewModel _SelectedTextureItem;
        public WallEditWindowViewModel SelectedTextureItem
        {
            get
            {
                return _SelectedTextureItem;
            }
            set
            {
                _SelectedTextureItem = value;
                OnPropertyChanged("SelectedTextureItem");
            }
        }
        #endregion
        #region SelectedTextureItem1
        private WallEditWindowViewModel _SelectedTextureItem1;
        public WallEditWindowViewModel SelectedTextureItem1
        {
            get
            {
                return _SelectedTextureItem1;
            }
            set
            {
                _SelectedTextureItem1 = value;
                OnPropertyChanged("SelectedTextureItem1");
            }
        }
        #endregion

        #region SelectedIndex
        private int _SelectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                _SelectedIndex = value;
                OnPropertyChanged("SelectedIndex");
            }
        }
        #endregion
        #region SelectedIndex1
        private int _SelectedIndex1;
        public int SelectedIndex1
        {
            get
            {
                return _SelectedIndex1;
            }
            set
            {
                _SelectedIndex1 = value;
                OnPropertyChanged("SelectedIndex1");
            }
        }
        #endregion
        public WallEditWindowViewModel[] All
        {
            get
            {
                return TextureImages;
            }
        }
        public WallEditWindowViewModel(string imgName, ImageSource imgSource)
        {
            ImgName = imgName;
            ImgSource = imgSource;
            TextureIndex = index;
            index++;
        }
        public WallEditWindowViewModel(Wall wall, WallEditWindow _this)
        {
            this.wall = wall;
            this.This = _this;
            ImgeDic = new Dictionary<string, BitmapImage>();

            //이미지 초기화
            string dirPath = @"..\..\Texture\sources\";
            if (System.IO.Directory.Exists(dirPath))
            {
                DirectoryInfo di = new DirectoryInfo(dirPath);
                foreach (var item in di.GetFiles())
                {
                    if (item.Extension.Equals(".png"))
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri(item.FullName);
                        bi.EndInit();

                        ImgeDic[item.Name] = bi;
                    }
                    if (item.Extension.Equals(".jpg"))
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri(item.FullName);
                        bi.EndInit();

                        ImgeDic[item.Name] = bi;
                    }
                }
            }
            TextureImages = new WallEditWindowViewModel[ImgeDic.Count];
            int cnt = 0;
            foreach (KeyValuePair<string, BitmapImage> iter in ImgeDic)
            {
                ImageSource temp_Source = iter.Value as ImageSource;
                TextureImages[cnt] = new WallEditWindowViewModel(iter.Key, temp_Source);
                if (wall.Side2_TextureName.Equals(iter.Key))
                {
                    SelectedIndex = cnt;
                }
                if (wall.Side3_TextureName.Equals(iter.Key))
                {
                    SelectedIndex1 = cnt;
                }
                cnt++;
            }

        }

        #region StartPoint X
        private double _StartPointX = 0;
        public double StartPointX
        {
            get
            {
                return _StartPointX;
            }
            set
            {
                _StartPointX = value;
                if (StartPointX != 0 && StartPointY != 0 && EndPointX != 0 && EndPointY != 0)
                {
                    Point3D p = new Point3D(_StartPointX, _StartPointY, 0);
                    Point3D e = new Point3D(_EndPointX, _EndPointY, 0);
                    Distance = p.DistanceTo(e) * 10;
                }
                OnPropertyChanged("StartPointX");
            }
        }
        #endregion
        #region StartPoint Y
        private double _StartPointY = 0;
        public double StartPointY
        {
            get
            {
                return _StartPointY;
            }
            set
            {
                _StartPointY = value;
                if (StartPointX != 0 && StartPointY != 0 && EndPointX != 0 && EndPointY != 0)
                {
                    Point3D p = new Point3D(_StartPointX, _StartPointY, 0);
                    Point3D e = new Point3D(_EndPointX, _EndPointY, 0);
                    Distance = p.DistanceTo(e) * 10;
                }
                OnPropertyChanged("StartPointY");
            }
        }
        #endregion
        #region EndPointX
        private double _EndPointX = 0;
        public double EndPointX
        {
            get
            {
                return _EndPointX;
            }
            set
            {
                _EndPointX = Math.Round(value);
                if (StartPointX != 0 && StartPointY != 0 && EndPointX != 0 && EndPointY != 0)
                {
                    Point3D p = new Point3D(_StartPointX, _StartPointY, 0);
                    Point3D e = new Point3D(_EndPointX, _EndPointY, 0);
                    Distance = p.DistanceTo(e) * 10;
                }
                OnPropertyChanged("EndPointX");
            }
        }
        #endregion
        #region EndPointY
        private double _EndPointY = 0;
        public double EndPointY
        {
            get
            {
                return _EndPointY;
            }
            set
            {
                _EndPointY = Math.Round(value);
                if (StartPointX != 0 && StartPointY != 0 && EndPointX != 0 && EndPointY != 0)
                {
                    Point3D p = new Point3D(_StartPointX, _StartPointY, 0);
                    Point3D e = new Point3D(_EndPointX, _EndPointY, 0);
                    Distance = p.DistanceTo(e) * 10;
                }
                OnPropertyChanged("EndPointY");
            }
        }
        #endregion
        #region Distance 길이
        private double _Distance;
        public double Distance
        {
            get
            {
                return _Distance;
            }
            set
            {
                _Distance = Math.Round(value);
                OnPropertyChanged("Distance");
            }
        }
        #endregion
        #region 높이
        private double _Depth;
        public double Depth
        {
            get
            {
                return _Depth;
            }
            set
            {
                _Depth = value;
                OnPropertyChanged("Depth");
            }
        }
        #endregion
        #region 두께
        private double _Height;
        public double Height
        {
            get
            {
                return _Height;
            }
            set
            {
                _Height = value;
                OnPropertyChanged("Height");
            }
        }
        #endregion
        #region OkCommand
        private ICommand _OkCommand;
        public ICommand OkCommand
        {
            get
            {
                return _OkCommand ?? (_OkCommand = new AppCommand(OkCom));
            }
        }
        private void OkCom(object obj)
        {
            wall.Width = Distance / 10;
            wall.Height = Height / 10;
            wall.Depth = Depth / 10;
            wall.StartPoint = new Point3D(StartPointX, StartPointY, 0);

            wall.EndPoint = new Point3D(((Distance / 10) * Math.Sin(GetRad(StartPointX, StartPointY, EndPointX, EndPointY)) + StartPointX),
               ((Distance / 10) * Math.Cos(GetRad(StartPointX, StartPointY, EndPointX, EndPointY)) + StartPointY), 0);
            wall.Angle = -GetAngle(wall.StartPoint.X, wall.StartPoint.Y, wall.EndPoint.X, wall.EndPoint.Y);
            if(SelectedTextureItem != null)
                wall.Side2_TextureName =SelectedTextureItem.ImgName;
            if (SelectedTextureItem1 != null)
                wall.Side3_TextureName =SelectedTextureItem1.ImgName;
            This.Close();
        }
        #endregion
        #region 메소드
        private double GetRad(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;

            double rad = Math.Atan2(dx, dy);
            return rad;
        }
        private double GetAngle(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;

            double rad = Math.Atan2(dx, dy);
            double degree = (rad * 180) / Math.PI;
            return degree - 90;
        }
        #endregion
    }
}
