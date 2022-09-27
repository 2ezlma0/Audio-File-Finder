
namespace Local_file_reader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            string path = textBox1.Text;
            string tempPath = Path.Combine(Path.GetTempPath(), "aflchk.tmp"); //decided that 'aflchk.temp' (short for audiofilecheck) is the name of the temp file created in the user's temp folder

            if (path == "") //if the textbox is empty, assume they want to search the entire c drive (or whatever their main drive's letter is), hopefully they put something in the box though because searching the whole c drive is very slow atm
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                path = drives[0].Name;
            }

            if (TempSearch(path, tempPath)) //calls the function to check if the temp file exists, and if it does that the root path is contained within the temp file
            {
                MessageBox.Show("path already searched, please delete " + tempPath + " and relaunch the program if you believe audio files in directory have updated");
            }
            else
            {
                List<string> audioFiles = new List<string>(); //list for the audiofiles, used later
                List<string> accessableDirectories = CheckAccessableDirectories(Directory.GetDirectories(path)); //first layer of subdirectories that are accessable

                List<string> directoriesToCheck = accessableDirectories; //sets first list to check the accessable directories already found on the first layer
                List<string> checkedDirectories = new List<string>(); //list to store the directories that have been checked and are accessable
                List<string> tempDirectories = new List<string>(); // list to store temporary directories to add to checked directories
                bool finished = false; //assume unfinished
                while (!finished)
                {
                    for (int i = 0; i < directoriesToCheck.Count; i++)
                    {
                        tempDirectories = CheckAccessableDirectories(Directory.GetDirectories(directoriesToCheck[i]));
                        if (tempDirectories.Count > 0)
                        {
                            for (int j = 0; j < tempDirectories.Count; j++)
                            {
                                checkedDirectories.Add(tempDirectories[j]);
                            }
                        }
                    }

                    if (checkedDirectories.Count == 0)
                    {
                        finished = true;
                    }
                    else
                    {
                        for (int j = 0; j < checkedDirectories.Count; j++)
                        {
                            accessableDirectories.Add(checkedDirectories[j]); //adds checked directories of the next layer in the accessible directories list to check for audio files
                        }
                        directoriesToCheck = checkedDirectories; //sets the directories to check the next layer in                    
                    }
                    checkedDirectories = new List<string>(); //clears checked directories for the next loop (.Clear doesnt work for some reason because uhhh c#)
                }


                /* (old and broken attempt to try and get all layers of accessable subdirectories)
                
                List<string> tempList = accessableDirectories;
                List<string> tempList2 = new List<string>();
                List<string> tempList3 = new List<string>();
                int tempAmount = tempList.Count;
                bool finished = false;
                int count = 0;
                while (!finished)
                {
                    for (int i = 0; i < tempAmount; i++)
                    {
                        tempList3 = new List<string>();
                        tempList2 = CheckAccessableDirectories(Directory.GetDirectories(tempList[i]));
                        if (tempList2.Count > 0 && tempList2 != null)
                        {
                            for (int j = 0; j < tempList2.Count; j++)
                            {
                                tempList3.Add(tempList2[j]);
                                count++;
                            }
                        }
                    }
                    if (count == 0)
                    {
                        finished = true;
                    }
                    else
                    {
                        tempList = tempList3;
                        tempAmount = tempList3.Count;
                        count = 0;
                        for (int i = 0; i < tempList3.Count; i++)
                        {
                            accessableDirectories.Add(tempList3[i]);
                        }
                    }
                }
                */


                audioFiles = AudioFileSearch(path, accessableDirectories);
                string output = "";
                if (audioFiles.Count > 0)
                {
                    for (int i = 0; i < audioFiles.Count; i++)
                    {
                        output += audioFiles[i] + ", "; //puts the audiofiles list in a more readable format for outputting in final product
                    }
                }
               
                if(File.Exists(tempPath)) //if the temp file exists dont overwrite the text already existing in the file, but add the new search to it
                    File.WriteAllText(tempPath, File.ReadAllText(tempPath) + "\n" + path + "\n" + output); 
                else
                    File.WriteAllText(tempPath, path + "\n" + output);

                MessageBox.Show(tempPath); //for final product it will use spotify's c# API with this audio file list (https://johnnycrazy.github.io/SpotifyAPI-NET/) and wont just output the temp file location, this is just to make sure everything has worked correctly and to show some form of output
            }
        }

        private List<string> CheckAccessableDirectories(string[] directories)
        {
            List<string> accessableDirectories = new List<string>();
            bool accessable = false;
            for(int i = 0; i<directories.Length; i++)
            {
                accessable = false;

                try
                {
                    string[] temp = Directory.GetFiles(directories[i]);
                    if (temp != null) //checks if it can access files (and directories in the next try), if it cant read them the string array's value is null, therefore if it isnt then the directory is readable
                    {
                        accessable = true;
                    }
                }
                catch (UnauthorizedAccessException) //just in case it causes an error
                {

                }

                if (!accessable)
                {
                    try
                    {
                        string[] temp2 = Directory.GetDirectories(directories[i]);
                        if (temp2 != null)
                        {
                            accessable = true;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {

                    }
                }

                //if (directories[i] == "C:\\Documents and Settings" || directories[i] == "C:\\System Volume Information")
                //   accessable = false;

                if (accessable) //if accessable has been set to true because files/directories have been read without an error, adds the directory to the list that will be returned when the function ends
                    accessableDirectories.Add(directories[i]);
            }
            
            return accessableDirectories;
        }
           
        private List<string> AudioFileSearch(string root, List<string> directories)
        {
            List<string> audioFiles = new List<string>();
            string[] tempFiles = Directory.GetFiles(root, "*.mp3", SearchOption.TopDirectoryOnly); //goes through the root folder for audio files
            if (tempFiles.Length > 0)
            {
                for (int i = 0; i < tempFiles.Length; i++)
                {
                    audioFiles.Add(tempFiles[i]);
                }
            }

            string[] tempFiles1 = Directory.GetFiles(root, "*.wav" , SearchOption.TopDirectoryOnly);
            if (tempFiles1.Length > 0)
            {
                for (int i = 0; i < tempFiles1.Length; i++)
                {
                    audioFiles.Add(tempFiles1[i]);
                }
            }
            string[] tempFiles2 = Directory.GetFiles(root, "*.m4a", SearchOption.TopDirectoryOnly);
            if (tempFiles2.Length > 0)
            {
                for (int i = 0; i < tempFiles2.Length; i++)
                {
                    audioFiles.Add(tempFiles2[i]);
                }
            }

            for(int i = 0; i < directories.Count; i++) //goes through all the subdirectories from the root path (that have been checked that theyre readable) for audio files
            {
                string[] tempFiles3 = Directory.GetFiles(directories[i], "*.mp3", SearchOption.TopDirectoryOnly);
                if (tempFiles3.Length > 0)
                {
                    for (int j = 0; j < tempFiles3.Length; j++)
                    {
                        audioFiles.Add(tempFiles3[j]);
                    }
                }
                string[] tempFiles4 = Directory.GetFiles(directories[i], "*.wav", SearchOption.TopDirectoryOnly);
                if (tempFiles4.Length > 0)
                {
                    for (int j = 0; j < tempFiles4.Length; j++)
                    {
                        audioFiles.Add(tempFiles4[j]);
                    }
                }

                string[] tempFiles5 = Directory.GetFiles(directories[i], "*.m4a", SearchOption.TopDirectoryOnly);
                if (tempFiles5.Length > 0)
                {
                    for (int j = 0; j < tempFiles5.Length; j++)
                    {
                        audioFiles.Add(tempFiles5[j]);
                    }
                }
            }

            return audioFiles;
        }

        private static bool TempSearch(string path, string tempPath)
        {
            if (File.Exists(tempPath))
            {
                if (File.ReadAllText(tempPath).Contains(path)) //checks if the tempfile already contains the root path thats trying to be searched to save time
                {
                    return true;
                }
            }

            return false;
        }
    }
}