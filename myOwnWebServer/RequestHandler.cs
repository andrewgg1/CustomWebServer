/*
 *	FILE				: RequestHandler.cs
 *	PROJECT				: Web Design and Development PROG2001 - Assignment 6
 *	PROGRAMMER			: Andrew Gordon
 *	FIRST VERSION		: Nov. 23 2020
 *  LAST UPDATE         : Dec. 2, 2020
 *	DESCRIPTION			: Contains the RequestHandler class, used to handle incoming client
 *	                      http requests.
 */

using System.IO;
using System.Text.RegularExpressions;

namespace myOwnWebServer
{
    /* ---------------------------------------------------------------------------------
    CLASS NAME  :	RequestHandler
    PURPOSE     :	The purpose of this class is to provide request handling functionality.
                    This includes parsing the request header, and validating those parsed
                    values.

                    Inlcudes properties:
                    - string StatusCode
                    - string Method
                    - string Resource
                    - string VersionNumber
                    - string Host

                    Includes methods:
                    - ParseHeader()
                    - ValidateRequest()

    --------------------------------------------------------------------------------- */
    class RequestHandler
    {
        public static string StatusCode { get; set; }          // response status code
        public static string Method { get; set; }              // parsed method
        public static string Resource { get; set; }            // parsed resource URL
        public static string VersionNumber { get; set; }       // parsed htpp version number
        public static string Host { get; set; }                // parsed host

        /*  -- Method Header Comment
        Name	:	ParseHeader
        Purpose :	Parses the passed request header to ensure it matches with the correct HTTP format.
                    Inserts each value into the appropriate class property.
        Inputs	:	string requestHeader     raw request header from the client
        Outputs	:	None
        Returns	:	bool    true        arguments were successfully parsed
                            false       arguments were unsuccessfully parsed
        */
        public static bool ParseHeader(string requestHeader)
        {
            string[] parsedRequest = new string[3];     // array to contain parsed values
            bool validRequest = false;                   // flag if the request is valid
            // regex for the first line of the http request
            Regex requestLineReg = new Regex(@"^(\S+) (\S+?)(?:\?(?:\S+)?)? (?:HTTP/)(\S+)(?:\r\n)");
            // regex for the line with the host
            Regex hostReg = new Regex(@"(?i)(?:HOST:)(?-i) (\S+)(?:\r\n)");
            // regex to ensure that there is a double newline in the header
            Regex endOfReqReg = new Regex(@"(?:\r\n\r\n)");
            // used to capture the method and resource in case other parts of the request are invalid
            Regex methodAndResourceReg = new Regex(@"^(\S+) (\S+) ");

            // if the request header is null, set status code and return
            if (requestHeader == null)
            {
                RequestHandler.StatusCode = "400 Bad Request";
                return validRequest;
            }

            // Check if the request header matches the required format
            Match r = requestLineReg.Match(requestHeader);
            Match h = hostReg.Match(requestHeader);
            Match e = endOfReqReg.Match(requestHeader);

            // if everything matched
            if (r.Success && h.Success && e.Success)
            {
                // get the values of each part of the request
                for (int i = 0; i < parsedRequest.Length; ++i)
                {
                    // save values to the array
                    parsedRequest[i] = r.Groups[i + 1].ToString();
                }

                // assign each value to its appropriate property
                Method = parsedRequest[0];
                Resource = parsedRequest[1];
                VersionNumber = parsedRequest[2];
                Host = h.Groups[1].ToString();
                // set valid request to true
                validRequest = true;
            }
            // if the header had format issues
            else
            {
                // capture method and resource form request, this will be used for the log file
                Match m = methodAndResourceReg.Match(requestHeader);
                if (m.Success)
                {
                    Method = m.Groups[1].ToString();
                    Resource = m.Groups[2].ToString();
                }
                // set status code and flag to show invalid request
                StatusCode = "400 Bad Request";
                validRequest = false;
            }
            return validRequest;
        }

        /*  -- Method Header Comment
        Name	:	ValidateRequest
        Purpose :	Validates the request header, sets the appropriate status code if anything
                    invalid is detected
        Inputs	:	string method            the request method
                    string resource          the request resource
                    string versionNumber     the request http version number
        Outputs	:	None
        Returns	:	bool    true    if the request contained valid data
                            false   if any data was invalid
        */
        public static bool ValidateRequest(string method, string resource, string versionNumber)
        {
            bool validData = true;      // flag if all data was valid

            // check version number
            if (versionNumber != "1.1")
            {
                StatusCode = "505 HTTP Version Not Supported";
                validData = false;
            }
            // check file extension
            else if (ResponseHandler.ValidExtension(Path.GetExtension(resource)) == "")
            {
                StatusCode = "406 Not Acceptable Filetype";
                validData = false;
            }
            // check resource file exists
            else if (!new FileInfo(ServerHandler.WebRoot + resource).Exists)
            {
                StatusCode = "404 Not Found";
                validData = false;
            }
            // check method was a GET request
            else if (method != "GET")
            {
                StatusCode = "405 Method Not Allowed";
                validData = false;
            }

            return validData;
        }
    }
}
