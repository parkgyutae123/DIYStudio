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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SF_DIY
{
    /// <summary>
    /// MaterialEditControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MaterialEditControl : UserControl
    {
        public MaterialEditControl()
        {
            InitializeComponent();
        }
        public static Action<int,int> AngleEventHandler = null;
        private void RotateButton_Click(object sender, MouseButtonEventArgs e)
        {
            Button btn = sender as Button;
            int type = 0;
            int angle = 0;
            if(btn!=null)
            {
                switch (btn.Name)
                {
                    case "x_0": type = 1; angle = 0; break;
                    case "x_45": type = 1; angle = 45; break;
                    case "x_90": type = 1; angle = 90; break;
                    case "x_135": type = 1; angle =135; break;
                    case "x_180": type = 1; angle = 180; break;
                    case "x_225": type = 1; angle = 225; break;
                    case "x_270": type = 1; angle = 270; break;
                    case "x_315": type = 1; angle = 315; break;
                    case "x_360": type = 1; angle = 360; break;

                    case "y_0": type = 2; angle = 0; break;
                    case "y_45": type = 2; angle = 45; break;
                    case "y_90": type = 2; angle = 90; break;
                    case "y_135": type = 2; angle = 135; break;
                    case "y_180": type = 2; angle = 180; break;
                    case "y_225": type = 2; angle = 225; break;
                    case "y_270": type = 2; angle = 270; break;
                    case "y_315": type = 2; angle = 315; break;
                    case "y_360": type = 2; angle = 360; break;

                    case "z_0": type = 3; angle = 0; break;
                    case "z_45": type = 3; angle = 45; break;
                    case "z_90": type = 3; angle = 90; break;
                    case "z_135": type = 3; angle = 135; break;
                    case "z_180": type = 3; angle = 180; break;
                    case "z_225": type = 3; angle = 225; break;
                    case "z_270": type = 3; angle = 270; break;
                    case "z_315": type = 3; angle = 315; break;
                    case "z_360": type = 3; angle = 360; break;
                }
            }
            AngleEventHandler(type, angle);
        }
    }
}
