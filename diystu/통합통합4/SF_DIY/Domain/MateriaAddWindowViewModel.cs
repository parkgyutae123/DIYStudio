using MVVMBase.Command;
using MVVMBase.ViewModel;
using SF_DIY.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SF_DIY.Domain
{
    class MateriaAddWindowViewModel :ViewModelBase
    {
        

        MyModel mymodel;

        Dictionary<string, Product> Cylin_NameDic;
        Dictionary<string, Product> Rec_NameDic;

        Product SelectedProduct;
        public MateriaAddWindowViewModel()
        {
            mymodel = MyModel.GetInstance();
            Cylin_NameDic = new Dictionary<string, Product>();
            Rec_NameDic = new Dictionary<string, Product>();
            SelectedProduct = new Product();

            for (short a = 0; a < 3; a++)
            {
                SelectedTabControl = a;
                LoadList();
            }
            SelectedLength = 600.ToString();
            SelectedWidth = 600.ToString();
            SelectedTabControl = 0;
        }


        #region RectangularList
        private ObservableCollection<Product> _RectangularList = new ObservableCollection<Product>();
        public ObservableCollection<Product> RectangularList
        {
            get
            {
                return _RectangularList;
            }
        }
        #endregion

        #region CylinderList
        private ObservableCollection<Product> _CylinderList = new ObservableCollection<Product>();
        public ObservableCollection<Product> CylinderList
        {
            get
            {
                return _CylinderList;
            }
        }
        #endregion

        #region BoardList
        private ObservableCollection<Product> _BoardList = new ObservableCollection<Product>();
        public ObservableCollection<Product> BoardList
        {
            get
            {
                return _BoardList;
            }
        }
        #endregion

        #region HeaderNumber
        private int _SelectedTabControl;
        public int SelectedTabControl
        {
            get
            {
                return _SelectedTabControl;
            }
            set
            {
                _SelectedTabControl = value;
                OnPropertyChanged("SelectedTabControl");
                LoadList();
            }
        }
        private void LoadList()
        {
            Dictionary<string, Product> Cylin_NameDic = new Dictionary<string, Product>();
            Dictionary<string, Product> Rec_NameDic = new Dictionary<string, Product>();

            List<string> Board_NameList = new List<string>();

            if (SelectedTabControl == 0)
            {
                RectangularList.Clear();
                var temp_List = mymodel.Get_RectangleList();
                foreach (Product p in temp_List)
                {
                    if (!Rec_NameDic.ContainsKey(p.Name))
                    {
                        Rec_NameDic[p.Name] = p;
                    }
                    else
                    {
                        if (Rec_NameDic[p.Name].MaxLength < p.MaxLength)
                        {
                            Rec_NameDic[p.Name].MaxLength = p.MaxLength;
                        }
                    }
                }

                foreach (KeyValuePair<string, Product> iter in Rec_NameDic)
                {
                    _RectangularList.Add(iter.Value);
                }
            }
            else if (SelectedTabControl == 1)
            {
                CylinderList.Clear();
                var Test_List = mymodel.Get_CylinderList();
                foreach (Product p in Test_List)
                {
                    if (!Cylin_NameDic.ContainsKey(p.Name))
                    {
                        Cylin_NameDic[p.Name] = p;
                    }
                    else
                    {
                        if (Cylin_NameDic[p.Name].MaxLength < p.MaxLength)
                        {
                            Cylin_NameDic[p.Name].MaxLength = p.MaxLength;
                        }
                    }
                }

                foreach (KeyValuePair<string, Product> iter in Cylin_NameDic)
                {
                    _CylinderList.Add(iter.Value);
                }
            }
            else
            {
                BoardList.Clear();
                var Test_List = mymodel.Get_BoardList();
                foreach (Product p in Test_List)
                {
                    if (!Board_NameList.Contains(p.Name))
                    {
                        Board_NameList.Add(p.Name);
                        BoardList.Add(p);
                    }
                }
            }
        }
        #endregion

        #region SelectedLength
        private string _SelectedLength;
        public string SelectedLength
        {
            get
            {
                return _SelectedLength;
            }
            set
            {

                if (value != _SelectedLength)
                {
                    if(SelectedDataGridItem != null)
                    {
                        if (int.Parse(value) > SelectedDataGridItem.MaxLength)
                        {
                            MessageBox.Show("최대길이는 " + SelectedDataGridItem.MaxLength + "mm 입니다.");
                            _SelectedLength = SelectedDataGridItem.MaxLength.ToString();
                            OnPropertyChanged("SelectedLength");
                            return;
                        }
                    }
                    _SelectedLength = value;
                    OnPropertyChanged("SelectedLength");
                }
            }
        }
        #endregion

        #region SelectedWidth
        private string _SelectedWidth;
        public string SelectedWidth
        {
            get
            {
                return _SelectedWidth;
            }
            set
            {
                if(value != _SelectedWidth)
                {
                    if(SelectedDataGridItem != null && SelectedTabControl == 1)
                    {
                        if(int.Parse(value) > SelectedDataGridItem.MaxWidth)
                        {
                            MessageBox.Show("최대너비는 " + SelectedDataGridItem.MaxWidth + "mm 입니다.");
                            _SelectedWidth = SelectedDataGridItem.MaxWidth.ToString();
                            OnPropertyChanged("SelectedWidth");
                            return;
                        }
                    }
                    _SelectedWidth = value;
                    OnPropertyChanged("SelectedWidth");
                }
            }
        }
        #endregion

        #region SelectedDataGridItem
        private Product _SelectedDataGridItem;
        public Product SelectedDataGridItem
        {
            get
            {
                return _SelectedDataGridItem;
            }
            set
            {
                if (_SelectedDataGridItem != value)
                {
                    _SelectedDataGridItem = value;
                    OnPropertyChanged("SelectedDataGridItem");
                }

                SelectedProduct = _SelectedDataGridItem;
            }
        }
        #endregion


        public static event Action Close;

        #region AcceptCommand
        private ICommand _AcceptCommand;
        public ICommand AcceptCommand
        {
            get
            {
                return _AcceptCommand ?? (_AcceptCommand = new AppCommand((object obj) =>
                {
                    if(SelectedProduct.Name == null)
                    {
                        return;
                    }
                    if (SelectedProduct != null)
                    {
                        Product temp = new Product();
                        temp.Diameter = SelectedProduct.Diameter;
                        temp.Height = SelectedProduct.Height;
                        temp.MaxLength = SelectedProduct.MaxLength;
                        temp.MinLength = SelectedProduct.MinLength;
                        temp.MaxWidth = SelectedProduct.MaxWidth;
                        temp.MinWidth = SelectedProduct.MinWidth;
                        temp.Name = SelectedProduct.Name;
                        temp.Num = SelectedProduct.Num;
                        temp.Price = SelectedProduct.Price;
                        temp.Texture = SelectedProduct.Texture;
                        temp.Width = SelectedProduct.Width;
                        temp.SelectedLength = int.Parse(SelectedLength);
                        if(SelectedTabControl == 0)
                        {
                            temp.ModelType = 1;
                        }
                        else if(SelectedTabControl == 1)
                        {
                            temp.ModelType = 2;
                            temp.Width = 100;
                            temp.SelectedWidth = int.Parse(SelectedWidth);    
                        }
                        else
                        {
                            temp.ModelType = 3;
                        }           
                                     
                        MyModel.GetInstance().SelectedProduct = temp;
                        Close();
                    }
                }));
              
            }
            #endregion
        }
    }

}

