using SF_DIY.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF_DIY
{
    public class RemoveProductListArgs :EventArgs
    {
        private List<Product> _ProductList;
        private List<int> _RemoveNumber;
        public RemoveProductListArgs(List<Product> ProductList)
        {
            _ProductList = new List<Product>();
            _ProductList = ProductList;
        }
        public RemoveProductListArgs()
        {

        }
        public List<Product> ProductList
        {
            get
            {
                return _ProductList;
            }
            set
            {
                _ProductList = value;
            }
        }
        public List<int> RemoveNumber
        {
            get
            {
                return _RemoveNumber;
            }
            set
            {
                _RemoveNumber = value;
            }
        }
    }
}
