/*
 *	FILE				: ResponseHandler.cs
 *	PROJECT				: Web Design and Development PROG2001 - Assignment 6
 *	PROGRAMMER			: Andrew Gordon
 *	FIRST VERSION		: Nov. 23 2020
 *  LAST UPDATE         : Dec. 2, 2020
 *	DESCRIPTION			: Contains the ResponseHandler class, used to handle outgoing
 *	                      http responses
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace myOwnWebServer
{
    /* ---------------------------------------------------------------------------------
    CLASS NAME  :	ResponseHandler
    PURPOSE     :	The purpose of this class is to provide response handling functionality.
                    This includes building the response and validating the extension of the
                    outgoing file.

                    Includes methods:
                    - BuildResponse()
                    - ValidExtension()

    --------------------------------------------------------------------------------- */
    class ResponseHandler
    {
        /*  -- Method Header Comment
        Name	:	BuildResponse
        Purpose :	Builds the response to be sent to the client.
        Inputs	:	string fullfilePath         the full filepath of the resource
        Outputs	:	None
        Returns	:	Byte[]      array of bytes containing the full http response message
        */
        public static Byte[] BuildResponse(string fullfilePath)
        {
            string strHeader = "";      // response header as a string
            Byte[] byteHeader;          // response header as a Byte array
            Byte[] byteBody;            // response body as a Byte array
            string contentType = "";    // content type or MIME type

            // current date and time
            string currentDateTime = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");

            // if the status code was not set up to this point, then there are no errors
            if (RequestHandler.StatusCode == "")
            {
                // set status code and content type
                RequestHandler.StatusCode = "200 OK";
                contentType = ResponseHandler.ValidExtension(new FileInfo(fullfilePath).Extension);
                // read the file and save it as a byte array
                byteBody = File.ReadAllBytes(fullfilePath);
                // write to the log file
                Logger.WriteToLog("RESPONSE SENT", $"content-type:{contentType} content-length:{byteBody.Length} server:{ServerHandler.Server} date:{currentDateTime}");
            }
            // errors were detected
            else
            {
                // set content type to HTML
                contentType = ResponseHandler.ValidExtension(".html");
                // simple html error body to be passed to client
                string bodyError = $"<!DOCTYPE html><html><head><title>{RequestHandler.StatusCode}</title></head><body>{RequestHandler.StatusCode}</body></html>";
                // convert response error body to array of bytes
                byteBody = System.Text.Encoding.ASCII.GetBytes(bodyError);
                // write to the log file
                Logger.WriteToLog("RESPONSE SENT", RequestHandler.StatusCode);
            }
            // build the response header
            strHeader = $"HTTP/1.1 {RequestHandler.StatusCode}\r\nContent-Type: {contentType}\r\nServer: {ServerHandler.Server}\r\nDate: {currentDateTime}\r\nContent-Length: {byteBody.Length}\r\n\r\n";
            // convert response header to array of bytes
            byteHeader = System.Text.Encoding.ASCII.GetBytes(strHeader);

            // return the full response with header and body in byte format
            return byteHeader.Concat(byteBody).ToArray();
        }


        /*  -- Method Header Comment
        Name	:	ValidExtension
        Purpose :	Finds the matching permitted file extension and returns the corresponding MIME type
        Inputs	:	string fileExtension     the file extension to be checked
        Outputs	:	None
        Returns	:	string value of the corresponding MIME type to the file extension
                    empty string if the extension is not permitted
        */
        public static string ValidExtension(string fileExtension)
        {
            // list of key value pairs containing valid filetypes and their MIME type
            List<KeyValuePair<string, string>> contentTypes = new List<KeyValuePair<string, string>>()
                {
                new KeyValuePair<string, string>(".txt" , "text/plain"),
                new KeyValuePair<string, string>(".html", "text/html"),
                new KeyValuePair<string, string>(".htm", "text/html"),
                new KeyValuePair<string, string>(".htmls", "text/html"),
                new KeyValuePair<string, string>(".jpg", "image/jpeg"),
                new KeyValuePair<string, string>(".jpeg", "image/jpeg"),
                new KeyValuePair<string, string>(".gif", "image/gif"),
                };
            // check the list for the file extension
            KeyValuePair<string, string> fileType = contentTypes.SingleOrDefault(value => value.Key == fileExtension);

            // if the file extension was foud return its associated MIME type
            if (fileType.Value != null)
            {
                return fileType.Value;
            }
            // file extension was not found, return a blank string
            else
            {
                return "";
            }
        }
    }
}
