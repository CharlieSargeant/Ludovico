using System.Collections.Generic;
using System.Windows;
using Discord;
using Discord.WebSocket;

namespace Ludovico
{
    public partial class MainWindow : Window
    {
        private BotService _botService;
        private BotEventHandler _botEventHandler;
        private bool _isConnected = false;

        // Guild and channel data
        private List<KeyValuePair<string, ulong>> _guilds = new();
        private Dictionary<ulong, List<ITextChannel>> _guildChannels = new();

        public MainWindow()
        {
            InitializeComponent();

            _botService = new BotService("Bot_Token");
            _botEventHandler = new BotEventHandler(this, _botService);

            GuildComboBox.SelectionChanged += GuildComboBox_SelectionChanged;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                await _botService.ConnectAsync();
                _isConnected = true;
                ConnectButton.Content = "Disconnect Bot";
            }
            else
            {
                await _botService.DisconnectAsync();
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

        public void SetupGuilds(IEnumerable<SocketGuild> guilds)
        {
            _guilds.Clear();
            _guildChannels.Clear();

            foreach (var guild in guilds)
            {
                _guilds.Add(new KeyValuePair<string, ulong>(guild.Name, guild.Id));
                var channels = new List<ITextChannel>(guild.TextChannels);
                _guildChannels[guild.Id] = channels;
            }

            GuildComboBox.ItemsSource = _guilds;
            GuildComboBox.DisplayMemberPath = "Key";
            GuildComboBox.SelectedValuePath = "Value";
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
                var channel = _botService.GetChannel(ulong.Parse(channelId));
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
