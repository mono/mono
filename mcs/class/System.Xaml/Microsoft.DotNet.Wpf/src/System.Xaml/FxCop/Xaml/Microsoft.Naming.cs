//
// FxCop Violation Message Suppressions
//  Approved List
//

using System.Diagnostics.CodeAnalysis;

// Whitespace is consistent with WhitespaceSignificantCollectionAttribute from 3.0
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Scope="member", Target="System.Xaml.XamlType.#IsWhitespaceSignificantCollectionCore", MessageId="Whitespace", Justification="Add Whitespace to the dictionary if we already shipped, and it seems good.")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Scope="member", Target="System.Xaml.XamlType.#IsWhitespaceSignificantCollection", MessageId="Whitespace", Justification="Add Whitespace to the dictionary if we already shipped, and it seems good.")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Scope="member", Target="System.Xaml.XamlType.#TrimSurroundingWhitespace", MessageId="Whitespace", Justification="Add Whitespace to the dictionary if we already shipped, and it seems good.")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Scope="member", Target="System.Xaml.XamlType.#TrimSurroundingWhitespaceCore", MessageId="Whitespace", Justification="Add Whitespace to the dictionary if we already shipped, and it seems good.")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Scope="member", Target="System.Xaml.Schema.XaslType.#IsWhitespaceSignificantCollection", MessageId="Whitespace", Justification="Add Whitespace to the dictionary if we already shipped, and it seems good.")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Scope="member", Target="System.Xaml.Schema.XaslType.#TrimSurroundingWhitespace", MessageId="Whitespace", Justification="Add Whitespace to the dictionary if we already shipped, and it seems good.")]

// use of Eof is an API choice.
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "System.Xaml.XamlReader.#IsEof", MessageId = "Eof", Justification = "Review Eof")]

[module: SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope="type", Target="System.Xaml.XamlNodeQueue", Justification="This is unnecessarily limiting.")]

[module: SuppressMessage("Microsoft.Naming","CA1721:PropertyNamesShouldNotMatchGetMethods", Scope="member", Target="System.Windows.Markup.PropertyDefinition.#Type", Justification="Makes sense for our problem domain.")]

[module: SuppressMessage("Microsoft.Naming","CA1721:PropertyNamesShouldNotMatchGetMethods", Scope="member", Target="System.Xaml.XamlMember.#Type", Justification="Makes sense for our problem domain.")]

[module: SuppressMessage("Microsoft.Naming","CA1721:PropertyNamesShouldNotMatchGetMethods", Scope="member", Target="System.Xaml.XamlReader.#Type", Justification="Makes sense for our problem domain.")]

[module: SuppressMessage("Microsoft.Naming","CA1716:IdentifiersShouldNotMatchKeywords", Scope="member", Target="System.Xaml.XamlReader.#Namespace", MessageId="Namespace", Justification="Works for our problem domain.")]

[module: SuppressMessage("Microsoft.Naming","CA1702:CompoundWordsShouldBeCasedCorrectly", Scope="member", Target="System.Xaml.XamlType.#LookupIsWhitespaceSignificantCollection()", MessageId="Whitespace", Justification="Back compat.")]

[module: SuppressMessage("Microsoft.Naming","CA1702:CompoundWordsShouldBeCasedCorrectly", Scope="member", Target="System.Xaml.XamlType.#LookupTrimSurroundingWhitespace()", MessageId="Whitespace", Justification="Back compat.")]

[module: SuppressMessage("Microsoft.Naming","CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="SubClass", Scope="member", Target="System.Xaml.XamlLanguage.#SubClass", Justification="Needs to match the capitalization used in XAML syntax.")]

[module: SuppressMessage("Microsoft.Naming","CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="Whitespace", Scope="type", Target="System.Windows.Markup.TrimSurroundingWhitespaceAttribute", Justification="Kept for compatibility.")]

[module: SuppressMessage("Microsoft.Naming","CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="Whitespace", Scope="type", Target="System.Windows.Markup.WhitespaceSignificantCollectionAttribute", Justification="Kept for compatibility.")]

[module: SuppressMessage("Microsoft.Naming","CA1721:PropertyNamesShouldNotMatchGetMethods", Scope="member", Target="System.Windows.Markup.ArrayExtension.#Type", Justification="Kept for compatibility.")]

[module: SuppressMessage("Microsoft.Naming","CA1721:PropertyNamesShouldNotMatchGetMethods", Scope="member", Target="System.Windows.Markup.TypeExtension.#Type", Justification="Kept for compatibility.")]

[module: SuppressMessage("Microsoft.Naming","CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="nameSpace", Scope="member", Target="System.Windows.Markup.RootNamespaceAttribute.#.ctor(System.String)", Justification="Inherited from Base.")]

[module: SuppressMessage("Microsoft.Naming","CA1721:PropertyNamesShouldNotMatchGetMethods", Scope="member", Target="System.Windows.Markup.NameScopePropertyAttribute.#Type", Justification="Kept for compatibility.")]

// FrugalList suppressions copied over from WindowsBase
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="System.Xaml.MS.Impl.ThreeItemList`1.Promote(System.Xaml.MS.Impl.ThreeItemList`1<T>):System.Void")]
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="System.Xaml.MS.Impl.ThreeItemList`1.SetCount(System.Int32):System.Void")]
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="System.Xaml.MS.Impl.ThreeItemList`1.Promote(System.Xaml.MS.Impl.FrugalListBase`1<T>):System.Void")]
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="System.Xaml.MS.Impl.ArrayItemList`1.Promote(System.Xaml.MS.Impl.SixItemList`1<T>):System.Void")]
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="System.Xaml.MS.Impl.ArrayItemList`1.SetCount(System.Int32):System.Void")]
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="System.Xaml.MS.Impl.SingleItemList`1.SetCount(System.Int32):System.Void")]
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="System.Xaml.MS.Impl.SixItemList`1.Promote(System.Xaml.MS.Impl.SixItemList`1<T>):System.Void")]
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="System.Xaml.MS.Impl.SixItemList`1.Promote(System.Xaml.MS.Impl.ThreeItemList`1<T>):System.Void")]
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="System.Xaml.MS.Impl.SixItemList`1.SetCount(System.Int32):System.Void")]
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="System.Xaml.MS.Impl.SixItemList`1.Promote(System.Xaml.MS.Impl.FrugalListBase`1<T>):System.Void")]

[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Uid", Scope = "type", Target = "System.Windows.Markup.UidPropertyAttribute")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Uid", Scope = "member", Target = "System.Xaml.XamlLanguage.#Uid")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Namescope", Scope = "member", Target = "System.Xaml.XamlObjectWriterSettings.#RegisterNamesOnExternalNamescope")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Uids", Scope = "member", Target = "System.Xaml.XamlReaderSettings.#IgnoreUidsOnPropertyElements")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Arity", Scope = "member", Target = "System.Xaml.XamlSchemaContext.#SupportMarkupExtensionsWithDuplicateArity")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Arity", Scope = "member", Target = "System.Xaml.XamlSchemaContextSettings.#SupportMarkupExtensionsWithDuplicateArity")]
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "Arity", Scope = "resource", Target = "ExceptionStringTable.resources")]
