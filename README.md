[![build](https://img.shields.io/github/actions/workflow/status/hypercore-one/znn_walletapi_csharp/ci.yml)](https://github.com/hypercore-one/znn_walletapi_csharp/actions/workflows/ci.yml)

# Zenon Wallet API for .NET

The Zenon Wallet API for .NET is a cross-platform solution designed to interface with the Zenon Alphanet. This API facilitates various operations such as accessing blockchain data, managing wallets, and submitting transactions. It's built to be compatible with Microsoft .NET 8.0 and requires a Zenon Node for full functionality.

Key features of the API include:

- Blockchain Interaction: Users can retrieve blockchain data and submit transactions directly through the API.
- Wallet Management: Provides functionalities for wallet creation, management, and interaction with the Zenon network.
- Extensive Documentation and Testing: The API comes with detailed documentation and a Swagger UI for easy testing and interaction.
- Configuration and Security: It supports JWT bearer authentication for secure access, and its configuration is customizable to meet various user needs.
- Development and Contribution: Designed for .NET developers, it also welcomes contributions from the community to enhance its capabilities.

## Requirements

- [Microsoft .NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Zenon Node](https://github.com/zenon-network/go-zenon) - [Setup Instructions](https://forum.hypercore.one/t/how-to-setup-a-public-node/422)

## Dependencies

- [Serilog logging for ASP.NET Core](https://github.com/serilog/serilog-aspnetcore)
- [Zenon SDK for .NET](https://github.com/hypercore-one/znn_sdk_csharp)

## Documentation

To explore the endpoints without setting up your own Wallet API, you can visit the [online Wallet API](https://walletapi.hypercore.one). You can call the endpoints via the web interface in your browser by clicking Try it out.

> Note: to access protected API routes, a user account is required. Please contact us if you want to try it out.

When the API is installed, configured and running, a user-friendly Swagger UI is available at `https://localhost:443/swagger/`.

## Installation on Linux

``` bash
sudo apt-get update && sudo apt-get install -y dotnet8

git clone https://github.com/hypercore-one/znn_walletapi_csharp.git

cd znn_walletapi_csharp/src/ZenonWalletApi

export Api__Jwt__Secret="[SECRET KEY]"
export Api__Utilities__PlasmaBot__ApiKey="[API KEY]"

dotnet build -c Release

../../bin/ZenonWalletApi/release/net8.0/ZenonWalletApi --environment Production --urls https://localhost:443
```

## Configuration

The following documentation explains how to configure the Zenon Wallet API.

Visit [Microsoft's Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0) for more information on how to configure ASP.NET Core.

Visit [Serilog's Logging for ASP.NET Core](https://github.com/serilog/serilog-settings-configuration) for more information on how to configure Serilog logging for ASP.NET Core.

### Authentication

The Zenon Wallet API uses [JWT](https://en.wikipedia.org/wiki/JSON_Web_Token) bearer authentication. The tokens hold claims and are signed using a private secret key.

By default tokens do not expire and require a private secret to be configured. 

**Configuration**

Use the `Api:Jwt` configuration section to configure Jwt authentication.

| Name                                   | Description                                                  | Default                     |
| -------------------------------------- | :----------------------------------------------------------- | --------------------------- |
| `Secret` **string** *required*         | Sets the issuer security key that is to be used for signature validation. |                             |
| `ValidIssuer` **string** *optional*    | Sets a string that represents a valid issuer that will be used to check against the token's issuer. | `"walletapi.zenon.network"` |
| `ValudAudience` **string** *optional*  | Sets a string that represents a valid audience that will be used to check against the token's audience. | `"zenon.network"`           |
| `ExpiresOn` **datetime** *optional*    | Sets the expiration datetime in UTC when tokens expire. Cannot be used simultaneously with `ExpiresAfter`. | `null`                      |
| `ExpiresAfter` **timespan** *optional* | Sets the expiration timespan when tokens expire. Cannot be used simultaneously with `ExpiresOn`. | `null`                      |

Use the following Python code to generate a secret key.

``` python
import hashlib
import secrets

# Generate a secure random number as the base for the key
random_number = secrets.token_bytes(32)  # 32 bytes = 256 bits

# Use SHA256 to hash the secure random number
sha256_key = hashlib.sha256(random_number).hexdigest()

print(sha256_key)
```
**Example**

``` json
"Api": {
  "Jwt": {
    "Secret": "This is a sample secret key - please don't use in production environment."
    "ValidIssuer": "walletapi.zenon.network"
    "ValidAudience": "zenon.network"
    "ExpiresOn": null
    "ExpiresAfter": null
  }
}
```

### Authorization

Access to the API endpoints is restricted depending on specific claims in the JWT bearer token.

The API uses custom authorization policies to validate whether a token contains certain role claims.

Users and roles need to be configured in order to authorize users and create tokens.

**Configuration**

Use the `Api:Users` configuration section to configure an array of users.

| User                                 | Description                                                  |
| ------------------------------------ | :----------------------------------------------------------- |
| `Id` **guid** *required*             | A guid that represents an unique user id.                    |
| `Username` **string** *required*     | The user name of the user.                                   |
| `PasswordHash` **string** *required* | The password of the user stored as a hash. The hash is based on the [Blowfish cipher](https://en.wikipedia.org/wiki/Blowfish_(cipher)). See code below how to generate a password hash. |
| `Roles` **string[]** *required*      | An array of user roles. Available roles are: `"User"` and `"Admin"`. |

Execute the following Python code to generate a password hash.

``` python
import bcrypt

password = "admin".encode()  # Password to hash
salt = bcrypt.gensalt(rounds=11)  # Generate a salt with a cost of 11
hashed_password = bcrypt.hashpw(password, salt)  # Hash the password with the salt

print(hashed_password)
```

**Example**

``` json
"Api": {
  "Users": [
    {
      "Id": "236220d1-fafe-450a-9ce2-018e06c84a46",
      "Username": "admin",
      "PasswordHash": "$2a$11$J/uwDiRqEx89synrHN3y0eG2TMKxsLGIgp96hK2/7iUZX0FQs2Q9i",
      "Roles": [ "Admin", "User" ]
    },
    {
      "Id": "b0c632ff-429a-404b-898e-c2e3f4d78f08",
      "Username": "user",
      "PasswordHash": "$2a$11$iCG8gQXxfwUkl2j.pyjcS.GmnS82iTlCzyUHAH3GjPIqRyPCXOkgC",
      "Roles": [ "User" ]
    }
  ]
}
```

### Node

A node client needs to be configured in order to interact with the Zenon Network.

The node client can either connect a local Zenon Node or connect an external node. It is highly recommended to setup and use a local [Zenon Node](https://github.com/zenon-network/go-zenon) in a production environment.  Setup instructions can be found [here](https://forum.hypercore.one/t/how-to-setup-a-public-node/422).

**Configuration**

Use the `Api:Node` configuration section to configure the node client.

| Name                                        | Description                                                  | Default                  |
| ------------------------------------------- | :----------------------------------------------------------- | ------------------------ |
| `NodeUrl` **uri** *optional*                | The url of the node.                                         | `"ws://127.0.0.1:35998"` |
| `ChainId`  **int** *optional*               | The chain identifier the client uses when signing and sending transactions. | `1`                      |
| `ProtocolVersion` **int** *optional*        | The protocol version the client uses when signing and sending transactions. | `1`                      |
| `MaxPoWThreads` **int** *optional*[^1]      | The maximum amount of Proof-of-Work threads that can run simultaneously. Must be a value between `1` and `100`. | `5`                      |
| `PlasmaMode` **plasmamode** *optional* [^2] | Indicates how plasma is generated when the minimum QSR threshold is not reached. | `PoW`                    |
| `MinQsrThreshold` **int** *optional*        | The minimum amount of QSR that must be fused to an address. Must be a value between `100` and `5000`. | `100`                      |
| `FuseTimeout` **timespan** *optional*       | The maximum amount of time to wait for the fusion to complete. | `00:00:30`               |

[^1]: PoW is performed on the machine to generate plasma in order to send or receive transactions when the sending or receiving address does not have sufficient plasma.
[^2]: The plasma-bot needs to be correctly configured with an API key in order to use plasma mode `Fuse` or `Both`.  

**Example**

``` json
"Api": {
  "Node": {
    "NodeUrl": "ws://127.0.0.1:35998",
    "ChainId": 1,
    "ProtocolVersion": 1,
    "MaxPoWThreads": 5,
    "PlasmaMode": "Both",
    "MinQsrThreshold": 100,
    "FuseTimeout": "00:00:30"
  }
}
```

### Wallet

A wallet needs to be initialized in order to properly interact with the Zenon Network.

The endpoints `/api/wallet/init` or `/api/wallet/restore` are used to initialize a wallet and require the `Admin` role claim.

**Configuration**

Use the `Api:Wallet` configuration section to configure the wallet.

| Name                            | Description                                                  | Default               |
| ------------------------------- | :----------------------------------------------------------- | --------------------- |
| `Path` **string** *optioanl*    | The directory path to store the encrypted wallet file.       | `"~/.znn/wallet"`[^3] |
| `Name` **string** *optional*    | The name of the encrypted wallet file.                       | `"api"`               |
| `EraseLimit` **int** *optional* | The number of unlock attempts before the wallet is uninitialized. Can be `null`. | `3`                   |

[^3]: The default value varies depending on the OS being used.

**Example**

``` json
"Api": {
  "Wallet": {
    "Path": "~/.znn/wallet",
    "Name": "api",
    "EraseLimit": 3,
  }
}
```

### AutoReceiver

The auto-receiver will receive transactions for subscribed accounts when the wallet is initialized and unlocked.

**Configuration**

Use the `Api:AutoReceiver` configuration section to configure the auto-receiver.

| Name                                    | Description                                                  | Default      |
| --------------------------------------- | :----------------------------------------------------------- | ------------ |
| `Enabled` **boolean** *optional*        | Determines whether or not the auto-receiver is enabled.      | `true`       |
| `TimerInterval` **timespan** *optional* | The timer interval the auto-receiver checks whether new transactions are available to process. | `"00:00:05"` |

**Example**

``` json
"Api": {
  "AutoReceiver": {
    "Enabled": true,
    "TimerInterval": "00:00:05"
  }
}
```


### AutoLocker

The auto-locker automatically locks the wallet after a period of inactivity. A locked wallet is unloaded from memory.

**Configuration**

Use the `Api:AutoLocker` configuration section to configure the auto-locker.

| Name                                    | Description                                                  | Default      |
| --------------------------------------- | :----------------------------------------------------------- | ------------ |
| `Enabled` **boolean** *optional*        | Determines whether or not the auto-locker is enabled.        | `true`       |
| `LockTimeout` **timespan** *optional*   | The lock timeout determines the amount of time of wallet inactivity before the wallet is locked. | `"00:05:00"` |
| `TimerInterval` **timespan** *optional* | The timer interval the auto-locker checks whether the lock timeout has expired. | `"00:00:05"` |

**Example**

``` json
"Api": {
  "AutoLocker": {
    "Enabled": true,
    "LockTimeout": "00:05:00",
    "TimerInterval": "00:00:05"
  }
}
```


### PlasmaBot

The community [plasma-bot](https://zenonhub.io/tools/plasma-bot) offers plasma as a service. It generates plasma by fusing QSR for a limited amount of time to an address.

> Note: A valid API key is needed to make use of the plasma-bot. Contact us to receive an key.

**Configuration**

Use the `Api:Utilities:PlasmaBot` configuration section to configure the plasma-bot.

| Name                           | Description                         | Default                                           |
| ------------------------------ | :---------------------------------- | ------------------------------------------------- |
| `ApiKey` **string** *required* | The api key of the plasma-bot api.  |                                                   |
| `ApiUrl` **uri** *optional*    | The base url of the plasma-bot api. | `"https://zenonhub.io/api/utilities/plasma-bot/"` |

**Example**

``` json
"Api": {
  "Utilities": {
    "PlasmaBot": {
      "ApiKey": "[API KEY]",
      "ApiUrl": "https://zenonhub.io/api/utilities/plasma-bot/"
    }
  }
}
```


## Usage

### To authenticate

1. Authenticate an user to create a token.

``` shell
curl --location --request POST 'https://localhost/api/users/authenticate' \ 
	--header 'Content-Type: application/json' \ 
	--header 'Accept: */*' \ 
	--data-raw '{"username": "admin","password": "admin" }'
```

2. Use the token from the response and place it in the `Authorization` header for future requests.


### To check wallet status

1. Authenticate an user with an user role.
2. Check wallet status.

``` shell
curl --location --request GET 'https://localhost/api/wallet/status' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer [enter token here]' \
--header 'Accept: */*' \
```


### To create a new wallet

1. Authenticate an user with an admin role.
2. Create a new wallet.

``` shell
curl --location --request POST 'https://localhost/api/wallet/init' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer [enter token here]' \
--header 'Accept: */*' \
--data-raw '{
    "password": "[enter valid wallet password]"
}'
```

3. Check wallet status (should be initialized and unlocked).


### To restore an existing wallet

1. Authenticate an user with an admin role.
2. Restore an existing wallet.

``` shell
curl --location --request POST 'https://localhost/api/wallet/restore' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer [enter token here]' \
--header 'Accept: */*' \
--data-raw '{
    "password": "[enter valid wallet password]",
    "mnemonic": "[enter valid wallet mnemonic]"
}'
```

3. Check wallet status (should be initialized and unlocked).


### To unlock the wallet

1. Authenticate an user with an user role.
2. Unlock the wallet.

``` shell
curl --location --request POST 'https://localhost/api/wallet/unlock' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer [enter token here]' \
--header 'Accept: */*' \
--data-raw '{
    "password": "[enter wallet password]"
}'
```

3. Check wallet status (should be initialized and unlocked).


## Contributing

Please check [CONTRIBUTING](./CONTRIBUTING.md) for more details.

## License

The MIT License (MIT). Please check [LICENSE](./LICENSE) for more information.
