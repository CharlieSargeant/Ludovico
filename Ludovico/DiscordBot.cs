using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ludovico
{
    public class DiscordBot
    {
        public DiscordSocketClient Client { get; private set; }
        private string _botToken;

        public event Func<LogMessage, Task> LogReceived;
        public event Func<SocketMessage, Task> MessageReceived;
        public event Func<Task> Ready;

        public DiscordBot(string token)
        {
            _botToken = token;

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.GuildMessageReactions |
                                 GatewayIntents.DirectMessages |
                                 GatewayIntents.MessageContent
            };

            Client = new DiscordSocketClient(config);

            // Subscribe to client events and forward them
            Client.Log += log => LogReceived?.Invoke(log);
            Client.MessageReceived += msg => MessageReceived?.Invoke(msg);
            Client.Ready += () => Ready?.Invoke();
        }

        public async Task ConnectAsync()
        {
            await Client.LoginAsync(TokenType.Bot, _botToken);
            await Client.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            await Client.LogoutAsync();
            await Client.StopAsync();
        }
    }
}
