using AAPTForNet.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Globalization;
using Windows.System.UserProfile;

namespace APKInstaller.Helpers
{
    public static class LanguageHelper
    {
        public const string AutoLanguageCode = "auto";
        public const string FallbackLanguageCode = "en-US";

        public static readonly List<string> SupportLanguages = new()
        {
            "af-ZA",
            "ar-SA",
            "ca-ES",
            "cs-CZ",
            "da-DK",
            "de-DE",
            "el-GR",
            "en-US",
            "es-ES",
            "fa-IR",
            "fi-FI",
            "fr-FR",
            "he-IL",
            "hu-HU",
            "id-ID",
            "it-IT",
            "ja-JP",
            "kaa-UZ",
            "ko-KR",
            "nl-NL",
            "no-NO",
            "pl-PL",
            "pt-BR",
            "pt-PT",
            "ro-RO",
            "ru-RU",
            "sv-SE",
            "tr-TR",
            "ug-CN",
            "uk-UA",
            "vi-VN",
            "zh-CN",
            "zh-Latn",
            "zh-TW"
        };

        private static readonly List<string> SupportLanguageCodes = new()
        {
            "af, af-za",
            "ar, ar-sa, ar-ae, ar-bh, ar-dz, ar-eg, ar-iq, ar-jo, ar-kw, ar-lb, ar-ly, ar-ma, ar-om, ar-qa, ar-sy, ar-tn, ar-ye",
            "ca, ca-es, ca-es-valencia",
            "cs, cs-cz",
            "da, da-dk",
            "de, de-at, de-ch, de-de, de-lu, de-li",
            "el, el-gr",
            "en, en-au, en-ca, en-gb, en-ie, en-in, en-nz, en-sg, en-us, en-za, en-bz, en-hk, en-id, en-jm, en-kz, en-mt, en-my, en-ph, en-pk, en-tt, en-vn, en-zw, en-053, en-021, en-029, en-011, en-018, en-014",
            "es, es-cl, es-co, es-es, es-mx, es-ar, es-bo, es-cr, es-do, es-ec, es-gt, es-hn, es-ni, es-pa, es-pe, es-pr, es-py, es-sv, es-us, es-uy, es-ve, es-019, es-419",
            "fa, fa-ir",
            "fi, fi-fi",
            "fr, fr-be, fr-ca, fr-ch, fr-fr, fr-lu, fr-015, fr-cd, fr-ci, fr-cm, fr-ht, fr-ma, fr-mc, fr-ml, fr-re, frc-latn, frp-latn, fr-155, fr-029, fr-021, fr-011",
            "he, he-il",
            "hu, hu-hu",
            "id, id-id",
            "it, it-it, it-ch",
            "ja, ja-jp",
            "kaa, kaa-uz, kaa-kz, kaa-tm",
            "ko, ko-kr",
            "nl, nl-nl, nl-be",
            "nb, nb-no, nn, nn-no, no, no-no",
            "pl, pl-pl",
            "pt-br",
            "pt, pt-pt",
            "ro, ro-ro",
            "ru, ru-ru",
            "sv, sv-se, sv-fi",
            "tr, tr-tr",
            "ug-arab, ug-cn, ug-cyrl, ug-latn",
            "uk, uk-ua",
            "vi, vi-vn",
            "zh-Hans, zh-cn, zh-hans-cn, zh-sg, zh-hans-sg",
            "zh-Latn, zh-latn-pinyin, zh-latn-cn, zh-latn-sg",
            "zh-Hant, zh-hk, zh-mo, zh-tw, zh-hant-hk, zh-hant-mo, zh-hant-tw"
        };

        public static readonly List<CultureInfo> SupportCultures = SupportLanguages.Select(x => new CultureInfo(x)).ToList();

        public static int FindIndexFromSupportLanguageCodes(string language) => SupportLanguageCodes.FindIndex(code => code.ToLowerInvariant().Split(", ").Contains(language.ToLowerInvariant()));

        public static string GetCurrentLanguage()
        {
            IReadOnlyList<string> languages = GlobalizationPreferences.Languages;
            foreach (string language in languages)
            {
                int temp = FindIndexFromSupportLanguageCodes(language);
                if (temp != -1)
                {
                    return SupportLanguages[temp];
                }
            }
            return FallbackLanguageCode;
        }

        public static string GetPrimaryLanguage()
        {
            string language = ApplicationLanguages.PrimaryLanguageOverride;
            if (string.IsNullOrWhiteSpace(language)) { return GetCurrentLanguage(); }
            int temp = FindIndexFromSupportLanguageCodes(language);
            return temp == -1 ? FallbackLanguageCode : SupportLanguages[temp];
        }

        public static string GetLocaleLabel(this ApkInfo info)
        {
            if (info.LocaleLabels.Any())
            {
                int index = -1;
                string language = ApplicationLanguages.PrimaryLanguageOverride;
                if (string.IsNullOrWhiteSpace(language))
                {
                    IReadOnlyList<string> languages = GlobalizationPreferences.Languages;
                    foreach (string lang in languages)
                    {
                        index = FindIndexFromSupportLanguageCodes(lang);
                        if (index != -1) { break; }
                    }
                }
                else
                {
                    index = FindIndexFromSupportLanguageCodes(language);
                }
                if (index != -1)
                {
                    string code = SupportLanguageCodes[index].ToLowerInvariant();
                    foreach (KeyValuePair<string, string> label in info.LocaleLabels)
                    {
                        if (code.ToLowerInvariant().Contains(label.Key.ToLowerInvariant()))
                        {
                            return label.Value;
                        }
                    }
                }
            }
            return info.AppName;
        }
    }
}
