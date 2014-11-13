//---------------------------------------------------------------------
// <copyright file="DbCommandTree.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees.Internal;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Describes the different "kinds" (classes) of command trees.
    /// </summary>
    internal enum DbCommandTreeKind
    {
        Query,
        Update,
        Insert,
        Delete,
        Function,
    }

    /// <summary>
    /// DbCommandTree is the abstract base type for the Delete, Query, Insert and Update DbCommandTree types.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbCommandTree
    {      
        // Metadata collection
        private readonly MetadataWorkspace _metadata;
        private readonly DataSpace _dataSpace;
                
        /// <summary>
        /// Initializes a new command tree with a given metadata workspace.
        /// </summary>
        /// <param name="metadata">The metadata workspace against which the command tree should operate.</param>
        /// <param name="dataSpace">The logical 'space' that metadata in the expressions used in this command tree must belong to.</param>
        internal DbCommandTree(MetadataWorkspace metadata, DataSpace dataSpace)
        {
            // Ensure the metadata workspace is non-null
            EntityUtil.CheckArgumentNull(metadata, "metadata");

            // Ensure that the data space value is valid
            if (!DbCommandTree.IsValidDataSpace(dataSpace))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_CommandTree_InvalidDataSpace, "dataSpace");
            }

            //
            // Create the tree's metadata workspace and initalize commonly used types.
            //
            MetadataWorkspace effectiveMetadata = new MetadataWorkspace();
                
            //While EdmItemCollection and StorageitemCollections are required
            //ObjectItemCollection may or may not be registered on the workspace yet.
            //So register the ObjectItemCollection if it exists.
            ItemCollection objectItemCollection;
            if (metadata.TryGetItemCollection(DataSpace.OSpace, out objectItemCollection))
            {
                effectiveMetadata.RegisterItemCollection(objectItemCollection);
            }                
            effectiveMetadata.RegisterItemCollection(metadata.GetItemCollection(DataSpace.CSpace));
            effectiveMetadata.RegisterItemCollection(metadata.GetItemCollection(DataSpace.CSSpace));
            effectiveMetadata.RegisterItemCollection(metadata.GetItemCollection(DataSpace.SSpace));

            this._metadata = effectiveMetadata;
            this._dataSpace = dataSpace;
        }
                                        
        /// <summary>
        /// Gets the name and corresponding type of each parameter that can be referenced within this command tree.
        /// </summary>
        public IEnumerable<KeyValuePair<string, TypeUsage>> Parameters
        {
            get
            {
                return this.GetParameters();
            }
        }
                
        #region Internal Implementation
        
        /// <summary>
        /// Gets the kind of this command tree.
        /// </summary>
        internal abstract DbCommandTreeKind CommandTreeKind { get; }

        /// <summary>
        /// Gets the name and type of each parameter declared on the command tree.
        /// </summary>
        /// <returns></returns>
        internal abstract IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters();
        
        /// <summary>
        /// Gets the metadata workspace used by this command tree.
        /// </summary>
        internal MetadataWorkspace MetadataWorkspace { get { return _metadata; } }

        /// <summary>
        /// Gets the data space in which metadata used by this command tree must reside.
        /// </summary>
        internal DataSpace DataSpace { get { return _dataSpace; } }
                
        #region Dump/Print Support

        internal void Dump(ExpressionDumper dumper)
        {
            //
            // Dump information about this command tree to the specified ExpressionDumper
            //
            // First dump standard information - the DataSpace of the command tree and its parameters
            //
            Dictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add("DataSpace", this.DataSpace);
            dumper.Begin(this.GetType().Name, attrs);

            //
            // The name and type of each Parameter in turn is added to the output
            //
            dumper.Begin("Parameters", null);
            foreach (KeyValuePair<string, TypeUsage> param in this.Parameters)
            {
                Dictionary<string, object> paramAttrs = new Dictionary<string, object>();
                paramAttrs.Add("Name", param.Key);
                dumper.Begin("Parameter", paramAttrs);
                dumper.Dump(param.Value, "ParameterType");
                dumper.End("Parameter");
            }
            dumper.End("Parameters");

            //
            // Delegate to the derived type's implementation that dumps the structure of the command tree
            //
            this.DumpStructure(dumper);

            //
            // Matching call to End to correspond with the call to Begin above
            //
            dumper.End(this.GetType().Name);
        }

        internal abstract void DumpStructure(ExpressionDumper dumper);

        internal string DumpXml()
        {
            //
            // This is a convenience method that dumps the command tree in an XML format.
            // This is intended primarily as a debugging aid to allow inspection of the tree structure.
            //
            // Create a new MemoryStream that the XML dumper should write to.
            //
            MemoryStream stream = new MemoryStream();

            //
            // Create the dumper
            //
            XmlExpressionDumper dumper = new XmlExpressionDumper(stream);

            //
            // Dump this tree and then close the XML dumper so that the end document tag is written
            // and the output is flushed to the stream.
            //
            this.Dump(dumper);
            dumper.Close();

            //
            // Construct a string from the resulting memory stream and return it to the caller
            //
            return XmlExpressionDumper.DefaultEncoding.GetString(stream.ToArray());
        }

        internal string Print()
        {
            return this.PrintTree(new ExpressionPrinter());
        }

        internal abstract string PrintTree(ExpressionPrinter printer);

        #endregion

        internal static bool IsValidDataSpace(DataSpace dataSpace)
        {
            return (DataSpace.OSpace == dataSpace ||
                    DataSpace.CSpace == dataSpace ||
                    DataSpace.SSpace == dataSpace);
        }

        internal static bool IsValidParameterName(string name)
        {
            return (!StringUtil.IsNullOrEmptyOrWhiteSpace(name) &&
                    _paramNameRegex.IsMatch(name));
        }
        private static readonly Regex _paramNameRegex = new Regex("^([A-Za-z])([A-Za-z0-9_])*$");
                
        #endregion
    }
}
