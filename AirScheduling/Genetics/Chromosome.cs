using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AirScheduling.Aviation;
using GeneticSharp.Domain.Chromosomes;

namespace AirScheduling.Genetics
{
    public class Chromosome : ChromosomeBase
    {
        private readonly Airport _airport;
        public Dictionary<string, string> LastLanding { get; }

        /// <summary>
        /// Construtor that receives an airport radar
        /// </summary>
        /// <param name="airport"></param>
        public Chromosome(Airport airport) : base(airport.Radar.Count)
        {
            _airport = airport;
            LastLanding = new Dictionary<string, string>(airport.Runways.Count);
            GenerateAllGenes();
            SortChromosome();
        }

        /// <summary>
        /// Constructor used to create a new chromossome when crossover happens
        /// </summary>
        /// <param name="airport"></param>
        /// <param name="genes"></param>
        public Chromosome(Airport airport, IList<GeneticSharp.Domain.Chromosomes.Gene> genes) : base(
            airport.Radar.Count)
        {
            _airport = airport;
            LastLanding = new Dictionary<string, string>(airport.Runways.Count);

            for (var i = 0; i < _airport.Radar.Count; i++)
                ReplaceGene(i, genes[i]);
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
                _airport.GetRandomRunway(), TimeSpan.Zero);

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
        /// Returns a slice of the the array of genes
        /// </summary>
        /// <param name="from">Initial index (will be included)</param>
        /// <param name="to">Last index (will not be included in final list)</param>
        /// <returns></returns>
        public IEnumerable<GeneticSharp.Domain.Chromosomes.Gene> GetSliceOfChromosome(int from, int to)
        {
            return GetGenes().ToList().GetRange(from, to - from);
        }

        /// <summary>
        /// Retrieves a new chromosome with new prepended genes and with repeated genes cleared.
        /// </summary>
        /// <param name="genes">IList of genes to be prepended</param>
        /// <returns>A new chromosome without repeated genes and a new head of genes</returns>
        public IList<GeneticSharp.Domain.Chromosomes.Gene> PrependGenes(
            IEnumerable<GeneticSharp.Domain.Chromosomes.Gene> genes)
        {
            var curChromosome = GetGenes().ToList();
            curChromosome.InsertRange(0, genes);

            var toRemove = new List<GeneticSharp.Domain.Chromosomes.Gene>();

            for (var i = 0; i < curChromosome.Count - 1; i++)
            {
                for (var j = i + 1; j < curChromosome.Count; j++)
                {
                    if (curChromosome[i] != curChromosome[j]) continue;

                    toRemove.Add(curChromosome[j]);
                    break;
                }
            }

            foreach (var geneToBeRemoved in toRemove)
            {
                curChromosome.Remove(geneToBeRemoved);
            }

            return curChromosome;
        }


        /// <summary>
        /// Shows list of aircrafts and their schedule timer
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // sort by landing time
            var _t = GetGenes().ToList().OrderBy(e => ((Gene) e.Value).GetArrivalTime()).ToList();

            var average_cost = 0.0;
            var average_delay = 0.0;

            foreach (var g in GetGenes())
            {
                average_cost += ((Gene) g.Value).Cost;
                average_delay += (((Gene) g.Value).Aircraft.GetDesiredLandingTime() - ((Gene) g.Value).GetArrivalTime()).Duration().Ticks;
            }

            var text = String.Format("{0}, {1}", (average_cost / 8).ToString("C", CultureInfo.CurrentCulture), new TimeSpan((long)average_delay / 8));
            
            return text;
        }

        /// <summary>
        /// Sorts the chromosome by future trip time
        /// </summary>
        private void SortChromosome()
        {

            var genes = this.GetGenes();
            var _ = genes.ToList().OrderBy( e => ((Gene) e.Value).Aircraft.GetNextFlightTime()).ToArray();

            ReplaceGenes(0, _);
        }
    }
}