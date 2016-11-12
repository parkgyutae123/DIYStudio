using SF_DIY.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF_DIY
{
    public class AddProductListArgs :EventArgs
    {
        private readonly List<Product> _ProductList;
        private readonly int _Index;

        public AddProductListArgs(List<Product> ProductList)
        {
            _ProductList = new List<Product>();
            _ProductList = ProductList;
        }
        public List<Product> ProductList
        {
            get
            {
                return _ProductList;
            }
        }

        public int Index
        {
            get
            {
                return _Index;
            }
        }
    }
}
