using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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
using System.Xml.Serialization;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.Win32;

namespace OutputDiagram
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }
        static object locker = new object();

        private ChartValues<DateTimePoint> _temperatureLineSeriesValues { get; set; }

        private Assembly _inputAsssembly;
        private AppDomain _inputDomain;
        private Window _inputWindow;

        public void InitializeWindow()
        {
            InitializeComponent();
            _temperatureLineSeriesValues = new ChartValues<DateTimePoint>();
            XFormatter = val => new DateTime((long)val).ToString("yyyy-MM-dd");
            YFormatter = val => "   " + val.ToString("N") + " ℃ ";

            tempL.LabelFormatter = YFormatter;
            DateL.LabelFormatter = XFormatter;
          
            RefreshChart();


        }

        public MainWindow()
        {
            InitializeWindow();
        }

        //назначение внешнего источника
        public void SetDataInput(AppDomain tiDomain, Assembly tiAssembly)
        {
            _inputDomain = tiDomain;
            _inputAsssembly = tiAssembly;
            InitializeWindow();
        }
        //добафить новую запись в график
        public void AddNewRecordFromInput(DateTime date,double value)
        {         
            mainChart.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                DateTimePoint newTimePoint =    new DateTimePoint(date, value);
                try
                {
                    DateTimePoint exisitingTimePoint = _temperatureLineSeriesValues.First(d => d.DateTime == newTimePoint.DateTime);
                    if (exisitingTimePoint != null)
                    {
                        _temperatureLineSeriesValues.Remove(exisitingTimePoint);                    
                    }
                }
                catch (Exception)
                {
                 
                }               
                _temperatureLineSeriesValues.Add(newTimePoint);

                RefreshChart();
            }));     
        }
        //перерисовка Графика
        public void RefreshChart()
        {
            SeriesCollection printGraphCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Temperature",
                        Values = new ChartValues<DateTimePoint>(_temperatureLineSeriesValues.OrderBy((t) => t.DateTime)),
                    }
                };
            mainChart.Series = printGraphCollection;
        }
        //сохранить график
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "Text Files(*.xml)|*.xml|All(*.*)|*"
            };
            if (dialog.ShowDialog() == true)
            {         
                try
                {
                    ChartValues<DateTimePoint> tmp = new ChartValues<DateTimePoint>(_temperatureLineSeriesValues);
                    XmlSerializer formatter = new XmlSerializer(typeof(ChartValues<DateTimePoint>));

                    using (FileStream fs = new FileStream(dialog.FileName, FileMode.OpenOrCreate))
                    {
                        formatter.Serialize(fs, tmp);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        //прочитать график
        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Text Files(*.xml)|*.xml|All(*.*)|*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(ChartValues<DateTimePoint>));
                    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.OpenOrCreate))
                    {
                        _temperatureLineSeriesValues = new ChartValues<DateTimePoint>((ChartValues<DateTimePoint>)formatter.Deserialize(fs));
                    }
                    RefreshChart();
                }   
                  catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
        }
       
        //запуск приложения источника
        private void SetInputBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Text Files(*.exe)|*.exe|All(*.*)|*"
            };

            if (openFileDialog.ShowDialog() == true)
            {

                _inputDomain = AppDomain.CreateDomain("TemperatureInput");
                _inputAsssembly = _inputDomain.Load(AssemblyName.GetAssemblyName("TemperatureInput.exe"));
                try
                {
                       Thread thread = new Thread(() =>
                        {
                        
                            lock (locker)
                            {
                              _inputWindow = Activator.CreateInstance(_inputAsssembly.GetType("TemperatureInput.MainWindow"), new object[] {
                               Assembly.GetExecutingAssembly().GetModule("OutputDiagram.exe"),
                                  this
                              }) as Window;
                          }
                         _inputWindow.ShowDialog();
                      });
                     thread.SetApartmentState(ApartmentState.STA);
                     thread.Start();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        //эвенты окна
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "F")
            {
                if (WindowState == WindowState.Normal)
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                }
            }
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
            Left = Left - 200;
        }
    }
}
