using System.Collections;

namespace AUS2_Sem2.DynamicHash
{
    public interface IRecord<T>
    {
        bool Equals(T key);
        BitArray GetHash();
        int GetSize();
        byte[] ToByteArray();
        void FromByteArray(byte[] array);
        T CreateClass();
        List<string> GetStrings();
    }
}
