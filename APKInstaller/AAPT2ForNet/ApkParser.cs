using AAPT2ForNet.Filters;
using AAPT2ForNet.Models;

using System.Collections.Generic;
using System.Linq;

namespace AAPT2ForNet
{
    /// <summary>
    /// Parse output messages from AAPTool
    /// </summary>
    internal class ApkParser
    {
        public static ApkInfo Parse(DumpModel model)
        {
            if (!model.IsSuccess)
            {
                return new ApkInfo();
            }

            List<BaseFilter> filters = new() {
                new ABIFilter(),
                new SDKFilter(),
                new PackageFilter(),
                new PermissionFilter(),
                new SupportScrFilter(),
                new ApplicationFilter()
            };

            foreach (string msg in model.Messages)
            {
                foreach (BaseFilter f in filters)
                {
                    if (f.CanHandle(msg))
                    {
                        f.AddMessage(msg);
                        break;
                    }
                }
            }

            return ApkInfo.Merge(filters.Select(f => f.GetAPK()));
        }
    }
}

