using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using AirScheduling.Aviation;
using GeneticSharp.Domain.Chromosomes;

namespace AirScheduling.Genetics
{
    public class Chromosome: ChromosomeBase
    {
        private readonly Airport _airport;
        
        /// <summary>
        /// Construtor that receives an airport radar
        /// </summary>
        /// <param name="airport"></param>
        public Chromosome(Airport airport) : base(airport.Radar.Count)
        {
            _airport = airport;
            GenerateAllGenes();
        }

        /// <summary>
        /// Responsible for iterating all the aircrafts present in radar and populate the chromossome
        /// </summary>
        private void GenerateAllGenes()
        {
            for (var i = 0; i < _airport.Radar.Count; i++)
            {
                ReplaceGene(i, GenerateGene(i));
            }
        }
        
        /// <summary>
        /// Picks the ith aircraft in radar and a random runway to generate a gene
        /// </summary>
        /// <param name="geneIndex">The ith in radar</param>
        /// <returns>A new gene</returns>
        public override GeneticSharp.Domain.Chromosomes.Gene GenerateGene(int geneIndex)
        {
            
            var geneInformation = new Gene(_airport.Radar[_airport.Radar.Keys.ElementAt(geneIndex)],
                _airport.GetRandomRunway(), -1);
            
            return new GeneticSharp.Domain.Chromosomes.Gene(geneInformation);
        }

        /// <summary>
        /// Allows the use of the airport object to outer classes
        /// </summary>
        /// <returns></returns>
        public Airport GetAirport()
        {
            return this._airport;
        }

        /// <summary>
        /// Creates a new chromossome with same aircrafts but with different runway attribution
        /// </summary>
        /// <returns>New Chromosome</returns>
        public override IChromosome CreateNew()
        {
            return new Chromosome(_airport);
        }

        /// <summary>
        /// Swap gene in from position with the gene in to position
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void SwapGenes(int from, int to)
        {
            var temp = GetGene(from);
            
            ReplaceGene(from, GetGene(to));
            ReplaceGene(to, temp);
        }

        /// <summary>
        /// Shows list of aircrafts and their schedule timer
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var text = "";

            foreach (var gene in GetGenes())
            {
                var castedGene = (Gene) gene.Value;

                text += castedGene.GetRadarAircraft() + " " + castedGene.GetRunway() + " " + 
                        castedGene.GetArrivalTime() + Environment.NewLine;
            }

            return text;
        }
    }
}