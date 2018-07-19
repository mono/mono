//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel;

    class UriTemplateTrieNode
    {
        int depth; // relative segment depth (root = 0)
        UriTemplatePathPartiallyEquivalentSet endOfPath; // matches the non-existent segment at the end of a slash-terminated path
        AscendingSortedCompoundSegmentsCollection<UriTemplatePathPartiallyEquivalentSet> finalCompoundSegment; // matches e.g. "{var}.{var}"
        Dictionary<UriTemplateLiteralPathSegment, UriTemplatePathPartiallyEquivalentSet> finalLiteralSegment; // matches e.g. "segmentThatDoesntEndInSlash"
        UriTemplatePathPartiallyEquivalentSet finalVariableSegment; // matches e.g. "{var}"
        AscendingSortedCompoundSegmentsCollection<UriTemplateTrieLocation> nextCompoundSegment; // all are AfterLiteral; matches e.g. "{var}.{var}/"
        Dictionary<UriTemplateLiteralPathSegment, UriTemplateTrieLocation> nextLiteralSegment; // all are BeforeLiteral; matches e.g. "path/"
        UriTemplateTrieLocation nextVariableSegment; // is BeforeLiteral; matches e.g. "{var}/"
        UriTemplateTrieLocation onFailure; // points to parent, at 'after me'
        UriTemplatePathPartiallyEquivalentSet star; // matches any "extra/path/segments" at the end

        UriTemplateTrieNode(int depth)
        {
            this.depth = depth;
            this.nextLiteralSegment = null;
            this.nextCompoundSegment = null;
            this.finalLiteralSegment = null;
            this.finalCompoundSegment = null;
            this.finalVariableSegment = new UriTemplatePathPartiallyEquivalentSet(depth + 1);
            this.star = new UriTemplatePathPartiallyEquivalentSet(depth);
            this.endOfPath = new UriTemplatePathPartiallyEquivalentSet(depth);
        }

        public static UriTemplateTrieNode Make(IEnumerable<KeyValuePair<UriTemplate, object>> keyValuePairs,
            bool allowDuplicateEquivalentUriTemplates)
        {
            // given a UTT at MakeReadOnly time, build the trie
            // note that root.onFailure == null;
            UriTemplateTrieNode root = new UriTemplateTrieNode(0);
            foreach (KeyValuePair<UriTemplate, object> kvp in keyValuePairs)
            {
                Add(root, kvp);
            }
            Validate(root, allowDuplicateEquivalentUriTemplates);
            return root;
        }

        public bool Match(UriTemplateLiteralPathSegment[] wireData, ICollection<UriTemplateTableMatchCandidate> candidates)
        {
            UriTemplateTrieLocation currentLocation = new UriTemplateTrieLocation(this, UriTemplateTrieIntraNodeLocation.BeforeLiteral);
            return GetMatch(currentLocation, wireData, candidates);
        }

        static void Add(UriTemplateTrieNode root, KeyValuePair<UriTemplate, object> kvp)
        {
            // Currently UTT doesn't support teplates with ignoreTrailingSlash == true; thus we
            //  don't care about supporting it in the trie as well.
            UriTemplateTrieNode current = root;
            UriTemplate ut = kvp.Key;
            bool needProcessingOnFinalNode = ((ut.segments.Count == 0) || ut.HasWildcard ||
                ut.segments[ut.segments.Count - 1].EndsWithSlash);
            for (int i = 0; i < ut.segments.Count; ++i)
            {
                if (i >= ut.firstOptionalSegment)
                {
                    current.endOfPath.Items.Add(kvp);
                }
                UriTemplatePathSegment ps = ut.segments[i];
                if (!ps.EndsWithSlash)
                {
                    Fx.Assert(i == ut.segments.Count - 1, "only the last segment can !EndsWithSlash");
                    Fx.Assert(!ut.HasWildcard, "path star cannot have !EndsWithSlash");
                    switch (ps.Nature)
                    {
                        case UriTemplatePartType.Literal:
                            current.AddFinalLiteralSegment(ps as UriTemplateLiteralPathSegment, kvp);
                            break;

                        case UriTemplatePartType.Compound:
                            current.AddFinalCompoundSegment(ps as UriTemplateCompoundPathSegment, kvp);
                            break;

                        case UriTemplatePartType.Variable:
                            current.finalVariableSegment.Items.Add(kvp);
                            break;

                        default:
                            Fx.Assert("Invalid value as PathSegment.Nature");
                            break;
                    }
                }
                else
                {
                    Fx.Assert(ps.EndsWithSlash, "ps.EndsWithSlash");
                    switch (ps.Nature)
                    {
                        case UriTemplatePartType.Literal:
                            current = current.AddNextLiteralSegment(ps as UriTemplateLiteralPathSegment);
                            break;

                        case UriTemplatePartType.Compound:
                            current = current.AddNextCompoundSegment(ps as UriTemplateCompoundPathSegment);
                            break;

                        case UriTemplatePartType.Variable:
                            current = current.AddNextVariableSegment();
                            break;

                        default:
                            Fx.Assert("Invalid value as PathSegment.Nature");
                            break;
                    }
                }
            }
            if (needProcessingOnFinalNode)
            {
                // if the last segment ended in a slash, there is still more to do
                if (ut.HasWildcard)
                {
                    // e.g. "path1/path2/*"
                    current.star.Items.Add(kvp);
                }
                else
                {
                    // e.g. "path1/path2/"
                    current.endOfPath.Items.Add(kvp);
                }
            }
        }

        static bool CheckMultipleMatches(IList<IList<UriTemplateTrieLocation>> locationsSet, UriTemplateLiteralPathSegment[] wireData,
            ICollection<UriTemplateTableMatchCandidate> candidates)
        {
            bool result = false;
            for (int i = 0; ((i < locationsSet.Count) && !result); i++)
            {
                for (int j = 0; j < locationsSet[i].Count; j++)
                {
                    if (GetMatch(locationsSet[i][j], wireData, candidates))
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
        static bool GetMatch(UriTemplateTrieLocation location, UriTemplateLiteralPathSegment[] wireData,
            ICollection<UriTemplateTableMatchCandidate> candidates)
        {
            int initialDepth = location.node.depth;
            SingleLocationOrLocationsSet nextStep;
            UriTemplatePathPartiallyEquivalentSet answer;
            do
            {
                if (TryMatch(wireData, location, out answer, out nextStep))
                {
                    if (answer != null)
                    {
                        for (int i = 0; i < answer.Items.Count; i++)
                        {
                            candidates.Add(new UriTemplateTableMatchCandidate(answer.Items[i].Key, answer.SegmentsCount,
                                answer.Items[i].Value));
                        }
                    }
                    return true;
                }
                if (nextStep.IsSingle)
                {
                    location = nextStep.SingleLocation;
                }
                else
                {
                    Fx.Assert(nextStep.LocationsSet != null, "This should be set to a valid value by TryMatch");
                    if (CheckMultipleMatches(nextStep.LocationsSet, wireData, candidates))
                    {
                        return true;
                    }
                    location = GetFailureLocationFromLocationsSet(nextStep.LocationsSet);
                }
            } while ((location != null) && (location.node.depth >= initialDepth));

            // we walked the whole trie down and found nothing
            return false;
        }
        static bool TryMatch(UriTemplateLiteralPathSegment[] wireUriSegments, UriTemplateTrieLocation currentLocation,
            out UriTemplatePathPartiallyEquivalentSet success, out SingleLocationOrLocationsSet nextStep)
        {
            // if returns true, success is set to answer
            // if returns false, nextStep is set to next place to look
            success = null;
            nextStep = new SingleLocationOrLocationsSet();

            if (wireUriSegments.Length <= currentLocation.node.depth)
            {
                Fx.Assert(wireUriSegments.Length == 0 || wireUriSegments[wireUriSegments.Length - 1].EndsWithSlash,
                    "we should not have traversed this deep into the trie unless the wire path ended in a slash");

                if (currentLocation.node.endOfPath.Items.Count != 0)
                {
                    // exact match of e.g. "path1/path2/"
                    success = currentLocation.node.endOfPath;
                    return true;
                }
                else if (currentLocation.node.star.Items.Count != 0)
                {
                    // inexact match of e.g. WIRE("path1/path2/") against TEMPLATE("path1/path2/*")
                    success = currentLocation.node.star;
                    return true;
                }
                else
                {
                    nextStep = new SingleLocationOrLocationsSet(currentLocation.node.onFailure);
                    return false;
                }
            }
            else
            {
                UriTemplateLiteralPathSegment curWireSeg = wireUriSegments[currentLocation.node.depth];
                bool considerLiteral = false;
                bool considerCompound = false;
                bool considerVariable = false;
                bool considerStar = false;
                switch (currentLocation.locationWithin)
                {
                    case UriTemplateTrieIntraNodeLocation.BeforeLiteral:
                        considerLiteral = true;
                        considerCompound = true;
                        considerVariable = true;
                        considerStar = true;
                        break;
                    case UriTemplateTrieIntraNodeLocation.AfterLiteral:
                        considerLiteral = false;
                        considerCompound = true;
                        considerVariable = true;
                        considerStar = true;
                        break;
                    case UriTemplateTrieIntraNodeLocation.AfterCompound:
                        considerLiteral = false;
                        considerCompound = false;
                        considerVariable = true;
                        considerStar = true;
                        break;
                    case UriTemplateTrieIntraNodeLocation.AfterVariable:
                        considerLiteral = false;
                        considerCompound = false;
                        considerVariable = false;
                        considerStar = true;
                        break;
                    default:
                        Fx.Assert("bad kind");
                        break;
                }
                if (curWireSeg.EndsWithSlash)
                {
                    IList<IList<UriTemplateTrieLocation>> compoundLocationsSet;

                    if (considerLiteral && currentLocation.node.nextLiteralSegment != null &&
                        currentLocation.node.nextLiteralSegment.ContainsKey(curWireSeg))
                    {
                        nextStep = new SingleLocationOrLocationsSet(currentLocation.node.nextLiteralSegment[curWireSeg]);
                        return false;
                    }
                    else if (considerCompound && currentLocation.node.nextCompoundSegment != null &&
                        AscendingSortedCompoundSegmentsCollection<UriTemplateTrieLocation>.Lookup(currentLocation.node.nextCompoundSegment, curWireSeg, out compoundLocationsSet))
                    {
                        nextStep = new SingleLocationOrLocationsSet(compoundLocationsSet);
                        return false;
                    }
                    else if (considerVariable && currentLocation.node.nextVariableSegment != null &&
                        !curWireSeg.IsNullOrEmpty())
                    {
                        nextStep = new SingleLocationOrLocationsSet(currentLocation.node.nextVariableSegment);
                        return false;
                    }
                    else if (considerStar && currentLocation.node.star.Items.Count != 0)
                    {
                        // matches e.g. WIRE("path1/path2/path3") and TEMPLATE("path1/*")
                        success = currentLocation.node.star;
                        return true;
                    }
                    else
                    {
                        nextStep = new SingleLocationOrLocationsSet(currentLocation.node.onFailure);
                        return false;
                    }
                }
                else
                {
                    IList<IList<UriTemplatePathPartiallyEquivalentSet>> compoundPathEquivalentSets;

                    Fx.Assert(!curWireSeg.EndsWithSlash, "!curWireSeg.EndsWithSlash");
                    Fx.Assert(!curWireSeg.IsNullOrEmpty(), "!curWireSeg.IsNullOrEmpty()");
                    if (considerLiteral && currentLocation.node.finalLiteralSegment != null &&
                        currentLocation.node.finalLiteralSegment.ContainsKey(curWireSeg))
                    {
                        // matches e.g. WIRE("path1/path2") and TEMPLATE("path1/path2")
                        success = currentLocation.node.finalLiteralSegment[curWireSeg];
                        return true;
                    }
                    else if (considerCompound && currentLocation.node.finalCompoundSegment != null &&
                        AscendingSortedCompoundSegmentsCollection<UriTemplatePathPartiallyEquivalentSet>.Lookup(currentLocation.node.finalCompoundSegment, curWireSeg, out compoundPathEquivalentSets))
                    {
                        // matches e.g. WIRE("path1/path2") and TEMPLATE("path1/p{var}th2")
                        // we should take only the highest order match!
                        Fx.Assert(compoundPathEquivalentSets.Count >= 1, "Lookup is expected to return false otherwise");
                        Fx.Assert(compoundPathEquivalentSets[0].Count > 0, "Find shouldn't return empty sublists");
                        if (compoundPathEquivalentSets[0].Count == 1)
                        {
                            success = compoundPathEquivalentSets[0][0];
                        }
                        else
                        {
                            success = new UriTemplatePathPartiallyEquivalentSet(currentLocation.node.depth + 1);
                            for (int i = 0; i < compoundPathEquivalentSets[0].Count; i++)
                            {
                                success.Items.AddRange(compoundPathEquivalentSets[0][i].Items);
                            }
                        }
                        return true;
                    }
                    else if (considerVariable && currentLocation.node.finalVariableSegment.Items.Count != 0)
                    {
                        // matches e.g. WIRE("path1/path2") and TEMPLATE("path1/{var}")
                        success = currentLocation.node.finalVariableSegment;
                        return true;
                    }
                    else if (considerStar && currentLocation.node.star.Items.Count != 0)
                    {
                        // matches e.g. WIRE("path1/path2") and TEMPLATE("path1/*")
                        success = currentLocation.node.star;
                        return true;
                    }
                    else
                    {
                        nextStep = new SingleLocationOrLocationsSet(currentLocation.node.onFailure);
                        return false;
                    }
                }
            }
        }

        static UriTemplateTrieLocation GetFailureLocationFromLocationsSet(IList<IList<UriTemplateTrieLocation>> locationsSet)
        {
            Fx.Assert(locationsSet != null, "Shouldn't be called on null set");
            Fx.Assert(locationsSet.Count > 0, "Shouldn't be called on empty set");
            Fx.Assert(locationsSet[0] != null, "Shouldn't be called on a set with null sub-lists");
            Fx.Assert(locationsSet[0].Count > 0, "Shouldn't be called on a set with empty sub-lists");

            return locationsSet[0][0].node.onFailure;
        }

        static void Validate(UriTemplateTrieNode root, bool allowDuplicateEquivalentUriTemplates)
        {
            // walk the entire tree, and ensure that each PathEquivalentSet is ok (no ambiguous queries),
            // verify thst compound segments didn't add potentialy multiple matchs;
            // also Assert various data-structure invariants
            Queue<UriTemplateTrieNode> nodesQueue = new Queue<UriTemplateTrieNode>();

            UriTemplateTrieNode current = root;
            while (true)
            {
                // validate all the PathEquivalentSets that live in this node
                Validate(current.endOfPath, allowDuplicateEquivalentUriTemplates);
                Validate(current.finalVariableSegment, allowDuplicateEquivalentUriTemplates);
                Validate(current.star, allowDuplicateEquivalentUriTemplates);
                if (current.finalLiteralSegment != null)
                {
                    foreach (KeyValuePair<UriTemplateLiteralPathSegment, UriTemplatePathPartiallyEquivalentSet> kvp in current.finalLiteralSegment)
                    {
                        Validate(kvp.Value, allowDuplicateEquivalentUriTemplates);
                    }
                }
                if (current.finalCompoundSegment != null)
                {
                    IList<IList<UriTemplatePathPartiallyEquivalentSet>> pesLists = current.finalCompoundSegment.Values;
                    for (int i = 0; i < pesLists.Count; i++)
                    {
                        if (!allowDuplicateEquivalentUriTemplates && (pesLists[i].Count > 1))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                                SR.UTTDuplicate, pesLists[i][0].Items[0].Key.ToString(), pesLists[i][1].Items[0].Key.ToString())));
                        }
                        for (int j = 0; j < pesLists[i].Count; j++)
                        {
                            Validate(pesLists[i][j], allowDuplicateEquivalentUriTemplates);
                        }
                    }
                }
                // deal with children of this node
                if (current.nextLiteralSegment != null)
                {
                    foreach (KeyValuePair<UriTemplateLiteralPathSegment, UriTemplateTrieLocation> kvp in current.nextLiteralSegment)
                    {
                        Fx.Assert(kvp.Value.locationWithin == UriTemplateTrieIntraNodeLocation.BeforeLiteral, "forward-pointers should always point to a BeforeLiteral location");
                        Fx.Assert(kvp.Value.node.depth == current.depth + 1, "kvp.Value.node.depth == current.depth + 1");
                        Fx.Assert(kvp.Value.node.onFailure.node == current, "back pointer should point back to here");
                        Fx.Assert(kvp.Value.node.onFailure.locationWithin == UriTemplateTrieIntraNodeLocation.AfterLiteral, "back-pointer should be AfterLiteral");
                        nodesQueue.Enqueue(kvp.Value.node);
                    }
                }
                if (current.nextCompoundSegment != null)
                {
                    IList<IList<UriTemplateTrieLocation>> locations = current.nextCompoundSegment.Values;
                    for (int i = 0; i < locations.Count; i++)
                    {
                        if (!allowDuplicateEquivalentUriTemplates && (locations[i].Count > 1))
                        {
                            // In the future we might ease up the restrictions and verify if there is realy
                            // a potential multiple match here; for now we are throwing.
                            UriTemplate firstTemplate = FindAnyUriTemplate(locations[i][0].node);
                            UriTemplate secondTemplate = FindAnyUriTemplate(locations[i][1].node);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                                SR.UTTDuplicate, firstTemplate.ToString(), secondTemplate.ToString())));
                        }
                        for (int j = 0; j < locations[i].Count; j++)
                        {
                            UriTemplateTrieLocation location = locations[i][j];
                            Fx.Assert(location.locationWithin == UriTemplateTrieIntraNodeLocation.BeforeLiteral, "forward-pointers should always point to a BeforeLiteral location");
                            Fx.Assert(location.node.depth == current.depth + 1, "kvp.Value.node.depth == current.depth + 1");
                            Fx.Assert(location.node.onFailure.node == current, "back pointer should point back to here");
                            Fx.Assert(location.node.onFailure.locationWithin == UriTemplateTrieIntraNodeLocation.AfterCompound, "back-pointer should be AfterCompound");
                            nodesQueue.Enqueue(location.node);
                        }
                    }
                }
                if (current.nextVariableSegment != null)
                {
                    Fx.Assert(current.nextVariableSegment.locationWithin == UriTemplateTrieIntraNodeLocation.BeforeLiteral, "forward-pointers should always point to a BeforeLiteral location");
                    Fx.Assert(current.nextVariableSegment.node.depth == current.depth + 1, "current.nextVariableSegment.node.depth == current.depth + 1");
                    Fx.Assert(current.nextVariableSegment.node.onFailure.node == current, "back pointer should point back to here");
                    Fx.Assert(current.nextVariableSegment.node.onFailure.locationWithin == UriTemplateTrieIntraNodeLocation.AfterVariable, "back-pointer should be AfterVariable");
                    nodesQueue.Enqueue(current.nextVariableSegment.node);
                }
                // move on to next bit of work
                if (nodesQueue.Count == 0)
                {
                    break;
                }
                current = nodesQueue.Dequeue();
            }
        }
        static void Validate(UriTemplatePathPartiallyEquivalentSet pes, bool allowDuplicateEquivalentUriTemplates)
        {
            // A set with 0 or 1 items is valid by definition
            if (pes.Items.Count < 2)
            {
                return;
            }
            // Assert all paths are partially-equivalent
            for (int i = 0; i < pes.Items.Count - 1; ++i)
            {
                Fx.Assert(pes.Items[i].Key.IsPathPartiallyEquivalentAt(pes.Items[i + 1].Key, pes.SegmentsCount),
                    "all elements of a PES must be path partially-equivalent");
            }
            // We will check that the queries disambiguate only for templates, which are
            //  matched completely at the segments count; templates, which are match at
            //  that point due to terminal defaults, will be ruled out.
            UriTemplate[] a = new UriTemplate[pes.Items.Count];
            int arrayIndex = 0;
            foreach (KeyValuePair<UriTemplate, object> kvp in pes.Items)
            {
                if (pes.SegmentsCount < kvp.Key.segments.Count)
                {
                    continue;
                }
                Fx.Assert(arrayIndex < a.Length, "We made enough room for all the items");
                a[arrayIndex++] = kvp.Key;
            }
            // Ensure that queries disambiguate (if needed) :
            if (arrayIndex > 0)
            {
                UriTemplateHelpers.DisambiguateSamePath(a, 0, arrayIndex, allowDuplicateEquivalentUriTemplates);
            }
        }

        static UriTemplate FindAnyUriTemplate(UriTemplateTrieNode node)
        {
            while (node != null)
            {
                if (node.endOfPath.Items.Count > 0)
                {
                    return node.endOfPath.Items[0].Key;
                }
                if (node.finalVariableSegment.Items.Count > 0)
                {
                    return node.finalVariableSegment.Items[0].Key;
                }
                if (node.star.Items.Count > 0)
                {
                    return node.star.Items[0].Key;
                }
                if (node.finalLiteralSegment != null)
                {
                    UriTemplatePathPartiallyEquivalentSet pes =
                        GetAnyDictionaryValue<UriTemplatePathPartiallyEquivalentSet>(node.finalLiteralSegment);
                    Fx.Assert(pes.Items.Count > 0, "Otherwise, why creating the dictionary?");
                    return pes.Items[0].Key;
                }
                if (node.finalCompoundSegment != null)
                {
                    UriTemplatePathPartiallyEquivalentSet pes = node.finalCompoundSegment.GetAnyValue();
                    Fx.Assert(pes.Items.Count > 0, "Otherwise, why creating the collection?");
                    return pes.Items[0].Key;
                }

                if (node.nextLiteralSegment != null)
                {
                    UriTemplateTrieLocation location =
                        GetAnyDictionaryValue<UriTemplateTrieLocation>(node.nextLiteralSegment);
                    node = location.node;
                }
                else if (node.nextCompoundSegment != null)
                {
                    UriTemplateTrieLocation location = node.nextCompoundSegment.GetAnyValue();
                    node = location.node;
                }
                else if (node.nextVariableSegment != null)
                {
                    node = node.nextVariableSegment.node;
                }
                else
                {
                    node = null;
                }
            }
            Fx.Assert("How did we got here without finding a UriTemplate earlier?");
            return null;
        }
        static T GetAnyDictionaryValue<T>(IDictionary<UriTemplateLiteralPathSegment, T> dictionary)
        {
            using (IEnumerator<T> valuesEnumerator = dictionary.Values.GetEnumerator())
            {
                valuesEnumerator.MoveNext();
                return valuesEnumerator.Current;
            }
        }

        void AddFinalCompoundSegment(UriTemplateCompoundPathSegment cps, KeyValuePair<UriTemplate, object> kvp)
        {
            Fx.Assert(cps != null, "must be - based on the segment nature");
            if (this.finalCompoundSegment == null)
            {
                this.finalCompoundSegment = new AscendingSortedCompoundSegmentsCollection<UriTemplatePathPartiallyEquivalentSet>();
            }
            UriTemplatePathPartiallyEquivalentSet pes = this.finalCompoundSegment.Find(cps);
            if (pes == null)
            {
                pes = new UriTemplatePathPartiallyEquivalentSet(this.depth + 1);
                this.finalCompoundSegment.Add(cps, pes);
            }
            pes.Items.Add(kvp);
        }
        void AddFinalLiteralSegment(UriTemplateLiteralPathSegment lps, KeyValuePair<UriTemplate, object> kvp)
        {
            Fx.Assert(lps != null, "must be - based on the segment nature");
            if (this.finalLiteralSegment != null && this.finalLiteralSegment.ContainsKey(lps))
            {
                this.finalLiteralSegment[lps].Items.Add(kvp);
            }
            else
            {
                if (this.finalLiteralSegment == null)
                {
                    this.finalLiteralSegment = new Dictionary<UriTemplateLiteralPathSegment, UriTemplatePathPartiallyEquivalentSet>();
                }
                UriTemplatePathPartiallyEquivalentSet pes = new UriTemplatePathPartiallyEquivalentSet(this.depth + 1);
                pes.Items.Add(kvp);
                this.finalLiteralSegment.Add(lps, pes);
            }
        }
        UriTemplateTrieNode AddNextCompoundSegment(UriTemplateCompoundPathSegment cps)
        {
            Fx.Assert(cps != null, "must be - based on the segment nature");
            if (this.nextCompoundSegment == null)
            {
                this.nextCompoundSegment = new AscendingSortedCompoundSegmentsCollection<UriTemplateTrieLocation>();
            }
            UriTemplateTrieLocation nextLocation = this.nextCompoundSegment.Find(cps);
            if (nextLocation == null)
            {
                UriTemplateTrieNode nextNode = new UriTemplateTrieNode(this.depth + 1);
                nextNode.onFailure = new UriTemplateTrieLocation(this, UriTemplateTrieIntraNodeLocation.AfterCompound);
                nextLocation = new UriTemplateTrieLocation(nextNode, UriTemplateTrieIntraNodeLocation.BeforeLiteral);
                this.nextCompoundSegment.Add(cps, nextLocation);
            }
            return nextLocation.node;
        }
        UriTemplateTrieNode AddNextLiteralSegment(UriTemplateLiteralPathSegment lps)
        {
            Fx.Assert(lps != null, "must be - based on the segment nature");
            if (this.nextLiteralSegment != null && this.nextLiteralSegment.ContainsKey(lps))
            {
                return this.nextLiteralSegment[lps].node;
            }
            else
            {
                if (this.nextLiteralSegment == null)
                {
                    this.nextLiteralSegment = new Dictionary<UriTemplateLiteralPathSegment, UriTemplateTrieLocation>();
                }
                UriTemplateTrieNode newNode = new UriTemplateTrieNode(this.depth + 1);
                newNode.onFailure = new UriTemplateTrieLocation(this, UriTemplateTrieIntraNodeLocation.AfterLiteral);
                this.nextLiteralSegment.Add(lps, new UriTemplateTrieLocation(newNode, UriTemplateTrieIntraNodeLocation.BeforeLiteral));
                return newNode;
            }
        }
        UriTemplateTrieNode AddNextVariableSegment()
        {
            if (this.nextVariableSegment != null)
            {
                return this.nextVariableSegment.node;
            }
            else
            {
                UriTemplateTrieNode newNode = new UriTemplateTrieNode(this.depth + 1);
                newNode.onFailure = new UriTemplateTrieLocation(this, UriTemplateTrieIntraNodeLocation.AfterVariable);
                this.nextVariableSegment = new UriTemplateTrieLocation(newNode, UriTemplateTrieIntraNodeLocation.BeforeLiteral);
                return newNode;
            }
        }

        struct SingleLocationOrLocationsSet
        {
            readonly bool isSingle;
            readonly IList<IList<UriTemplateTrieLocation>> locationsSet;
            readonly UriTemplateTrieLocation singleLocation;

            public SingleLocationOrLocationsSet(UriTemplateTrieLocation singleLocation)
            {
                this.isSingle = true;
                this.singleLocation = singleLocation;
                this.locationsSet = null;
            }
            public SingleLocationOrLocationsSet(IList<IList<UriTemplateTrieLocation>> locationsSet)
            {
                this.isSingle = false;
                this.singleLocation = null;
                this.locationsSet = locationsSet;
            }

            public bool IsSingle
            {
                get
                {
                    return this.isSingle;
                }
            }
            public IList<IList<UriTemplateTrieLocation>> LocationsSet
            {
                get
                {
                    Fx.Assert(!this.isSingle, "!this.isSingle");
                    return this.locationsSet;
                }
            }
            public UriTemplateTrieLocation SingleLocation
            {
                get
                {
                    Fx.Assert(this.isSingle, "this.isSingle");
                    return this.singleLocation;
                }
            }
        }

        class AscendingSortedCompoundSegmentsCollection<T>
            where T : class
        {
            SortedList<UriTemplateCompoundPathSegment, Collection<CollectionItem>> items;

            public AscendingSortedCompoundSegmentsCollection()
            {
                this.items = new SortedList<UriTemplateCompoundPathSegment, Collection<AscendingSortedCompoundSegmentsCollection<T>.CollectionItem>>();
            }

            public IList<IList<T>> Values
            {
                get
                {
                    IList<IList<T>> results = new List<IList<T>>(this.items.Count);
                    for (int i = 0; i < this.items.Values.Count; i++)
                    {
                        results.Add(new List<T>(this.items.Values[i].Count));
                        Fx.Assert(results.Count == i + 1, "We are adding item for each values collection");
                        for (int j = 0; j < this.items.Values[i].Count; j++)
                        {
                            results[i].Add(this.items.Values[i][j].Value);
                            Fx.Assert(results[i].Count == j + 1, "We are adding item for each value in the collection");
                        }
                        Fx.Assert(results[i].Count == this.items.Values[i].Count, "We were supposed to add an item for each value in the collection");
                    }
                    Fx.Assert(results.Count == this.items.Values.Count, "We were supposed to add a sub-list for each values collection");
                    return results;
                }
            }

            public void Add(UriTemplateCompoundPathSegment segment, T value)
            {
                int index = this.items.IndexOfKey(segment);
                if (index == -1)
                {
                    Collection<CollectionItem> subItems = new Collection<CollectionItem>();
                    subItems.Add(new CollectionItem(segment, value));
                    this.items.Add(segment, subItems);
                }
                else
                {
                    Collection<CollectionItem> subItems = this.items.Values[index];
                    subItems.Add(new CollectionItem(segment, value));
                }
            }

            public T Find(UriTemplateCompoundPathSegment segment)
            {
                int index = this.items.IndexOfKey(segment);
                if (index == -1)
                {
                    return null;
                }
                Collection<CollectionItem> subItems = this.items.Values[index];
                for (int i = 0; i < subItems.Count; i++)
                {
                    if (subItems[i].Segment.IsEquivalentTo(segment, false))
                    {
                        return subItems[i].Value;
                    }
                }
                return null;
            }
            public IList<IList<T>> Find(UriTemplateLiteralPathSegment wireData)
            {
                IList<IList<T>> results = new List<IList<T>>();
                for (int i = 0; i < this.items.Values.Count; i++)
                {
                    List<T> sameOrderResults = null;
                    for (int j = 0; j < this.items.Values[i].Count; j++)
                    {
                        if (this.items.Values[i][j].Segment.IsMatch(wireData))
                        {
                            if (sameOrderResults == null)
                            {
                                sameOrderResults = new List<T>();
                            }
                            sameOrderResults.Add(this.items.Values[i][j].Value);
                        }
                    }
                    if (sameOrderResults != null)
                    {
                        results.Add(sameOrderResults);
                    }
                }
                return results;
            }

            public T GetAnyValue()
            {
                if (this.items.Values.Count > 0)
                {
                    Fx.Assert(this.items.Values[0].Count > 0, "We are not adding a sub-list unless there is at list one item");
                    return this.items.Values[0][0].Value;
                }
                else
                {
                    return null;
                }
            }

            public static bool Lookup(AscendingSortedCompoundSegmentsCollection<T> collection,
                UriTemplateLiteralPathSegment wireData, out IList<IList<T>> results)
            {
                results = collection.Find(wireData);
                return ((results != null) && (results.Count > 0));
            }

            struct CollectionItem
            {
                UriTemplateCompoundPathSegment segment;
                T value;

                public CollectionItem(UriTemplateCompoundPathSegment segment, T value)
                {
                    this.segment = segment;
                    this.value = value;
                }

                public UriTemplateCompoundPathSegment Segment
                {
                    get
                    {
                        return this.segment;
                    }
                }
                public T Value
                {
                    get
                    {
                        return this.value;
                    }
                }
            }
        }
    }
}
