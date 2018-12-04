using System.Diagnostics.CodeAnalysis;

//***************************************************************************************************************************
// Ignoring the return is intentional in all of the cases below.  Successful ETW registration isn’t critical for the app to continue running since it’s just for diagnostic purposes.
//***************************************************************************************************************************
[module: SuppressMessage("Microsoft.Usage","CA1806:DoNotIgnoreMethodResults", MessageId="MS.Win32.ClassicEtw.UnregisterTraceGuids(System.UInt64)", Scope="member", Target="MS.Internal.Xaml.ClassicTraceProvider.#Finalize()")]
[module: SuppressMessage("Microsoft.Usage","CA1806:DoNotIgnoreMethodResults", MessageId="MS.Win32.ClassicEtw.RegisterTraceGuidsW(MS.Win32.ClassicEtw+ControlCallback,System.IntPtr,System.Guid@,System.Int32,MS.Win32.ClassicEtw+TRACE_GUID_REGISTRATION@,System.String,System.String,System.UInt64@)", Scope="member", Target="MS.Internal.Xaml.ClassicTraceProvider.#Register(System.Guid)")]
[module: SuppressMessage("Microsoft.Usage","CA1806:DoNotIgnoreMethodResults", MessageId="MS.Win32.ManifestEtw.EventUnregister(System.UInt64)", Scope="member", Target="MS.Internal.Xaml.ManifestTraceProvider.#Finalize()")]
[module: SuppressMessage("Microsoft.Usage","CA1806:DoNotIgnoreMethodResults", MessageId="MS.Win32.ManifestEtw.EventRegister(System.Guid@,MS.Win32.ManifestEtw+EtwEnableCallback,System.Void*,System.UInt64@)", Scope="member", Target="MS.Internal.Xaml.ManifestTraceProvider.#Register(System.Guid)")]

[module: SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Xaml.GCNotificationToken", Scope = "member", Target = "System.Xaml.GCNotificationToken.#RegisterCallback(System.Threading.WaitCallback,System.Object)", Justification = "The GCNotificationToken object is created only for the executing code to be notified of a GC and is intended to be released immediately")]

//***************************************************************************************************************************
// Warnings related to the No Primary Interop Assembly (NoPIA) are suppressed.  This is not relevant to WPF at the moment.
//***************************************************************************************************************************
[module: SuppressMessage("Microsoft.Usage","CA2302:FlagServiceProviders", Scope="type", Target="MS.Internal.Xaml.ServiceProviderContext")]
[module: SuppressMessage("Microsoft.Usage","CA2301:EmbeddableTypesInContainersRule", MessageId="DelegateCreators", Scope="member", Target="MS.Internal.Xaml.Runtime.DynamicMethodRuntime.#CreateDelegate(System.Type,System.Object,System.String)")]
[module: SuppressMessage("Microsoft.Usage","CA2301:EmbeddableTypesInContainersRule", MessageId="ConverterInstances", Scope="member", Target="MS.Internal.Xaml.Runtime.DynamicMethodRuntime.#GetConverterInstance`1(System.Xaml.Schema.XamlValueConverter`1<!!0>)")]
[module: SuppressMessage("Microsoft.Usage","CA2303:FlagTypeGetHashCode", Scope="member", Target="System.Windows.Markup.ContentWrapperAttribute.#GetHashCode()")]
[module: SuppressMessage("Microsoft.Usage","CA2302:FlagServiceProviders", Scope="type", Target="System.Windows.Markup.IValueSerializerContext")]
[module: SuppressMessage("Microsoft.Usage","CA2303:FlagTypeGetHashCode", Scope="member", Target="System.Xaml.AttachableMemberIdentifier.#GetHashCode()")]
[module: SuppressMessage("Microsoft.Usage","CA2302:FlagServiceProviders", Scope="type", Target="System.Xaml.XamlObjectReader+TypeDescriptorAndValueSerializerContext")]
[module: SuppressMessage("Microsoft.Usage","CA2303:FlagTypeGetHashCode", MessageId="get_UnderlyingType", Scope="member", Target="System.Xaml.XamlType.#GetHashCode()")]
[module: SuppressMessage("Microsoft.Usage","CA2302:FlagServiceProviders", Scope="type", Target="System.Xaml.Replacements.DateTimeValueSerializerContext")]
[module: SuppressMessage("Microsoft.Usage","CA2303:FlagTypeGetHashCode", MessageId="get_ConverterType", Scope="member", Target="System.Xaml.Schema.XamlValueConverter`1.#GetHashCode()")]
