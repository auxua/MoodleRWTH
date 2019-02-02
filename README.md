# MoodleRWTH
Basic API Client for RWTH Moodle system

The good old RWTH eLearning System (L²P) is succeeded by the RWTH Moodle system. Currently, the ITC is implementing an API for the Moodle system. This project builds up a small client for the API, including the authentication implementation.

To use this Client, you will need a Developer Client ID from ITC of RWTH Aachen University.

# Technical
This project consists of two sub-projects:

## RWTHMoodleClient

* A .NetStandard 2.0 implementation of the Client and Authentication.
* Only has a Newtonsoft.Json Dependency via Nuget
* Targets .NetStandard 2.0, but should be possible to adapt to older .Net Framework easily if needed
* Allows easy typesafe API Calling
* Can be configured depending on platform for persistent storage

## RWTHMoodleExample

* A .Net Core (C# 7.1) example of the usage of the Client
* Authentication, Enrollment Listing and Error Handling
 
## CI Pipeline

[![pipeline status](https://git.rwth-aachen.de/auxua/rwthmoodleclient/badges/master/pipeline.svg)](https://git.rwth-aachen.de/auxua/rwthmoodleclient/commits/master)

# How to use?

## 0. Preparing

Please make sure, you have an Developer Client ID from ITC of RWTH Aachen University. This is mandatory to use the API at all.
Include the Client - The easy way is to load the Client via nuget (TODO: Link)

## 1. Config & Authentication

The API system relies on OAuth2 Authentication.

```csharp
// Set your ClientID here
RWTHMoodleClient.Config.ClientID = "XXX.apps.rwth-aachen.de";
            
Console.WriteLine("Starting Authentication Process");
// Get URL for Authentication (OAuth2)
string url = await RWTHMoodleClient.AuthenticationManager.StartAuthenticationProcess();
// Present Web to user (ITC RWTH requires you to user system web browser instead of own webview)

// Traditional approach (e.g. Mono/.NET - use Process.Start directly)
//Process.Start(url);
// .Net Core Workaround
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    url = url.Replace("&", "^&");
    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
}
```
The user now gets a website to login and authorize your app. If you keep a persistent storage, this is only needed once.

You need to wait, until the authorization is finished

```csharp
// Wait for authentication
// so far, not authenticated
bool done = false;

while (!done)
{
    // Just wait 5 seconds - this is the recommended querying time for OAuth by ITC
    Thread.Sleep(5000);
    await RWTHMoodleClient.AuthenticationManager.CheckAuthenticationProgress();

    done = (RWTHMoodleClient.AuthenticationManager.State == RWTHMoodleClient.AuthenticationManager.AuthenticationState.ACTIVE);

    if (!done)
    {
        Console.WriteLine("App not authenticated right now...");
    }
    else
    {
        Console.WriteLine("App authenticated!");
    }

}

// Now, you are authenticated, Tokens are stored (and refreshed if needed)
```

## 2. Call the API

Now you can simply call the API. The client deals automatically with filling up additional fields and refreshing tokens.

```csharp
// Getting List of current Course rooms in Moodle
RWTHMoodleClient.MoodleEnrolledCoursesResponse result = await RWTHMoodleClient.Api.RESTClient.MoodleGetEnrolledCourses();

// Use Result
foreach (var item in result.Data)
    Console.WriteLine("You are in course: " + item.courseTitle);
```

## Extra: Error handling

While technical errors (No internet, etc.) will cause Exceptions, errors because of missing roles or invalid parameters will return indicators.

```csharp
// Example of error handling
var errorResult = await RWTHMoodleClient.Api.RESTClient.MoodleGetEnrolledCourseById(-1);
// Checking for Error
if (errorResult.IsError)
    Console.WriteLine(errorResult.StatusInfo);
```

# License

This project will be available under MIT License. This allows you to use the Client in your projects without problems.
