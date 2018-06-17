using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            try
            {
                for (var i = 0; i < _airport.Radar.Count; i++)
                    ReplaceGene(i, genes[i]);
            }
            catch (Exception e)
            {
                Console.WriteLine("OUCGH");
                throw;
            }
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
            return _airport;
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
            var toRemove = new List<GeneticSharp.Domain.Chromosomes.Gene>();

            foreach (var g in genes)
            {
                ((Gene) g.Value).Cost = 0;
                foreach (var origGene in curChromosome)
                {
                    if ( ((Gene) g.Value).GetRadarAircraft().GetFlightIdentification() == ((Gene) origGene.Value).GetRadarAircraft().GetFlightIdentification())
                    {
                        toRemove.Add(origGene);
                        break;
                    }
                }
            }

            foreach (var geneToBeRemoved in toRemove)
            {
                curChromosome.Remove(geneToBeRemoved);
            }
            
            
            curChromosome.InsertRange(0, genes);

            Debug.Assert(curChromosome.Count == 8);
            return curChromosome;
        }


        /// <summary>
        /// Shows list of aircrafts and their schedule timer
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // sort by landing time
            var t = GetGenes().ToList().OrderBy(e => ((Gene) e.Value).GetArrivalTime()).ToList();

            var averageCost = 0.0;
            var averageDelay = TimeSpan.Zero;

            foreach (var g in t)
            {
                averageCost += ((Gene) g.Value).Cost;

                var landingT = ((Gene) g.Value).GetArrivalTime();
                var desiredT = ((Gene) g.Value).Aircraft.GetDesiredLandingTime();

                averageDelay += desiredT.Subtract(landingT);
            }
            var text =
                $"{(averageCost / 8).ToString("C", CultureInfo.CurrentCulture)}, " +
                $"{new TimeSpan(averageDelay.Ticks / 8)}, " +
                $"{Fitness}";
            
            return text;
        }

        /// <summary>
        /// Sorts the chromosome by future trip time
        /// </summary>
        private void SortChromosome()
        {

            var genes = GetGenes();
            var _ = genes.ToList().OrderBy( e => ((Gene) e.Value).Aircraft.GetNextFlightTime()).ToArray();

            ReplaceGenes(0, _);
        }

        public override bool Equals(object obj)
        {
            var obc = (Chromosome) obj;

            return obc != null && (obc._airport == _airport && obc.LastLanding == LastLanding && obc.GetGenes() == GetGenes());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}