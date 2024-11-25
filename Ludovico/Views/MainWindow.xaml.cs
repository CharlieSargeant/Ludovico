using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Discord;
using Discord.WebSocket;
using Ludovico.Services;

namespace Ludovico.Views
{
    public partial class MainWindow : Window
    {
        private BotService _botService;
        private BotEventHandler _botEventHandler;
        private TokenManager _tokenManager;
        private bool _isConnected = false;

        // Guild and channel data
        private List<KeyValuePair<string, ulong>> _guilds = new();
        private Dictionary<ulong, List<ITextChannel>> _guildChannels = new();

        public MainWindow()
        {
            InitializeComponent();

            _tokenManager = new TokenManager();
            // Initialize BotService with the first token or null if no tokens are loaded
            _botService = new BotService(_tokenManager.Tokens.FirstOrDefault() ?? string.Empty);
            _botEventHandler = new BotEventHandler(this, _botService);

            GuildComboBox.SelectionChanged += GuildComboBox_SelectionChanged;

            SetupTokens();
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

        private void AddToken_Click(object sender, RoutedEventArgs e)
        {
            // Show a dialog to enter a new token
            var tokenInput = Microsoft.VisualBasic.Interaction.InputBox("Enter Bot Token:", "Add Token");
            if (!string.IsNullOrWhiteSpace(tokenInput))
            {
                _tokenManager.AddToken(tokenInput);
                SetupTokens();
            }
        }

        private void LoadTokens_Click(object sender, RoutedEventArgs e)
        {
            // Display the list of loaded tokens
            string tokenList = string.Join("\n", _tokenManager.Tokens);
            MessageBox.Show(tokenList, "Loaded Tokens");
        }

        private void TokenComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TokenComboBox.SelectedItem is string selectedToken)
            {
                // Update BotService with the new token
                _botService = new BotService(selectedToken);

                // Reinitialize BotEventHandler with the new BotService
                _botEventHandler = new BotEventHandler(this, _botService);

                // Optionally, reconnect with the new token if already connected
                if (_isConnected)
                {
                    ConnectButton_Click(sender, e);
                }
            }
        }


        public void SetupTokens()
        {
            TokenComboBox.ItemsSource = _tokenManager.Tokens;
            TokenComboBox.SelectedIndex = 0;
        }
    }
}
