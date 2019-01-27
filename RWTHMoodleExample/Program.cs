using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using auxua;

namespace RWTHMoodleExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting RWTH Moodle API Usage Example");

            Console.WriteLine("Start Configuration");
            // Set your ClientID here
            auxua.RWTHMoodleClient.Config.ClientID = "XXX.apps.rwth-aachen.de";
            
            Console.WriteLine("Starting Authentication Process");
            // Get URL for Authentication (OAuth2)
            string url = await auxua.RWTHMoodleClient.AuthenticationManager.StartAuthenticationProcess();
            // Present Web to user (ITC RWTH requires you to user system web browser instead of own webview)

            // Traditional approach (e.g. Mono/.NET - use Process.Start directly)
            //Process.Start(url);
            // .Net Core Workaround
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }

            // Wait for authentication
            // so far, not authenticated
            bool done = false;

            while (!done)
            {
                // Just wait 5 seconds - this is the recommended querying time for OAuth by ITC
                Thread.Sleep(5000);
                await auxua.RWTHMoodleClient.AuthenticationManager.CheckAuthenticationProgress();

                done = (auxua.RWTHMoodleClient.AuthenticationManager.State == auxua.RWTHMoodleClient.AuthenticationManager.AuthenticationState.ACTIVE);

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

            Console.WriteLine("Calling API");
            // Getting List of current Course rooms in Moodle
            auxua.RWTHMoodleClient.MoodleEnrolledCoursesResponse result = await auxua.RWTHMoodleClient.Api.RESTClient.MoodleGetEnrolledCourses();

            // Use Result
            foreach (var item in result.Data)
                Console.WriteLine("You are in course: " + item.courseTitle);

            // Example of error handling
            var errorResult = await auxua.RWTHMoodleClient.Api.RESTClient.MoodleGetEnrolledCourseById(-1);
            // Checking for Error
            if (errorResult.IsError)
                Console.WriteLine(errorResult.StatusInfo);
            Console.WriteLine("Finished Examples");

            Console.ReadLine();
        }
    }
}
