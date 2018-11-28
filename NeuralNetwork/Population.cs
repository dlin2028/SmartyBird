using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork
{
    public class Population
    {
        private NeuralNet BestNet => networks[0];

        private List<NeuralNet> networks;
        private readonly int size;

        Random rng;

        public Population(Random rng, int size, int inputs, params int[] layerNeurons)
        {
            this.rng = rng;
            networks = new List<NeuralNet>();
            this.size = size;
            for (int i = 0; i < size; i++)
            {
                networks.Add(new NeuralNet(Activations.BinaryStep, inputs, layerNeurons));
                networks[networks.Count-1].Randomize(rng);
            }
        }

        public double[] Compute(double[] data)
        {
            return BestNet.Compute(data);
        }

        public int Train(double[][] inputs, double[][] desiredOutputs, int maxGeneration = -1)
        {
            int gen = 0;
            while(true)
            {
                networks.Sort((x, y) =>
                    x.Fitness(inputs, desiredOutputs)
                    .CompareTo(
                        y.Fitness(inputs, desiredOutputs)));

                if(networks[0].Fitness(inputs, desiredOutputs) == 0 || gen == maxGeneration)
                {
                    break;
                }

                int start = (int)(networks.Count * 0.05);
                int end = (int)(networks.Count * 0.90);

                for (int i = start; i < end; i++)
                {
                    networks[i].Crossover(networks[rng.Next(start)], rng);
                    networks[i].Mutate(rng, 0.2);
                }

                for (int i = end; i < networks.Count; i++)
                {
                    networks[i].Randomize(rng);
                }

                gen++;
                
            }

            return gen;
        }
    }
}
