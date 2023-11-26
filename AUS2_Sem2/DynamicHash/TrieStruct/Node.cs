namespace AUS2_Sem2.DynamicHash.TrieStruct
{
    public class Node
    {
        public InternalNode? Parent { get; set; }

        public Node(InternalNode? parent = null)
        {
            Parent = parent;
        }
    }
}
