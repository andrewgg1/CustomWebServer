/*
 *	FILE				: Program.cs
 *	PROJECT				: Web Design and Development PROG2001 - Assignment 6
 *	PROGRAMMER			: Andrew Gordon
 *	FIRST VERSION		: Nov. 23 2020
 *  LAST UPDATE         : Dec. 2, 2020
 *	DESCRIPTION			: This program allows the running of a web server. GET requests
 *	                      can be made for text, html, jpg, and gif files. A logfile is
 *	                      created, keeping track of when the server starts, receives requests,
 *	                      sends responses, and encounters errors.
 */

using System;
using System.IO;
using System.Net.Sockets;

namespace myOwnWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // 3 arguments mandatory
            if (args.Length != 3)
            {
                return;
            }
            TcpListener server = null;  // server listener

            // parse arguments for incorrect formatting
            if (!ServerHandler.ParseArgs(args))
            {
                Console.WriteLine(@"There's a problem with the arguments. Format must be: -webRoot=C:\localWebSite -webIP=127.0.0.1 -webPort=5300");
                return;
            }

            // check the root directory exists
            if (!Directory.Exists(ServerHandler.WebRoot))
            {
                Console.WriteLine("WebRoot directory doesn't exist");
                return;
            }

            try
            {
                // start server
                server = new TcpListener(ServerHandler.WebIP, ServerHandler.WebPort);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // invalid IP Address or Port
                Console.WriteLine("Unable to start server, IP address or Port is invalid");
                return;
            }

            try
            {
                // start server and create the log file
                server.Start();
                Logger.CreateLogFile(ServerHandler.WebRoot, ServerHandler.WebIP.ToString(), ServerHandler.WebPort.ToString());
            }
            catch (SocketException ex)
            {
                // invalid IP Address or Port
                Console.WriteLine("Unable to start server, IP address or Port is invalid");
                return;
            }

            try
            {
                while (true)
                {
                    Socket client = server.AcceptSocket();
                    ServerHandler.ServerWorker(client);
                    RequestHandler.StatusCode = "";
                }
            }
            // catch any unexpected exceptions
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception: {ex.Message}");
                Logger.WriteToLog("ERROR MESSAGE", $"Unexpected exception: {ex.Message}");
                return;
            }
        }
    }
}
