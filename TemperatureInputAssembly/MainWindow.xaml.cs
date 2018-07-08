using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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

namespace TemperatureInput
{


    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Module _graphModule;
        Window _graphWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(Module graphAssembly,Window graphWindow)
        {

            InitializeComponent();
            _graphModule = graphAssembly;
            _graphWindow = graphWindow;
          

        }
        //отправка записи в другое окно
        private void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            _graphModule.GetType("OutputDiagram.MainWindow").GetMethod("AddNewRecordFromInput").Invoke(_graphWindow, new object[] { DateValue.SelectedDate,Convert.ToDouble(temperatureValue.Text)});
          
        }

        //эвенты окна
        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void TrayBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void OnCenterOfScreen()
        {
            double screenHeight = SystemParameters.FullPrimaryScreenHeight;
            double screenWidth = SystemParameters.FullPrimaryScreenWidth;
            this.Top = (screenHeight - this.Height) / 0x00000002;
            this.Left = (screenWidth - this.Width) / 0x00000002;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OnCenterOfScreen();
            Left = Left+400;
        }

    }
}
