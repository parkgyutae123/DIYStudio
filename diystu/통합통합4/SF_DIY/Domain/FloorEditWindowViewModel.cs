using MVVMBase.Command;
using MVVMBase.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SF_DIY.Domain
{
    class FloorEditWindowViewModel : ViewModelBase
    {
        private Floor floor;
        private FloorEditWindow This;
        private FloorEditWindowViewModel[] TextureImages;
        Dictionary<string, BitmapImage> ImgeDic;
        static int index = 0;

        #region FName
        private string _FName;
        public string FName
        {
            get
            {
                return _FName;
            }
            set
            {
                _FName = value;
                OnPropertyChanged("FName");
            }
        }
        #endregion
        #region FHeight
        private double _FHeight;
        public double FHeight
        {
            get
            {
                return _FHeight;
            }
            set
            {
                _FHeight = value;
                OnPropertyChanged("FHeight");
            }
        }
        #endregion
        #region IsShowFloor
        private bool _IsShowFloor;
        public bool IsShowFloor
        {
            get
            {
                return _IsShowFloor;
            }
            set
            {
                _IsShowFloor = value;
                OnPropertyChanged("IsShowFloor");
            }
        }
        #endregion
        #region IsShowCeiling
        private bool _IsShowCeiling;
        public bool IsShowCeiling
        {
            get
            {
                return _IsShowCeiling;
            }
            set
            {
                _IsShowCeiling = value;
                OnPropertyChanged("IsShowCeiling");
            }
        }
        #endregion
        #region IsCeilingColorUsing
        private bool _IsCeilingColorUsing;
        public bool IsCeilingColorUsing
        {
            get
            {
                return _IsCeilingColorUsing;
            }
            set
            {
                _IsCeilingColorUsing = value;
                OnPropertyChanged("IsCeilingColorUsing");
            }
        }
        #endregion
        #region IsCeilingTextureUsing
        private bool _IsCeilingTextureUsing;
        public bool IsCeilingTextureUsing
        {
            get
            {
                return _IsCeilingTextureUsing;
            }
            set
            {
                _IsCeilingTextureUsing = value;
                OnPropertyChanged("IsCeilingTextureUsing");
            }
        }
        #endregion
        #region IsFloorColorUsing
        private bool _IsFloorColorUsing;
        public bool IsFloorColorUsing
        {
            get
            {
                return _IsFloorColorUsing;
            }
            set
            {
                _IsFloorColorUsing = value;
                OnPropertyChanged("IsFloorColorUsing");
                
            }
        }
        #endregion
        #region IsFloorTextureUsing
        private bool _IsFloorTextureUsing;
        public bool IsFloorTextureUsing
        {
            get
            {
                return _IsFloorTextureUsing;
            }
            set
            {
                _IsFloorTextureUsing = value;
                OnPropertyChanged("IsFloorTextureUsing");

            }
        }
        #endregion
        #region FloorSelectedColor
        private Brush _FloorSelectedColor;
        public Brush FloorSelectedColor
        {
            get
            {
                return _FloorSelectedColor;
            }
            set
            {
                _FloorSelectedColor = (Brush)value;
                if(IsShowFloor==false)
                {
                    IsShowFloor = true;
                }
                IsFloorColorUsing = true;
                OnPropertyChanged("FloorSelectedColor");
            }

        }
        #endregion
        #region CeilingSelectedColor
        private Brush _CeilingSelectedColor;
        public Brush CeilingSelectedColor
        {
            get
            {
                return _CeilingSelectedColor;
            }
            set
            {
                _CeilingSelectedColor = value;
                IsShowCeiling = true;
                IsCeilingColorUsing = true;
                OnPropertyChanged("CeilingSelectedColor");
            }
        }
        #endregion

        #region OkCommand
        private ICommand _OkCommand;
        public ICommand OkCommand
        {
            get
            {
                return _OkCommand ?? (_OkCommand = new AppCommand(okcommand));
            }
        }
        private void okcommand(object obj)
        {
            floor.RoomName = FName;
            floor.FHeight = FHeight;
            floor.IsShowCeiling = IsShowCeiling;
            floor.IsShowFloor = IsShowFloor;
            if(IsShowFloor)
            {
                if(IsFloorColorUsing)
                {
                    if(FloorSelectedColor!=null)
                    {
                        byte a = ((Color)FloorSelectedColor.GetValue(SolidColorBrush.ColorProperty)).A;
                        byte g = ((Color)FloorSelectedColor.GetValue(SolidColorBrush.ColorProperty)).G;
                        byte r = ((Color)FloorSelectedColor.GetValue(SolidColorBrush.ColorProperty)).R;
                        byte b = ((Color)FloorSelectedColor.GetValue(SolidColorBrush.ColorProperty)).B;
                        floor.FloorColor = Color.FromArgb(a, r, g, b);
                    }
                }
                else if(IsFloorTextureUsing)
                {
                    if (SelectedFloorTextureItem != null)
                        floor.FloorTextruePath = SelectedFloorTextureItem.ImgName;
                }
            }
            if(IsShowCeiling)
            {
                if(IsCeilingColorUsing)
                {
                    if(CeilingSelectedColor!=null)
                    {
                        byte a = ((Color)CeilingSelectedColor.GetValue(SolidColorBrush.ColorProperty)).A;
                        byte g = ((Color)CeilingSelectedColor.GetValue(SolidColorBrush.ColorProperty)).G;
                        byte r = ((Color)CeilingSelectedColor.GetValue(SolidColorBrush.ColorProperty)).R;
                        byte b = ((Color)CeilingSelectedColor.GetValue(SolidColorBrush.ColorProperty)).B;
                        floor.CeilingColor = Color.FromArgb(a, r, g, b);
                    }
                }
                if(IsCeilingTextureUsing)
                {
                    if (SelectedCeilingTextureItem != null)
                        floor.CeilingTextruePath = SelectedCeilingTextureItem.ImgName;
                }
            }

            floor.IsShowFloor = IsShowFloor;
            floor.IsShowCeiling = IsShowCeiling;
            floor.IsFloorColorUsing = IsFloorColorUsing;
            floor.IsFloorTextureUsing = IsFloorTextureUsing;
            floor.IsCeilingColorUsing = IsCeilingColorUsing;
            floor.IsCeilingTextureUsing = IsCeilingTextureUsing;
            This.Close();
        }
        #endregion

        public FloorEditWindowViewModel(Floor floor, FloorEditWindow This)
        {
            this.floor = floor;
            this.This = This;
            FName = floor.RoomName;
            FHeight = floor.FHeight;
            IsShowFloor = floor.IsShowFloor;
            IsShowCeiling = floor.IsShowCeiling;
            IsFloorColorUsing = floor.IsFloorColorUsing;
            IsFloorTextureUsing = floor.IsFloorTextureUsing;
            IsCeilingColorUsing = floor.IsCeilingColorUsing;
            IsCeilingTextureUsing = floor.IsCeilingTextureUsing;


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
            TextureImages = new FloorEditWindowViewModel[ImgeDic.Count];
            int cnt = 0;
            foreach (KeyValuePair<string, BitmapImage> iter in ImgeDic)
            {
                ImageSource temp_Source = iter.Value as ImageSource;
                TextureImages[cnt] = new FloorEditWindowViewModel(iter.Key, temp_Source);
                if(floor.FloorTextruePath != null)
                {
                    if (floor.FloorTextruePath.Equals(iter.Key))
                    {
                        SelectedFloorIndex = cnt;
                    }
                }
                if(floor.CeilingTextruePath != null)
                {
                    if (floor.CeilingTextruePath.Equals(iter.Key))
                    {
                        SelectedCeilingIndex = cnt;
                    }
                }
                cnt++;
            }
        }

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

        #region SelectedFloorTextureItem
        private FloorEditWindowViewModel _SelectedFloorTextureItem;
        public FloorEditWindowViewModel SelectedFloorTextureItem
        {
            get
            {
                return _SelectedFloorTextureItem;
            }
            set
            {
                _SelectedFloorTextureItem = value;
                OnPropertyChanged("SelectedFloorTextureItem");
            }
        }
        #endregion
        #region SelectedCeilingTextureItem
        private FloorEditWindowViewModel _SelectedCeilingTextureItem;
        public FloorEditWindowViewModel SelectedCeilingTextureItem
        {
            get
            {
                return _SelectedCeilingTextureItem;
            }
            set
            {
                _SelectedCeilingTextureItem = value;
                OnPropertyChanged("SelectedCeilingTextureItem");
            }
        }
        #endregion
        #region SelectedFloorIndex
        private int _SelectedFloorIndex;
        public int SelectedFloorIndex
        {
            get
            {
                return _SelectedFloorIndex;
            }
            set
            {
                _SelectedFloorIndex = value;
                if(IsShowFloor == false)
                {
                    IsShowFloor = true;
                }
                IsFloorTextureUsing = true;
                OnPropertyChanged("SelectedFloorIndex");
            }
        }
        #endregion
        #region SelectedCeilingIndex
        private int _SelectedCeilingIndex;
        public int SelectedCeilingIndex
        {
            get
            {
                return _SelectedCeilingIndex;
            }
            set
            {
                _SelectedCeilingIndex = value;
                if (IsShowCeiling == false)
                {
                    IsShowCeiling = true;
                }
                IsCeilingTextureUsing = true;
                OnPropertyChanged("SelectedCeilingIndex");
            }
        }
        #endregion

        public FloorEditWindowViewModel[] All
        {
            get
            {
                return TextureImages;
            }
        }
        public FloorEditWindowViewModel(string imgName, ImageSource imgSource)
        {
            ImgName = imgName;
            ImgSource = imgSource;
            TextureIndex = index;
            index++;
        }
    }
}
