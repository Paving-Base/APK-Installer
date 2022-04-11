using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.System.UserProfile;

namespace APKInstaller.Helpers
{
    public static class LanguageHelper
    {
        public static string AutoLanguageCode = "auto";

        public static List<string> SupportLanguages = new()
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
            "fi-FI",
            "fr-FR",
            "he-IL",
            "hu-HU",
            "id-ID",
            "it-IT",
            "ja-JP",
            "ko-KR",
            "nl-NL",
            "no-NO",
            "pl-PL",
            "pt-BR",
            "pt-PT",
            "ro-RO",
            "ru-RU",
            "sr-SP",
            "sv-SE",
            "tr-TR",
            "uk-UA",
            "vi-VN",
            "zh-CN",
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
            "fi, fi-fi",
            "fr, fr-be, fr-ca, fr-ch, fr-fr, fr-lu, fr-015, fr-cd, fr-ci, fr-cm, fr-ht, fr-ma, fr-mc, fr-ml, fr-re, frc-latn, frp-latn, fr-155, fr-029, fr-021, fr-011",
            "he, he-il",
            "hu, hu-hu",
            "id, id-id",
            "it, it-it, it-ch",
            "ja, ja-jp",
            "ko, ko-kr",
            "nl, nl-nl, nl-be",
            "nb, nb-no, nn, nn-no, no, no-no",
            "pl, pl-pl",
            "pt-br",
            "pt, pt-pt",
            "ro, ro-ro",
            "ru, ru-ru",
            "sr-Latn, sr-latn-cs, sr, sr-latn-ba, sr-latn-me, sr-latn-rs, sr-cyrl, sr-cyrl-ba, sr-cyrl-cs, sr-cyrl-me, sr-cyrl-rs",
            "sv, sv-se, sv-fi",
            "tr, tr-tr",
            "uk, uk-ua",
            "vi, vi-vn",
            "zh-Hans, zh-cn, zh-hans-cn, zh-sg, zh-hans-sg",
            "zh-Hant, zh-hk, zh-mo, zh-tw, zh-hant-hk, zh-hant-mo, zh-hant-tw"
        };

        public static List<CultureInfo> SupportCultures = SupportLanguages.Select(x => new CultureInfo(x)).ToList();

        public static string GetCurrentLanguage()
        {
            IReadOnlyList<string> languages = GlobalizationPreferences.Languages;
            foreach (string language in languages)
            {
                foreach (string code in SupportLanguageCodes)
                {
                    if (code.ToLower().Contains(language.ToLower()))
                    {
                        int temp = SupportLanguageCodes.IndexOf(code);
                        return SupportLanguages[temp];
                    }
                }
            }
            return SupportLanguages[6];
        }
    }
}
