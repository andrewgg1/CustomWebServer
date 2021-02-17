/*
 *	FILE				: Logger.cs
 *	PROJECT				: Web Design and Development PROG2001 - Assignment 6
 *	PROGRAMMER			: Andrew Gordon
 *	FIRST VERSION		: Nov. 23 2020
 *  LAST UPDATE         : Dec. 2, 2020
 *	DESCRIPTION			: Contains the Logger class and the methods needed to provide logging
 *	                      functionality to the server.
 */

using System;
using System.IO;
using System.Threading;

namespace myOwnWebServer
{
    /* ---------------------------------------------------------------------------------
    CLASS NAME  :	Logger
    PURPOSE     :	The purpose of this class is to provide logging functionality for
                    the web server. This includes creating the log file at serve start
                    and writing messages to the log file as the program runs.

                    Inlcudes properties:
                    - Mutex logMutex
                    - string defaultLogFile

                    Includes methods:
                    - CreateLogFile()
                    - WriteToLog()

    --------------------------------------------------------------------------------- */
    class Logger
    {
        private static readonly Mutex logMutex = new Mutex();                   // allows writing to the file
        private static readonly string defaultLogFile = "myOwnWebServer.log";   // default filename of the log file

        /*  -- Method Header Comment
        Name	:	CreateLogFile
        Purpose :	Creates the logfile at startup
        Inputs	:	string WebRoot      root folder location
                    string WebIP        server IP address
                    string WebPort      server port number
        Outputs	:	None
        Returns	:	None
        */
        public static void CreateLogFile(string webRoot, string webIP, string webPort)
        {
            // mutex wait
            logMutex.WaitOne();
            FileStream newLog = null;       // used to create the new log file
            try
            {
                // create and close enw log file
                newLog = File.Create(defaultLogFile);
                newLog.Close();
                // write server statup message
                WriteToLog("SERVER STARTED", $"webRoot:{webRoot} webIP:{webIP} webPort:{webPort}");
            }
            // write logfile exception messages to screen, as unable to write to log file
            catch (IOException ex)
            {
                Console.WriteLine("Logfile having IO problems");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Unexpected Logfile exception: {ex.Message}");
            }
            finally
            {
                // in case of file exceptions, attempt to close the file
                if(newLog != null)
                {
                    newLog.Close();
                }
            }
            // release mutex
            logMutex.ReleaseMutex();
        }

        /*  -- Method Header Comment
        Name	:	WriteToLog
        Purpose :	Appends new lines to the logfile
        Inputs	:	string logType          type of message to be written
                    string logMessage       body of the message to be written
        Outputs	:	None
        Returns	:	None
        */
        public static void WriteToLog(string logType, string logMessage)
        {
            // start mutex
            logMutex.WaitOne();

            try
            {
                // write to the log file
                using (StreamWriter writer = new StreamWriter(defaultLogFile, append: true))
                {
                    writer.WriteLine($"{DateTime.Now.ToString("yyyy'-'MM'-'dd HH':'mm':'ss")} [{logType}] - <{logMessage}>");
                }
            }
            // write logfile exception messages to screen, as unable to write to log file
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine("Logfile was unexpectedly disposed of");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Logfile having IO problems");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Logfile exception: {ex.Message}");
            }
            // release mutex
            logMutex.ReleaseMutex();
        }
    }
}
