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
using GalaSoft.MvvmLight.Messaging;

namespace AxView.View
{
    /// <summary>
    /// Logique d'interaction pour MazeCircuitMenu.xaml
    /// </summary>
    public partial class MazeCircuitMenu : UserControl
    {
        public MazeCircuitMenu()
        {
            InitializeComponent();
            Messenger.Default.Register<bool>(this, "RefreshPlot", this.RefreshPlot);
        }

        private void RefreshPlot(bool status)
        {
            this.PlotError.InvalidatePlot(true);
            this.PlotSpeed.InvalidatePlot(true);
        }
    }
}
