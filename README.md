[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=alert_status&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=bugs&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=code_smells&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=coverage&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=duplicated_lines_density&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=ncloc&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=reliability_rating&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=security_rating&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=sqale_index&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=sqale_rating&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=xxmaku_sirccli&metric=vulnerabilities&token=6bd6eb88a7cc21ce1b19f0c60df6847b3127551a)](https://sonarcloud.io/summary/new_code?id=xxmaku_sirccli)

# SIRCCELI (Secure Internet Relay Chat Client)

## Description

SIRCCELI is a crude [IRC](#irc) client implementation trying to combine secure development practices with a small-scale project while also learning something new. This is not meant to be a fully featured client, and rather focuses on basic functionality (connecting to a server, joining channels, sending messages, etc.).

Security is mainly being focused through DevOps practices, such as static code analysis, unit testing, and CI/CD pipelines. 

The project should be able to support multiple operating systems, but has not been extensively tested on other platforms than Linux. 

Containerization is a remnant from an earlier stage of development, and is not currently being used for anything. Seeing as utilising containers for a desktop GUI application is somewhat impractical, it is unlikely that this will change in the future.

## Building and Running
Requirements:
- Dotnet 10.0 SDK or later.
- A personal computer that's preferrably not as old as the IRC protocol itself. Dotnet runtime is heavier than optimised C code.

To build and run the application, simply run the following commands in the terminal:

```bash
# Clone the repository
git clone
# Navigate to the project directory
cd sirccli
# Build the application
dotnet build
# Run the application
dotnet run --project sircceli.desktop
```

There is a readily made configuration file in `sircceli/appsettings.json` that can be used to connect to a server and join channels with prefilled values. These values are not required, and can be changed in the application itself. The configuration file exists for default values and ease of testing, and is not meant to be a secure way of storing credentials. If using the prefilled values, it is enough to send a `/connect` command without any parameters to connect to the server and join the channels.

**NOTE:** Using IRC requires some ports being allowed through the firewall, such as 6667 for unencrypted connections and 6697 for encrypted connections. Using a public Wi-Fi network may block these ports, effectively preventing the application from connecting to any servers. During the development of this application, this was discovered on a long train ride, which caused a headache for testing. If you are having trouble connecting to servers, try using a different network or a VPN.

### Glossary

#### IRC

**IRC** = Internet Relay Chat, real-time messaging protocol. Refer to [RFC 1459](https://datatracker.ietf.org/doc/html/rfc1459) for more thorough details and [RFC 2812](https://datatracker.ietf.org/doc/html/rfc2812) for an update on the protocol.
