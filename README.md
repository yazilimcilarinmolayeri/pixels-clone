# Pixels
> An application API that allows its users to set pixels on a canvas.

### Installation
- You can clone the repository using:
```
$ git clone https://github.com/yazilimcilarinmolayeri/pixels.git
```
- Or you can simply download the files [here](https://github.com/yazilimcilarinmolayeri/pixels/archive/refs/heads/master.zip)

### Configuration
After you clone this repository or simply downloaded the files, you must fill in the required
configuration fields in `appsettings.Development.json`. Then run the SQL script named
`Structure.sql` on your **PostgreSQL** server.

### Endpoints
There are few endpoints which users can use.
<details><summary>Show endpoints</summary>
<h4>/api/canvas</h4>
Users can execute a <i>GET</i> request here to fetch currently active canvases image.
<h4>/api/auth/login</h4>
Users must login and get a <i>jwt</i> token from this endpoint in order to use the API.
This endpoint simply redirects the user to Discord OAuth authentication page.
<h4>/api/auth/discord/callback</h4>
Users will come to this endpoint after they authenticate with their Discord account.
This endpoint will authenticate them using a <i>jwt</i> token.
<h4>/api/pixel/{x}-{y}</h4>
Users can execute a <i>GET</i> request here to fetch the pixel information on currently active canvas.
<h4>/api/pixel</h4>
Users can execute a <i>PUT</i> request here with a <i>SetPixelModel</i> object to put a pixel to currently active canvas.

Example of a *SetPixelModel* object:
```json
{
  "x": 10,
  "y": 15,
  "color": "f30a2b"
}
```
</details>