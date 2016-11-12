using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF_DIY
{
    public class SelectedVoxelArgs :EventArgs
    {
        private int _VoxelNum;
        private bool _IsSelected;
        public SelectedVoxelArgs(int _Num, bool Selected)
        {
            _VoxelNum = _Num;
            _IsSelected = Selected;
        }
        public int VoxelNum
        {
            get
            {
                return _VoxelNum;
            }
        }
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
        }
    }
}
