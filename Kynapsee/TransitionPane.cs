using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kynapsee.ViewModels;

namespace Kynapsee
{
    public partial class TransitionPane : UserControl
    {
        public TransitionPane(TransitionsViewModel model)
        {
            InitializeComponent();
            transitionsView.Bind(model);
        }

    }
}
