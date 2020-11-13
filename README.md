# Schlauchboot.Hosting.SoundCloud

This Project aims to provide a simple to use REST-API to download Tracks from [SoundCloud](https://www.soundcloud.com).

## Usage

Sometimes, this API is available at [mphosting.ddnss.de](https://mphosting.ddnss.de/api/SoundCloud). If you want to try and use this API and do not mind to pass your client_id for authentication purposes, you can execute the following Call:

- Method: POST
- Uri: https://mphosting.ddnss.de/api/SoundCloud
- Body: { "clientId": "YOUR_SOUNDCLOUD_CLIENT_ID", "soundcloudUrl": "TRACK_URL" }

## Planned Tasks

- [X] Provide a way to download singular Songs
- [ ] Implement Logic that regards easily downloadable Tracks
- [X] Implement Playlist-Support
- [ ] Create an Endpoint for Premium-Accounts

## Additional Information

This Repository works in Combination with a Manager-Repository, which can be found [here](https://github.com/Schlauchboot/Schlauchboot.Hosting.SoundCloud.Manager).
