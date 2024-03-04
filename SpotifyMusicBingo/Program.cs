using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata;
using Document = iText.Layout.Document;

class Program
{
    //created by Jarno Looij
    //create a spotify dev app at https://developer.spotify.com/


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
            (string[] trackNames, string playlistName) = await GetPlaylistDetails(accessToken, playlistId);

            Console.WriteLine($"\nPlaylist Tracks for {playlistName}:");
            foreach (var trackName in trackNames)
            {
                Console.WriteLine(trackName);
            }

            GenerateBingoCards(trackNames, playlistName);
        }
        else
        {
            Console.WriteLine("Failed to get access token.");
        }
    }

    static async Task<string> GetAccessToken(string clientId, string clientSecret)
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

    static async Task<(string[], string)> GetPlaylistDetails(string accessToken, string playlistId)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var playlistResponse = await httpClient.GetStringAsync($"https://api.spotify.com/v1/playlists/{playlistId}");
            var playlistJson = JObject.Parse(playlistResponse);

            string playlistName = playlistJson["name"].ToString();

            var tracksResponse = await httpClient.GetStringAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks");
            var jsonResponse = JObject.Parse(tracksResponse);

            string[] trackNames = jsonResponse["items"]
                .Select(item => item["track"]["name"].ToString())
                .ToArray();

            return (trackNames, playlistName);
        }
    }

    static void GenerateBingoCards(string[] trackNames, string playlistName)
    {
        const int cardsCount = 30;
        const int rows = 5;
        const int columns = 5;
        var fileName = $"{playlistName}BingoCards.pdf";

        if (trackNames.Length < rows * columns)
        {
            Console.WriteLine("Not enough songs for a bingo card. Please add more songs to your playlist.");
            return;
        }

        using (var writer = new PdfWriter(fileName))
        using (var pdf = new PdfDocument(writer))
        {
            var document = new Document(pdf);

            for (int cardNumber = 1; cardNumber <= cardsCount; cardNumber++)
            {
                DrawBingoCard(document, trackNames, rows, columns);
                document.Add(new AreaBreak());
            }

            Console.WriteLine($"All bingo cards generated successfully. Check {fileName}");
        }
    }

    static void DrawBingoCard(Document document, string[] trackNames, int rows, int columns)
    {
        const int cellWidth = 100;
        const int cellHeight = 50;
        const int margin = 20;

        var shuffledTracks = trackNames.OrderBy(x => Guid.NewGuid()).ToArray();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int index = (row * columns) + col;

                if (index < shuffledTracks.Length)
                {
                    string trackName = shuffledTracks[index];

                    var paragraph = new Paragraph(trackName)
                        .SetWidth(cellWidth)
                        .SetHeight(cellHeight)
                        .SetMarginLeft(margin + (col * cellWidth))
                        .SetMarginTop(margin + (row * cellHeight))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(10);

                    document.Add(paragraph);
                }
            }
        }
    }
}