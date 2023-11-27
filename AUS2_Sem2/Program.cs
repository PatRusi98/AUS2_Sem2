using AUS2_Sem2.DynamicHash;
using System.Collections;

class Program
{
    static void Main()
    {
        int blockFactor = 4;
        string fileName = "HashData.dat";

        DynamicHash<TestRecord> dynamicHash = new DynamicHash<TestRecord>(fileName, blockFactor);
        TestInsertion(dynamicHash, 10000);
        TestFinding(dynamicHash, 10000);
        dynamicHash.DisposeAndCloseFile();
    }

    static void TestInsertion(DynamicHash<TestRecord> dynamicHash, int numberOfRecords)
    {
        Random random = new Random();

        for (int i = 0; i < numberOfRecords; i++)
        {
            TestRecord record = CreateRandomRecord();
            bool inserted = dynamicHash.Insert(record);

            if (inserted)
            {
                Console.WriteLine($"Record {i + 1} inserted successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to insert Record {i + 1}.");
            }
        }
    }

    static void TestFinding(DynamicHash<TestRecord> dynamicHash, int numberOfRecords)
    {
        Random random = new Random();

        for (int i = 0; i < numberOfRecords; i++)
        {
            TestRecord record = CreateRandomRecord();

            TestRecord foundRecord = dynamicHash.Find(record);

            if (foundRecord != null)
            {
                Console.WriteLine($"Record {i + 1} found successfully.");
            }
            else
            {
                Console.WriteLine($"Record {i + 1} not found.");
            }
        }
    }

    static TestRecord CreateRandomRecord()
    {
        int randomValue = new Random().Next(1000);
        return new TestRecord(randomValue);
    }
}



class TestRecord : IRecord<TestRecord>
{
    private int key;

    public TestRecord(int key)
    {
        this.key = key;
    }

    public bool Equals(TestRecord other)
    {
        return this.key == other.key;
    }

    public BitArray GetHash()
    {
        return new BitArray(BitConverter.GetBytes(key));
    }

    public int GetSize()
    {
        return sizeof(int);
    }

    public byte[] ToByteArray()
    {
        return BitConverter.GetBytes(key);
    }

    public void FromByteArray(byte[] array)
    {
        this.key = BitConverter.ToInt32(array, 0);
    }

    public TestRecord CreateClass()
    {
        return new TestRecord(0);
    }

    public List<string> GetStrings()
    {
        return new List<string> { $"Key: {key}" };
    }
}