// PackUriParser.cs created with MonoDevelop
// User: alan at 14:50 31/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO.Packaging
{
    class PackUriParser : System.GenericUriParser
    {
        const string SchemaName = "pack";

        StringBuilder builder = new StringBuilder();

        public PackUriParser ()
            : base (GenericUriParserOptions.Default)
        {
        }
        
        protected override string GetComponents(Uri uri, UriComponents components, UriFormat format)
        {
            string s = uri.OriginalString;
            builder.Remove(0, builder.Length);

            if ((components & UriComponents.Scheme) == UriComponents.Scheme)
            {
                int start = 0;
                int end = s.IndexOf(':');
                builder.Append(s, start, end - start);
            }

            if ((components & UriComponents.Host) == UriComponents.Host)
            {
                // Skip past pack://
                int start = 7;
                int end = s.IndexOf('/', start);
                if (end == -1)
                    end = s.Length;

                if (builder.Length > 0)
                    builder.Append("://");

                builder.Append(s, start, end - start);
            }

            // Port is always -1, so i think i can ignore both Port and StrongPort
            // Normally they'd get parsed here

            if ((components & UriComponents.Path) == UriComponents.Path)
            {
                // Skip past pack://
                int start = s.IndexOf('/', 7);
                int end = s.IndexOf('?');
                if (end == -1)
                    end = s.IndexOf('#');
                if (end == -1)
                    end = s.Length;

                if ((components & UriComponents.KeepDelimiter) != UriComponents.KeepDelimiter &&
                    builder.Length == 0)
                    start++;

                if (start > 0) builder.Append(s, start, end - start);
            }

            if ((components & UriComponents.Query) == UriComponents.Query)
            {
                int index = s.IndexOf('?');

                if (index != -1)
                {
                        if ((components & UriComponents.KeepDelimiter) != UriComponents.KeepDelimiter &&
                            builder.Length == 0)
                                index++;

                        int fragIndex = s.IndexOf('#');
                        int end = fragIndex == -1 ? s.Length : fragIndex;
                        builder.Append(s, index, end - index);
                }
            }

            if ((components & UriComponents.Fragment) == UriComponents.Fragment)
            {
                int index = s.IndexOf('#');

                if (index != -1)
                {
                        if ((components & UriComponents.KeepDelimiter) != UriComponents.KeepDelimiter &&
                            builder.Length == 0)
                                index++;

                        builder.Append(s, index, s.Length - index);
                }
            }

            return builder.ToString();
        }
        
        protected override void InitializeAndValidate(Uri uri, out UriFormatException parsingError)
        {
            parsingError = null;
        }

        protected override UriParser OnNewUri()
        {
            return new PackUriParser();
        }
    }
}
