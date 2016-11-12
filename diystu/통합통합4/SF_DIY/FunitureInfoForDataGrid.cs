using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF_DIY
{
    class FunitureInfoForDataGrid
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

        public int FunitureNum { get;  set; }
        #endregion
    }
}
