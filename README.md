# HTTP/2 Rapid Reset Client (C#)

The HTTP/2 Rapid Reset Client, implemented in C#, is designed for testing mitigations and assessing vulnerability to the CVE-2023-44487 (Rapid Reset DDoS attack vector). This client establishes a lone TCP socket, conducts TLS negotiation while disregarding certificates, and engages in the exchange of SETTINGS frames. Subsequently, the client swiftly dispatches HEADERS frames, succeeded by RST_STREAM frames. It actively monitors server frames post-initial setup and outputs them to the console.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)

## Installation

### Clone the Repository
```
git clone https://github.com/terrorist/HTTP-2-Rapid-Reset-Client.git
```

### Installing
```
cd Http2Attack

// make sure to change the hard coded arguments before building the .exe
dotnet build -o Http2Attack
```

### Hard coded arguments

- `requests`: The count of requests to be sent (default is 5)

- `url`: The URL of the server (default is https://localhost:443)

- `wait`: The time, in milliseconds, to wait between starting workers (default is 0)

- `delay`: The delay, in milliseconds, between sending HEADERS and RST_STREAM frames (default is 0)

- `concurrency`: The maximum number of concurrent workers (default is 0)

## Built With

- [System.Net.Http](https://docs.microsoft.com/en-us/dotnet/api/system.net.http) - .NET library for sending HTTP requests and receiving HTTP responses.

## License

This project is licensed under the Apache License - see the [LICENSE](LICENSE) file for details

## Acknowledgments

This work is based on the [initial analysis of CVE-2023-44487](https://cloud.google.com/blog/products/identity-security/how-it-works-the-novel-http2-rapid-reset-ddos-attack) by Juho Snellman and  Daniele Iamartino at Google.
