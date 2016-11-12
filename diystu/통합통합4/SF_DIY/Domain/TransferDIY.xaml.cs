using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace SF_DIY.Domain
{
    /// <summary>
    /// TransferDIY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TransferDIY : Window
    {
        Model3DGroup saveModels;

        public TransferDIY()
        {
            InitializeComponent();
        }

        public TransferDIY(Model3DGroup saveModels)
        {
            InitializeComponent();
            this.saveModels = saveModels;
        }

        private void CancelButtonOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string name = tbox_furnitureName.Text;
            if (name == null) return;
            string path = @"..\..\resources\user\" + name + ".xaml";
            var xm = XamlWriter.Save(saveModels);
            using (var stream = File.Create(path))
            {
                StreamWriter sw = new StreamWriter(stream);
                sw.Write(xm);
                sw.Close();
            }
            Close();
        }
    }
}
