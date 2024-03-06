using iText.IO.Util;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Document = iText.Layout.Document;

class Program
{
    //Created by Jarno Looij
    //Create a spotify dev app at https://developer.spotify.com/
    //Insert the last part (playlist id) of the url of a playlist into the console on runtime and press enter

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
        var outputFolder = "..\\..\\..\\BingoCards";
        var fileName = $"{PlaylistRegexConverter(playlistName)} Bingo Cards.pdf";
        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), outputFolder, fileName);

        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), outputFolder)))
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), outputFolder));
        }

        if (trackNames.Length < rows * columns)
        {
            Console.WriteLine("Not enough songs for a bingo card. Please add more songs to your playlist.");
            return;
        }

        using (var writer = new PdfWriter(outputPath))
        using (var pdf = new PdfDocument(writer))
        using (var document = new Document(pdf))
        {
            for (int cardNumber = 1; cardNumber <= cardsCount; cardNumber++)
            {
                DrawBingoCard(document, trackNames, rows, columns, playlistName, cardNumber);
                document.Add(new AreaBreak());
            }

            Console.WriteLine($"All bingo cards generated successfully. Check {outputPath}");
        }
    }

    static void DrawBingoCard(iText.Layout.Document document, string[] trackNames, int rows, int columns, string playlistName, int cardNum)
    {
        const int cellWidth = 100;
        const int cellHeight = 100;
        const int headerFontSize = 24;
        const int subheaderFontSize = 16;
        const float spacingAfterSubheader = 75f;
        const float spacingBeforeheader = 50f;

        var shuffledTracks = trackNames.OrderBy(x => Guid.NewGuid()).ToArray();

        var headerParagraph = new Paragraph(playlistName)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginTop(spacingBeforeheader)
            .SetFontSize(headerFontSize);
        document.Add(headerParagraph);

        var subheaderParagraph = new Paragraph($"Bingo Card {cardNum}")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(subheaderFontSize);
        document.Add(subheaderParagraph);

        document.Add(new Paragraph().SetMarginBottom(spacingAfterSubheader));

        document.Add(new Paragraph());

        Table table = new Table(columns, false);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int index = (row * columns) + col;

                if (index < shuffledTracks.Length)
                {
                    string trackName = shuffledTracks[index];

                    Cell cell = new Cell()
                        .SetWidth(cellWidth)
                        .SetHeight(cellHeight)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetFontSize(10)
                        .Add(new Paragraph(trackName))
                        .SetBorder(new SolidBorder(ColorConstants.BLACK, 1));

                    table.AddCell(cell);
                }
            }
        }

        document.Add(table);
    }

    static string PlaylistRegexConverter(string playlistName)
    {
        string pattern = "[^a-zA-Z0-9_\\s\\-\\.]+";
        string convertedName = Regex.Replace(playlistName, pattern, "");

        return convertedName;
    }
}