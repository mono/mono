//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel;
    using System.Runtime.CompilerServices;

    // this class is thread-safe
    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UriTemplateTable
    {
        Uri baseAddress;
        string basePath;
        Dictionary<string, FastPathInfo> fastPathTable; // key is uri.PathAndQuery, fastPathTable may be null
        bool noTemplateHasQueryPart;
        int numSegmentsInBaseAddress;
        Uri originalUncanonicalizedBaseAddress;
        UriTemplateTrieNode rootNode;
        UriTemplatesCollection templates;
        object thisLock;
        bool addTrailingSlashToBaseAddress;

        public UriTemplateTable()
            : this(null, null, true)
        {
        }
        public UriTemplateTable(IEnumerable<KeyValuePair<UriTemplate, object>> keyValuePairs)
            : this(null, keyValuePairs, true)
        {
        }
        public UriTemplateTable(Uri baseAddress)
            : this(baseAddress, null, true)
        {
        }

        internal UriTemplateTable(Uri baseAddress, bool addTrailingSlashToBaseAddress)
            : this(baseAddress, null, addTrailingSlashToBaseAddress)
        {
        }

        public UriTemplateTable(Uri baseAddress, IEnumerable<KeyValuePair<UriTemplate, object>> keyValuePairs)
            : this(baseAddress, keyValuePairs, true)
        {
        }

        internal UriTemplateTable(Uri baseAddress, IEnumerable<KeyValuePair<UriTemplate, object>> keyValuePairs, bool addTrailingSlashToBaseAddress)
        {
            if (baseAddress != null && !baseAddress.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("baseAddress", SR.GetString(
                    SR.UTTMustBeAbsolute));
            }
            
            this.addTrailingSlashToBaseAddress = addTrailingSlashToBaseAddress;
            this.originalUncanonicalizedBaseAddress = baseAddress;
            
            if (keyValuePairs != null)
            {
                this.templates = new UriTemplatesCollection(keyValuePairs);
            }
            else
            {
                this.templates = new UriTemplatesCollection();
            }

            this.thisLock = new object();
            this.baseAddress = baseAddress;
            NormalizeBaseAddress();
        }

        public Uri BaseAddress
        {
            get
            {
                return this.baseAddress;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                lock (this.thisLock)
                {
                    if (this.IsReadOnly)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.UTTCannotChangeBaseAddress)));
                    }
                    else
                    {
                        if (!value.IsAbsoluteUri)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(
                                SR.UTTBaseAddressMustBeAbsolute));
                        }
                        else
                        {
                            this.originalUncanonicalizedBaseAddress = value;
                            this.baseAddress = value;
                            NormalizeBaseAddress();
                        }
                    }
                }
            }
        }

        public Uri OriginalBaseAddress
        {
            get
            {
                return this.originalUncanonicalizedBaseAddress;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.templates.IsFrozen;
            }
        }
        public IList<KeyValuePair<UriTemplate, object>> KeyValuePairs
        {
            get
            {
                return this.templates;
            }
        }

        public void MakeReadOnly(bool allowDuplicateEquivalentUriTemplates)
        {
            // idempotent
            lock (this.thisLock)
            {
                if (!this.IsReadOnly)
                {
                    this.templates.Freeze();
                    Validate(allowDuplicateEquivalentUriTemplates);
                    ConstructFastPathTable();
                }
            }
        }
        public Collection<UriTemplateMatch> Match(Uri uri)
        {
            if (uri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
            }
            if (!uri.IsAbsoluteUri)
            {
                return None();
            }

            this.MakeReadOnly(true);

            // Matching path :
            Collection<String> relativeSegments;
            IList<UriTemplateTableMatchCandidate> candidates;
            if (!FastComputeRelativeSegmentsAndLookup(uri, out relativeSegments, out candidates))
            {
                return None();
            }
            // Matching query :
            NameValueCollection queryParameters = null;
            if (!this.noTemplateHasQueryPart && AtLeastOneCandidateHasQueryPart(candidates))
            {
                Collection<UriTemplateTableMatchCandidate> nextCandidates = new Collection<UriTemplateTableMatchCandidate>();
                Fx.Assert(nextCandidates.Count == 0, "nextCandidates should be empty");

                // then deal with query
                queryParameters = UriTemplateHelpers.ParseQueryString(uri.Query);
                bool mustBeEspeciallyInteresting = NoCandidateHasQueryLiteralRequirementsAndThereIsAnEmptyFallback(candidates);
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (UriTemplateHelpers.CanMatchQueryInterestingly(candidates[i].Template, queryParameters, mustBeEspeciallyInteresting))
                    {
                        nextCandidates.Add(candidates[i]);
                    }
                }
                if (nextCandidates.Count > 1)
                {
                    Fx.Assert(AllEquivalent(nextCandidates, 0, nextCandidates.Count), "demux algorithm problem, multiple non-equivalent matches");
                }

                if (nextCandidates.Count == 0)
                {
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        if (UriTemplateHelpers.CanMatchQueryTrivially(candidates[i].Template))
                        {
                            nextCandidates.Add(candidates[i]);
                        }
                    }
                }
                if (nextCandidates.Count == 0)
                {
                    return None();
                }
                if (nextCandidates.Count > 1)
                {
                    Fx.Assert(AllEquivalent(nextCandidates, 0, nextCandidates.Count), "demux algorithm problem, multiple non-equivalent matches");
                }

                candidates = nextCandidates;
            }
            // Verifying that we have not broken the allowDuplicates settings because of terminal defaults
            //  This situation can be caused when we are hosting ".../" and ".../{foo=xyz}" in the same
            //  table. They are not equivalent; yet they reside together in the same path partially-equivalent
            //  set. If we hit a uri that ends up in that particular end-of-path set, we want to provide the
            //  user only the 'best' match and not both; thus preventing inconsistancy between the MakeReadonly
            //  settings and the matching results. We will assume that the 'best' matches will be the ones with
            //  the smallest number of segments - this will prefer ".../" over ".../{x=1}[/...]".
            if (NotAllCandidatesArePathFullyEquivalent(candidates))
            {
                Collection<UriTemplateTableMatchCandidate> nextCandidates = new Collection<UriTemplateTableMatchCandidate>();
                int minSegmentsCount = -1;
                for (int i = 0; i < candidates.Count; i++)
                {
                    UriTemplateTableMatchCandidate candidate = candidates[i];
                    if (minSegmentsCount == -1)
                    {
                        minSegmentsCount = candidate.Template.segments.Count;
                        nextCandidates.Add(candidate);
                    }
                    else if (candidate.Template.segments.Count < minSegmentsCount)
                    {
                        minSegmentsCount = candidate.Template.segments.Count;
                        nextCandidates.Clear();
                        nextCandidates.Add(candidate);
                    }
                    else if (candidate.Template.segments.Count == minSegmentsCount)
                    {
                        nextCandidates.Add(candidate);
                    }
                }
                Fx.Assert(minSegmentsCount != -1, "At least the first entry in the list should be kept");
                Fx.Assert(nextCandidates.Count >= 1, "At least the first entry in the list should be kept");
                Fx.Assert(nextCandidates[0].Template.segments.Count == minSegmentsCount, "Trivial");
                candidates = nextCandidates;
            }

            // Building the actual result
            Collection<UriTemplateMatch> actualResults = new Collection<UriTemplateMatch>();
            for (int i = 0; i < candidates.Count; i++)
            {
                UriTemplateTableMatchCandidate candidate = candidates[i];
                UriTemplateMatch match = candidate.Template.CreateUriTemplateMatch(this.originalUncanonicalizedBaseAddress,
                    uri, candidate.Data, candidate.SegmentsCount, relativeSegments, queryParameters);
                actualResults.Add(match);
            }
            return actualResults;
        }
        public UriTemplateMatch MatchSingle(Uri uri)
        {
            Collection<UriTemplateMatch> c = this.Match(uri);
            if (c.Count == 0)
            {
                return null;
            }
            if (c.Count == 1)
            {
                return c[0];
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new UriTemplateMatchException(SR.GetString(
                SR.UTTMultipleMatches)));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method is called within a Debug assert")]
        static bool AllEquivalent(IList<UriTemplateTableMatchCandidate> list, int a, int b)
        {
            for (int i = a; i < b - 1; ++i)
            {
                if (!list[i].Template.IsPathPartiallyEquivalentAt(list[i + 1].Template, list[i].SegmentsCount))
                {
                    return false;
                }
                if (!list[i].Template.IsQueryEquivalent(list[i + 1].Template))
                {
                    return false;
                }
            }
            return true;
        }

        static bool AtLeastOneCandidateHasQueryPart(IList<UriTemplateTableMatchCandidate> candidates)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (!UriTemplateHelpers.CanMatchQueryTrivially(candidates[i].Template))
                {
                    return true;
                }
            }
            return false;
        }
        static bool NoCandidateHasQueryLiteralRequirementsAndThereIsAnEmptyFallback(
            IList<UriTemplateTableMatchCandidate> candidates)
        {
            bool thereIsAmEmptyFallback = false;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (UriTemplateHelpers.HasQueryLiteralRequirements(candidates[i].Template))
                {
                    return false;
                }
                if (candidates[i].Template.queries.Count == 0)
                {
                    thereIsAmEmptyFallback = true;
                }
            }
            return thereIsAmEmptyFallback;
        }

        static Collection<UriTemplateMatch> None()
        {
            return new Collection<UriTemplateMatch>();
        }
        static bool NotAllCandidatesArePathFullyEquivalent(IList<UriTemplateTableMatchCandidate> candidates)
        {
            if (candidates.Count <= 1)
            {
                return false;
            }

            int segmentsCount = -1;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (segmentsCount == -1)
                {
                    segmentsCount = candidates[i].Template.segments.Count;
                }
                else if (segmentsCount != candidates[i].Template.segments.Count)
                {
                    return true;
                }
            }
            return false;
        }

        bool ComputeRelativeSegmentsAndLookup(Uri uri,
            ICollection<string> relativePathSegments, // add to this
            ICollection<UriTemplateTableMatchCandidate> candidates) // matched candidates
        {
            string[] uriSegments = uri.Segments;
            int numRelativeSegments = uriSegments.Length - this.numSegmentsInBaseAddress;
            Fx.Assert(numRelativeSegments >= 0, "bad num segments");
            UriTemplateLiteralPathSegment[] uSegments = new UriTemplateLiteralPathSegment[numRelativeSegments];
            for (int i = 0; i < numRelativeSegments; ++i)
            {
                string seg = uriSegments[i + this.numSegmentsInBaseAddress];
                // compute representation for matching
                UriTemplateLiteralPathSegment lps = UriTemplateLiteralPathSegment.CreateFromWireData(seg);
                uSegments[i] = lps;
                // compute representation to project out into results
                string relPathSeg = Uri.UnescapeDataString(seg);
                if (lps.EndsWithSlash)
                {
                    Fx.Assert(relPathSeg.EndsWith("/", StringComparison.Ordinal), "problem with relative path segment");
                    relPathSeg = relPathSeg.Substring(0, relPathSeg.Length - 1); // trim slash
                }
                relativePathSegments.Add(relPathSeg);
            }
            return rootNode.Match(uSegments, candidates);
        }
        void ConstructFastPathTable()
        {
            this.noTemplateHasQueryPart = true;
            foreach (KeyValuePair<UriTemplate, object> kvp in this.templates)
            {
                UriTemplate ut = kvp.Key;
                if (!UriTemplateHelpers.CanMatchQueryTrivially(ut))
                {
                    this.noTemplateHasQueryPart = false;
                }
                if (ut.HasNoVariables && !ut.HasWildcard)
                {
                    // eligible for fast path
                    if (this.fastPathTable == null)
                    {
                        this.fastPathTable = new Dictionary<string, FastPathInfo>();
                    }
                    Uri uri = ut.BindByPosition(this.originalUncanonicalizedBaseAddress);
                    string uriPath = UriTemplateHelpers.GetUriPath(uri);
                    if (this.fastPathTable.ContainsKey(uriPath))
                    {
                        // nothing to do, we've already seen it
                    }
                    else
                    {
                        FastPathInfo fpInfo = new FastPathInfo();
                        if (ComputeRelativeSegmentsAndLookup(uri, fpInfo.RelativePathSegments,
                            fpInfo.Candidates))
                        {
                            fpInfo.Freeze();
                            this.fastPathTable.Add(uriPath, fpInfo);
                        }
                    }
                }
            }
        }
        // this method checks the literal cache for a match if none, goes through the slower path of cracking the segments
        bool FastComputeRelativeSegmentsAndLookup(Uri uri, out Collection<string> relativePathSegments,
            out IList<UriTemplateTableMatchCandidate> candidates)
        {
            // Consider fast-path and lookup
            // return false if not under base uri
            string uriPath = UriTemplateHelpers.GetUriPath(uri);
            FastPathInfo fpInfo = null;
            if ((this.fastPathTable != null) && this.fastPathTable.TryGetValue(uriPath, out fpInfo))
            {
                relativePathSegments = fpInfo.RelativePathSegments;
                candidates = fpInfo.Candidates;
                VerifyThatFastPathAndSlowPathHaveSameResults(uri, relativePathSegments, candidates);
                return true;
            }
            else
            {
                relativePathSegments = new Collection<string>();
                candidates = new Collection<UriTemplateTableMatchCandidate>();
                return SlowComputeRelativeSegmentsAndLookup(uri, uriPath, relativePathSegments, candidates);
            }
        }

        void NormalizeBaseAddress()
        {
            if (this.baseAddress != null)
            {
                // ensure trailing slash on baseAddress, so that IsBaseOf will work later
                UriBuilder ub = new UriBuilder(this.baseAddress);
                if (this.addTrailingSlashToBaseAddress && !ub.Path.EndsWith("/", StringComparison.Ordinal))
                {
                    ub.Path = ub.Path + "/";
                }
                ub.Host = "localhost"; // always normalize to localhost
                ub.Port = -1;
                ub.UserName = null;
                ub.Password = null;
                ub.Path = ub.Path.ToUpperInvariant();
                ub.Scheme = Uri.UriSchemeHttp;
                this.baseAddress = ub.Uri;
                basePath = UriTemplateHelpers.GetUriPath(this.baseAddress);
            }
        }
        bool SlowComputeRelativeSegmentsAndLookup(Uri uri, string uriPath, Collection<string> relativePathSegments,
            ICollection<UriTemplateTableMatchCandidate> candidates)
        {
            // ensure 'under' the base address
            if (uriPath.Length < basePath.Length)
            {
                return false;
            }

            if (!uriPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else
            {
                // uriPath StartsWith basePath, but this check is not enough - basePath 'service1' should not match with uriPath 'service123'
                // make sure that after the match the next character is /, this is to avoid a uriPath of the form /service12/ matching with a basepath of the form /service1
                if (uriPath.Length > basePath.Length && !basePath.EndsWith("/", StringComparison.Ordinal) && uriPath[basePath.Length] != '/')
                {
                    return false;
                }
            }

            return ComputeRelativeSegmentsAndLookup(uri, relativePathSegments, candidates);
        }

        void Validate(bool allowDuplicateEquivalentUriTemplates)
        {
            if (this.baseAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.UTTBaseAddressNotSet)));
            }
            this.numSegmentsInBaseAddress = this.baseAddress.Segments.Length;
            if (this.templates.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.UTTEmptyKeyValuePairs)));
            }
            // build the trie and
            // validate that forall Uri u, at most one UriTemplate is a best match for u
            rootNode = UriTemplateTrieNode.Make(this.templates, allowDuplicateEquivalentUriTemplates);
        }
        [Conditional("DEBUG")]
        void VerifyThatFastPathAndSlowPathHaveSameResults(Uri uri, Collection<string> fastPathRelativePathSegments,
            IList<UriTemplateTableMatchCandidate> fastPathCandidates)
        {
            Collection<string> slowPathRelativePathSegments = new Collection<string>();
            List<UriTemplateTableMatchCandidate> slowPathCandidates = new List<UriTemplateTableMatchCandidate>();
            if (!SlowComputeRelativeSegmentsAndLookup(uri, UriTemplateHelpers.GetUriPath(uri),
                slowPathRelativePathSegments, slowPathCandidates))
            {
                Fx.Assert("fast path yielded a result but slow path yielded no result");
            }
            // compare results
            if (fastPathRelativePathSegments.Count != slowPathRelativePathSegments.Count)
            {
                Fx.Assert("fast path yielded different number of segments from slow path");
            }
            for (int i = 0; i < fastPathRelativePathSegments.Count; ++i)
            {
                if (fastPathRelativePathSegments[i] != slowPathRelativePathSegments[i])
                {
                    Fx.Assert("fast path yielded different segments from slow path");
                }
            }
            if (fastPathCandidates.Count != slowPathCandidates.Count)
            {
                Fx.Assert("fast path yielded different number of candidates from slow path");
            }
            for (int i = 0; i < fastPathCandidates.Count; i++)
            {
                if (!slowPathCandidates.Contains(fastPathCandidates[i]))
                {
                    Fx.Assert("fast path yielded different candidates from slow path");
                }
            }
        }

        class FastPathInfo
        {
            FreezableCollection<UriTemplateTableMatchCandidate> candidates;
            FreezableCollection<string> relativePathSegments;

            public FastPathInfo()
            {
                this.relativePathSegments = new FreezableCollection<string>();
                this.candidates = new FreezableCollection<UriTemplateTableMatchCandidate>();
            }
            public Collection<UriTemplateTableMatchCandidate> Candidates
            {
                get
                {
                    return this.candidates;
                }
            }

            public Collection<string> RelativePathSegments
            {
                get
                {
                    return this.relativePathSegments;
                }
            }

            public void Freeze()
            {
                this.relativePathSegments.Freeze();
                this.candidates.Freeze();
            }
        }

        class UriTemplatesCollection : FreezableCollection<KeyValuePair<UriTemplate, object>>
        {
            public UriTemplatesCollection()
                : base()
            {
            }
            [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "This is a private class; virtual methods cannot be overriden")]
            public UriTemplatesCollection(IEnumerable<KeyValuePair<UriTemplate, object>> keyValuePairs)
                : base()
            {
                foreach (KeyValuePair<UriTemplate, object> kvp in keyValuePairs)
                {
                    ThrowIfInvalid(kvp.Key, "keyValuePairs");
                    base.Add(kvp);
                }
            }

            protected override void InsertItem(int index, KeyValuePair<UriTemplate, object> item)
            {
                ThrowIfInvalid(item.Key, "item");
                base.InsertItem(index, item);
            }
            protected override void SetItem(int index, KeyValuePair<UriTemplate, object> item)
            {
                ThrowIfInvalid(item.Key, "item");
                base.SetItem(index, item);
            }

            static void ThrowIfInvalid(UriTemplate template, string argName)
            {
                if (template == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(argName,
                        SR.GetString(SR.UTTNullTemplateKey));
                }
                if (template.IgnoreTrailingSlash)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(argName,
                        SR.GetString(SR.UTTInvalidTemplateKey, template));
                }
            }
        }
    }
}
