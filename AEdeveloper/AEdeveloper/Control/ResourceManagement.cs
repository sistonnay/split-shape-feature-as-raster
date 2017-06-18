using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AEdeveloper.Control
{
    public partial class ResourceManagement : UserControl
    {
        Mainform _mainFrm;
        public ResourceManagement(Mainform parent)
        {
            InitializeComponent();
            _mainFrm = parent;
        }
    }
}
