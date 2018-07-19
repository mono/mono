//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System
{
    using System.Collections.Specialized;
    using System.Runtime;
    using System.ServiceModel;
    using System.Text;

    // thin wrapper around string; use type system to help ensure we
    // are doing canonicalization right/consistently
    class UriTemplateLiteralPathSegment : UriTemplatePathSegment, IComparable<UriTemplateLiteralPathSegment>
    {

        // segment doesn't store trailing slash
        readonly string segment;
        static Uri dummyUri = new Uri("http://localhost");

        UriTemplateLiteralPathSegment(string segment)
            : base(segment, UriTemplatePartType.Literal, segment.EndsWith("/", StringComparison.Ordinal))
        {
            Fx.Assert(segment != null, "bad literal segment");
            if (this.EndsWithSlash)
            {
                this.segment = segment.Remove(segment.Length - 1);
            }
            else
            {
                this.segment = segment;
            }
        }
        public static new UriTemplateLiteralPathSegment CreateFromUriTemplate(string segment, UriTemplate template)
        {
            // run it through UriBuilder to escape-if-necessary it
            if (string.Compare(segment, "/", StringComparison.Ordinal) == 0)
            {
                // running an empty segment through UriBuilder has unexpected/wrong results
                return new UriTemplateLiteralPathSegment("/");
            }
            if (segment.IndexOf(UriTemplate.WildcardPath, StringComparison.Ordinal) != -1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(
                    SR.GetString(SR.UTInvalidWildcardInVariableOrLiteral, template.originalTemplate, UriTemplate.WildcardPath)));
            }
            // '*' is not usually escaped by the Uri\UriBuilder to %2a, since we forbid passing a
            // clear character and the workaroud is to pass the escaped form, we should replace the
            // escaped form with the regular one.
            segment = segment.Replace("%2a", "*").Replace("%2A", "*");
            UriBuilder ub = new UriBuilder(dummyUri);
            ub.Path = segment;
            string escapedIfNecessarySegment = ub.Uri.AbsolutePath.Substring(1);
            if (escapedIfNecessarySegment == string.Empty)
            {
                // This path through UriBuilder will sometimes '----' various segments
                // such as '../' and './'.  When this happens and the result is an empty
                // string, we should just throw and tell the user we don't handle that.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("segment",
                    SR.GetString(SR.UTInvalidFormatSegmentOrQueryPart, segment));
            }
            return new UriTemplateLiteralPathSegment(escapedIfNecessarySegment);
        }
        public static UriTemplateLiteralPathSegment CreateFromWireData(string segment)
        {
            return new UriTemplateLiteralPathSegment(segment);
        }

        public string AsUnescapedString()
        {
            Fx.Assert(this.segment != null, "this should only be called by Bind\\Lookup");
            return Uri.UnescapeDataString(this.segment);
        }
        public override void Bind(string[] values, ref int valueIndex, StringBuilder path)
        {
            if (this.EndsWithSlash)
            {
                path.AppendFormat("{0}/", AsUnescapedString());
            }
            else
            {
                path.Append(AsUnescapedString());
            }
        }

        public int CompareTo(UriTemplateLiteralPathSegment other)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(this.segment, other.segment);
        }

        public override bool Equals(object obj)
        {
            UriTemplateLiteralPathSegment lps = obj as UriTemplateLiteralPathSegment;
            if (lps == null)
            {
                Fx.Assert("why would we ever call this?");
                return false;
            }
            else
            {
                return ((this.EndsWithSlash == lps.EndsWithSlash) &&
                    StringComparer.OrdinalIgnoreCase.Equals(this.segment, lps.segment));
            }
        }
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(this.segment);
        }

        public override bool IsEquivalentTo(UriTemplatePathSegment other, bool ignoreTrailingSlash)
        {
            if (other == null)
            {
                Fx.Assert("why would we ever call this?");
                return false;
            }
            if (other.Nature != UriTemplatePartType.Literal)
            {
                return false;
            }
            UriTemplateLiteralPathSegment otherAsLiteral = other as UriTemplateLiteralPathSegment;
            Fx.Assert(otherAsLiteral != null, "The nature requires that this will be OK");
            return IsMatch(otherAsLiteral, ignoreTrailingSlash);
        }
        public override bool IsMatch(UriTemplateLiteralPathSegment segment, bool ignoreTrailingSlash)
        {
            if (!ignoreTrailingSlash && (segment.EndsWithSlash != this.EndsWithSlash))
            {
                return false;
            }
            return (CompareTo(segment) == 0);
        }
        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(this.segment);
        }
        public override void Lookup(string segment, NameValueCollection boundParameters)
        {
            Fx.Assert(StringComparer.OrdinalIgnoreCase.Compare(AsUnescapedString(), segment) == 0,
                "How can that be? Lookup is expected to be called after IsMatch");
        }
    }
}
