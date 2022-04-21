# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/yazilimcilarinmolayeri/pixels/compare/v0.0.3...HEAD
[0.0.3]: https://github.com/yazilimcilarinmolayeri/pixels/compare/v0.0.2...v0.0.3
[0.0.2]: https://github.com/yazilimcilarinmolayeri/pixels/compare/v0.0.1...v0.0.2
[0.0.1]: https://github.com/yazilimcilarinmolayeri/pixels/releases/tag/v0.0.1