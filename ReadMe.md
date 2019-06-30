# Leap Play HMAC SHA256 Authentication
## Preparation
---
This is a example for the "amx" authentication scheme used at <a href="https://api.leap-play.com" target="_blank">api.leap-play.com</a>

To use this example you will need to:
1. Have an account at <a href="https://api.leap-play.com" target="_blank">api.leap-play.com</a>.
See the <a href="https://www.leap-play.com/docs/quick-start/" target="_blank">QuickStart Guide</a> for help
2. Created at one Station including at least one API Key
3. Downloaded or cloned this repository

Before we can run the example code we have to insert our key and secret.
Change the code an replace the placeholders with the values you received.

```csharp
//Your API Public Key
const string ApiPublicKey = "PublicKey goes here";

//Your API Secret
const string ApiSecret = "Api Secret goes here";
```

Run the Code and watch the console window for the request and response data!

## Authentication
---
The Header values for authentication are set in the following class
```csharp
/// <summary>
/// Creates the Authentication Header
/// </summary>
private class AuthenticationHandler : DelegatingHandler
```

### Signature String
Create the string that is used as input for the HMAC SHA256 signature.
Assemble the string in the exact same order and same rules as described:
```csharp
string signatureString =
        $"{ApiPublicKey}{requestHttpMethod}{requestUri}{requestTimeStamp}{nonce}{requestContentMd5AsBase64}";
```
  
| Key                       | Value                                    |
|---------------------------|------------------------------------------|
| ApiPublicKey              | Public Key received from the Server      |
| requestHttpMethod         | The http method used as upper case (GET, POST, PUT, DELETE) |
| requestUri                | Full route with all parameters, lower case and url encoded :<br/>Plain: "https://api.leap-play.com/api/v1/station/settings"<br/>UrlEncoded: "https%3a%2f%2flocalhost%3a5001%2fapi%2fv1%2fstation%2fsettings" |
| requestTimeStamp          | Unix timestamp in milliseconds           |
| nounce                    | Some unique value for each command send and decided by the sender.  The value can not contain any values that might break the format as : or whitespace. In general a GUID is a good fit |
| requestContentMd5AsBase64 | Base64 encoded MD5 hash of the content body or empty string in case there is no body |


### Signature Hash
Create the Signature 
```csharp
string signatureHmacSha256AsBase64 = Convert.ToBase64String(hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(signatureString)));
```
### Authorization Header
Set the authorization value in following format for the header.

```csharp
authorizationValue = $"{ApiPublicKey}:{signatureHmacSha256AsBase64}:{nonce}:{requestTimeStamp}";
```

| Key                         | Value                                  |
|-----------------------------|----------------------------------------|
| ApiPublicKey                | Same value as used in Signature String |
| signatureHmacSha256AsBase64 | The Signature Hash Base64 encoded      |
| nonce                       | Same value as used in Signature String |
| requestTimeStamp            | Same value as used in Signature String |


<br/>  
Example of the resulting authorization value including the amx prefix.

```text
amx b764336fcc99484dbe319870445125e9:4f5axEM26tMfb6jB8fHYmJXFpHU4nFPByNcdkfCuzUA=:56ceb37ddf3240609b918a7c1be14477:1561887475966
```
<br/>

**Visit <a href="https://www.leap-play.com" target="_blank">Leap Play</a> to learn more about the project.** 