using System.Collections.Generic;

namespace AAPTForNet.Models
{
    internal class DumpModel
    {
        public string FilePath { get; }
        public bool isSuccess { get; }
        public List<string> Messages { get; }

        internal DumpModel(string path, bool success, List<string> msg)
        {
            FilePath = path;
            isSuccess = success;
            Messages = msg;
        }
    }
}
