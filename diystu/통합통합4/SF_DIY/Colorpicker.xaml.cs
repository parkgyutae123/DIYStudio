﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SF_DIY
{
    /// <summary>
    /// Colorpicker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Colorpicker : UserControl
    {
        public Colorpicker()
        {
            InitializeComponent();
        }


        public Brush SelectedColor
        {
            get { return (Brush)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Brush), typeof(Colorpicker), new UIPropertyMetadata(null));


    }
}
