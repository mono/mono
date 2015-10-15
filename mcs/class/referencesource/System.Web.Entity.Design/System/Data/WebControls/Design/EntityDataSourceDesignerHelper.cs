//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceDesignerHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.EntityClient;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.Web.UI.Design.WebControls.Util;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml;

namespace System.Web.UI.Design.WebControls
{
    internal class EntityDataSourceDesignerHelper
    {
        #region Private and Internal static constants
        private static readonly string s_virtualRoot = "~/";
        private static readonly string s_ecmPublicKeyToken = "PublicKeyToken=" + AssemblyRef.EcmaPublicKey;        
        private static readonly string s_entityClientProviderName = "System.Data.EntityClient";
        private static readonly string s_metadataPathSeparator = "|";
        private static readonly string s_resPathPrefix = "res://";
        private static readonly string s_relativeParentFolder = "../";
        private static readonly string s_relativeCurrentFolder = "./";
        private static readonly string s_altRelativeParentFolder = @"..\";
        private static readonly string s_altRelativeCurrentFolder = @".\";
        private static readonly string s_dataDirectoryNoPipes = "DataDirectory";
        private static readonly string s_dataDirectory = "|DataDirectory|";
        private static readonly string s_dataDirectoryPath = String.Concat(s_virtualRoot, "app_data");
        private static readonly string s_resolvedResPathFormat = String.Concat(s_resPathPrefix, "{0}/{1}");
        private static readonly string DesignerStateDataSourceSchemaKey = "EntityDataSourceSchema";
        private static readonly string DesignerStateDataSourceConnectionStringKey = "EntityDataSourceConnectionString";
        private static readonly string DesignerStateDataSourceDefaultContainerNameKey = "EntityDataSourceDefaultContainerName";
        private static readonly string DesignerStateDataSourceEntitySetNameKey = "EntityDataSourceEntitySetNameKey";
        private static readonly string DesignerStateDataSourceSelectKey = "EntityDataSourceSelectKey";
        private static readonly string DesignerStateDataSourceCommandTextKey = "EntityDataSourceCommandTextKey";
        private static readonly string DesignerStateDataSourceEnableFlatteningKey = "EntityDataSourceEnableFlattening";
        
        internal static readonly string DefaultViewName = "DefaultView";
        #endregion

        #region Private instance fields
        private readonly EntityDataSource _entityDataSource;
        private EntityConnection _entityConnection;        
        private readonly IWebApplication _webApplication;
        // determines if any errors or warnings are displayed and if the EntityConnection and metadata are automatically loaded when accessed
        private bool _interactiveMode;        
        private HashSet<Assembly> _assemblies;
        private EntityDesignerDataSourceView _view;
        private bool _forceSchemaRetrieval;
        private readonly EntityDataSourceDesigner _owner;
        private bool _canLoadWebConfig;
        private bool _usingEntityFrameworkVersionHigherThanFive = false;
        #endregion
        
        internal EntityDataSourceDesignerHelper(EntityDataSource entityDataSource, bool interactiveMode)
        {
            Debug.Assert(entityDataSource != null, "null entityDataSource");

            _entityDataSource = entityDataSource;
            _interactiveMode = interactiveMode;

            _canLoadWebConfig = true;

            IServiceProvider serviceProvider = _entityDataSource.Site;
            if (serviceProvider != null)
            {
                // Get the designer instance associated with the specified data source control
                IDesignerHost designerHost = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
                if (designerHost != null)
                {
                    _owner = designerHost.GetDesigner(this.EntityDataSource) as EntityDataSourceDesigner;
                }

                // Get other services used to determine design-time information                
                _webApplication = serviceProvider.GetService(typeof(IWebApplication)) as IWebApplication;

            }            
            Debug.Assert(_owner != null, "expected non-null owner");            
            Debug.Assert(_webApplication != null, "expected non-null web application service");
        }

        internal void AddSystemWebEntityReference()
        {
            IServiceProvider serviceProvider = _entityDataSource.Site;
            if (serviceProvider != null)
            {
                ITypeResolutionService typeResProvider = (ITypeResolutionService)serviceProvider.GetService(typeof(ITypeResolutionService));
                if (typeResProvider != null)
                {
                    try
                    {
                        // Adding the reference using just the name and public key since we don't want to be
                        // tied to a particular version here.
                        typeResProvider.ReferenceAssembly(
                            new AssemblyName("System.Web.Entity,PublicKeyToken=" + AssemblyRef.EcmaPublicKey));
                    }
                    catch (FileNotFoundException)
                    {
                        Debug.Fail("Failed to find System.Web.Entity assembly.");
                        // Intentionally ignored exception - the assembly should always be
                        // found, but if it isn't, then we don't want to stop the rest of the
                        // control from working, especially since the assembly may not always
                        // be required.
                    }
                }
            }
        }

        #region Helpers for EntityDataSource properties   
        internal bool AutoGenerateWhereClause
        {
            get
            {
                return _entityDataSource.AutoGenerateWhereClause;
            }
        }

        internal bool AutoGenerateOrderByClause
        {
            get
            {
                return _entityDataSource.AutoGenerateOrderByClause;
            }
        }

        internal bool CanPage
        {
            get
            {
                DataSourceView view = ((IDataSource)_entityDataSource).GetView(DefaultViewName);
                if (view != null)
                {
                    return view.CanPage;
                }

                return false;
            }
        }

        internal bool CanSort
        {
            get
            {
                DataSourceView view = ((IDataSource)_entityDataSource).GetView(DefaultViewName);
                if (view != null)
                {
                    return view.CanSort;
                }

                return false;
            }
        }
        
        internal string ConnectionString
        {
            get
            {
                return _entityDataSource.ConnectionString;
            }
            set
            {
                if (value != ConnectionString)
                {
                    _entityDataSource.ConnectionString = value;
                    _owner.FireOnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        internal string CommandText
        {
            get
            {
                return _entityDataSource.CommandText;
            }
            set
            {
                if (value != CommandText)
                {
                    _entityDataSource.CommandText = value;
                    _owner.FireOnDataSourceChanged(EventArgs.Empty);
                }                
            }
        }

        internal ParameterCollection CommandParameters
        {
            get
            {
                return _entityDataSource.CommandParameters;
            }
        }

        internal string DefaultContainerName
        {
            get
            {
                return _entityDataSource.DefaultContainerName;
            }
            set
            {
                if (value != DefaultContainerName)
                {
                    _entityDataSource.DefaultContainerName = value;
                    _owner.FireOnDataSourceChanged(EventArgs.Empty);
                }                 
            }
        }

        internal bool EnableDelete
        {
            get
            {
                return _entityDataSource.EnableDelete;
            }
        }

        internal bool EnableInsert
        {
            get
            {
                return _entityDataSource.EnableInsert;
            }
        }

        internal bool EnableUpdate
        {
            get
            {
                return _entityDataSource.EnableUpdate;
            }
        }

        internal bool EnableFlattening
        {
            get
            {
                return _entityDataSource.EnableFlattening;
            }
        }

        internal string EntitySetName
        {
            get
            {
                return _entityDataSource.EntitySetName;
            }
            set
            {
                if (value != EntitySetName)
                {
                    _entityDataSource.EntitySetName = value;
                    _owner.FireOnDataSourceChanged(EventArgs.Empty);
                }                
            }
        }

        internal string EntityTypeFilter
        {
            get
            {
                return _entityDataSource.EntityTypeFilter;
            }
            set
            {
                if (value != EntityTypeFilter)
                {
                    _entityDataSource.EntityTypeFilter = value;
                    _owner.FireOnDataSourceChanged(EventArgs.Empty);
                }                 
            }
        }

        internal string GroupBy
        {
            get
            {
                return _entityDataSource.GroupBy;
            }
        }

        internal string OrderBy
        {
            get
            {
                return _entityDataSource.OrderBy;
            }
            set
            {
                if (value != OrderBy)
                {
                    _entityDataSource.OrderBy = value;
                    _owner.FireOnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        internal ParameterCollection OrderByParameters
        {
            get
            {
                return _entityDataSource.OrderByParameters;
            }
        }

        internal string Select
        {
            get
            {
                return _entityDataSource.Select;
            }
            set
            {
                if (value != Select)
                {
                    _entityDataSource.Select = value;
                    _owner.FireOnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        internal ParameterCollection SelectParameters
        {
            get
            {
                return _entityDataSource.SelectParameters;
            }
        }
        
        internal string Where
        {
            get
            {
                return _entityDataSource.Where;
            }
            set
            {
                if (value != Where)
                {
                    _entityDataSource.Where = value;
                    _owner.FireOnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        internal ParameterCollection WhereParameters
        {
            get
            {
                return _entityDataSource.WhereParameters;
            }
        }        
        #endregion

        private EntityDataSource EntityDataSource
        {
            get
            {
                return _entityDataSource;
            }
        }

        private EdmItemCollection EdmItemCollection
        {
            get
            {
                // In interactive mode, we will explicitly load metadata when needed, so never load it here
                // When not in interactive mode, we only need to load metadata once, which is determined by the presence of an EntityConnection
                if (!_interactiveMode && _entityConnection == null)
                {
                    LoadMetadata();
                }
                    
                // _entityConnection may still be null if the load failed or if we are in interactive mode and the metadata has not been explicitly loaded
                if (_entityConnection != null)
                {
                    ItemCollection itemCollection = null;

                    try
                    {
                        _entityConnection.GetMetadataWorkspace().TryGetItemCollection(DataSpace.CSpace, out itemCollection);
                    }
                    catch (Exception)
                    {
                        // Never expecting a failure because we have already initialized the workspace when the metadata was loaded,
                        // and any errors would have been trapped then. Just ignore any errors that might occur here to prevent a crash.
                    }

                    return itemCollection as EdmItemCollection; // not guaranteed not to be null, caller must check anyway before using
                }

                return null;
            }
        }

        // The default DesignerDataSourceView
        private EntityDesignerDataSourceView View
        {
            get
            {
                return _view;
            }
            set
            {
                _view = value;
            }
        }

        /// <summary>
        /// The status of loading the web.config file for named connections.
        /// This helps to determine if we've already tried to load the web.config file and if it failed
        /// we do not continue to load it again.
        /// </summary>
        internal bool CanLoadWebConfig
        {
            get
            {
                return _canLoadWebConfig;
            }
            set
            {
                _canLoadWebConfig = value;
            }
        }

        // Whether or not a schema retrieval is being forced (used in RefreshSchema)
        private bool ForceSchemaRetrieval
        {
            get
            {
                return _forceSchemaRetrieval;
            }
            set
            {
                _forceSchemaRetrieval = value;
            }
        }

        // Loads metadata for the current connection string on the data source control
        private bool LoadMetadata()
        {
            EntityConnectionStringBuilder connStrBuilder = VerifyConnectionString(this.EntityDataSource.ConnectionString, true /*allowNamedConnections*/);
            if (connStrBuilder != null)
            {
                return LoadMetadata(connStrBuilder);
            }
            // else the connection string could not be verified, and any errors are already displayed during the failed verification, so nothing more to do

            return false;
        }

        internal bool LoadMetadata(EntityConnectionStringBuilder connStrBuilder)
        {
            Debug.Assert(connStrBuilder != null, "expected non-null connStrBuilder");

            // if these services are not available for some reason, we will not be able to do anything useful, so don't try to load metadata
            if (_webApplication != null)
            {
                // _assemblies could already be loaded if this call is coming from the wizard, because metadata could be loaded
                // multiple times if the connection string is changed, so don't load it again if we already have it. It can't have 
                // changed since the last load, because the wizard dialog is modal and there is no way to have changed the project between loads.
                if (_assemblies == null)
                {
                    LoadAssemblies();
                }

                return LoadMetadataFromBuilder(connStrBuilder);                
            }

            return false;
        }
        
        // Loads C-Space metadata from the specified connection string builder.
        // Expects that the specified builder has already been verified and has minimum required properties
        private bool LoadMetadataFromBuilder(EntityConnectionStringBuilder connStrBuilder)
        {
            Debug.Assert(connStrBuilder != null, "expected non-null connStrBuilder");

            // We will be replacing the metadata with a new collection, and if something fails to load we want to make sure to clear out any existing data
            ClearMetadata();

            if (String.IsNullOrEmpty(connStrBuilder.ConnectionString))
            {
                // Although we can't load metadata here, this is not an error because the user should not expect an empty connection string
                // to produce any metadata, so it will not be confusing when no values are available in the dropdowns. This is different from
                // an invalid connection string because in that case they have entered a value and are expecting to get metadata from it. 
                return false;
            }

            // If this is a named connection, load the contents of the named connection from the web.config and verify itet
            if (!String.IsNullOrEmpty(connStrBuilder.Name))
            {
                connStrBuilder = GetBuilderForNamedConnection(connStrBuilder);
                if (connStrBuilder == null)
                {
                    // some verification failed when getting the connection string builder,
                    // so we have nothing to load metadata from, just return
                    return false;
                }
            }

            string originalMetadata = connStrBuilder.Metadata;
            Debug.Assert(!String.IsNullOrEmpty(originalMetadata), "originalMetadata should have aleady been verified to be non-null or empty");

            List<string> metadataWarnings = new List<string>(); // keeps track of all warnings that happen during the parsing of the connection string
            List<string> metadataPaths = new List<string>();    // collection of resolved paths

            // We need to use the | separator to split the metadata value into individual paths, so first remove and process any paths
            // containing the macro |DataDirectory|. These will get combined again with the rest of the paths once they have been processed.
            List<string> dataDirectoryPaths = new List<string>();
            string metadataWithoutDataDirectory = ResolveDataDirectory(originalMetadata, dataDirectoryPaths, metadataWarnings);

            foreach (string path in metadataWithoutDataDirectory.Split(new string[] { s_metadataPathSeparator }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmedPath = path.Trim();
                if (!String.IsNullOrEmpty(trimmedPath))
                {
                    if (trimmedPath.StartsWith(s_virtualRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        // ~/
                        ResolveVirtualRootPath(trimmedPath, metadataPaths, metadataWarnings);
                    }
                    else if (trimmedPath.StartsWith(s_relativeCurrentFolder, StringComparison.OrdinalIgnoreCase) ||
                        trimmedPath.StartsWith(s_altRelativeCurrentFolder, StringComparison.OrdinalIgnoreCase) ||
                        trimmedPath.StartsWith(s_relativeParentFolder, StringComparison.OrdinalIgnoreCase) ||
                        trimmedPath.StartsWith(s_altRelativeParentFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        // ../, ..\, ./, or ..\
                        ResolveRelativePath(trimmedPath, metadataPaths, metadataWarnings);
                    }
                    else
                    {
                        // We are not trying to resolve any other types of paths, so just pass it along directly.
                        // If the format of the path is unrecognized, or if the path is not valid, metadata will throw an exception that will
                        // be displayed to the user at that time.
                        metadataPaths.Add(trimmedPath);
                    }                       
                }
            }

            // Add the paths with |DataDirectory| back to the list
            if (dataDirectoryPaths.Count > 0)
            {
                metadataPaths.AddRange(dataDirectoryPaths);
            }

            if (metadataWarnings.Count > 0)
            {
                ShowWarning(BuildWarningMessage(Strings.Warning_ConnectionStringMessageHeader, metadataWarnings));
            }

            return SetEntityConnection(metadataPaths, connStrBuilder);            
        }

        private bool SetEntityConnection(List<string> metadataPaths, EntityConnectionStringBuilder connStrBuilder)
        {
            // It's possible the metadata was specified in the original connection string, but we filtered out everything due to not being able to resolve it to anything.
            // In that case, warnings have already been displayed to indicate which paths were removed, so no need to display another message.            
            if (metadataPaths.Count > 0)
            {
                try
                {
                    // Get the connection first, because it might be needed to gather provider services information
                    DbConnection dbConnection = GetDbConnection(connStrBuilder);

                    MetadataWorkspace metadataWorkspace = new MetadataWorkspace(metadataPaths, _assemblies);

                    // Ensure that we have all of the item collections registered. If some of them are missing this will cause problems eventually if we need to 
                    // execute a query to get detailed schema information, but that will be handled later. For now just register everything to prevent errors in the 
                    // stack that would not be understood by the user in the designer at this point.
                    ItemCollection edmItemCollection;
                    ItemCollection storeItemCollection;
                    ItemCollection csItemCollection;
                    if (!metadataWorkspace.TryGetItemCollection(DataSpace.CSpace, out edmItemCollection))
                    {
                        edmItemCollection = new EdmItemCollection();
                        metadataWorkspace.RegisterItemCollection(edmItemCollection);
                    }

                    if (!metadataWorkspace.TryGetItemCollection(DataSpace.SSpace, out storeItemCollection))
                    {
                        return false;
                    }

                    if (!metadataWorkspace.TryGetItemCollection(DataSpace.CSSpace, out csItemCollection))
                    {
                        Debug.Assert(edmItemCollection != null && storeItemCollection != null, "edm and store ItemCollection should be populated already");
                        metadataWorkspace.RegisterItemCollection(new StorageMappingItemCollection(edmItemCollection as EdmItemCollection, storeItemCollection as StoreItemCollection));
                    }
                    
                    // Create an ObjectItemCollection beforehand so that we can load objects by-convention
                    metadataWorkspace.RegisterItemCollection(new ObjectItemCollection());

                    // Load OSpace metadata from all of the assemblies we know about
                    foreach (Assembly assembly in _assemblies)
                    {
                        metadataWorkspace.LoadFromAssembly(assembly);
                    }
                    
                    if (dbConnection != null)
                    {
                        _entityConnection = new EntityConnection(metadataWorkspace, dbConnection);
                        return true;
                    }
                    // else the DbConnection could not be created and the error should have already been displayed
                }
                catch (Exception ex)
                {   
                    StringBuilder exceptionMessage = new StringBuilder();
                    if (_usingEntityFrameworkVersionHigherThanFive)
                    {
                        exceptionMessage.Append(Strings.Error_UnsupportedVersionOfEntityFramework);
                    }
                    else
                    {
                        exceptionMessage.AppendLine(Strings.Error_MetadataLoadError);
                        exceptionMessage.AppendLine();
                        exceptionMessage.Append(ex.Message);
                    }
                    ShowError(exceptionMessage.ToString());
                }
            }

            return false;
        }

        // Clears out the existing metadata
        internal void ClearMetadata()
        {
            _entityConnection = null;            
        }

        /// <summary>
        /// Finds and caches all non system assemblies that are found using the TypeDiscoveryService
        /// This searches places like the ~/bin folder, app_code, and the 'assemblies' section of web.config, among others
        /// </summary>
        private void LoadAssemblies()
        {
            _assemblies = new HashSet<Assembly>();

            // Find the assemblies using the ITypeDiscoveryService
            ITypeDiscoveryService typeDiscoverySvc = this.EntityDataSource.Site.GetService(typeof(ITypeDiscoveryService)) as ITypeDiscoveryService;
            if (typeDiscoverySvc != null)
            {
                foreach (Type type in typeDiscoverySvc.GetTypes(typeof(object), false /*excludeGlobalTypes*/))
                {
                    var assembly = type.Assembly;
                    if (!_usingEntityFrameworkVersionHigherThanFive
                            && assembly.GetName().Name.Equals("EntityFramework", StringComparison.InvariantCultureIgnoreCase)
                            && assembly.GetName().Version.Major > 5)
                    {
                        _usingEntityFrameworkVersionHigherThanFive = true;
                        ShowError(Strings.Error_UnsupportedVersionOfEntityFramework);
                    }
                    if (!_assemblies.Contains(assembly) && !IsSystemAssembly(assembly.FullName))
                    {
                        _assemblies.Add(assembly);
                    }
                }
            }
        }

        // Explicitly rebuild the known assembly cache. This is done when launching the wizard and allows the wizard to pick up the latest
        // assemblies in the project, without having to reload them everytime the connection string changes while the wizard is running
        internal void ReloadResources()
        {
            Debug.Assert(_interactiveMode == true, "resource cache should only explicitly be loaded in interactive mode");
            LoadAssemblies();
        }

        // Is the assembly and its referenced assemblies not expected to have any metadata
        // This does not detect all possible system assemblies, but just those we can detect as system for sure
        private static bool IsSystemAssembly(string fullName)
        {
            return (String.Equals(fullName, "*", StringComparison.OrdinalIgnoreCase) ||
                fullName.EndsWith(s_ecmPublicKeyToken, StringComparison.OrdinalIgnoreCase));
        }

        // Combines all warnings into one message
        private string BuildWarningMessage(string headerMessage, List<string> warnings)
        {
            Debug.Assert(warnings != null && warnings.Count > 0, "expected non-null and non-empty warnings");

            StringBuilder warningMessage = new StringBuilder();
            warningMessage.AppendLine(headerMessage);
            warningMessage.AppendLine();
            foreach (string warning in warnings)
            {
                warningMessage.AppendLine(warning);
            }

            return warningMessage.ToString();
        }

        // Get a connection string builder for the specified connection string, and do some basic verification
        // namedConnStrBuilder should be based on a named connection and should already have been verified to be structurally valid
        private EntityConnectionStringBuilder GetBuilderForNamedConnection(EntityConnectionStringBuilder namedConnStrBuilder)
        {
            Debug.Assert(namedConnStrBuilder != null && !String.IsNullOrEmpty(namedConnStrBuilder.Name), "expected non-null connStrBuilder for a named connection");

            // Need to get the actual string from the web.config
            EntityConnectionStringBuilder connStrBuilder = null;

            if (CanLoadWebConfig)
            {
                try
                {
                    System.Configuration.Configuration webConfig = _webApplication.OpenWebConfiguration(true /*isReadOnly*/);
                    if (webConfig != null)
                    {
                        ConnectionStringSettings connStrSettings = webConfig.ConnectionStrings.ConnectionStrings[namedConnStrBuilder.Name];
                        if (connStrSettings != null && !String.IsNullOrEmpty(connStrSettings.ConnectionString) && connStrSettings.ProviderName == s_entityClientProviderName)
                        {
                            // Verify the contents of the named connection and create a new builder from it
                            // It can't reference another named connection, and must have both the provider and metadata keywords
                            connStrBuilder = VerifyConnectionString(connStrSettings.ConnectionString, false /*allowNamedConnections*/);
                        }
                        else
                        {
                            ShowError(Strings.Error_NamedConnectionNotFound);
                        }
                    }
                    else
                    {
                        ShowError(Strings.Error_CannotOpenWebConfig_SpecificConnection);
                    }
                }
                catch (ConfigurationException ce)
                {
                    StringBuilder error = new StringBuilder();
                    error.AppendLine(Strings.Error_CannotOpenWebConfig_SpecificConnection);
                    error.AppendLine();
                    error.AppendLine(ce.Message);
                    ShowError(error.ToString());
                }
            }

            // could be null if verification failed
            return connStrBuilder;
        }

        // Make sure we have at least some basic keywords.This method does not attempt to do as much verification as EntityClient would do.
        // We are just looking for a named connection, or both the provider and metadata keywords.
        /// <summary>
        /// Make sure we have at least some basic keywords.This method does not attempt to do as much verification as EntityClient would do.
        /// We are just looking for a named connection, or both the provider and metadata keywords.        /// 
        /// </summary>
        /// <param name="connectionString">Connection string to be verified. Can be empty or null.</param>
        /// <param name="allowNamedConnections">
        /// Indicates if the specified string can be a named connection in the form "name=ConnectionName".
        /// If this method is being called with a connection string that came from a named connection entry in the web.config, this should be false
        /// because we do not support nested named connections.
        /// </param>
        /// <returns>
        /// A new EntityConnectionStringBuilder if the basic verification succeeded, otherwise null. Can return a builder for an empty string.
        /// </returns>
        private EntityConnectionStringBuilder VerifyConnectionString(string connectionString, bool allowNamedConnections)
        {
            // Verify if we have a structurally valid connection string with both "provider" and "metadata" keywords
            EntityConnectionStringBuilder connStrBuilder = null;

            try
            {
                // Verify that it can be loaded into the builder to ensure basic valid structure and keywords               
                connStrBuilder = new EntityConnectionStringBuilder(connectionString);
            }
            catch (ArgumentException ex)
            {
                // The message thrown from the connection string builder is not always useful to the user in this context, so add our own error text as well
                ShowError(Strings.Error_CreatingConnectionStringBuilder(ex.Message));
                return null;
            }
            Debug.Assert(connStrBuilder != null, "expected non-null connStrBuilder");

            // If the connection string is not empty, do some validation on it
            //     (a) If this is not supposed to be a named connection, make sure it isn't
            //     (b) Then it's not a named connection, then verify the keywords
            //     (c) Otherwise the connection string is a named connection, no further validation is needed

            // devnote: Using the ConnectionString property on the builder in the check for empty, because the original connection string
            //          could have been something like "name=", which produces an empty ConnectionString in the builder, although the original was not empty
            if (!String.IsNullOrEmpty(connStrBuilder.ConnectionString))
            {
                // If named connection is not allowed, make sure it is not specified
                if (!allowNamedConnections && !String.IsNullOrEmpty(connStrBuilder.Name))
                {
                    ShowError(Strings.Error_NestedNamedConnection);
                    return null;
                }

                // If the connection string is not a named connection, verify the keywords
                if (String.IsNullOrEmpty(connStrBuilder.Name))
                {
                    if (String.IsNullOrEmpty(connStrBuilder.Metadata))
                    {
                        ShowError(Strings.Error_MissingMetadataKeyword);
                        return null;
                    }
                }
                // else it's a named connection and we don't need to validate it further
            }

            return connStrBuilder;
        }

        internal void ShowError(string message)
        {
            if (_interactiveMode)
            {
                UIHelper.ShowError(EntityDataSource.Site, message);
            }
            // else we are in a mode where we just want to ignore errors (typically this happens when called from the property grid)
        }

        internal void ShowWarning(string message)
        {
            if (_interactiveMode)
            {
                UIHelper.ShowWarning(EntityDataSource.Site, message);
            }
            // else we are in a mode where we just want to ignore warnings (typically this happens when called from the property grid)
        }

        // Removes any paths containing |DataDirectory| from a string of metadata locations, adds them to a separate list and expands
        // the macro to the full path to ~/ for any paths that start with the macro
        private string ResolveDataDirectory(string metadataPaths, List<string> dataDirectoryPaths, List<string> warnings)
        {
            Debug.Assert(dataDirectoryPaths != null, "null dataDirectoryPaths");

            // If the argument contains one or more occurrences of the macro '|DataDirectory|', we
            // pull those paths out so that we don't lose them in the string-splitting logic below.
            // Note that the macro '|DataDirectory|' cannot have any whitespace between the pipe 
            // symbols and the macro name. Also note that the macro must appear at the beginning of 
            // a path (else we will eventually fail with an invalid path exception, because in that
            // case the macro is not expanded). If a real/physical folder named 'DataDirectory' needs
            // to be included in the metadata path, whitespace should be used on either or both sides
            // of the name.
            //
            int indexStart = metadataPaths.IndexOf(s_dataDirectory, StringComparison.OrdinalIgnoreCase);
            while (indexStart != -1)
            {
                int prevSeparatorIndex = indexStart == 0 ? -1 : metadataPaths.LastIndexOf(
                                                                s_metadataPathSeparator,
                                                                indexStart - 1, // start looking here
                                                                StringComparison.Ordinal
                                                            );

                int macroPathBeginIndex = prevSeparatorIndex + 1;

                // The '|DataDirectory|' macro is composable, so identify the complete path, like
                // '|DataDirectory|\item1\item2'. If the macro appears anywhere other than at the
                // beginning, splice out the entire path, e.g. 'C:\item1\|DataDirectory|\item2'. In this
                // latter case the macro will not be expanded, and downstream code will throw an exception.
                //
                int indexEnd = metadataPaths.IndexOf(s_metadataPathSeparator,
                                             indexStart + s_dataDirectory.Length,
                                             StringComparison.Ordinal);
                string resolvedPath;
                if (indexEnd == -1)
                {
                    resolvedPath = ExpandDataDirectory(metadataPaths.Substring(macroPathBeginIndex), warnings);
                    if (resolvedPath != null)
                    {
                        // only add to the list if no warning occurred
                        dataDirectoryPaths.Add(resolvedPath);
                    }
                    metadataPaths = metadataPaths.Remove(macroPathBeginIndex);   // update the concatenated list of paths
                    break;
                }

                resolvedPath = ExpandDataDirectory(metadataPaths.Substring(macroPathBeginIndex, indexEnd - macroPathBeginIndex), warnings);
                if (resolvedPath != null)
                {
                    // only add to the list if no warning occurred
                    dataDirectoryPaths.Add(resolvedPath);
                }
                
                // Update the concatenated list of paths by removing the one containing the macro.
                //
                metadataPaths = metadataPaths.Remove(macroPathBeginIndex, indexEnd - macroPathBeginIndex);
                indexStart = metadataPaths.IndexOf(s_dataDirectory, StringComparison.OrdinalIgnoreCase);
            }

            return metadataPaths;
        }

        // If the specified string starts with |DataDirectory|, replace that macro with the full path for ~/app_data in the application
        private string ExpandDataDirectory(string pathWithMacro, List<string> warnings)
        {
            string trimmedPath = pathWithMacro.Trim();
            if (trimmedPath.StartsWith(s_dataDirectory, StringComparison.OrdinalIgnoreCase))
            {
                string dataDirectoryPath = GetDataDirectory();
                if (dataDirectoryPath != null)
                {
                    return String.Concat(dataDirectoryPath, trimmedPath.Substring(s_dataDirectory.Length));
                }
                else
                {
                    warnings.Add(Strings.Warning_DataDirectoryNotFound(trimmedPath));
                    return null;
                }
            }
            // else the macro is somewhere in the middle of the string which is not valid anyway, so just pass it along and let the metadata failure occur
            
            return trimmedPath;
        }

        /// <summary>
        /// Resolves the |DataDirecotry| macro from the current web application
        /// </summary>
        /// <returns>The physical path for the macro expansion, or null if the data directory could not be found</returns>
        private string GetDataDirectory()
        {
            IProjectItem dataDirectoryPath = _webApplication.GetProjectItemFromUrl(s_dataDirectoryPath);
            if (dataDirectoryPath != null)
            {
                return dataDirectoryPath.PhysicalPath;
            }
            else
            {
                return null;
            }
        }

        private void ResolveVirtualRootPath(string resourcePath, List<string> metadataPaths, List<string> warnings)
        {
            IProjectItem rootItem = _webApplication.GetProjectItemFromUrl(s_virtualRoot);
            if (rootItem != null)
            {
                metadataPaths.Add(String.Concat(rootItem.PhysicalPath, resourcePath.Substring(s_virtualRoot.Length)));
            }
            else
            {
                warnings.Add(Strings.Warning_VirtualRootNotFound(resourcePath));
            }
        }

        private void ResolveRelativePath(string resourcePath, List<string> metadataPaths, List<string> warnings)
        {
            IProjectItem rootItem = _webApplication.GetProjectItemFromUrl(s_virtualRoot);
            if (rootItem != null)
            {
                metadataPaths.Add(String.Concat(rootItem.PhysicalPath, resourcePath));
            }
            else
            {
                warnings.Add(Strings.Warning_VirtualRootNotFound(resourcePath));
            }
        }

        // Create a DbConnection for the specified connection string
        private DbConnection GetDbConnection(EntityConnectionStringBuilder connStrBuilder)
        {
            DbProviderFactory factory = null;
            if (!string.IsNullOrEmpty(connStrBuilder.Provider))
            {
                try
                {
                    // Get the correct provider factory
                    factory = DbProviderFactories.GetFactory(connStrBuilder.Provider);
                }
                catch (Exception ex)
                {
                    ShowError(Strings.Error_CannotCreateDbProviderFactory(ex.Message));
                }
            }
            else
            {
                ShowError(Strings.Error_CannotCreateDbProviderFactory(Strings.Error_MissingProviderKeyword));
            }

            if (factory != null)
            {
                try
                {
                    
                    // Create the underlying provider specific connection and give it the specified provider connection string
                    DbConnection storeConnection = factory.CreateConnection();
                    if (storeConnection != null)
                    {   
                        storeConnection.ConnectionString = connStrBuilder.ProviderConnectionString;
                        return storeConnection;
                    }
                }                
                catch (Exception)
                {
                    // eat any exceptions and just show the general error below
                }

                ShowError(Strings.Error_ReturnedNullOnProviderMethod(factory.GetType().Name));
            }
            return null;
        }        

        internal void RefreshSchema(bool preferSilent)
        {
            string originalDataDirectory = null;
            try
            {
                _owner.SuppressDataSourceEvents();
                Cursor originalCursor = Cursor.Current;

                // Make sure we have set the |DataDirectory| field in the AppDomain so that the underlying providers
                // can make use of this macro
                originalDataDirectory = AppDomain.CurrentDomain.GetData(s_dataDirectoryNoPipes) as string;
                AppDomain.CurrentDomain.SetData(s_dataDirectoryNoPipes, GetDataDirectory());

                // Verify that we can get the current schema
                DataTable currentSchema = GetCurrentSchema(preferSilent);
                if (currentSchema == null)
                {
                    // error occurred when getting current schema
                    return;
                }

                try
                {
                    Cursor.Current = Cursors.WaitCursor;                    

                    EntityDesignerDataSourceView view = GetView(DefaultViewName);
                    IDataSourceViewSchema oldViewSchema = view.Schema;
                    bool wasForceUsed = false;
                    if (oldViewSchema == null)
                    {
                        ForceSchemaRetrieval = true;
                        oldViewSchema = view.Schema;
                        ForceSchemaRetrieval = false;
                        wasForceUsed = true;
                    }

                    SaveSchema(this.ConnectionString, this.DefaultContainerName, this.EntitySetName, this.Select, this.CommandText, this.EnableFlattening, currentSchema);

                    // Compare new schema to old schema and if it changed, raise the SchemaRefreshed event
                    bool viewSchemaEquivalent = _owner.InternalViewSchemasEquivalent(oldViewSchema, view.Schema);
                    if (!viewSchemaEquivalent)
                    {
                        _owner.FireOnSchemaRefreshed(EventArgs.Empty);                        
                    }
                    else if (wasForceUsed)
                    {
                        // if the schemas were equivalent but the schema retrieval was forced, still raise the data source changed event
                        _owner.FireOnDataSourceChanged(EventArgs.Empty);
                    }
                }
                finally
                {
                    Cursor.Current = originalCursor;
                }            
            }
            finally
            {
                // Reset the AppDomain to its original |DataDirectory| value
                AppDomain.CurrentDomain.SetData(s_dataDirectoryNoPipes, originalDataDirectory);
                _owner.ResumeDataSourceEvents();
            }
        }

        private DataTable GetCurrentSchema(bool preferSilent)
        {
            // Verify that we have values for a minimum set of properties that will be required to get schema
            if (String.IsNullOrEmpty(this.EntityDataSource.ConnectionString) ||
                String.IsNullOrEmpty(this.EntityDataSource.DefaultContainerName) || 
                String.IsNullOrEmpty(this.EntityDataSource.CommandText) && String.IsNullOrEmpty(this.EntityDataSource.EntitySetName))
            {
                if (!preferSilent)
                {
                    ShowError(Strings.Error_CannotRefreshSchema_MissingProperties);
                }

                return null;
            }

            bool originalMode = _interactiveMode;
            try
            {
                // Suppress error messages while loading metadata if we are in silent mode
                _interactiveMode = !preferSilent;

                // In interactive mode, always clear any cached information so we are sure to get the latest schema
                // This is necessary in case the metadata or entity classes in referenced assemblies has changed
                // or in case the metadata files have changed without changing any of the properties on the control
                if (_interactiveMode)
                {
                    ReloadResources();
                    ClearMetadata();
                }

                // Try to load metadata if we don't have it yet
                if (_entityConnection == null)
                {
                    if (!LoadMetadata())
                    {
                        return null;
                    }
                    // else metadata was successfully loaded, so continue refreshing schema
                }
            }
            finally
            {
                _interactiveMode = originalMode;
            }

            // Either _entityConnection was already set, or we should have successfully loaded it
            Debug.Assert(_entityConnection != null, "_entityConnection should have been initialized");

            try
            {
                // Create a temporary data source based on the EntityConnection we have built with
                // the right metadata from the design-time environment
                EntityDataSource entityDataSource = new EntityDataSource(_entityConnection);

                // This is workaround for a 





                DbProviderServices.GetProviderServices(_entityConnection.StoreConnection).GetProviderManifestToken(_entityConnection.StoreConnection);
                
                // Copy only the properties that can affect the schema
                entityDataSource.CommandText = this.EntityDataSource.CommandText;
                CopyParameters(this.EntityDataSource.CommandParameters, entityDataSource.CommandParameters);
                entityDataSource.DefaultContainerName = this.EntityDataSource.DefaultContainerName;
                entityDataSource.EntitySetName = this.EntityDataSource.EntitySetName;
                entityDataSource.EntityTypeFilter = this.EntityDataSource.EntityTypeFilter;
                entityDataSource.GroupBy = this.EntityDataSource.GroupBy;
                entityDataSource.Select = this.EntityDataSource.Select;
                entityDataSource.EnableFlattening = this.EntityDataSource.EnableFlattening;
                CopyParameters(this.EntityDataSource.SelectParameters, entityDataSource.SelectParameters);

                EntityDataSourceView view = (EntityDataSourceView)(((IDataSource)entityDataSource).GetView(DefaultViewName));
                DataTable viewTable = view.GetViewSchema();
                viewTable.TableName = DefaultViewName;
                return viewTable;
            }
            catch (Exception ex)
            {
                if (!preferSilent)
                {
                    StringBuilder errorMessage = new StringBuilder();
                    errorMessage.AppendLine(Strings.Error_CannotRefreshSchema_RuntimeException(ex.Message));
                    if (ex.InnerException != null)
                    {
                        errorMessage.AppendLine(Strings.Error_CannotRefreshSchema_RuntimeException_InnerException(ex.InnerException.Message));
                    }

                    ShowError(errorMessage.ToString());
                }
            }

            return null;
        }

        private void CopyParameters(ParameterCollection originalParameters, ParameterCollection newParameters)
        {
            Debug.Assert(originalParameters != null && newParameters != null, "parameter collections on the data source should never be null");
            Debug.Assert(newParameters.Count == 0, "new parameter collection should not contain any parameters yet");

            _owner.CloneParameters(originalParameters, newParameters);
        }

        // Loads the schema
        internal DataTable LoadSchema()
        {
            if (!ForceSchemaRetrieval)
            {
                // Only check for consistency if we are not forcing the retrieval
                string connectionString = _owner.LoadFromDesignerState(DesignerStateDataSourceConnectionStringKey) as string;
                string defaultContainerName = _owner.LoadFromDesignerState(DesignerStateDataSourceDefaultContainerNameKey) as string;
                string entitySetName = _owner.LoadFromDesignerState(DesignerStateDataSourceEntitySetNameKey) as string;
                string select = _owner.LoadFromDesignerState(DesignerStateDataSourceSelectKey) as string;
                string commandText = _owner.LoadFromDesignerState(DesignerStateDataSourceCommandTextKey) as string;
                object enableFlattening = _owner.LoadFromDesignerState(DesignerStateDataSourceEnableFlatteningKey);
                
                if (!String.Equals(connectionString, this.ConnectionString, StringComparison.OrdinalIgnoreCase) ||
                    !String.Equals(defaultContainerName, this.DefaultContainerName, StringComparison.OrdinalIgnoreCase) ||
                    !String.Equals(entitySetName, this.EntitySetName, StringComparison.OrdinalIgnoreCase) ||
                    !String.Equals(select, this.Select, StringComparison.OrdinalIgnoreCase) ||
                    !String.Equals(commandText, this.CommandText, StringComparison.OrdinalIgnoreCase) ||
                    (enableFlattening == null || (((bool)enableFlattening) != this.EnableFlattening)))
                {
                    return null;
                }
            }

            // Either we are forcing schema retrieval, or we're not forcing but we're consistent, so get the schema
            DataTable schema = _owner.LoadFromDesignerState(DesignerStateDataSourceSchemaKey) as DataTable;
            return schema;
        }

        private void SaveSchema(string connectionString, string defaultContainerName, string entitySetName,
            string select, string commandText, bool enableFlattening, DataTable currentSchema)
        {
            // Save the schema to DesignerState
            _owner.SaveDesignerState(DesignerStateDataSourceConnectionStringKey, connectionString);
            _owner.SaveDesignerState(DesignerStateDataSourceDefaultContainerNameKey, defaultContainerName);
            _owner.SaveDesignerState(DesignerStateDataSourceEntitySetNameKey, entitySetName);
            _owner.SaveDesignerState(DesignerStateDataSourceSelectKey, select);
            _owner.SaveDesignerState(DesignerStateDataSourceCommandTextKey, commandText);
            _owner.SaveDesignerState(DesignerStateDataSourceEnableFlatteningKey, enableFlattening);
            _owner.SaveDesignerState(DesignerStateDataSourceSchemaKey, currentSchema);
        }

        // Gets a view (can only get the default view)
        internal EntityDesignerDataSourceView GetView(string viewName)
        {
            if (String.IsNullOrEmpty(viewName) ||
                String.Equals(viewName, DefaultViewName, StringComparison.OrdinalIgnoreCase))
            {
                if (View == null)
                {
                    View = new EntityDesignerDataSourceView(_owner);   
                }
                return View;
            }
            return null;
        }

        // Gets a list of view names
        internal string[] GetViewNames()
        {
            return new string[] { DefaultViewName };
        }

        // Caller can specify that the results should not be sorted if they may add something to the list and sort themselves
        internal List<EntityDataSourceContainerNameItem> GetContainerNames(bool sortResults)
        {
            List<EntityDataSourceContainerNameItem> entityContainerItems = new List<EntityDataSourceContainerNameItem>();
            if (this.EdmItemCollection != null)
            {
                ReadOnlyCollection<EntityContainer> entityContainers = this.EdmItemCollection.GetItems<EntityContainer>();
                foreach (EntityContainer entityContainer in entityContainers)
                {
                    entityContainerItems.Add(new EntityDataSourceContainerNameItem(entityContainer));
                }

                if (sortResults)
                {
                    entityContainerItems.Sort();
                }
            }
            return entityContainerItems;
        }

        internal EntityDataSourceContainerNameItem GetEntityContainerItem(string entityContainerName)
        {
            if (String.IsNullOrEmpty(entityContainerName))
            {
                return null; // can't make a valid wrapper with an empty container name
            }

            EntityContainer container = null;
            if (this.EdmItemCollection != null &&
                this.EdmItemCollection.TryGetEntityContainer(entityContainerName, true /*ignoreCase*/, out container) &&
                container != null)
            {
                return new EntityDataSourceContainerNameItem(container);
            }
            else
            {
                return new EntityDataSourceContainerNameItem(entityContainerName);
            }
        }

        internal List<EntityDataSourceEntitySetNameItem> GetEntitySets(string entityContainerName)
        {
            EntityContainer container = null;
            if (this.EdmItemCollection != null)
            {
                this.EdmItemCollection.TryGetEntityContainer(entityContainerName, true /*ignoreCase*/, out container);
            }
            return GetEntitySets(container, true /*sortResults*/);
        }

        // Caller can specify that the results should not be sorted if they may add something to the list and sort themselves
        internal List<EntityDataSourceEntitySetNameItem> GetEntitySets(EntityContainer entityContainer, bool sortResults)
        {
            List<EntityDataSourceEntitySetNameItem> entitySetNameItems = new List<EntityDataSourceEntitySetNameItem>();
            if (entityContainer != null)
            {
                foreach (EntitySetBase entitySetBase in entityContainer.BaseEntitySets)
                {
                    // BaseEntitySets returns RelationshipSets too, but we only want EntitySets
                    if (entitySetBase.BuiltInTypeKind == BuiltInTypeKind.EntitySet)
                    {
                        entitySetNameItems.Add(new EntityDataSourceEntitySetNameItem(entitySetBase as EntitySet));
                    }
                }

                if (sortResults)
                {
                    entitySetNameItems.Sort();
                }
            }
            return entitySetNameItems;
        }

        // Caller can specify that the results should not be sorted if they may add something to the list and sort themselves
        internal List<EntityConnectionStringBuilderItem> GetNamedEntityClientConnections(bool sortResults)
        {
            List<EntityConnectionStringBuilderItem> namedEntityClientConnections = new List<EntityConnectionStringBuilderItem>();
            
            System.Configuration.Configuration webConfig = _webApplication.OpenWebConfiguration(true /*isReadOnly*/);
            if (webConfig != null)
            {
                try
                {
                    foreach (ConnectionStringSettings connStrSettings in webConfig.ConnectionStrings.ConnectionStrings)
                    {
                        if (connStrSettings.ProviderName == s_entityClientProviderName)
                        {
                            EntityConnectionStringBuilder connStrBuilder = new EntityConnectionStringBuilder();
                            connStrBuilder.Name = connStrSettings.Name;
                            namedEntityClientConnections.Add(new EntityConnectionStringBuilderItem(connStrBuilder));
                        }
                    }
                    
                    if (sortResults)
                    {
                        namedEntityClientConnections.Sort();
                    }
                }
                catch (ConfigurationException ce)
                {
                    CanLoadWebConfig = false;
                    namedEntityClientConnections.Clear();
                    StringBuilder error = new StringBuilder();
                    error.AppendLine(Strings.Warning_CannotOpenWebConfig_AllConnections);
                    error.AppendLine();
                    error.AppendLine(ce.Message);
                    ShowWarning(error.ToString());
                }
            }
            else
            {
                ShowWarning(Strings.Warning_CannotOpenWebConfig_AllConnections);
             }
            
            return namedEntityClientConnections;
        }

        internal EntityConnectionStringBuilderItem GetEntityConnectionStringBuilderItem(string connectionString)
        {
            EntityConnectionStringBuilder connStrBuilder = VerifyConnectionString(connectionString, true /*allowNamedConnections*/);
            if (connStrBuilder != null)
            {
                return new EntityConnectionStringBuilderItem(connStrBuilder);
            }
            else
            {
                return new EntityConnectionStringBuilderItem(connectionString);
            }
        }

        internal List<string> GetEntityTypeProperties(EntityType entityType)
        {
            List<string> properties = new List<string>();
            foreach (EdmProperty property in entityType.Properties)
            {
                properties.Add(property.Name);
            }

            Debug.Assert(properties.Count > 0, "expected entity to have at least one property");

            // don't sort the properties here because it will cause them to be displayed to the user in a non-intuitive order

            return properties;
        }

        internal List<EntityDataSourceEntityTypeFilterItem> GetEntityTypeFilters(string entityContainerName, string entitySetName)
        {
            EntityType baseEntitySetType = null;
            if (this.EdmItemCollection != null)
            {
                EntityContainer container;
                if (this.EdmItemCollection.TryGetEntityContainer(entityContainerName, true /*ignoreCase*/, out container) && (container != null))                
                {
                    EntitySet entitySet;
                    if (container.TryGetEntitySetByName(entitySetName, true /*ignoreCase*/, out entitySet) && entitySet != null)
                    {
                        baseEntitySetType = entitySet.ElementType;
                    }
                }
            }
            return GetEntityTypeFilters(baseEntitySetType, true /*sortResults*/);
        }

        internal List<EntityDataSourceEntityTypeFilterItem> GetEntityTypeFilters(EntityType baseEntitySetType, bool sortResults)
        {
            List<EntityDataSourceEntityTypeFilterItem> derivedTypes = new List<EntityDataSourceEntityTypeFilterItem>();

            if (baseEntitySetType != null && this.EdmItemCollection != null)
            {
                foreach (EntityType entityType in GetTypeAndSubtypesOf(baseEntitySetType, this.EdmItemCollection))
                {
                    derivedTypes.Add(new EntityDataSourceEntityTypeFilterItem(entityType));
                }

                if (sortResults)
                {
                    derivedTypes.Sort();
                }
            }
            return derivedTypes;
        }

        #region Helper methods for finding possible types for EntityTypeFilter
        private static IEnumerable<EntityType> GetTypeAndSubtypesOf(EntityType entityType, EdmItemCollection itemCollection)
        {
            // Always include the specified type, even if it's abstract
            yield return entityType;

            // Get the subtypes of the type from the item collection
            IEnumerable<EntityType> entityTypesInCollection = itemCollection.OfType<EntityType>();
            foreach (EntityType typeInCollection in entityTypesInCollection)
            {
                if (entityType.Equals(typeInCollection) == false && IsStrictSubtypeOf(typeInCollection, entityType))
                {
                    yield return typeInCollection;
                }
            }

            yield break;
        }

        // requires: firstType is not null
        // effects: if otherType is among the base types, return true, 
        // otherwise returns false.
        // when othertype is same as the current type, return false.
        private static bool IsStrictSubtypeOf(EntityType firstType, EntityType secondType)
        {
            Debug.Assert(firstType != null, "firstType should not be not null");
            if (secondType == null)
            {
                return false;
            }

            // walk up my type hierarchy list
            for (EntityType t = (EntityType)firstType.BaseType; t != null; t = (EntityType)t.BaseType)
            {
                if (t == secondType)
                    return true;
            }
            return false;
        }
        #endregion

        // Copy properties from temporary state to the data source
        internal void SaveEntityDataSourceProperties(EntityDataSourceState state)
        {
            this.EntityDataSource.ConnectionString = state.ConnectionString;
            this.EntityDataSource.DefaultContainerName = state.DefaultContainerName;
            this.EntityDataSource.EnableDelete = state.EnableDelete;
            this.EntityDataSource.EnableInsert = state.EnableInsert;
            this.EntityDataSource.EnableUpdate = state.EnableUpdate;            
            this.EntityDataSource.EntitySetName = state.EntitySetName;
            this.EntityDataSource.EntityTypeFilter = state.EntityTypeFilter;
            this.EntityDataSource.Select = state.Select;
            this.EntityDataSource.EnableFlattening = state.EnableFlattening;
        }

        // Copy  properties from the data source to temporary state
        internal EntityDataSourceState LoadEntityDataSourceState()
        {
            EntityDataSourceState state = new EntityDataSourceState();
            state.ConnectionString = this.EntityDataSource.ConnectionString;
            state.DefaultContainerName = this.EntityDataSource.DefaultContainerName;
            state.EnableDelete = this.EntityDataSource.EnableDelete;
            state.EnableInsert = this.EntityDataSource.EnableInsert;
            state.EnableUpdate = this.EntityDataSource.EnableUpdate;            
            state.EntitySetName = this.EntityDataSource.EntitySetName;
            state.EntityTypeFilter = this.EntityDataSource.EntityTypeFilter;
            state.Select = this.EntityDataSource.Select;
            return state;
        }
    }
}
