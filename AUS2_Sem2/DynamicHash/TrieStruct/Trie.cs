using System.Collections;

namespace AUS2_Sem2.DynamicHash.TrieStruct
{
    public class Trie
    {
        public InternalNode? Root { get; set; }

        public Trie(List<(ExternalNode, BitArray)>? externalNodes = null)
        {
            Root = new InternalNode();

            if (externalNodes == null)
            {
                Root.Right = new ExternalNode(-1, 0, Root);
                Root.Left = new ExternalNode(-1, 0, Root);
            }
            else
            {
                foreach (var (node, bits) in externalNodes)
                {
                    var currentNode = Root;
                    var insertNode = node;
                    var bitPath = bits;

                    for (int i = 0; i < bitPath.Length; i++)
                    {
                        if (bitPath[i])
                        {
                            if (currentNode.HasRightChild())
                            {
                                currentNode = (InternalNode)currentNode.Right;
                            }
                            else
                            {
                                currentNode.InsertRight(insertNode);
                                break;
                            }
                        }
                        else
                        {
                            if (currentNode.HasLeftChild())
                            {
                                currentNode = (InternalNode)currentNode.Left;
                            }
                            else
                            {
                                currentNode.InsertLeft(insertNode);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public (bool, ExternalNode?, int) FindExternalNode(BitArray data)
        {
            var currentNode = Root;
            var lastNode = (ExternalNode?)null;
            var lastBit = -1;

            if (currentNode == null)
            {
                return (false, null, -1);
            }

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i])
                {
                    if (currentNode.HasRightChild())
                    {
                        currentNode = (InternalNode?)currentNode.Right;
                    }
                    else
                    {
                        lastNode = (ExternalNode?)currentNode.Left;
                        lastBit = i;
                        break;
                    }
                }
                else
                {
                    if (currentNode.HasLeftChild())
                    {
                        currentNode = (InternalNode?)currentNode.Left;
                    }
                    else
                    {
                        lastNode = (ExternalNode?)currentNode.Right;
                        lastBit = i;
                        break;
                    }
                }
            }

            return (lastNode != null, lastNode, lastBit);
        }

        public List<(ExternalNode, BitArray)> GetAll()
        {
            if (Root == null)
            {
                return new List<(ExternalNode, BitArray)>();
            }

            var result = new List<(ExternalNode, BitArray)>();
            var stack = new Stack<(Node, BitArray)>();
            stack.Push((Root, new BitArray(0)));

            while (stack.Count > 0)
            {
                var (node, bits) = stack.Pop();

                if (node is InternalNode)
                {
                    if (((InternalNode)node).HasLeftChild())
                    {
                        stack.Push((((InternalNode?)node).Left, bits));
                    }
                    else
                    {
                        stack.Push((((InternalNode)node).Left, bits));
                    }
                }
                else
                {
                    result.Add(((ExternalNode)node, bits));
                }
            }
            return result;
        }

        public void SaveFile(string path)
        {
            var content = GetAll();
            var list = new List<string>();

            foreach ( var node in content )
            {
                string trace = "";

                foreach ( bool bit in node.Item2 )
                {
                    trace += bit ? "1" : "0";
                }

                list.Add($"{ node.Item1.Offset };{ node.Item1.Count };{ trace }");
            }

            File.WriteAllLines(path, list);
        }

        public static List<(ExternalNode, BitArray)> LoadFile(string path)
        {
            var result = new List<(ExternalNode, BitArray)>();
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var split = line.Split(';');
                var offset = long.Parse(split[0]);
                var count = int.Parse(split[1]);
                var trace = split[2];
                var bits = new BitArray(trace.Length);

                for (int i = 0; i < trace.Length; i++)
                {
                    bits[i] = trace[i] == '1';
                }

                result.Add((new ExternalNode(count, offset), bits));
            }

            return result;
        }
    }
}
