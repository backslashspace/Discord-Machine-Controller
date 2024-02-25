using System;
using System.IO;
using System.Runtime.Serialization;

namespace LogViewer
{
    internal static partial class Program
    {
        public static Object Deserialize(Byte[] Data, Type DestinationObjectType)
        {
            using Stream Stream = new MemoryStream();

            Stream.Write(Data, 0, Data.Length);

            Stream.Position = 0;

            DataContractSerializer Deserializer = new(DestinationObjectType);

            return Deserializer.ReadObject(Stream);
        }
    }

    internal enum RequestType
    {
        SendPastLog = 0x11,
        SendUpdates = 0x22,
        DataNotAvailable = 0xfa,
        DataAvailable = 0xda
    }
}