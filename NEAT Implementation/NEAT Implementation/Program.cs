namespace NEAT_Implementation
{
    using System;
    using System.Linq;

    internal class Program
    {
        //revert from readonly when finished
        static int innovCount = 8;
        static Random random = new();
        readonly static int[] outputIds = { 4 };
        readonly static int[] inputIds = { 1, 2, 3 };
        readonly static bool managesInstances = true;
        //static bool speciateInstances = true;
        readonly static int targetInstanceCount = 10;
        static void Main()
        {
            //speciateInstances = speciateInstances && managesInstances;
            if (managesInstances)
            {

                Genome[] instances = new Genome[targetInstanceCount];
                List<Action> tasks = new();
                List<Connection> connections = new();
                for (int i = 0; i < inputIds.Length; i++)
                {
                    for (int j = 0; j < outputIds.Length; j++)
                    {
                        connections.Add(new(0, inputIds[i], outputIds[j], inputIds.Length + outputIds.Length + (i + 1) * (j + 1)));
                    }
                }
                for (int i = 0; i < targetInstanceCount; i++)
                {

                    instances[i] = (new(connections));
                    //Creates new variable because instances[i] grabs i at time of execution
                    int tmp = i;
                    tasks.Add(() => instances[tmp].Mutate());
                    tasks.Add(() => instances[tmp].RunConnections());
                }
                Parallel.Invoke(tasks.ToArray());

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
            public double cost;
            public List<Node> nodeGenes;
            public List<Connection> connectionGenes;
            public void RunConnections()
            {
                List<Connection> outputPath = new();

                foreach (int id in outputIds)
                {
                    List<int> temp = new();
                    SortConnections(connectionGenes, ref outputPath, id, ref temp);
                }
                foreach (Connection connection in outputPath)
                {
                    for (int i = 0; i < nodeGenes.Count; i++)
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
                if (random.Next(2) == 0)
                {
                    int randIndex = random.Next(connectionGenes.Count);
                    connectionGenes[randIndex].enabled = false;
                    int createdNode;
                    createdNode = innovCount;
                    innovCount++;
                    nodeGenes.Add(new(createdNode));
                    connectionGenes.Add(new(connectionGenes[randIndex].weight, connectionGenes[randIndex].input, createdNode, innovCount));
                    innovCount++;
                    connectionGenes.Add(new(1, createdNode, connectionGenes[randIndex].output, innovCount));
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
            void SortConnections(List<Connection> input, ref List<Connection> output, int nodeToCheck, ref List<int> loopChecker)
            {
                List<Connection> attachments;
                attachments = input.Where(o => o.output == nodeToCheck && o.enabled).ToList();
                foreach (Connection attachment in attachments)
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
            public Genome(List<Connection> connections)
            {
                connectionGenes = new();
                //Had to copy over like this so the list wouldnt save a reference
                foreach (Connection item in connections)
                {
                    connectionGenes.Add(new Connection(item.weight, item.input, item.output, item.innovId));

                }

                nodeGenes = new();
                List<int> nodeIds = new();
                nodeIds = connections.Select(o => o.input).Union(connections.Select(o => o.output)).ToList();
                foreach (int id in nodeIds)
                {
                    nodeGenes.Add(new(id));
                }

            }
        }
        static List<Connection> Reproduce(List<Connection> father, List<Connection> mother)
        {
            List<Connection> offspring = new();
            offspring.AddRange(father.Where(f => !mother.Select(m => m.innovId).Contains(f.innovId)).ToList());
            offspring.AddRange(mother.Where(m => !father.Select(f => f.innovId).Contains(m.innovId)).ToList());
            List<int> sharedConnections;
            sharedConnections = father.Select(o => o.innovId).Where(f => mother.Select(o => o.innovId).Contains(f)).ToList();
            foreach (int id in sharedConnections)
            {
                List<Connection> temp;
                temp = random.Next(2) == 1 ? father : mother;
                offspring.Add(Search(temp, o => o.innovId == id));
                if (!(Search(father, o => o.innovId == id).enabled && Search(mother, o => o.innovId == id).enabled))
                {
                    offspring[^1].enabled = false;
                }
            }
            return (offspring);
        }
        
    }

}