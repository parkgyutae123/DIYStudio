using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF_DIY
{
    class DateGridUpdateArgs :EventArgs
    {
        private List<ProductForDataGrid> _ProductList = new List<ProductForDataGrid>();
        public List<ProductForDataGrid> ProductList
        {
            get
            {
                return _ProductList;
            }
        }

        public DateGridUpdateArgs(List<ProductForDataGrid> p)
        {
            _ProductList = p;
        }
    }
}
