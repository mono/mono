//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Runtime.Serialization;
    using System.Linq;
    using System.Xml.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    static class SerializationUtilities
    {
        public static byte[] CreateKeyBinaryBlob(List<CorrelationKey> correlationKeys)
        {
            long memoryRequired = correlationKeys.Sum(i => i.BinaryData.Count);
            byte[] concatenatedBlob = null;

            if (memoryRequired > 0)
            {
                concatenatedBlob = new byte[memoryRequired];
                long insertLocation = 0;

                foreach (CorrelationKey correlationKey in correlationKeys)
                {
                    Buffer.BlockCopy(correlationKey.BinaryData.Array, 0, concatenatedBlob, Convert.ToInt32(insertLocation), Convert.ToInt32(correlationKey.BinaryData.Count));
                    correlationKey.StartPosition = insertLocation;
                    insertLocation += correlationKey.BinaryData.Count;
                }
            }

            return concatenatedBlob;
        }

        public static object CreateCorrelationKeyXmlBlob(List<CorrelationKey> correlationKeys)
        {
            if (correlationKeys == null || correlationKeys.Count == 0)
            {
                return DBNull.Value;
            }

            StringBuilder stringBuilder = new StringBuilder(SqlWorkflowInstanceStoreConstants.DefaultStringBuilderCapacity);

            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
            {
                xmlWriter.WriteStartElement("CorrelationKeys");

                foreach (CorrelationKey correlationKey in correlationKeys)
                {
                    correlationKey.SerializeToXmlElement(xmlWriter);
                }

                xmlWriter.WriteEndElement();
            }

            return stringBuilder.ToString();
        }

        public static bool IsPropertyTypeSqlVariantCompatible(InstanceValue value)
        {
            if ((value.IsDeletedValue) ||
                (value.Value == null) ||
                (value.Value is string && ((string)value.Value).Length <= 4000) ||
                (value.Value is Guid) ||
                (value.Value is DateTime) ||
                (value.Value is int) ||
                (value.Value is double) ||
                (value.Value is float) ||
                (value.Value is long) ||
                (value.Value is short) ||
                (value.Value is byte) ||
                (value.Value is decimal && CanDecimalBeStoredAsSqlVariant((decimal)value.Value)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Dictionary<XName, InstanceValue> DeserializeMetadataPropertyBag(byte[] serializedMetadataProperties, InstanceEncodingOption instanceEncodingOption)
        {
            Dictionary<XName, InstanceValue> metadataProperties = new Dictionary<XName, InstanceValue>();

            if (serializedMetadataProperties != null)
            {
                IObjectSerializer serializer = ObjectSerializerFactory.GetObjectSerializer(instanceEncodingOption);
                Dictionary<XName, object> propertyBag = serializer.DeserializePropertyBag(serializedMetadataProperties);

                foreach (KeyValuePair<XName, object> property in propertyBag)
                {
                    metadataProperties.Add(property.Key, new InstanceValue(property.Value));
                }
            }

            return metadataProperties;
        }

        public static ArraySegment<byte> SerializeMetadataPropertyBag(SaveWorkflowCommand saveWorkflowCommand,
            InstancePersistenceContext context, InstanceEncodingOption instanceEncodingOption)
        {
            IObjectSerializer serializer = ObjectSerializerFactory.GetObjectSerializer(instanceEncodingOption);
            Dictionary<XName, object> propertyBagToSerialize = new Dictionary<XName, object>();

            if (context.InstanceView.InstanceMetadataConsistency == InstanceValueConsistency.None)
            {
                foreach (KeyValuePair<XName, InstanceValue> metadataProperty in context.InstanceView.InstanceMetadata)
                {
                    if ((metadataProperty.Value.Options & InstanceValueOptions.WriteOnly) == 0)
                    {
                        propertyBagToSerialize.Add(metadataProperty.Key, metadataProperty.Value.Value);
                    }
                }
            }

            foreach (KeyValuePair<XName, InstanceValue> metadataChange in saveWorkflowCommand.InstanceMetadataChanges)
            {
                if (metadataChange.Value.IsDeletedValue)
                {
                    if (context.InstanceView.InstanceMetadataConsistency == InstanceValueConsistency.None)
                    {
                        propertyBagToSerialize.Remove(metadataChange.Key);
                    }
                    else
                    {
                        propertyBagToSerialize[metadataChange.Key] = new DeletedMetadataValue();
                    }
                }
                else if ((metadataChange.Value.Options & InstanceValueOptions.WriteOnly) == 0)
                {
                    propertyBagToSerialize[metadataChange.Key] = metadataChange.Value.Value;
                }
            }

            if (propertyBagToSerialize.Count > 0)
            {
                return serializer.SerializePropertyBag(propertyBagToSerialize);
            }

            return new ArraySegment<byte>();
        }

        public static ArraySegment<byte>[] SerializePropertyBag(IDictionary<XName, InstanceValue> properties, InstanceEncodingOption encodingOption)
        {
            ArraySegment<byte>[] dataArrays = new ArraySegment<byte>[4];

            if (properties.Count > 0)
            {
                IObjectSerializer serializer = ObjectSerializerFactory.GetObjectSerializer(encodingOption);
                XmlPropertyBag primitiveProperties = new XmlPropertyBag();
                XmlPropertyBag primitiveWriteOnlyProperties = new XmlPropertyBag();
                Dictionary<XName, object> complexProperties = new Dictionary<XName, object>();
                Dictionary<XName, object> complexWriteOnlyProperties = new Dictionary<XName, object>();
                Dictionary<XName, object>[] propertyBags = new Dictionary<XName, object>[] { primitiveProperties, complexProperties,
                    primitiveWriteOnlyProperties, complexWriteOnlyProperties };

                foreach (KeyValuePair<XName, InstanceValue> property in properties)
                {
                    bool isComplex = (XmlPropertyBag.GetPrimitiveType(property.Value.Value) == PrimitiveType.Unavailable);
                    bool isWriteOnly = (property.Value.Options & InstanceValueOptions.WriteOnly) == InstanceValueOptions.WriteOnly;
                    int index = (isWriteOnly ? 2 : 0) + (isComplex ? 1 : 0);
                    propertyBags[index].Add(property.Key, property.Value.Value);
                }

                // Remove the properties that are already stored as individual columns from the serialized blob
                primitiveWriteOnlyProperties.Remove(SqlWorkflowInstanceStoreConstants.StatusPropertyName);
                primitiveWriteOnlyProperties.Remove(SqlWorkflowInstanceStoreConstants.LastUpdatePropertyName);
                primitiveWriteOnlyProperties.Remove(SqlWorkflowInstanceStoreConstants.PendingTimerExpirationPropertyName);

                complexWriteOnlyProperties.Remove(SqlWorkflowInstanceStoreConstants.BinaryBlockingBookmarksPropertyName);

                for (int i = 0; i < propertyBags.Length; i++)
                {
                    if (propertyBags[i].Count > 0)
                    {
                        if (propertyBags[i] is XmlPropertyBag)
                        {
                            dataArrays[i] = serializer.SerializeValue(propertyBags[i]);
                        }
                        else
                        {
                            dataArrays[i] = serializer.SerializePropertyBag(propertyBags[i]);
                        }
                    }
                }
            }

            return dataArrays;
        }

        public static ArraySegment<byte> SerializeKeyMetadata(IDictionary<XName, InstanceValue> metadataProperties, InstanceEncodingOption encodingOption)
        {
            if (metadataProperties != null && metadataProperties.Count > 0)
            {
                Dictionary<XName, object> propertyBag = new Dictionary<XName, object>();

                foreach (KeyValuePair<XName, InstanceValue> property in metadataProperties)
                {
                    if ((property.Value.Options & InstanceValueOptions.WriteOnly) != InstanceValueOptions.WriteOnly)
                    {
                        propertyBag.Add(property.Key, property.Value.Value);
                    }
                }

                IObjectSerializer serializer = ObjectSerializerFactory.GetObjectSerializer(encodingOption);
                return serializer.SerializePropertyBag(propertyBag);
            }

            return new ArraySegment<byte>();
        }

        public static Dictionary<XName, InstanceValue> DeserializeKeyMetadata(byte[] serializedKeyMetadata, InstanceEncodingOption encodingOption)
        {
            return DeserializeMetadataPropertyBag(serializedKeyMetadata, encodingOption);
        }

        public static Dictionary<XName, InstanceValue> DeserializePropertyBag(byte[] primitiveDataProperties, byte[] complexDataProperties, InstanceEncodingOption encodingOption)
        {
            IObjectSerializer serializer = ObjectSerializerFactory.GetObjectSerializer(encodingOption);
            Dictionary<XName, InstanceValue> properties = new Dictionary<XName, InstanceValue>();
            Dictionary<XName, object>[] propertyBags = new Dictionary<XName, object>[2];

            if (primitiveDataProperties != null)
            {
                propertyBags[0] = (Dictionary<XName, object>)serializer.DeserializeValue(primitiveDataProperties);
            }

            if (complexDataProperties != null)
            {
                propertyBags[1] = serializer.DeserializePropertyBag(complexDataProperties);
            }

            foreach (Dictionary<XName, object> propertyBag in propertyBags)
            {
                if (propertyBag != null)
                {
                    foreach (KeyValuePair<XName, object> property in propertyBag)
                    {
                        properties.Add(property.Key, new InstanceValue(property.Value));
                    }
                }
            }

            return properties;
        }

        static bool CanDecimalBeStoredAsSqlVariant(decimal value)
        {
            string decimalAsString = value.ToString("G", CultureInfo.InvariantCulture);
            return ((decimalAsString.Length - decimalAsString.IndexOf(".", StringComparison.Ordinal)) - 1 <= 18);
        }

        static Guid GetIdentityHash(WorkflowIdentity id)
        {
            byte[] identityHashBuffer = Encoding.Unicode.GetBytes(id.ToString());
            return new Guid(HashHelper.ComputeHash(identityHashBuffer));
        }

        static Guid GetIdentityAnyRevisionFilterHash(WorkflowIdentity id)
        {
            if (id.Version != null)
            {
                Version version;
                if (id.Version.Build >= 0)
                {
                    version = new Version(id.Version.Major, id.Version.Minor, id.Version.Build);
                }
                else
                {
                    version = new Version(id.Version.Minor, id.Version.Minor);
                }

                return GetIdentityHash(new WorkflowIdentity(id.Name, version, id.Package));
            }
            else
            {
                return GetIdentityHash(id);
            }
        }

        public static string GetIdentityMetadataXml(InstancePersistenceCommand command)
        {
            StringBuilder stringBuilder = new StringBuilder(512);
            Guid idHash = Guid.Empty;
            Guid idAnyRevisionHash = Guid.Empty;
            int workflowIdentityFilter = (int)WorkflowIdentityFilter.Exact;

            IList<WorkflowIdentity> identityCollection = null;

            if (command is CreateWorkflowOwnerWithIdentityCommand)
            {
                InstanceValue instanceValueIdentityCollection = null;
                CreateWorkflowOwnerWithIdentityCommand ownerCommand = command as CreateWorkflowOwnerWithIdentityCommand;

                if (ownerCommand.InstanceOwnerMetadata.TryGetValue(Workflow45Namespace.DefinitionIdentities, out instanceValueIdentityCollection))
                {
                    if (instanceValueIdentityCollection.Value != null)
                    {
                        identityCollection = instanceValueIdentityCollection.Value as IList<WorkflowIdentity>;
                        if (identityCollection == null)
                        {
                            string typeName = typeof(IList<>).Name.Replace("`1", "<" + typeof(WorkflowIdentity).Name + ">");
                            throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.InvalidMetadataValue(Workflow45Namespace.DefinitionIdentities, typeName)));
                        }
                    }
                }

                InstanceValue instanceValue = null;
                if (ownerCommand.InstanceOwnerMetadata.TryGetValue(Workflow45Namespace.DefinitionIdentityFilter, out instanceValue))
                {
                    if (instanceValue.Value != null)
                    {
                        if (instanceValue.Value is WorkflowIdentityFilter)
                        {
                            workflowIdentityFilter = (int)instanceValue.Value;
                        }
                        else
                        {
                            workflowIdentityFilter = -1;
                        }

                        if (workflowIdentityFilter != (int)WorkflowIdentityFilter.Exact &&
                            workflowIdentityFilter != (int)WorkflowIdentityFilter.Any &&
                            workflowIdentityFilter != (int)WorkflowIdentityFilter.AnyRevision)
                        {
                            throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.InvalidMetadataValue(Workflow45Namespace.DefinitionIdentityFilter, typeof(WorkflowIdentityFilter).Name)));
                        }
                    }
                }
            }
            else if (command is SaveWorkflowCommand)
            {
                InstanceValue instanceValue = null;
                SaveWorkflowCommand saveCommand = command as SaveWorkflowCommand;
                if (saveCommand.InstanceMetadataChanges.TryGetValue(Workflow45Namespace.DefinitionIdentity, out instanceValue))
                {
                    if (!instanceValue.IsDeletedValue && instanceValue.Value != null)
                    {
                        identityCollection = new Collection<WorkflowIdentity>();
                        if (!(instanceValue.Value is WorkflowIdentity))
                        {
                            throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.InvalidMetadataValue(Workflow45Namespace.DefinitionIdentity, typeof(WorkflowIdentity).Name)));
                        }

                        identityCollection.Add((WorkflowIdentity)instanceValue.Value);
                    }
                }
                else
                {
                    // If identity isn't specified, we preserve the instance's existing identity
                    return null;
                }
            }
            else
            {
                return null;
            }

            if (identityCollection == null)
            {
                // Assume NULL Identity
                identityCollection = new Collection<WorkflowIdentity>();
                identityCollection.Add(null);
            }

            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("IdentityMetadata");

                // Write the Identity Collection
                xmlWriter.WriteStartElement("IdentityCollection");
                foreach (WorkflowIdentity id in identityCollection)
                {
                    xmlWriter.WriteStartElement("Identity");
        
                    if (id == null)
                    {
                        xmlWriter.WriteElementString("DefinitionIdentityHash", Guid.Empty.ToString());
                        xmlWriter.WriteElementString("DefinitionIdentityAnyRevisionHash", Guid.Empty.ToString());
                    }
                    else
                    {
                        idHash = GetIdentityHash(id);
                        idAnyRevisionHash = GetIdentityAnyRevisionFilterHash(id);

                        xmlWriter.WriteElementString("DefinitionIdentityHash", idHash.ToString());
                        xmlWriter.WriteElementString("DefinitionIdentityAnyRevisionHash", idAnyRevisionHash.ToString());
                        xmlWriter.WriteElementString("Name", id.Name);
                        
                        if (id.Package != null)
                        {
                            xmlWriter.WriteElementString("Package", id.Package);
                        }

                        if (id.Version != null)
                        {
                            xmlWriter.WriteElementString("Major", id.Version.Major.ToString(CultureInfo.InvariantCulture));
                            xmlWriter.WriteElementString("Minor", id.Version.Minor.ToString(CultureInfo.InvariantCulture));
                            if (id.Version.Build >= 0)
                            {
                                xmlWriter.WriteElementString("Build", id.Version.Build.ToString(CultureInfo.InvariantCulture));
                                if (id.Version.Revision >= 0)
                                {
                                    xmlWriter.WriteElementString("Revision", id.Version.Revision.ToString(CultureInfo.InvariantCulture));
                                }
                            }
                        }
                    }
        
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                // Write the WorkflowIdentityFilter
                xmlWriter.WriteElementString("WorkflowIdentityFilter", workflowIdentityFilter.ToString(CultureInfo.InvariantCulture));

                xmlWriter.WriteEndElement();
            }
            return stringBuilder.ToString();
        }

    }
}
