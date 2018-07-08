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

namespace MultithreadingHomeWork
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static object locker = new object();
        static AppDomain _temperatureInputDomain;
        static Window _temperatureInputWindow;
        static Assembly _temperatureInputAssembly;

        static AppDomain _graphDomain;
        static Assembly _graphAssembly;
        static Window _graphWindow;

        Thread inputAppTgread;


        public MainWindow()
        {
            InitializeComponent();
            this.Hide();
             _temperatureInputDomain = AppDomain.CreateDomain("TemperatureInput");
            _graphDomain = AppDomain.CreateDomain("Graph");

            _temperatureInputAssembly = _temperatureInputDomain.Load(AssemblyName.GetAssemblyName("TemperatureInput.exe"));
            _graphAssembly = _graphDomain.Load(AssemblyName.GetAssemblyName("OutputDiagram.exe"));

            Thread thread = new Thread(() =>
            {
                lock (locker)
                {
                    _graphWindow = Activator.CreateInstance(_graphAssembly.GetType("OutputDiagram.MainWindow")) as Window;
                }
                _graphWindow.ShowDialog();
                App.Current.Shutdown();
            });
        

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            
            inputAppTgread = new Thread(() =>
            {
                lock(locker)
                {
                    _temperatureInputWindow = Activator.CreateInstance(_temperatureInputAssembly.GetType("TemperatureInput.MainWindow"), new object[] {
                    _graphAssembly.GetModule("OutputDiagram.exe"),
                    _graphWindow
                 }) as Window;
                }
                _temperatureInputWindow.ShowDialog();
                AppDomain.Unload(_temperatureInputDomain);
                inputAppTgread.Abort();

            });
            inputAppTgread.SetApartmentState(ApartmentState.STA);
            inputAppTgread.Start();
        }
    }
}
