using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using LinqToSqlShared.Mapping;

namespace System.Data.Linq.Mapping {
    using System.Data.Linq.Provider;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a source for mapping information.
    /// </summary>
    public abstract class MappingSource {
        MetaModel primaryModel;
        ReaderWriterLock rwlock;
        Dictionary<Type, MetaModel> secondaryModels;

        /// <summary>
        /// Gets the MetaModel representing a DataContext and all it's 
        /// accessible tables, functions and entities.
        /// </summary>
        public MetaModel GetModel(Type dataContextType) {
            if (dataContextType == null) {
                throw Error.ArgumentNull("dataContextType");
            }
            MetaModel model = null;
            if (this.primaryModel == null) {
                model = this.CreateModel(dataContextType);
                Interlocked.CompareExchange<MetaModel>(ref this.primaryModel, model, null);
            }

            // if the primary one matches, use it!
            if (this.primaryModel.ContextType == dataContextType) {
                return this.primaryModel;
            }

            // the rest of this only happens if you are using the mapping source for
            // more than one context type

            // build a map if one is not already defined
            if (this.secondaryModels == null) {
                Interlocked.CompareExchange<Dictionary<Type, MetaModel>>(ref this.secondaryModels, new Dictionary<Type, MetaModel>(), null);
            }

            // if we haven't created a read/writer lock, make one now
            if (this.rwlock == null) {
                Interlocked.CompareExchange<ReaderWriterLock>(ref this.rwlock, new ReaderWriterLock(), null);
            }

            // lock the map and look inside
            MetaModel foundModel;
            this.rwlock.AcquireReaderLock(Timeout.Infinite);
            try {
                if (this.secondaryModels.TryGetValue(dataContextType, out foundModel)) {
                    return foundModel;
                }
            }
            finally {
                this.rwlock.ReleaseReaderLock();
            }
            // if it wasn't found, lock for write and try again
            this.rwlock.AcquireWriterLock(Timeout.Infinite);
            try {
                if (this.secondaryModels.TryGetValue(dataContextType, out foundModel)) {
                    return foundModel;
                }
                if (model == null) {
                    model = this.CreateModel(dataContextType);
                }
                this.secondaryModels.Add(dataContextType, model);
            }
            finally {
                this.rwlock.ReleaseWriterLock();
            }
            return model;
        }

        /// <summary>
        /// Creates a new instance of a MetaModel.  This method is called by GetModel().
        /// Override this method when defining a new type of MappingSource.
        /// </summary>
        /// <param name="dataContextType"></param>
        /// <returns></returns>
        protected abstract MetaModel CreateModel(Type dataContextType);
    }


    /// <summary>
    /// A mapping source that uses attributes on the context to create the mapping model.
    /// </summary>
    public sealed class AttributeMappingSource : MappingSource {
        public AttributeMappingSource() {
        }

        protected override MetaModel CreateModel(Type dataContextType) {
            if (dataContextType == null) {
                throw Error.ArgumentNull("dataContextType");
            }
            return new AttributedMetaModel(this, dataContextType);
        }
    }


    /// <summary>
    /// A mapping source that uses an external XML mapping source to create the model.
    /// </summary>
    public sealed class XmlMappingSource : MappingSource {
        DatabaseMapping map;
        
        [ResourceExposure(ResourceScope.Assembly)] // map parameter contains type names.
        private XmlMappingSource(DatabaseMapping map) {
            this.map = map;
        }

        [ResourceExposure(ResourceScope.None)] // Exposure is via map instance variable.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine, ResourceScope.Assembly | ResourceScope.Machine)] // For MappedMetaModel constructor call.
        protected override MetaModel CreateModel(Type dataContextType) {
            if (dataContextType == null) {
                throw Error.ArgumentNull("dataContextType");
            }
            return new MappedMetaModel(this, dataContextType, this.map);
        }

        /// <summary>
        /// Create a mapping source from xml string.
        /// </summary>
        /// <param name="dataContextType">The type of DataContext to base the mapping on.</param>
        /// <param name="xml">A string containing XML.</param>
        /// <returns>The mapping source.</returns>
        [ResourceExposure(ResourceScope.Assembly)] // Xml contains type names.
        [ResourceConsumption(ResourceScope.Assembly)] // For FromReader method call.
        public static XmlMappingSource FromXml(string xml) {
            if (xml == null) {
                throw Error.ArgumentNull("xml");
            }
            XmlTextReader reader = new XmlTextReader(new System.IO.StringReader(xml));
            reader.DtdProcessing = DtdProcessing.Prohibit;

            return FromReader(reader);
        }

        /// <summary>
        /// Create a mapping source from xml reader.
        /// </summary>
        /// <param name="dataContextType">The type of DataContext to base the mapping on.</param>
        /// <param name="xml">An xml reader.</param>
        /// <returns>The mapping source.</returns>
        [ResourceExposure(ResourceScope.Assembly)] // reader parameter contains type names.
        [ResourceConsumption(ResourceScope.Assembly)] // XmlMappingSource constructor call.
        public static XmlMappingSource FromReader(XmlReader reader) {
            if (reader == null) {
                throw Error.ArgumentNull("reader");
            }
            reader.MoveToContent();
            DatabaseMapping db = XmlMappingReader.ReadDatabaseMapping(reader);

            if (db == null) {
                throw Error.DatabaseNodeNotFound(XmlMappingConstant.MappingNamespace);
            }

            return new XmlMappingSource(db);
        }

        /// <summary>
        /// Create a mapping source from xml in a stream.
        /// </summary>
        /// <param name="dataContextType">The type of DataContext to base the mapping on.</param>
        /// <param name="xml">A stream of xml.</param>
        /// <returns>The mapping source.</returns>
        [ResourceExposure(ResourceScope.Assembly)] // Stream contains type names.
        [ResourceConsumption(ResourceScope.Assembly)] // For FromReader method call.
        public static XmlMappingSource FromStream(System.IO.Stream stream) {
            if (stream == null) {
                throw Error.ArgumentNull("stream");
            }
            XmlTextReader reader = new XmlTextReader(stream);
            reader.DtdProcessing = DtdProcessing.Prohibit;

            return FromReader(reader);
        }

        /// <summary>
        /// Create a mapping source from xml loaded from a url.
        /// </summary>
        /// <param name="dataContextType">The type of DataContext to base the mapping on.</param>
        /// <param name="url">The Url pointing to the xml.</param>
        /// <returns>The mapping source.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification="Unknown reason.")]
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)] // url parameter which may contain type names.
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly)] // XmlTextReader constructor & FromReader method call.
        public static XmlMappingSource FromUrl(string url) {
            if (url == null) {
                throw Error.ArgumentNull("url");
            }
            XmlTextReader reader = new XmlTextReader(url);
            reader.DtdProcessing = DtdProcessing.Prohibit;

            try {
                return FromReader(reader);
            }
            finally {
                reader.Close();
            }
        }
    }

    class XmlMappingReader {
        private static string RequiredAttribute(XmlReader reader, string attribute) {
            string result = OptionalAttribute(reader, attribute);
            if (result == null) {
                throw Error.CouldNotFindRequiredAttribute(attribute, reader.ReadOuterXml());
            }
            return result;
        }

        private static string OptionalAttribute(XmlReader reader, string attribute) {
            return reader.GetAttribute(attribute);
        }

        private static bool OptionalBoolAttribute(XmlReader reader, string attribute, bool @default) {
            string value = OptionalAttribute(reader, attribute);
            return (value != null) ? bool.Parse(value) : @default;
        }

        private static bool? OptionalNullableBoolAttribute(XmlReader reader, string attribute) {
            string value = OptionalAttribute(reader, attribute);
            return (value != null) ? (bool?)bool.Parse(value) : null;
        }

        private static void AssertEmptyElement(XmlReader reader) {
            if (!reader.IsEmptyElement) {
                string nodeName = reader.Name;
                reader.Read();
                if (reader.NodeType != XmlNodeType.EndElement) {
                    throw Error.ExpectedEmptyElement(nodeName, reader.NodeType, reader.Name);
                }
            }

            reader.Skip();
        }

        internal static DatabaseMapping ReadDatabaseMapping(XmlReader reader) {
            System.Diagnostics.Debug.Assert(reader.NodeType == XmlNodeType.Element);
            if (!IsInNamespace(reader) || reader.LocalName != XmlMappingConstant.Database) {
                return null;
            }

            ValidateAttributes(reader, new[] { 
                                               XmlMappingConstant.Name, 
                                               XmlMappingConstant.Provider 
                                           });

            DatabaseMapping dm = new DatabaseMapping();

            dm.DatabaseName = RequiredAttribute(reader, XmlMappingConstant.Name);
            dm.Provider = OptionalAttribute(reader, XmlMappingConstant.Provider);

            if (!reader.IsEmptyElement) {
                reader.ReadStartElement();
                reader.MoveToContent();
                while (reader.NodeType != XmlNodeType.EndElement) {
                    if (reader.NodeType == XmlNodeType.Whitespace || !IsInNamespace(reader)) {
                        reader.Skip();
                        continue;
                    }

                    switch (reader.LocalName) {
                        case XmlMappingConstant.Table:
                            dm.Tables.Add(ReadTableMapping(reader));
                            break;
                        case XmlMappingConstant.Function:
                            dm.Functions.Add(ReadFunctionMapping(reader));
                            break;
                        default:
                            throw Error.UnrecognizedElement(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
                    }
                    reader.MoveToContent();
                }
                
                if (reader.LocalName != XmlMappingConstant.Database) {
                    throw Error.UnexpectedElement(XmlMappingConstant.Database, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
                } 
                
                reader.ReadEndElement();
            }
            else {
                System.Diagnostics.Debug.Assert(false, "DatabaseMapping has no content");
                reader.Skip();
            }

            return dm;
        }

        internal static bool IsInNamespace(XmlReader reader) {
            return reader.LookupNamespace(reader.Prefix) == XmlMappingConstant.MappingNamespace;
        }

        internal static void ValidateAttributes(XmlReader reader, string[] validAttributes) {
            if (reader.HasAttributes) {
                List<string> attrList = new List<string>(validAttributes);
                const string xmlns = "xmlns";

                for (int i = 0; i < reader.AttributeCount; i++) {
                    reader.MoveToAttribute(i);

                    // if the node's in the namespace, it is required to be one of the valid ones
                    if (IsInNamespace(reader) && reader.LocalName != xmlns && !attrList.Contains(reader.LocalName)) {
                        throw Error.UnrecognizedAttribute(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : ":", reader.LocalName));
                    }
                }

                reader.MoveToElement(); // Moves the reader back to the element node.
            }
        }

        internal static FunctionMapping ReadFunctionMapping(XmlReader reader) {
            System.Diagnostics.Debug.Assert(reader.NodeType == XmlNodeType.Element);
            if (!IsInNamespace(reader) || reader.LocalName != XmlMappingConstant.Function) {
                throw Error.UnexpectedElement(XmlMappingConstant.Function, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
            } 

            ValidateAttributes(reader, new[] { 
                                               XmlMappingConstant.Name, 
                                               XmlMappingConstant.Method, 
                                               XmlMappingConstant.IsComposable
                                           });

            FunctionMapping fm = new FunctionMapping();
            fm.MethodName = RequiredAttribute(reader, XmlMappingConstant.Method);
            fm.Name = OptionalAttribute(reader, XmlMappingConstant.Name);
            fm.IsComposable = OptionalBoolAttribute(reader, XmlMappingConstant.IsComposable, false);

            if (!reader.IsEmptyElement) {
                reader.ReadStartElement();
                reader.MoveToContent();
                while (reader.NodeType != XmlNodeType.EndElement) {
                    if (reader.NodeType == XmlNodeType.Whitespace || !IsInNamespace(reader)) {
                        reader.Skip();
                        continue;
                    }
                    
                    switch (reader.LocalName) {
                        case XmlMappingConstant.Parameter:
                            fm.Parameters.Add(ReadParameterMapping(reader));
                            break;
                        case XmlMappingConstant.ElementType:
                            fm.Types.Add(ReadElementTypeMapping(null, reader));
                            break;
                        case XmlMappingConstant.Return:
                            fm.FunReturn = ReadReturnMapping(reader);
                            break;
                        default:
                            throw Error.UnrecognizedElement(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
                    }
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
            }
            else {
                // no content is okay
                reader.Skip();
            }

            return fm;
        }

        private static ReturnMapping ReadReturnMapping(XmlReader reader) {
            System.Diagnostics.Debug.Assert(reader.NodeType == XmlNodeType.Element);
            if (!IsInNamespace(reader) || reader.LocalName != XmlMappingConstant.Return) {
                throw Error.UnexpectedElement(XmlMappingConstant.Return, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
            } 

            ValidateAttributes(reader, new[] { 
                                               XmlMappingConstant.DbType
                                           });

            ReturnMapping rm = new ReturnMapping();
            rm.DbType = OptionalAttribute(reader, XmlMappingConstant.DbType);

            AssertEmptyElement(reader);

            return rm;
        }

        private static ParameterMapping ReadParameterMapping(XmlReader reader) {
            System.Diagnostics.Debug.Assert(reader.NodeType == XmlNodeType.Element);
            if (!IsInNamespace(reader) || reader.LocalName != XmlMappingConstant.Parameter) {
                throw Error.UnexpectedElement(XmlMappingConstant.Parameter, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
            }

            ValidateAttributes(reader, new[] { 
                                               XmlMappingConstant.Name, 
                                               XmlMappingConstant.DbType, 
                                               XmlMappingConstant.Parameter, 
                                               XmlMappingConstant.Direction
                                           });

            ParameterMapping pm = new ParameterMapping();
            pm.Name = RequiredAttribute(reader, XmlMappingConstant.Name);
            pm.ParameterName = RequiredAttribute(reader, XmlMappingConstant.Parameter);
            pm.DbType = OptionalAttribute(reader, XmlMappingConstant.DbType);
            pm.XmlDirection = OptionalAttribute(reader, XmlMappingConstant.Direction);

            AssertEmptyElement(reader);

            return pm;
        }

        private static TableMapping ReadTableMapping(XmlReader reader) {
            System.Diagnostics.Debug.Assert(reader.NodeType == XmlNodeType.Element);
            if (!IsInNamespace(reader) || reader.LocalName != XmlMappingConstant.Table) {
                throw Error.UnexpectedElement(XmlMappingConstant.Table, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
            }

            ValidateAttributes(reader, new[] { 
                                               XmlMappingConstant.Name, 
                                               XmlMappingConstant.Member
                                           });

            TableMapping tm = new TableMapping();
            tm.TableName = OptionalAttribute(reader, XmlMappingConstant.Name);
            tm.Member = OptionalAttribute(reader, XmlMappingConstant.Member);

            if (!reader.IsEmptyElement) {
                reader.ReadStartElement();
                reader.MoveToContent();
                while (reader.NodeType != XmlNodeType.EndElement) {
                    if (reader.NodeType == XmlNodeType.Whitespace || !IsInNamespace(reader)) {
                        reader.Skip();
                        continue;
                    }

                    switch (reader.LocalName) {
                        case XmlMappingConstant.Type:
                            if (tm.RowType != null) {
                                goto default;
                            }
                            tm.RowType = ReadTypeMapping(null, reader);
                            break;
                        default:
                            throw Error.UnrecognizedElement(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
                    }
                    reader.MoveToContent();
                }
                
                if (reader.LocalName != XmlMappingConstant.Table) {
                    throw Error.UnexpectedElement(XmlMappingConstant.Table, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
                } 
                
                reader.ReadEndElement();
            }
            else {
                System.Diagnostics.Debug.Assert(false, "Table has no content");
                reader.Skip();
            }

            return tm;
        }

        private static TypeMapping ReadElementTypeMapping(TypeMapping baseType, XmlReader reader) {
            System.Diagnostics.Debug.Assert(reader.NodeType == XmlNodeType.Element);
            if (!IsInNamespace(reader) || reader.LocalName != XmlMappingConstant.ElementType) {
                throw Error.UnexpectedElement(XmlMappingConstant.Type, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
            }

            return ReadTypeMappingImpl(baseType, reader);
        }

        private static TypeMapping ReadTypeMapping(TypeMapping baseType, XmlReader reader) {
            System.Diagnostics.Debug.Assert(reader.NodeType == XmlNodeType.Element);
            if (!IsInNamespace(reader) || reader.LocalName != XmlMappingConstant.Type) {
                throw Error.UnexpectedElement(XmlMappingConstant.Type, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
            }

            return ReadTypeMappingImpl(baseType, reader);
        }

        private static TypeMapping ReadTypeMappingImpl(TypeMapping baseType, XmlReader reader) {
            ValidateAttributes(reader, new[] { 
                                                 XmlMappingConstant.Name, 
                                                 XmlMappingConstant.InheritanceCode, 
                                                 XmlMappingConstant.IsInheritanceDefault
                                             });

            TypeMapping tm = new TypeMapping();
            tm.BaseType = baseType;
            tm.Name = RequiredAttribute(reader, XmlMappingConstant.Name);
            tm.InheritanceCode = OptionalAttribute(reader, XmlMappingConstant.InheritanceCode);
            tm.IsInheritanceDefault = OptionalBoolAttribute(reader, XmlMappingConstant.IsInheritanceDefault, false);

            if (!reader.IsEmptyElement) {
                reader.ReadStartElement();
                reader.MoveToContent();
                while (reader.NodeType != XmlNodeType.EndElement) {
                    if (reader.NodeType == XmlNodeType.Whitespace || !IsInNamespace(reader)) {
                        reader.Skip();
                        continue;
                    }

                    switch (reader.LocalName) {
                        case XmlMappingConstant.Type:
                            tm.DerivedTypes.Add(ReadTypeMapping(tm, reader));
                            break;
                        case XmlMappingConstant.Association:
                            tm.Members.Add(ReadAssociationMapping(reader));
                            break;
                        case XmlMappingConstant.Column:
                            tm.Members.Add(ReadColumnMapping(reader));
                            break;
                        default:
                            throw Error.UnrecognizedElement(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
                    }
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
            }
            else {
                // no content is okay
                reader.Skip();
            }
            return tm;
        }

        private static AssociationMapping ReadAssociationMapping(XmlReader reader) {
            System.Diagnostics.Debug.Assert(reader.NodeType == XmlNodeType.Element);
            if (!IsInNamespace(reader) || reader.LocalName != XmlMappingConstant.Association) {
                throw Error.UnexpectedElement(XmlMappingConstant.Association, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
            }

            ValidateAttributes(reader, new[] { 
                                               XmlMappingConstant.Name, 
                                               XmlMappingConstant.IsForeignKey, 
                                               XmlMappingConstant.IsUnique, 
                                               XmlMappingConstant.Member, 
                                               XmlMappingConstant.OtherKey, 
                                               XmlMappingConstant.Storage, 
                                               XmlMappingConstant.ThisKey, 
                                               XmlMappingConstant.DeleteRule, 
                                               XmlMappingConstant.DeleteOnNull, 
                                           });

            AssociationMapping am = new AssociationMapping();
            am.DbName = OptionalAttribute(reader, XmlMappingConstant.Name);
            am.IsForeignKey = OptionalBoolAttribute(reader, XmlMappingConstant.IsForeignKey, false);
            am.IsUnique = OptionalBoolAttribute(reader, XmlMappingConstant.IsUnique, false);
            am.MemberName = RequiredAttribute(reader, XmlMappingConstant.Member);
            am.OtherKey = OptionalAttribute(reader, XmlMappingConstant.OtherKey);
            am.StorageMemberName = OptionalAttribute(reader, XmlMappingConstant.Storage);
            am.ThisKey = OptionalAttribute(reader, XmlMappingConstant.ThisKey);
            am.DeleteRule = OptionalAttribute(reader, XmlMappingConstant.DeleteRule);
            am.DeleteOnNull = OptionalBoolAttribute(reader, XmlMappingConstant.DeleteOnNull, false);

            AssertEmptyElement(reader);

            return am;
        }

        private static ColumnMapping ReadColumnMapping(XmlReader reader) {
            System.Diagnostics.Debug.Assert(reader.NodeType == XmlNodeType.Element);
            if (!IsInNamespace(reader) || reader.LocalName != XmlMappingConstant.Column) {
                throw Error.UnexpectedElement(XmlMappingConstant.Column, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}", reader.Prefix, String.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName));
            }

            ValidateAttributes(reader, new[] { 
                                               XmlMappingConstant.Name, 
                                               XmlMappingConstant.DbType, 
                                               XmlMappingConstant.IsDbGenerated, 
                                               XmlMappingConstant.IsDiscriminator, 
                                               XmlMappingConstant.IsPrimaryKey, 
                                               XmlMappingConstant.IsVersion, 
                                               XmlMappingConstant.Member, 
                                               XmlMappingConstant.Storage, 
                                               XmlMappingConstant.Expression, 
                                               XmlMappingConstant.CanBeNull, 
                                               XmlMappingConstant.UpdateCheck, 
                                               XmlMappingConstant.AutoSync
                                           });

            ColumnMapping cm = new ColumnMapping();
            cm.DbName = OptionalAttribute(reader, XmlMappingConstant.Name);
            cm.DbType = OptionalAttribute(reader, XmlMappingConstant.DbType);
            cm.IsDbGenerated = OptionalBoolAttribute(reader, XmlMappingConstant.IsDbGenerated, false);
            cm.IsDiscriminator = OptionalBoolAttribute(reader, XmlMappingConstant.IsDiscriminator, false);
            cm.IsPrimaryKey = OptionalBoolAttribute(reader, XmlMappingConstant.IsPrimaryKey, false);
            cm.IsVersion = OptionalBoolAttribute(reader, XmlMappingConstant.IsVersion, false);
            cm.MemberName = RequiredAttribute(reader, XmlMappingConstant.Member);
            cm.StorageMemberName = OptionalAttribute(reader, XmlMappingConstant.Storage);
            cm.Expression = OptionalAttribute(reader, XmlMappingConstant.Expression);
            cm.CanBeNull = OptionalNullableBoolAttribute(reader, XmlMappingConstant.CanBeNull);
            string updateCheck = OptionalAttribute(reader, XmlMappingConstant.UpdateCheck);
            cm.UpdateCheck = (updateCheck == null) ? UpdateCheck.Always : (UpdateCheck)Enum.Parse(typeof(UpdateCheck), updateCheck);
            string autoSync = OptionalAttribute(reader, XmlMappingConstant.AutoSync);
            cm.AutoSync = (autoSync == null) ? AutoSync.Default : (AutoSync)Enum.Parse(typeof(AutoSync), autoSync);

            AssertEmptyElement(reader);

            return cm;
        }
    }
}
