using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AirScheduling.Aviation;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;

namespace AirScheduling
{
    internal class AirScheduling
    {
        private const string Airport = "1";
        private static List<Aircraft> _aircraftModels = new List<Aircraft>();
        private static Thread radar;

        private static Airport _currentAirport;
        
        public static void Main(string[] args)
        {
            read_configuration_files();

            while (_currentAirport.Radar.IsEmpty)
            {}
            
            var selection = new EliteSelection();
            var crossover = new OrderedCrossover();
            var mutation = new ReverseSequenceMutation();
            var fitness = new Genetics.Fitness();
            var chromosome = new Genetics.Chromosome(_currentAirport);
            var population = new Population (50, 70, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            ga.Termination = new GenerationNumberTermination(100);

            Console.WriteLine("GA running...");
            ga.Start();

            Console.WriteLine("Best solution found has {0} fitness.", ga.BestChromosome.Fitness);
            
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
                radar = new Thread(() => read_radar_thread("../../Data/Airport" + Airport + "/Radar.csv"));
                var allRunways = read_runway_information("../../Data/Airport" + Airport + "/Runways.csv");
                
                if (allRunways != null)
                    _currentAirport = new Airport(allRunways);
                
                var (runways, timeInterf) = read_runway_time_interference("../../Data/Airport" + Airport + "/Runway_times.csv");
                
                
                if (runways.Length != timeInterf.Count())
                    throw new Exception("Runway time interference matrix != Runway matrix");

                for (var i = 0; i < timeInterf.Count; i++)
                {
                    var pickedFirstRunway = timeInterf[i];
                    
                    for (var j = 0; j < pickedFirstRunway.Count; j++)
                    {
                        var pickingSecondRunway = pickedFirstRunway[j].Split('-');

                        for (var t = 0 ; t < pickingSecondRunway.Length; t++)
                        {
                            if (pickingSecondRunway[t].Replace(" ", string.Empty) == "x")
                                continue;
                                
                            for (var l = 0; l < pickingSecondRunway[t].Split(':').Length; l++)
                            {
                                _currentAirport.Runways[runways[i]].AddTimeDependecy(runways[j], 
                                    (ConvertIndexToTypeOfAircraft(t), ConvertIndexToTypeOfAircraft(l)), 
                                    int.Parse(pickingSecondRunway[t].Split(':')[l]));
                            }
                        }
                    }
                }
                
                radar.Start();
                
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
                
                    var aircraftSpecification = new Aircraft(type, model, minimumSpeed, optimalSpeed, maximumSpeed);
                    AirScheduling._aircraftModels.Add(aircraftSpecification);
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
                    var lines = File.ReadAllLines(fileUrl).Skip(1).ToArray();

                    foreach (var line in lines)
                    {
                        var splittedLine = line.Split(',');

                        var flighId = splittedLine[0];
                        var aircrafId = int.Parse(splittedLine[1]);
                        var urgency = bool.Parse(splittedLine[2]);
                        var timeNextFlight = double.Parse(splittedLine[3]);
                        var prec = splittedLine[4];

                        if (_currentAirport.Radar.ContainsKey(flighId))
                            continue;

                        var aicraftInRadar = new AircraftRadar(flighId, AirScheduling._aircraftModels[aircrafId - 1],
                            timeNextFlight, urgency);

                        _currentAirport.Radar.TryAdd(flighId, aicraftInRadar);
                    }
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
            
            for(var i = 0; i < lines.Length; i++)
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
        /// Convertes an index of a matrix to a specific type of aircraft
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static string ConvertIndexToTypeOfAircraft(int index)
        {
            if (index == 0)
                return "Heavy";
            else if (index == 1)
                return "Medium";
            else if (index == 2)
                return "Light";
            else
                throw new Exception();
        }
        
        
    }
}