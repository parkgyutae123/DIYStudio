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
    /// MaterialAddWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MaterialAddWindow : Window
    {
        
        public MaterialAddWindow()
        {
            InitializeComponent();
            MateriaAddWindowViewModel addViewModel = new MateriaAddWindowViewModel();
            DataContext = addViewModel;
            MateriaAddWindowViewModel.Close += () =>
            {
                this.Close();
            };
        }

        private void MateriaAddWindowViewModel_Test_Action()
        {
            this.Close();
        }

        private void CancelButtonOnClick(object sender, RoutedEventArgs e)
        {
            //Task.Delay(TimeSpan.FromSeconds(3))
            //   .ContinueWith((t, _) => Close(), null,
            //       TaskScheduler.FromCurrentSynchronizationContext());
            Close();
        }
        
        
    }
}
