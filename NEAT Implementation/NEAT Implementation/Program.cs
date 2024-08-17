namespace NEAT_Implementation
{
    using System.Linq;
    internal class Program
    {
        static int innovCount = 4;
        static int[] outputIds = { 2 };
        static int[] inputIds = { 1 };
        static void Main()
        {
            Genome genome = new Genome();
            foreach (int id in outputIds)
            {
                genome.nodeGenes.Add(new(id));
            }
            foreach (int id in inputIds)
            {
                genome.nodeGenes.Add(new(id));
                
                foreach (int outId in outputIds)
                {
                    genome.connectionGenes.Add(new(0, id, outId, 2 + id));
                }
            }
            while (true)
            {
                genome.Mutate();
                foreach(Connection connection in genome.connectionGenes)
                {
                    if (connection.enabled)
                    {
                        Console.WriteLine(connection.input + " -> " + connection.weight + " -> " + connection.output);
                    }
                }
                try
                {
                    genome.nodeGenes[1].value = 1;
                    genome.RunConnections();
                    Console.WriteLine(Search(genome.nodeGenes, o => o.id == outputIds[0]).value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Console.ReadLine();
            }
        }
        static T? Search<T>(List<T> list, Predicate<T> predicate)
        {
            foreach (var item in list)
            {
                if (predicate(item))
                {
                    return item;
                }
            }
            return default;
        }


        class Connection
        {
            public double weight;
            public bool enabled;
            public int input;
            public int output;
            public int innovId;
            public Connection(double weight, int input, int output, int innovId)
            {
                this.weight = weight;
                this.input = input;
                this.output = output;
                this.innovId = innovId;
                enabled = true;
            }
        }
        class Node
        {
            public double value;
            public int id;
            public Node(int id)
            {
                this.id = id;
            }
        }

        class Genome
        {
            Random random = new();
            public List<Node> nodeGenes = new List<Node>();
            public List<Connection> connectionGenes = new List<Connection>();
            public void RunConnections()
            {
                
                List<Connection> outputPath = new List<Connection>();
                
                foreach(int id in outputIds)
                {
                    List<int> temp = new List<int>();
                    SortConnections(connectionGenes, ref outputPath, id, ref temp);
                }
                foreach(Connection connection in outputPath)
                {
                    for(int i = 0; i < nodeGenes.Count; i++)
                    {
                        if (nodeGenes[i].id == connection.output)
                        {
                            double value;
                            value = Search(nodeGenes, o => o.id == connection.input).value;
                            if (!inputIds.Contains(connection.input))
                            {
                                value = Math.Tanh(value);
                            }
                            value *= connection.weight;
                            nodeGenes[i].value += value;
                            break;
                        }
                    }
                }
            }
            void SortConnections(List<Connection> input, ref List<Connection> output, int nodeToCheck, ref List<int> loopChecker)
            {
                List<Connection> attachments;
                attachments = input.Where(o => o.output == nodeToCheck && o.enabled).ToList();
                foreach(Connection attachment in attachments)
                {
                    if (loopChecker.Contains(attachment.innovId))
                    {
                        continue;
                    }
                    if (output.Select(o => o.innovId).Contains(attachment.innovId))
                    {
                        continue;
                    }
                    loopChecker.Add(attachment.innovId);
                    SortConnections(input, ref output, attachment.input, ref loopChecker);
                    output.Add(attachment);
                }

            }
            public void Mutate()
            {
                switch (random.Next(10))
                {
                    case 9:
                        StructuralMutation();
                        connectionGenes[random.Next(connectionGenes.Count)].weight += random.NextDouble() - 0.5;
                        break;
                    case > 0:
                        connectionGenes[random.Next(connectionGenes.Count)].weight += random.NextDouble() - 0.5;
                        break;
                    default:
                        return;
                }
            }
            void StructuralMutation()
            {
                Console.WriteLine("structMutate");
                if(random.Next(2) == 0)
                {
                    int randIndex = random.Next(connectionGenes.Count);
                    connectionGenes[randIndex].enabled = false;
                    int createdNode;
                    createdNode = innovCount;
                    innovCount++;
                    nodeGenes.Add(new(createdNode));
                    connectionGenes.Add(new(connectionGenes[randIndex].weight, connectionGenes[randIndex].input, createdNode,innovCount));
                    innovCount++;
                    connectionGenes.Add(new(connectionGenes[randIndex].weight, createdNode, connectionGenes[randIndex].output, innovCount));
                    innovCount++;
                }
                else
                {
                    int randIndex = random.Next(nodeGenes.Count);
                    List<int> availableIndices;
                    availableIndices = nodeGenes.Select(o => o.id)
                        .Where(o => 
                        /*connectionGenes.Where(c => (c.output == nodeGenes[randIndex].id && c.input == o)).Count() == 0
                        && */o != nodeGenes[randIndex].id).ToList();
                    if (availableIndices.Count == 0)
                    {
                        return;
                    }
                    int randAvail = random.Next(availableIndices.Count);
                    connectionGenes.Add(new(0, nodeGenes[randIndex].id, availableIndices[randAvail], innovCount));
                    innovCount++;
                }
            }
        }
    }

}