using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ludovico
{
    public class BotService
    {
        private readonly DiscordSocketClient _client;
        private readonly string _botToken;

        public event Func<LogMessage, Task>? LogReceived;
        public event Func<SocketMessage, Task>? MessageReceived;
        public event Func<Task>? Ready;

        public BotService(string token)
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

            _client = new DiscordSocketClient(config);

            // Subscribe to client events and forward them
            _client.Log += log => LogReceived?.Invoke(log);
            _client.MessageReceived += msg => MessageReceived?.Invoke(msg);
            _client.Ready += () => Ready?.Invoke();
        }

        public async Task ConnectAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _botToken);
            await _client.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }

        public IReadOnlyCollection<SocketGuild> GetGuilds() => _client.Guilds;

        public ITextChannel? GetChannel(ulong channelId)
        {
            return _client.GetChannel(channelId) as ITextChannel;
        }
    }
}
