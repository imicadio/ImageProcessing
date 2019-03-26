using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            int number = 11;
            Dictionary<string, int> dataDictionary = new Dictionary<string, int>();
            while (number != 0){
                Console.WriteLine("Wcisnij 1 aby zliczyć pixele pliku i wygenerować plik CSV. \nWciśnij 2 aby generować raport wystąpienia pixeli");
                int caseSwitch = Convert.ToInt16(Console.ReadLine());
                switch (caseSwitch)
                {
                    case 1:
                        Console.WriteLine("Musisz podać liczbę wątków");
                        int threadsNumber = Convert.ToInt16(Console.ReadLine());
                        ReadPixelToCSV(threadsNumber);
                        break;
                    case 2:
                        Console.WriteLine("Musisz podać liczbę wątków");
                        int threadsNumber1 = Convert.ToInt16(Console.ReadLine());
                        ReadCSV(threadsNumber1);
                        break;
                    default:
                        Console.WriteLine("Koniec programu. Wciśnij dowolny klawisz aby zakończyć.");
                        break;
                }
                number = caseSwitch;
            }
            
            Console.ReadKey();
        }

        public static void ReadPixelToCSV(int threadsNumber)
        {
            Bitmap image = new Bitmap("E:\\Magisterka\\Popek\\cw1\\ConsoleApp2\\kobyla.jpg");

            StringBuilder csvcontent = new StringBuilder();
            csvcontent.AppendLine("X;Y;R;G;B;A");

            // metoda do wykonywania pomiaru czasu
            Stopwatch timeStart = new Stopwatch();
            timeStart.Start();
            Console.WriteLine("Cierpliwości program się wykonuje. \n");

            int widthJump = image.Width / threadsNumber;
            int heightJump = image.Height / threadsNumber;

            int moduloWidth = image.Width % threadsNumber;
            int moduloHeight = image.Height % threadsNumber;

            int[] widthBreakPoints = new int[threadsNumber + 1];
            int[] heightBreakPoints = new int[threadsNumber + 1];
            widthBreakPoints[0] = 0;
            heightBreakPoints[0] = 0;

            for (int i = 2; i < threadsNumber + 2; i++)
            {
                widthBreakPoints[i - 1] = widthJump * (i - 1);
                heightBreakPoints[i - 1] = heightJump * (i - 1);
            }

            if (moduloWidth != 0)
            {
                widthBreakPoints[threadsNumber] = widthBreakPoints[threadsNumber] + moduloWidth;
            }

            if (moduloHeight != 0)
            {
                heightBreakPoints[threadsNumber] = heightBreakPoints[threadsNumber] + moduloHeight;
            }

            Dictionary<string, int> dataDictionary = new Dictionary<string, int>();

            Parallel.For(0, threadsNumber,
                index =>
                {

                    int widthStartIndex = widthBreakPoints[index];
                    int heightStartIndex = heightBreakPoints[index];
                    int widthLimit = widthBreakPoints[index + 1];
                    int heightLimit = heightBreakPoints[index + 1];

                    for (int i = widthStartIndex; i < widthLimit; i++)
                    {
                        for (int j = heightStartIndex; j < heightLimit; j++)
                        {
                            Color pixelColor;

                            //zablokuje przepływ kodu dla innych wątków, aż do jego zwolnienia.
                            lock (image)
                            {
                                pixelColor = image.GetPixel(i, j);
                            }
                            csvcontent.AppendLine(i + ";" + j + ";" + pixelColor.R + ";" + pixelColor.G + ";" + pixelColor.B + ";" + pixelColor.A);

                            // moje
                            //string color = $"{pixelColor.R.ToString()}; {pixelColor.G.ToString()}; {pixelColor.B.ToString()}";

                            //if (dataDictionary.ContainsKey(color))
                            //{
                            //    dataDictionary[color]++;
                            //}
                            //else
                            //{
                            //    dataDictionary[color] = 1;
                            //}

                            // Adrian
                            //string color = $"{pixel.R.ToString()}, {pixel.G.ToString()}, {pixel.B.ToString()}";
                            //if (shades.ContainsKey(color))
                            //{
                            //    shades[color]++;
                            //}
                            //else
                            //{
                            //    shades[color] = 1;
                            //}
                        }
                    }
                });

            timeStart.Stop();

            Console.WriteLine("{0} wątków, czas czytania pliku: {1}", threadsNumber, timeStart.Elapsed);

            string csvpath = "E:\\Magisterka\\Popek\\cw1\\ConsoleApp2\\kobyla.csv";
            File.AppendAllText(csvpath, csvcontent.ToString());

            Console.WriteLine("Wygenerowano plik CSV: " + csvpath + "\n");
        }

        public static void ReadCSV(int threadsNumber)
        {
            var st = File.ReadAllLines("E:\\Magisterka\\Popek\\cw1\\ConsoleApp2\\kobyla.csv");

            StringBuilder csvcontent = new StringBuilder();

            Dictionary<string, int> dataDictionary = new Dictionary<string, int>();

            Stopwatch timeStart = new Stopwatch();
            Console.WriteLine("Cierpliwości program się wykonuje. \n");

            int lines = File.ReadAllLines("E:\\Magisterka\\Popek\\cw1\\ConsoleApp2\\kobyla.csv").Length;

            Console.WriteLine(lines);

            int linesJump = lines / threadsNumber;
            int moduloLines = lines % threadsNumber;
            int[] linesBreakpoints = new int[threadsNumber + 1];
            linesBreakpoints[0] = 0;

            for (int i = 2; i < threadsNumber; i++)
            {
                linesBreakpoints[i - 1] = linesJump * (i - 1);
            }

            if (moduloLines != 0)
            {
                linesBreakpoints[threadsNumber] = linesBreakpoints[threadsNumber] + moduloLines;
            }

            Parallel.For(0, threadsNumber,
                index =>
                {
                    int linestStartIndex = linesBreakpoints[index];
                    int linesLimit = linesBreakpoints[index + 1];

                    for (int i = linestStartIndex; i < linesLimit; i++)
                    {                       
                        var line = st[i];
                        lock (line)
                        {
                            var vec = line.Split(';');
                            string RGB = vec[2] + ";" + vec[3] + ";" + vec[4];
                            string dictionaryKey = dataDictionary.ContainsKey(RGB).ToString();

                            if (dictionaryKey == "False")
                            {
                                dataDictionary.Add(RGB, 1);
                            }
                            else
                            {
                                dataDictionary[RGB] += 1;
                            }
                        }                  
                    }
                });            

            StreamWriter sw = new StreamWriter(@"E:\\Magisterka\\Popek\\cw1\\ConsoleApp2\\RGB1.csv");

            string line1 = string.Empty;

            foreach (KeyValuePair<string, int> entry in dataDictionary)
            {
                line1 = entry.Key + ";" + entry.Value;
                sw.WriteLine(line1);
            }

            timeStart.Stop();

            Console.WriteLine("{0} wątków, czas czytania pliku: {1}", threadsNumber, timeStart.Elapsed);

            Console.WriteLine("Koniec Programu");

        }
    }
}
