using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace ACC_Auto_Fuel
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            GetCurrentData();
        }

        private void GetData(object sender, RoutedEventArgs e)
        {
            GetCurrentData();
        }

        private void GetCurrentData()
        {
            //C:\Users\%username%\AppData\Local\AC2\Saved\Logs
            string log = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\AC2\Saved\Logs\AC2.log";
            //Console.WriteLine(log);

            if (File.Exists(log)) { //TODO falta el tiempo de la sesión !!!!! ---------------------------
                statusReload.Content = "";

                string temp = @"AC2.tmp";
                File.Copy(log, temp, true);
                // Read the file and display it line by line.  
                System.IO.StreamReader file =
                    new System.IO.StreamReader(temp);
                string line;
                string session = "";
                string track = "";
                string fuelperlap = "";
                float fuelLap;
                string fuelxminute = "";
                float fuelMinute;
                string car = "";
                //string bestlap = "35791:23:647";
                int secLap = 2147483;
                string currentTime = "";
                //int counter = 0;
                while ((line = file.ReadLine()) != null)
                {
                    //Console.WriteLine(line);
                    //counter++;
                    if (line.Contains("Session ") && !line.Contains("---"))
                        session = GetStringAfter(line, "Session ");
                    if (line.Contains("Track: "))
                        track = GetStringAfter(line, "Track: ");
                    if (line.Contains("FuelPerLaps "))
                    {
                        fuelperlap = GetStringAfter(line, "FuelPerLaps "); //get
                        fuelMinute = float.Parse(fuelperlap.Replace(".", ",")); //float
                        fuelperlap = fuelMinute.ToString("0.00"); //string 2 decimal
                    }                        
                    if (line.Contains("Fuel x Minute "))
                    {
                        fuelxminute = GetStringAfter(line, "Fuel x Minute "); //get
                        fuelLap = float.Parse(fuelxminute.Replace(".", ",")); //float
                        fuelxminute = fuelLap.ToString("0.00"); //string 2 decimal
                    }
                    if (line.Contains("- car: "))
                        car = Regex.Match(line, @"- car: (.+?) -").Groups[1].Value.Replace("_", " ").ToUpper();
                    if (line.Contains("BESTLAP: "))
                    {
                        //string[] splitOld = bestlap.Split(':');
                        //int segOld = secLap;

                        var lap = Regex.Match(line, @"BESTLAP: (.+?) ").Groups[1].Value;

                        string[] splitNew = lap.Split(':');
                        int segNew = 2147483;
                        try
                        {
                            //segOld = Int32.Parse(splitOld[0])*60 + Int32.Parse(splitOld[1]);
                            segNew = Int32.Parse(splitNew[0])*60 + Int32.Parse(splitNew[1]);
                        }
                        catch (FormatException error)
                        {
                            //Console.WriteLine(error.Message);
                        }
                        if (segNew < secLap) secLap = segNew;
                    }
                    if (line.Contains("["+DateTime.Now.Year.ToString()))
                    {
                        string[] logTime = GetStringAfter(line, "[").Split(']')[0].Split('-');
                        currentTime = logTime[0].Replace(".","/") + " " + logTime[1].Split(':')[0].Replace(".", ":");
                    }
                    //Connecting to server 
                    //Setting ambient temp:
                }

                file.Close();
                File.Delete(temp);
                //Console.WriteLine("There were {0} lines.", counter);
                TimeSpan time = TimeSpan.FromSeconds(secLap);
                string bestlap = time.ToString(@"mm\:ss"); //\:fff

                // write data
                dataCar.Text = car.Replace("_", " ").ToUpper();
                dataTrack.Text = track.Replace("_", " ").ToUpper();
                dataSession.Text = session.Replace("_", " ").ToUpper();
                dataBestlap.Text = bestlap;
                dataFuelperlap.Text = fuelperlap;
                dataFuelxminute.Text = fuelxminute;

                statusReload.Content = currentTime;

                // Suspend the screen.  
                System.Console.ReadLine();

                // Set current data to Fuel Grid
                //SetCurrentData();
                //length.Text = "15";
                Keyboard.Focus(length);

                minutes.Text = time.ToString(@"mm");
                seconds.Text = time.ToString(@"ss");
                fuel.Text = fuelperlap;

                //total.Content = "45";

            } else
            {
                statusReload.Content = "(No current data)";
            }
        }

        private String GetStringAfter(String line, String search)
        {
            return line.Substring(line.IndexOf(search) + search.Length);
        }
        
        private void SetFuel()
        {
            try
            {
                var templ = int.Parse(length.Text);
                var tempm = int.Parse(minutes.Text);
                var temps = int.Parse(seconds.Text);
                var tempf = double.Parse(fuel.Text);
                var tempspare = int.Parse("0");
                if (extra.Text != "")
                    tempspare = int.Parse(extra.Text);

                //Console.WriteLine(tempspare);

                var tempvueltas = templ * 60 / (tempm * 60 + temps) + tempspare;
                var tempfuel = (int)Math.Ceiling(tempvueltas * tempf);

                laps.Content = tempvueltas;

                total.Content = tempfuel;
            } catch
            {
                total.Content = "";
            }
            
        }
        
        private void textChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            TextBox textBox = (TextBox)sender;
            var valor = "";
            //Console.WriteLine(textBox.Name.ToString());
            if (textBox.Name.ToString().Equals("fuel"))
            {
                textBox.MaxLength = 4;
                string temp = Regex.Replace(textBox.ToString(), @"[^\d{1,2}]", String.Empty); //sólo números
                var index = temp.IndexOf(',', temp.IndexOf(',') + 1);
                if (index > 0) temp = temp.Remove(index);
                valor = temp.ToString();
            } else
            {
                textBox.MaxLength = 3;
                valor = Regex.Replace(textBox.ToString(), @"[^\d]", String.Empty); //sólo números
            }
            textBox.Text = valor;
            textBox.SelectionStart = textBox.Text.Length; //poner el cursor al final

            if (length.Text.Length > 0 && minutes.Text.Length > 0 && seconds.Text.Length > 0 && fuel.Text.Length > 0)
                SetFuel();
        }
    }
}
