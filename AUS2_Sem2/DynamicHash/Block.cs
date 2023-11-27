namespace AUS2_Sem2.DynamicHash
{
    public class Block<T> where T : IRecord<T>
    {
        public int BlockFactor { get; set; }
        public int ValidCount { get; set; }
        public List<T> Records { get; set; }

        public Block(int blockFactor)
        {
            BlockFactor = blockFactor;
            Records = new List<T>(blockFactor);
            for (int i = 0; i < blockFactor; i++)
            {
                Records.Add(Activator.CreateInstance<T>().CreateClass());  // crashuje
            }
            ValidCount = 0;
        }

        public int GetSize()
        {
            return sizeof(int) + Records[0].GetSize() * BlockFactor;
        }

        public byte[] ToByteArray()
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(ValidCount);
                    foreach (var record in Records)
                    {
                        bw.Write(record.ToByteArray());
                    }
                }
                return ms.ToArray();
            }
        }

        public void FromByteArray(byte[] array)
        {
            ValidCount = BitConverter.ToInt32(array, 0);
            for (int i = 0; i < BlockFactor; i++)
            {
                Records[i].FromByteArray(array.Skip(sizeof(int) + i * Records[i].GetSize()).Take(Records[i].GetSize()).ToArray());
            }
        }

        public bool Insert(T record)
        {
            for (int i = 0; i < ValidCount; i++)
            {
                if (Records[i].Equals(record))
                {
                    return false;
                }
            }

            if (ValidCount < BlockFactor)
            {
                Records[ValidCount] = record;
                ValidCount++;
                return true;
            }

            return false;
        }

        public bool Remove(T record)
        {
            for (int i = 0; i < ValidCount; i++)
            {
                if (Records[i].Equals(record))
                {
                    Records[i] = Records[ValidCount];
                    Records[ValidCount] = Activator.CreateInstance<T>().CreateClass();
                    ValidCount--;
                    return true;
                }
            }
            return false;
        }
    }
}
