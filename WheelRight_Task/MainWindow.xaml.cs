using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

namespace WheelRight_Task
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // A little simple click for generating a ID
        // Is used to nicely display information on  the data grid via WPF
        private static int id = 1;
        static int GenerateId()
        {
            return id++;
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



            // Set filter for file
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt";


            Nullable<bool> result = dlg.ShowDialog();


            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                searchBox.Text = filename;


                // Used to debug the sensor sensitivity
                int sensor_sensitivity;
                try
                {
                    // Range of 1 - 6
                    if (Int32.Parse(sensSensivity.Text) > 0 && Int32.Parse(sensSensivity.Text) <= 6)
                    {
                        sensor_sensitivity = Int32.Parse(sensSensivity.Text);
                    }
                    else
                    {
                        // Invalid
                        MessageBox.Show("Sensitivity range is 1 - 6, sensitivity has been set to 2 (default)", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                        sensor_sensitivity = 2;
                        sensSensivity.Text = "2";
                    }
                }
                catch (System.FormatException)
                {
                    // Error
                    MessageBox.Show("Single number only, sensitivity has been set to 2 (default)", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                    sensor_sensitivity = 2;
                    sensSensivity.Text = "2";
                }

                // Create object with all data given in parameters
                Sensor_Data dataSet = new Sensor_Data(Read_textData(filename), sensor_sensitivity);

                // Load in Ambient temperatures
                leftSensorAmbient.Text = dataSet.Left_Sensor_Ambient.ToString() + "°C";
                rightSensorAmbient.Text = dataSet.Right_Sensor_Ambient.ToString() + "°C";

                // Clear datagrid if new text file is loaded
                leftSensorGrid.Items.Clear();
                rightSensorGrid.Items.Clear();

                id = 1;
                // Load in type data, highest and average temperatures (Left sensor)
                for (int i = 0; i < dataSet.CarData.Left_Wheel_Logs.Count; i++)
                {
                    Wheel_Data current = dataSet.CarData.Left_Wheel_Logs[i];
                    current.Id = GenerateId();
                    leftSensorGrid.Items.Add(current);
                }
                id = 1;
                // Load in type data, highest and average temperatures (right sensor)
                for (int i = 0; i < dataSet.CarData.Right_Wheel_Logs.Count; i++)
                {
                    Wheel_Data current = dataSet.CarData.Right_Wheel_Logs[i];
                    current.Id = GenerateId();
                    rightSensorGrid.Items.Add(current);
                }
            }
        }
        /// <summary>
        /// Reads a text file and returns an array of strings
        /// </summary>
        /// <param name="file_directory"></param>
        /// <returns></returns>
        string[] Read_textData(string file_directory)
        {
            List<string> all_lines = new List<string>();
            bool skip_first_line = true;

            const Int32 BufferSize = 512;
            using (var fileStream = File.OpenRead(file_directory))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    // Uses a simple boolean to skip the first line
                    // This is assuming that the format is consistent
                    if (skip_first_line == false)
                        all_lines.Add(line);
                    else
                        skip_first_line = false;
                }
            }
            return all_lines.ToArray();
        }
    }
}
