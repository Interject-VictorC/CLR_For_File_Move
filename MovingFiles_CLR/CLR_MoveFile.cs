
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CLR_MoveFile
{

    public class KnownException : Exception
    {
        string _Message;

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        public KnownException(string message)
        {
            this.Message = message;
        }
    }

    public partial class StoredProcedures
    {

        //////////////////////
        /// CLR PARAMETERS ///
        //////////////////////

        /*
        FileName:
                Type: String
                Usage: Full name of the file to be moved including the extension.
                Example: "test.txt"
        
        SourcePath:
                Type: String
                Usage: Fully qualified path where the file to be moved is located.
                Example: "D:\\TempArea\\FilesToMove"
                Note: When the CLR is being called from SQL Server use a single slash. "D:\TempArea\FilesToMove" 
        
        DestinationSubFolder:
                Type: String
                Usage: Name of the folder where the file will be moved to. Must be a direct SubFolder of the SourcePath.
                Example: "D:\\TempArea\\FilesToMove\Imported". "Imported" would be the DestinationSubFolder.
        
        FilePrefix: (Optional)
                Type: String
                Usage: Any legal characters and/or a date stamp to be added at the beginning of the file name.
                Examples: "VictorC_", "VictorC_[date]_" or "VictorC_[date/yyyyMMdd]_"
                Note: The [date] and [date/] are patterns that will be replaced with the current date stamp.
        
        FileSuffix: (Optional)
                Type: String
                Usage: Any legal characters and/or a date stamp to be added at the end of the file name.
                Examples: "_VictorC", "_VictorC_[date]" or "_VictorC_[date/yyyyMMdd]"
                Note: The [date] and [date/] are patterns that will be replaced with the current date stamp.
        
        VersionLeadingZeroes:
                Type: Integer
                Usage: The number of digits to be use in the file version number.
                Example: "3", the next version number will be “test_v002.txt”.
                Note: A version number will only be added if a file exists in the destination folder with the same name as the file being moved.
        
        TestMode:
                Type: Boolean
                Usage: Determines if the file will be moved or not.
                       A value of "true", the CLR will return the final file name WITHOUT moving the file from the SourcePath.
                       A value of "false", the CLR will move the file from the SourcePath to the DestinationSubFolder.
               
        Success:
                Type: Boolean
                Usage: Output parameter.
                       Returns FALSE if errors were found.
                       Returns TRUE if there were no errors during processing.
        
        ReturnMessage:
                Type: String
                Usage: Output parameter.
                       Returns any error message(s) found during processing.
        
        NewFileNameOut:
                Type: String
                Usage: Output parameter.
                       Returns the final FileName whether the file was moved or not.
        
        *///CLR Parameters

        [Microsoft.SqlServer.Server.SqlProcedure]
        public static void CLR_MoveFile(
             SqlString FileName
            ,SqlString SourcePath
            ,SqlString DestinationSubFolder
            ,SqlString FilePrefix
            ,SqlString FileSuffix
            ,SqlInt32 VersionLeadingZeroes
            ,SqlBoolean TestMode
            ,out SqlBoolean Success
            ,out SqlString ReturnMessage
            ,out SqlString NewFileNameOut)
        {

            // Initialize output parameters
            Success = false;
            ReturnMessage = "";
            NewFileNameOut = "";

            try
            {

                // Local output variables.
                bool success = false;
                string returnMessage = "";
                string newFileNameOut = "";

                Process(
                         (string)FileName
                        ,(string)SourcePath
                        ,(string)DestinationSubFolder
                        ,(string)FilePrefix
                        ,(string)FileSuffix
                        ,(Int32)VersionLeadingZeroes
                        ,(bool)TestMode
                        ,ref success
                        ,ref returnMessage
                        ,ref newFileNameOut
                      );

                Success = success;
                ReturnMessage = returnMessage;
                NewFileNameOut = newFileNameOut;

            }
            catch (KnownException kex)
            {
                Success = false;
                ReturnMessage = kex.Message;
            }
            catch (Exception ex)
            {
                Success = false;
                ReturnMessage = ex.Message;
            }

        }

        public static void Process(
             string fileName
            , string sourcePath
            , string destinationSubFolder
            , string filePrefix
            , string fileSuffix
            , Int32 versionLeadingZeroes
            , bool testMode
            , ref bool success
            , ref string returnMessage
            , ref string newFileNameOut)
        {

            // Declare local variables.
            string errorMsg = null;          // Validation errors will be returned to SQL stored procedure.
            string destinationFolder;        // Path of the folder without the fileName
            string fileNameWithOutExtension; // The fileName value name without extension
            string fileExtension;            // Extension holds the extension of the file name
            string newFileName = null;       // Name of the imported file in the destination folder

            ///////////////////////////
            //// START VALIDATION /////
            ///////////////////////////

            // Validate required parameters
            if (fileName.Length == 0)
            {
                errorMsg += "FileName parameter is required." + Environment.NewLine;
            }

            if (sourcePath.Length == 0)
            {
                errorMsg += "SourcePath parameter is required." + Environment.NewLine;
            }

            if (destinationSubFolder.Length == 0)
            {
                errorMsg += "DestinationFolder parameter is required." + Environment.NewLine;
            }

            // Validate invalid characters
            //if (HasInvalidCharacter(fileName) ||
            //    HasInvalidCharacter(sourcePath) ||
            //    HasInvalidCharacter(destinationSubFolder) ||
            //    HasInvalidCharacter(filePrefix) ||
            //    HasInvalidCharacter(fileSuffix))
            //{
            //    errorMsg += "An invalid character was found in your parameters." + Environment.NewLine;
            //}

            if (HasInvalidCharacter(fileName))
            {
                errorMsg += "An invalid character in fileName." + Environment.NewLine;
            }

            if (HasInvalidCharacter(sourcePath))
            {
                errorMsg += "An invalid character in sourcePath." + Environment.NewLine;
            }

            if (HasInvalidCharacter(destinationSubFolder))
            {
                errorMsg += "An invalid character in destinationSubFolder." + Environment.NewLine;
            }

            if (HasInvalidCharacter(filePrefix))
            {
                errorMsg += "An invalid character in filePrefix" + Environment.NewLine;
            }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                success = false;
                returnMessage = errorMsg.TrimEnd('\n');
                newFileNameOut = "Unknown";
                return;
            }

            // Assign values to the local variables
            fileExtension = Path.GetExtension(fileName); // extract filename extension
            fileNameWithOutExtension = Path.GetFileNameWithoutExtension(fileName);

            ////////////////////////////////////
            /////****** IMPORTANT *******///////
            ////////////////////////////////////
            
            // Check if sourcePath is part of the System folder.
            // NOTE: System folders are defined (HARCODED) in a list in the IsSystemFolder function
            // and do not guarantee safe use.
            if (IsSystemFolder(sourcePath))
            {
                errorMsg += "CLR cannot operate using system folder." + Environment.NewLine;
            }

            // Check if the sourcePath exists.
            if (Directory.Exists(sourcePath) == false)
            {
                errorMsg += "SourcePath " + sourcePath + " does not exist." + Environment.NewLine;
            }

            // Check if the fileName exists in the sourcePath.
            if (File.Exists(sourcePath + "\\" + fileName) == false)
            {
                errorMsg += "FileName " + fileName + " does not exist in SourcePath " + sourcePath + " ." + Environment.NewLine;
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////
            // **************************************** IMPORTANT **************************************** //
            /////////////////////////////////////////////////////////////////////////////////////////////////
            // Check if the destinationFolder is inside the main folder. This only checks one folder deep. //
            //                                                                                             //
            // ------------------------------------- EXAMPLE SCENARIO ------------------------------------ //
            // Existing folder structure: "D:\\FileToMove\\Imported\MoveToHere"                            //
            // SourcePath = "D:\\FileToMove"                                                               //
            // DestinationSubFolder = "MoveToHere"                                                         //
            // This will cause an error as the "MoveToHere" folder is more than one folder deep. In this   //
            // scenario, "Imported" would be an acceptable value for DestinationSubFolder.                 //
            /////////////////////////////////////////////////////////////////////////////////////////////////

            // Directory.GetDirectories: Returns the names of subdirectories (including their paths) in the specified directory.
            string[] folders = Directory.GetDirectories(sourcePath, destinationSubFolder, SearchOption.AllDirectories);

            if (folders.Length > 0)
            {
                destinationFolder = folders[0];
            }
            else
            {
                destinationFolder = null;
                errorMsg += "Destination folder does not exist or it is not a subfolder of SourcePath " + sourcePath + "." + Environment.NewLine;
            }


            ////////////////////////////////////////
            /////// STAMP for Prefix/Suffix ////////
            ////////////////////////////////////////

            // Prefix and suffix hold the final date, text, or date and text as a stamp for the filename.
            string prefix = ""; 
            string suffix = "";

            try
            {
                // GetStampForPrefixAndSuffix replaces the [date] token with a datestamp and joins the other text as provided.
                // Custom formatting such as [date/yyyyMMdd] can be used anywere in the parameter.
                // For example: filePrefix = "myprefix_[date/yyyyMMdd]"
                //              Result = myprefix_20180503
                // If [date] is used without custom formatting, the current date is returned in this format "yyyyMMdd_HHmmss"
                prefix = GetStampForPrefixAndSuffix(filePrefix);
                suffix = GetStampForPrefixAndSuffix(fileSuffix);
            }
            catch (KnownException)
            {
                errorMsg = "Make sure the prefix/suffix are valid." + Environment.NewLine;
            }


            // Default when filePrefix and fileSuffix are empty.
            if ((filePrefix == "" && fileSuffix == ""))
            {
                suffix = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            }


            ////////////////////////////////////////
            // GET FINAL NEW FILE NAME VERSIONING //
            ////////////////////////////////////////

            // Set new fileName here for output param called newFileName.
            newFileName = prefix + Regex.Replace(fileName, fileNameWithOutExtension, fileNameWithOutExtension + suffix);


            /////////////////////
            // FILE VERSIONING //
            /////////////////////

            // RenameForVersioning adds a version number if file already exists in the destination folder.
            newFileName = RenameForVersioning(versionLeadingZeroes, destinationFolder, fileExtension, newFileName);


            ///////////////
            // TEST MODE //
            ///////////////
            
            //Determines if the file will be moved or not based on user preference.           
            if (testMode)
            {
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    success = false;
                    returnMessage = errorMsg.TrimEnd('\n');
                    newFileNameOut = "Unknown";
                    return;
                }
                else
                {
                    success = true;
                    returnMessage = "You are in Test Mode!!";
                    newFileNameOut = newFileName;
                    return;
                }

            }
            

            // Process to move fileName from the source to the target folder.
            try
            {
                if (errorMsg == null)
                {
                    File.Move(sourcePath + "\\" + fileName, destinationFolder + "\\" + newFileName);

                    success = true;
                    returnMessage = "File has been moved successfully";
                    newFileNameOut = newFileName;

                }
                else
                {
                    success = false;
                    returnMessage = errorMsg.TrimEnd('\n');
                    newFileNameOut = "Unknown";
                    return;
                }

            }
            catch (Exception)
            {
                success = false;
                returnMessage = "The process failed. Process Abort";
            }
        }

        // RenameForVersioning will rename the file in the case where the file already exists in the destinationFolder.
        private static string RenameForVersioning(int versionLeadingZeroes, string destinationFolder, string fileExtension, string newFileName)
        {
            // Check if the fileName already exists in the destinationFolder.
            int lastverionNumber = 0;

            // Check if fileName already exists in the destinationFolder.
            // If so, loop through the files and assign a new version number.
            if (File.Exists(destinationFolder + "\\" + newFileName))
            {

                String searchPattern = String.Format("{0}*{1}", newFileName.Replace(fileExtension, ""), fileExtension);

                var targetfiles = Directory.GetFiles(destinationFolder, searchPattern);
                List<string> list_targetfiles = new List<string>(targetfiles);
                list_targetfiles.Remove(destinationFolder + "\\" + newFileName);


                if (list_targetfiles.Count > 0)
                {
                    foreach (var item in list_targetfiles)
                    {
                        int thisInt = int.Parse(Path.GetFileNameWithoutExtension(item).Replace(newFileName.Replace(fileExtension, "") + "_v", ""));
                        if (thisInt > lastverionNumber)
                        {
                            lastverionNumber = thisInt;
                        }
                    }

                    lastverionNumber += 1;

                }
                else
                {
                    lastverionNumber = 2;
                }

                // Create the version name for the files in case it needed.
                StringBuilder leadingZeros_builder = new StringBuilder();

                for (int i = 0; i < versionLeadingZeroes; i++)
                {
                    leadingZeros_builder.Append(i);
                }

                var leadingZeros = Regex.Replace(leadingZeros_builder.ToString(), @"[\d-]", "0");

                var leadingZeros_to_VersionFileName = String.Format("{0:" + leadingZeros + "}", lastverionNumber);


                // Assigning new fileName value.
                newFileName = newFileName.Replace(fileExtension, "") + "_v" + leadingZeros_to_VersionFileName + fileExtension;
            }

            return newFileName;
        }

        // GetFinalDateStamp returns a default or custom date stamp for the final fileName.
        private static string GetStampForPrefixAndSuffix(string stamp)
        {

            // Pattern to look for a hardcoded "[date]" or "[date/]" in the FilePrefix or FileSuffix.
            // If the value is equal to that then we are defaulting a date stamp.
            var pattern_default = @"\[(.*?)\]";

            var matches_in_stamp = Regex.Matches(stamp, pattern_default, RegexOptions.IgnoreCase);
            var stamp_default = "";
            foreach (Match m in matches_in_stamp)
            {
                stamp_default = m.Groups[1].ToString();
            }

            // clean_stamp also will be use for the custom date stamp
            string clean_stamp = "[" + stamp_default + "]";

            if ((clean_stamp == "[date/]" || clean_stamp == "[date]"))
            {
                //return Regex.Replace(stamp, @"\s*?(?:\(.*?\)|\[.*?\]|\{.*?\})", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                return Regex.Replace(stamp, @"\s*?(?:\(.*?\)|\[.*?\]|\{.*?\})", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            }

            // Pattern to look for a hardcoded "[date]" in the FilePrefix or FileSuffix.
            // The content inside brackets after "date/" will be replaced with the date in the format specified.
            // For example: yymmdd, yy-mm-dd, etc.
            var pattern_custom = @"\[date/(.*?)\]";

            var matches_in_clean_stamp = Regex.Matches(clean_stamp, pattern_custom, RegexOptions.IgnoreCase);
            var customStamp = "";
            foreach (Match m in matches_in_clean_stamp)
            {
                customStamp = m.Groups[1].ToString();
            }

            return Regex.Replace(stamp, @"\s*?(?:\(.*?\)|\[.*?\]|\{.*?\})", DateTime.Now.ToString(customStamp));

        }

        // Checks if the first folder in sourcePath is a system folder.
        private static bool IsSystemFolder(string sourcePath)
        {

            // Get parent folder of the sourcePath.
            string parentFolder_local_drive;
            DirectoryInfo directoryInfo = Directory.GetParent(sourcePath);
            parentFolder_local_drive = directoryInfo.FullName;

            // Get parent folder of the passed shared drive path.
            string parentFolder_share_drive = "";
            
            if (sourcePath.StartsWith(@"\"))
            {
                string[] sharedParentFolder;
                sharedParentFolder = sourcePath.Split(Path.DirectorySeparatorChar);
                parentFolder_share_drive = "\\" + sharedParentFolder[1];
            }

            string[] system_folders = new string[]
            {
                "C:\\inetpub",
                "C:\\Program Files",
                "C:\\Program Files (x86)",
                "C:\\ProgramData",
                "C:\\SymCache",
                "C:\\Users",
                "\\inetpub",
                "\\Program Files",
                "\\Program Files (x86)",
                "\\ProgramData",
                "\\SymCache",
                "\\Users"
            };

            for (int i = 0; i < system_folders.Length; i++)
            {
                string element = system_folders[i];
                if (element == parentFolder_local_drive || element == parentFolder_share_drive)
                {
                    return true;
                }
            }

            return false;

        }

        // Search for invalid characters in all the parametrs
        private static bool HasInvalidCharacter(string param)
        {
            Regex regex = new Regex(string.Format("[{0}]", Regex.Escape("{}?@#$%^&*()~!<>;")));
            Match match = regex.Match(param);
            return match.Success;
        }
    }

}
