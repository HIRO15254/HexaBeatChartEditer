using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HexaBeatChartEditer.Core;

namespace HexaBeatChartEditer.UI
{
    public partial class BookPropertiesForm : Form
    {
        public SoundSource MusicSource { get { return musicSourceSelector.Value; } }

        public BookPropertiesForm(ScoreBook book, SoundSource musicSource)
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            if (musicSource != null) musicSourceSelector.Value = musicSource;
        }

        private void musicSourceSelector_Load(object sender, EventArgs e)
        {

        }
    }
}
