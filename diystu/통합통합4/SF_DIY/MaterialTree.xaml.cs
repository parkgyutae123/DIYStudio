using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SF_DIY
{
    /// <summary>
    /// MaterialTree.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MaterialTree : UserControl
    {
        public static Action<string> stringHandler = null;
        public MaterialTree()
        {
            InitializeComponent();
            BaseTreeViewInit();
            SetUserFurnitureItemInit();
            SetObjFurnitureItemInit();
        }

        private void BaseTreeViewInit()
        { 

        }

        private void SetObjFurnitureItemInit()
        {
            string dirPath = @"..\..\resources\obj";
            if (System.IO.Directory.Exists(dirPath))
            {
                DirectoryInfo di = new DirectoryInfo(dirPath);
                ObservableCollection<TreeViewItem> useritemso = new ObservableCollection<TreeViewItem>();
                foreach (var item in di.GetFiles())
                {
                    if (item.Extension.Equals(".obj"))
                    {
                        TreeViewItem tvitem = new TreeViewItem();
                        tvitem.Header = string.Format("{0} ({1})", item.Name.Replace(".obj", ""), item.CreationTime);
                        tvitem.Tag = dirPath + "\\" + item.Name;
                        tvitem.PreviewMouseLeftButtonDown += ModelAdd_MouseClick;
                        useritemso.Add(tvitem);
                    }
                }
                ObjTree.ItemsSource = useritemso;
            }
        }

        private void SetUserFurnitureItemInit()
        {
            string dirPath = @"..\..\resources\user";
            if(System.IO.Directory.Exists(dirPath))
            {
                DirectoryInfo di = new DirectoryInfo(dirPath);
                ObservableCollection<TreeViewItem> useritemso = new ObservableCollection<TreeViewItem>();
                foreach (var item in di.GetFiles())
                {
                    if(item.Extension.Equals(".xaml"))
                    {
                        TreeViewItem tvitem = new TreeViewItem();
                        tvitem.Header = string.Format("{0} ({1})", item.Name.Replace(".xaml", ""),item.CreationTime);
                        tvitem.Tag = dirPath +"\\" + item.Name;
                        tvitem.PreviewMouseLeftButtonDown += ModelAdd_MouseClick;
                        useritemso.Add(tvitem);
                    }
                }
                UserFurniture.ItemsSource = useritemso;
            }
            
        }

        private void ModelAdd_MouseClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            string itemName = item.Name;
            if (item.Tag != null)
            {
                itemName = (string)item.Tag;
            }
            if (itemName != null)
            {
                stringHandler(itemName);
            }

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetUserFurnitureItemInit();
            SetObjFurnitureItemInit();
        }
        
    }
}
