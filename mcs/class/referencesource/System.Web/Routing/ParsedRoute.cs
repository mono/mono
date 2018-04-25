namespace System.Web.Routing {
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal sealed class ParsedRoute {
        public ParsedRoute(IList<PathSegment> pathSegments) {
            Debug.Assert(pathSegments != null);

            PathSegments = pathSegments;
        }

        private IList<PathSegment> PathSegments {
            get;
            set;
        }

        public BoundUrl Bind(RouteValueDictionary currentValues, RouteValueDictionary values, RouteValueDictionary defaultValues, RouteValueDictionary constraints) {
            if (currentValues == null) {
                currentValues = new RouteValueDictionary();
            }
            if (values == null) {
                values = new RouteValueDictionary();
            }
            if (defaultValues == null) {
                defaultValues = new RouteValueDictionary();
            }


            // The set of values we should be using when generating the URL in this route
            RouteValueDictionary acceptedValues = new RouteValueDictionary();

            // Keep track of which new values have been used
            HashSet<string> unusedNewValues = new HashSet<string>(values.Keys, StringComparer.OrdinalIgnoreCase);


            // Step 1: Get the list of values we're going to try to use to match and generate this URL


            // Find out which entries in the URL are valid for the URL we want to generate.
            // If the URL had ordered parameters a="1", b="2", c="3" and the new values
            // specified that b="9", then we need to invalidate everything after it. The new
            // values should then be a="1", b="9", c=<no value>.
            ForEachParameter(PathSegments, delegate(ParameterSubsegment parameterSubsegment) {
                // If it's a parameter subsegment, examine the current value to see if it matches the new value
                string parameterName = parameterSubsegment.ParameterName;

                object newParameterValue;
                bool hasNewParameterValue = values.TryGetValue(parameterName, out newParameterValue);
                if (hasNewParameterValue) {
                    unusedNewValues.Remove(parameterName);
                }

                object currentParameterValue;
                bool hasCurrentParameterValue = currentValues.TryGetValue(parameterName, out currentParameterValue);

                if (hasNewParameterValue && hasCurrentParameterValue) {
                    if (!RoutePartsEqual(currentParameterValue, newParameterValue)) {
                        // Stop copying current values when we find one that doesn't match
                        return false;
                    }
                }

                // If the parameter is a match, add it to the list of values we will use for URL generation
                if (hasNewParameterValue) {
                    if (IsRoutePartNonEmpty(newParameterValue)) {
                        acceptedValues.Add(parameterName, newParameterValue);
                    }
                }
                else {
                    if (hasCurrentParameterValue) {
                        acceptedValues.Add(parameterName, currentParameterValue);
                    }
                }
                return true;
            });

            // Add all remaining new values to the list of values we will use for URL generation
            foreach (var newValue in values) {
                if (IsRoutePartNonEmpty(newValue.Value)) {
                    if (!acceptedValues.ContainsKey(newValue.Key)) {
                        acceptedValues.Add(newValue.Key, newValue.Value);
                    }
                }
            }

            // Add all current values that aren't in the URL at all
            foreach (var currentValue in currentValues) {
                string parameterName = currentValue.Key;
                if (!acceptedValues.ContainsKey(parameterName)) {
                    ParameterSubsegment parameterSubsegment = GetParameterSubsegment(PathSegments, parameterName);
                    if (parameterSubsegment == null) {
                        acceptedValues.Add(parameterName, currentValue.Value);
                    }
                }
            }

            // Add all remaining default values from the route to the list of values we will use for URL generation
            ForEachParameter(PathSegments, delegate(ParameterSubsegment parameterSubsegment) {
                if (!acceptedValues.ContainsKey(parameterSubsegment.ParameterName)) {
                    object defaultValue;
                    if (!IsParameterRequired(parameterSubsegment, defaultValues, out defaultValue)) {
                        // Add the default value only if there isn't already a new value for it and
                        // only if it actually has a default value, which we determine based on whether
                        // the parameter value is required.
                        acceptedValues.Add(parameterSubsegment.ParameterName, defaultValue);
                    }
                }
                return true;
            });


            // All required parameters in this URL must have values from somewhere (i.e. the accepted values)
            bool hasAllRequiredValues = ForEachParameter(PathSegments, delegate(ParameterSubsegment parameterSubsegment) {
                object defaultValue;
                if (IsParameterRequired(parameterSubsegment, defaultValues, out defaultValue)) {
                    if (!acceptedValues.ContainsKey(parameterSubsegment.ParameterName)) {
                        // If the route parameter value is required that means there's
                        // no default value, so if there wasn't a new value for it
                        // either, this route won't match.
                        return false;
                    }
                }
                return true;
            });
            if (!hasAllRequiredValues) {
                return null;
            }

            // All other default values must match if they are explicitly defined in the new values
            RouteValueDictionary otherDefaultValues = new RouteValueDictionary(defaultValues);
            ForEachParameter(PathSegments, delegate(ParameterSubsegment parameterSubsegment) {
                otherDefaultValues.Remove(parameterSubsegment.ParameterName);
                return true;
            });

            foreach (var defaultValue in otherDefaultValues) {
                object value;
                if (values.TryGetValue(defaultValue.Key, out value)) {
                    unusedNewValues.Remove(defaultValue.Key);
                    if (!RoutePartsEqual(value, defaultValue.Value)) {
                        // If there is a non-parameterized value in the route and there is a
                        // new value for it and it doesn't match, this route won't match.
                        return null;
                    }
                }
            }


            // Step 2: If the route is a match generate the appropriate URL

            StringBuilder url = new StringBuilder();
            StringBuilder pendingParts = new StringBuilder();

            bool pendingPartsAreAllSafe = false;
            bool blockAllUrlAppends = false;

            for (int i = 0; i < PathSegments.Count; i++) {
                PathSegment pathSegment = PathSegments[i]; // parsedRouteUrlPart

                if (pathSegment is SeparatorPathSegment) {
                    if (pendingPartsAreAllSafe) {
                        // Accept
                        if (pendingParts.Length > 0) {
                            if (blockAllUrlAppends) {
                                return null;
                            }

                            // Append any pending literals to the URL
                            url.Append(pendingParts.ToString());
                            pendingParts.Length = 0;
                        }
                    }
                    pendingPartsAreAllSafe = false;

                    // Guard against appending multiple separators for empty segements
                    if (pendingParts.Length > 0 && pendingParts[pendingParts.Length - 1] == '/') {
                        // Dev10 676725: Route should not be matched if that causes mismatched tokens
                        // Dev11 86819: We will allow empty matches if all subsequent segments are null
                        if (blockAllUrlAppends) {
                            return null;
                        }

                        // Append any pending literals to the URL(without the trailing slash) and prevent any future appends
                        url.Append(pendingParts.ToString(0, pendingParts.Length - 1));
                        pendingParts.Length = 0;
                        blockAllUrlAppends = true;
                    }
                    else {
                        pendingParts.Append("/");
                    }
                }
                else {
                    ContentPathSegment contentPathSegment = pathSegment as ContentPathSegment;
                    if (contentPathSegment != null) {
                        // Segments are treated as all-or-none. We should never output a partial segment.
                        // If we add any subsegment of this segment to the generated URL, we have to add
                        // the complete match. For example, if the subsegment is "{p1}-{p2}.xml" and we
                        // used a value for {p1}, we have to output the entire segment up to the next "/".
                        // Otherwise we could end up with the partial segment "v1" instead of the entire
                        // segment "v1-v2.xml".
                        bool addedAnySubsegments = false;

                        foreach (PathSubsegment subsegment in contentPathSegment.Subsegments) {
                            LiteralSubsegment literalSubsegment = subsegment as LiteralSubsegment;
                            if (literalSubsegment != null) {
                                // If it's a literal we hold on to it until we are sure we need to add it
                                pendingPartsAreAllSafe = true;
                                pendingParts.Append(UrlEncode(literalSubsegment.Literal));
                            }
                            else {
                                ParameterSubsegment parameterSubsegment = subsegment as ParameterSubsegment;
                                if (parameterSubsegment != null) {
                                    if (pendingPartsAreAllSafe) {
                                        // Accept
                                        if (pendingParts.Length > 0) {
                                            if (blockAllUrlAppends) {
                                                return null;
                                            }

                                            // Append any pending literals to the URL
                                            url.Append(pendingParts.ToString());
                                            pendingParts.Length = 0;

                                            addedAnySubsegments = true;
                                        }
                                    }
                                    pendingPartsAreAllSafe = false;

                                    // If it's a parameter, get its value
                                    object acceptedParameterValue;
                                    bool hasAcceptedParameterValue = acceptedValues.TryGetValue(parameterSubsegment.ParameterName, out acceptedParameterValue);
                                    if (hasAcceptedParameterValue) {
                                        unusedNewValues.Remove(parameterSubsegment.ParameterName);
                                    }

                                    object defaultParameterValue;
                                    defaultValues.TryGetValue(parameterSubsegment.ParameterName, out defaultParameterValue);

                                    if (RoutePartsEqual(acceptedParameterValue, defaultParameterValue)) {
                                        // If the accepted value is the same as the default value, mark it as pending since
                                        // we won't necessarily add it to the URL we generate.
                                        pendingParts.Append(UrlEncode(Convert.ToString(acceptedParameterValue, CultureInfo.InvariantCulture)));
                                    }
                                    else {
                                        if (blockAllUrlAppends) {
                                            return null;
                                        }

                                        // Add the new part to the URL as well as any pending parts
                                        if (pendingParts.Length > 0) {
                                            // Append any pending literals to the URL
                                            url.Append(pendingParts.ToString());
                                            pendingParts.Length = 0;
                                        }
                                        url.Append(UrlEncode(Convert.ToString(acceptedParameterValue, CultureInfo.InvariantCulture)));

                                        addedAnySubsegments = true;
                                    }
                                }
                                else {
                                    Debug.Fail("Invalid path subsegment type");
                                }
                            }
                        }

                        if (addedAnySubsegments) {
                            // See comment above about why we add the pending parts
                            if (pendingParts.Length > 0) {
                                if (blockAllUrlAppends) {
                                    return null;
                                }

                                // Append any pending literals to the URL
                                url.Append(pendingParts.ToString());
                                pendingParts.Length = 0;
                            }
                        }
                    }
                    else {
                        Debug.Fail("Invalid path segment type");
                    }
                }
            }

            if (pendingPartsAreAllSafe) {
                // Accept
                if (pendingParts.Length > 0) {
                    if (blockAllUrlAppends) {
                        return null;
                    }

                    // Append any pending literals to the URL
                    url.Append(pendingParts.ToString());
                }
            }

            // Process constraints keys
            if (constraints != null) {
                // If there are any constraints, mark all the keys as being used so that we don't
                // generate query string items for custom constraints that don't appear as parameters
                // in the URL format.
                foreach (var constraintsItem in constraints) {
                    unusedNewValues.Remove(constraintsItem.Key);
                }
            }


            // Add remaining new values as query string parameters to the URL
            if (unusedNewValues.Count > 0) {
                // Generate the query string
                bool firstParam = true;
                foreach (string unusedNewValue in unusedNewValues) {
                    object value;
                    if (acceptedValues.TryGetValue(unusedNewValue, out value)) {
                        url.Append(firstParam ? '?' : '&');
                        firstParam = false;
                        url.Append(Uri.EscapeDataString(unusedNewValue));
                        url.Append('=');
                        url.Append(Uri.EscapeDataString(Convert.ToString(value, CultureInfo.InvariantCulture)));
                    }
                }
            }

            return new BoundUrl {
                Url = url.ToString(),
                Values = acceptedValues
            };
        }

        private static string EscapeReservedCharacters(Match m) {
            return "%" + Convert.ToUInt16(m.Value[0]).ToString("x2", CultureInfo.InvariantCulture);
        }

        private static bool ForEachParameter(IList<PathSegment> pathSegments, Func<ParameterSubsegment, bool> action) {
            for (int i = 0; i < pathSegments.Count; i++) {
                PathSegment pathSegment = pathSegments[i];

                if (pathSegment is SeparatorPathSegment) {
                    // We only care about parameter subsegments, so skip this
                    continue;
                }
                else {
                    ContentPathSegment contentPathSegment = pathSegment as ContentPathSegment;
                    if (contentPathSegment != null) {
                        foreach (PathSubsegment subsegment in contentPathSegment.Subsegments) {
                            LiteralSubsegment literalSubsegment = subsegment as LiteralSubsegment;
                            if (literalSubsegment != null) {
                                // We only care about parameter subsegments, so skip this
                                continue;
                            }
                            else {
                                ParameterSubsegment parameterSubsegment = subsegment as ParameterSubsegment;
                                if (parameterSubsegment != null) {
                                    if (!action(parameterSubsegment)) {
                                        return false;
                                    }
                                }
                                else {
                                    Debug.Fail("Invalid path subsegment type");
                                }
                            }
                        }
                    }
                    else {
                        Debug.Fail("Invalid path segment type");
                    }
                }
            }

            return true;
        }

        private static ParameterSubsegment GetParameterSubsegment(IList<PathSegment> pathSegments, string parameterName) {
            ParameterSubsegment foundParameterSubsegment = null;

            bool continueProcessing = ForEachParameter(pathSegments, delegate(ParameterSubsegment parameterSubsegment) {
                if (String.Equals(parameterName, parameterSubsegment.ParameterName, StringComparison.OrdinalIgnoreCase)) {
                    foundParameterSubsegment = parameterSubsegment;
                    return false;
                }
                else {
                    return true;
                }
            });

            return foundParameterSubsegment;
        }

        private static bool IsParameterRequired(ParameterSubsegment parameterSubsegment, RouteValueDictionary defaultValues, out object defaultValue) {
            if (parameterSubsegment.IsCatchAll) {
                defaultValue = null;
                return false;
            }

            return !defaultValues.TryGetValue(parameterSubsegment.ParameterName, out defaultValue);
        }

        private static bool IsRoutePartNonEmpty(object routePart) {
            string routePartString = routePart as string;
            if (routePartString != null) {
                return (routePartString.Length > 0);
            }
            return (routePart != null);
        }

        public RouteValueDictionary Match(string virtualPath, RouteValueDictionary defaultValues) {
            IList<string> requestPathSegments = RouteParser.SplitUrlToPathSegmentStrings(virtualPath);

            if (defaultValues == null) {
                defaultValues = new RouteValueDictionary();
            }

            RouteValueDictionary matchedValues = new RouteValueDictionary();

            // This flag gets set once all the data in the URL has been parsed through, but
            // the route we're trying to match against still has more parts. At this point
            // we'll only continue matching separator characters and parameters that have
            // default values.
            bool ranOutOfStuffToParse = false;

            // This value gets set once we start processing a catchall parameter (if there is one
            // at all). Once we set this value we consume all remaining parts of the URL into its
            // parameter value.
            bool usedCatchAllParameter = false;

            for (int i = 0; i < PathSegments.Count; i++) {
                PathSegment pathSegment = PathSegments[i];

                if (requestPathSegments.Count <= i) {
                    ranOutOfStuffToParse = true;
                }

                string requestPathSegment = ranOutOfStuffToParse ? null : requestPathSegments[i];

                if (pathSegment is SeparatorPathSegment) {
                    if (ranOutOfStuffToParse) {
                        // If we're trying to match a separator in the route but there's no more content, that's OK
                    }
                    else {
                        if (!String.Equals(requestPathSegment, "/", StringComparison.Ordinal)) {
                            return null;
                        }
                    }
                }
                else {
                    ContentPathSegment contentPathSegment = pathSegment as ContentPathSegment;
                    if (contentPathSegment != null) {
                        if (contentPathSegment.IsCatchAll) {
                            Debug.Assert(i == (PathSegments.Count - 1), "If we're processing a catch-all, we should be on the last route segment.");
                            MatchCatchAll(contentPathSegment, requestPathSegments.Skip(i), defaultValues, matchedValues);
                            usedCatchAllParameter = true;
                        }
                        else {
                            if (!MatchContentPathSegment(contentPathSegment, requestPathSegment, defaultValues, matchedValues)) {
                                return null;
                            }
                        }
                    }
                    else {
                        Debug.Fail("Invalid path segment type");
                    }
                }
            }

            if (!usedCatchAllParameter) {
                if (PathSegments.Count < requestPathSegments.Count) {
                    // If we've already gone through all the parts defined in the route but the URL
                    // still contains more content, check that the remaining content is all separators.
                    for (int i = PathSegments.Count; i < requestPathSegments.Count; i++) {
                        if (!RouteParser.IsSeparator(requestPathSegments[i])) {
                            return null;
                        }
                    }
                }
            }

            // Copy all remaining default values to the route data
            if (defaultValues != null) {
                foreach (var defaultValue in defaultValues) {
                    if (!matchedValues.ContainsKey(defaultValue.Key)) {
                        matchedValues.Add(defaultValue.Key, defaultValue.Value);
                    }
                }
            }

            return matchedValues;
        }

        private void MatchCatchAll(ContentPathSegment contentPathSegment, IEnumerable<string> remainingRequestSegments, RouteValueDictionary defaultValues, RouteValueDictionary matchedValues) {
            string remainingRequest = String.Join(String.Empty, remainingRequestSegments.ToArray());

            ParameterSubsegment catchAllSegment = contentPathSegment.Subsegments[0] as ParameterSubsegment;

            object catchAllValue;

            if (remainingRequest.Length > 0) {
                catchAllValue = remainingRequest;
            }
            else {
                defaultValues.TryGetValue(catchAllSegment.ParameterName, out catchAllValue);
            }
            matchedValues.Add(catchAllSegment.ParameterName, catchAllValue);
        }

        private bool MatchContentPathSegment(ContentPathSegment routeSegment, string requestPathSegment, RouteValueDictionary defaultValues, RouteValueDictionary matchedValues) {
            if (String.IsNullOrEmpty(requestPathSegment)) {
                // If there's no data to parse, we must have exactly one parameter segment and no other segments - otherwise no match

                if (routeSegment.Subsegments.Count > 1) {
                    return false;
                }

                ParameterSubsegment parameterSubsegment = routeSegment.Subsegments[0] as ParameterSubsegment;
                if (parameterSubsegment == null) {
                    return false;
                }

                // We must have a default value since there's no value in the request URL
                object parameterValue;
                if (defaultValues.TryGetValue(parameterSubsegment.ParameterName, out parameterValue)) {
                    // If there's a default value for this parameter, use that default value
                    matchedValues.Add(parameterSubsegment.ParameterName, parameterValue);
                    return true;
                }
                else {
                    // If there's no default value, this segment doesn't match
                    return false;
                }
            }


            // Find last literal segment and get its last index in the string

            int lastIndex = requestPathSegment.Length;
            int indexOfLastSegmentUsed = routeSegment.Subsegments.Count - 1;

            ParameterSubsegment parameterNeedsValue = null; // Keeps track of a parameter segment that is pending a value
            LiteralSubsegment lastLiteral = null; // Keeps track of the left-most literal we've encountered

            while (indexOfLastSegmentUsed >= 0) {
                int newLastIndex = lastIndex;

                ParameterSubsegment parameterSubsegment = routeSegment.Subsegments[indexOfLastSegmentUsed] as ParameterSubsegment;
                if (parameterSubsegment != null) {
                    // Hold on to the parameter so that we can fill it in when we locate the next literal
                    parameterNeedsValue = parameterSubsegment;
                }
                else {
                    LiteralSubsegment literalSubsegment = routeSegment.Subsegments[indexOfLastSegmentUsed] as LiteralSubsegment;
                    if (literalSubsegment != null) {
                        lastLiteral = literalSubsegment;

                        int startIndex = lastIndex - 1;
                        // If we have a pending parameter subsegment, we must leave at least one character for that
                        if (parameterNeedsValue != null) {
                            startIndex--;
                        }

                        if (startIndex < 0) {
                            return false;
                        }

                        int indexOfLiteral = requestPathSegment.LastIndexOf(literalSubsegment.Literal, startIndex, StringComparison.OrdinalIgnoreCase);
                        if (indexOfLiteral == -1) {
                            // If we couldn't find this literal index, this segment cannot match
                            return false;
                        }

                        // If the first subsegment is a literal, it must match at the right-most extent of the request URL.
                        // Without this check if your route had "/Foo/" we'd match the request URL "/somethingFoo/".
                        // This check is related to the check we do at the very end of this function.
                        if (indexOfLastSegmentUsed == (routeSegment.Subsegments.Count - 1)) {
                            if ((indexOfLiteral + literalSubsegment.Literal.Length) != requestPathSegment.Length) {
                                return false;
                            }
                        }

                        newLastIndex = indexOfLiteral;
                    }
                    else {
                        Debug.Fail("Invalid path segment type");
                    }
                }

                if ((parameterNeedsValue != null) && (((lastLiteral != null) && (parameterSubsegment == null)) || (indexOfLastSegmentUsed == 0))) {
                    // If we have a pending parameter that needs a value, grab that value

                    int parameterStartIndex;
                    int parameterTextLength;

                    if (lastLiteral == null) {
                        if (indexOfLastSegmentUsed == 0) {
                            parameterStartIndex = 0;
                        }
                        else {
                            parameterStartIndex = newLastIndex;
                            Debug.Fail("indexOfLastSegementUsed should always be 0 from the check above");
                        }
                        parameterTextLength = lastIndex;
                    }
                    else {
                        // If we're getting a value for a parameter that is somewhere in the middle of the segment
                        if ((indexOfLastSegmentUsed == 0) && (parameterSubsegment != null)) {
                            parameterStartIndex = 0;
                            parameterTextLength = lastIndex;
                        }
                        else {
                            parameterStartIndex = newLastIndex + lastLiteral.Literal.Length;
                            parameterTextLength = lastIndex - parameterStartIndex;
                        }
                    }

                    string parameterValueString = requestPathSegment.Substring(parameterStartIndex, parameterTextLength);

                    if (String.IsNullOrEmpty(parameterValueString)) {
                        // If we're here that means we have a segment that contains multiple sub-segments.
                        // For these segments all parameters must have non-empty values. If the parameter
                        // has an empty value it's not a match.
                        return false;
                    }
                    else {
                        // If there's a value in the segment for this parameter, use the subsegment value
                        matchedValues.Add(parameterNeedsValue.ParameterName, parameterValueString);
                    }

                    parameterNeedsValue = null;
                    lastLiteral = null;
                }

                lastIndex = newLastIndex;
                indexOfLastSegmentUsed--;
            }

            // If the last subsegment is a parameter, it's OK that we didn't parse all the way to the left extent of
            // the string since the parameter will have consumed all the remaining text anyway. If the last subsegment
            // is a literal then we *must* have consumed the entire text in that literal. Otherwise we end up matching
            // the route "Foo" to the request URL "somethingFoo". Thus we have to check that we parsed the *entire*
            // request URL in order for it to be a match.
            // This check is related to the check we do earlier in this function for LiteralSubsegments.
            return (lastIndex == 0) || (routeSegment.Subsegments[0] is ParameterSubsegment);
        }

        private static bool RoutePartsEqual(object a, object b) {
            string sa = a as string;
            string sb = b as string;
            if (sa != null && sb != null) {
                // For strings do a case-insensitive comparison
                return String.Equals(sa, sb, StringComparison.OrdinalIgnoreCase);
            }
            else {
                if (a != null && b != null) {
                    // Explicitly call .Equals() in case it is overridden in the type
                    return a.Equals(b);
                }
                else {
                    // At least one of them is null. Return true if they both are
                    return a == b;
                }
            }
        }

        // Dev10 601636 Work around Uri.EscapeUriString not encoding #,&
        private static string UrlEncode(string str) {
            string escape = Uri.EscapeUriString(str);
            return Regex.Replace(escape, "([#;?:@&=+$,])", new MatchEvaluator(EscapeReservedCharacters));
        }
    }
}
