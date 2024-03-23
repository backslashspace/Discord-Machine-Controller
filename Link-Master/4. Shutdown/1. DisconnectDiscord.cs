using Discord;
using System;

namespace Link_Master.Control
{
    internal static partial class Shutdown
    {
        private static void DisconnectDiscord(ref Boolean unsafeShutdown)
        {
            Logging.Log.IgnoreNewDiscord = true;

            lock (Logging.Log.DiscordLogQueue_LOCK)
            {
                Logging.Log.DiscordLogQueue.Clear();
            }

            if (unsafeShutdown)
            {
                SendDiscordGoodbye(Color.Red, "Error => service shutting down, read logs for more information");
            }
            else
            {
                SendDiscordGoodbye(Color.Orange, "Service shutting down");
            }

            Client.IsConnected = false;

            Log.FastLog("Shutdown", "Disconnecting from discord", LogSeverity.Info);

            try
            {
                Client.Discord.LogoutAsync().Wait();
                Client.Discord.StopAsync().Wait();
                Client.Discord.Dispose();
            }
            catch { }
        }

        private static void SendDiscordGoodbye(Color color, String message)
        {
            EmbedBuilder sendGoodbye = new()
            {
                Color = color,
                Description = message
            };

            try
            {
                CurrentConfig.LogChannel.SendMessageAsync(embed: sendGoodbye.Build()).Wait();
            }
            catch { }
        }
    }
}