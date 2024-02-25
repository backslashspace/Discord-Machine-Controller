using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        internal static DiscordSocketClient dc_client;

        internal static Boolean IsConnected;

        internal static void SRV_Main()
        {
            FastLog("Win32", $"Service start initiated v{Service.Program.Version}", LogSeverity.Info).Wait();

            ConfigLoader.Load();

            Initiate();

            Start();
        }

        //

        private static void Initiate()
        {
            Thread localIPCThread = new(() => IPCAdapter.LogClientHandler(CurrentConfig.consoleUser))
            {
                IsBackground = true,
                Name = "IPCAdapter - Log"
            };
            localIPCThread.Start();

            //

            DiscordSocketConfig config = new()
            {
                GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessages //| GatewayIntents.GuildMembers
            };

            dc_client = new DiscordSocketClient(config);

            dc_client.Connected += GatewayConnected;
            dc_client.Disconnected += GatewayDisconnected;

            dc_client.Ready += OnReady;
            dc_client.Log += DCLogHandler;
            dc_client.SlashCommandExecuted += SlashCommandHandler;

            dc_client.SetGameAsync("your government files", null, ActivityType.Watching).Wait();
        }

        private static void Start()
        {
            dc_client.LoginAsync(TokenType.Bot, GetToken()).Wait();
            dc_client.StartAsync().Wait();
        }

        //

        private static String GetToken()
        {
            try
            {
                FileStream fileStream = new(CurrentConfig.tokenPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                Byte[] rawToken = new Byte[96];

                fileStream.Read(rawToken, 0, rawToken.Length);

                fileStream.Close();
                fileStream.Dispose();

                return Encoding.ASCII.GetString(Convert.FromBase64String(Encoding.ASCII.GetString(rawToken)));
            }
            catch (Exception ex)
            {
                FastLog("Initiator", $"{Lang.log_critical_no_token} 5120ms", LogSeverity.Critical).Wait();
                FastLog("Initiator", ex.Message, LogSeverity.Verbose).Wait();

                Exit.Service();

                return null;
            }
        }
    }
}