﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace MovingFiles_ConsoleApp
{

    public static class Program
    {
        static bool wasDLLHexProvided = false;
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

                if (json.ToLower() == "givememydllhex")
                {
                    if (!wasDLLHexProvided)
                    {

                        string clrProjectPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), @"..\"));
                        string dllFullPath = clrProjectPath + @"MovingFiles_CLR\bin\Debug\MovingFiles_CLR.dll";

                        if (File.Exists(dllFullPath) == false)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Assembly does not exists in the '~\\bin\\Debug' folder.");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("");
                            keepConsoleOpen = true;
                            return;
                        }

                        //Console.ForegroundColor = ConsoleColor.Yellow;
                        //string dllHashData = GenerateSHA512String(dllFullPath);
                        //Console.WriteLine(dllHashData);
                        //wasDLLHexProvided = true;
                        //return;

                        //Console.ForegroundColor = ConsoleColor.Yellow;
                        //string dllHexData = GetHexString(dllFullPath);
                        //Console.WriteLine(dllHexData);
                        //wasDLLHexProvided = true;
                        //return;

                        //WriteAllText creates a file, writes the specified string to the file,
                        //and then closes the file.    You do NOT need to call Flush() or Close().
                        string hexdataTxtFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), @"..\"));
                        string fullHexdataTxtFilePath = hexdataTxtFilePath + @"MovingFiles_ConsoleApp\dll_hex.txt";
                        System.IO.File.WriteAllText(fullHexdataTxtFilePath, GetHexString(dllFullPath));

                        // Open the stream and read it back.
                        Process.Start(fullHexdataTxtFilePath);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Hex data was generated. See dll_hex.txt file.");
                        wasDLLHexProvided = true;
                        return;

                    }
                }
                else
                {
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

        public static string GenerateSHA512String(string assemblyPath)
        {

            if (!Path.IsPathRooted(assemblyPath))
                assemblyPath = Path.Combine(Environment.CurrentDirectory, assemblyPath);

            //StringBuilder builder = new StringBuilder();
            //builder.Append("0x");
            List<byte> byteList = new List<byte>();
            byte[] hashValue;
            using (FileStream stream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int currentByte = stream.ReadByte();
                while (currentByte > -1)
                {
                    //builder.Append(currentByte.ToString("X2", CultureInfo.InvariantCulture));
                    currentByte = stream.ReadByte();
                    byteList.Add((byte)currentByte);
                }

                System.Security.Cryptography.SHA512 sha512 = System.Security.Cryptography.SHA512Managed.Create();
                byte[] bytes = byteList.ToArray(); //Encoding.UTF8.GetBytes(builder.ToString());
                hashValue = sha512.ComputeHash(bytes);
            }

            return GetStringFromHash(hashValue);
        }

        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        private static string GetHexString(string assemblyPath)
        {
            if (!Path.IsPathRooted(assemblyPath))
                assemblyPath = Path.Combine(Environment.CurrentDirectory, assemblyPath);

            StringBuilder builder = new StringBuilder();
            builder.Append("0x");

            using (FileStream stream = new FileStream(assemblyPath,
                  FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int currentByte = stream.ReadByte();
                while (currentByte > -1)
                {
                    builder.Append(currentByte.ToString("X2", CultureInfo.InvariantCulture));
                    currentByte = stream.ReadByte();
                }
            }

            return builder.ToString();
        }

        // Print the byte array in a readable format.
        //public static void PrintByteArray(byte[] array)
        //{
        //    int i;
        //    for (i = 0; i < array.Length; i++)
        //    {
        //        Console.Write(String.Format("{0:X2}", array[i]));
        //        if ((i % 4) == 3) Console.Write(" ");
        //    }
        //    Console.WriteLine();
        //}
    }
}