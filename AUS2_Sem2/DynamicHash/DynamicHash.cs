using System.Collections;
using System.Net.WebSockets;
using AUS2_Sem2.DynamicHash.TrieStruct;
using AUS2_Sem2.Helpers;

namespace AUS2_Sem2.DynamicHash
{
    public class DynamicHash<T> where T : IRecord<T>
    {
        public FileStream HashFile { get; set; }
        public Trie Trie { get; set; }
        public int BlockFactor { get; set; }
        public int BlockSize { get; set; }
        public List<long> EmptyBlocksOffset { get; set; }
        public static string Path { get; set; } = "BaseData.csv";
        public static string PathTrie { get; set; } = "TrieData.csv";
        public static string PathEmptyBlocks { get; set; } = "EmptyBlocks.csv";

        public DynamicHash(string fileName, int blockFactor, Trie? trie = null, List<long>? offsets = null)
        {
            if (trie != null)
            {
                Trie = trie;
            }
            else
            {
                Trie = new Trie();
            }

            if (offsets != null)
            {
                EmptyBlocksOffset = offsets;
            }
            else
            {
                EmptyBlocksOffset = new List<long>();
            }

            BlockSize = new Block<T>(blockFactor).GetSize();
            BlockFactor = blockFactor;

            try
            {
                HashFile = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
            catch (Exception)
            {
                throw new Exception($"HashFile not found!");
            }
        }

        public (Block<T>?, long) FindBlock(T data)
        {
            BitArray hash = data.GetHash();
            var offset = GetOffset(hash);
            if (offset != -1)
            {
                var block = TryReadBlockFromFile(offset);
                return (block, offset);
            }
            else
            {
                return (null, -1);
            }
        }

        public T? Find(T data)
        {
            var result = FindBlock(data);
            var block = result.Item1;
            if (block != null)
            {
                for (int i = 0; i < block.Records.Count; i++)
                {
                    if (i < block.ValidCount)
                    {
                        if (data.Equals(block.Records[i]))
                            return block.Records[i];
                    }
                }
            }
            return default(T);
        }

        public bool Insert(T data)
        {
            var hash = data.GetHash();
            var result = Trie.FindExternalNode(hash);
            if (result.Item1)
            {
                var oldNode = result.Item2;

                if (oldNode!.Offset == -1)
                {
                    AssignOffsetToNode(oldNode);
                }

                if (oldNode!.Count >= BlockFactor)
                {
                    return RehashAndInsert(hash, data, oldNode, result.Item3);
                }
                else
                {
                    var (block, offset) = FindBlock(data);
                    if (block!.Insert(data))
                    {
                        TryWriteBlockToFile(offset, block);
                        oldNode.Count++;
                        return true;
                    }

                    return false;
                }
            }
            return false;
        }

        private bool RehashAndInsert(BitArray newHash, T data, ExternalNode splitNode, int bit)
        {
            var block = TryReadBlockFromFile(splitNode.Offset);
            var freeOffset = splitNode.Offset;
            InternalNode? internalNode = null;
            ExternalNode? left = null;
            ExternalNode? right = null;
            BitArray hash;
            var dataRight = false;

            splitNode.Count++;
            Direction nodeDirection;

            while (splitNode.Count > BlockFactor)
            {
                bit++;
                if (splitNode.IsLeft())
                {
                    nodeDirection = Direction.Left;
                }
                else
                {
                    nodeDirection = Direction.Right;
                }

                right = new ExternalNode(-1, 0);
                left = new ExternalNode(-1, 0);

                if (nodeDirection == Direction.Left)
                {
                    internalNode = new InternalNode(splitNode.Parent);
                    ((InternalNode)splitNode.Parent!).Left = internalNode;
                }
                else
                {
                    internalNode = new InternalNode(splitNode.Parent);
                    ((InternalNode)splitNode.Parent!).Right = internalNode;
                }

                internalNode.Left = left;
                internalNode.Right = right;
                left.Parent = internalNode;
                right.Parent = internalNode;

                left.Count = 0;
                right.Count = 0;

                foreach (var record in block.Records)
                {
                    hash = record.GetHash();

                    if (bit < hash.Count)
                    {
                        if (hash[bit])
                        {
                            right.Count++;
                        }
                        else
                        {
                            left.Count++;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                if (bit < newHash.Count)
                { 
                    if (newHash[bit])
                    {
                        dataRight = true;
                        right.Count++;
                    }
                    else
                    {
                        dataRight = false;
                        left.Count++;
                    }
                }
                else
                {
                    return false;
                }

                if (left.Count > BlockFactor)
                {
                    splitNode = left;
                }
                else if (right.Count > BlockFactor)
                {
                    splitNode = right;
                }
                else
                {
                    splitNode.Count = 0;
                }
            }

            var leftBlock = new Block<T>(BlockFactor);
            var rightBlock = new Block<T>(BlockFactor);

            foreach (var record in block.Records)
            {
                hash = record.GetHash();

                if (bit < hash.Count)
                {
                    if (hash[bit])
                    {
                        rightBlock.Insert(record);
                    }
                    else
                    {
                        leftBlock.Insert(record);
                    }
                }
                else
                {
                    return false;
                }
            }

            if (dataRight)
            {
                left!.Offset = freeOffset;
                TryWriteBlockToFile(left.Offset, leftBlock);
                AssignOffsetToNode(right!);
                rightBlock.Insert(data);
                TryWriteBlockToFile(right!.Offset, rightBlock);
                return true;
            }
            else
            {
                right!.Offset = freeOffset;
                TryWriteBlockToFile(right.Offset, rightBlock);
                AssignOffsetToNode(left!);
                leftBlock.Insert(data);
                TryWriteBlockToFile(left!.Offset, leftBlock);
                return true;
            }
        }

        public long GetOffset(BitArray hash)
        {
            var result = Trie.FindExternalNode(hash);
            if (result.Item1)
            {
                return result.Item2.Offset;
            }

            return -1;
        }

        public void TryWriteBlockToFile(long offset, Block<T> block)
        {
            try
            {
                HashFile.Seek(offset, SeekOrigin.Begin);
                HashFile.Write(block.ToByteArray());
            }
            catch (Exception e)
            {
                throw new IOException($"Exception: {e.Message}");
            }
        }

        public Block<T> TryReadBlockFromFile(long offset)
        {
            var block = new Block<T>(BlockFactor);
            var blockSize = block.GetSize();
            byte[] blockBytes = new byte[blockSize];
            try
            {
                HashFile.Seek(offset, SeekOrigin.Begin);
                HashFile.Read(blockBytes);
            }
            catch (Exception e)
            {
                throw new Exception($"Exception occured: {e.Message}");
            }
            block.FromByteArray(blockBytes);
            return block;
        }

        public void ExportAppDataToFile()
        {
            Trie.SaveFile(PathTrie);
            var offsets = new List<string>();

            foreach (var offset in EmptyBlocksOffset)
            {
                offsets.Add(offset.ToString());
            }

            File.WriteAllLines(PathEmptyBlocks, offsets);
        }

        public void SaveBaseDataToFile()
        {
            File.WriteAllText(Path, $"{BlockFactor};{HashFile.Name}");
        }

        public static (int, string) LoadBaseDataFromFile()
        {
            var line = File.ReadAllText(Path);
            var results = line.Split(";");
            var blockFactor = int.Parse(results[0]);

            return (blockFactor, results[1]);
        }

        public List<string> GetSequenceOfBlocks()
        {
            var result = new List<string>();
            HashFile.Seek(0, SeekOrigin.Begin);
            var block = new Block<T>(BlockFactor);
            var blockCount = HashFile.Length / block.GetSize();
            var valids = 0;
            for (long i = 0; i < blockCount; i++)
            {
                result.Add("======");
                result.Add($"Address: {HashFile.Position}");
                var blockBytes = new byte[block.GetSize()];

                HashFile.Read(blockBytes);
                block.FromByteArray(blockBytes);

                result.Add($"Block {i}: Valid count = {block.ValidCount}");
                foreach (var record in block.Records)
                {
                    result.AddRange(record.GetStrings());
                }
                valids += block.ValidCount;
            }
            result.Insert(0, $"Valid = {valids}");
            return result;
        }

        public static (Trie, List<long>) LoadDataFromFile()
        {
            var trieItems = Trie.LoadFile(PathTrie);
            var trie = new Trie(trieItems);
            var emptyBlocks = File.ReadAllLines(PathEmptyBlocks);
            var emptyOffsets = new List<long>();

            foreach (var item in emptyBlocks)
            {
                emptyOffsets.Add(long.Parse(item));
            }

            return (trie, emptyOffsets);
        }

        private void AssignOffsetToNode(ExternalNode node)
        {
            var address = 0;
            if (EmptyBlocksOffset.Count > 0)
            {
                address = (int)EmptyBlocksOffset[EmptyBlocksOffset.Count - 1];
                EmptyBlocksOffset.RemoveAt(EmptyBlocksOffset.Count - 1);
            }
            else
            {
                address = (int)HashFile.Length;
                HashFile.SetLength(HashFile.Length + BlockSize);
            }
            node.Offset = address;
        }

        public void DisposeAndCloseFile()
        {
            HashFile.Dispose();
            HashFile.Close();
        }
    }
}
