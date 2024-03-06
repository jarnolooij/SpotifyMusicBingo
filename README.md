# Spotify Bingo Card Generator

## Overview

This C# program allows you to generate bingo cards based on the tracks in a Spotify playlist. It utilizes the Spotify API to retrieve playlist details and creates both standard bingo cards and a checker card.

## Prerequisites

Before using this program, make sure you have the following:

- **Spotify Developer App:** Create a Spotify Developer App to obtain your Client ID and Client Secret.

## How to Use

### Clone the Repository:

```bash
git clone https://github.com/yourusername/spotify-bingo-generator.git
```

## Update Spotify Developer Credentials:

1. Open `Program.cs` in a text editor.
2. Replace `your_spotify_dev_app_client_id` and `your_spotify_dev_app_client_secret` with your Spotify Developer App credentials.

## Build and Run:

Build and run the program using your preferred C# development environment.

## Enter Spotify Playlist ID:

When prompted, enter the Playlist ID. You can find the Playlist ID by taking the last part of the URL of the Spotify playlist.

## Check Output:

The generated bingo cards will be available in the `BingoCards` folder.

## Troubleshooting:

- If you encounter any issues, ensure that your Spotify Developer App credentials are correctly configured.
- Make sure the provided Playlist ID is valid.
