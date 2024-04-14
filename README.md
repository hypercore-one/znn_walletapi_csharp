# Zenon Wallet API for .NET

A .NET based cross-platform Wallet API for interacting with Zenon Alphanet - Network of Momentum Phase 1

## Requirements

- [Microsoft .NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Zenon Node](https://github.com/zenon-network/go-zenon)

## Dependencies

- [Serilog logging for ASP.NET Core](https://github.com/serilog/serilog-aspnetcore)
- [Zenon SDK for .NET](https://github.com/hypercore-one/znn_sdk_csharp)

## Installation on Linux

``` bash
sudo apt-get update && sudo apt-get install -y dotnet8

git clone https://github.com/hypercore-one/znn_walletapi_csharp.git

cd znn_walletapi_csharp/src/ZenonWalletApi

export Api__Jwt__Secret=""
export Api__Utilities__PlasmaBot__ApiKey=""

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

Use the `Api:Jwt` configuration section to configure Jwt authentication.

``` Json
"Api": {
  "Jwt": {
    "Secret": "This is a sample secret key - please don't use in production environment."
    "ValidIssuer": "zenon.wallet.api"
    "ValidAudience": "zenon.network"
    "ExpiresOn": null
    "ExpiresAfter": null
  }
}
```

**Options:**

- **Secret**  
Sets the issuer security key that is to be used for signature validation.
- **ValidIssuer**  
Sets a string that represents a valid issuer that will be used to check against the token's issuer. Default value is: `"zenon.wallet.api"`.
- **ValudAudience**  
Sets a string that represents a valid audience that will be used to check against the token's audience. Default value is: `"zenon.network"`.
- **ExpiresOn**  
Sets the expiration datetime in UTC when tokens expire. Cannot be used simultaneously with `ExpiresAfter`. Default value is: `null`.
- **ExpiresAfter**  
Sets the expiration timespan when tokens expire. Cannot be used simultaneously with `ExpiresOn`. Default value is: `null`.


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


### Authorization

Access to the API endpoints is restricted depending on specific claims in the JWT bearer token.

The API uses custom authorization policies to validate whether a token contains certain role claims.

Users and roles need to be configured in order to authorize an user and create tokens.

Use the `Api:Users` configuration section to configure Users authorization.

``` Json
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

**Options**:

- **Id**  
A guid that represents an unique user id.
- **Username**  
The username of the user.
- **Password**  
The password hash of the user. The hash is based on the Blowfish cipher. See code below to generate a hash.
- **Roles**  
An array of user roles. Available roles are: `"User"` or `"Admin"`.


Use the following Python code to generate a password hash.

``` python
import bcrypt

password = "admin".encode()  # Password to hash
salt = bcrypt.gensalt(rounds=11)  # Generate a salt with a cost of 11
hashed_password = bcrypt.hashpw(password, salt)  # Hash the password with the salt

print(hashed_password)
```


### Wallet

A wallet needs to be initialized in order to properly interact with the Zenon Network.

The endpoints `/api/wallet/init` or `/api/wallet/restore` are used to initialize a wallet and require the `Admin` role claim.

Use the `Api:Wallet` configuration section to configure the wallet.

``` Json
"Api": {
  "Wallet": {
    "Path": "~/.znn/wallet",
    "Name": "api",
    "EraseLimit": 3,
  }
}
```

**Options:**

- **Path**  
The directory path to store the encrypted wallet file.
- **Name**  
The name of the encrypted wallet file.
- **EraseLimit**  
The number of unlock attempts before the wallet is uninitialized. Default value is: `3`. Can be `null`.


### Node

A node client needs to be configured in order to interact with the Zenon Network.

The node client can either connect a local Zenon Node or connect an external node. It is highly recommended to setup and use a local Zenon Node in a production environment. 

Use the `Api:Node` configuration section to configure the node client.

``` Json
"Api": {
  "Node": {
    "NodeUrl": "ws://127.0.0.1:35998",
    "ChainId": 1,
    "ProtocolVersion": 1,
    "MaxPoWThreads": 5
  }
}
```

**Options:**

- **NodeUrl**  
The url of the node. Default value is: `"ws://127.0.0.1:35998"`.
- **ChainId**  
The chain identifier the client uses when signing and sending transactions. Default value is: `1`.
- **ProtocolVersion**  
The protocol version the client uses when signing and sending transactions. Default value is: `1`.
- **MaxPoWThreads**  
The maximum number of PoW threads that can run simultaneously. Must be a value between 1 and 100. Default value is: `1`.


### AutoReceiver

The auto-receiver will receive transactions for subscribed accounts when the wallet is initialized and unlocked.

Use the `Api:AutoReceiver` configuration section to configure the auto-receiver.

``` Json
"Api": {
  "AutoReceiver": {
    "Enabled": true,
    "TimerInterval": "00:00:05"
  }
}
```

**Options:**

- **Enabled**  
Determines whether or not the auto-receiver service is enabled. Default value is: `true`.
- **TimeInterval**  
The timer interval the service checks whether new transactions are available to process. Default value is: `"00:00:05"`.


### AutoLocker

The auto-locker automatically locks the wallet after a period of inactivity. A locked wallet is unloaded from memory.

Use the `Api:AutoLocker` configuration section to configure the auto-locker.

``` Json
"Api": {
  "AutoLocker": {
    "Enabled": true,
    "LockTimeout": "00:05:00"
    "TimerInterval": "00:00:05"
  }
}
```

**Options:**

- **Enabled**  
Determines whether or not the auto-receiver service is enabled. Default value is: `true`.
- **LockTimeout**  
The lock timeout determines the amount of time of wallet inactivity before the wallet is locked. Default value is: `"00:05:00"`.
- **TimeInterval**  
The timer interval the service checks whether the lock timeout has expired. Default value is: `"00:00:05"`.


### PlasmaBot

The community plasma bot offers plasma as a service. It fuses QSR for a limited amount of time to an address.

A valid API key is needed to make use of the plasma bot service.

Use the `Api:Utilities:PlasmaBot` configuration section to configure the auto-locker.

``` Json
"Api": {
  "Utilities": {
    "PlasmaBot": {
      "ApiUrl": "https://zenonhub.io/api/utilities/plasma-bot/",
      "ApiKey": "[API KEY]"
    }
  }
}
```

**Options:**

- **ApiUrl**  
The base url of the plasma-bot api. Default value is: `https://zenonhub.io/api/utilities/plasma-bot/`.
- **ApiKey**  
The api key of the plasma-bot api.


## API Documentation

Once the Zenon Wallet API is installed, configured and running. Navigate to `https://localhost:443/swagger` with a browser to consult the interactief API Documentation.


## Usage

### To initialize or restore the wallet

1. Authenticate an user with an admin role to create a token with an admin role claim.

```
curl --location --request POST 'https://localhost/api/users/authenticate' \ 
	--header 'Content-Type: application/json' \ 
	--header 'Accept: */*' \ 
	--data-raw '{"username": "admin","password": "admin" }'
```

2. Use the token in the response for future requests in the `Authorization` header.
3. Initialize the wallet.

```
curl --location --request POST 'https://localhost/api/wallet/init' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer [enter token here]' \
--header 'Accept: */*' \
--data-raw '{
    "password": "[enter valid wallet password]"
}'
```

or to restore:

```
curl --location --request POST 'https://localhost/api/wallet/restore' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer [enter token here]' \
--header 'Accept: */*' \
--data-raw '{
    "password": "[enter valid wallet password]",
    "mnemonic": "[enter valid wallet mnemonic]"
}'
```

4. Check wallet status (should be initialized and unlocked).

```
curl --location --request GET 'https://localhost/api/wallet/status' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer [enter token here]' \
--header 'Accept: */*' \
```


### To unlock the wallet

1. Authenticate an user with an user role to create a token with an user role claim.

```
curl --location --request POST 'https://localhost/api/users/authenticate' \ 
	--header 'Content-Type: application/json' \ 
	--header 'Accept: */*' \ 
	--data-raw '{"username": "user","password": "user" }'
```

2. Use the token in the response for future requests in the `Authorization` header.
3. Unlock the wallet.

```
curl --location --request POST 'https://localhost/api/wallet/unlock' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer [enter token here]' \
--header 'Accept: */*' \
--data-raw '{
    "password": "[enter wallet password]"
}'
```

4. Check wallet status (should be initialized and unlocked).

```
curl --location --request GET 'https://localhost/api/wallet/status' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer [enter token here]' \
--header 'Accept: */*' \
```


## Contributing

Please check [CONTRIBUTING](./CONTRIBUTING.md) for more details.

## License

The MIT License (MIT). Please check [LICENSE](./LICENSE) for more information.