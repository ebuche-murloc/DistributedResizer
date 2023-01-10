using System.IO;

namespace ResizerASPNET.Models
{
    public class IndexViewModel
    {
        public string RootFolder { get; private set; } = Environment.CurrentDirectory;
        public string Files { get; private set; }

        public IndexViewModel() 
        {
            Files = "423";
            string[] allfiles = Directory.GetDirectories(RootFolder);
            foreach (string filename in allfiles)
            {
                Files += filename + " ";
            }
        }
    }
}
