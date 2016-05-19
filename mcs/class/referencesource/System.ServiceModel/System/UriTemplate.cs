//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Runtime.CompilerServices;
    using System.Globalization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UriTemplate
    {
        internal readonly int firstOptionalSegment;

        internal readonly string originalTemplate;
        internal readonly Dictionary<string, UriTemplateQueryValue> queries; // keys are original case specified in UriTemplate constructor, dictionary ignores case
        internal readonly List<UriTemplatePathSegment> segments;
        internal const string WildcardPath = "*";
        readonly Dictionary<string, string> additionalDefaults; // keys are original case specified in UriTemplate constructor, dictionary ignores case
        readonly string fragment;

        readonly bool ignoreTrailingSlash;

        const string NullableDefault = "null";
        readonly WildcardInfo wildcard;
        IDictionary<string, string> defaults;
        Dictionary<string, string> unescapedDefaults;

        VariablesCollection variables;

        // constructors validates that template is well-formed
        public UriTemplate(string template)
            : this(template, false)
        {
        }
        public UriTemplate(string template, bool ignoreTrailingSlash)
            : this(template, ignoreTrailingSlash, null)
        {
        }
        public UriTemplate(string template, IDictionary<string, string> additionalDefaults)
            : this(template, false, additionalDefaults)
        {
        }
        public UriTemplate(string template, bool ignoreTrailingSlash, IDictionary<string, string> additionalDefaults)
        {
            if (template == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("template");
            }
            this.originalTemplate = template;
            this.ignoreTrailingSlash = ignoreTrailingSlash;
            this.segments = new List<UriTemplatePathSegment>();
            this.queries = new Dictionary<string, UriTemplateQueryValue>(StringComparer.OrdinalIgnoreCase);

            // parse it
            string pathTemplate;
            string queryTemplate;
            // ignore a leading slash
            if (template.StartsWith("/", StringComparison.Ordinal))
            {
                template = template.Substring(1);
            }
            // pull out fragment
            int fragmentStart = template.IndexOf('#');
            if (fragmentStart == -1)
            {
                this.fragment = "";
            }
            else
            {
                this.fragment = template.Substring(fragmentStart + 1);
                template = template.Substring(0, fragmentStart);
            }
            // pull out path and query
            int queryStart = template.IndexOf('?');
            if (queryStart == -1)
            {
                queryTemplate = string.Empty;
                pathTemplate = template;
            }
            else
            {
                queryTemplate = template.Substring(queryStart + 1);
                pathTemplate = template.Substring(0, queryStart);
            }
            template = null; // to ensure we don't accidentally reference this variable any more

            // setup path template and validate
            if (!string.IsNullOrEmpty(pathTemplate))
            {
                int startIndex = 0;
                while (startIndex < pathTemplate.Length)
                {
                    // Identify the next segment
                    int endIndex = pathTemplate.IndexOf('/', startIndex);
                    string segment;
                    if (endIndex != -1)
                    {
                        segment = pathTemplate.Substring(startIndex, endIndex + 1 - startIndex);
                        startIndex = endIndex + 1;
                    }
                    else
                    {
                        segment = pathTemplate.Substring(startIndex);
                        startIndex = pathTemplate.Length;
                    }
                    // Checking for wildcard segment ("*") or ("{*<var name>}")
                    UriTemplatePartType wildcardType;
                    if ((startIndex == pathTemplate.Length) &&
                        UriTemplateHelpers.IsWildcardSegment(segment, out wildcardType))
                    {
                        switch (wildcardType)
                        {
                            case UriTemplatePartType.Literal:
                                this.wildcard = new WildcardInfo(this);
                                break;

                            case UriTemplatePartType.Variable:
                                this.wildcard = new WildcardInfo(this, segment);
                                break;

                            default:
                                Fx.Assert("Error in identifying the type of the wildcard segment");
                                break;
                        }
                    }
                    else
                    {
                        this.segments.Add(UriTemplatePathSegment.CreateFromUriTemplate(segment, this));
                    }
                }
            }

            // setup query template and validate
            if (!string.IsNullOrEmpty(queryTemplate))
            {
                int startIndex = 0;
                while (startIndex < queryTemplate.Length)
                {
                    // Identify the next query part
                    int endIndex = queryTemplate.IndexOf('&', startIndex);
                    int queryPartStart = startIndex;
                    int queryPartEnd;
                    if (endIndex != -1)
                    {
                        queryPartEnd = endIndex;
                        startIndex = endIndex + 1;
                        if (startIndex >= queryTemplate.Length)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                                SR.UTQueryCannotEndInAmpersand, this.originalTemplate)));
                        }
                    }
                    else
                    {
                        queryPartEnd = queryTemplate.Length;
                        startIndex = queryTemplate.Length;
                    }
                    // Checking query part type; identifying key and value
                    int equalSignIndex = queryTemplate.IndexOf('=', queryPartStart, queryPartEnd - queryPartStart);
                    string key;
                    string value;
                    if (equalSignIndex >= 0)
                    {
                        key = queryTemplate.Substring(queryPartStart, equalSignIndex - queryPartStart);
                        value = queryTemplate.Substring(equalSignIndex + 1, queryPartEnd - equalSignIndex - 1);
                    }
                    else
                    {
                        key = queryTemplate.Substring(queryPartStart, queryPartEnd - queryPartStart);
                        value = null;
                    }
                    if (string.IsNullOrEmpty(key))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                            SR.UTQueryCannotHaveEmptyName, this.originalTemplate)));
                    }
                    if (UriTemplateHelpers.IdentifyPartType(key) != UriTemplatePartType.Literal)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("template", SR.GetString(
                            SR.UTQueryMustHaveLiteralNames, this.originalTemplate));
                    }
                    // Adding a new entry to the queries dictionary
                    key = UrlUtility.UrlDecode(key, Encoding.UTF8);
                    if (this.queries.ContainsKey(key))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                            SR.UTQueryNamesMustBeUnique, this.originalTemplate)));
                    }
                    this.queries.Add(key, UriTemplateQueryValue.CreateFromUriTemplate(value, this));
                }
            }

            // Process additional defaults (if has some) :
            if (additionalDefaults != null)
            {
                if (this.variables == null)
                {
                    if (additionalDefaults.Count > 0)
                    {
                        this.additionalDefaults = new Dictionary<string, string>(additionalDefaults, StringComparer.OrdinalIgnoreCase);
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, string> kvp in additionalDefaults)
                    {
                        string uppercaseKey = kvp.Key.ToUpperInvariant();
                        if ((this.variables.DefaultValues != null) && this.variables.DefaultValues.ContainsKey(uppercaseKey))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("additionalDefaults",
                                SR.GetString(SR.UTAdditionalDefaultIsInvalid, kvp.Key, this.originalTemplate));
                        }
                        if (this.variables.PathSegmentVariableNames.Contains(uppercaseKey))
                        {
                            this.variables.AddDefaultValue(uppercaseKey, kvp.Value);
                        }
                        else if (this.variables.QueryValueVariableNames.Contains(uppercaseKey))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                SR.GetString(SR.UTDefaultValueToQueryVarFromAdditionalDefaults, this.originalTemplate,
                                uppercaseKey)));
                        }
                        else if (string.Compare(kvp.Value, UriTemplate.NullableDefault, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                SR.GetString(SR.UTNullableDefaultAtAdditionalDefaults, this.originalTemplate,
                                uppercaseKey)));
                        }
                        else
                        {
                            if (this.additionalDefaults == null)
                            {
                                this.additionalDefaults = new Dictionary<string, string>(additionalDefaults.Count, StringComparer.OrdinalIgnoreCase);
                            }
                            this.additionalDefaults.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
            }

            // Validate defaults (if should)
            if ((this.variables != null) && (this.variables.DefaultValues != null))
            {
                this.variables.ValidateDefaults(out this.firstOptionalSegment);
            }
            else
            {
                this.firstOptionalSegment = this.segments.Count;
            }
        }

        public IDictionary<string, string> Defaults
        {
            get
            {
                if (this.defaults == null)
                {
                    this.defaults = new UriTemplateDefaults(this);
                }
                return this.defaults;
            }
        }
        public bool IgnoreTrailingSlash
        {
            get
            {
                return this.ignoreTrailingSlash;
            }
        }
        public ReadOnlyCollection<string> PathSegmentVariableNames
        {
            get
            {
                if (this.variables == null)
                {
                    return VariablesCollection.EmptyCollection;
                }
                else
                {
                    return this.variables.PathSegmentVariableNames;
                }
            }
        }
        public ReadOnlyCollection<string> QueryValueVariableNames
        {
            get
            {
                if (this.variables == null)
                {
                    return VariablesCollection.EmptyCollection;
                }
                else
                {
                    return this.variables.QueryValueVariableNames;
                }
            }
        }

        internal bool HasNoVariables
        {
            get
            {
                return (this.variables == null);
            }
        }
        internal bool HasWildcard
        {
            get
            {
                return (this.wildcard != null);
            }
        }

        // make a Uri by subbing in the values, throw on bad input
        public Uri BindByName(Uri baseAddress, IDictionary<string, string> parameters)
        {
            return BindByName(baseAddress, parameters, false);
        }
        public Uri BindByName(Uri baseAddress, IDictionary<string, string> parameters, bool omitDefaults)
        {
            if (baseAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseAddress");
            }
            if (!baseAddress.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("baseAddress", SR.GetString(
                    SR.UTBadBaseAddress));
            }

            BindInformation bindInfo;
            if (this.variables == null)
            {
                bindInfo = PrepareBindInformation(parameters, omitDefaults);
            }
            else
            {
                bindInfo = this.variables.PrepareBindInformation(parameters, omitDefaults);
            }
            return Bind(baseAddress, bindInfo, omitDefaults);
        }
        public Uri BindByName(Uri baseAddress, NameValueCollection parameters)
        {
            return BindByName(baseAddress, parameters, false);
        }
        public Uri BindByName(Uri baseAddress, NameValueCollection parameters, bool omitDefaults)
        {
            if (baseAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseAddress");
            }
            if (!baseAddress.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("baseAddress", SR.GetString(
                    SR.UTBadBaseAddress));
            }

            BindInformation bindInfo;
            if (this.variables == null)
            {
                bindInfo = PrepareBindInformation(parameters, omitDefaults);
            }
            else
            {
                bindInfo = this.variables.PrepareBindInformation(parameters, omitDefaults);
            }
            return Bind(baseAddress, bindInfo, omitDefaults);
        }
        public Uri BindByPosition(Uri baseAddress, params string[] values)
        {
            if (baseAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseAddress");
            }
            if (!baseAddress.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("baseAddress", SR.GetString(
                    SR.UTBadBaseAddress));
            }

            BindInformation bindInfo;
            if (this.variables == null)
            {
                if (values.Length > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(
                        SR.UTBindByPositionNoVariables, this.originalTemplate, values.Length)));
                }
                bindInfo = new BindInformation(this.additionalDefaults);
            }
            else
            {
                bindInfo = this.variables.PrepareBindInformation(values);
            }
            return Bind(baseAddress, bindInfo, false);
        }

        // A note about UriTemplate equivalency:
        //  The introduction of defaults and, more over, terminal defaults, broke the simple
        //  intuative notion of equivalency between templates. We will define equivalent
        //  templates as such based on the structure of them and not based on the set of uri
        //  that are matched by them. The result is that, even though they do not match the
        //  same set of uri's, the following templates are equivalent:
        //      - "/foo/{bar}"
        //      - "/foo/{bar=xyz}"
        //  A direct result from the support for 'terminal defaults' is that the IsPathEquivalentTo
        //  method, which was used both to determine the equivalence between templates, as 
        //  well as verify that all the templates, combined together in the same PathEquivalentSet, 
        //  are equivalent in thier path is no longer valid for both purposes. We will break 
        //  it to two distinct methods, each will be called in a different case.
        public bool IsEquivalentTo(UriTemplate other)
        {
            if (other == null)
            {
                return false;
            }
            if (other.segments == null || other.queries == null)
            {
                // they never are null, but PreSharp is complaining, 
                // and warning suppression isn't working
                return false;
            }
            if (!IsPathFullyEquivalent(other))
            {
                return false;
            }
            if (!IsQueryEquivalent(other))
            {
                return false;
            }
            Fx.Assert(UriTemplateEquivalenceComparer.Instance.GetHashCode(this) == UriTemplateEquivalenceComparer.Instance.GetHashCode(other), "bad GetHashCode impl");
            return true;
        }

        public UriTemplateMatch Match(Uri baseAddress, Uri candidate)
        {
            if (baseAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseAddress");
            }
            if (!baseAddress.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("baseAddress", SR.GetString(
                    SR.UTBadBaseAddress));
            }
            if (candidate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("candidate");
            }

            // ensure that the candidate is 'under' the base address
            if (!candidate.IsAbsoluteUri)
            {
                return null;
            }
            string basePath = UriTemplateHelpers.GetUriPath(baseAddress);
            string candidatePath = UriTemplateHelpers.GetUriPath(candidate);
            if (candidatePath.Length < basePath.Length)
            {
                return null;
            }
            if (!candidatePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Identifying the relative segments \ checking matching to the path :
            int numSegmentsInBaseAddress = baseAddress.Segments.Length;
            string[] candidateSegments = candidate.Segments;
            int numMatchedSegments;
            Collection<string> relativeCandidateSegments;
            if (!IsCandidatePathMatch(numSegmentsInBaseAddress, candidateSegments,
                out numMatchedSegments, out relativeCandidateSegments))
            {
                return null;
            }
            // Checking matching to the query (if should) :
            NameValueCollection candidateQuery = null;
            if (!UriTemplateHelpers.CanMatchQueryTrivially(this))
            {
                candidateQuery = UriTemplateHelpers.ParseQueryString(candidate.Query);
                if (!UriTemplateHelpers.CanMatchQueryInterestingly(this, candidateQuery, false))
                {
                    return null;
                }
            }

            // We matched; lets build the UriTemplateMatch
            return CreateUriTemplateMatch(baseAddress, candidate, null, numMatchedSegments,
                relativeCandidateSegments, candidateQuery);
        }

        public override string ToString()
        {
            return this.originalTemplate;
        }

        internal string AddPathVariable(UriTemplatePartType sourceNature, string varDeclaration)
        {
            bool hasDefaultValue;
            return AddPathVariable(sourceNature, varDeclaration, out hasDefaultValue);
        }
        internal string AddPathVariable(UriTemplatePartType sourceNature, string varDeclaration,
            out bool hasDefaultValue)
        {
            if (this.variables == null)
            {
                this.variables = new VariablesCollection(this);
            }
            return this.variables.AddPathVariable(sourceNature, varDeclaration, out hasDefaultValue);
        }
        internal string AddQueryVariable(string varDeclaration)
        {
            if (this.variables == null)
            {
                this.variables = new VariablesCollection(this);
            }
            return this.variables.AddQueryVariable(varDeclaration);
        }

        internal UriTemplateMatch CreateUriTemplateMatch(Uri baseUri, Uri uri, object data,
            int numMatchedSegments, Collection<string> relativePathSegments, NameValueCollection uriQuery)
        {
            UriTemplateMatch result = new UriTemplateMatch();
            result.RequestUri = uri;
            result.BaseUri = baseUri;
            if (uriQuery != null)
            {
                result.SetQueryParameters(uriQuery);
            }
            result.SetRelativePathSegments(relativePathSegments);
            result.Data = data;
            result.Template = this;
            for (int i = 0; i < numMatchedSegments; i++)
            {
                this.segments[i].Lookup(result.RelativePathSegments[i], result.BoundVariables);
            }
            if (this.wildcard != null)
            {
                this.wildcard.Lookup(numMatchedSegments, result.RelativePathSegments,
                    result.BoundVariables);
            }
            else if (numMatchedSegments < this.segments.Count)
            {
                BindTerminalDefaults(numMatchedSegments, result.BoundVariables);
            }
            if (this.queries.Count > 0)
            {
                foreach (KeyValuePair<string, UriTemplateQueryValue> kvp in this.queries)
                {
                    kvp.Value.Lookup(result.QueryParameters[kvp.Key], result.BoundVariables);
                    //UriTemplateHelpers.AssertCanonical(varName);
                }
            }
            if (this.additionalDefaults != null)
            {
                foreach (KeyValuePair<string, string> kvp in this.additionalDefaults)
                {
                    result.BoundVariables.Add(kvp.Key, UnescapeDefaultValue(kvp.Value));
                }
            }
            Fx.Assert(result.RelativePathSegments.Count - numMatchedSegments >= 0, "bad segment computation");
            result.SetWildcardPathSegmentsStart(numMatchedSegments);

            return result;
        }

        internal bool IsPathPartiallyEquivalentAt(UriTemplate other, int segmentsCount)
        {
            // Refer to the note on template equivalency at IsEquivalentTo
            // This method checks if any uri with given number of segments, which can be matched
            //  by this template, can be also matched by the other template.
            Fx.Assert(segmentsCount >= this.firstOptionalSegment - 1, "How can that be? The Trie is constructed that way!");
            Fx.Assert(segmentsCount <= this.segments.Count, "How can that be? The Trie is constructed that way!");
            Fx.Assert(segmentsCount >= other.firstOptionalSegment - 1, "How can that be? The Trie is constructed that way!");
            Fx.Assert(segmentsCount <= other.segments.Count, "How can that be? The Trie is constructed that way!");
            for (int i = 0; i < segmentsCount; ++i)
            {
                if (!this.segments[i].IsEquivalentTo(other.segments[i],
                    ((i == segmentsCount - 1) && (this.ignoreTrailingSlash || other.ignoreTrailingSlash))))
                {
                    return false;
                }
            }
            return true;
        }
        internal bool IsQueryEquivalent(UriTemplate other)
        {
            if (this.queries.Count != other.queries.Count)
            {
                return false;
            }
            foreach (string key in this.queries.Keys)
            {
                UriTemplateQueryValue utqv = this.queries[key];
                UriTemplateQueryValue otherUtqv;
                if (!other.queries.TryGetValue(key, out otherUtqv))
                {
                    return false;
                }
                if (!utqv.IsEquivalentTo(otherUtqv))
                {
                    return false;
                }
            }
            return true;
        }

        internal static Uri RewriteUri(Uri uri, string host)
        {
            if (!string.IsNullOrEmpty(host))
            {
                string originalHostHeader = uri.Host + ((!uri.IsDefaultPort) ? ":" + uri.Port.ToString(CultureInfo.InvariantCulture) : string.Empty);
                if (!String.Equals(originalHostHeader, host, StringComparison.OrdinalIgnoreCase))
                {
                    Uri sourceUri = new Uri(String.Format(CultureInfo.InvariantCulture, "{0}://{1}", uri.Scheme, host));
                    return (new UriBuilder(uri) { Host = sourceUri.Host, Port = sourceUri.Port }).Uri;
                }
            }
            return uri;
        }

        Uri Bind(Uri baseAddress, BindInformation bindInfo, bool omitDefaults)
        {
            UriBuilder result = new UriBuilder(baseAddress);
            int parameterIndex = 0;
            int lastPathParameter = ((this.variables == null) ? -1 : this.variables.PathSegmentVariableNames.Count - 1);
            int lastPathParameterToBind;
            if (lastPathParameter == -1)
            {
                lastPathParameterToBind = -1;
            }
            else if (omitDefaults)
            {
                lastPathParameterToBind = bindInfo.LastNonDefaultPathParameter;
            }
            else
            {
                lastPathParameterToBind = bindInfo.LastNonNullablePathParameter;
            }
            string[] parameters = bindInfo.NormalizedParameters;
            IDictionary<string, string> extraQueryParameters = bindInfo.AdditionalParameters;
            // Binding the path :
            StringBuilder pathString = new StringBuilder(result.Path);
            if (pathString[pathString.Length - 1] != '/')
            {
                pathString.Append('/');
            }
            if (lastPathParameterToBind < lastPathParameter)
            {
                // Binding all the parameters we need
                int segmentIndex = 0;
                while (parameterIndex <= lastPathParameterToBind)
                {
                    Fx.Assert(segmentIndex < this.segments.Count,
                        "Calculation of LastNonDefaultPathParameter,lastPathParameter or parameterIndex failed");
                    this.segments[segmentIndex++].Bind(parameters, ref parameterIndex, pathString);
                }
                Fx.Assert(parameterIndex == lastPathParameterToBind + 1,
                    "That is the exit criteria from the loop");
                // Maybe we have some literals yet to bind
                Fx.Assert(segmentIndex < this.segments.Count,
                    "Calculation of LastNonDefaultPathParameter,lastPathParameter or parameterIndex failed");
                while (this.segments[segmentIndex].Nature == UriTemplatePartType.Literal)
                {
                    this.segments[segmentIndex++].Bind(parameters, ref parameterIndex, pathString);
                    Fx.Assert(parameterIndex == lastPathParameterToBind + 1,
                        "We have moved the parameter index in a literal binding");
                    Fx.Assert(segmentIndex < this.segments.Count,
                        "Calculation of LastNonDefaultPathParameter,lastPathParameter or parameterIndex failed");
                }
                // We're done; skip to the beggining of the query parameters
                parameterIndex = lastPathParameter + 1;
            }
            else if (this.segments.Count > 0 || this.wildcard != null)
            {
                for (int i = 0; i < this.segments.Count; i++)
                {
                    this.segments[i].Bind(parameters, ref parameterIndex, pathString);
                }
                if (this.wildcard != null)
                {
                    this.wildcard.Bind(parameters, ref parameterIndex, pathString);
                }
            }
            if (this.ignoreTrailingSlash && (pathString[pathString.Length - 1] == '/'))
            {
                pathString.Remove(pathString.Length - 1, 1);
            }
            result.Path = pathString.ToString();
            // Binding the query :
            if ((this.queries.Count != 0) || (extraQueryParameters != null))
            {
                StringBuilder query = new StringBuilder("");
                foreach (string key in this.queries.Keys)
                {
                    this.queries[key].Bind(key, parameters, ref parameterIndex, query);
                }
                if (extraQueryParameters != null)
                {
                    foreach (string key in extraQueryParameters.Keys)
                    {
                        if (this.queries.ContainsKey(key.ToUpperInvariant()))
                        {
                            // This can only be if the key passed has the same name as some literal key
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("parameters", SR.GetString(
                                SR.UTBothLiteralAndNameValueCollectionKey, key));
                        }
                        string value = extraQueryParameters[key];
                        string escapedValue = (string.IsNullOrEmpty(value) ? string.Empty : UrlUtility.UrlEncode(value, Encoding.UTF8));
                        query.AppendFormat("&{0}={1}", UrlUtility.UrlEncode(key, Encoding.UTF8), escapedValue);
                    }
                }
                if (query.Length != 0)
                {
                    query.Remove(0, 1); // remove extra leading '&'
                }
                result.Query = query.ToString();
            }
            // Adding the fragment (if needed)
            if (this.fragment != null)
            {
                result.Fragment = this.fragment;
            }

            return result.Uri;
        }
        void BindTerminalDefaults(int numMatchedSegments, NameValueCollection boundParameters)
        {
            Fx.Assert(!this.HasWildcard, "There are no terminal default when ends with wildcard");
            Fx.Assert(numMatchedSegments < this.segments.Count, "Otherwise - no defaults to bind");
            Fx.Assert(this.variables != null, "Otherwise - no default values to bind");
            Fx.Assert(this.variables.DefaultValues != null, "Otherwise - no default values to bind");
            for (int i = numMatchedSegments; i < this.segments.Count; i++)
            {
                switch (this.segments[i].Nature)
                {
                    case UriTemplatePartType.Variable:
                        {
                            UriTemplateVariablePathSegment vps = this.segments[i] as UriTemplateVariablePathSegment;
                            Fx.Assert(vps != null, "How can that be? That its nature");
                            this.variables.LookupDefault(vps.VarName, boundParameters);
                        }
                        break;

                    default:
                        Fx.Assert("We only support terminal defaults on Variable segments");
                        break;
                }
            }
        }

        bool IsCandidatePathMatch(int numSegmentsInBaseAddress, string[] candidateSegments,
            out int numMatchedSegments, out Collection<string> relativeSegments)
        {
            int numRelativeSegments = candidateSegments.Length - numSegmentsInBaseAddress;
            Fx.Assert(numRelativeSegments >= 0, "bad segments num");
            relativeSegments = new Collection<string>();
            bool isStillMatch = true;
            int relativeSegmentsIndex = 0;
            while (isStillMatch && (relativeSegmentsIndex < numRelativeSegments))
            {
                string segment = candidateSegments[relativeSegmentsIndex + numSegmentsInBaseAddress];
                // Mathcing to next regular segment in the template (if there is one); building the wire segment representation
                if (relativeSegmentsIndex < this.segments.Count)
                {
                    bool ignoreSlash = (this.ignoreTrailingSlash && (relativeSegmentsIndex == numRelativeSegments - 1));
                    UriTemplateLiteralPathSegment lps = UriTemplateLiteralPathSegment.CreateFromWireData(segment);
                    if (!this.segments[relativeSegmentsIndex].IsMatch(lps, ignoreSlash))
                    {
                        isStillMatch = false;
                        break;
                    }
                    string relPathSeg = Uri.UnescapeDataString(segment);
                    if (lps.EndsWithSlash)
                    {
                        Fx.Assert(relPathSeg.EndsWith("/", StringComparison.Ordinal), "problem with relative path segment");
                        relPathSeg = relPathSeg.Substring(0, relPathSeg.Length - 1); // trim slash
                    }
                    relativeSegments.Add(relPathSeg);
                }
                // Checking if the template has a wild card ('*') or a final star var segment ("{*<var name>}"
                else if (this.HasWildcard)
                {
                    break;
                }
                else
                {
                    isStillMatch = false;
                    break;
                }
                relativeSegmentsIndex++;
            }
            if (isStillMatch)
            {
                numMatchedSegments = relativeSegmentsIndex;
                // building the wire representation to segments that were matched to a wild card
                if (relativeSegmentsIndex < numRelativeSegments)
                {
                    while (relativeSegmentsIndex < numRelativeSegments)
                    {
                        string relPathSeg = Uri.UnescapeDataString(candidateSegments[relativeSegmentsIndex + numSegmentsInBaseAddress]);
                        if (relPathSeg.EndsWith("/", StringComparison.Ordinal))
                        {
                            relPathSeg = relPathSeg.Substring(0, relPathSeg.Length - 1); // trim slash
                        }
                        relativeSegments.Add(relPathSeg);
                        relativeSegmentsIndex++;
                    }
                }
                // Checking if we matched all required segments already
                else if (numMatchedSegments < this.firstOptionalSegment)
                {
                    isStillMatch = false;
                }
            }
            else
            {
                numMatchedSegments = 0;
            }

            return isStillMatch;
        }

        bool IsPathFullyEquivalent(UriTemplate other)
        {
            // Refer to the note on template equivalency at IsEquivalentTo
            // This method checks if both templates has a fully equivalent path.
            if (this.HasWildcard != other.HasWildcard)
            {
                return false;
            }
            if (this.segments.Count != other.segments.Count)
            {
                return false;
            }
            for (int i = 0; i < this.segments.Count; ++i)
            {
                if (!this.segments[i].IsEquivalentTo(other.segments[i],
                    (i == this.segments.Count - 1) && !this.HasWildcard && (this.ignoreTrailingSlash || other.ignoreTrailingSlash)))
                {
                    return false;
                }
            }
            return true;
        }

        BindInformation PrepareBindInformation(IDictionary<string, string> parameters, bool omitDefaults)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }

            IDictionary<string, string> extraParameters = new Dictionary<string, string>(UriTemplateHelpers.GetQueryKeyComparer());
            foreach (KeyValuePair<string, string> kvp in parameters)
            {
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("parameters",
                        SR.GetString(SR.UTBindByNameCalledWithEmptyKey));
                }

                extraParameters.Add(kvp);
            }
            BindInformation bindInfo;
            ProcessDefaultsAndCreateBindInfo(omitDefaults, extraParameters, out bindInfo);
            return bindInfo;
        }
        BindInformation PrepareBindInformation(NameValueCollection parameters, bool omitDefaults)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }

            IDictionary<string, string> extraParameters = new Dictionary<string, string>(UriTemplateHelpers.GetQueryKeyComparer());
            foreach (string key in parameters.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("parameters",
                        SR.GetString(SR.UTBindByNameCalledWithEmptyKey));
                }

                extraParameters.Add(key, parameters[key]);
            }
            BindInformation bindInfo;
            ProcessDefaultsAndCreateBindInfo(omitDefaults, extraParameters, out bindInfo);
            return bindInfo;
        }
        void ProcessDefaultsAndCreateBindInfo(bool omitDefaults, IDictionary<string, string> extraParameters,
            out BindInformation bindInfo)
        {
            Fx.Assert(extraParameters != null, "We are expected to create it at the calling PrepareBindInformation");
            if (this.additionalDefaults != null)
            {
                if (omitDefaults)
                {
                    foreach (KeyValuePair<string, string> kvp in this.additionalDefaults)
                    {
                        string extraParameter;
                        if (extraParameters.TryGetValue(kvp.Key, out extraParameter))
                        {
                            if (string.Compare(extraParameter, kvp.Value, StringComparison.Ordinal) == 0)
                            {
                                extraParameters.Remove(kvp.Key);
                            }
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, string> kvp in this.additionalDefaults)
                    {
                        if (!extraParameters.ContainsKey(kvp.Key))
                        {
                            extraParameters.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
            }
            if (extraParameters.Count == 0)
            {
                extraParameters = null;
            }
            bindInfo = new BindInformation(extraParameters);
        }

        string UnescapeDefaultValue(string escapedValue)
        {
            if (string.IsNullOrEmpty(escapedValue))
            {
                return escapedValue;
            }
            if (this.unescapedDefaults == null)
            {
                this.unescapedDefaults = new Dictionary<string, string>(StringComparer.Ordinal);
            }
            string unescapedValue;
            if (!this.unescapedDefaults.TryGetValue(escapedValue, out unescapedValue))
            {
                unescapedValue = Uri.UnescapeDataString(escapedValue);
                this.unescapedDefaults.Add(escapedValue, unescapedValue);
            }

            return unescapedValue;
        }

        struct BindInformation
        {
            IDictionary<string, string> additionalParameters;
            int lastNonDefaultPathParameter;
            int lastNonNullablePathParameter;
            string[] normalizedParameters;

            public BindInformation(string[] normalizedParameters, int lastNonDefaultPathParameter,
                int lastNonNullablePathParameter, IDictionary<string, string> additionalParameters)
            {
                this.normalizedParameters = normalizedParameters;
                this.lastNonDefaultPathParameter = lastNonDefaultPathParameter;
                this.lastNonNullablePathParameter = lastNonNullablePathParameter;
                this.additionalParameters = additionalParameters;
            }
            public BindInformation(IDictionary<string, string> additionalParameters)
            {
                this.normalizedParameters = null;
                this.lastNonDefaultPathParameter = -1;
                this.lastNonNullablePathParameter = -1;
                this.additionalParameters = additionalParameters;
            }

            public IDictionary<string, string> AdditionalParameters
            {
                get
                {
                    return this.additionalParameters;
                }
            }
            public int LastNonDefaultPathParameter
            {
                get
                {
                    return this.lastNonDefaultPathParameter;
                }
            }
            public int LastNonNullablePathParameter
            {
                get
                {
                    return this.lastNonNullablePathParameter;
                }
            }
            public string[] NormalizedParameters
            {
                get
                {
                    return this.normalizedParameters;
                }
            }
        }

        class UriTemplateDefaults : IDictionary<string, string>
        {
            Dictionary<string, string> defaults;
            ReadOnlyCollection<string> keys;
            ReadOnlyCollection<string> values;

            public UriTemplateDefaults(UriTemplate template)
            {
                this.defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if ((template.variables != null) && (template.variables.DefaultValues != null))
                {
                    foreach (KeyValuePair<string, string> kvp in template.variables.DefaultValues)
                    {
                        this.defaults.Add(kvp.Key, kvp.Value);
                    }
                }
                if (template.additionalDefaults != null)
                {
                    foreach (KeyValuePair<string, string> kvp in template.additionalDefaults)
                    {
                        this.defaults.Add(kvp.Key.ToUpperInvariant(), kvp.Value);
                    }
                }
                this.keys = new ReadOnlyCollection<string>(new List<string>(this.defaults.Keys));
                this.values = new ReadOnlyCollection<string>(new List<string>(this.defaults.Values));
            }

            // ICollection<KeyValuePair<string, string>> Members
            public int Count
            {
                get
                {
                    return this.defaults.Count;
                }
            }
            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            // IDictionary<string, string> Members
            public ICollection<string> Keys
            {
                get
                {
                    return this.keys;
                }
            }
            public ICollection<string> Values
            {
                get
                {
                    return this.values;
                }
            }
            public string this[string key]
            {
                get
                {
                    return this.defaults[key];
                }
                set
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                        SR.GetString(SR.UTDefaultValuesAreImmutable)));
                }
            }

            public void Add(string key, string value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                    SR.GetString(SR.UTDefaultValuesAreImmutable)));
            }

            public void Add(KeyValuePair<string, string> item)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                    SR.GetString(SR.UTDefaultValuesAreImmutable)));
            }
            public void Clear()
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                    SR.GetString(SR.UTDefaultValuesAreImmutable)));
            }
            public bool Contains(KeyValuePair<string, string> item)
            {
                return (this.defaults as ICollection<KeyValuePair<string, string>>).Contains(item);
            }
            public bool ContainsKey(string key)
            {
                return this.defaults.ContainsKey(key);
            }
            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                (this.defaults as ICollection<KeyValuePair<string, string>>).CopyTo(array, arrayIndex);
            }

            // IEnumerable<KeyValuePair<string, string>> Members
            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return this.defaults.GetEnumerator();
            }
            public bool Remove(string key)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                    SR.GetString(SR.UTDefaultValuesAreImmutable)));
            }
            public bool Remove(KeyValuePair<string, string> item)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                    SR.GetString(SR.UTDefaultValuesAreImmutable)));
            }

            // IEnumerable Members
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.defaults.GetEnumerator();
            }
            public bool TryGetValue(string key, out string value)
            {
                return this.defaults.TryGetValue(key, out value);
            }
        }

        class VariablesCollection
        {
            readonly UriTemplate owner;
            static ReadOnlyCollection<string> emptyStringCollection = null;
            Dictionary<string, string> defaultValues; // key is the variable name (in uppercase; as appear in the variable names lists)
            int firstNullablePathVariable;
            List<string> pathSegmentVariableNames; // ToUpperInvariant, in order they occur in the original template string
            ReadOnlyCollection<string> pathSegmentVariableNamesSnapshot = null;
            List<UriTemplatePartType> pathSegmentVariableNature;
            List<string> queryValueVariableNames; // ToUpperInvariant, in order they occur in the original template string
            ReadOnlyCollection<string> queryValueVariableNamesSnapshot = null;

            public VariablesCollection(UriTemplate owner)
            {
                this.owner = owner;
                this.pathSegmentVariableNames = new List<string>();
                this.pathSegmentVariableNature = new List<UriTemplatePartType>();
                this.queryValueVariableNames = new List<string>();
                this.firstNullablePathVariable = -1;
            }

            public static ReadOnlyCollection<string> EmptyCollection
            {
                get
                {
                    if (emptyStringCollection == null)
                    {
                        emptyStringCollection = new ReadOnlyCollection<string>(new List<string>());
                    }
                    return emptyStringCollection;
                }
            }

            public Dictionary<string, string> DefaultValues
            {
                get
                {
                    return this.defaultValues;
                }
            }
            public ReadOnlyCollection<string> PathSegmentVariableNames
            {
                get
                {
                    if (this.pathSegmentVariableNamesSnapshot == null)
                    {
                        this.pathSegmentVariableNamesSnapshot = new ReadOnlyCollection<string>(
                            this.pathSegmentVariableNames);
                    }
                    return this.pathSegmentVariableNamesSnapshot;
                }
            }
            public ReadOnlyCollection<string> QueryValueVariableNames
            {
                get
                {
                    if (this.queryValueVariableNamesSnapshot == null)
                    {
                        this.queryValueVariableNamesSnapshot = new ReadOnlyCollection<string>(
                            this.queryValueVariableNames);
                    }
                    return this.queryValueVariableNamesSnapshot;
                }
            }

            public void AddDefaultValue(string varName, string value)
            {
                int varIndex = this.pathSegmentVariableNames.IndexOf(varName);
                Fx.Assert(varIndex != -1, "Adding default value is restricted to path variables");
                if ((this.owner.wildcard != null) && this.owner.wildcard.HasVariable &&
                    (varIndex == this.pathSegmentVariableNames.Count - 1))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.UTStarVariableWithDefaultsFromAdditionalDefaults,
                        this.owner.originalTemplate, varName)));
                }
                if (this.pathSegmentVariableNature[varIndex] != UriTemplatePartType.Variable)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.UTDefaultValueToCompoundSegmentVarFromAdditionalDefaults,
                        this.owner.originalTemplate, varName)));
                }
                if (string.IsNullOrEmpty(value) ||
                    (string.Compare(value, UriTemplate.NullableDefault, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    value = null;
                }
                if (this.defaultValues == null)
                {
                    this.defaultValues = new Dictionary<string, string>();
                }
                this.defaultValues.Add(varName, value);
            }

            public string AddPathVariable(UriTemplatePartType sourceNature, string varDeclaration, out bool hasDefaultValue)
            {
                Fx.Assert(sourceNature != UriTemplatePartType.Literal, "Literal path segments can't be the source for path variables");
                string varName;
                string defaultValue;
                ParseVariableDeclaration(varDeclaration, out varName, out defaultValue);
                hasDefaultValue = (defaultValue != null);
                if (varName.IndexOf(UriTemplate.WildcardPath, StringComparison.Ordinal) != -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(
                        SR.GetString(SR.UTInvalidWildcardInVariableOrLiteral, this.owner.originalTemplate, UriTemplate.WildcardPath)));
                }
                string uppercaseVarName = varName.ToUpperInvariant();
                if (this.pathSegmentVariableNames.Contains(uppercaseVarName) ||
                    this.queryValueVariableNames.Contains(uppercaseVarName))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.UTVarNamesMustBeUnique, this.owner.originalTemplate, varName)));
                }
                this.pathSegmentVariableNames.Add(uppercaseVarName);
                this.pathSegmentVariableNature.Add(sourceNature);
                if (hasDefaultValue)
                {
                    if (defaultValue == string.Empty)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.UTInvalidDefaultPathValue, this.owner.originalTemplate,
                            varDeclaration, varName)));
                    }
                    if (string.Compare(defaultValue, UriTemplate.NullableDefault, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        defaultValue = null;
                    }
                    if (this.defaultValues == null)
                    {
                        this.defaultValues = new Dictionary<string, string>();
                    }
                    this.defaultValues.Add(uppercaseVarName, defaultValue);
                }
                return uppercaseVarName;
            }
            public string AddQueryVariable(string varDeclaration)
            {
                string varName;
                string defaultValue;
                ParseVariableDeclaration(varDeclaration, out varName, out defaultValue);
                if (varName.IndexOf(UriTemplate.WildcardPath, StringComparison.Ordinal) != -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(
                        SR.GetString(SR.UTInvalidWildcardInVariableOrLiteral, this.owner.originalTemplate, UriTemplate.WildcardPath)));
                }
                if (defaultValue != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.UTDefaultValueToQueryVar, this.owner.originalTemplate,
                        varDeclaration, varName)));
                }
                string uppercaseVarName = varName.ToUpperInvariant();
                if (this.pathSegmentVariableNames.Contains(uppercaseVarName) ||
                    this.queryValueVariableNames.Contains(uppercaseVarName))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.UTVarNamesMustBeUnique, this.owner.originalTemplate, varName)));
                }
                this.queryValueVariableNames.Add(uppercaseVarName);
                return uppercaseVarName;
            }

            public void LookupDefault(string varName, NameValueCollection boundParameters)
            {
                Fx.Assert(this.defaultValues.ContainsKey(varName), "Otherwise, we don't have a value to bind");
                boundParameters.Add(varName, owner.UnescapeDefaultValue(this.defaultValues[varName]));
            }

            public BindInformation PrepareBindInformation(IDictionary<string, string> parameters, bool omitDefaults)
            {
                if (parameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
                }

                string[] normalizedParameters = PrepareNormalizedParameters();
                IDictionary<string, string> extraParameters = null;
                foreach (string key in parameters.Keys)
                {
                    ProcessBindParameter(key, parameters[key], normalizedParameters, ref extraParameters);
                }
                BindInformation bindInfo;
                ProcessDefaultsAndCreateBindInfo(omitDefaults, normalizedParameters, extraParameters, out bindInfo);
                return bindInfo;
            }
            public BindInformation PrepareBindInformation(NameValueCollection parameters, bool omitDefaults)
            {
                if (parameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
                }

                string[] normalizedParameters = PrepareNormalizedParameters();
                IDictionary<string, string> extraParameters = null;
                foreach (string key in parameters.AllKeys)
                {
                    ProcessBindParameter(key, parameters[key], normalizedParameters, ref extraParameters);
                }
                BindInformation bindInfo;
                ProcessDefaultsAndCreateBindInfo(omitDefaults, normalizedParameters, extraParameters, out bindInfo);
                return bindInfo;
            }
            public BindInformation PrepareBindInformation(params string[] parameters)
            {
                if (parameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("values");
                }
                if ((parameters.Length < this.pathSegmentVariableNames.Count) ||
                    (parameters.Length > this.pathSegmentVariableNames.Count + this.queryValueVariableNames.Count))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(
                        SR.GetString(SR.UTBindByPositionWrongCount, this.owner.originalTemplate,
                        this.pathSegmentVariableNames.Count, this.queryValueVariableNames.Count,
                        parameters.Length)));
                }

                string[] normalizedParameters;
                if (parameters.Length == this.pathSegmentVariableNames.Count + this.queryValueVariableNames.Count)
                {
                    normalizedParameters = parameters;
                }
                else
                {
                    normalizedParameters = new string[this.pathSegmentVariableNames.Count + this.queryValueVariableNames.Count];
                    parameters.CopyTo(normalizedParameters, 0);
                    for (int i = parameters.Length; i < normalizedParameters.Length; i++)
                    {
                        normalizedParameters[i] = null;
                    }
                }
                int lastNonDefaultPathParameter;
                int lastNonNullablePathParameter;
                LoadDefaultsAndValidate(normalizedParameters, out lastNonDefaultPathParameter,
                    out lastNonNullablePathParameter);
                return new BindInformation(normalizedParameters, lastNonDefaultPathParameter,
                    lastNonNullablePathParameter, this.owner.additionalDefaults);
            }
            public void ValidateDefaults(out int firstOptionalSegment)
            {
                Fx.Assert(this.defaultValues != null, "We are checking this condition from the c'tor");
                Fx.Assert(this.pathSegmentVariableNames.Count > 0, "Otherwise, how can we have default values");
                // Finding the first valid nullable defaults
                for (int i = this.pathSegmentVariableNames.Count - 1; (i >= 0) && (this.firstNullablePathVariable == -1); i--)
                {
                    string varName = this.pathSegmentVariableNames[i];
                    string defaultValue;
                    if (!this.defaultValues.TryGetValue(varName, out defaultValue))
                    {
                        this.firstNullablePathVariable = i + 1;
                    }
                    else if (defaultValue != null)
                    {
                        this.firstNullablePathVariable = i + 1;
                    }
                }
                if (this.firstNullablePathVariable == -1)
                {
                    this.firstNullablePathVariable = 0;
                }
                // Making sure that there are no nullables to the left of the first valid nullable
                if (this.firstNullablePathVariable > 1)
                {
                    for (int i = this.firstNullablePathVariable - 2; i >= 0; i--)
                    {
                        string varName = this.pathSegmentVariableNames[i];
                        string defaultValue;
                        if (this.defaultValues.TryGetValue(varName, out defaultValue))
                        {
                            if (defaultValue == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                    SR.GetString(SR.UTNullableDefaultMustBeFollowedWithNullables, this.owner.originalTemplate,
                                    varName, this.pathSegmentVariableNames[i + 1])));
                            }
                        }
                    }
                }
                // Making sure that there are no Literals\WildCards to the right
                // Based on the fact that only Variable Path Segments support default values,
                //  if firstNullablePathVariable=N and pathSegmentVariableNames.Count=M then
                //  the nature of the last M-N path segments should be StringNature.Variable; otherwise,
                //  there was a literal segment in between. Also, there shouldn't be a wildcard.
                if (this.firstNullablePathVariable < this.pathSegmentVariableNames.Count)
                {
                    if (this.owner.HasWildcard)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.UTNullableDefaultMustNotBeFollowedWithWildcard,
                            this.owner.originalTemplate, this.pathSegmentVariableNames[this.firstNullablePathVariable])));
                    }
                    for (int i = this.pathSegmentVariableNames.Count - 1; i >= this.firstNullablePathVariable; i--)
                    {
                        int segmentIndex = this.owner.segments.Count - (this.pathSegmentVariableNames.Count - i);
                        if (this.owner.segments[segmentIndex].Nature != UriTemplatePartType.Variable)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                SR.GetString(SR.UTNullableDefaultMustNotBeFollowedWithLiteral,
                                this.owner.originalTemplate, this.pathSegmentVariableNames[this.firstNullablePathVariable],
                                this.owner.segments[segmentIndex].OriginalSegment)));
                        }
                    }
                }
                // Now that we have the firstNullablePathVariable set, lets calculate the firstOptionalSegment.
                //  We already knows that the last M-N path segments (when M=pathSegmentVariableNames.Count and
                //  N=firstNullablePathVariable) are optional (see the previos comment). We will start there and
                //  move to the left, stopping at the first segment, which is not a variable or is a variable
                //  and doesn't have a default value.
                int numNullablePathVariables = (this.pathSegmentVariableNames.Count - this.firstNullablePathVariable);
                firstOptionalSegment = this.owner.segments.Count - numNullablePathVariables;
                if (!this.owner.HasWildcard)
                {
                    while (firstOptionalSegment > 0)
                    {
                        UriTemplatePathSegment ps = this.owner.segments[firstOptionalSegment - 1];
                        if (ps.Nature != UriTemplatePartType.Variable)
                        {
                            break;
                        }
                        UriTemplateVariablePathSegment vps = (ps as UriTemplateVariablePathSegment);
                        Fx.Assert(vps != null, "Should be; that's his nature");
                        if (!this.defaultValues.ContainsKey(vps.VarName))
                        {
                            break;
                        }
                        firstOptionalSegment--;
                    }
                }
            }

            void AddAdditionalDefaults(ref IDictionary<string, string> extraParameters)
            {
                if (extraParameters == null)
                {
                    extraParameters = this.owner.additionalDefaults;
                }
                else
                {
                    foreach (KeyValuePair<string, string> kvp in this.owner.additionalDefaults)
                    {
                        if (!extraParameters.ContainsKey(kvp.Key))
                        {
                            extraParameters.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
            }
            void LoadDefaultsAndValidate(string[] normalizedParameters, out int lastNonDefaultPathParameter,
                out int lastNonNullablePathParameter)
            {
                // First step - loading defaults
                for (int i = 0; i < this.pathSegmentVariableNames.Count; i++)
                {
                    if (string.IsNullOrEmpty(normalizedParameters[i]) && (this.defaultValues != null))
                    {
                        this.defaultValues.TryGetValue(this.pathSegmentVariableNames[i], out normalizedParameters[i]);
                    }
                }
                // Second step - calculating bind constrains
                lastNonDefaultPathParameter = this.pathSegmentVariableNames.Count - 1;
                if ((this.defaultValues != null) &&
                    (this.owner.segments[this.owner.segments.Count - 1].Nature != UriTemplatePartType.Literal))
                {
                    bool foundNonDefaultPathParameter = false;
                    while (!foundNonDefaultPathParameter && (lastNonDefaultPathParameter >= 0))
                    {
                        string defaultValue;
                        if (this.defaultValues.TryGetValue(this.pathSegmentVariableNames[lastNonDefaultPathParameter],
                            out defaultValue))
                        {
                            if (string.Compare(normalizedParameters[lastNonDefaultPathParameter],
                                defaultValue, StringComparison.Ordinal) != 0)
                            {
                                foundNonDefaultPathParameter = true;
                            }
                            else
                            {
                                lastNonDefaultPathParameter--;
                            }
                        }
                        else
                        {
                            foundNonDefaultPathParameter = true;
                        }
                    }
                }
                if (this.firstNullablePathVariable > lastNonDefaultPathParameter)
                {
                    lastNonNullablePathParameter = this.firstNullablePathVariable - 1;
                }
                else
                {
                    lastNonNullablePathParameter = lastNonDefaultPathParameter;
                }
                // Third step - validate
                for (int i = 0; i <= lastNonNullablePathParameter; i++)
                {
                    // Skip validation for terminating star variable segment :
                    if (this.owner.HasWildcard && this.owner.wildcard.HasVariable &&
                        (i == this.pathSegmentVariableNames.Count - 1))
                    {
                        continue;
                    }
                    // Validate
                    if (string.IsNullOrEmpty(normalizedParameters[i]))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("parameters",
                            SR.GetString(SR.BindUriTemplateToNullOrEmptyPathParam, this.pathSegmentVariableNames[i]));
                    }
                }
            }
            void ParseVariableDeclaration(string varDeclaration, out string varName, out string defaultValue)
            {
                if ((varDeclaration.IndexOf('{') != -1) || (varDeclaration.IndexOf('}') != -1))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(
                        SR.GetString(SR.UTInvalidVarDeclaration, this.owner.originalTemplate, varDeclaration)));
                }
                int equalSignIndex = varDeclaration.IndexOf('=');
                switch (equalSignIndex)
                {
                    case -1:
                        varName = varDeclaration;
                        defaultValue = null;
                        break;

                    case 0:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(
                            SR.GetString(SR.UTInvalidVarDeclaration, this.owner.originalTemplate, varDeclaration)));

                    default:
                        varName = varDeclaration.Substring(0, equalSignIndex);
                        defaultValue = varDeclaration.Substring(equalSignIndex + 1);
                        if (defaultValue.IndexOf('=') != -1)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(
                                SR.GetString(SR.UTInvalidVarDeclaration, this.owner.originalTemplate, varDeclaration)));
                        }
                        break;
                }
            }
            string[] PrepareNormalizedParameters()
            {
                string[] normalizedParameters = new string[this.pathSegmentVariableNames.Count + this.queryValueVariableNames.Count];
                for (int i = 0; i < normalizedParameters.Length; i++)
                {
                    normalizedParameters[i] = null;
                }
                return normalizedParameters;
            }
            void ProcessBindParameter(string name, string value, string[] normalizedParameters,
                ref IDictionary<string, string> extraParameters)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("parameters",
                        SR.GetString(SR.UTBindByNameCalledWithEmptyKey));
                }

                string uppercaseVarName = name.ToUpperInvariant();
                int pathVarIndex = this.pathSegmentVariableNames.IndexOf(uppercaseVarName);
                if (pathVarIndex != -1)
                {
                    normalizedParameters[pathVarIndex] = (string.IsNullOrEmpty(value) ? string.Empty : value);
                    return;
                }
                int queryVarIndex = this.queryValueVariableNames.IndexOf(uppercaseVarName);
                if (queryVarIndex != -1)
                {
                    normalizedParameters[this.pathSegmentVariableNames.Count + queryVarIndex] = (string.IsNullOrEmpty(value) ? string.Empty : value);
                    return;
                }
                if (extraParameters == null)
                {
                    extraParameters = new Dictionary<string, string>(UriTemplateHelpers.GetQueryKeyComparer());
                }
                extraParameters.Add(name, value);
            }
            void ProcessDefaultsAndCreateBindInfo(bool omitDefaults, string[] normalizedParameters,
                IDictionary<string, string> extraParameters, out BindInformation bindInfo)
            {
                int lastNonDefaultPathParameter;
                int lastNonNullablePathParameter;
                LoadDefaultsAndValidate(normalizedParameters, out lastNonDefaultPathParameter,
                    out lastNonNullablePathParameter);
                if (this.owner.additionalDefaults != null)
                {
                    if (omitDefaults)
                    {
                        RemoveAdditionalDefaults(ref extraParameters);
                    }
                    else
                    {
                        AddAdditionalDefaults(ref extraParameters);
                    }
                }
                bindInfo = new BindInformation(normalizedParameters, lastNonDefaultPathParameter,
                    lastNonNullablePathParameter, extraParameters);
            }
            void RemoveAdditionalDefaults(ref IDictionary<string, string> extraParameters)
            {
                if (extraParameters == null)
                {
                    return;
                }

                foreach (KeyValuePair<string, string> kvp in this.owner.additionalDefaults)
                {
                    string extraParameter;
                    if (extraParameters.TryGetValue(kvp.Key, out extraParameter))
                    {
                        if (string.Compare(extraParameter, kvp.Value, StringComparison.Ordinal) == 0)
                        {
                            extraParameters.Remove(kvp.Key);
                        }
                    }
                }
                if (extraParameters.Count == 0)
                {
                    extraParameters = null;
                }
            }
        }

        class WildcardInfo
        {
            readonly UriTemplate owner;
            readonly string varName;

            public WildcardInfo(UriTemplate owner)
            {
                this.varName = null;
                this.owner = owner;
            }
            public WildcardInfo(UriTemplate owner, string segment)
            {
                Fx.Assert(!segment.EndsWith("/", StringComparison.Ordinal), "We are expecting to check this earlier");

                bool hasDefault;
                this.varName = owner.AddPathVariable(UriTemplatePartType.Variable,
                    segment.Substring(1 + WildcardPath.Length, segment.Length - 2 - WildcardPath.Length),
                    out hasDefault);
                // Since this is a terminating star segment there shouldn't be a default
                if (hasDefault)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.UTStarVariableWithDefaults, owner.originalTemplate,
                        segment, this.varName)));
                }
                this.owner = owner;
            }

            internal bool HasVariable
            {
                get
                {
                    return (!string.IsNullOrEmpty(this.varName));
                }
            }

            public void Bind(string[] values, ref int valueIndex, StringBuilder path)
            {
                if (HasVariable)
                {
                    Fx.Assert(valueIndex < values.Length, "Not enough values to bind");
                    if (string.IsNullOrEmpty(values[valueIndex]))
                    {
                        valueIndex++;
                    }
                    else
                    {
                        path.Append(values[valueIndex++]);
                    }
                }
            }
            public void Lookup(int numMatchedSegments, Collection<string> relativePathSegments,
                NameValueCollection boundParameters)
            {
                Fx.Assert(numMatchedSegments == this.owner.segments.Count, "We should have matched the other segments");
                if (HasVariable)
                {
                    StringBuilder remainingPath = new StringBuilder();
                    for (int i = numMatchedSegments; i < relativePathSegments.Count; i++)
                    {
                        if (i < relativePathSegments.Count - 1)
                        {
                            remainingPath.AppendFormat("{0}/", relativePathSegments[i]);
                        }
                        else
                        {
                            remainingPath.Append(relativePathSegments[i]);
                        }
                    }
                    boundParameters.Add(this.varName, remainingPath.ToString());
                }
            }
        }
    }
}
