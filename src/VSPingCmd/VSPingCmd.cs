using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace VSPingCmd
{
    public class Params
    {
        /// <summary>
        /// This class handles the parsing and storage of input parameters. 
        /// </summary>
        public string inUrlListFile;
        public string outJsonResponseFile;
        public string jsonMappingFile;
        public string inputType;
        public string inputColumn;
        public bool writeInfo = false;
        public bool includeTags = false;
        public char delimiter = '\t'; // the default delimiter is \t

        public bool AllMandatoryParamsExist()
        {
            if (!string.IsNullOrEmpty(jsonMappingFile)) 
                includeTags = false; // If the user has specified a JSON mapping file, the tags shortcut can't be used and should instead be listed in the file
            if (string.IsNullOrEmpty(inUrlListFile) || string.IsNullOrEmpty(outJsonResponseFile) || string.IsNullOrEmpty(inputType))
                return false;// returns false if it doesnt have -i -t and -o
            return true;
        }
        public void PrintUsage()
        {
            Console.WriteLine("Supported parameters are:");
            Console.WriteLine("-i\tSpecify a single URL, local image, headered text file of URL's and image paths to search, or a local folder containing images");
            Console.WriteLine("-t\tSpceify whether you have entered a single image (image), headered file containing URL's/paths to images (file), or a local folder containing images (folder)");
            Console.WriteLine("-o\tSpecify where the response JSON will be saved");
            Console.WriteLine("-column\tSpecify the name of the column which contains your image URL's");
            Console.WriteLine("-delimiter\tSpecify what delimiter your file uses (default is tabs)");
            Console.WriteLine("-jsonpath\tSpecify a mapping file that contains '<outputColumnName> \\t <ApiResponseJsonPath>' mappings (1 per line). This allows extracting data from within JSON.\n Ex.");
            Console.WriteLine("\tJSON  $");
            Console.WriteLine("\tTags  $.tags[*].displayName");
            Console.WriteLine("\tEntityIds  $.tags[*].actions[?(@.actionType == 'Entity')].data.bingId");
            Console.WriteLine("If no mapping specified, out contains <url> <Json>");
            Console.WriteLine("-p\tPrint selected jsonpath information to the console");
            Console.WriteLine("-tags\tShortcut to only write tags to the output file. Only usable if a jsonpath mapping file isn't specified.");
        }
        public Params(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "-i": // REQUIRED: input (it can be a imagePath, image Uri, FolderPath, filePath where it has one ore multiple imagePaths or imageUri)
                        this.inUrlListFile = args[i + 1];
                        break;
                    case "-column": // describes the title of the column which contains all the queryImages [for files only]
                        this.inputColumn = args[i + 1];
                        break;
                    case "-delimiter": // describes which delimiter you are using to establish new columns [for default the program uses '\t', or tabs]
                        this.delimiter = char.Parse(args[i + 1]);
                        break;
                    case "-o": // REQUIRED: output (filePath where the output will be printed)
                        this.outJsonResponseFile = args[i + 1];
                        break;
                    case "-t": // REQUIRED: which type of input are you adding (ie: -t file/folder/image)
                        this.inputType = args[i + 1];
                        break;
                    case "-p": // print (prints the output also to the command prompt)
                        i--;
                        this.writeInfo = true;
                        break;
                    case "-jsonpath": // specify a file which defines what parts of the response JSON you want in the output file 
                        this.jsonMappingFile = args[i + 1];
                        break;
                    case "-tags": // Shortcut to only write tags to the output file, not usable along with -jsonpath
                        i--;
                        this.includeTags = true;
                        break;
                }
            }
        }
    }
    public class VSPingCmd
    {
        /// <summary>
        /// This class handles the main flow, invoking other classes to read the input, make the search, and write to the output
        /// </summary>
        private static JObject appConfigObject;
        public static JObject AppConfig
        {
            get
            {
                if (appConfigObject == null)
                {
                    appConfigObject = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(@"appConfig.json"));
                }
                return appConfigObject;
            }
        }
        private static void Main(string[] args)
        {
            Params cmdLine = new Params(args);
            if (!cmdLine.AllMandatoryParamsExist()) // Checks to see if the program has enough information to run
            {
                Console.WriteLine("Not all cmdLine params provided");
                return;
            }
            var map = BuildJsonPathMappings(cmdLine.jsonMappingFile, cmdLine.includeTags); // Creates a map which determines what values will be extracted and printed from the JSON

            IImageUriSource reader = GetImageSource(cmdLine, map); // Calls reader factory to return an appropriate reader for the input type specified by the user
            FileWriter myFileWriter = new FileWriter(cmdLine.outJsonResponseFile, cmdLine.delimiter, reader.HeaderColumns);
            var connection = new WebApi();
            foreach (IList<string> row in reader.Rows) // Searches all images in the specified input
            {
                if (!Uri.TryCreate(row[reader.ImageColumnIndex], UriKind.Absolute, out Uri uri)) // in case the URI is ill-formatted
                    continue;
                string response;
                try
                {
                    response = connection.Search(uri).Result; // Completes the Kapi search and returns the response JSON
                }
                catch
                {
                    Console.WriteLine($"Error. The file {uri} has been skipped.");
                    continue;
                }
                var output = myFileWriter.WriteRow(row, response, map); // Writes the appropriate portion of the response to the file
                if (cmdLine.writeInfo) // Prints information to the command line if -p was part of the parameters
                    Console.WriteLine(output);
            }
            myFileWriter.Close(); // Closes the writer after everything else is done
        }
        public static IImageUriSource GetImageSource(Params cmdLine, Dictionary<string, string> map)// Returns an appropriate "reader" for the input type
        {
            switch (cmdLine.inputType) // Creates a different reader for each input type
            {
                case "file":
                    return new FileReader(cmdLine.inUrlListFile, map, cmdLine.delimiter, cmdLine.inputColumn); 
                case "folder":
                    return new FolderReader(cmdLine.inUrlListFile, map, cmdLine.delimiter);
                case "image":
                    return new ImageReader(cmdLine.inUrlListFile, map, cmdLine.delimiter);
                default:
                    throw new ApplicationException("Inappropriate input type provided");
            }
        }
        private static Dictionary<string, string> BuildJsonPathMappings(string mappingFile, bool includeTags)// Creates a dictonary of values that'll be extracted from the response JSON
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(mappingFile))
            {
                string[] mappings = File.ReadAllLines(mappingFile);

                foreach (string line in mappings)
                {
                    if (line.Length > 0 && line[0] == '#') // Skips commented out lines
                        continue;

                    string[] fields = line.Split('\t');

                    if (fields.Length < 2) // Skips lines with too many columns
                        continue;

                    map.Add(fields[0], fields[1]);
                }
            }
            if (includeTags)
                map.Add("Tags", "$.tags[*].displayName"); // Prints just tags if the shortcut was specified
            if (map.Count == 0)
                map.Add("JSON", "$"); // Default case if no mappings are specified
            return map;
        }
    }
    public interface IImageUriSource // General interface for all readers
    {
        /// <summary>
        /// This interface contains the necessary features for the three current readers (possibly more in the future).
        /// </summary>
        IEnumerable<IList<string>> Rows { get; } // All the rows in the input file
        IEnumerable<string> HeaderColumns { get; } // The columns in the header of the input file
        int ImageColumnIndex { get; } // Keeps track of which column contains the image URLs
    }
    public class ImageReader : IImageUriSource // This class handles the reading of a single image, whether local or from the web
    {
        /// <summary>
        /// This class handles the reading of single images, whether from the web or locally
        /// </summary>
        private char delimiter;
        public ImageReader(string filePath, Dictionary<string, string> map, char delimiter) // Constructor for the class
        {
            this.delimiter = delimiter;
            this.HeaderColumns = new List<string>() { "Url" + this.delimiter + string.Join(this.delimiter, map.Keys) }; // Generates the header based on the parts of the JSON specified to print
            this.Rows = GetRowsFromFolder(filePath);
            this.ImageColumnIndex = 0;
        }
        private IEnumerable<IList<string>> GetRowsFromFolder(string folderPath) //Populates the rows of query images (which is in this case just one row)
        {
            var file = folderPath;
            var retVal = (file.Split(this.delimiter) as IList<string>);
            yield return retVal;
        }
        public int ImageColumnIndex { get; private set; } // Keeps track of which column contains the image URLs
        public IEnumerable<string> HeaderColumns { get; private set; } // The columns in the header of the input file
        public IEnumerable<IList<string>> Rows { get; private set; } // All the rows in the input file (in this case, just the one)
    } 
    public class FolderReader : IImageUriSource // This class handles the reading of every image in a local folder
    {
        /// <summary>
        /// This class handles the reading of local folders containing multiple images
        /// </summary>
        private char delimiter;
        public FolderReader(string folderPath, Dictionary<string, string> map, char delimiter) // Constructor for the class
        {
            this.delimiter = delimiter;
            this.HeaderColumns = new List<string>() { "Url" + this.delimiter + string.Join(this.delimiter, map.Keys) }; // Generates the header based on the parts of the JSON specified to print
            this.Rows = GetRowsFromFolder(folderPath);
            this.ImageColumnIndex = 0;
        }
        private IEnumerable<IList<string>> GetRowsFromFolder(string folderPath) // Populates the rows of query images
        {
            var folder = Directory.EnumerateFiles(folderPath);

            foreach (string file in folder) // Checks the folder for only image files
            {
                if (file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".gif") || file.EndsWith(".png")) // Checks for valid image files
                {
                    var retVal = (file.Split(this.delimiter) as IList<string>);
                    yield return retVal;
                }
            }
        }
        public int ImageColumnIndex { get; private set; } // Keeps track of which column contains the image URLs
        public IEnumerable<string> HeaderColumns { get; private set; } // The columns in the header of the input file
        public IEnumerable<IList<string>> Rows { get; private set; } // All the rows in the input file
    }
    public class FileReader : IImageUriSource // This class handles the reading of every image in a local file, TSV or otherwise
    {
        /// <summary>
        /// This class handles the reading of text files containing the URL's of multiple images, whether from the internet or locally
        /// </summary>
        private StreamReader sr;
        private char delimiter;
        private string URLColumn;
        public FileReader(string fileStream, Dictionary<string, string> map, char delimiter, string URLColumn) // Constructor for the class
        {
                this.sr = new StreamReader(fileStream);
                this.delimiter = delimiter;
                this.HeaderColumns = new List<string>(sr.ReadLine().Split(this.delimiter)) { string.Join(delimiter, map.Keys) }.ToArray();// Adds on additional header columns to the ones already in the file
                this.Rows = this.GetRowsFromFile();
                this.URLColumn = URLColumn;
                this.ImageColumnIndex = GetIndex();
        }
        public IEnumerable<string> HeaderColumns { get; } // The columns in the header of the input file
        public int ImageColumnIndex { get; private set; } // Keeps track of which column contains the image URLs
        public IEnumerable<IList<string>> Rows { get; private set; } //
        private int GetIndex() // Determines which column of the file contains the actual image URL's, based on command line arguments
        {
            int i = 0;
            foreach (string entry in this.HeaderColumns) // Checks each column of the header for the one with the URL
            {
                if (entry == URLColumn) // Reads the URL from the column specifed by the user to contain comments
                {
                    return i;
                }
                i++;
            }
            throw new ApplicationException($"The expected column {this.URLColumn} isn't found in the input file header"); 
        }
        private IEnumerable<IList<string>> GetRowsFromFile() // Populates the rows, with preexisting information in the input file
        {
            while(true)
            {
                var line = this.sr.ReadLine();

                if (line == null) // Skips empty lines
                {
                    sr.Close();
                    yield break;
                }

                if (line.Trim() == string.Empty) // Skips lines with nothing but whitespace
                    continue;
                
                var retVal = (line.Split(this.delimiter) as IList<string>);
                yield return retVal;
            }
        }
    }
    public class FileWriter // This class handles all the writing to a file
    {
        /// <summary>
        /// This class writes the recieved information to a file, combining previous information that was in the source file (if any) with the new JSON results from the search
        /// </summary>
        private StreamWriter sw;
        private char delimiter;
        public FileWriter(string filePath, char delimiter, IEnumerable<string> header) // Constructor for the class
        {
            this.sw = new StreamWriter(filePath);
            this.delimiter = delimiter;
            sw.WriteLine(string.Join(delimiter, header));
        }
        public void Close() // Closes the writer
        {
            sw?.Close();
        }
        public string WriteRow(IList<string> originalRow, string response, Dictionary<string, string> map) // Combines the inputs and writes a combined line to the file
        {
            var newColumns = new List<string>();
            foreach (var kvp in map) // Extract the requested values from the JSON
            {
                var jsonResponse = JObject.Parse(response);
                var values = jsonResponse.SelectTokens(kvp.Value).Select(j => j.ToString(Newtonsoft.Json.Formatting.None));

                string columnData = string.Join(",", values);

                newColumns.Add(columnData);
            }
            string outputLine = string.Join(delimiter, originalRow.Concat(newColumns)); // Combine the original information in the row (if any) with the new columns

            sw.WriteLine(outputLine); // Write the new line to the file
            return outputLine;
        }
    }
    public class WebApi
    {
        /// <summary>
        /// This class handles sending the image to the API and recieving the response from the API
        /// </summary>
        private string accessKey;
        private static Uri endpointUrl = new Uri("https://api.cognitive.microsoft.com/bing/v7.0/images/visualsearch");
        private HttpClient client;
       
        public WebApi() // Constructor for the class
        {
            client = new HttpClient();
            accessKey = VSPingCmd.AppConfig["accessKey"].ToString();

            if (   string.IsNullOrEmpty(accessKey) // Checks for valid length access key
                || accessKey.Length != 32) 
            {
                throw new ApplicationException("Invalid access key, please check your app.config");
            }

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", accessKey);
        }
        public async Task<string> Search(Uri imgUri) // This method sends the request to the server and returns a response
        {
            // Part #1 - Lets construct an object which carries the request. Read the MSDN documentation for the schema
            var request =
                new
                {
                    imageInfo = new
                    {
                        cropArea = new
                        {
                            top = 0.0,
                            left = 0.0,
                            right = 1.0,
                            bottom = 1.0
                        },
                        url = imgUri.IsFile ? (string)null : imgUri.ToString()
                    }
                };
            MultipartFormDataContent mfdc = new MultipartFormDataContent();

            // Part #2 - Add binary image file if using a local image
            // NOTE: the file needs to be an image file that is < 1MB
            if (imgUri.IsFile)
            {
                string path = imgUri.LocalPath;

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                var sizeMB = fs.Length / 1024.0 / 1024.0;

                if (sizeMB > 1.0) // Enforces file size restriction
                    throw new ApplicationException($"The file {imgUri.LocalPath} is greater than 1MB. Please resize it and try again");

                StreamContent sc = new StreamContent(fs);
                mfdc.Add(
                        sc,         // binay image path
                        "image",    // name = image
                        "image"     // filename = image
                    );
            }
            // Part #3 - Add KnowledgeRequest JSON object
            mfdc.Add(new StringContent(JsonConvert.SerializeObject(request)), "knowledgeRequest"); 

            // Part #4 - Invoke the service and read the response
            var response = await client.PostAsync(endpointUrl, mfdc);
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
