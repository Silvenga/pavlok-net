using System;
using System.Net;

namespace Pavlok
{
    [Serializable]
    public class PavlokApiException : Exception
    {
        public string Body { get; }

        public HttpStatusCode StatusCode { get; }

        public PavlokApiException(string body, HttpStatusCode statusCode)
            : base($"A Pavlok API call failed with status code '{statusCode}'.")
        {
            Body = body;
            StatusCode = statusCode;
        }

        public PavlokApiException(Exception inner, string body, HttpStatusCode statusCode)
            : base($"A Pavlok API call failed with status code '{statusCode}'.", inner)
        {
            Body = body;
            StatusCode = statusCode;
        }
    }
}