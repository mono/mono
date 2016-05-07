//------------------------------------------------------------------------------
// <copyright file="UriUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Linq;
    using System.Text;

    // Contains helpers for URI generation and parsing

    internal static class UriUtil {

        private static readonly char[] _queryFragmentSeparators = new char[] { '?', '#' };

        // Similar to UriBuilder, but contains semantics specific to generation
        // of the Request.Url property.
        internal static Uri BuildUri(string scheme, string serverName, string port, string path, string queryString) {
            return BuildUriImpl(scheme, serverName, port, path, queryString, AppSettings.UseLegacyRequestUrlGeneration);
        }

        // for unit testing
        internal static Uri BuildUriImpl(string scheme, string serverName, string port, string path, string queryString, bool useLegacyRequestUrlGeneration) {
            Debug.Assert(!String.IsNullOrEmpty(scheme));
            Debug.Assert(!String.IsNullOrEmpty(serverName));
            Debug.Assert(!String.IsNullOrEmpty(path));

            if (!useLegacyRequestUrlGeneration) {
                if (path != null) {
                    // The path that is provided to us is expected to be in an already-decoded
                    // state, but the Uri class expects encoded input, so we'll re-encode.
                    // This removes ambiguity that can lead to unintentional double-unescaping.
                    path = EscapeForPath(path);
                }

                if (queryString != null) {
                    // Need to replace any stray '#' characters that appear in the
                    // query string so that we don't end up accidentally generating
                    // a fragment in the resulting URI.
                    string reencodedQueryString = queryString.Replace("#", "%23");
                    queryString = reencodedQueryString;
                }
            }

            if (port != null) {
                port = ":" + port;
            }

            string uriString = scheme + "://" + serverName + port + path + queryString;
            return new Uri(uriString);
        }

        private static string EscapeForPath(string unescaped) {
            // DevDiv 762893: Applications might not call Uri.UnescapeDataString when looking
            // at components of the URI, and they'll be broken if certain path-safe characters
            // are now escaped.
            if (String.IsNullOrEmpty(unescaped) || ContainsOnlyPathSafeCharacters(unescaped))
                return unescaped;

            string escaped = Uri.EscapeDataString(unescaped);

            // If nothing was escaped, no need to decode
            if (String.Equals(escaped, unescaped, StringComparison.Ordinal))
                return unescaped;

            // We're going to perform multiple replace operations.
            // StringBuilder.Replace is much more memory-efficient than String.Replace
            StringBuilder builder = new StringBuilder(escaped);

            // Uri.EscapeDataString() is guaranteed to produce uppercase escape sequences.
            // Path-safe characters are listed in RFC 3986, Appendix A. We also add '/' to
            // this list since EscapeDataString may contain path segments.
            builder.Replace("%21", "!");
            builder.Replace("%24", "$");
            builder.Replace("%26", "&");
            builder.Replace("%27", "'");
            builder.Replace("%28", "(");
            builder.Replace("%29", ")");
            builder.Replace("%2A", "*");
            builder.Replace("%2B", "+");
            builder.Replace("%2C", ",");
            builder.Replace("%2F", "/");
            builder.Replace("%3A", ":");
            builder.Replace("%3B", ";");
            builder.Replace("%3D", "=");
            builder.Replace("%40", "@");
            return builder.ToString();
        }

        private static bool ContainsOnlyPathSafeCharacters(string input) {
            // See RFC 3986, Appendix A for the list of path-safe characters.
            for (int i = 0; i < input.Length; i++) {
                char c = input[i];

                // unreserved = ALPHA / DIGIT / ...
                if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || ('0' <= c && c <= '9')) {
                    continue;
                }

                switch (c) {
                    case '/': // path-abempty; path-absolute
                    case '-': case '.': case '_': case '~': // unreserved
                    case ':': case '@': // pchar
                    case '!': case '$': case '&': case '\'': case '(': case ')': // sub-delims
                    case '*': case '+': case ',': case ';': case '=': // sub-delims, cont.
                        continue;

                    default:
                        return false; // not path-safe
                }
            }

            // no bad characters found
            return true;
        }

        // Just extracts the query string and fragment from the input path by splitting on the separator characters.
        // Doesn't perform any validation as to whether the input represents a valid URL.
        // Concatenating the pieces back together will form the original input string.
        internal static void ExtractQueryAndFragment(string input, out string path, out string queryAndFragment) {
            int queryFragmentSeparatorPos = input.IndexOfAny(_queryFragmentSeparators);
            if (queryFragmentSeparatorPos != -1) {
                path = input.Substring(0, queryFragmentSeparatorPos);
                queryAndFragment = input.Substring(queryFragmentSeparatorPos);
            }
            else {
                // no query or fragment separator
                path = input;
                queryAndFragment = null;
            }
        }

        // Schemes that are generally considered safe for the purposes of redirects or other places where URLs are rendered to the page.
        internal static bool IsSafeScheme(String url) {
            return url.IndexOf(":", StringComparison.Ordinal) == -1 ||
                    url.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("ftp:", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("news:", StringComparison.OrdinalIgnoreCase);
        }

        // Attempts to split a URI into its constituent pieces.
        // Even if this method returns true, one or more of the out parameters might contain a null or empty string, e.g. if there is no query / fragment.
        // Concatenating the pieces back together will form the original input string.
        internal static bool TrySplitUriForPathEncode(string input, out string schemeAndAuthority, out string path, out string queryAndFragment, bool checkScheme) {
            // Strip off ?query and #fragment if they exist, since we're not going to look at them
            string inputWithoutQueryFragment;
            ExtractQueryAndFragment(input, out inputWithoutQueryFragment, out queryAndFragment);

            // DevDiv #450404: UrlPathEncode shouldn't care about the scheme of the incoming URL when it is
            // performing encoding; only Response.Redirect should.
            bool isValidScheme = (checkScheme) ? IsSafeScheme(inputWithoutQueryFragment) : true;

            // Use Uri class to parse the url into authority and path, use that to help decide
            // where to split the string. Do not rebuild the url from the Uri instance, as that
            // might have subtle changes from the original string (for example, see below about "://").
            Uri uri;
            if (isValidScheme && Uri.TryCreate(inputWithoutQueryFragment, UriKind.Absolute, out uri)) {
                string authority = uri.Authority; // e.g. "foo:81" in "http://foo:81/bar"
                if (!String.IsNullOrEmpty(authority)) {
                    // don't make any assumptions about the scheme or the "://" part.
                    // For example, the "//" could be missing, or there could be "///" as in "file:///C:\foo.txt"
                    // To retain the same string as originally given, find the authority in the original url and include
                    // everything up to that.
                    int authorityIndex = inputWithoutQueryFragment.IndexOf(authority, StringComparison.OrdinalIgnoreCase);
                    if (authorityIndex != -1) {
                        int schemeAndAuthorityLength = authorityIndex + authority.Length;
                        schemeAndAuthority = inputWithoutQueryFragment.Substring(0, schemeAndAuthorityLength);
                        path = inputWithoutQueryFragment.Substring(schemeAndAuthorityLength);
                        return true;
                    }
                }
            }

            // Not a safe URL
            schemeAndAuthority = null;
            path = null;
            queryAndFragment = null;
            return false;
        }

    }
}
