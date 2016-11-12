using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace SF_DIY
{
    /// <summary>
    /// EditWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WallEditWindow : Window
    {
        WallEditWindowViewModel vm;
        Wall wallInfo;
        public WallEditWindow(Wall wall)
        {
            InitializeComponent();
            this.Title = "벽 상세수정";
            vm = new WallEditWindowViewModel(wall, this);
            this.DataContext = vm;
            wallInfo = wall;
            vm.StartPointX = wallInfo.StartPoint.X;
            vm.StartPointY = wallInfo.StartPoint.Y;
            vm.EndPointX = wallInfo.EndPoint.X;
            vm.EndPointY = wallInfo.EndPoint.Y;
            vm.Distance = (wallInfo.Width * 10);
            vm.Height = (wallInfo.Height * 10);
            vm.Depth = (wallInfo.Depth * 10);
        }
    }
}
