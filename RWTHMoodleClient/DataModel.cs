using System;
using System.Collections.Generic;
using System.Text;

namespace RWTHMoodleClient
{
#pragma warning disable IDE1006 // Name Style | Provided by API Reference
#pragma warning disable CS1591 // Missing XML Doc | provided by API Reference

    #region OAuth-DataTypes

    public class TokenInfoResponse
    {
        public string status { get; set; }
        public string audience { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public string state { get; set; }

        public bool IsValid() => state.Equals("valid");
    }


    /// <summary>
    /// The basic response on starting an authorization process (Call to /code )
    /// </summary>
    public class OAuthRequestData
    {
        public string status { get; set; }
        public string device_code { get; set; }
        public int expires_in { get; set; }
        public int interval { get; set; }
        public string verification_url { get; set; }
        public string user_code { get; set; }
    }

    /// <summary>
    /// The response on a authorization token request. (Call to /token )
    /// </summary>
    public class OAuthTokenRequestData
    {
        public string status { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string error { get; set; }
    }

    /// <summary>
    /// The datatype that is send to the Token endpoint for checking authorization status
    /// </summary>
    public class OAuthTokenRequestSendData
    {
        public string client_id { get; set; }
        public string code { get; set; }
        public string grant_type { get; set; }
    }

    /// <summary>
    /// The response to the TokenInfo request (/tokeninfo)
    /// </summary>
    public class OAuthTokenInfo
    {
        public string status { get; set; }
        public string audience { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public string state { get; set; }
    }


    #endregion

    #region Moodle Datatypes

    /// <summary>
    /// Common structure for Moodle API responses
    /// </summary>
    public class MoodleBaseResponse
    {
        public bool IsError { get; set; }
        public int StatusCode { get; set; }
        public string StatusInfo { get; set; }
    }

    public class MoodleEnrolledCoursesResponse : MoodleBaseResponse
    {
        public List<MoodleCourseInfo> Data { get; set; }
    }

    public class MoodleEnrolledCourseResponse : MoodleBaseResponse
    {
        public MoodleCourseInfo Data { get; set; }
    }

    public class MoodleGetFilesResponse : MoodleBaseResponse
    {
        public MoodleFileListInfo Data { get; set; }
    }

    public class MoodleCourseInfo
    {
        public MoodleCourseCategory category { get; set; }
        public string courseTitle { get; set; }
        public string description { get; set; }
        public long endDate { get; set; }
        public int id { get; set; }
        public string shortName { get; set; }
        public long startDate { get; set; }
        public long timeModified { get; set; }
        public string url { get; set; }
    }

    public class MoodleCourseCategory
    {
        public int id { get; set; }
        public string idnumber { get; set; }
        public string name { get; set; }
    }

    public class MoodleFileListInfo
    {
        public long created { get; set; }
        public string downloadUrl { get; set; }
        public MoodleFileInfo fileinformation { get; set; }
        public string filename { get; set; }
        public long lastModified { get; set; }
        public string modulename { get; set; }
        public string selfUrl { get; set; }
        public string sourceDirectory { get; set; }
        public string topicname { get; set; }
    }

    public class MoodleFileInfo
    {
        public int filesize { get; set; }
        public string mimetype { get; set; }
    }

    #endregion

#pragma warning restore CS1591 
#pragma warning restore IDE1006

}
