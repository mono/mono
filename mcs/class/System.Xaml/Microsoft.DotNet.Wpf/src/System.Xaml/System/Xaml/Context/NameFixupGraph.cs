// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.MS.Impl;

namespace MS.Internal.Xaml.Context
{
    // Graph of unresolved forward references, and the objects that depend on them.
    // The nodes are objects and names. The edges (NameFixupTokens) are dependencies from an object to 
    // a set of unresolved names, or from an object to another object that has unresolved dependencies.
    internal class NameFixupGraph
    {
        // Node -> out-edges (other objects the parent is dependent on)
        Dictionary<object, FrugalObjectList<NameFixupToken>> _dependenciesByParentObject;

        // Node -> in-edge (other object that is dependent on this child)
        Dictionary<object, NameFixupToken> _dependenciesByChildObject;

        // Node -> in-edges (other objects that are dependent on this name)
        Dictionary<string, FrugalObjectList<NameFixupToken>> _dependenciesByName;

        // Queue of tokens whose dependencies have been resolved, and are awaiting processing
        Queue<NameFixupToken> _resolvedTokensPendingProcessing;

        // Token for a pending call to ProvideValue on the root object. Can't store this in
        // _dependenciesByParentObject because it has no parent.
        NameFixupToken _deferredRootProvideValue;

        // At the end of the parse, we start running reparses on partially initialized objects,
        // and remove those dependencies. But we still want to be able to inform MEs/TCs that
        // the named objects they're getting aren't actually fully initialized. So we save this list
        // of incompletely initialized objects at the point we start completing references.
        HashSet <object> _uninitializedObjectsAtParseEnd;

        public NameFixupGraph()
        {
            var referenceComparer = System.Xaml.Schema.ReferenceEqualityComparer<object>.Singleton;
            _dependenciesByChildObject = new Dictionary<object, NameFixupToken>(referenceComparer);
            _dependenciesByName = new Dictionary<string, FrugalObjectList<NameFixupToken>>(StringComparer.Ordinal);
            _dependenciesByParentObject = new Dictionary<object, FrugalObjectList<NameFixupToken>>(referenceComparer);
            _resolvedTokensPendingProcessing = new Queue<NameFixupToken>();
            _uninitializedObjectsAtParseEnd = new HashSet<object>(referenceComparer);
        }

        // Add an edge to the graph. We need to look up edges in both directions, so each edge is
        // stored in two dictionaries.
        public void AddDependency(NameFixupToken fixupToken)
        {
            // Need to special case a deferred ProvideValue at the root, because it has no parent
            if (fixupToken.Target.Property == null)
            {
                Debug.Assert(fixupToken.Target.Instance == null && 
                    fixupToken.Target.InstanceType == null &&
                    fixupToken.FixupType == FixupType.MarkupExtensionFirstRun);
                Debug.Assert(_deferredRootProvideValue == null);
                _deferredRootProvideValue = fixupToken;
                return;
            }

            object parentObject = fixupToken.Target.Instance;
            // References aren't allowed in non-instantiating directives, except for:
            // - Initialization, in which case FixupTarget.Instance is the object whose property the
            //   initialized object will be assigned to; and
            // - Key, in which case the FixupTarget.Instance is the dictionary
            Debug.Assert(parentObject != null);

            AddToMultiDict(_dependenciesByParentObject, parentObject, fixupToken);

            if (fixupToken.ReferencedObject != null)
            {
                Debug.Assert(fixupToken.FixupType == FixupType.UnresolvedChildren ||
                    fixupToken.FixupType == FixupType.MarkupExtensionFirstRun);
                // These fixups are only used for the immediate parent of the object, so there can
                // only be one per child instance
                Debug.Assert(!_dependenciesByChildObject.ContainsKey(fixupToken.ReferencedObject));

                _dependenciesByChildObject.Add(fixupToken.ReferencedObject, fixupToken);
            }
            else
            {
                Debug.Assert(fixupToken.FixupType != FixupType.UnresolvedChildren &&
                    fixupToken.FixupType != FixupType.MarkupExtensionFirstRun);

                foreach (string name in fixupToken.NeededNames)
                {
                    AddToMultiDict(_dependenciesByName, name, fixupToken);
                }   
            }
        }

        public bool HasUnresolvedChildren(object parent)
        {
            if (parent == null)
            {
                return false;
            }
            return _dependenciesByParentObject.ContainsKey(parent);
        }

        public bool HasUnresolvedOrPendingChildren(object instance)
        {
            if (HasUnresolvedChildren(instance))
            {
                return true;
            }
            foreach (NameFixupToken pendingToken in _resolvedTokensPendingProcessing)
            {
                if (pendingToken.Target.Instance == instance)
                {
                    return true;
                }
            }
            return false;
        }

        public bool WasUninitializedAtEndOfParse(object instance)
        {
            return _uninitializedObjectsAtParseEnd.Contains(instance);
        }

        // Finds the names that this object's subtree is blocked on.
        public void GetDependentNames(object instance, List<string> result)
        {
            // We're only interested in the immediate subtree, not named-references to other subtrees
            // that might exist but not be fully initialized. So we only follow UnresolvedChildren and
            // MarkupExtensionFirstRun edges, which means there is no risk of cycles
            FrugalObjectList<NameFixupToken> dependencies;
            if (!_dependenciesByParentObject.TryGetValue(instance, out dependencies))
            {
                return;
            }
            for (int i = 0; i < dependencies.Count; i++)
            {
                NameFixupToken token = dependencies[i];
                if (token.FixupType == FixupType.MarkupExtensionFirstRun ||
                    token.FixupType == FixupType.UnresolvedChildren)
                {
                    GetDependentNames(token.ReferencedObject, result);
                }
                else if (token.NeededNames != null)
                {
                    foreach (string name in token.NeededNames)
                    {
                        if (!result.Contains(name))
                        {
                            result.Add(name);
                        }
                    }
                }
            }
        }

        // Remove a resolved dependency from the graph.
        // Enqueues all removed edges so that the ObjectWriter can process the dependents
        // (rerun converters, apply simple fixups, call EndInit on parent ojects, etc).
        public void ResolveDependenciesTo(object instance, string name)
        {
            // Remove any dependency on this instance
            NameFixupToken token = null;
            if (instance != null)
            {
                if (_dependenciesByChildObject.TryGetValue(instance, out token))
                {
                    _dependenciesByChildObject.Remove(instance);
                    RemoveTokenByParent(token);
                    _resolvedTokensPendingProcessing.Enqueue(token);
                }
            }

            // Remove any dependencies on this name, and return any tokens whose dependencies
            // have all been resolved.
            FrugalObjectList<NameFixupToken> nameDependencies;
            if (name != null && _dependenciesByName.TryGetValue(name, out nameDependencies))
            {
                int i = 0;
                while (i < nameDependencies.Count)
                {
                    token = nameDependencies[i];
                    
                    // The same name can occur in multiple namescopes, so we need to make sure that
                    // this named object is visible in the scope of the token.
                    object resolvedName = token.ResolveName(name);
                    if (instance != resolvedName)
                    {
                        i++;
                        continue;
                    }

                    if (token.CanAssignDirectly)
                    {
                        // For simple fixups, we need to return the resolved object
                        token.ReferencedObject = instance;
                    }
                    token.NeededNames.Remove(name);
                    nameDependencies.RemoveAt(i);
                    if (nameDependencies.Count == 0)
                    {
                        _dependenciesByName.Remove(name);
                    }
                    if (token.NeededNames.Count == 0)
                    {
                        RemoveTokenByParent(token);
                        _resolvedTokensPendingProcessing.Enqueue(token);
                    }
                }
            }
        }

        public bool HasResolvedTokensPendingProcessing
        {
            get { return _resolvedTokensPendingProcessing.Count > 0; }
        }

        public NameFixupToken GetNextResolvedTokenPendingProcessing()
        {
            return _resolvedTokensPendingProcessing.Dequeue();
        }

        // ObjectWriter calls this whenever an object that has pending fixups goes off the stack.
        public void IsOffTheStack(object instance, string name, int lineNumber, int linePosition)
        {
            FrugalObjectList<NameFixupToken> dependencies;
            if (_dependenciesByParentObject.TryGetValue(instance, out dependencies))
            {
                for (int i = 0; i < dependencies.Count; i++)
                {
                    dependencies[i].Target.InstanceIsOnTheStack = false;
                    dependencies[i].Target.InstanceName = name;
                    dependencies[i].Target.EndInstanceLineNumber = lineNumber;
                    dependencies[i].Target.EndInstanceLinePosition = linePosition;
                }
            }
        }

        public void AddEndOfParseDependency(object childThatHasUnresolvedChildren, FixupTarget parentObject)
        {
            NameFixupToken token = new NameFixupToken();
            token.Target = parentObject;
            token.FixupType = FixupType.UnresolvedChildren;
            token.ReferencedObject = childThatHasUnresolvedChildren;
            AddToMultiDict(_dependenciesByParentObject, parentObject.Instance, token);
            // We don't add to the _dependenciesByChildObject, because at end-of-parse, a single
            // child object can be a dependency of multiple parents
        }

        // At end of parse, removes and returns all remaining simple fixups, whether or not they
        // are resolved
        public IEnumerable<NameFixupToken> GetRemainingSimpleFixups()
        {
            foreach (object key in _dependenciesByParentObject.Keys)
            {
                _uninitializedObjectsAtParseEnd.Add(key);
            }

            List<string> names = new List<string>(_dependenciesByName.Keys);
            foreach (string name in names)
            {
                FrugalObjectList<NameFixupToken> dependencies = _dependenciesByName[name];
                int i = 0;
                while (i < dependencies.Count)
                {
                    NameFixupToken token = dependencies[i];
                    if (!token.CanAssignDirectly)
                    {
                        i++;
                        continue;
                    }
                    dependencies.RemoveAt(i);
                    if (dependencies.Count == 0)
                    {
                        _dependenciesByName.Remove(name);
                    }
                    RemoveTokenByParent(token);
                    yield return token;
                }
            }
        }

        // At end of parse, removes and returns all remaining reparse fixups, whether or not they
        // are resolved. Assumes that all simple fixups have already been removed.
        public IEnumerable<NameFixupToken> GetRemainingReparses()
        {
            List<object> parentObjs = new List<object>(_dependenciesByParentObject.Keys);
            foreach (object parentObj in parentObjs)
            {
                FrugalObjectList<NameFixupToken> dependencies = _dependenciesByParentObject[parentObj];
                int i = 0;
                while (i < dependencies.Count)
                {
                    NameFixupToken token = dependencies[i];
                    if (token.FixupType == FixupType.MarkupExtensionFirstRun ||
                        token.FixupType == FixupType.UnresolvedChildren)
                    {
                        i++;
                        continue;
                    }

                    // Remove this token from the _dependenciesByParentObject dictionary
                    dependencies.RemoveAt(i);
                    if (dependencies.Count == 0)
                    {
                        _dependenciesByParentObject.Remove(parentObj);
                    }

                    // Remove this token from the _dependenciesByName dictionary
                    foreach (string name in token.NeededNames)
                    {
                        FrugalObjectList<NameFixupToken> nameDependencies = _dependenciesByName[name];
                        if (nameDependencies.Count == 1)
                        {
                            nameDependencies.Remove(token);
                        }
                        else
                        {
                            _dependenciesByName.Remove(name);
                        }
                    }

                    yield return token;
                }
            }
        }

        // At end of parse, removes and returns all remaining MarkupExtensionFirstRun and UnresolvedChildren
        // tokens, even if they are not fully initialized. Assumes that all simple fixups and reparses have
        // already been removed.
        public IEnumerable<NameFixupToken> GetRemainingObjectDependencies()
        {
            // We'd like to return dependencies in a topologically sorted order, but the graph is
            // not acylic. (If it were, all references would have been resolved during the regular parse.)
            // However, we don't allow ProvideValue cycles. So find a MarkupExtension that doesn't have
            // dependencies on any other MarkupExtension.

            // Note: at this point we can't use _dependenciesByChildObject for general traversal,
            // because it's not updated by AddEndOfParseDependency. However, AddEndOfParseDependency
            // doesn't add MarkupExtension edges, so we can still use _dependenciesByChildObject for that.
            List<NameFixupToken> markupExtensionTokens = new List<NameFixupToken>();
            foreach (NameFixupToken curToken in _dependenciesByChildObject.Values)
            {
                if (curToken.FixupType == FixupType.MarkupExtensionFirstRun)
                {
                    markupExtensionTokens.Add(curToken);
                }
            }
            while (markupExtensionTokens.Count > 0)
            {
                bool found = false;
                int i = 0;
                while (i < markupExtensionTokens.Count)
                {
                    NameFixupToken meToken = markupExtensionTokens[i];
                    List<NameFixupToken> dependencies = new List<NameFixupToken>();
                    if (!FindDependencies(meToken, dependencies))
                    {
                        i++;
                        continue;
                    }
                    // Iterate the list in backwards order, so we return the deepest first
                    for (int j = dependencies.Count - 1; j >= 0; j--)
                    {
                        NameFixupToken token = dependencies[j];
                        RemoveTokenByParent(token);
                        yield return token;
                    }
                    found = true;
                    markupExtensionTokens.RemoveAt(i);
                }
                if (!found)
                {
                    // We have MEs left, but they all have dependencies on other MEs.
                    // That means we have a cycle.
                    ThrowProvideValueCycle(markupExtensionTokens);
                }
            }

            // For the remaining EndInits, we pick an arbitrary point and return a DFS of its dependencies
            while (_dependenciesByParentObject.Count > 0)
            {
                FrugalObjectList<NameFixupToken> startNodeOutEdges = null;
                foreach (FrugalObjectList<NameFixupToken> list in _dependenciesByParentObject.Values)
                {
                    startNodeOutEdges = list;
                    break;
                }
                for (int i = 0; i < startNodeOutEdges.Count; i++)
                {
                    List<NameFixupToken> dependencies = new List<NameFixupToken>();
                    FindDependencies(startNodeOutEdges[i], dependencies);
                    // Iterate the list in backwards order, so we return the deepest first
                    for (int j = dependencies.Count - 1; j >= 0; j--)
                    {
                        NameFixupToken token = dependencies[j];
                        RemoveTokenByParent(token);
                        yield return token;
                    }
                }
            }

            // Finally, if there was a deferred ProvideValue at the root, return it
            if (_deferredRootProvideValue != null)
            {
                yield return _deferredRootProvideValue;
            }
        }

        // Depth-first traversal of the graph starting at a given edge. Ignores edges that would cause cycles.
        // Returns true if the dependency list is complete, false if we aborted because we found an ME.
        private bool FindDependencies(NameFixupToken inEdge, List<NameFixupToken> alreadyTraversed)
        {
            if (alreadyTraversed.Contains(inEdge))
            {
                // Cycle, skip it
                return true;
            }
            alreadyTraversed.Add(inEdge);
            FrugalObjectList<NameFixupToken> outEdges;
            if (inEdge.ReferencedObject == null || 
                !_dependenciesByParentObject.TryGetValue(inEdge.ReferencedObject, out outEdges))
            {
                // No dependencies, we're done with this subgraph
                return true;
            }
            for (int i = 0; i < outEdges.Count; i++)
            {
                NameFixupToken outEdge = outEdges[i];
                if (outEdge.FixupType == FixupType.MarkupExtensionFirstRun)
                {
                    return false;
                }
                Debug.Assert(outEdge.FixupType == FixupType.UnresolvedChildren);
                if (!FindDependencies(outEdge, alreadyTraversed))
                {
                    return false;
                }
            }
            return true;
        }

        private void RemoveTokenByParent(NameFixupToken token)
        {
            object parentInstance = token.Target.Instance;
            FrugalObjectList<NameFixupToken> parentDependencies = _dependenciesByParentObject[parentInstance];
            Debug.Assert(parentDependencies.Contains(token));

            if (parentDependencies.Count == 1)
            {
                _dependenciesByParentObject.Remove(parentInstance);
            }
            else
            {
                parentDependencies.Remove(token);
            }
        }

        private static void AddToMultiDict<TKey>(Dictionary<TKey, FrugalObjectList<NameFixupToken>> dict,
            TKey key, NameFixupToken value)
        {
            FrugalObjectList<NameFixupToken> tokenList;
            if (!dict.TryGetValue(key, out tokenList))
            {
                tokenList = new FrugalObjectList<NameFixupToken>(1);
                dict.Add(key, tokenList);
            }
            tokenList.Add(value);
        }

        private static void ThrowProvideValueCycle(IEnumerable<NameFixupToken> markupExtensionTokens)
        {
            StringBuilder exceptionMessage = new StringBuilder();
            exceptionMessage.Append(SR.Get(SRID.ProvideValueCycle));
            foreach (NameFixupToken token in markupExtensionTokens)
            {
                exceptionMessage.AppendLine();
                string meName = token.ReferencedObject.ToString();
                if (token.LineNumber != 0)
                {
                    if (token.LinePosition != 0)
                    {
                        exceptionMessage.Append(SR.Get(SRID.LineNumberAndPosition, meName, token.LineNumber, token.LinePosition));
                    }
                    else
                    {
                        exceptionMessage.Append(SR.Get(SRID.LineNumberOnly, meName, token.LineNumber));
                    }
                }
                else
                {
                    exceptionMessage.Append(meName);
                }
            }
            throw new XamlObjectWriterException(exceptionMessage.ToString());
        }
    }
}
