using System.Collections.Generic;

namespace AAPTForNet.Models
{
    internal class DumpModel
    {
        public string FilePath { get; }
        public bool IsSuccess { get; }
        public List<string> Messages { get; }

        internal DumpModel(string path, bool success, List<string> msg)
        {
            FilePath = path;
            IsSuccess = success;
            Messages = msg;
        }
    }
}
