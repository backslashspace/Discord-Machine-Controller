using System.IO;
using System.Text;

namespace Link_Master.Control
{
    internal static partial class ConfigLoader
    {
        private static void CreateConfigTemplate()
        {
            using StreamWriter streamWriter = new($"{Program.AssemblyPath}\\config.txt", true, Encoding.UTF8);

            streamWriter.WriteLine("tokenPath: \"C:\\bot\\encodedTokenFile\"");
            streamWriter.WriteLine("discordAdminUserID: \"456456456456546\"");
            streamWriter.WriteLine("cmdRegisterOnBoot: \"false\"");
            streamWriter.WriteLine("tcpListenerIP: \"10.0.0.20\"");
            streamWriter.WriteLine("tcpListenerPort: \"9001\"");
            streamWriter.WriteLine("");
            streamWriter.WriteLine("guildID: \"344564656\"");
            streamWriter.WriteLine("discordGatewayDebugMode: \"false\"");
            streamWriter.WriteLine("");
            streamWriter.WriteLine("logChannelID: \"65675677\"");
            streamWriter.WriteLine("discordAnnounceEndpointConnect: \"true\"");
            streamWriter.WriteLine("");
            streamWriter.WriteLine("# Name : Guid : ScriptTimeOut : ChannelID : Base64 {hmac + aes key}");
            streamWriter.WriteLine("vmChannelLink: \"VM1Name:Guid:TimeOut:ChannelID:EncryptionKeys\"");
            streamWriter.WriteLine("vmChannelLink: \"VM2Name:Guid:TimeOut:ChannelID:EncryptionKeys\"");
            streamWriter.WriteLine("vmChannelLink: \"VM3Name:Guid:TimeOut:ChannelID:EncryptionKeys\"");
        }
    }
}