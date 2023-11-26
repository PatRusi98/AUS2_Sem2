namespace AUS2_Sem2.DynamicHash.TrieStruct
{
    public class InternalNode : Node
    {
        public Node? Left { get; set; }
        public Node? Right { get; set; }

        public InternalNode(InternalNode? parent = null) : base(parent)
        {
            Left = null;
            Right = null;
        }

        public void ReplaceChild(Node oldNode, Node newNode)
        {
            if (Left == oldNode)
            {
                Left = newNode;
                newNode.Parent = this;
            }
            else if (Right == oldNode)
            {
                Right = newNode;
                newNode.Parent = this;
            }
        }

        public void InsertLeft(Node node)
        {
            if (Left != null)
            {
                Left.Parent = null;
            }

            Left = node;
            node.Parent = this;
        }

        public void InsertRight(Node node) 
        {
            if (Right != null)
            {
                Right.Parent = null;
            }

            Right = node;
            node.Parent = this;
        }

        public bool HasLeftChild()
        {
            return Left != null;
        }

        public bool HasRightChild() 
        { 
            return Right != null;
        }
    }
}
