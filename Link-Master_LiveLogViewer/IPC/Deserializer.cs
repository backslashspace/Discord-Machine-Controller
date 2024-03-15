using System;
using System.IO;
using System.Runtime.Serialization;

namespace LogViewer
{
    internal static partial class Program
    {
        private static Object Deserialize(Byte[] data, Type type)
        {
            using MemoryStream stream = new();

            stream.Write(data, 0, data.Length);

            stream.Position = 0;

            DataContractSerializer deserializer = new(type);

            return deserializer.ReadObject(stream);
        }
    }
}