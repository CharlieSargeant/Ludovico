using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Diagnostics;


namespace Ludovico.Services
{
    public class TokenManager
    {
        private const string TokenFilePath = "Tokens/tokens.json";

        public List<string> Tokens { get; private set; }

        public TokenManager()
        {
            Tokens = LoadTokens();
        }

        // Load tokens from the JSON file
        public List<string> LoadTokens()
        {
            if (File.Exists(TokenFilePath))
            {
                var json = File.ReadAllText(TokenFilePath);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            return new List<string>();
        }

        // Save tokens to the JSON file
        public void SaveTokens()
        {
            // Ensure the directory exists
            string? directory = Path.GetDirectoryName(TokenFilePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(Tokens, new JsonSerializerOptions { WriteIndented = true });
            string path = Path.GetFullPath(TokenFilePath); // Get the full path of the file
            File.WriteAllText(path, json);
        }

        // Add a new token
        public void AddToken(string token)
        {
            Tokens.Add(token);
            SaveTokens();
        }
    }
}
