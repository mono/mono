//
//
//

using System;
using System.Text;

namespace Mono.Tools.LocaleBuilder {

        public class Entry {

                protected static String EncodeString (string str)
                {
                        if (str == null)
                                return String.Empty;

                        StringBuilder ret = new StringBuilder ();
                        byte [] ba = new UTF8Encoding ().GetBytes (str);
                        bool in_hex = false;
                        foreach (byte b in ba) {
                                if (b > 127 || (in_hex && is_hex (b))) {
                                        ret.AppendFormat ("\\x{0:x}", b);
                                        in_hex = true;
                                } else {
                                        if (b == '\\')
                                                ret.Append ('\\');
                                        ret.Append ((char) b);
                                        in_hex = false;
                                }
                        }
                        return ret.ToString ();
                }

                private static bool is_hex (int e)
		{
			return (e >= '0' && e <= '9') || (e >= 'A' && e <= 'F') || (e >= 'a' && e <= 'f');
		}
        }
}




