using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace auxua.RWTHMoodleClient.Api
{
    /// <summary>
    /// Contains Generic and API Rest Calls
    /// </summary>
    public static class RESTClient
    {
        private static string ToQueryString(NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            return "?" + string.Join("&", array);
        }

        #region generic calls

        public static async Task<string> GetAsync(string url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;
            string result = await client.GetStringAsync(url);
            return result;
        }

        public static async Task<byte[]> GetAsyncByteArray(string url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;
            var result = await client.GetByteArrayAsync(url);
            return result;
        }


        public static async Task<string> PostAsync(string url, string data)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;

            StringContent content = new StringContent(data);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");

            //HttpResponseMessage response = await client.PostAsync(url, content);
            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();
            return result;

        }

        /// <summary>
        /// A generic REST-Call to an endpoint using GET or POST method
        /// 
        /// Uses a WebRequest for POST, a httpClient for GET calls
        /// 
        /// throws an Exception in Case of Error
        /// </summary>
        /// <typeparam name="T1">The Datatype to await for response</typeparam>
        /// <param name="input">the data as string (ignored, if using GET)</param>
        /// <param name="endpoint">The REST-Endpoint to call</param>
        /// <param name="post">A flag indicating whether to use POST or GET</param>
        /// <returns>The datatype that has been awaited for the call or default(T1) on error</returns>
        public async static Task<T1> RestCallAsync<T1>(string input, string endpoint, bool post)
        {

            if (post)
            {
                var answerCall = await PostAsync(endpoint, input);
                T1 answer = JsonConvert.DeserializeObject<T1>(answerCall, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                return answer;
            }
            else
            {
                var answerCall = await GetAsync(endpoint);
                T1 answer = JsonConvert.DeserializeObject<T1>(answerCall, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                return answer;
            }

        }


        #endregion

        #region Moodle Calls

        /// <summary>
        /// Gets the Course information of currently enrolled courses
        /// </summary>
        /// <param name="semester">semester for filtering (see doc)</param>
        /// <param name="semesterOffset">offset for semester search (see doc)</param>
        public async static Task<MoodleEnrolledCoursesResponse> MoodleGetEnrolledCourses(string semester=null, int semesterOffset=int.MinValue)
        {
            await AuthenticationManager.CheckAccessToken();
            NameValueCollection c = new NameValueCollection();
            if (!String.IsNullOrWhiteSpace(semester)) c.Add("semester",semester);
            if (!(int.MinValue ==(semesterOffset))) c.Add("semesterOffset", semesterOffset.ToString());
            c.Add("token", Config.AccessToken);
            string callURL = Config.MoodleEndPoint + "/GetMyEnrolledCourses" + ToQueryString(c);
            var answer = await RestCallAsync<MoodleEnrolledCoursesResponse>("", callURL, false);
            return answer;
        }

        /// <summary>
        /// Gets Information about specific course
        /// </summary>
        /// <param name="courseid">the id of the course</param>
        public async static Task<MoodleEnrolledCourseResponse> MoodleGetEnrolledCourseById(int courseid)
        {
            await AuthenticationManager.CheckAccessToken();
            NameValueCollection c = new NameValueCollection();
            c.Add("courseid", courseid.ToString());
            c.Add("token", Config.AccessToken);
            string callURL = Config.MoodleEndPoint + "/GetMyEnrolledCourseById" + ToQueryString(c);
            var answer = await RestCallAsync<MoodleEnrolledCourseResponse>("", callURL, false);
            return answer;
        }

        /// <summary>
        /// Gets the available files in a course room
        /// </summary>
        /// <param name="courseid">id of the course</param>
        /// <param name="topicname">(optional) filter for topics</param>
        public async static Task<MoodleGetFilesResponse> MoodleGetFiles(int courseid, string topicname=null)
        {
            await AuthenticationManager.CheckAccessToken();
            NameValueCollection c = new NameValueCollection();
            c.Add("courseid", courseid.ToString());
            if (!String.IsNullOrWhiteSpace(topicname)) c.Add("topicname", topicname);
            c.Add("token", Config.AccessToken);
            string callURL = Config.MoodleEndPoint + "/GetFiles" + ToQueryString(c);
            var answer = await RestCallAsync<MoodleGetFilesResponse>("", callURL, false);
            return answer;
        }

        /// <summary>
        /// Downloads a file from Moodle
        /// 
        /// File is returned as stream. For unavailable files or invalid urls, stream is empty
        /// </summary>
        /// <param name="downloadUrl">Url to file in moodle</param>
        public async static Task<Stream> MoodleDownloadFile(string downloadUrl)
        {
            await AuthenticationManager.CheckAccessToken();
            NameValueCollection c = new NameValueCollection();
            c.Add("downloadUrl", downloadUrl);
            c.Add("token", Config.AccessToken);
            string callURL = Config.MoodleEndPoint + "/DownloadFile/" + ToQueryString(c);

            //string postData = JsonConvert.SerializeObject(data);
            var answer = await GetAsyncByteArray(callURL);
            MemoryStream ms = new MemoryStream(answer);
            return ms;
        }

        /// <summary>
        /// Workaround:
        /// Check the Token for being valid by calling the Moodle Api (Only in case of errors of the tokeninfo-endpoint)
        /// </summary>
        /// <returns>true, if the token seems to be valid</returns>
        public async static Task<bool> CheckValidToken()
        {
            TokenInfoResponse answer;
            try
            {
                answer = await OAuthTokenInfo();
            }
            catch (System.Net.Http.HttpRequestException ex) // On some OS, an Exception is thrown
            {
                return false;
            }
            if ((answer == null) || (answer.IsValid() == false))
                return false;
            return true;
        }

        internal class OAuthTokenInfoRequest
        {
            public string client_id { get; set; }
            public string access_token { get; set; }
        }

        /// <summary>
        /// Gets information about the current Token
        /// </summary>
        public static Task<TokenInfoResponse> OAuthTokenInfo()
        {
            OAuthTokenInfoRequest req = new OAuthTokenInfoRequest()
            {
                client_id = Config.ClientID,
                access_token = Config.AccessToken
            };
            //Remark: parameters need to be passed completely as POST body (ITC documentation is incorrect at this point)
            string callURL = Config.OAuthTokenInfoEndPoint;
            return RestCallAsync<TokenInfoResponse>(JsonConvert.SerializeObject(req), callURL, true);
        }
        #endregion
    }
}
