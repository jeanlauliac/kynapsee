using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kynapsee.ViewModels;

namespace Kynapsee.Views
{
    /// <summary>
    /// Interaction logic for TransitionsView.xaml
    /// </summary>
    public partial class TransitionsView : UserControl
    {
        public TransitionsView()
        {
            InitializeComponent();
        }

        private TransitionsViewModel model_;

        public void Bind(TransitionsViewModel model)
        {
            model_ = model;
            DataContext = model;
            
        }

        private void listBox1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            listBox1.SelectedItem = null;
        }

    }
}
