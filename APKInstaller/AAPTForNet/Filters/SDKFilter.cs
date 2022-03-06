using AAPTForNet.Models;
using System;
using System.Collections.Generic;

namespace AAPTForNet.Filters
{
    internal class SDKFilter : BaseFilter
    {

        private readonly List<string> msgs = new List<string>();
        private string[] segments => String.Join(" ", msgs).Split(seperator);

        public override bool canHandle(string msg)
        {
            return msg.StartsWith("sdkVersion:") || msg.StartsWith("targetSdkVersion:");
        }

        public override void addMessage(string msg)
        {
            if (!msgs.Contains(msg))
            {
                msgs.Add(msg);
            }
        }

        public override ApkInfo getAPK()
        {
            return new ApkInfo()
            {
                MinSDK = SDKInfo.GetInfo(getMinSDKVersion()),
                TargetSDK = SDKInfo.GetInfo(getTargetSDKVersion())
            };
        }

        public override void clear() => msgs.Clear();

        private string getMinSDKVersion()
        {
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].Contains("sdkVersion"))
                {
                    return segments[++i];
                }
            }
            return string.Empty;
        }

        private string getTargetSDKVersion()
        {
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].Contains("targetSdkVersion"))
                {
                    return segments[++i];
                }
            }
            return string.Empty;
        }
    }
}
