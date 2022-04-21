<img width="200" src="https://i.imgur.com/XWnXKnU.png" align="right">

# Pixels

An application API that allows its users to set pixels on a canvas. Like Reddit Place but for devolopers and end users. Visit the [Yazılımcıların Mola Yeri](https://discord.gg/KazHgb2) server for more information and support. Good enjoy!

### Content

- [Features](#features)
- General
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Configuration](#configuration)
  - [Building](#building)
  - [Endpoints](#endpoints)
- Others
  - [Docs and Changelog](#docs-and-changelog)
  - [License](#license)

## Features

Soon...

## Prerequisites

- [Dotnet 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
  - You can install Dotnet 6.0 SDK via installer on Windows and macOS.
  - If you want to install Dotnet 6.0 SDK to GNU/Linux, check out [this](https://docs.microsoft.com/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website) page.

## Installation

You can clone the repository using:
```
$ git clone https://github.com/yazilimcilarinmolayeri/pixels.git
```
Or you can simply download the files [here](https://github.com/yazilimcilarinmolayeri/pixels/archive/refs/heads/master.zip)...

## Configuration

After you clone this repository or simply downloaded the files, you must fill in the required configuration fields in `appsettings.Development.json`. Then run the SQL script named `Structure.sql` on your **PostgreSQL** server.

You can change listening ports `Properties/launchSettings.json` Here is the default ports for each environment:

| Environment | HTTP | HTTPS |
| - | - | - |
| Development | `7000` | `7001` |
| Production | `5000` | `5001` |

## Building

You can build the project using launch profiles.

If you want to build the project using **development** environment, execute:
```shell
$ dotnet run --launch-profile "Development"
```
If you want to build the project using **production** environment, execute:
```shell
$ dotnet run --launch-profile "Production"
```

## Endpoints

There are few endpoints which users can use.
  
| Endpoint | Description |
| - | - |
| `/api/canvas` | Users can execute a `GET` request here to fetch currently active canvases image. |
| `/api/pixel` | Users can execute a `PUT` request here with a `SetPixelModel` object to put a pixel to currently active canvas. |
| `/api/pixel/{x}-{y}` | Users can execute a `GET` request here to fetch the pixel information on currently active canvas. |
| `/api/auth/login` | Users must login and get a `jwt` token from this endpoint in order to use the API. This endpoint simply redirects the user to Discord OAuth authentication page. |
| `/api/auth/discord/callback` | Users will come to this endpoint after they authenticate with their Discord account. This endpoint will authenticate them using a `jwt` token. |

Example of a `SetPixelModel` object: `{"x": 10, "y": 15, "color": "f30a2b"}`

## Docs and Changelog

Pixels documentation lives at X (not yet) and changelog lives at [changelog](CHANGELOG.md).

## License

This project is licensed under the MIT - see the [LICENSE](LICENSE.md) file for details.