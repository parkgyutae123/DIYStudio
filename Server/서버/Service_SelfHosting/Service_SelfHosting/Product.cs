using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace For_Seller
{
    public class Product
    {
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

        #region Texture
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
        #endregion`

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

        #region MaxLength
        private int _MaxLength;
        public int MaxLength
        {
            get
            {
                return _MaxLength;
            }
            set
            {
                _MaxLength = value;
            }
        }
        #endregion

        #region MinLength
        private int _MinLength;
        public int MinLength
        {
            get
            {
                return _MinLength;
            }
            set
            {
                _MinLength = value;
            }
        }
        #endregion

        #region MaxWidth
        private int _MaxWidth;
        public int MaxWidth
        {
            get
            {
                return _MaxWidth;
            }
            set
            {
                _MaxWidth = value;
            }
        }
        #endregion

        #region MinWidth
        private int _MinWidth;
        public int MinWidth
        {
            get
            {
                return _MinWidth;
            }
            set
            {
                _MinWidth = value;
            }
        }
        #endregion

        #region Price
        private double _Price;
        public double Price
        {
            get
            {
                return _Price;
            }
            set
            {
                _Price = value;
            }
        }
        #endregion

        #region Diameter
        private double _Diameter;
        public double Diameter
        {
            get
            {
                return _Diameter;
            }
            set
            {
                _Diameter = value;
            }
        }
        #endregion

        #region SelectedLength
        private int _SelectedLength;
        public int SelectedLength
        {
            get
            {
                return _SelectedLength;
            }
            set
            {
                _SelectedLength = value;
            }
        }
        #endregion

        #region SelectedWidth
        private int _SelectedWidth;
        public int SelectedWidth
        {
            get
            {
                return _SelectedWidth;
            }
            set
            {
                _SelectedLength = value;
            }
        }
        #endregion
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

        #region ModelType
        private int _ModelType;
        public int ModelType
        {
            get
            {
                return _ModelType;
            }
            set
            {
                _ModelType = value;
            }
        }
        #endregion

        #region CompareToVoxel
        private int _CompareToVoxel;
        public int CompareToVoxel
        {
            get
            {
                return _CompareToVoxel;
            }
            set
            {
                _CompareToVoxel = value;
            }
        }
        #endregion
    }
}
