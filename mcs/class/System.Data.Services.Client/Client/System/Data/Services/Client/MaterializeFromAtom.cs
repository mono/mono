//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    #region Namespaces.

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Text;
    using System.Linq.Expressions;

    #endregion Namespaces.

    internal class MaterializeAtom : IDisposable, IEnumerable, IEnumerator
    {
        internal readonly MergeOption MergeOptionValue;

        #region Private fields.

        private const long CountStateInitial = -1;

        private const long CountStateFailure = -2;

        private readonly bool ignoreMissingProperties;

        private readonly DataServiceContext context;

        private readonly Type elementType;

        private readonly bool expectingSingleValue;

        private readonly AtomMaterializer materializer;

        private readonly AtomParser parser;

        private XmlReader reader;

        private object current;

        private bool calledGetEnumerator;

        private long countValue;

        private bool moved;

#if DEBUG && !ASTORIA_LIGHT
        private System.IO.TextWriter writer = new System.IO.StringWriter(System.Globalization.CultureInfo.InvariantCulture);
#else
#pragma warning disable 649
        private System.IO.TextWriter writer;
#pragma warning restore 649
#endif

        #endregion Private fields.

        internal MaterializeAtom(DataServiceContext context, XmlReader reader, QueryComponents queryComponents, ProjectionPlan plan, MergeOption mergeOption)
        {
            Debug.Assert(queryComponents != null, "queryComponents != null");

            this.context = context;
            this.elementType = queryComponents.LastSegmentType;
            this.MergeOptionValue = mergeOption;
            this.ignoreMissingProperties = context.IgnoreMissingProperties;
            this.reader = (reader == null) ? null : new System.Data.Services.Client.Xml.XmlAtomErrorReader(reader);
            this.countValue = CountStateInitial;
            this.expectingSingleValue = ClientConvert.IsKnownNullableType(elementType);

            Debug.Assert(reader != null, "Materializer reader is null! Did you mean to use Materializer.ResultsWrapper/EmptyResults?");

            reader.Settings.NameTable.Add(context.DataNamespace);

            string typeScheme = this.context.TypeScheme.OriginalString;
            this.parser = new AtomParser(this.reader, AtomParser.XElementBuilderCallback, typeScheme, context.DataNamespace);
            AtomMaterializerLog log = new AtomMaterializerLog(this.context, mergeOption);
            Type implementationType;
            Type materializerType = GetTypeForMaterializer(this.expectingSingleValue, this.elementType, out implementationType);
            this.materializer = new AtomMaterializer(parser, context, materializerType, this.ignoreMissingProperties, mergeOption, log, this.MaterializedObjectCallback, queryComponents, plan);
        }

        private void MaterializedObjectCallback(object tag, object entity)
        {
            Debug.Assert(tag != null, "tag != null");
            Debug.Assert(entity != null, "entity != null");

            XElement data = (XElement)tag;
            if (this.context.HasReadingEntityHandlers)
            {
                XmlUtil.RemoveDuplicateNamespaceAttributes(data);
                this.context.FireReadingEntityEvent(entity, data);
            }
        }

        private MaterializeAtom()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private MaterializeAtom(DataServiceContext context, XmlReader reader, Type type, MergeOption mergeOption)
            : this(context, reader, new QueryComponents(null, Util.DataServiceVersionEmpty, type, null, null), null, mergeOption)
        {
        }

        #region Current

        public object Current
        {
            get
            {
                object currentValue = this.current;
                return currentValue;
            }
        }

        #endregion

        internal static MaterializeAtom EmptyResults
        {
            get
            {
                return new ResultsWrapper(null, null);
            }
        }

        internal bool IsEmptyResults
        {
            get { return this.reader == null; }
        }

        internal DataServiceContext Context
        {
            get { return this.context; }
        }

        #region IDisposable
        public void Dispose()
        {
            this.current = null;

            if (null != this.reader)
            {
                ((IDisposable)this.reader).Dispose();
            }

            if (null != this.writer)
            {
                this.writer.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEnumerable
        public virtual IEnumerator GetEnumerator()
        {
            this.CheckGetEnumerator();
            return this;
        }
        #endregion

        private static Type GetTypeForMaterializer(bool expectingSingleValue, Type elementType, out Type implementationType)
        {
            if (!expectingSingleValue && typeof(IEnumerable).IsAssignableFrom(elementType))
            {
                implementationType = ClientType.GetImplementationType(elementType, typeof(ICollection<>));
                if (implementationType != null)
                {
                    Type expectedType = implementationType.GetGenericArguments()[0];                    return expectedType;
                }
            }

            implementationType = null;
            return elementType;
        }

        public bool MoveNext()
        {
            bool applying = this.context.ApplyingChanges;
            try
            {
                this.context.ApplyingChanges = true;
                return this.MoveNextInternal();
            }
            finally
            {
                this.context.ApplyingChanges = applying;
            }
        }

        private bool MoveNextInternal()
        {
            if (this.reader == null)
            {
                Debug.Assert(this.current == null, "this.current == null -- otherwise this.reader should have some value.");
                return false;
            }

            this.current = null;
            this.materializer.Log.Clear();

            bool result = false;
            Type implementationType;
            GetTypeForMaterializer(this.expectingSingleValue, this.elementType, out implementationType);
            if (implementationType != null)
            {
                if (this.moved)
                {
                    return false;
                }

                Type expectedType = implementationType.GetGenericArguments()[0];                implementationType = this.elementType;
                if (implementationType.IsInterface)
                {
                    implementationType = typeof(System.Collections.ObjectModel.Collection<>).MakeGenericType(expectedType);
                }

                IList list = (IList)Activator.CreateInstance(implementationType);

                while (this.materializer.Read())
                {
                    this.moved = true;
                    list.Add(this.materializer.CurrentValue);
                }

                this.current = list;
                result = true;
            }

            if (null == this.current)
            {
                if (this.expectingSingleValue && this.moved)
                {
                    result = false;
                }
                else
                {
                    result = this.materializer.Read();
                    if (result)
                    {
                        this.current = this.materializer.CurrentValue;
                    }

                    this.moved = true;
                }
            }

            this.materializer.Log.ApplyToContext();

            return result;
        }

        void System.Collections.IEnumerator.Reset()
        {
            throw Error.NotSupported();
        }

        internal static MaterializeAtom CreateWrapper(IEnumerable results)
        {
            return new ResultsWrapper(results, null);
        }

        internal static MaterializeAtom CreateWrapper(IEnumerable results, DataServiceQueryContinuation continuation)
        {
            return new ResultsWrapper(results, continuation);
        }

        internal void SetInsertingObject(object addedObject)
        {
            this.materializer.TargetInstance = addedObject;
        }

        internal static void SkipToEnd(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(reader.NodeType == XmlNodeType.Element, "reader.NodeType == XmlNodeType.Element");

            if (reader.IsEmptyElement)
            {
                return;
            }

            int readerDepth = reader.Depth;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == readerDepth)
                {
                    return;
                }
            }
        }

        internal long CountValue()
        {
            if (this.countValue == CountStateInitial)
            {
                this.ReadCountValue();
            }
            else if (this.countValue == CountStateFailure)
            {
                throw new InvalidOperationException(Strings.MaterializeFromAtom_CountNotPresent);
            }

            return this.countValue;
        }

        internal virtual DataServiceQueryContinuation GetContinuation(IEnumerable key)
        {
            Debug.Assert(this.materializer != null, "Materializer is null!");

            DataServiceQueryContinuation result;
            if (key == null)
            {
                if ((this.expectingSingleValue && !this.moved) || (!this.expectingSingleValue && !this.materializer.IsEndOfStream))
                {
                    throw new InvalidOperationException(Strings.MaterializeFromAtom_TopLevelLinkNotAvailable);
                }

                if (this.expectingSingleValue || this.materializer.CurrentFeed == null)
                {
                    result = null;
                }
                else
                {
                    result = DataServiceQueryContinuation.Create(
                        this.materializer.CurrentFeed.NextLink, 
                        this.materializer.MaterializeEntryPlan);
                }
            }
            else
            {
                if (!this.materializer.NextLinkTable.TryGetValue(key, out result))
                {
                    throw new ArgumentException(Strings.MaterializeFromAtom_CollectionKeyNotPresentInLinkTable);
                }
            }

            return result;
        }
            
        private void CheckGetEnumerator()
        {
            if (this.calledGetEnumerator)
            {
                throw Error.NotSupported(Strings.Deserialize_GetEnumerator);
            }

            this.calledGetEnumerator = true;
        }

        private void ReadCountValue()
        {
            Debug.Assert(this.countValue == CountStateInitial, "Count value is not in the initial state");

            if (this.materializer.CurrentFeed != null &&
                this.materializer.CurrentFeed.Count.HasValue)
            {
                this.countValue = this.materializer.CurrentFeed.Count.Value;
                return;
            }

            while (this.reader.NodeType != XmlNodeType.Element && this.reader.Read())
            {
            }

            if (this.reader.EOF)
            {
                throw new InvalidOperationException(Strings.MaterializeFromAtom_CountNotPresent);
            }

            Debug.Assert(
                (Util.AreSame(XmlConstants.AtomNamespace, this.reader.NamespaceURI) &&
                Util.AreSame(XmlConstants.AtomFeedElementName, this.reader.LocalName)) ||
                (Util.AreSame(XmlConstants.DataWebNamespace, this.reader.NamespaceURI) &&
                Util.AreSame(XmlConstants.LinkCollectionElementName, this.reader.LocalName)),
                "<feed> or <links> tag expected");

            XElement element = XElement.Load(this.reader);
            this.reader.Close();

            XElement countNode = element.Descendants(XNamespace.Get(XmlConstants.DataWebMetadataNamespace) + XmlConstants.RowCountElement).FirstOrDefault();

            if (countNode == null)
            {
                throw new InvalidOperationException(Strings.MaterializeFromAtom_CountNotPresent);
            }
            else
            {
                if (!long.TryParse(countNode.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out this.countValue))
                {
                    throw new FormatException(Strings.MaterializeFromAtom_CountFormatError);
                }

                if (this.countValue < 0)
                {
                    throw new FormatException(Strings.MaterializeFromAtom_CountFormatError);
                }
            }

            this.reader = new System.Data.Services.Client.Xml.XmlAtomErrorReader(element.CreateReader());
            this.parser.ReplaceReader(this.reader);
        }

        internal static ClientType GetEntryClientType(string typeName, DataServiceContext context, Type expectedType, bool checkAssignable)
        {
            Debug.Assert(context != null, "context != null");
            Type resolvedType = context.ResolveTypeFromName(typeName, expectedType, checkAssignable);
            ClientType result = ClientType.Create(resolvedType);
            Debug.Assert(result != null, "result != null -- otherwise ClientType.Create returned null");
            return result;
        }

        internal static string ReadElementString(XmlReader reader, bool checkNullAttribute)
        {
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(
                reader.NodeType == XmlNodeType.Element,
                "reader.NodeType == XmlNodeType.Element -- otherwise caller is confused as to where the reader is");

            string result = null;
            bool empty = checkNullAttribute && !Util.DoesNullAttributeSayTrue(reader);

            if (reader.IsEmptyElement)
            {
                return (empty ? String.Empty : null);
            }

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        return result ?? (empty ? String.Empty : null);
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                        if (null != result)
                        {
                            throw Error.InvalidOperation(Strings.Deserialize_MixedTextWithComment);
                        }

                        result = reader.Value;
                        break;
                    case XmlNodeType.Comment:
                    case XmlNodeType.Whitespace:
                        break;

                    #region XmlNodeType error
                    case XmlNodeType.Element:
                        goto default;

                    default:
                        throw Error.InvalidOperation(Strings.Deserialize_ExpectingSimpleValue);
                    #endregion
                }
            }

            throw Error.InvalidOperation(Strings.Deserialize_ExpectingSimpleValue);
        }

        private class ResultsWrapper : MaterializeAtom
        {
            #region Private fields.

            private readonly IEnumerable results;

            private readonly DataServiceQueryContinuation continuation;

            #endregion Private fields.

            internal ResultsWrapper(IEnumerable results, DataServiceQueryContinuation continuation)
            {
                this.results = results ?? new object[0];
                this.continuation = continuation;
            }

            internal override DataServiceQueryContinuation GetContinuation(IEnumerable key)
            {
                if (key == null)
                {
                    return this.continuation;
                }
                else
                {
                    throw new InvalidOperationException(Strings.MaterializeFromAtom_GetNestLinkForFlatCollection);
                }
            }

            public override IEnumerator GetEnumerator()
            {
                return this.results.GetEnumerator();
            }
        }
    }
}
