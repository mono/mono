//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    static class FxCop
    {
        public static class Category
        {
            public const string Design = "Microsoft.Design";
            public const string Globalization = "Microsoft.Globalization";
            public const string Maintainability = "Microsoft.Maintainability";
            public const string MSInternal = "Microsoft.MSInternal";
            public const string Naming = "Microsoft.Naming";
            public const string Performance = "Microsoft.Performance";
            public const string Reliability = "Microsoft.Reliability";
            public const string Security = "Microsoft.Security";
            public const string Usage = "Microsoft.Usage";
            public const string Configuration = "Configuration";
            public const string ReliabilityBasic = "Reliability";
            public const string Xaml = "XAML";
        }

        public static class Rule
        {
            public const string AptcaMethodsShouldOnlyCallAptcaMethods = "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods";
            public const string AssembliesShouldHaveValidStrongNames = "CA2210:AssembliesShouldHaveValidStrongNames";
            public const string AvoidCallingProblematicMethods = "CA2001:AvoidCallingProblematicMethods";
            public const string AvoidExcessiveComplexity = "CA1502:AvoidExcessiveComplexity";
            public const string AvoidNamespacesWithFewTypes = "CA1020:AvoidNamespacesWithFewTypes";
            public const string AvoidOutParameters = "CA1021:AvoidOutParameters";
            public const string AvoidUncalledPrivateCode = "CA1811:AvoidUncalledPrivateCode";
            public const string AvoidUninstantiatedInternalClasses = "CA1812:AvoidUninstantiatedInternalClasses";
            public const string AvoidUnsealedAttributes = "CA1813:AvoidUnsealedAttributes";
            public const string CollectionPropertiesShouldBeReadOnly = "CA2227:CollectionPropertiesShouldBeReadOnly";
            public const string CollectionsShouldImplementGenericInterface = "CA1010:CollectionsShouldImplementGenericInterface";
            public const string ConfigurationPropertyAttributeRule = "Configuration102:ConfigurationPropertyAttributeRule";
            public const string ConfigurationValidatorAttributeRule = "Configuration104:ConfigurationValidatorAttributeRule";
            public const string ConsiderPassingBaseTypesAsParameters = "CA1011:ConsiderPassingBaseTypesAsParameters";
            public const string CommunicationObjectThrowIf = "Reliability106";
            public const string ConfigurationPropertyNameRule = "Configuration103:ConfigurationPropertyNameRule";     
            public const string DefaultParametersShouldNotBeUsed = "CA1026:DefaultParametersShouldNotBeUsed";       
            public const string DefineAccessorsForAttributeArguments = "CA1019:DefineAccessorsForAttributeArguments";
            public const string DiagnosticsUtilityIsFatal = "Reliability108";
            public const string DisposableFieldsShouldBeDisposed = "CA2213:DisposableFieldsShouldBeDisposed";
            public const string DoNotCallOverridableMethodsInConstructors = "CA2214:DoNotCallOverridableMethodsInConstructors";
            public const string DoNotCatchGeneralExceptionTypes = "CA1031:DoNotCatchGeneralExceptionTypes";
            public const string DoNotDeclareReadOnlyMutableReferenceTypes = "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes";
            public const string DoNotDeclareVisibleInstanceFields = "CA1051:DoNotDeclareVisibleInstanceFields";
            public const string DoNotLockOnObjectsWithWeakIdentity = "CA2002:DoNotLockOnObjectsWithWeakIdentity";
            public const string DoNotIgnoreMethodResults = "CA1806:DoNotIgnoreMethodResults";
            public const string DoNotIndirectlyExposeMethodsWithLinkDemands = "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands";
            public const string DoNotPassLiteralsAsLocalizedParameters = "CA1303:DoNotPassLiteralsAsLocalizedParameters";
            public const string DoNotRaiseReservedExceptionTypes = "CA2201:DoNotRaiseReservedExceptionTypes";
            public const string EnumsShouldHaveZeroValue = "CA1008:EnumsShouldHaveZeroValue";
            public const string FlagsEnumsShouldHavePluralNames = "CA1714:FlagsEnumsShouldHavePluralNames";
            public const string GenericMethodsShouldProvideTypeParameter = "CA1004:GenericMethodsShouldProvideTypeParameter";
            public const string IdentifiersShouldBeSpelledCorrectly = "CA1704:IdentifiersShouldBeSpelledCorrectly";
            public const string IdentifiersShouldHaveCorrectSuffix = "CA1710:IdentifiersShouldHaveCorrectSuffix";
            public const string IdentifiersShouldNotContainTypeNames = "CA1720:IdentifiersShouldNotContainTypeNames";
            public const string IdentifiersShouldNotHaveIncorrectSuffix = "CA1711:IdentifiersShouldNotHaveIncorrectSuffix";
            public const string IdentifiersShouldNotMatchKeywords = "CA1716:IdentifiersShouldNotMatchKeywords";
            public const string ImplementStandardExceptionConstructors = "CA1032:ImplementStandardExceptionConstructors";
            public const string InstantiateArgumentExceptionsCorrectly = "CA2208:InstantiateArgumentExceptionsCorrectly";
            public const string InitializeReferenceTypeStaticFieldsInline = "CA1810:InitializeReferenceTypeStaticFieldsInline";
            public const string InterfaceMethodsShouldBeCallableByChildTypes = "CA1033:InterfaceMethodsShouldBeCallableByChildTypes";
            public const string MarkISerializableTypesWithSerializable = "CA2237:MarkISerializableTypesWithSerializable";
            public const string InvariantAssertRule = "Reliability101:InvariantAssertRule";
            public const string IsFatalRule = "Reliability108:IsFatalRule";
            public const string MarkMembersAsStatic = "CA1822:MarkMembersAsStatic";
            public const string NestedTypesShouldNotBeVisible = "CA1034:NestedTypesShouldNotBeVisible";
            public const string NormalizeStringsToUppercase = "CA1308:NormalizeStringsToUppercase";
            public const string OperatorOverloadsHaveNamedAlternates = "CA2225:OperatorOverloadsHaveNamedAlternates";
            public const string PropertyNamesShouldNotMatchGetMethods = "CA1721:PropertyNamesShouldNotMatchGetMethods";
            public const string PropertyTypesMustBeXamlVisible = "XAML1002:PropertyTypesMustBeXamlVisible";
            public const string PropertyExternalTypesMustBeKnown = "XAML1010:PropertyExternalTypesMustBeKnown";
            public const string ReplaceRepetitiveArgumentsWithParamsArray = "CA1025:ReplaceRepetitiveArgumentsWithParamsArray";
            public const string ResourceStringsShouldBeSpelledCorrectly = "CA1703:ResourceStringsShouldBeSpelledCorrectly";
            public const string ReviewSuppressUnmanagedCodeSecurityUsage = "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage";
            public const string ReviewUnusedParameters = "CA1801:ReviewUnusedParameters";
            public const string SecureAsserts = "CA2106:SecureAsserts";
            public const string SecureGetObjectDataOverrides = "CA2110:SecureGetObjectDataOverrides";
            public const string ShortAcronymsShouldBeUppercase = "CA1706:ShortAcronymsShouldBeUppercase";            
            public const string SpecifyIFormatProvider = "CA1305:SpecifyIFormatProvider";
            public const string SpecifyMarshalingForPInvokeStringArguments = "CA2101:SpecifyMarshalingForPInvokeStringArguments";
            public const string StaticHolderTypesShouldNotHaveConstructors = "CA1053:StaticHolderTypesShouldNotHaveConstructors";
            public const string SystemAndMicrosoftNamespacesRequireApproval = "CA:SystemAndMicrosoftNamespacesRequireApproval";
            public const string UsePropertiesWhereAppropriate = "CA1024:UsePropertiesWhereAppropriate";
            public const string UriPropertiesShouldNotBeStrings = "CA1056:UriPropertiesShouldNotBeStrings";
            public const string VariableNamesShouldNotMatchFieldNames = "CA1500:VariableNamesShouldNotMatchFieldNames";
            public const string ThunkCallbackRule = "Reliability109:ThunkCallbackRule";
            public const string TransparentMethodsMustNotReferenceCriticalCode = "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule";
            public const string TypeConvertersMustBePublic = "XAML1004:TypeConvertersMustBePublic";
            public const string TypesMustHaveXamlCallableConstructors = "XAML1007:TypesMustHaveXamlCallableConstructors";
            public const string TypeNamesShouldNotMatchNamespaces = "CA1724:TypeNamesShouldNotMatchNamespaces";
            public const string TypesShouldHavePublicParameterlessConstructors = "XAML1009:TypesShouldHavePublicParameterlessConstructors";
            public const string UseEventsWhereAppropriate = "CA1030:UseEventsWhereAppropriate";
            public const string UseNewGuidHelperRule = "Reliability113:UseNewGuidHelperRule";
            public const string WrapExceptionsRule = "Reliability102:WrapExceptionsRule";
        }
    }
}
