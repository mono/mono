//
// FxCop Violation Message Suppressions
//  Approved List
//

using System.Diagnostics.CodeAnalysis;

// This is used in a Stack<ObjectWriterFrame> and all the calls to "new" are "new T"
[module: SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type", Target = "MS.Internal.Xaml.Context.ObjectWriterFrame")]

// Need this public Ctor Override that takes an InnerExcepetion.
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.XamlParseException.#.ctor(MS.Internal.Xaml.Context.XamlParserContext,System.String,System.Exception)")]

[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.EventTrace.#EasyTraceEvent(MS.Internal.Xaml.EventTrace+Keyword,MS.Internal.Xaml.EventTrace+Event)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.EventTrace.#EasyTraceEvent(MS.Internal.Xaml.EventTrace+Keyword,MS.Internal.Xaml.EventTrace+Event,System.Object)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.EventTrace.#EasyTraceEvent(MS.Internal.Xaml.EventTrace+Keyword,MS.Internal.Xaml.EventTrace+Event,System.Object,System.Object)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.EventTrace.#EasyTraceEvent(MS.Internal.Xaml.EventTrace+Keyword,MS.Internal.Xaml.EventTrace+Event,System.Object,System.Object,System.Object)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.EventTrace.#EasyTraceEvent(MS.Internal.Xaml.EventTrace+Keyword,MS.Internal.Xaml.EventTrace+Level,MS.Internal.Xaml.EventTrace+Event)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.EventTrace.#EasyTraceEvent(MS.Internal.Xaml.EventTrace+Keyword,MS.Internal.Xaml.EventTrace+Level,MS.Internal.Xaml.EventTrace+Event,System.Object)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.EventTrace.#EasyTraceEvent(MS.Internal.Xaml.EventTrace+Keyword,MS.Internal.Xaml.EventTrace+Level,MS.Internal.Xaml.EventTrace+Event,System.Object,System.Object)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.TraceProvider.#get_Keywords()", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.TraceProvider.#get_Level()", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.TraceProvider.#get_MatchAllKeywords()", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.TraceProvider.#TraceEvent(MS.Internal.Xaml.EventTrace+Event,MS.Internal.Xaml.EventTrace+Keyword,MS.Internal.Xaml.EventTrace+Level)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.TraceProvider.#TraceEvent(MS.Internal.Xaml.EventTrace+Event,MS.Internal.Xaml.EventTrace+Keyword,MS.Internal.Xaml.EventTrace+Level,System.Object)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.Context.ObjectWriterContext.#get_ParentInstanceRegisteredName()", Justification = "We need the setter, and write-only properties are bad practice")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Internal.Xaml.Context.ObjectWriterContext.#get_ParentKey()", Justification = "We need the setter, and write-only properties are bad practice")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "MS.Win32.ClassicEtw.#GetTraceLoggerHandle(MS.Win32.ClassicEtw+WNODE_HEADER*)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.ReflectionHelper.#GetAlreadyLoadedAssembly(System.String)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.ReflectionHelper.#GetCustomAttributeData(System.Collections.Generic.IList`1<System.Reflection.CustomAttributeData>,System.Type,System.Type&,System.Boolean,System.Boolean)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.ReflectionHelper.#GetCustomAttributeData(System.Reflection.MemberInfo,System.Type,System.Type&)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.ReflectionHelper.#GetTypeConverterAttributeData(System.Reflection.MemberInfo,System.Type&)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.ReflectionHelper.#IsInternalType(System.Type)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.ReflectionHelper.#ResetCacheForAssembly(System.String)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.SR.#get_ResourceManager()", Justification = "Auto-generated")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.XmlCompatibilityReader.#.ctor(System.Xml.XmlReader,System.Collections.Generic.IEnumerable`1<System.String>)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.XmlCompatibilityReader.#.ctor(System.Xml.XmlReader,System.Xaml.IsXmlNamespaceSupportedCallback,System.Collections.Generic.IEnumerable`1<System.String>)", Justification = "Shared source file")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.XmlCompatibilityReader.#get_Encoding()", Justification = "Shared source file")]

// This is a debug-only method, we should mark it as Conditional("DEBUG")
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Xaml.XamlNode.#IsEof_Helper(System.Xaml.XamlNodeType,System.Object)", Justification = "Fix doesn't meet Ask Mode bar")]

[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.Xaml.XamlObjectWriter.#get_ObjectWriterContext()", Justification="Fix doesn't meet Ask Mode bar - Bug 773900")]

// New since v4 RTM:

//this is used by subclasses, bad FxCop detection
[module: SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses", Scope="type", Target="System.Xaml.MS.Impl.FrugalObjectList`1+Compacter")]
