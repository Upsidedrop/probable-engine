namespace NEAT_Implementation
{
    using System.Linq;
    internal class Program
    {   
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
        static int innovCount = 0;
        static int[] outputIds = {5};
        static int[] inputIds = {1,2,3};

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

            public List<Node> nodeGenes = new List<Node>();
            List<Connection> connectionGenes = new List<Connection>();
            public void RunConnections()
            {
                
                List<Connection> outputPath = new List<Connection>();
                foreach(int id in outputIds)
                {
                    SortConnections(connectionGenes, ref outputPath, id);
                }
                foreach(Connection connection in outputPath)
                {
                    for(int i = 0; i < nodeGenes.Count; i++)
                    {
                        if (nodeGenes[i].id == connection.output)
                        {
                            double value;
                            value = Search(nodeGenes, o => o.id == connection.input).value * connection.weight;
                            if (!inputIds.Contains(connection.input))
                            {
                                value = Math.Tanh(value);
                            }
                            nodeGenes[i].value += value;
                            break;
                        }
                    }
                }
            }
            void SortConnections(List<Connection> input, ref List<Connection> output, int nodeToCheck)
            {
                List<Connection> attachments;
                attachments = input.Where(o => o.output == nodeToCheck && o.enabled).ToList();
                foreach(Connection attachment in attachments)
                {
                    if (output.Select(o => o.innovId).Contains(attachment.innovId))
                    {
                        continue;
                    }
                    SortConnections(input, ref output, attachment.input);
                    output.Add(attachment);
                }

            }
        }
    }

}