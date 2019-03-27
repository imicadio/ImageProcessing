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
                Console.WriteLine("Wcisnij 1 aby zliczyć pixele pliku i wygenerować plik CSV. \nWciśnij 2 aby generować raport wystąpienia pixeli\n");
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
            Bitmap image = new Bitmap("E:\\Magisterka\\Popek\\cw1\\ConsoleApp2\\BAKU1.bmp");

            StringBuilder csvcontent = new StringBuilder();
            csvcontent.AppendLine("X;Y;R;G;B;A");

            // metoda do wykonywania pomiaru czasu
            Stopwatch timeStart = new Stopwatch();
            timeStart.Start();
            Console.WriteLine("\nCierpliwości program się wykonuje. \n");

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
                                csvcontent.AppendLine(String.Format("{0};{1};{2};{3};{4};{5}",
                                    i.ToString(), j.ToString(), pixelColor.R.ToString(), pixelColor.G.ToString(), pixelColor.B.ToString(), pixelColor.A.ToString()));
                            }
                        }
                    }
                });

            timeStart.Stop();

            Console.WriteLine("{0} wątków, czas czytania pliku: {1}", threadsNumber, timeStart.Elapsed);

            string csvpath = "E:\\Magisterka\\Popek\\cw1\\ConsoleApp2\\BAKU1.csv";
            File.AppendAllText(csvpath, csvcontent.ToString());

            Console.WriteLine("Wygenerowano plik CSV: " + csvpath + "\n");
        }

        public static void ReadCSV(int threadsNumber)
        {
            var st = File.ReadLines("E:\\Magisterka\\Popek\\cw1\\ConsoleApp2\\BAKU1.csv").Select(l => l).ToList();

            StringBuilder csvcontent = new StringBuilder();

            Dictionary<string, int> dataDictionary = new Dictionary<string, int>();

            Stopwatch timeStart = new Stopwatch();
            timeStart.Start();
            Console.WriteLine("Cierpliwości program się wykonuje. \n");

            Console.WriteLine("Plik posiada {0} linijek\n", st.Count);

            int linesJump = st.Count / threadsNumber;
            int moduloLines = st.Count % threadsNumber;
            int[] linesBreakpoints = new int[threadsNumber + 1];
            linesBreakpoints[0] = 0;

            for (int i = 2; i < threadsNumber+2; i++)
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
                        if (line != null)
                        {
                            lock (line)
                            {
                                var vec = line.Split(';');
                                string RGB = vec[2] + ";" + vec[3] + ";" + vec[4];
                                string dictionaryKey = dataDictionary.ContainsKey(RGB).ToString();

                                lock (dictionaryKey)
                                {
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
                        }
                    }
                });            

            StreamWriter sw = new StreamWriter(@"E:\\Magisterka\\Popek\\cw1\\ConsoleApp2\\RGB1.csv");

            string line1 = string.Empty;
            foreach (var entry in dataDictionary.OrderByDescending(key => key.Value))
            {
                line1 = entry.Key + ";" + entry.Value;                
                sw.WriteLine(line1);
            }

            timeStart.Stop();

            var sorted = dataDictionary.OrderByDescending(key => key.Value);

            Console.WriteLine("{0} wątków, czas czytania pliku: {1}\n", threadsNumber, timeStart.Elapsed);            

            Console.WriteLine("Najliczlniejsze parametry: ");
            Console.WriteLine("{0}: {1} razy\n", sorted.First().Key, sorted.First().Value);

            Console.WriteLine("Koniec Programu");

        }
    }
}
