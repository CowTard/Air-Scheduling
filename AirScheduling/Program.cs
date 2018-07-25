using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AirScheduling.Aviation;
using AirScheduling.Genetics;
using AirScheduling.Utils;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Terminations;

namespace AirScheduling
{
    internal class AirScheduling
    {
        private const string Airport = "2";
        private static List<Aircraft> _aircraftModels = new List<Aircraft>();
        private static Thread _radar;
        private static Random rnd;
        private static Airport _currentAirport;

        public static void Main(string[] args)
        {
            FileWriting.CreateTestFile("../../Data/Test.csv", "ID, Type, Minimum Time, Average Delay, Average Cost");

            rnd = new Random();

            read_configuration_files();
            if (_currentAirport.Radar.Count == 0)
            {
                _radar = new Thread(() => read_radar_thread("../../Data/Airport" + Airport + "/Radar.csv"));
                _radar.Start();
            }
                
            while (!_currentAirport.Ready)
            {}

            var selection = new Selection();
            var crossover = new Crossover(2, 2);
            var mutation = new Mutation();
            var fitness = new Fitness();
            var chromosome = new Chromosome(_currentAirport);
            var population = new Population(250, 500, chromosome);

            // FIFO
            var fifoChr = new Chromosome(_currentAirport, true);
            fitness.Evaluate(fifoChr);


            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                CrossoverProbability = 0.5f,
                MutationProbability = 0.7f,
                Termination = new TimeEvolvingTermination(TimeSpan.FromMinutes(0.1)),
                Reinsertion = new CustomReinsertion(),
            };

            var initialTimeSpan = DateTime.Now;
            Console.WriteLine("[{0}] Scheduling started", initialTimeSpan);

            Chromosome lastBest = null;
            ga.GenerationRan += (sender, e) =>
            {
                var bestChromosome = ga.BestChromosome as Chromosome;

                if (lastBest == null)
                    lastBest = bestChromosome;
            };


            ga.Start();
            Console.WriteLine("[{0}] New Round !", DateTime.Now);
                
                
            FileWriting.WriteToFile("../../Data/Test.csv", "FIFO, " + fifoChr.ToString());
            FileWriting.WriteToFile("../../Data/Test.csv", "--GA, " + ga.BestChromosome.ToString());
            ga.Stop();
            _radar.Interrupt();
            _currentAirport.Ready = false;
            _currentAirport.Radar.Clear();
            
        }

        /// <summary>
        /// Function responsible for handling the reading of configuration files as well as creating the varibles that
        /// will be created as files are read
        /// </summary>
        /// <returns>True if no errors were found in the files, false otherwise</returns>
        private static bool read_configuration_files()
        {
            try
            {
                var aircraftTypes = read_aircraft_database("../../Data/AircraftDatabase.csv");
                _radar = new Thread(() => read_radar_thread("../../Data/Airport" + Airport + "/Radar.csv"));
                var allRunways = read_runway_information("../../Data/Airport" + Airport + "/Runways.csv");
                var landingDistances = read_landing_distances("../../Data/Airport" + Airport + "/LandingRoutes.csv");

                if (allRunways != null)
                    _currentAirport = new Airport(allRunways, landingDistances);

                var (runways, timeInterf) =
                    read_runway_time_interference("../../Data/Airport" + Airport + "/Runway_landing_separation.csv");


                if (runways.Length != timeInterf.Count())
                    throw new Exception("Runway time interference matrix != Runway matrix");

                for (var i = 0; i < timeInterf.Count; i++)
                {
                    var pickedFirstRunway = timeInterf[i];

                    for (var j = 0; j < pickedFirstRunway.Count; j++)
                    {
                        var pickingSecondRunway = pickedFirstRunway[j].Split('-');

                        for (var t = 0; t < pickingSecondRunway.Length; t++)
                        {
                            for (var l = 0; l < pickingSecondRunway[t].Split(':').Length; l++)
                            {
                                _currentAirport.Runways[runways[i]].AddTimeDependecy(runways[j],
                                    (ConvertIndexToTypeOfAircraft(t), ConvertIndexToTypeOfAircraft(l)),
                                    int.Parse(pickingSecondRunway[t].Split(':')[l]));
                            }
                        }
                    }
                }


                _radar.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }


            return true;
        }

        /// <summary>
        /// Reads and populates database of aircrafts
        /// </summary>
        /// <param name="fileUrl">Location of AircraftDatabase.csv</param>
        /// <returns>True if no errors, false otherwise</returns>
        private static bool read_aircraft_database(string fileUrl)
        {
            try
            {
                var lines = File.ReadAllLines(fileUrl).Skip(1).ToArray();

                foreach (var line in lines)
                {
                    var splittedLine = line.Split(',');

                    var model = splittedLine[1];
                    var type = Aircraft.convert_string_into_AicraftType(splittedLine[2]);
                    var minimumSpeed = double.Parse(splittedLine[3]);
                    var optimalSpeed = double.Parse(splittedLine[4]);
                    var maximumSpeed = double.Parse(splittedLine[5]);
                    var numberOfPassengers = double.Parse(splittedLine[6]);

                    var aircraftSpecification = new Aircraft(type, model, minimumSpeed, optimalSpeed, maximumSpeed, numberOfPassengers);
                    _aircraftModels.Add(aircraftSpecification);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Function running in background in order to add aircrafts that are present in airport's airspace
        /// </summary>
        /// <param name="fileUrl">Url of the file that simulates radar - Radar.csv</param>
        /// <returns>Void</returns>
        private static void read_radar_thread(string fileUrl)
        {
            while (true)
            {
                try
                {
                    //_currentAirport.Radar.Clear();
                
                    var lines = File.ReadAllLines(fileUrl).Skip(1).ToArray();

                    foreach (var line in lines)
                    {
                        var splittedLine = line.Split(',');

                        var flighId = splittedLine[0];
                        var distanceToAirport = splittedLine[1];
                        var aircrafId = int.Parse(splittedLine[2]);
                        var urgency = bool.Parse(splittedLine[3]);
                        var timeNextFlight = double.Parse(splittedLine[4]);
                        var time = TimeSpan.FromMinutes(double.Parse(splittedLine[5]));

                        if (_currentAirport.Radar.ContainsKey(flighId))
                            continue;

                        var aicraftInRadar = new AircraftRadar(flighId, distanceToAirport,
                            _aircraftModels[aircrafId - 1],
                            timeNextFlight, urgency, time);

                        _currentAirport.Radar.TryAdd(flighId, aicraftInRadar);
                    }

                    _currentAirport.Ready = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Function responsible for parsing the files Runways.csv present in the folder of the active airport
        /// </summary>
        /// <param name="fileUrl">Url for the Runways.csv file</param>
        /// <returns>True if there are no errors, false otherwise</returns>
        private static Dictionary<string, Airport.Runway> read_runway_information(string fileUrl)
        {
            var allRunways = new Dictionary<string, Airport.Runway>();
            try
            {
                var lines = File.ReadAllLines(fileUrl).Skip(1).ToArray();

                foreach (var line in lines)
                {
                    var splittedLine = line.Split(',');

                    var identification = splittedLine[0];
                    var permissions = splittedLine[1];

                    allRunways.Add(identification, new Airport.Runway(identification, permissions));
                }

                return allRunways;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Function responsible for reading and handling the file that contains interferences times between runways
        /// </summary>
        /// <param name="fileUrl">Url for the file to be read</param>
        /// <returns>A list of list of string</returns>
        private static (string[], List<List<string>>) read_runway_time_interference(string fileUrl)
        {
            var interferences = new List<List<string>>();

            var lines = File.ReadAllLines(fileUrl).ToArray();
            string[] runways = null;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (i == 0)
                    runways = line.Replace(" ", string.Empty).Split(',');
                else
                {
                    var runwayInterference =
                        line.Replace("[", string.Empty).Replace("]", string.Empty).Split(',').ToList();

                    interferences.Add(runwayInterference);
                }
            }

            return (runways, interferences);
        }

        /// <summary>
        /// Function responsible for reading and handling the file that contains landing approach's landings
        /// </summary>
        /// <param name="fileUrl">Url for the file to be read</param>
        /// <returns>Dictionary of runwayID, length</returns>
        private static Dictionary<string, double> read_landing_distances(string fileUrl)
        {
            var lines = File.ReadAllLines(fileUrl).Skip(1).ToArray();

            return lines.Select(line => line.Split(','))
                .ToDictionary(info => info[0], info => double.Parse(info[1].Trim()));
        }

        /// <summary>
        /// Convertes an index of a matrix to a specific type of aircraft
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static string ConvertIndexToTypeOfAircraft(int index)
        {
            switch (index)
            {
                case 0:
                    return "Heavy";
                case 1:
                    return "Medium";
                case 2:
                    return "Light";
            }

            throw new Exception();
        }
    }
}