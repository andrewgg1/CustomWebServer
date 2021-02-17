/*
 *	FILE				: ServerHandler.cs
 *	PROJECT				: Web Design and Development PROG2001 - Assignment 6
 *	PROGRAMMER			: Andrew Gordon
 *	FIRST VERSION		: Nov. 23 2020
 *  LAST UPDATE         : Dec. 2, 2020
 *	DESCRIPTION			: Contains the ServerHandler class, used to handle server duties
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace myOwnWebServer
{
    /* ---------------------------------------------------------------------------------
    CLASS NAME  :	ServerHandler
    PURPOSE     :	The purpose of this class is to provide server handling functionality.
                    This includes the main server worker: reading and writing from the
                    client socket. ParseArgs used to parse the incoming command line
                    arguments.

                    Includes properties:
                    - string Server
                    - string WebRoot
                    - IPAddress WebIP
                    - int WebPort

                    Includes methods:
                    - ServerWorker()
                    - ParseArgs()

    --------------------------------------------------------------------------------- */
    class ServerHandler
    {
        public static readonly string Server = "myOwnWebServer/1.0";       // server name
        public static string WebRoot { get; set; }                         // root location
        public static IPAddress WebIP { get; set; }                        // server ip address
        public static int WebPort { get; set; }                            // server port number

        /*  -- Method Header Comment
        Name	:	Worker
        Purpose :	Worker thread of the program. Controls starting and shutting down the server, receiving
                    and sending of the client, parsing and validating requests, and writing to the logfile.
        Inputs	:	Socket client         currently connected client
        Outputs	:	None
        Returns	:	None
        */
        public static void ServerWorker(Socket client)
        {
            // initialize values
            RequestHandler.Method = "";
            RequestHandler.Resource = "";
            RequestHandler.VersionNumber = "";
            string requestHeaderStr = "";   // buffer to contain the data as an ASCII string

            try
            {
                // Occasionally the browser will immediately reconnect to the server after the first
                // socket has been closed with a write-only socket. If that is the case, shut it down
                // as nothing can be done with it server side
                if (!client.Poll(1 * 1000 * 1000, SelectMode.SelectRead))
                {
                    // Socket is not readable
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    return;
                }

                // Buffer for reading data
                Byte[] byteData = new Byte[client.Available];

                // Loop to receive all the data sent by the client.
                while (client.Available > 0)
                {
                    requestHeaderStr += Encoding.ASCII.GetString(byteData, 0, client.Receive(byteData));
                }

                if (RequestHandler.ParseHeader(requestHeaderStr))
                {
                    RequestHandler.ValidateRequest(RequestHandler.Method, RequestHandler.Resource, RequestHandler.VersionNumber);
                }

                Logger.WriteToLog("REQUEST RECEIVED", $"method:{RequestHandler.Method} resource:{RequestHandler.Resource}");

                client.Send(ResponseHandler.BuildResponse(ServerHandler.WebRoot + RequestHandler.Resource));
            }
            // catch exceptions
            catch (SocketException ex)
            {
                Logger.WriteToLog("ERROR MESSAGE", $"Socket exception {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("ERROR MESSAGE", $"Unexpected exception: {ex.Message}");
            }

            // Shutdown and end connection
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        /*  -- Method Header Comment
        Name	:	ParseArgs
        Purpose :	Parses the passed argument values to ensure they match with the correct command line arguments format
        Inputs	:	string[] arg          raw arguments passed straight from the command line
                    string[] parsedArgs   will contain parsed arguments if successful
        Outputs	:	None
        Returns	:	bool    true        arguments were successfully parsed
                            false       arguments were unsuccessfully parsed
        */
        public static bool ParseArgs(string[] arg)
        {
            bool matchFound = false;                // flag if the arguments match the expected format
            string[] parsedArgs = new string[3];    // stores parsed arguments

            // check there are no null strings in arg
            foreach (string s in arg)
            {
                if (s == null)
                {
                    return matchFound;
                }
            }

            // Regex format array for the command line arguments
            Regex[] argRegex = { new Regex(@"(?<=-webRoot=).+"), new Regex(@"(?<=-webIP=)(.+)"), new Regex(@"(?<=-webPort=)(.+)") };

            int i = 0;      // counter used to set the correct array element to the current parsed value

            // loop through each regex format
            foreach (Regex r in argRegex)
            {
                matchFound = false;     // reset matchFound to false
                // loop through each command line argument
                foreach (string s in arg)
                {
                    // if these is a match
                    Match m = r.Match(s);
                    if (m.Success)
                    {
                        // save the value to the array
                        parsedArgs[i] = m.Groups[0].ToString();
                        // set matchFound to true
                        matchFound = true;
                        break;
                    }
                }
                // if a regex is unable to find a match, then return
                if (!matchFound)
                {
                    return matchFound;
                }
                ++i;
            }
            // try to parse the values to their corresponding property
            try
            {
                ServerHandler.WebRoot = parsedArgs[0];
                ServerHandler.WebIP = IPAddress.Parse(parsedArgs[1]);
                ServerHandler.WebPort = Int32.Parse(parsedArgs[2]);
                RequestHandler.StatusCode = "";
            }
            // catch any exceptions
            catch (FormatException ex)
            {
                // Parameters in incorrect format
                matchFound = false;
            }
            catch (OverflowException ex)
            {
                // Port value overflow
                matchFound = false;
            }
            catch (Exception ex)
            {
                // Parameters in incorrect format
                matchFound = false;
            }

            return matchFound;
        }
    }
}
