using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using ScottPlot;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ISMPlot
{
    class Program
    {
        public const string VERSION = "1.0";

        public static void Main(string[] args)
        {
            Console.WriteLine($"433Plot {VERSION} by Cam K.");

            // Argument parsing
            string jsonFile = "";
            string varToPlot = "";
            string devID = "";
            if (args.Length <= 1 || args[0] == "-h")
            {
                ShowHelp();
                return;
            }
            jsonFile = args[0];
            varToPlot = args[1];
            if (args.Length > 2)
                devID = args[2];

            // Validate json file
            if (!File.Exists(jsonFile))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid or no json file provided!");
                Console.ResetColor();
                ShowHelp();
                return;
            }
            string[] json = File.ReadAllLines(jsonFile);

            // Parse json file
            List<double> yVals = new List<double>();
            string model = null;
            foreach (string line in json)
            {
                dynamic packet = JsonConvert.DeserializeObject(line);

                // Iterate over each property in the packet and set val to the var to plot
                string val = null;
                string tModel = null;
                foreach(JProperty property in packet) {
                    // If the ID doesn't match, continue
                    if(!string.IsNullOrWhiteSpace(devID))
                        if(property.Name == "id")
                            if(property.Value.ToString() != devID) {
                                val = null;
                                tModel = null;
                                continue;
                            }
                    if(property.Name == varToPlot)
                        val = property.Value.ToString();
                    if(property.Name == "model")
                        tModel = property.Value.ToString();
                }
                
                // Parse the property
                if(double.TryParse(val, out double yVal))
                    yVals.Add(yVal);
                if(tModel != null)
                    model = tModel;
            }

            // Graph setup
            double[] ys = yVals.ToArray();
            double[] xs = new double[ys.Length];
            for(int i = 0; i < xs.Length; i++)
                xs[i] = i;

            // Create graph
            Plot plt = new Plot(1920, 1080);
            plt.Style(System.Drawing.Color.FromArgb(52, 54, 60), System.Drawing.Color.FromArgb(52, 54, 60), null, System.Drawing.Color.White, System.Drawing.Color.White, System.Drawing.Color.White);
            plt.XLabel("Packet #", null, null, null, 25.5f, false);
            plt.YLabel(varToPlot, null, null, 25.5f, null, false);
            if(string.IsNullOrWhiteSpace(devID))
                plt.Title("433MHz device plot");
            else
                plt.Title($"{model} sensor data");
            plt.PlotFillAboveBelow(xs, ys, lineWidth: 4, lineColor: System.Drawing.Color.FromArgb(100, 119, 183), fillAlpha: .5, fillColorBelow: System.Drawing.Color.FromArgb(100, 119, 183), fillColorAbove: System.Drawing.Color.FromArgb(100, 119, 183));
            plt.Layout(xScaleHeight: 128);
            plt.SaveFig("433.png");

            Console.WriteLine("Graph saved to 433.png!");
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: 433plot [-h] json valToPlot device-id");
            Console.WriteLine(" -h      Show help");
        }
    }
}