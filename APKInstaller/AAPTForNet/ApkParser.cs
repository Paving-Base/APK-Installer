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
            if (!model.IsSuccess)
            {
                return new ApkInfo();
            }

            List<BaseFilter> filters = new() {
                new PermissionFilter(),
                new LabelFilter(),
                new FeatureFilter(),
                new SDKFilter(),
                new PackageFilter(),
                new ApplicationFilter(),
                new SupportScrFilter(),
                new LocaleFilter(),
                new DensityFilter(),
                new ABIFilter(),
                new LaunchableFilter()
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

