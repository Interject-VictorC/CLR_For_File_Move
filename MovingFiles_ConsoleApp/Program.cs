using CLR_MoveFile;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace MovingFiles_ConsoleApp
{

    public static class Program
    {
        static bool keepConsoleOpen = true;

        static string filename;
        static string sourcePath;
        static string destinationSubFolder;
        static string filePrefix;
        static string fileSuffix;
        static Int32 versionleadingzeroes;
        static bool testMode;

        public class KnownException : Exception
        {
            public KnownException(string message)
            {
                Console.WriteLine(Message);
            }
        }

        static void Main(string[] args)
        {

            try
            {
                do
                {
                    RunProcess();
                } while (keepConsoleOpen);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }

        }

        private static void RunProcess()
        {
            // local output variables
            bool success = false;
            string returnMessage = "";
            string newFileNameOut = "";

            try
            {

                // JSON examples for testing:

                /* 
                    /////////////////////////////
                    // TESTING REQUIRED FIELDS //
                    /////////////////////////////

                    {  
                       "FileName":"",
                       "SourcePath":"",
                       "DestinationSubFolder":"",
                       "FilePrefix":"Prefix_[date]_",
                       "FileSuffix":"",
                       "TestMode":false,
                       "Versionleadingzeroes":3
                    }

                    ***** COPY AND PASTE INTO THE COMMAND PROMPT *****
                    
                    {"FileName": "","SourcePath": "","DestinationSubFolder": "","FilePrefix": "Prefix_[date]_","FileSuffix": "","TestMode": false,"Versionleadingzeroes": 3}


                    ///////////////////////////////////////
                    //// ADDING DATESTAMP TO FILE NAME ////
                    ///////////////////////////////////////
                 
                    {  
                        "FileName":"test.pdf",
                        "SourcePath":"D:\\TempArea\\FilesToMove",
                        "DestinationSubFolder":"Imported",
                        "FilePrefix":"[date]",
                        "FileSuffix":"",
                        "TestMode":false,
                        "Versionleadingzeroes":3,
                    }

                    ***** COPY AND PASTE INTO THE COMMAND PROMPT *****

                    // Default datestamp
                    {"FileName": "test.pdf","SourcePath": "D:\\TempArea\\FilesToMove","DestinationSubFolder": "Imported","FilePrefix": "[date]","FileSuffix": "","TestMode": false,"Versionleadingzeroes": 3}

                    // Custom datestamp with formatting
                    {"FileName": "test.pdf","SourcePath": "D:\\TempArea\\FilesToMove","DestinationSubFolder": "Imported","FilePrefix": "[date/yyyyMMdd]","FileSuffix": "","TestMode": false,"Versionleadingzeroes": 3}

                    
                    ///////////////////////////////////////////////////
                    //// ADDING TEXt AROUND DATESTAMP TO FILE NAME ////
                    ///////////////////////////////////////////////////
                 
                    {  
                        "FileName":"test.pdf",
                        "SourcePath":"D:\\TempArea\\FilesToMove",
                        "DestinationSubFolder":"Imported",
                        "FilePrefix":"MyPrefix_[date]_",
                        "FileSuffix":"",
                        "TestMode":false,
                        "Versionleadingzeroes":3,
                    }

                    ***** COPY AND PASTE INTO THE COMMAND PROMPT *****

                    // Default datestamp
                    {"FileName": "test.pdf","SourcePath": "D:\\TempArea\\FilesToMove","DestinationSubFolder": "Imported","FilePrefix": "MyPrefix_[date]_","FileSuffix": "","TestMode": false,"Versionleadingzeroes": 3}


                 */

                Console.WriteLine("");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Enter JSON:");
                Console.ForegroundColor = ConsoleColor.White;
                string json = Console.ReadLine();

                try
                {
                    ParseJsonIntoParams(json);
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid JSON. Make sure JSON is well formatted.");
                    Console.ForegroundColor = ConsoleColor.White;
                    keepConsoleOpen = true;
                    return;
                }
                

                CLR_MoveFile.StoredProcedures.Process(
                     filename
                    , sourcePath
                    , destinationSubFolder
                    , filePrefix
                    , fileSuffix
                    , versionleadingzeroes
                    , testMode
                    , ref success
                    , ref returnMessage
                    , ref newFileNameOut
                );

                if (success)
                {
                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Success:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(success);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Returned Message:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(returnMessage);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Result:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(newFileNameOut);
                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Success:");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(success);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Returned Message:");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(returnMessage);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Result:");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(newFileNameOut);
                }

                keepConsoleOpen = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                keepConsoleOpen = false;
            }

        }

        private static void ParseJsonIntoParams(string json)
        {
            JObject jObj = JObject.Parse(json);

            filename = jObj["FileName"].ToString();
            sourcePath = jObj["SourcePath"].ToString();
            destinationSubFolder = jObj["DestinationSubFolder"].ToString();
            filePrefix = jObj["FilePrefix"].ToString();
            fileSuffix = jObj["FileSuffix"].ToString();
            versionleadingzeroes = (Int32)jObj["Versionleadingzeroes"];
            testMode = (bool)jObj["TestMode"];

        }

    }
}