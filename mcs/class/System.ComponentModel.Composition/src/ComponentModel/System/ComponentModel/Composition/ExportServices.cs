// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition
{
    // Provides helpers for creating and dealing with Exports
    internal static partial class ExportServices
    {
        private static readonly MethodInfo _createStronglyTypedLazyOfTM = typeof(ExportServices).GetMethod("CreateStronglyTypedLazyOfTM", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _createStronglyTypedLazyOfT = typeof(ExportServices).GetMethod("CreateStronglyTypedLazyOfT", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _createSemiStronglyTypedLazy = typeof(ExportServices).GetMethod("CreateSemiStronglyTypedLazy", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _createStronglyTypedExportFactoryOfT = typeof(ExportServices).GetMethod("CreateStronglyTypedExportFactoryOfT", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _createStronglyTypedExportFactoryOfTM = typeof(ExportServices).GetMethod("CreateStronglyTypedExportFactoryOfTM", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly Type DefaultMetadataViewType = typeof(IDictionary<string, object>);
        internal static readonly Type DefaultExportedValueType = typeof(object);

        internal static bool IsDefaultMetadataViewType(Type metadataViewType)
        {
            Assumes.NotNull(metadataViewType);

            // Consider all types that IDictionary<string, object> derives from, such
            // as ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>> 
            // and IEnumerable, as default metadata view
            return metadataViewType.IsAssignableFrom(DefaultMetadataViewType);
        }

        internal static bool IsDictionaryConstructorViewType(Type metadataViewType)
        {
            Assumes.NotNull(metadataViewType);

            // Does the view type have a constructor that is a Dictionary<string, object>
            return metadataViewType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                    Type.DefaultBinder,
                                                    new Type[] { typeof(IDictionary<string, object>) },
                                                    new ParameterModifier[0]) != null;
        }

        internal static Func<Export, object> CreateStronglyTypedLazyFactory(Type exportType, Type metadataViewType)
        {
            MethodInfo genericMethod = null;
            if (metadataViewType != null)
            {
                genericMethod = _createStronglyTypedLazyOfTM.MakeGenericMethod(exportType ?? ExportServices.DefaultExportedValueType, metadataViewType);
            }
            else
            {
                genericMethod = _createStronglyTypedLazyOfT.MakeGenericMethod(exportType ?? ExportServices.DefaultExportedValueType);
            }
            Assumes.NotNull(genericMethod);
            return (Func<Export, object>)Delegate.CreateDelegate(typeof(Func<Export, object>), genericMethod);
        }

        internal static Func<Export, Lazy<object, object>> CreateSemiStronglyTypedLazyFactory(Type exportType, Type metadataViewType)
        {
            MethodInfo genericMethod = _createSemiStronglyTypedLazy.MakeGenericMethod(
                exportType ?? ExportServices.DefaultExportedValueType,
                metadataViewType ?? ExportServices.DefaultMetadataViewType);
            Assumes.NotNull(genericMethod);
            return (Func<Export, Lazy<object, object>>)Delegate.CreateDelegate(typeof(Func<Export, Lazy<object,object>>), genericMethod);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static Lazy<T, M> CreateStronglyTypedLazyOfTM<T, M>(Export export)
        {
            IDisposable disposable = export as IDisposable;
            if (disposable != null)
            {
                return new DisposableLazy<T, M>(
                    () => ExportServices.GetCastedExportedValue<T>(export),
                    AttributedModelServices.GetMetadataView<M>(export.Metadata),
                    disposable);
            }
            else
            {
                return new Lazy<T, M>(
                    () => ExportServices.GetCastedExportedValue<T>(export),
                    AttributedModelServices.GetMetadataView<M>(export.Metadata),
                    false);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static Lazy<T> CreateStronglyTypedLazyOfT<T>(Export export)
        {
            IDisposable disposable = export as IDisposable;
            if (disposable != null)
            {
                return new DisposableLazy<T>(
                    () => ExportServices.GetCastedExportedValue<T>(export),
                    disposable);
            }
            else
            {
                return new Lazy<T>(() => ExportServices.GetCastedExportedValue<T>(export), false);

            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static Lazy<object, object> CreateSemiStronglyTypedLazy<T, M>(Export export)
        {
            IDisposable disposable = export as IDisposable;
            if (disposable != null)
            {
                return new DisposableLazy<object, object>(
                    () => ExportServices.GetCastedExportedValue<T>(export),
                    AttributedModelServices.GetMetadataView<M>(export.Metadata),
                    disposable);
            }
            else
            {
                return new Lazy<object, object>(
                    () => ExportServices.GetCastedExportedValue<T>(export),
                    AttributedModelServices.GetMetadataView<M>(export.Metadata),
                    false
                    );
            }
        }

        internal static Func<Export, object> CreateStronglyTypedExportFactoryFactory(Type exportType, Type metadataType, ConstructorInfo constructor)
        {
            MethodInfo genericMethod = null;
            if (metadataType == null)
            {
                 genericMethod = _createStronglyTypedExportFactoryOfT.MakeGenericMethod(exportType);
            }
            else
            {
                genericMethod = _createStronglyTypedExportFactoryOfTM.MakeGenericMethod(exportType, metadataType);
            }
            
            Assumes.NotNull(genericMethod);
            Func<Export, ConstructorInfo, object> exportFactoryFactory = (Func<Export, ConstructorInfo, object>)Delegate.CreateDelegate(typeof(Func<Export, ConstructorInfo, object>), genericMethod);
            return (e) => exportFactoryFactory.Invoke(e, constructor);
        }

        private static Tuple<T, Action> GetExportLifetimeContextFromExport<T>(Export export)
        {
            T exportedValue;
            Action disposeAction;
            IDisposable disposable = null;

            CatalogExportProvider.PartCreatorExport partCreatorExport = export as CatalogExportProvider.PartCreatorExport;

            if (partCreatorExport != null)
            {
                // PartCreatorExport is the more optimized route
                Export exportProduct = partCreatorExport.CreateExportProduct();
                exportedValue = GetCastedExportedValue<T>(exportProduct);
                disposable = exportProduct as IDisposable;
            }
            else
            {
                // If it comes from somewhere else we walk through the ComposablePartDefinition
                var factoryPartDefinition = GetCastedExportedValue<ComposablePartDefinition>(export);
                var part = factoryPartDefinition.CreatePart();
                var exportDef = factoryPartDefinition.ExportDefinitions.Single();

                exportedValue = CastExportedValue<T>(part.ToElement(), part.GetExportedValue(exportDef));
                disposable = part as IDisposable;
            }

            if (disposable != null)
            {
                disposeAction = () => disposable.Dispose();
            }
            else
            {
                disposeAction = () => { };
            }

            return new Tuple<T, Action>(exportedValue, disposeAction);
        }

        private static object CreateStronglyTypedExportFactoryOfT<T>(Export export, ConstructorInfo constructor)
        {
            Func<Tuple<T, Action>> exportLifetimeContextCreator = () => ExportServices.GetExportLifetimeContextFromExport<T>(export);
            return constructor.Invoke(new object[] { exportLifetimeContextCreator });
        }

        private static object CreateStronglyTypedExportFactoryOfTM<T, M>(Export export, ConstructorInfo constructor)
        {
            Func<Tuple<T, Action>> exportLifetimeContextCreator = () => ExportServices.GetExportLifetimeContextFromExport<T>(export);
            return constructor.Invoke(new object[] { exportLifetimeContextCreator, AttributedModelServices.GetMetadataView<M>(export.Metadata) });
        }

        internal static T GetCastedExportedValue<T>(Export export)
        {
            return CastExportedValue<T>(export.ToElement(), export.Value);
        }

        internal static T CastExportedValue<T>(ICompositionElement element, object exportedValue)
        {
            object typedExportedValue = null;

            bool succeeded = ContractServices.TryCast(typeof(T), exportedValue, out typedExportedValue);
            if (!succeeded)
            {
                throw new CompositionContractMismatchException(string.Format(CultureInfo.CurrentCulture,
                    Strings.ContractMismatch_ExportedValueCannotBeCastToT,
                    element.DisplayName,
                    typeof(T)));
            }

            return (T)typedExportedValue;
        }
        
        internal static ExportCardinalityCheckResult CheckCardinality(ImportDefinition definition, IEnumerable<Export> exports)
        {
            EnumerableCardinality actualCardinality = exports.GetCardinality();

            switch (actualCardinality)
            {
                case EnumerableCardinality.Zero:
                    if (definition.Cardinality == ImportCardinality.ExactlyOne)
                    {
                        return ExportCardinalityCheckResult.NoExports;
                    }
                    break;

                case EnumerableCardinality.TwoOrMore:
                    if (definition.Cardinality.IsAtMostOne())
                    {
                        return ExportCardinalityCheckResult.TooManyExports;
                    }
                    break;

                default:
                    Assumes.IsTrue(actualCardinality == EnumerableCardinality.One);
                    break;

            }

            return ExportCardinalityCheckResult.Match;
        }
    }
}
