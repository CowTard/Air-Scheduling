using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AirScheduling.Aviation;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Mutations;

namespace AirScheduling.Genetics
{
    public class Mutation : IMutation
    {
        
        Random rnd = new Random();
        
        public void Mutate(IChromosome chromosome, float probability)
        {
            if (!(rnd.NextDouble() < probability)) return;

            var t = chromosome;
            chromosome = PerformeSequenceMutation(chromosome);
            PerformeRunwayMutation(chromosome);
            PerformSpeedMutation(chromosome);
        }

        /// <summary>
        /// Performs a sequence change
        /// </summary>
        /// <param name="chromosome">Chromosome in which mutation will occur</param>
        /// <returns>Chromossome</returns>
        private IChromosome PerformeSequenceMutation(IChromosome chromosome)
        {
            var chr = (Chromosome) chromosome;


            var rnd = new Random();
            var generateValued = rnd.Next(chr.Length - 1);
            var gen = (Gene) chr.GetGene(generateValued).Value;
            var candidates = new List<int>();

            if (generateValued - 1 >= 0)
                candidates.Add(generateValued - 1);

            if (generateValued + 1 < chr.Length)
                candidates.Add(generateValued + 1);


            if (candidates.Count == 1)
                chr.SwapGenes(generateValued, candidates[0]);
            else
                chr.SwapGenes(generateValued, rnd.NextDouble() <= 0.5 ? candidates[0] : candidates[1]);

            return chr;
        }

        /// <summary>
        /// Allows a change of the runway assigned to this gene
        /// </summary>
        /// <param name="chromosome"></param>
        /// <returns></returns>
        private IChromosome PerformeRunwayMutation(IChromosome chromosome)
        {
            var chr = (Chromosome) chromosome;

            var indexToMutate = rnd.Next(chr.Length);

            var currentRunway = ((Gene) chr.GetGene(indexToMutate).Value).GetRunway().GetIdentification();
            var allRunways = ((Chromosome) chr).GetAirport().Runways.Keys.ToList();

            allRunways.Remove(currentRunway);

            var chosenRunway = ((Chromosome) chr).GetAirport()
                .Runways[allRunways.ToList()[rnd.Next(allRunways.Count)]];

            ((Gene) chr.GetGene(indexToMutate).Value).MutateRunway(chosenRunway);

            return chr;
        }

        /// <summary>
        /// Function responsible for mutating the speed of a current aircraft
        /// </summary>
        /// <param name="chromosome"></param>
        /// <returns></returns>
        private IChromosome PerformSpeedMutation(IChromosome chromosome)
        {

            var chr = (Chromosome) chromosome;
            var indexToMutate = rnd.Next(chr.Length);

            var currentSpeed = ((Gene) chr.GetGene(indexToMutate).Value).usedSpeed;
            
            var listOfSpeeds = Enum.GetValues(typeof(AircraftSpeed)).Cast<AircraftSpeed>().ToList();

            listOfSpeeds.Remove(currentSpeed);
            
            ((Gene) chr.GetGene(indexToMutate).Value).MutateSpeed(listOfSpeeds[rnd.Next(0, listOfSpeeds.Count)]);

            return chr;

        }
        
        public bool IsOrdered { get; }
    }
}