//---------------------------------------------------------------------
// <copyright file="StoreItemCollection.Loader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.EntityModel.SchemaObjectModel;
    using System.Diagnostics;
    using System.Text;
    using System.Xml;

    public sealed partial class StoreItemCollection
    {
        private class Loader
        {
            private string _provider = null;
            private string _providerManifestToken = null;
            private DbProviderManifest _providerManifest;
            private DbProviderFactory _providerFactory;
            IList<EdmSchemaError> _errors;
            IList<Schema> _schemas;
            bool _throwOnError;

            public Loader(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> sourceFilePaths, bool throwOnError)
            {
                _throwOnError = throwOnError;
                LoadItems(xmlReaders, sourceFilePaths);
            }



            public IList<EdmSchemaError> Errors
            {
                get { return _errors; }
            }

            public IList<Schema> Schemas
            {
                get { return _schemas; }
            }

            public DbProviderManifest ProviderManifest
            {
                get { return _providerManifest; }
            }
            
            public DbProviderFactory ProviderFactory
            {
                get { return _providerFactory; }
            }

            public string ProviderManifestToken
            {
                get { return _providerManifestToken; }
            }

            public bool HasNonWarningErrors
            {
                get
                {
                    return !MetadataHelper.CheckIfAllErrorsAreWarnings(_errors);
                }
            }
            
            private void LoadItems(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> sourceFilePaths)
            {

                Debug.Assert(_errors == null, "we are expecting this to be the location that sets _errors for the first time");
                _errors = SchemaManager.ParseAndValidate(xmlReaders,
                                                        sourceFilePaths,
                                                        SchemaDataModelOption.ProviderDataModel,
                                                        OnProviderNotification,
                                                        OnProviderManifestTokenNotification,
                                                        OnProviderManifestNeeded,
                                                        out _schemas
                                                        );
                if (_throwOnError)
                {
                    ThrowOnNonWarningErrors();
                }
            }

            internal void ThrowOnNonWarningErrors()
            {
                if (!MetadataHelper.CheckIfAllErrorsAreWarnings(_errors))
                {
                    //Future Enhancement: if there is an error, we throw exception with error and warnings.
                    //Otherwise the user has no clue to know about warnings.
                    throw EntityUtil.InvalidSchemaEncountered(Helper.CombineErrorMessage(_errors));
                }
            }

            private void OnProviderNotification(string provider, Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
            {
                string expected = _provider;
                if (_provider == null)
                {
                    // Even if the Provider is only now being discovered from the first SSDL file,
                    // it must still match the 'implicit' provider that is implied by the DbConnection
                    // or DbProviderFactory that was used to construct this StoreItemCollection.
                    _provider = provider;
                    InitializeProviderManifest(addError);
                    return;
                }
                else
                {
                    // The provider was previously discovered from a preceeding SSDL file; it is an error
                    // if the 'Provider' attributes in all SSDL files are not identical.
                    if (_provider == provider)
                    {
                        return;
                    }
                }

                Debug.Assert(expected != null, "Expected provider name not initialized from _provider or _providerFactory?");

                addError(Strings.AllArtifactsMustTargetSameProvider_InvariantName(expected, _provider),
                                               ErrorCode.InconsistentProvider,
                                               EdmSchemaErrorSeverity.Error);
            }

            private void InitializeProviderManifest(Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
            {

                if (_providerManifest == null && (_providerManifestToken != null && _provider != null))
                {
                    DbProviderFactory factory = null;
                    try
                    {
                        factory = DbProviderServices.GetProviderFactory(_provider);
                    }
                    catch (ArgumentException e)
                    {
                        addError(e.Message, ErrorCode.InvalidProvider, EdmSchemaErrorSeverity.Error);
                        return;
                    }

                    try
                    {
                        DbProviderServices services = DbProviderServices.GetProviderServices(factory);
                        _providerManifest = services.GetProviderManifest(_providerManifestToken);
                        _providerFactory = factory;
                        if (_providerManifest is EdmProviderManifest)
                        {
                            if (_throwOnError)
                            {
                                throw EntityUtil.NotSupported(Strings.OnlyStoreConnectionsSupported);
                            }
                            else
                            {
                                addError(Strings.OnlyStoreConnectionsSupported, ErrorCode.InvalidProvider, EdmSchemaErrorSeverity.Error);
                            }
                            return;
                        }
                    }
                    catch (ProviderIncompatibleException e)
                    {
                        if (_throwOnError)
                        {
                            // we want to surface these as ProviderIncompatibleExceptions if we are "allowed" to.
                            throw;
                        }

                        AddProviderIncompatibleError(e, addError);
                    }
                }
            }

            private void OnProviderManifestTokenNotification(string token, Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
            {
                if (_providerManifestToken == null)
                {
                    _providerManifestToken = token;
                    InitializeProviderManifest(addError);
                    return;
                }

                if (_providerManifestToken != token)
                {
                    addError(Strings.AllArtifactsMustTargetSameProvider_ManifestToken(token, _providerManifestToken),
                                                   ErrorCode.ProviderManifestTokenMismatch,
                                                   EdmSchemaErrorSeverity.Error);
                }
            }

            private DbProviderManifest OnProviderManifestNeeded(Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
            {
                if (_providerManifest == null)
                {
                    addError(Strings.ProviderManifestTokenNotFound,
                                       ErrorCode.ProviderManifestTokenNotFound,
                                       EdmSchemaErrorSeverity.Error);
                }
                return _providerManifest;
            }

            private void AddProviderIncompatibleError(ProviderIncompatibleException provEx, Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
            {
                Debug.Assert(provEx != null);
                Debug.Assert(addError != null);

                StringBuilder message = new StringBuilder(provEx.Message);
                if (provEx.InnerException != null && !string.IsNullOrEmpty(provEx.InnerException.Message))
                {
                    message.AppendFormat(" {0}", provEx.InnerException.Message);
                }

                addError(message.ToString(),
                                   ErrorCode.FailedToRetrieveProviderManifest,
                                   EdmSchemaErrorSeverity.Error);
            }
        }
    }
}
