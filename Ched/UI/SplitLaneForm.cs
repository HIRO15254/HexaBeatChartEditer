using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HexaBeatChartEditer.UI
{
    public partial class SplitLaneForm : Form
    {

        public bool[] Lane
        {
            get {
                bool[] vs = new bool[6];
                vs[0] = lane1_2.Checked;
                vs[1] = lane2_2.Checked;
                vs[2] = lane3_2.Checked;
                vs[3] = lane4_2.Checked;
                vs[4] = lane5_2.Checked;
                vs[5] = lane6_2.Checked;
                return vs;
            }
            set
            {
                lane1_2.Checked = value[0];
                lane2_2.Checked = value[1];
                lane3_2.Checked = value[2];
                lane4_2.Checked = value[3];
                lane5_2.Checked = value[4];
                lane6_2.Checked = value[5];
            }
        }

        public SplitLaneForm()
        {
            InitializeComponent();
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;
        }
    }
}
