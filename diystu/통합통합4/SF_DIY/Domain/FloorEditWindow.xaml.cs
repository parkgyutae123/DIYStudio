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
using System.Windows.Shapes;

namespace SF_DIY.Domain
{
    /// <summary>
    /// FurnitureOptionWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FloorEditWindow : Window
    {
        private Floor floor;

        public FloorEditWindow()
        {
            InitializeComponent();
        }

        public FloorEditWindow(Floor floor)
        {
            InitializeComponent();
            this.floor = floor;
            this.DataContext = new FloorEditWindowViewModel(floor, this);

        }

        private void CancelButtonOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
