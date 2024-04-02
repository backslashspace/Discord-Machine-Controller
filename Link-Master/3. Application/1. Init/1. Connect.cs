using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;


namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        internal static async Task Connect()
        {
            try
            {
                DiscordSocketConfig config = new()
                {
                    GatewayIntents = GatewayIntents.Guilds,
                    //LogGatewayIntentWarnings = true,
                    //GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessages, //| GatewayIntents.GuildMembers
                    //LogLevel = xLogSeverity.Debug
                };

                Client.Discord = new DiscordSocketClient(config);

                Client.Discord.Log += Logging.Log.DiscordLogHandler;

                Client.Discord.Connected += GatewayConnected;
                Client.Discord.Disconnected += GatewayDisconnected;

                Client.Discord.Ready += OnReady;
                
                Client.Discord.SlashCommandExecuted += SlashCommandHandler;

                await Client.Discord.SetGameAsync("your government files", null, ActivityType.Watching);

                await Client.Discord.LoginAsync(TokenType.Bot, GetToken(), true);
                await Client.Discord.StartAsync();
            }
            catch (Exception ex)
            {
                Log.FastLog("Initiator", $"Failed to connect to discord more info:\n\n{ex.Message}\n\n\t=> terminating", xLogSeverity.Critical);

                Control.Shutdown.ServiceComponents();
            }
        }

        //

        private static String GetToken()
        {
            try
            {
                using FileStream fileStream = new(CurrentConfig.TokenPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                Byte[] rawToken = new Byte[96];

                fileStream.Read(rawToken, 0, rawToken.Length);

                fileStream.Close();

                String encodedToken = Encoding.UTF8.GetString(rawToken);
                Byte[] rawDecodedToken = Convert.FromBase64String(encodedToken);

                return Encoding.UTF8.GetString(rawDecodedToken);
            }
            catch (Exception ex)
            {
                Log.FastLog("Initiator", "Failed to load token from disk, terminating", xLogSeverity.Critical);
                Log.FastLog("Initiator", ex.Message, xLogSeverity.Verbose);

                Control.Shutdown.ServiceComponents();

                return null;
            }
        }
    }
}