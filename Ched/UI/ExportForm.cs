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
using HexaBeatChartEditer.Components.Exporter;
using HexaBeatChartEditer.Localization;

namespace HexaBeatChartEditer.UI
{
    public partial class ExportForm : Form
    {
        private readonly string ArgsKey = "chf";
        private readonly string Filter = "Hexabeat Chart file(*.chf)|*.chf";

        private SusExporter exporter = new SusExporter();

        public string OutputPath
        {
            get { return outputBox.Text; }
            set { outputBox.Text = value; }
        }

        public IExporter Exporter { get { return exporter; } }

        public ExportForm(ScoreBook book)
        {
            InitializeComponent();
            Icon = Properties.Resources.MainIcon;
            ShowInTaskbar = false;

            if (!book.ExporterArgs.ContainsKey(ArgsKey) || !(book.ExporterArgs[ArgsKey] is SusArgs))
            {
                book.ExporterArgs[ArgsKey] = new SusArgs();
            }

            var args = book.ExporterArgs[ArgsKey] as SusArgs;

            browseButton.Click += (s, e) =>
            {
                var dialog = new SaveFileDialog()
                {
                    Filter = Filter
                };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    outputBox.Text = dialog.FileName;
                }
            };

            exportButton.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(OutputPath)) browseButton.PerformClick();
                if (string.IsNullOrEmpty(OutputPath))
                {
                    MessageBox.Show(this, ErrorStrings.OutputPathRequired, Program.ApplicationName);
                    return;
                }

                try
                {
                    exporter.CustomArgs = args;
                    exporter.Export(OutputPath, book);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ErrorStrings.ExportFailed, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Program.DumpException(ex);
                }
            };
        }
    }
}
