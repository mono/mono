using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace System.Web.Util {
    internal class RegexUtil {

        // this method is for the regex match which accepts the pattern from developer
        // since asp.net doesn't have control of the regex pattern string and it is possible 
        // to take more than 2 sec to match a string, give developer option to set timeout value
        public static bool IsMatch(string stringToMatch, string pattern, RegexOptions regOption, int? timeoutInMillsec) {            
            int timeout = GetRegexTimeout(timeoutInMillsec);

            if (timeout > 0 || timeoutInMillsec.HasValue) {
                return Regex.IsMatch(stringToMatch, pattern, regOption, TimeSpan.FromMilliseconds((double)timeout));
            } else {
                return Regex.IsMatch(stringToMatch, pattern, regOption);
            }
        }

        public static Match Match(string stringToMatch, string pattern, RegexOptions regOption, int? timeoutInMillsec) {
            int timeout = GetRegexTimeout(timeoutInMillsec);

            if (timeout > 0 || timeoutInMillsec.HasValue) {
                return Regex.Match(stringToMatch, pattern, regOption, TimeSpan.FromMilliseconds((double)timeout));
            } else {
                return Regex.Match(stringToMatch, pattern, regOption);
            }
        }

        public static Regex CreateRegex(string pattern, RegexOptions option, int? timeoutInMillsec) {
            int timeout = GetRegexTimeout(timeoutInMillsec);

            if (timeout > 0 || timeoutInMillsec.HasValue) {
                return new Regex(pattern, option, TimeSpan.FromMilliseconds((double)timeout));
            } else {
                return new Regex(pattern, option);
            }
        }

        // This method is for the regex asp.net controls the regex pattern and it should NOT take longer than 2 secs to match the string
        // so no need for developer to specify a timeout value
        internal static Regex CreateRegex(string pattern, RegexOptions option) {
            return CreateRegex(pattern, option, null);
        }

        private static bool? _isRegexTimeoutSetInAppDomain;
        private static bool IsRegexTimeoutSetInAppDomain {
            get {
                if (!_isRegexTimeoutSetInAppDomain.HasValue) {
                    bool timeoutSetInAppDomain = false;
                    try {
                        timeoutSetInAppDomain = AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") != null;
                    } catch {
                    }
                    _isRegexTimeoutSetInAppDomain = timeoutSetInAppDomain;
                }
                return _isRegexTimeoutSetInAppDomain.Value;
            }
        }

        private static int GetRegexTimeout(int? timeoutInMillsec) {
            int timeout = -1;

            // here is the logic for using timeout in regex
            // 1. if the caller sets a timeout value, then we use it(this may cause Regex throw ArgumentOutOfRangeException, 
            // but developer will know what they need to do when seeing the exception)
            // 2. if there is global setting in AppDomain, we do nothing(leave it to Regex to handle the timeout)
            // 3. if the web app targets to 4.6.1+, then we set 2 secs timeout
            if (timeoutInMillsec.HasValue) {
                timeout = timeoutInMillsec.Value;
            } else {
                if (!IsRegexTimeoutSetInAppDomain && BinaryCompatibility.Current.TargetsAtLeastFramework461) {
                    timeout = 2000;
                }
            }
            return timeout;
        }
    }
}
