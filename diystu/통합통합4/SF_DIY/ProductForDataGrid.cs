using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF_DIY
{
    class ProductForDataGrid
    {
        #region Num
        private int _Num;
        public int Num
        {
            get
            {
                return _Num;
            }
            set
            {
                _Num = value;
            }
        }
        #endregion

        #region Name
        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }
        #endregion

        #region SelectedVoxel
        private bool _SelectedVoxel;
        public bool SelectedVoxel
        {
            get
            {
                return _SelectedVoxel;
            }
            set
            {
                _SelectedVoxel = value;
            }
        }
        #endregion

        #region Length
        private double _Length;
        public double Length
        {
            get
            {
                return _Length;
            }
            set
            {
                _Length = value;
            }
        }
        #endregion

        #region IsSelected
        private bool _IsSelected;
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
            }
        }
        #endregion

        #region CompareNum
        private int _CompareNum;
        public int CompareNum
        {
            get
            {
                return _CompareNum;
            }
            set
            {
                _CompareNum = value;
            }
        }
        #endregion

        #region Width
        private double _Width;
        public double Width
        {
            get
            {
                return _Width;
            }
            set
            {
                _Width = value;
            }
        }
        #endregion

        #region Height
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
            }
        }
        #endregion

        #region Texture;
        private string _Texture;
        public string Texture
        {
            get
            {
                return _Texture;
            }
            set
            {
                _Texture = value;
            }
        }
        #endregion

    }
}
