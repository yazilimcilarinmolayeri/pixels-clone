# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.6] - 2022-04-22
### Added
- *Heatmap* endpoint (GET request to `/api/canvas/{canvasId}/heatmap`)
- *Snapshot* endpoint (GET request to `/api/canvas/{canvasId}/snapshot/{timestamp}`)
- 2 overloads for *BuildPixels* method
- *Snapshot* class
- *GetActionsBetweenDates* method to `Utilities/Data.cs`
- *GetSnapshots* method to `Utilities/Data.cs`
- *pixelSnapshot* column to table 'actions'
### Changed
- Updated *GET_CURRENT_CANVAS* view
- Updated *InsertAction* method
- Updated *Action* class
- Updated SetPixel endpoint (PUT request to `/api/pixel/`)

## [0.0.5] - 2022-04-21
### Added
- *UpdateCanvas* method to `Utilities/Data.cs`
- Update canvas endpoint (PATCH request to `/api/canvas`)
- New canvas endpoint (PUT request to `/api/canvas`)
### Changed
- *NewCanvas* method on `Utilities/Data.cs`. Return type is *Task<int>* now.
- Get canvas endpoint. Users can view a specific canvas now.
- Updated README.md

## [0.0.4] - 2022-04-21
### Added
- `/api/user/{discordId}/ban` endpoint for moderators to allow banning users.
- *UpdateUser* method to `Utilities/Data.cs`

## [0.0.3] - 2022-04-21
### Added
- Websocket middleware for sending pixel changes live to listeners
- CODE_OF_CONDUCT.md
- CONTRIBUTORS.md
- Issue templates
- Pull request template
### Changed
- `expire_time` field's type to number on route `/api/auth/discord/callback`
### Removed
- *WebSocketController.cs* because added `Middleware/WebSocketMiddleware.cs`

## [0.0.2] - 2022-04-21
### Added
- TODO comments for future updates
- This changelog
### Changed
- Canvas endpoint is returning canvas info when sent accept header is `application/json`
### Removed
- `isActive` column is removed from `canvas` table on database

## [0.0.1] - 2022-04-20
### Added
- Foundation elements like set pixel, get pixel, get canvas as image, login with discord,
  get jwt token
- This CHANGELOG file to hopefully serve as an evolving example of a
  standardized open source project CHANGELOG.

[Unreleased]: https://github.com/yazilimcilarinmolayeri/pixels/compare/v0.0.6...HEAD
[0.0.6]: https://github.com/yazilimcilarinmolayeri/pixels/compare/v0.0.5...v0.0.6
[0.0.5]: https://github.com/yazilimcilarinmolayeri/pixels/compare/v0.0.4...v0.0.5
[0.0.4]: https://github.com/yazilimcilarinmolayeri/pixels/compare/v0.0.3...v0.0.4
[0.0.3]: https://github.com/yazilimcilarinmolayeri/pixels/compare/v0.0.2...v0.0.3
[0.0.2]: https://github.com/yazilimcilarinmolayeri/pixels/compare/v0.0.1...v0.0.2
[0.0.1]: https://github.com/yazilimcilarinmolayeri/pixels/releases/tag/v0.0.1