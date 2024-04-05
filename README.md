# Zenon Wallet API for .NET

A .NET based Wallet API for interacting with Zenon Alphanet - Network of Momentum Phase 1

## Requirements

- [Microsoft .NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Dependencies

- [Zenon SDK for .NET](https://github.com/hypercore-one/znn_sdk_csharp)

## Build on Linux

``` bash
sudo apt-get update && sudo apt-get install -y dotnet8

git clone https://github.com/hypercore-one/znn_walletapi_csharp.git

cd znn_walletapi_csharp/src/ZenonWalletApi

export Api__Jwt__Secret=""
export Api__Utilities__PlasmaBot__ApiKey=""

dotnet build -c Release

../../bin/ZenonWalletApi/release/net8.0/ZenonWalletApi --environment Production --urls https://localhost:443
```

## Contributing

Please check [CONTRIBUTING](./CONTRIBUTING.md) for more details.

## License

The MIT License (MIT). Please check [LICENSE](./LICENSE) for more information.