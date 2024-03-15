namespace LogViewer
{
    internal enum RequestType
    {
        SendPastLog = 0x11,
        SendUpdates = 0x22,
        DataNotAvailable = 0xfa,
        DataAvailable = 0xda,
        Goodbye = 0xdd
    }
}