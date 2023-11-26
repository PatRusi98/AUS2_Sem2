namespace AUS2_Sem2.DynamicHash.TrieStruct
{
    public class ExternalNode : Node
    {
        public int Count { get; set; }
        public long Offset { get; set; }

        public ExternalNode(int count, long offset, InternalNode? parent = null, ExternalNode? node = null) : base(parent)
        {
            if (node != null)
            {
                Count = node.Count;
                Offset = node.Offset;
                Parent = node.Parent;
            }
            else
            {
                Count = count;
                Offset = offset;
            }
        }

        public bool IsLeft()
        {
            if (Parent == null)
            {
                return false;
            }

            return Parent.Left == this;
        }

        public bool IsRight()
        {
            if (Parent == null)
            {
                return false;
            }

            return Parent.Right == this;
        }

        public ExternalNode? GetSibling()
        {
            if (Parent == null)
            {
                return null;
            }

            return IsLeft() ? (ExternalNode?)Parent.Right : (ExternalNode?)Parent.Left;
        }
    }
}
