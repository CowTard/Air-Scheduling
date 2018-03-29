using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        
        private static List<Aircraft> _aircraftModels = new List<Aircraft>();
        
        
        public static void Main(string[] args)
        {

            read_configuration_files();
            /*
            var selection = new EliteSelection();
            var crossover = new OrderedCrossover();
            var mutation = new ReverseSequenceMutation();
            var fitness = new Genetics.Fitness();
            var chromosome = new Genetics.Chromosome(10);
            var population = new Population (50, 70, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            ga.Termination = new GenerationNumberTermination(100);

            Console.WriteLine("GA running...");
            ga.Start();

            Console.WriteLine("Best solution found has {0} fitness.", ga.BestChromosome.Fitness);
            */
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
                    var minimumSpeed = Double.Parse(splittedLine[3]);
                    var optimalSpeed = Double.Parse(splittedLine[4]);
                    var maximumSpeed = Double.Parse(splittedLine[5]);
                
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
    }
}