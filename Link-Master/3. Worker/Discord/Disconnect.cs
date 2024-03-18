namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        internal static void Disconnect()
        {
            try
            {
                Client.Discord.LogoutAsync().Wait();
                Client.Discord.StopAsync().Wait();
                Client.Discord.Dispose();
            }
            catch { }
        }
    }
}