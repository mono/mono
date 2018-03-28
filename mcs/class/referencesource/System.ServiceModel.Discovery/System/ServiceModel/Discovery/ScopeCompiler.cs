//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Text;
    using SR2 = System.ServiceModel.Discovery.SR;

    static class ScopeCompiler
    {
        public static string[] Compile(ICollection<Uri> scopes)
        {
            if (scopes == null || scopes.Count == 0)
            {
                return null;
            }

            List<string> compiledScopes = new List<string>();
            foreach (Uri scope in scopes)
            {
                Compile(scope, compiledScopes);
            }

            return compiledScopes.ToArray();
        }

        public static CompiledScopeCriteria[] CompileMatchCriteria(ICollection<Uri> scopes, Uri matchBy)
        {
            Fx.Assert(matchBy != null, "The matchBy must be non null.");

            if (scopes == null || scopes.Count == 0)
            {
                return null;
            }

            List<CompiledScopeCriteria> compiledCriterias = new List<CompiledScopeCriteria>();
            foreach (Uri scope in scopes)
            {
                compiledCriterias.Add(CompileCriteria(scope, matchBy));
            }

            return compiledCriterias.ToArray();
        }

        public static bool IsSupportedMatchingRule(Uri matchBy)
        {
            Fx.Assert(matchBy != null, "The matchBy must be non null.");

            return (matchBy.Equals(FindCriteria.ScopeMatchByPrefix) ||
                matchBy.Equals(FindCriteria.ScopeMatchByUuid) ||
                matchBy.Equals(FindCriteria.ScopeMatchByLdap) ||
                matchBy.Equals(FindCriteria.ScopeMatchByExact) ||
                matchBy.Equals(FindCriteria.ScopeMatchByNone));
        }

        public static bool IsMatch(CompiledScopeCriteria compiledScopeMatchCriteria, string[] compiledScopes)
        {
            Fx.Assert(compiledScopeMatchCriteria != null, "The compiledScopeMatchCriteria must be non null.");
            Fx.Assert(compiledScopes != null, "The compiledScopes must be non null.");

            if (compiledScopeMatchCriteria.MatchBy == CompiledScopeCriteriaMatchBy.Exact)
            {
                for (int i = 0; i < compiledScopes.Length; i++)
                {
                    if (string.CompareOrdinal(compiledScopes[i], compiledScopeMatchCriteria.CompiledScope) == 0)
                    {
                        return true;
                    }
                }
            }
            else if (compiledScopeMatchCriteria.MatchBy == CompiledScopeCriteriaMatchBy.StartsWith)
            {
                for (int i = 0; i < compiledScopes.Length; i++)
                {
                    if (compiledScopes[i].StartsWith(compiledScopeMatchCriteria.CompiledScope,
                        StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static void Compile(Uri scope, List<string> compiledScopes)
        {
            // MatchByRfc2396 can be applied to any URI
            compiledScopes.Add(CompileForMatchByRfc2396(scope));

            // MatchByUuid can be applied to only UUIDs we treat urn:uuid:GUID same as uuid:GUID
            Guid guid;
            if (TryGetUuidGuid(scope, out guid))            
            {
                compiledScopes.Add(CompileForMatchByUuid(guid));
            }

            // MatchByStrcmp0 can be applied to any URI
            compiledScopes.Add(CompileForMatchByStrcmp0(scope));

            // MatchByLdap can be applied to only LDAP URI
            if (string.Compare(scope.Scheme, "ldap", StringComparison.OrdinalIgnoreCase) == 0)
            {
                compiledScopes.Add(CompileForMatchByLdap(scope));
            }
        }

        static CompiledScopeCriteria CompileCriteria(Uri scope, Uri matchBy)
        {
            string compiledScope;
            CompiledScopeCriteriaMatchBy compiledMatchBy;

            if (matchBy.Equals(FindCriteria.ScopeMatchByPrefix))
            {
                compiledScope = CompileForMatchByRfc2396(scope);
                compiledMatchBy = CompiledScopeCriteriaMatchBy.StartsWith;
            }
            else if (matchBy.Equals(FindCriteria.ScopeMatchByUuid))
            {
                Guid guid;
                if (!TryGetUuidGuid(scope, out guid))
                {
                    throw FxTrace.Exception.AsError(new FormatException(SR2.DiscoveryFormatInvalidScopeUuidUri(scope.ToString())));
                }
                compiledScope = CompileForMatchByUuid(guid);
                compiledMatchBy = CompiledScopeCriteriaMatchBy.Exact;
            }
            else if (matchBy.Equals(FindCriteria.ScopeMatchByLdap))
            {
                if (string.Compare(scope.Scheme, "ldap", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw FxTrace.Exception.AsError(new FormatException(SR2.DiscoveryFormatInvalidScopeLdapUri(scope.ToString())));
                }
                compiledScope = CompileForMatchByLdap(scope);
                compiledMatchBy = CompiledScopeCriteriaMatchBy.StartsWith;
            }
            else if (matchBy.Equals(FindCriteria.ScopeMatchByExact))
            {
                compiledScope = CompileForMatchByStrcmp0(scope);
                compiledMatchBy = CompiledScopeCriteriaMatchBy.Exact;
            }
            else
            {
                throw FxTrace.Exception.ArgumentOutOfRange("matchBy", matchBy,
                    SR2.DiscoveryMatchingRuleNotSupported(
                    FindCriteria.ScopeMatchByExact,
                    FindCriteria.ScopeMatchByPrefix,
                    FindCriteria.ScopeMatchByUuid,
                    FindCriteria.ScopeMatchByLdap));
            }

            return new CompiledScopeCriteria(compiledScope, compiledMatchBy);
        }

        static string CompileForMatchByRfc2396(Uri scope)
        {
            StringBuilder compiledScopeBuilder = new StringBuilder();

            // Append the matching rule name, so this compiled scope can only be 
            // matched for that particular matching rule.
            compiledScopeBuilder.Append("rfc2396match::");

            //
            // Rule: Using a case-insensitive comparison, The scheme [RFC 2396] 
            //       of S1 and S2 is the same and
            //
            string scheme = scope.GetComponents(UriComponents.Scheme, UriFormat.UriEscaped);
            if (scheme != null)
            {
                scheme = scheme.ToUpperInvariant();
            }
            else
            {
                scheme = string.Empty;
            }
            compiledScopeBuilder.Append(scheme);
            compiledScopeBuilder.Append(":");

            // 
            // Rule: Using a case-insensitive comparison, The authority of S1 
            //       and S2 is the same and
            // 
            string authority = scope.GetComponents(UriComponents.StrongAuthority, UriFormat.UriEscaped);
            if (authority != null)
            {
                authority = authority.ToUpperInvariant();
            }
            else
            {
                authority = string.Empty;
            }
            compiledScopeBuilder.Append(authority);
            compiledScopeBuilder.Append(":");

            // 
            // Rule: The path_segments of S1 is a segment-wise (not string) 
            //       prefix of the path_segments of S2 and Neither S1 nor S2 
            //       contain the "." segment or the ".." segment. All other 
            //       components (e.g., query and fragment) are explicitly 
            //       excluded from comparison. S1 and S2 MUST be canonicalized
            //       (e.g., unescaping escaped characters) before using this 
            //       matching rule.
            foreach (string segment in scope.Segments)
            {
                compiledScopeBuilder.Append(ProcessUriSegment(segment));
            }

            return compiledScopeBuilder.ToString();
        }

        static string ProcessUriSegment(string segment)
        {
            // ignore the segment parameters, if any
            int index = segment.IndexOf(';');
            if (index != -1)
            {
                segment = segment.Substring(0, index);
            }

            // prevent the comparision of partial segments
            // Note: this matching rule does NOT test whether the string 
            // representation of S1 is a prefix of the string representation 
            // of S2. For example, "http://example.com/abc" matches 
            // "http://example.com/abc/def" using this rule but 
            // "http://example.com/a" does not.
            if (!segment.EndsWith("/", StringComparison.Ordinal))
            {
                segment = segment + "/";
            }

            return segment;
        }

        static bool TryGetUuidGuid(Uri scope, out Guid guid)
        {
            string guidString = null;
            if (string.Compare(scope.Scheme, "uuid", StringComparison.OrdinalIgnoreCase) == 0)
            {
                guidString = scope.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
            }
            else if (string.Compare(scope.Scheme, "urn", StringComparison.OrdinalIgnoreCase) == 0)
            {
                string scopeString = scope.ToString();
                if (string.Compare(scopeString, 4, "uuid:", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    guidString = scopeString.Substring(9);
                }
            }

            return Fx.TryCreateGuid(guidString, out guid);            
        }

        static string CompileForMatchByUuid(Guid guid)
        {            
            // Append the matching rule name, so this compiled scope can only be 
            // matched for that particular matching rule.
            // 
            // Rule: Using a case-insensitive comparison, the scheme of S1 and 
            //      S2 is "uuid" and each of the unsigned integer fields [UUID]
            //      in S1 is equal to the corresponding field in S2, or 
            //      equivalently, the 128 bits of the in-memory representation 
            //      of S1 and S2 are the same 128 bit unsigned integer.
            return "uuidmatch::" + guid.ToString();
        }

        static string CompileForMatchByStrcmp0(Uri scope)
        {
            // 
            // Rule: Using a case-sensitive comparison, the string 
            //      representation of S1 and S2 is the same.
            //
            return "strcmp0match::" + scope.ToString();
        }

        static string CompileForMatchByLdap(Uri scope)
        {
            StringBuilder compiledScopeBuilder = new StringBuilder();

            // Append the matching rule name, so this compiled scope can only be 
            // matched for that particular matching rule.
            compiledScopeBuilder.Append("ldapmatch::");

            //
            // Rule: Using a case-insensitive comparison, the scheme of S1 
            // and S2 is "ldap" and
            compiledScopeBuilder.Append("ldap:");

            //
            // Rule: and the hostport [RFC 2255] of S1 and S2 is the 
            //       same and
            //
            string hostport = scope.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);
            if (hostport != null)
            {
                hostport = hostport.ToUpperInvariant();
            }
            else
            {
                hostport = string.Empty;
            }
            compiledScopeBuilder.Append(hostport);
            compiledScopeBuilder.Append(":");


            //
            // Rule: and the RDNSequence [RFC 2253] of the dn of S1 is a 
            //       prefix of the RDNSequence of the dn of S2, where comparison
            //       does not support the variants in an RDNSequence described 
            //       in Section 4 of RFC 2253 [RFC 2253].
            //
            // get the ldap DN string.
            string dn = scope.GetComponents(UriComponents.Path, UriFormat.Unescaped);

            // parse the RDNs in order from DN
            compiledScopeBuilder.Append(ParseLdapRDNSequence(dn));

            return compiledScopeBuilder.ToString();
        }

        static string ParseLdapRDNSequence(string dn)
        {
            // Assuming the conversion of DN to string as per Section 2 RFC2253
            // ignoring the variations described in section 4.

            StringBuilder rdnSequenceBuilder = new StringBuilder();
            string[] tokens = dn.Split(',');
            StringBuilder rdnBuilder = new StringBuilder();
            foreach (string token in tokens)
            {
                if (string.IsNullOrEmpty(token.Trim()))
                {
                    continue;
                }

                if (token.EndsWith("\\", StringComparison.Ordinal))
                {
                    // it is part of the RDN
                    rdnBuilder.Append(token.Substring(0, token.Length - 1));
                    rdnBuilder.Append(',');
                }
                else
                {
                    // RDN ends here.
                    rdnBuilder.Append(token);
                    rdnSequenceBuilder.Insert(0, "/");
                    rdnSequenceBuilder.Insert(0, ParseAndSortRDNAttributes(rdnBuilder.ToString()));
                    rdnBuilder = new StringBuilder();
                }
            }

            return rdnSequenceBuilder.ToString();
        }

        static string ParseAndSortRDNAttributes(string rdn)
        {
            //
            // Rule: RFC2253 Section 2: 
            //       When converting from an ASN.1 RelativeDistinguishedName 
            //       to a string, the output consists of the string encodings
            //       of each AttributeTypeAndValue (according to 2.3), in any
            //       order. Where there is a multi-valued RDN, the outputs 
            //       from adjoining AttributeTypeAndValues are separated by 
            //       a plus ('+' ASCII 43) character.
            // 

            // since the RDN attributes can be converted to string in any order
            // we must make sure that the compiled form of the scope and match
            // criteria have the same order so that simple string prefix 
            // comparision produces the same result of comparing the RDN 
            // attribute values individually, we sort the attributes or RDN
            // based on their name.

            // optimize the case where there is only one attrvalue for RDN
            if (rdn.IndexOf('+') == -1)
            {
                return rdn;
            }

            string[] tokens = rdn.Split('+');
            StringBuilder attrTypeAndValueBuilder = new StringBuilder();
            Dictionary<string, string> attrTypeValueTable = new Dictionary<string, string>();
            List<string> attrTypeList = new List<string>();

            foreach (string token in tokens)
            {
                if (string.IsNullOrEmpty(token.Trim()))
                {
                    continue;
                }

                if (token.EndsWith("\\", StringComparison.Ordinal))
                {
                    // it is part of the attribute value
                    attrTypeAndValueBuilder.Append(token.Substring(0, token.Length - 1));
                    attrTypeAndValueBuilder.Append('+');
                }
                else
                {
                    // attribute value ends here.
                    attrTypeAndValueBuilder.Append(token);

                    // get attribute and value.
                    string attrTypeAndValue = attrTypeAndValueBuilder.ToString();
                    string attrType = attrTypeAndValue;
                    string attrValue = null;

                    int equalIndex = attrTypeAndValue.IndexOf('=');
                    if (equalIndex != -1)
                    {
                        attrType = attrTypeAndValue.Substring(0, equalIndex);
                        attrValue = attrTypeAndValue.Substring(equalIndex + 1);
                    }

                    attrTypeList.Add(attrType);
                    attrTypeValueTable.Add(attrType, attrValue);
                    attrTypeAndValueBuilder = new StringBuilder();
                }
            }

            // sort the list based on the attribute type
            attrTypeList.Sort();

            // created the RDN from the sorted attribute values.
            StringBuilder rdnBuilder = new StringBuilder();
            for (int i = 0; i < attrTypeList.Count - 1; i++)
            {
                rdnBuilder.Append(attrTypeList[i]);
                if (attrTypeValueTable[attrTypeList[i]] != null)
                {
                    rdnBuilder.Append("=");
                    rdnBuilder.Append(attrTypeValueTable[attrTypeList[i]]);
                }
                rdnBuilder.Append("+");
            }

            if (attrTypeList.Count > 1)
            {
                rdnBuilder.Append(attrTypeList[attrTypeList.Count - 1]);
                if (attrTypeValueTable[attrTypeList[attrTypeList.Count - 1]] != null)
                {
                    rdnBuilder.Append("=");
                    rdnBuilder.Append(attrTypeValueTable[attrTypeList[attrTypeList.Count - 1]]);
                }
                rdnBuilder.Append("+");
            }

            return rdnBuilder.ToString();
        }
    }
}
