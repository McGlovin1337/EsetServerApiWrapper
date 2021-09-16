# EsetServerApiWrapper

## About
A simple .NET Standard Wrapper for the ESET Protect ServerApi Library

## Usage
### Pre-requisites
The wrapper has the following Dependencies -:
* ServerApi.dll
* Protobuf.dll
* ProtobufNetworkMessages.dll
* Network.dll

All these files can be obtained from the ESET Management Console Server. These should be placed in the same location as the compiled class library.

```csharp
// Initialize the ESET API Library
EsetApi esetApi = new EsetApi();

// Send a request and store response
string response = esetApi.SendRequest("{\"Era.ServerApi.StartRequest\":{}}");

// Free resources
esetApi.Dispose();
```

## Notes

SetDllDirectory is used so the Class Library can be loaded into a PowerShell Session. 
However, Network.dll only seems to successfuly load if it is placed in the Working Directory of the calling executable. This will generally be fine for most applications.

In the case of PowerShell, one must copy Network.dll to the location the PowerShell Process is run from. If anyone knows how to workaround/resolve this, that would be great!
