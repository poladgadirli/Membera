# Gateway dev HTTPS certificate

`docker-compose.yml` mounts this folder into the `gateway` container at `/https`.
Membera.Gateway serves HTTPS on container port 8443 (published as `https://localhost:7174`)
using `gateway.pfx` from here. Certificate files are git-ignored, so generate your own
after cloning.

## Generate

From `membera-backend/`, with the password you set as `GATEWAY_CERT_PASSWORD` in `.env`:

```sh
dotnet dev-certs https --trust
dotnet dev-certs https -ep certs/gateway.pfx -p <GATEWAY_CERT_PASSWORD>
```

This exports the ASP.NET Core development certificate (valid for `localhost`), which
`--trust` has made your OS and browser trust, so `https://localhost:7174` loads without warnings.
Then run `docker compose up -d gateway`.

## Regenerate (expired cert)

The dev certificate is valid for one year. Once it expires:

```sh
dotnet dev-certs https --clean
dotnet dev-certs https --trust
dotnet dev-certs https -ep certs/gateway.pfx -p <GATEWAY_CERT_PASSWORD>
```
