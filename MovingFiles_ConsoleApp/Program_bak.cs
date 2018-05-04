//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace MovingFiles_ConsoleApp
//{
    

//    public static class Program
//    {

//        public class KnownException : Exception
//        {
//            public KnownException(string message)
//            {
//                Console.WriteLine(Message);
//            }
//        }

//        static void Main(string[] args)
//        {
//            try
//            {
//                Process();
//            }
//            catch (KnownException kex)
//            {
//                 Console.WriteLine(kex.Message);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }

//            Console.ReadKey();
//        }

//        private static void Process()
//        {
//            //ResumeApp:

//            Console.WriteLine("Enter FileName:  -  Example: test.xml");
//            string file = Console.ReadLine();

//            Console.WriteLine("Enter Source File path:  -  Example D:\\Data\\Pretrial");
//            string sourceFile = Console.ReadLine();

//            Console.WriteLine("Enter Destination folder name:  -  Example: Imported");
//            string destinationFolderName = Console.ReadLine();

//            Console.WriteLine("Enter Prefix value:  -  Example: Empty or any value. Empty means that it will be assinged a default date. Custom dates: [date/yyyyHHmm]");
//            string fileprefix = Console.ReadLine();

//            Console.WriteLine("Enter Suffix value:  -  Example: Empty or any value. Empty means that it will be assinged a default date. Custom dates: [date/yyyyHHmm]");
//            string filesuffix = Console.ReadLine();

 
//            //string file; //= (string)FileName;
//            //string sourceFile;// = (string)SourcePath;                 // Source path that will be passed to the CLR
//            //string destinationFolderName;// = (string)DestinationSubFolder;       // Target path that will be passed to the CLR
//            //string fileprefix; // = (string)FilePrefix;
//            //string filesuffix; //= (string)FileSuffix;
//            //bool wasSuccess; // = (bool)Success;


//             string ReturnMsg; // = (string)ReturnMessage;
//             string destinationFolder;                  // fromPath holds only the path of source folder with out the file name
//             //string toPathToString;          // Convert to string in order to compare if target folder is subfolder of sourcer folder
//             string FileNameWithOutExtension;  // Hold the file name with out extension
//             string fileExtension;         // Hold the extension to validate if we are not moving xml files.

//             bool TestMode;
//             string NewFileName = null;

//             string prefix = "";
//             string Suffix = "";


//            int versionleadingzeroes = 3;


//            //Assign values to the variables

//            //file = "";
//            //sourceFile = "";
//            //destinationFolderName = "";

//            // hardcode variables if you will
//            //file = "test.pdf";
//            //sourceFile = @"D:\TempArea";
//            //destinationFolderName = "Imported";
//            //fileprefix = "[date/yyyyMMdd]_Victor_";
//            //filesuffix = "_[date/yyyyMMdd]_callegari";

//            TestMode = true;
//            ReturnMsg = null;

//            fileExtension = Path.GetExtension(file);
//            FileNameWithOutExtension = Path.GetFileNameWithoutExtension(file);


//            if (file.Length == 0)
//            {
//                //ReturnMsg = "The was not a value assigned to the file name";
//                throw new Exception("The was not a value assigned to the file name");
//            }

//            if (sourceFile.Length == 0)
//            {
//                //ReturnMsg = "The was not a value assigned for the sourcer path";
//                throw new Exception("The was not a value assigned for the sourcer path");
//            }

//            if (destinationFolderName.Length == 0)
//            {
//                //ReturnMsg = "The was not a value assigned for the destination folder";
//                throw new Exception("The was not a value assigned for the destination folder");
//            }



//            /*
//                 Patterns to handle tokens
//            */

//            // when file contains [date/] or [date]  -- Prefix
//            var pattern_fordefault = @"\[(.*?)\]";

//            // This pattern will work for prefix and suffix in order to extract the content inside brackets
//            var pattern_custom = @"\[date/(.*?)\]";


//            // when file contains [date/] or [date]  -- Prefix
//            var matches_in_prefix_default = Regex.Matches(fileprefix, pattern_fordefault, RegexOptions.IgnoreCase);
//            var customDate_for_prefix_default = "";
//            foreach (Match m in matches_in_prefix_default)
//            {
//                customDate_for_prefix_default = m.Groups[1].ToString();
//            }
//            var default_CustomDate_for_prefix = "[" + customDate_for_prefix_default + "]";
            
            
//            // when file contains [date/] or [date]  -- Suffix
//            var matches_in_suffix_default = Regex.Matches(filesuffix, pattern_fordefault, RegexOptions.IgnoreCase);
//            var customDate_for_suffix_default = "";
//            foreach (Match m in matches_in_suffix_default)
//            {
//                customDate_for_suffix_default = m.Groups[1].ToString();
//            }
//            var default_CustomDate_for_suffix = "[" + customDate_for_suffix_default + "]";



//            // assigning default velues for fileprefix
//            if ((default_CustomDate_for_prefix == "[date/]" || default_CustomDate_for_prefix == "[date]"))
//            {
//                prefix = Regex.Replace(fileprefix, @"\s*?(?:\(.*?\)|\[.*?\]|\{.*?\})", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

//            }
//            else
//            {
                
//                try
//                {

//                    var matches_in_prefix = Regex.Matches(default_CustomDate_for_prefix, pattern_custom, RegexOptions.IgnoreCase);
//                    var customDate_for_prefix = "";
//                    foreach (Match m in matches_in_prefix)
//                    {
//                        customDate_for_prefix = m.Groups[1].ToString();
//                    }

//                    prefix = Regex.Replace(fileprefix, @"\s*?(?:\(.*?\)|\[.*?\]|\{.*?\})", DateTime.Now.ToString(customDate_for_prefix));

//                }
//                catch (KnownException)
//                {
//                    throw new KnownException("Make sure the prefix value is valid for a file name.");
//                }   

//            }


//            // assigning default velues for filesuffix
//            if ((default_CustomDate_for_suffix == "[date/]" || default_CustomDate_for_suffix == "[date]"))
//            {
//                Suffix = Regex.Replace(filesuffix, @"\s*?(?:\(.*?\)|\[.*?\]|\{.*?\})", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
//            }
//            else
//            {
//                try
//                {
//                    if (NewFileName == null)
//                    {
//                        var matches_in_suffix = Regex.Matches(default_CustomDate_for_suffix, pattern_custom, RegexOptions.IgnoreCase);

//                        var customDate_for_suffix = "";
//                        foreach (Match m in matches_in_suffix)
//                        {
//                            customDate_for_suffix = m.Groups[1].ToString();
//                        }

//                        Suffix = Regex.Replace(filesuffix, @"\s*?(?:\(.*?\)|\[.*?\]|\{.*?\})", DateTime.Now.ToString(customDate_for_suffix));
//                    }
//                }
//                catch (KnownException)
//                {
//                    throw new KnownException("Make sure the suffix value is valid for a file name.");
//                }
//            }


//            // when fileprefix and filesuffix are empty
//            if ((fileprefix == "" && filesuffix == ""))
//            {
//                Suffix = DateTime.Now.ToString("yyyyMMdd_HHmmss");
//            }

          

//            // check subfolder of the source path
//            string[] folders = System.IO.Directory.GetDirectories(sourceFile, destinationFolderName, System.IO.SearchOption.AllDirectories);

//            if (folders.Length > 0)
//            {
//                destinationFolder = folders[0];
//            }
//            else
//                destinationFolder = null;



//            // get new file name here for output param called NewFileName

//            //construct the new name = replace(prefix,"[date/yyyyMMdd_HH_mm_ss]",YourDateText) + FileNameWithOutExtension
//            // + replace(suffix,"[date/yyyyMMdd_HH_mm_ss]",YourDateTimeText) + "." + fileExtension

//            NewFileName = prefix + Regex.Replace(file, FileNameWithOutExtension, FileNameWithOutExtension + Suffix);


//            // instantiating classes in order to take advantage their methods
//            DirectoryInfo dirFrom = new DirectoryInfo(sourceFile);

//            if (TestMode)
//            {
//                Console.WriteLine("This test mode: The new file name will be: " + NewFileName);
//                Console.ReadKey();
//                return;
//            }

//            // VALIDATIONS

//            //try
//            //{

//            // check if the source path exists.
//            if (dirFrom.Exists == false)
//            {
//                //ReturnMsg = "The source " + sourceFile + " folder does not exists.";
//                throw new Exception("The source " + sourceFile + " folder does not exists.");
//            }


//            string fileNameNoXML = null;
//            var sourcefiles = Directory.GetFiles(sourceFile, FileNameWithOutExtension + ".*");
//            if (sourcefiles.Length > 0)
//            {
//                fileNameNoXML = sourcefiles[0];
//            }
//            else
//                fileNameNoXML = null;


//            // this variable hold if there is a file that has the same name but with diferent extension in the source folder.
//            if ((fileNameNoXML != null) && (File.Exists(sourceFile + "\\" + file) == false))
//            {
//                //ReturnMsg = "There was a file named " + FileNameWithOutExtension.ToUpper() + " in the " + sourceFile +
//                //            " folder but its extension is not " + fileExtension;

//                throw new Exception("There was a file named " + FileNameWithOutExtension.ToUpper() + " in the " + sourceFile +
//                            " folder but its extension is not " + fileExtension);
//            }


//            // check if the destination folder exists
//            if (destinationFolder == null)
//            {
//                //ReturnMsg = "The " + destinationFolderName + " destination folder does not exists or it is not a subfolder of " + sourceFile;
//                throw new Exception("The " + destinationFolderName + " destination folder does not exists or it is not a subfolder of " + sourceFile);
//            }


//            // check if the file exists in the source folder.
//            if (File.Exists(sourceFile + "\\" + file) == false)
//            {
//                //ReturnMsg = "The " + file + " file name does not exists in the " + sourceFile + " folder";
//                throw new Exception("The " + file + " file name does not exists in the " + sourceFile + " folder");
//            }

//            //}
//            //catch (Exception)
//            //{
//            //    Console.WriteLine(ReturnMsg);
//            //    Console.ReadKey();
//            //}


//            // check if the file alerady exists in the destination folder.

//            List<int> versionList = new List<int>();
//            int lastverionNumber = 0;


//            // check if file already exists in the destination folder
//            // if so, loop throught the the files and assing a new version of it
//            if (File.Exists(destinationFolder + "\\" + NewFileName))
//            {

//                String searchPattern = String.Format("{0}*{1}", NewFileName.Replace(fileExtension, ""), fileExtension);

//                var targetfiles = Directory.GetFiles(destinationFolder, searchPattern);
//                List<string> list_targetfiles = new List<string>(targetfiles);
//                list_targetfiles.Remove(destinationFolder + "\\" + NewFileName);


//                if (list_targetfiles.Count > 0)
//                {
//                    foreach (var item in list_targetfiles)
//                    {
//                        int thisInt = int.Parse(Path.GetFileNameWithoutExtension(item).Replace(NewFileName.Replace(fileExtension, "") + "_v", ""));
//                        if (thisInt > lastverionNumber)
//                        {
//                            lastverionNumber = thisInt;
//                        }

//                        //versionList.Add(int.Parse(Path.GetFileNameWithoutExtension(item).Replace(NewFileName.Replace(fileExtension, "") + "_v", "")));
//                    }

//                    lastverionNumber += 1;
//                    //lastverionNumber = versionList.Count + 2;

//                }
//                else
//                    lastverionNumber = 2;


//                StringBuilder leadingZeros_builder = new StringBuilder();

//                for (int i = 0; i < versionleadingzeroes; i++)
//                {
//                    leadingZeros_builder.Append(i);
//                }

//                var leadingZeros = Regex.Replace(leadingZeros_builder.ToString(), @"[\d-]", "0");

//                var leadingZeros_to_VersionFileName = String.Format("{0:" + leadingZeros + "}", lastverionNumber);


//                // Assigning new file name
//                NewFileName = NewFileName.Replace(fileExtension, "") + "_v" + leadingZeros_to_VersionFileName + fileExtension;
//            }


//            //if (TestMode)
//            //{
//            //    Console.WriteLine("This test mode: The new file name will be: " + NewFileName);
//            //    Console.ReadKey();
//            //    return;
//            //}


//            // Process to move file from the source to the target folder
//            try
//            {
//                if (ReturnMsg == null)
//                {
//                    File.Move(sourceFile + "\\" + file, destinationFolder + "\\" + NewFileName);
//                    Console.WriteLine("The file was moved successfully");
//                }
//            }
//            catch (KnownException)
//            {
//                throw new KnownException("The process failed.");
//                //Console.WriteLine("The process failed: {0}", e.ToString());
//            }

//           // Console.ReadKey();
//        }
//    }
//}