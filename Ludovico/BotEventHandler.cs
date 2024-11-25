using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Ludovico
{
    public class BotEventHandler
    {
        private readonly MainWindow _mainWindow;
        private readonly BotService _botService;

        public BotEventHandler(MainWindow mainWindow, BotService botService)
        {
            _mainWindow = mainWindow;
            _botService = botService;

            _botService.LogReceived += LogAsync;
            _botService.MessageReceived += MessageReceivedAsync;
            _botService.Ready += BotReadyAsync;
        }

        private Task LogAsync(LogMessage log)
        {
            _mainWindow.Dispatcher.Invoke(() => _mainWindow.LogTextBox.AppendText(log.Message + "\n"));
            return Task.CompletedTask;
        }

        private Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot) return Task.CompletedTask;
            _mainWindow.Dispatcher.Invoke(() => _mainWindow.LogTextBox.AppendText($"Message received: {message.Content}\n"));
            return Task.CompletedTask;
        }

        private Task BotReadyAsync()
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                var guilds = _botService.GetGuilds();
                _mainWindow.SetupGuilds(guilds);
            });
            return Task.CompletedTask;
        }
    }
}
