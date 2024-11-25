using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Discord;
using Discord.WebSocket;

namespace Ludovico
{
    public partial class MainWindow : Window
    {
        private DiscordBot _bot;
        private bool _isConnected = false;

        // Guild and channel data
        private List<KeyValuePair<string, ulong>> _guilds = new();
        private Dictionary<ulong, List<ITextChannel>> _guildChannels = new();

        public MainWindow()
        {
            InitializeComponent();

            _bot = new DiscordBot("Bot_Token");
            _bot.LogReceived += LogAsync;
            _bot.MessageReceived += MessageReceivedAsync;
            _bot.Ready += BotReadyAsync;

            GuildComboBox.SelectionChanged += GuildComboBox_SelectionChanged;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                await _bot.ConnectAsync();
                _isConnected = true;
                ConnectButton.Content = "Disconnect Bot";
            }
            else
            {
                await _bot.DisconnectAsync();
                _isConnected = false;
                ConnectButton.Content = "Connect Bot";
            }
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            if (!string.IsNullOrEmpty(message) && _isConnected)
            {
                var selectedChannelId = ChannelComboBox.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(selectedChannelId))
                {
                    await SendMessageAsync(selectedChannelId, message);
                    MessageTextBox.Clear();
                }
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Dispatcher.Invoke(() => LogTextBox.AppendText(log.Message + "\n"));
            return Task.CompletedTask;
        }

        private Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot) return Task.CompletedTask;
            Dispatcher.Invoke(() => LogTextBox.AppendText($"Message received: {message.Content}\n"));
            return Task.CompletedTask;
        }

        private Task BotReadyAsync()
        {
            Dispatcher.Invoke(() =>
            {
                _guilds.Clear();
                _guildChannels.Clear();

                foreach (var guild in _bot.Client.Guilds)
                {
                    _guilds.Add(new KeyValuePair<string, ulong>(guild.Name, guild.Id));
                    var channels = new List<ITextChannel>(guild.TextChannels);
                    _guildChannels[guild.Id] = channels;
                }

                GuildComboBox.ItemsSource = _guilds;
                GuildComboBox.DisplayMemberPath = "Key";
                GuildComboBox.SelectedValuePath = "Value";
            });
            return Task.CompletedTask;
        }

        private void GuildComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GuildComboBox.SelectedValue is ulong guildId)
            {
                ChannelComboBox.ItemsSource = _guildChannels.TryGetValue(guildId, out var channels) ? channels : null;
                ChannelComboBox.DisplayMemberPath = "Name";
                ChannelComboBox.SelectedValuePath = "Id";
            }
        }

        private async Task SendMessageAsync(string channelId, string message)
        {
            try
            {
                var channel = _bot.Client.GetChannel(ulong.Parse(channelId)) as ITextChannel;
                if (channel != null)
                {
                    await channel.SendMessageAsync(message);
                    LogTextBox.AppendText($"Sent message: {message}\n");
                }
                else
                {
                    LogTextBox.AppendText("Error: Channel not found.\n");
                }
            }
            catch (Exception ex)
            {
                LogTextBox.AppendText($"Error: {ex.Message}\n");
            }
        }
    }
}
