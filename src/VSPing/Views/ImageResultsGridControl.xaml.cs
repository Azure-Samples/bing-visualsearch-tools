﻿/*// REMOVE Visual Search Experimental View
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

namespace IUPing.Views
{
    /// <summary>
    /// Interaction logic for ImageResultsGridControl.xaml
    /// </summary>
    public partial class ImageResultsGridControl : UserControl
    {
        public ImageResultsGridControl()
        {
            InitializeComponent();
        }

        private void SearchResultImage_MouseMove(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            e.Handled = false;
            if (img != null && e.LeftButton == MouseButtonState.Pressed)
            {
                object dataContext = img.DataContext;
                DragDrop.DoDragDrop(
                             img,
                             dataContext.ToString(),
                             DragDropEffects.Copy);

                e.Handled = false;              
            }
        }
    }
}
//*/