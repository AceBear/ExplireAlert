using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ExpireAlert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FlashableTrayWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var miCheck = new MenuItem(){ Header = "检查许可证(_R)" };
            miCheck.Command = ApplicationCommands.Open;
            miCheck.CommandTarget = this;
            base.m_tray.ContextMenu.Items.Insert(0, miCheck);

            var miClear = new MenuItem() {Header = "清空(_C)" };
            miClear.Command = ApplicationCommands.Close;
            miClear.CommandTarget = this;
            base.m_tray.ContextMenu.Items.Insert(1, miClear);
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = this.DataContext as MainVM;
            if(vm != null)
                vm.Check(null);
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = this.DataContext as MainVM;
            if (vm != null)
            {
                vm.Clear();
            }
        }
    }
}
