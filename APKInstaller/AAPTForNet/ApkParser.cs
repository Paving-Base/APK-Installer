using AAPTForNet.Filters;
using AAPTForNet.Models;

using System.Collections.Generic;
using System.Linq;

namespace AAPTForNet
{
    /// <summary>
    /// Parse output messages from AAPTool
    /// </summary>
    internal class ApkParser
    {
        public static ApkInfo Parse(DumpModel model)
        {
            if (!model.isSuccess)
            {
                return new ApkInfo();
            }

            List<BaseFilter> filters = new List<BaseFilter>() {
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
                    if (f.canHandle(msg))
                    {
                        f.addMessage(msg);
                        break;
                    }
                }
            }

            return ApkInfo.Merge(filters.Select(f => f.getAPK()));
        }
    }
}

