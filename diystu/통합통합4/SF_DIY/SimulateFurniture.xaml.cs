using HelixToolkit.Wpf;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SF_DIY
{
    /// <summary>
    /// SimulateFurniture.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SimulateFurniture : UserControl
    {
        private SimulationFurnitureViewModel vm;
        private Model3D _hitModel;
        private Voxel voxel;

        public SimulateFurniture()
        {
            InitializeComponent();
            vm = new SimulationFurnitureViewModel(viewport);
            this.DataContext = vm;
        }

        private void Refresh_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetUserFurnitureItemInit();
            vm.TimerClear();
        }
        private void SetUserFurnitureItemInit()
        {
            string dirPath = @"..\..\SaveFiles";
            if (System.IO.Directory.Exists(dirPath))
            {
                DirectoryInfo di = new DirectoryInfo(dirPath);
                ObservableCollection<ListBoxItem> useritemso = new ObservableCollection<ListBoxItem>();
                foreach (var item in di.GetFiles())
                {
                    if (item.Extension.Equals(".xml"))
                    {
                        ListBoxItem lvitem = new ListBoxItem();

                        lvitem.Margin = new Thickness(1);
                        lvitem.Content = string.Format("{0} ({1})", item.Name.Replace(".xml", ""), item.CreationTime);
                        lvitem.Tag = item.FullName;
                        lvitem.PreviewMouseLeftButtonDown += ModelSelect_MouseClick;
                        useritemso.Add(lvitem);
                    }
                }
                FurnitureItemList.ItemsSource = useritemso;
            }
        }

        private void ModelSelect_MouseClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem lvitem = sender as ListBoxItem;
            if (lvitem != null)
            {
                string path = (string)lvitem.Tag;
                vm.SelectedItem(path);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetUserFurnitureItemInit();
            vm.Time = 1;
        }

        private void viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(viewport);

            if (vm.IsNullCheckVoxel())
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    foreach (var v in vm.Voxels)
                    {
                        v.Selected = false;
                    }
                }
                _hitModel = FindSourceToHViewport(viewport, p);
                if (_hitModel == null)
                {
                    vm.SelectedRelease();
                    vm.UpdateModel();
                    return;
                }
                voxel = vm.GetVoxel(_hitModel);
                if (voxel == null)
                {
                    vm.SelectedRelease();
                    vm.UpdateModel();
                    return;
                }
                vm.SelectedVoxelItem = voxel;

                vm.UpdateModel();
                vm.UpdateEditControl();
                return;
            }
        }

                

        private Model3D FindSourceToHViewport(HelixViewport3D viewport, Point p)
        {
            try
            {
                var hits = Viewport3DHelper.FindHits(viewport.Viewport, p);
                foreach (var h in hits)
                {
                    return h.Model;
                }
                return null;
            }

            catch (Exception e)
            {
                return null;
            }
        }

        private void BackwardButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.ViewportClear();
            if(vm.IsNullCheckVoxel())
            {
                vm.Backward();
            }
        }
        private void PauseButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (vm.IsNullCheckVoxel())
            {
                vm.Pause();
            }
        }
        private void ResetButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.ViewportClear();
            if (vm.IsNullCheckVoxel())
            {
                vm.Reset();
            }
        }
        private void PlayButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (vm.IsNullCheckVoxel())
            {
                vm.Play();
            }
        }
        private void ForwardButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (vm.IsNullCheckVoxel())
            {
                vm.Forward();
            }
        }

        private void Save_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.Save();
        }
    }
}
