//
// I18N.CJK.KSConvert
//
// Author:
//   Hye-Shik Chang <perky@FreeBSD.org>
//

using System;

namespace I18N.CJK
{
    internal sealed class KSConvert : DbcsConvert
    {
        // Dummy constructor, no one is aupposed to call it
        private KSConvert() : base("ks.table") {}
        
        // The one and only KS conversion object in the system.
        private static DbcsConvert convert;
        
        // Get the primary KS conversion object.
        public static DbcsConvert Convert
        {
            get {
                if (convert == null) convert = new DbcsConvert("ks.table");
                return convert;
            }
        }
    }
}

// ex: ts=8 sts=4 et
