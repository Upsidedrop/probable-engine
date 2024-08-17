namespace NEAT_Implementation
{
    using System.Linq;
    internal class Program
    {
        static int innovCount = 4;
        static int[] outputIds = { 5 };
        static int[] inputIds = { 1,2,3 };
        static void Main()
        {
            List<Connection> father = new();
            List<Connection> mother = new();
            father.Add(new(0, 1, 4, 1));
            father.Add(new(0, 2, 4, 2));
            father.Add(new(0, 2, 5, 4));
            father.Add(new(0, 3, 5, 5));
            father.Add(new(0, 4, 5, 6));
            mother.Add(new(1, 1, 4, 1));
            mother.Add(new(1, 2, 4, 2));
            mother.Add(new(1, 3, 4, 3));
            mother.Add(new(1, 2, 5, 4));
            mother.Add(new(1, 4, 5, 6));
            mother.Add(new(1, 1, 6, 7));
            mother.Add(new(1, 6, 4, 8));
            father[2].enabled = false;
            mother[3].enabled = false;
            mother[0].enabled = false;
            Console.WriteLine("MOTHER");
            foreach (Connection item in mother)
            {
                Console.WriteLine(item.innovId + " " + item.input + " -> "+item.output);
            }
            Console.WriteLine("FATHER");
            foreach (Connection item in father)
            {
                Console.WriteLine(item.innovId+" "+item.input + " -> " + item.output);
            }
            Genome temp = Reproduce(father, mother);
            Console.WriteLine("COMBINED");
            foreach (Connection item in temp.connectionGenes)
            {
                if (item.weight > 0) {
                    Console.WriteLine(item.innovId + " " + item.input + " -> "+"MOTHER"+" -> " + item.output + " -> " + item.enabled);
                }
                else
                {
                    Console.WriteLine(item.innovId + " " + item.input + " -> " + "FATHER" + " -> " + item.output + " -> "+item.enabled);
                }
            }
            foreach(Node node in temp.nodeGenes)
            {
                Console.Write(node.id + " ");
            }
            temp.RunConnections();
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
            public List<Node> nodeGenes = new();
            public List<Connection> connectionGenes = new();
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
                connectionGenes = connections;
                nodeGenes = new();
                List<int> nodeIds = new();
                nodeIds = connections.Select(o => o.output).Union(connections.Select(o => o.input)).ToList();
                foreach (int id in nodeIds)
                {
                    nodeGenes.Add(new(id));
                }
            }
        }
        static Genome Reproduce(List<Connection> father, List<Connection> mother)
        {
            Random rand = new();
            List<Connection> offspring = new();
            offspring.AddRange(father.Where(f => !mother.Select(m => m.innovId).Contains(f.innovId)).ToList());
            offspring.AddRange(mother.Where(m => !father.Select(f => f.innovId).Contains(m.innovId)).ToList());
            List<int> sharedConnections;
            sharedConnections = father.Select(o => o.innovId).Where(f => mother.Select(o => o.innovId).Contains(f)).ToList();
            foreach (int id in sharedConnections)
            {
                List<Connection> temp;
                temp = rand.Next(2) == 1 ? father : mother;
                offspring.Add(Search(temp, o => o.innovId == id));
                if (!(Search(father, o => o.innovId == id).enabled && Search(mother, o => o.innovId == id).enabled))
                {
                    offspring[^1].enabled = false;
                }
            }
            return new Genome(offspring);
        }

    }

}