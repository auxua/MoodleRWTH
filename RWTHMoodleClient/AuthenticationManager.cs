using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RWTHMoodleClient
{
    /// <summary>
    /// The Authentication Management for the API
    /// 
    /// Usually, you want to use StartAuthenticationProcess
    /// </summary>
    public class AuthenticationManager
    {
        # region general Authentication Managemant

        /// <summary>
        /// Current state of the authentication
        /// </summary>
        public enum AuthenticationState
        {
            /// <summary>
            /// Not Authenticated, no Refresh Token known
            /// </summary>
            NONE = 0,
            /// <summary>
            /// Not necessarily authenticated at the moment, but can re-authenticate by getting an access Token using the refresh token or holding valid token at the moment
            /// </summary>
            ACTIVE = 1,
            /// <summary>
            /// There is an ongoing Authentication process (e.g. a user needs to confirm authorization in the browser
            /// </summary>
            WAITING = 2 
        }

        /// <summary>
        /// Storing the representation of the actual authentication state - You may want to change that depending on your Device
        /// </summary>
        public static AuthenticationState State { get; internal set; } = AuthenticationState.NONE;

        /// <summary>
        /// Forces the Manager to refresh the State and tries to regenerate an accessToken if possible
        /// </summary>
        public static async Task CheckState()
        {
            if (State == AuthenticationState.WAITING)
            {
                // Authentication Process ongoing - do nothing and wait
                return;
            }

            if (Config.AccessToken == "")
            {
                if (Config.RefreshToken == "")
                {
                    // No access or refresh Token available!
                    State = (AuthenticationState.NONE);
                    return;
                }
                else
                {
                    // No Access Token, but refreshToken
                    await GenerateAccessTokenFromRefreshToken();
                    return;
                }
            }

            if (Config.RefreshToken == "")
            {
                // No refreshtoken, but holding an Access Token - Check Token
                await CheckAccessToken();
                if (Config.AccessToken == "")
                {
                    // Holding a valid AccesToken now
                    State = (AuthenticationState.ACTIVE);
                    return;
                }
                else
                {
                    // Validation failed - no tokens!
                    State = (AuthenticationState.NONE);
                }
            }

        }

        /// <summary>
        /// An Exception for Indicating problem with the authorization state
        /// </summary>
        public class NotAuthorizedException : Exception
        {
            public NotAuthorizedException(string text) : base(text) { }
            public NotAuthorizedException() : base() { }

        }

        /// <summary>
        /// Checks the AccessToken against the tokenInfo REST-Service.
        /// If it fails, the system tries to refresh using the authentication manager
        /// If it still fails, accessToken is removed automatically
        /// </summary>
        public async static Task CheckAccessToken()
        {
            // use mutex for sync

            //var answer = await RESTCalls.RestCallAsync<OAuthTokenInfo>("", Config.OAuthTokenInfoEndPoint+"?accessToken="+Config.getAccessToken()+"&client_id="+Config.ClientID, true);
            var answer = await Api.RESTClient.CheckValidToken();

            if (!answer)
            {
                // Try to refresh the token
                bool success = await GenerateAccessTokenFromRefreshToken();
                //call = "{ \"client_id:\" \"" + Config.ClientID + "\" \"access_token\": \"" + Config.getAccessToken() + "\" }";

                if (!success)
                {
                    // refreshToken and AccessToken are not working!
                    Config.AccessToken = ("");
                    Config.RefreshToken = ("");
                    State = (AuthenticationState.NONE);
                    // Inform caller!
                    throw new NotAuthorizedException("App not authorized");
                }

                answer = await Api.RESTClient.CheckValidToken();
                if (!answer)
                {
                    // Invalid Token, no refreshToken success  - delete it
                    Config.AccessToken = ("");
                }
                else
                {
                    // Successful reconstructed Tokens
                    State = AuthenticationState.ACTIVE;
                }
            }
            else
            {
                State = AuthenticationState.ACTIVE;
            }
            //CheckAccessTokenMutex.ReleaseMutex();
        }

        # endregion

        # region Authorization Process

        /// <summary>
        /// Starts the Autehntication Process
        /// </summary>
        /// <returns>returns the verification URL for this app or an empty string on fails</returns>
        public async static Task<string> StartAuthenticationProcess()
        {
            var answer = await OAuthInitCall();
            if (answer.status == "ok")
            {
                // call was successfull!
                string url = answer.verification_url + "?q=verify&d=" + answer.user_code;
                Config.DeviceToken = (answer.device_code);
                State = (AuthenticationState.WAITING);
                expireTimeWaitingProcess = answer.expires_in;
                return url;
            }
            throw new Exception("Starting Authentication failed: " + answer?.status);
        }

        private static int expireTimeWaitingProcess;

        /// <summary>
        /// Checks whether the users has already authenticated the app (Part of the Authentication process!)
        /// </summary>
        /// <returns></returns>
        public async static Task<bool> CheckAuthenticationProgress()
        {
            var answer = await TokenCall();
            if (answer == null || answer.status == null || answer.status.StartsWith("Fail:") || answer.status.StartsWith("error:"))
            {
                // Not working!
                return false;
            }
            // working!
            // Store the tokens
            Config.AccessToken = (answer.access_token);
            Config.RefreshToken = (answer.refresh_token);
            State = (AuthenticationState.ACTIVE);
            return true;
        }

        /// <summary>
        /// Initiates the Authorization process
        /// </summary>
        /// <returns>The answer on the Initial Call to Endpoint or an empty answer if something went wrong! (having a status field Fail: (Error message) )</returns>
        internal async static Task<OAuthRequestData> OAuthInitCall()
        {
            //InitAuthMutex.WaitOne();
            string parsedContent = "{ \"client_id\": \"" + Config.ClientID + "\", \"scope\": \"l2p.rwth campus.rwth l2p2013.rwth moodle.rwth\" }";
            var answer = await Api.RESTClient.RestCallAsync<OAuthRequestData>(parsedContent, Config.OAuthEndPoint, true);
            //InitAuthMutex.ReleaseMutex();
            return answer;
        }


        /// <summary>
        /// Calls the /token Endpoint to check status of authorization process
        /// </summary>
        /// <returns>The answer to the Call or an artificial answer containing an Error Descripstion in the status-field</returns>
        private async static Task<OAuthTokenRequestData> TokenCall()
        {
            try
            {
                var req = new OAuthTokenRequestSendData();
                req.client_id = Config.ClientID;
                req.code = Config.DeviceToken;
                req.grant_type = "device";

                string postData = JsonConvert.SerializeObject(req);

                var answer = await Api.RESTClient.RestCallAsync<OAuthTokenRequestData>(postData, Config.OAuthTokenEndPoint, true);
                //TokenCallMutex.ReleaseMutex();
                return answer;
            }
            catch (Exception e)
            {
                var t = e.Message;
                var pseudo = new OAuthTokenRequestData();
                pseudo.status = "Fail: " + t;
                //TokenCallMutex.ReleaseMutex();
                return pseudo;
            }
        }


        /// <summary>
        /// A sinple method that will reset the State to NONE after (expireTime) seconds
        /// </summary>
        private static void ExpireThread()
        {
            // Wait for the expire time
            //Thread.Sleep(expireTimeWaitingProcess * 1000);
            // wake up and check, whether the process has expired
            State = (AuthenticationState.NONE);
        }

        # endregion


        /// <summary>
        /// Uses the current RefreshToken to get an Access Token
        /// </summary>
        /// <returns>true, if the call was successfull</returns>
        public async static Task<bool> GenerateAccessTokenFromRefreshToken()
        {
            //RefreshhMutex.WaitOne();
            string callBody = "{ \"client_id\": \"" + Config.ClientID + "\", \"refresh_token\": \"" + Config.RefreshToken + "\", \"grant_type\":\"refresh_token\" }";
            var answer = await Api.RESTClient.RestCallAsync<OAuthTokenRequestData>(callBody, Config.OAuthTokenEndPoint, true);
            if ((answer.error == null) && (!answer.status.StartsWith("error")) && (answer.expires_in > 0))
            {
                //Console.WriteLine("Got a new Token!");
                Config.AccessToken = (answer.access_token);
                TokenExpireTime = answer.expires_in;
                //TokenExpireTime = 10;
                //Refresher = new Thread(new ThreadStart(TokenRefresherThread));
                //Refresher.Start();
                //RefreshhMutex.ReleaseMutex();
                return true;
            }
            // Failed!
            //RefreshhMutex.ReleaseMutex();
            return false;
        }

        //private static Thread Refresher = null;
        private static int TokenExpireTime;

        private static void TokenRefresherThread()
        {
            //Console.WriteLine("Startet Refresh-Thread");
            //Thread.Sleep(TokenExpireTime * 1000);
            //Console.WriteLine("Refreshing!");
            GenerateAccessTokenFromRefreshToken().ConfigureAwait(false);
        }




    }
}
