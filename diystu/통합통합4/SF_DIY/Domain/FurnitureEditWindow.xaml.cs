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
    /// FurnitureEditWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FurnitureEditWindow : Window
    {
        private Furniture furniture;

        public FurnitureEditWindow()
        {
            InitializeComponent();
        }

        public FurnitureEditWindow(Furniture furniture)
        {
            InitializeComponent();
            this.furniture = furniture;
            tb_X.Text = Math.Round(furniture.Position.X * 10 ).ToString();
            tb_Y.Text = Math.Round(furniture.Position.Y * 10 ).ToString();
            tb_Z.Text = Math.Round(furniture.Position.Z * 10 ).ToString();
            tb_SizeX.Text = Math.Round((furniture.Width * furniture.ScaleX) * 10).ToString();
            tb_SizeY.Text = Math.Round((furniture.Height * furniture.ScaleY) * 10).ToString();
            tb_SizeZ.Text = Math.Round((furniture.Depth * furniture.ScaleZ) * 10).ToString();
            double calAngle;
            if (furniture.AngleZ < 0)
            {
                calAngle = furniture.AngleZ + 360;
            }
            else
            {
                calAngle = furniture.AngleZ;
            }
            string content = string.Format("{0}", Math.Round(calAngle).ToString());

            tb_AngleZ.Text = content;

            tb_Name.Text = furniture.FurnitureName.ToString();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            furniture.Position = new System.Windows.Media.Media3D.Point3D(double.Parse(tb_X.Text)/10, double.Parse(tb_Y.Text) / 10, double.Parse(tb_Z.Text) / 10);
            furniture.Width  = Math.Round((double.Parse(tb_SizeX.Text) / furniture.ScaleX) / 10);
            furniture.Height = Math.Round((double.Parse(tb_SizeY.Text) / furniture.ScaleY) / 10);
            furniture.Depth = Math.Round((double.Parse(tb_SizeZ.Text) / furniture.ScaleZ) / 10);
            double calAngle = double.Parse(tb_AngleZ.Text);
            furniture.AngleZ = calAngle;
            furniture.FurnitureName = tb_Name.Text;

            this.Close();
        }
    }
}
