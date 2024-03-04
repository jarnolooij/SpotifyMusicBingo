using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    private const string ClientId = "your_spotify_dev_app_client_id";
    private const string ClientSecret = "your_spotify_dev_app_client_secret";

    static async Task Main()
    {
        Console.Write("Enter the Spotify Playlist ID: ");
        string playlistId = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(playlistId))
        {
            Console.WriteLine("Playlist ID cannot be empty. Exiting program.");
            return;
        }

        string accessToken = await GetAccessToken(ClientId, ClientSecret);

        if (accessToken != null)
        {
            string[] trackNames = await GetPlaylistTracks(accessToken, playlistId);

            Console.WriteLine("\nPlaylist Tracks:");
            foreach (var trackName in trackNames)
            {
                Console.WriteLine(trackName);
            }
        }
        else
        {
            Console.WriteLine("Failed to get access token.");
        }
    }

    private static async Task<string> GetAccessToken(string clientId, string clientSecret)
    {
        using (var httpClient = new HttpClient())
        {
            string credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);
                string accessToken = json.Value<string>("access_token");
                return accessToken;
            }
            else
            {
                return null;
            }
        }
    }

    private static async Task<string[]> GetPlaylistTracks(string accessToken, string playlistId)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            var response = await httpClient.GetStringAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks");
            var jsonResponse = JObject.Parse(response);
            string[] trackNames = jsonResponse["items"]
                .Select(item => item["track"]["name"].ToString())
                .ToArray();

            return trackNames;
        }
    }
}