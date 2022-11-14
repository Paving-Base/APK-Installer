using AAPT2ForNet.Models;

namespace AAPT2ForNet.Filters
{
    internal abstract class BaseFilter
    {
        protected const char Seperator = '\'';
        protected const string DefaultEmptyValue = "Unknown";

        public abstract bool CanHandle(string msg);
        public abstract void AddMessage(string msg);
        public abstract ApkInfo GetAPK();

        /// <summary>
        /// Test in loop
        /// </summary>
        public abstract void Clear();
    }
}
