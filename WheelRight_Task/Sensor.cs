using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WheelRight_Task
{
    /// <summary>
    /// Contain sets of tyre data
    /// </summary>
    public class Car_Data
    {
        public List<Wheel_Data> Right_Wheel_Logs { get; set; } = new List<Wheel_Data>();
        public List<Wheel_Data> Left_Wheel_Logs { get; set; } = new List<Wheel_Data>();

    }
    /// <summary>
    /// All data on individual tyres
    /// </summary>
    public class Wheel_Data
    {
        // This is used in datagrid(WPF) to display each record in the correct order
        public int Id { get; set; }

        // Contains all values recorded of the tyre temperature
        public int[] Tyre_Detect { get; set; }

        public int Sample_Size { get; set; }

        public string Average_Temp { get; set; }
        public string Highest_Temp { get; set; }

        public Wheel_Data(int[] wheel_detection, int highest_temp)
        {
            Tyre_Detect = wheel_detection;
            Highest_Temp = highest_temp.ToString() + "°C";
            Sample_Size = wheel_detection.Count();

            // Calculate average temperature
            int sum = 0;
            foreach (int x in Tyre_Detect)
            {
                sum += x;
            }
            Average_Temp = (sum / Tyre_Detect.Length).ToString() + "°C";
        }

    }
    /// <summary>
    /// Contains logic for sensor data
    /// </summary>
    public class Sensor_Data
    {
        // Parameters
        public int Sensor_Sensitivity { get; set; }
        public int Spike_Tolerance = 3;

        // Left Sensor data
        public int[] Left_Sensor_Data { get; set; }
        public int Left_Sensor_Ambient { get; set; }

        // Right Sensor data
        public int[] Right_Sensor_Data { get; set; }
        public int Right_Sensor_Ambient { get; set; }

        // Contains left and right tyre readings
        public Car_Data CarData { get; set; } = new Car_Data();

        public Sensor_Data(string[] raw_list, int sensor_sensitivity)
        {
            Sensor_Sensitivity = sensor_sensitivity;

            // Split on these chars
            char[] delimiterChars = { ' ', '\t' };

            // Set array size
            Right_Sensor_Data = new int[raw_list.Length];
            Left_Sensor_Data = new int[raw_list.Length];

            // With the raw list given, find right and left sensor numbers
            for (int i = 0; i < raw_list.Length; i++)
            {
                string[] split = raw_list[i].Split(delimiterChars);
                try
                {
                    // I used 2 single arrays instead of a 2D array for the sake of clarity in further calucations
                    Left_Sensor_Data[i] = Int32.Parse(split[0].Trim());
                    Right_Sensor_Data[i] = Int32.Parse(split[1].Trim());
                }
                catch (System.FormatException)
                {
                    // Invalid text file was selected
                    MessageBox.Show("Invalid Text file", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }
            }
            // Used void methods for the sake of clarity and better work flow
            Calculate_Ambient_Temperature();
            Tyre_Detection();
        }
        /// <summary>
        /// Searches the data for tyre readings and also removes spikes in data
        /// </summary>
        void Tyre_Detection()
        {
            int highest_temp = -999;
            List<int> wheel_detection = new List<int>();

            // Right Wheel
            for (int i = 0; i < Right_Sensor_Data.Length; i++)
            {
                // Knowing the ambient temperature, and setting up a sensitivity value
                // loop through data to find (relatively) dramatic increases in temperature
                if (Right_Sensor_Data[i] > (Right_Sensor_Ambient + Sensor_Sensitivity))
                {
                    // A value that supasses (ambient temperature + sensitivity), Now each Subsequent value is recorded
                    // Until it drops down below the threshold, which implies the tyre has passed
                    for (int x = i; x < Right_Sensor_Data.Length; x++)
                    {
                        // This is here to see if it drops below the threshold
                        if (Right_Sensor_Data[x] > (Right_Sensor_Ambient + Sensor_Sensitivity))
                        {
                            // Add to collection of tyre readings
                            wheel_detection.Add(Right_Sensor_Data[x]);

                            // Since we are looping through the tyre readings, we might aswell look for the highest value
                            if (Right_Sensor_Data[x] > highest_temp)
                            {
                                highest_temp = Right_Sensor_Data[x];
                            }
                        }
                        else
                        {
                            // Stopped recording, this statement checks if it could of been a spike/glitch
                            // If the sample size was too small it wont create a wheel data 
                            // This could be improved/more advanced
                            if (wheel_detection.Count > Spike_Tolerance)
                            {
                                Wheel_Data rightwheel = new Wheel_Data(wheel_detection.ToArray(), highest_temp);
                                CarData.Right_Wheel_Logs.Add(rightwheel);
                            }

                            // Step out and continue where we left off
                            i = x;
                            // Reset
                            highest_temp = -999;
                            wheel_detection.Clear();
                            break;
                        }
                    }
                }


            }

            // Left Wheel
            for (int i = 0; i < Left_Sensor_Data.Length; i++)
            {
                if (Left_Sensor_Data[i] > (Left_Sensor_Ambient + Sensor_Sensitivity))
                {
                    for (int x = i; x < Left_Sensor_Data.Length; x++)
                    {
                        if (Left_Sensor_Data[x] > (Left_Sensor_Ambient + Sensor_Sensitivity))
                        {
                            wheel_detection.Add(Left_Sensor_Data[x]);

                            if (Left_Sensor_Data[x] > highest_temp)
                            {
                                highest_temp = Left_Sensor_Data[x];
                            }
                        }
                        else
                        {
                            if (wheel_detection.Count > Spike_Tolerance)
                            {
                                Wheel_Data leftwheel = new Wheel_Data(wheel_detection.ToArray(), highest_temp);
                                CarData.Left_Wheel_Logs.Add(leftwheel);
                            }

                            i = x;
                            highest_temp = -999;
                            wheel_detection.Clear();
                            break;
                        }
                    }
                }


            }
        }
        /// <summary>
        /// Finds the Ambient Temperature
        /// </summary>
        void Calculate_Ambient_Temperature()
        {
            // Right sensor grouped data to show frequency of numbers
            var right_Sensor_MetaData = from numbers in Right_Sensor_Data
                                        group numbers by numbers into grouped
                                        select new { Number = grouped.Key, Freq = grouped.Count() };

            // Left sensor grouped data to show frequency of numbers
            var left_Sensor_MetaData = from numbers in Left_Sensor_Data
                                       group numbers by numbers into grouped
                                       select new { Number = grouped.Key, Freq = grouped.Count() };

            int frequency = 0;
            int index_of_highest = 0;
            int current_frequency = 0;

            for (int q = 0; q < right_Sensor_MetaData.Count(); q++)
            {
                // Loop through frequency values on the right sensor to find the number with the highest frequency
                current_frequency = Int32.Parse(right_Sensor_MetaData.ElementAt(q).Freq.ToString());
                if (frequency < current_frequency)
                {
                    frequency = current_frequency;
                    index_of_highest = q;
                }
            }

            // Set ambient temperature of right sensor
            Right_Sensor_Ambient = Int32.Parse(right_Sensor_MetaData.ElementAt(index_of_highest).Number.ToString());


            frequency = 0;
            index_of_highest = 0;
            current_frequency = 0;

            for (int q = 0; q < left_Sensor_MetaData.Count(); q++)
            {
                // Loop through frequency values on the left sensor to find the number with the highest frequency
                current_frequency = Int32.Parse(left_Sensor_MetaData.ElementAt(q).Freq.ToString());
                if (frequency < current_frequency)
                {
                    frequency = current_frequency;
                    index_of_highest = q;
                }
            }

            // Set ambient temperature of left sensor
            Left_Sensor_Ambient = Int32.Parse(left_Sensor_MetaData.ElementAt(index_of_highest).Number.ToString());
        }

    }
}
