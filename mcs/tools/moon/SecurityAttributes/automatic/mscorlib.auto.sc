# [SecurityCritical] needed to execute code inside 'mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e'.
# 601 methods needs to be decorated.

# using 'System.IntPtr' as a parameter type
+SC-M: Mono.Interop.ComInteropProxy Mono.Interop.ComInteropProxy::FindProxy(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: Mono.Interop.ComInteropProxy Mono.Interop.ComInteropProxy::GetProxy(System.IntPtr,System.Type)

# localloc
+SC-M: System.Boolean Mono.Globalization.Unicode.SimpleCollator::IsPrefix(System.String,System.String,System.Int32,System.Int32,System.Globalization.CompareOptions)

# using 'System.Byte*' as a parameter type
+SC-M: System.Boolean Mono.Globalization.Unicode.SimpleCollator::MatchesBackward(System.String,System.Int32&,System.Int32,System.Int32,System.Int32,System.Byte*,System.Boolean,Mono.Globalization.Unicode.SimpleCollator/Context&)

# using 'System.Byte*' as a parameter type
+SC-M: System.Boolean Mono.Globalization.Unicode.SimpleCollator::MatchesBackwardCore(System.String,System.Int32&,System.Int32,System.Int32,System.Int32,System.Byte*,System.Boolean,Mono.Globalization.Unicode.SimpleCollator/ExtenderType,Mono.Globalization.Unicode.Contraction&,Mono.Globalization.Unicode.SimpleCollator/Context&)

# using 'System.Byte*' as a parameter type
+SC-M: System.Boolean Mono.Globalization.Unicode.SimpleCollator::MatchesForward(System.String,System.Int32&,System.Int32,System.Int32,System.Byte*,System.Boolean,Mono.Globalization.Unicode.SimpleCollator/Context&)

# using 'System.Byte*' as a parameter type
+SC-M: System.Boolean Mono.Globalization.Unicode.SimpleCollator::MatchesForwardCore(System.String,System.Int32&,System.Int32,System.Int32,System.Byte*,System.Boolean,Mono.Globalization.Unicode.SimpleCollator/ExtenderType,Mono.Globalization.Unicode.Contraction&,Mono.Globalization.Unicode.SimpleCollator/Context&)

# using 'System.Byte*' as a parameter type
+SC-M: System.Boolean Mono.Globalization.Unicode.SimpleCollator::MatchesPrimitive(System.Globalization.CompareOptions,System.Byte*,System.Int32,Mono.Globalization.Unicode.SimpleCollator/ExtenderType,System.Byte*,System.Int32,System.Boolean)

# using 'System.Byte*' as a parameter type
+SC-M: System.Boolean System.Double::ParseImpl(System.Byte*,System.Double&)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.IntPtr::op_Equality(System.IntPtr,System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.IntPtr::op_Inequality(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.IO.MonoIO::Close(System.IntPtr,System.IO.MonoIOError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.IO.MonoIO::SetLength(System.IntPtr,System.Int64,System.IO.MonoIOError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Security.Principal.WindowsImpersonationContext::CloseToken(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Security.Principal.WindowsImpersonationContext::SetCurrentToken(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Security.Principal.WindowsPrincipal::IsMemberOfGroupId(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Security.Principal.WindowsPrincipal::IsMemberOfGroupName(System.IntPtr,System.String)

# using 'System.Security.RuntimeDeclSecurityActions*' as a parameter type
+SC-M: System.Boolean System.Security.SecurityManager::GetLinkDemandSecurity(System.Reflection.MethodBase,System.Security.RuntimeDeclSecurityActions*,System.Security.RuntimeDeclSecurityActions*)

# using 'System.Security.RuntimeDeclSecurityActions*' as a parameter type
+SC-M: System.Boolean System.Security.SecurityManager::InheritanceDemand(System.AppDomain,System.Reflection.Assembly,System.Security.RuntimeDeclSecurityActions*)

# using 'System.Security.RuntimeDeclSecurityActions*' as a parameter type
+SC-M: System.Boolean System.Security.SecurityManager::LinkDemand(System.Reflection.Assembly,System.Security.RuntimeDeclSecurityActions*,System.Security.RuntimeDeclSecurityActions*)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Threading.Mutex::ReleaseMutex_internal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Threading.NativeEventCalls::ResetEvent_internal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Threading.NativeEventCalls::SetEvent_internal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Threading.Thread::Join_internal(System.Int32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Threading.ThreadPool::BindHandle(System.IntPtr)

# using 'System.Threading.NativeOverlapped*' as a parameter type
+SC-M: System.Boolean System.Threading.ThreadPool::UnsafeQueueNativeOverlapped(System.Threading.NativeOverlapped*)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Threading.WaitHandle::WaitOne_internal(System.IntPtr,System.Int32,System.Boolean)

# [VISIBLE] using 'System.UIntPtr' as a parameter type
+SC-M: System.Boolean System.UIntPtr::op_Equality(System.UIntPtr,System.UIntPtr)

# [VISIBLE] using 'System.UIntPtr' as a parameter type
+SC-M: System.Boolean System.UIntPtr::op_Inequality(System.UIntPtr,System.UIntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Byte System.Runtime.InteropServices.Marshal::ReadByte(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Byte System.Runtime.InteropServices.Marshal::ReadByte(System.IntPtr,System.Int32)

# [VISIBLE] using 'System.Byte*' as return type
+SC-M: System.Byte* System.IO.UnmanagedMemoryStream::get_PositionPointer()

# using 'System.Byte*' as a parameter type
+SC-M: System.Byte[] Mono.Security.BitConverterLE::GetUIntBytes(System.Byte*)

# using 'System.Byte*' as a parameter type
+SC-M: System.Byte[] Mono.Security.BitConverterLE::GetULongBytes(System.Byte*)

# using 'System.Byte*' as a parameter type
+SC-M: System.Byte[] Mono.Security.BitConverterLE::GetUShortBytes(System.Byte*)

# using 'System.Byte*' as a parameter type
+SC-M: System.Byte[] System.BitConverter::GetBytes(System.Byte*,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Byte[] System.Reflection.Module::ResolveSignature(System.IntPtr,System.Int32,System.Reflection.ResolveTokenError&)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Delegate System.Runtime.InteropServices.Marshal::GetDelegateForFunctionPointer(System.IntPtr,System.Type)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Delegate System.Runtime.InteropServices.Marshal::GetDelegateForFunctionPointerInternal(System.IntPtr,System.Type)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Exception System.Runtime.InteropServices.Marshal::GetExceptionForHR(System.Int32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Runtime.InteropServices.ObjectCreationDelegate::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# [VISIBLE] using 'System.Threading.NativeOverlapped*' as a parameter type
+SC-M: System.IAsyncResult System.Threading.IOCompletionCallback::BeginInvoke(System.UInt32,System.UInt32,System.Threading.NativeOverlapped*,System.AsyncCallback,System.Object)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Int16 System.Runtime.InteropServices.Marshal::ReadInt16(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Int16 System.Runtime.InteropServices.Marshal::ReadInt16(System.IntPtr,System.Int32)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegCloseKey(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegConnectRegistry(System.String,System.IntPtr,System.IntPtr&)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegCreateKey(System.IntPtr,System.String,System.IntPtr&)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegDeleteKey(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegDeleteValue(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegEnumKey(System.IntPtr,System.Int32,System.Text.StringBuilder,System.Int32)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegEnumValue(System.IntPtr,System.Int32,System.Text.StringBuilder,System.Int32&,System.IntPtr,Microsoft.Win32.RegistryValueKind&,System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegFlushKey(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegOpenKeyEx(System.IntPtr,System.String,System.IntPtr,System.Int32,System.IntPtr&)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegQueryValueEx(System.IntPtr,System.String,System.IntPtr,Microsoft.Win32.RegistryValueKind&,System.Byte[],System.Int32&)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegQueryValueEx(System.IntPtr,System.String,System.IntPtr,Microsoft.Win32.RegistryValueKind&,System.Int32&,System.Int32&)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegQueryValueEx(System.IntPtr,System.String,System.IntPtr,Microsoft.Win32.RegistryValueKind&,System.IntPtr,System.Int32&)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegSetValueEx(System.IntPtr,System.String,System.IntPtr,Microsoft.Win32.RegistryValueKind,System.Byte[],System.Int32)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegSetValueEx(System.IntPtr,System.String,System.IntPtr,Microsoft.Win32.RegistryValueKind,System.Int32&,System.Int32)

# p/invoke declaration
+SC-M: System.Int32 Microsoft.Win32.Win32RegistryApi::RegSetValueEx(System.IntPtr,System.String,System.IntPtr,Microsoft.Win32.RegistryValueKind,System.String,System.Int32)

# localloc
+SC-M: System.Int32 Mono.Globalization.Unicode.SimpleCollator::Compare(System.String,System.Int32,System.Int32,System.String,System.Int32,System.Int32,System.Globalization.CompareOptions)

# localloc
+SC-M: System.Int32 Mono.Globalization.Unicode.SimpleCollator::IndexOf(System.String,System.Char,System.Int32,System.Int32,System.Globalization.CompareOptions)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 Mono.Globalization.Unicode.SimpleCollator::IndexOf(System.String,System.String,System.Int32,System.Int32,System.Byte*,Mono.Globalization.Unicode.SimpleCollator/Context&)

# localloc
+SC-M: System.Int32 Mono.Globalization.Unicode.SimpleCollator::IndexOf(System.String,System.String,System.Int32,System.Int32,System.Globalization.CompareOptions)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 Mono.Globalization.Unicode.SimpleCollator::IndexOfSortKey(System.String,System.Int32,System.Int32,System.Byte*,System.Char,System.Int32,System.Boolean,Mono.Globalization.Unicode.SimpleCollator/Context&)

# localloc
+SC-M: System.Int32 Mono.Globalization.Unicode.SimpleCollator::LastIndexOf(System.String,System.Char,System.Int32,System.Int32,System.Globalization.CompareOptions)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 Mono.Globalization.Unicode.SimpleCollator::LastIndexOf(System.String,System.String,System.Int32,System.Int32,System.Byte*,Mono.Globalization.Unicode.SimpleCollator/Context&)

# localloc
+SC-M: System.Int32 Mono.Globalization.Unicode.SimpleCollator::LastIndexOf(System.String,System.String,System.Int32,System.Int32,System.Globalization.CompareOptions)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 Mono.Globalization.Unicode.SimpleCollator::LastIndexOfSortKey(System.String,System.Int32,System.Int32,System.Int32,System.Byte*,System.Int32,System.Boolean,Mono.Globalization.Unicode.SimpleCollator/Context&)

# p/invoke declaration
+SC-M: System.Int32 System.__ComObject::CoCreateInstance(System.Guid,System.IntPtr,System.UInt32,System.Guid,System.IntPtr&)

# p/invoke declaration
+SC-M: System.Int32 System.Console/WindowsConsole::GetConsoleCP()

# p/invoke declaration
+SC-M: System.Int32 System.Console/WindowsConsole::GetConsoleOutputCP()

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.IntPtr::op_Explicit(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.IO.FileStream::ReadData(System.IntPtr,System.Byte[],System.Int32,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.IO.MonoIO::Read(System.IntPtr,System.Byte[],System.Int32,System.Int32,System.IO.MonoIOError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.IO.MonoIO::Write(System.IntPtr,System.Byte[],System.Int32,System.Int32,System.IO.MonoIOError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Reflection.Module::GetMDStreamVersion(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Runtime.InteropServices.Marshal::AddRef(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Runtime.InteropServices.Marshal::AddRefInternal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Runtime.InteropServices.Marshal::QueryInterface(System.IntPtr,System.Guid&,System.IntPtr&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Runtime.InteropServices.Marshal::QueryInterfaceInternal(System.IntPtr,System.Guid&,System.IntPtr&)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Runtime.InteropServices.Marshal::ReadInt32(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Runtime.InteropServices.Marshal::ReadInt32(System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Runtime.InteropServices.Marshal::Release(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Runtime.InteropServices.Marshal::ReleaseInternal(System.IntPtr)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.ASCIIEncoding::GetByteCount(System.Char*,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.ASCIIEncoding::GetBytes(System.Char*,System.Int32,System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.ASCIIEncoding::GetCharCount(System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.ASCIIEncoding::GetChars(System.Byte*,System.Int32,System.Char*,System.Int32)

# [VISIBLE] using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.Decoder::GetCharCount(System.Byte*,System.Int32,System.Boolean)

# [VISIBLE] using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.Decoder::GetChars(System.Byte*,System.Int32,System.Char*,System.Int32,System.Boolean)

# [VISIBLE] using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.Encoder::GetByteCount(System.Char*,System.Int32,System.Boolean)

# [VISIBLE] using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.Encoder::GetBytes(System.Char*,System.Int32,System.Byte*,System.Int32,System.Boolean)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.Encoding::GetByteCount(System.Char*,System.Int32)

# [VISIBLE] using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.Encoding::GetBytes(System.Char*,System.Int32,System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.Encoding::GetCharCount(System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.Encoding::GetChars(System.Byte*,System.Int32,System.Char*,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UnicodeEncoding::GetByteCount(System.Char*,System.Int32)

# [VISIBLE] using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UnicodeEncoding::GetBytes(System.Char*,System.Int32,System.Byte*,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UnicodeEncoding::GetBytesInternal(System.Char*,System.Int32,System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UnicodeEncoding::GetCharCount(System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UnicodeEncoding::GetChars(System.Byte*,System.Int32,System.Char*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UnicodeEncoding::GetCharsInternal(System.Byte*,System.Int32,System.Char*,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UTF32Encoding::GetByteCount(System.Char*,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UTF32Encoding::GetBytes(System.Char*,System.Int32,System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UTF32Encoding::GetCharCount(System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UTF32Encoding::GetChars(System.Byte*,System.Int32,System.Char*,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UTF7Encoding::GetByteCount(System.Char*,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UTF7Encoding::GetBytes(System.Char*,System.Int32,System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UTF7Encoding::GetCharCount(System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UTF7Encoding::GetChars(System.Byte*,System.Int32,System.Char*,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding/UTF8Encoder::GetByteCount(System.Char*,System.Int32,System.Boolean)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding/UTF8Encoder::GetBytes(System.Char*,System.Int32,System.Byte*,System.Int32,System.Boolean)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding::Fallback(System.Object,System.Text.DecoderFallbackBuffer&,System.Byte[]&,System.Byte*,System.Int64,System.UInt32)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding::GetByteCount(System.Char*,System.Int32)

# [VISIBLE] using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding::GetBytes(System.Char*,System.Int32,System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding::GetCharCount(System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding::GetChars(System.Byte*,System.Int32,System.Char*,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding::InternalGetByteCount(System.Char*,System.Int32,System.Char&,System.Boolean)

# using 'System.Char*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding::InternalGetBytes(System.Char*,System.Int32,System.Byte*,System.Int32,System.Char&,System.Boolean)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding::InternalGetCharCount(System.Byte*,System.Int32,System.UInt32,System.UInt32,System.Object,System.Text.DecoderFallbackBuffer&,System.Byte[]&,System.Boolean)

# using 'System.Byte*' as a parameter type
+SC-M: System.Int32 System.Text.UTF8Encoding::InternalGetChars(System.Byte*,System.Int32,System.Char*,System.Int32,System.UInt32&,System.UInt32&,System.Object,System.Text.DecoderFallbackBuffer&,System.Byte[]&,System.Boolean)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Int64 System.IntPtr::op_Explicit(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int64 System.IO.MonoIO::GetLength(System.IntPtr,System.IO.MonoIOError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int64 System.IO.MonoIO::Seek(System.IntPtr,System.Int64,System.IO.SeekOrigin,System.IO.MonoIOError&)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Int64 System.Runtime.InteropServices.Marshal::ReadInt64(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Int64 System.Runtime.InteropServices.Marshal::ReadInt64(System.IntPtr,System.Int32)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Microsoft.Win32.Win32RegistryApi::GetHandle(Microsoft.Win32.RegistryKey)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Globalization.Unicode.MSCompatUnicodeTable::GetResource(System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.__ComObject::get_IDispatch()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.__ComObject::get_IUnknown()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.__ComObject::GetInterface(System.Type)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.__ComObject::GetInterface(System.Type,System.Boolean)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.__ComObject::GetInterfaceInternal(System.Type,System.Boolean)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.ArgIterator::IntGetNextArgType()

# [VISIBLE] using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.IntPtr::op_Explicit(System.Int32)

# [VISIBLE] using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.IntPtr::op_Explicit(System.Int64)

# [VISIBLE] using 'System.Void*' as a parameter type
+SC-M: System.IntPtr System.IntPtr::op_Explicit(System.Void*)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.IO.FileStream::get_Handle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.IO.IntPtrStream::get_BaseAddress()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.IO.MonoIO::get_ConsoleError()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.IO.MonoIO::get_ConsoleInput()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.IO.MonoIO::get_ConsoleOutput()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.IO.MonoIO::Open(System.String,System.IO.FileMode,System.IO.FileAccess,System.IO.FileShare,System.IO.FileOptions,System.IO.MonoIOError&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.ModuleHandle::get_Value()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Object::obj_address()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Reflection.Assembly::GetManifestResourceInternal(System.String,System.Int32&,System.Reflection.Module&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Reflection.Module::GetHINSTANCE()

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Reflection.Module::ResolveFieldToken(System.IntPtr,System.Int32,System.IntPtr[],System.IntPtr[],System.Reflection.ResolveTokenError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Reflection.Module::ResolveMethodToken(System.IntPtr,System.Int32,System.IntPtr[],System.IntPtr[],System.Reflection.ResolveTokenError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Reflection.Module::ResolveTypeToken(System.IntPtr,System.Int32,System.IntPtr[],System.IntPtr[],System.Reflection.ResolveTokenError&)

# [VISIBLE] using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.GCHandle::AddrOfPinnedObject()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.GCHandle::GetAddrOfPinnedObject(System.Int32)

# [VISIBLE] using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.GCHandle::op_Explicit(System.Runtime.InteropServices.GCHandle)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.GCHandle::ToIntPtr(System.Runtime.InteropServices.GCHandle)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.HandleRef::get_Handle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.HandleRef::op_Explicit(System.Runtime.InteropServices.HandleRef)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.HandleRef::ToIntPtr(System.Runtime.InteropServices.HandleRef)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.ICustomMarshaler::MarshalManagedToNative(System.Object)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::AllocCoTaskMem(System.Int32)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::AllocHGlobal(System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::AllocHGlobal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::CreateAggregatedObject(System.IntPtr,System.Object)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetCCW(System.Object,System.Type)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetComInterfaceForObject(System.Object,System.Type)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetComInterfaceForObjectInContext(System.Object,System.Type)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetComInterfaceForObjectInternal(System.Object,System.Type)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetExceptionPointers()

# [VISIBLE] using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetFunctionPointerForDelegate(System.Delegate)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetFunctionPointerForDelegateInternal(System.Delegate)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetHINSTANCE(System.Reflection.Module)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetIDispatchForObject(System.Object)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetIDispatchForObjectInContext(System.Object)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetIDispatchForObjectInternal(System.Object)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetITypeInfoForType(System.Type)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetIUnknownForObject(System.Object)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetIUnknownForObjectInContext(System.Object)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetIUnknownForObjectInternal(System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetManagedThunkForUnmanagedMethodPtr(System.IntPtr,System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::GetUnmanagedThunkForManagedMethodPtr(System.IntPtr,System.IntPtr,System.Int32)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::OffsetOf(System.Type,System.String)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::ReadIntPtr(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::ReadIntPtr(System.IntPtr,System.Int32)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::ReadIntPtr(System.Object,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::ReAllocCoTaskMem(System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::ReAllocHGlobal(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::SecureStringToBSTR(System.Security.SecureString)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::SecureStringToCoTaskMemAnsi(System.Security.SecureString)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::SecureStringToCoTaskMemUnicode(System.Security.SecureString)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::SecureStringToGlobalAllocAnsi(System.Security.SecureString)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::SecureStringToGlobalAllocUnicode(System.Security.SecureString)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::StringToBSTR(System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::StringToCoTaskMemAnsi(System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::StringToCoTaskMemAuto(System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::StringToCoTaskMemUni(System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::StringToHGlobalAnsi(System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::StringToHGlobalAuto(System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::StringToHGlobalUni(System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.Marshal::UnsafeAddrOfPinnedArrayElement(System.Array,System.Int32)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.ObjectCreationDelegate::EndInvoke(System.IAsyncResult)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Runtime.InteropServices.ObjectCreationDelegate::Invoke(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.InteropServices.SafeHandle::DangerousGetHandle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.Remoting.Proxies.RealProxy::GetCOMIUnknown(System.Boolean)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Runtime.Remoting.Proxies.RealProxy::SupportsInterface(System.Guid&)

# [VISIBLE] using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.RuntimeFieldHandle::get_Value()

# [VISIBLE] using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.RuntimeMethodHandle::get_Value()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.RuntimeMethodHandle::GetFunctionPointer()

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.RuntimeMethodHandle::GetFunctionPointer(System.IntPtr)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.RuntimeTypeHandle::get_Value()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Security.Cryptography.CryptoAPITransform::get_KeyHandle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Security.Cryptography.CspParameters::get_ParentWindowHandle()

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Security.Cryptography.RNGCryptoServiceProvider::RngGetBytes(System.IntPtr,System.Byte[])

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Security.Cryptography.RNGCryptoServiceProvider::RngInitialize(System.Byte[])

# [VISIBLE] using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Security.Cryptography.X509Certificates.X509Certificate::get_Handle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Security.Principal.WindowsIdentity::get_Token()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Security.Principal.WindowsIdentity::GetCurrentToken()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Security.Principal.WindowsIdentity::GetUserToken(System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Security.Principal.WindowsImpersonationContext::DuplicateToken(System.IntPtr)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Security.Principal.WindowsPrincipal::get_Token()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Security.SecurityContext::get_IdentityToken()

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Threading.Interlocked::CompareExchange(System.IntPtr&,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Threading.Interlocked::Exchange(System.IntPtr&,System.IntPtr)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Threading.Mutex::CreateMutex_internal(System.Boolean,System.String,System.Boolean&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Threading.Mutex::OpenMutex_internal(System.String,System.Security.AccessControl.MutexRights,System.IO.MonoIOError&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Threading.NativeEventCalls::CreateEvent_internal(System.Boolean,System.Boolean,System.String,System.Boolean&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Threading.NativeEventCalls::OpenEvent_internal(System.String,System.Security.AccessControl.EventWaitHandleRights,System.IO.MonoIOError&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Threading.Thread::Thread_internal(System.MulticastDelegate)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Threading.Thread::VolatileRead(System.IntPtr&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Threading.WaitHandle::get_Handle()

# using 'System.IntPtr' as a parameter type
+SC-M: System.IO.MonoFileType System.IO.MonoIO::GetFileType(System.IntPtr,System.IO.MonoIOError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Object System.Runtime.InteropServices.ICustomMarshaler::MarshalNativeToManaged(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Object System.Runtime.InteropServices.Marshal::GetObjectForCCW(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Object System.Runtime.InteropServices.Marshal::GetObjectForIUnknown(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Object System.Runtime.InteropServices.Marshal::GetObjectForNativeVariant(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Object System.Runtime.InteropServices.Marshal::GetTypedObjectForIUnknown(System.IntPtr,System.Type)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Object System.Runtime.InteropServices.Marshal::GetUniqueObjectForIUnknown(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Object System.Runtime.InteropServices.Marshal::PtrToStructure(System.IntPtr,System.Type)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Object[] System.Runtime.InteropServices.Marshal::GetObjectsForNativeVariants(System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Reflection.Emit.UnmanagedMarshal System.Reflection.MonoMethodInfo::get_retval_marshal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Reflection.FieldInfo System.Reflection.FieldInfo::internal_from_handle_type(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Reflection.MemberInfo System.Reflection.Module::ResolveMemberToken(System.IntPtr,System.Int32,System.IntPtr[],System.IntPtr[],System.Reflection.ResolveTokenError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Reflection.MethodBase System.Reflection.MethodBase::GetMethodFromHandleInternalType(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Reflection.MethodBody System.Reflection.MethodBase::GetMethodBody(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Reflection.MethodBody System.Reflection.MethodBase::GetMethodBodyInternal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Reflection.ParameterInfo[] System.Reflection.MonoMethodInfo::get_parameter_info(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Runtime.InteropServices.DllImportAttribute System.Reflection.MonoMethod::GetDllImportAttribute(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Runtime.InteropServices.GCHandle System.Runtime.InteropServices.GCHandle::FromIntPtr(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Runtime.InteropServices.GCHandle System.Runtime.InteropServices.GCHandle::op_Explicit(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Security.PermissionSet System.Security.SecurityManager::Decode(System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Security.Principal.WindowsImpersonationContext System.Security.Principal.WindowsIdentity::Impersonate(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String System.Reflection.Module::ResolveStringToken(System.IntPtr,System.Int32,System.Reflection.ResolveTokenError&)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.String System.Runtime.InteropServices.Marshal::PtrToStringAnsi(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.String System.Runtime.InteropServices.Marshal::PtrToStringAnsi(System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String System.Runtime.InteropServices.Marshal::PtrToStringAuto(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String System.Runtime.InteropServices.Marshal::PtrToStringAuto(System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String System.Runtime.InteropServices.Marshal::PtrToStringBSTR(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.String System.Runtime.InteropServices.Marshal::PtrToStringUni(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.String System.Runtime.InteropServices.Marshal::PtrToStringUni(System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String System.Security.Principal.WindowsIdentity::GetTokenName(System.IntPtr)

# arglist
+SC-M: System.String System.String::Concat(System.Object,System.Object,System.Object,System.Object)

# using 'System.Char*' as a parameter type
+SC-M: System.String System.String::CreateString(System.Char*)

# using 'System.Char*' as a parameter type
+SC-M: System.String System.String::CreateString(System.Char*,System.Int32,System.Int32)

# using 'System.SByte*' as a parameter type
+SC-M: System.String System.String::CreateString(System.SByte*)

# using 'System.SByte*' as a parameter type
+SC-M: System.String System.String::CreateString(System.SByte*,System.Int32,System.Int32)

# using 'System.SByte*' as a parameter type
+SC-M: System.String System.String::CreateString(System.SByte*,System.Int32,System.Int32,System.Text.Encoding)

# localloc
+SC-M: System.String System.String::ReplaceUnchecked(System.String,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String[] System.Security.Principal.WindowsIdentity::_GetRoles(System.IntPtr)

# [VISIBLE] using 'System.Threading.NativeOverlapped*' as return type
+SC-M: System.Threading.NativeOverlapped* System.Threading.Overlapped::Pack(System.Threading.IOCompletionCallback,System.Object)

# [VISIBLE] using 'System.Threading.NativeOverlapped*' as a parameter type
+SC-M: System.Threading.Overlapped System.Threading.Overlapped::Unpack(System.Threading.NativeOverlapped*)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Type System.Runtime.InteropServices.Marshal::GetTypeForITypeInfo(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Type System.Type::internal_from_handle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.TypedReference System.ArgIterator::IntGetNextArg(System.IntPtr)

# using 'System.Byte*' as a parameter type
+SC-M: System.UInt32 Mono.Globalization.Unicode.MSCompatUnicodeTable::UInt32FromBytePtr(System.Byte*,System.UInt32)

# [VISIBLE] using 'System.UIntPtr' as a parameter type
+SC-M: System.UInt32 System.UIntPtr::op_Explicit(System.UIntPtr)

# [VISIBLE] using 'System.UIntPtr' as a parameter type
+SC-M: System.UInt64 System.UIntPtr::op_Explicit(System.UIntPtr)

# using 'System.UIntPtr' as return type
+SC-M: System.UIntPtr System.Threading.Thread::VolatileRead(System.UIntPtr&)

# [VISIBLE] using 'System.UIntPtr' as return type
+SC-M: System.UIntPtr System.UIntPtr::op_Explicit(System.UInt32)

# [VISIBLE] using 'System.UIntPtr' as return type
+SC-M: System.UIntPtr System.UIntPtr::op_Explicit(System.UInt64)

# [VISIBLE] using 'System.Void*' as a parameter type
+SC-M: System.UIntPtr System.UIntPtr::op_Explicit(System.Void*)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Microsoft.Win32.RegistryKey::.ctor(Microsoft.Win32.RegistryHive,System.IntPtr,System.Boolean)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void Microsoft.Win32.SafeHandles.SafeWaitHandle::.ctor(System.IntPtr,System.Boolean)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void Mono.Globalization.Unicode.SimpleCollator/Context::.ctor(System.Globalization.CompareOptions,System.Byte*,System.Byte*,System.Byte*,System.Byte*,System.Byte*,System.Boolean)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void Mono.Globalization.Unicode.SimpleCollator::ClearBuffer(System.Byte*,System.Int32)

# localloc
+SC-M: System.Void Mono.Globalization.Unicode.SimpleCollator::GetSortKey(System.String,System.Int32,System.Int32,Mono.Globalization.Unicode.SortKeyBuffer,System.Globalization.CompareOptions)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Interop.ComInteropProxy::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Interop.ComInteropProxy::.ctor(System.IntPtr,System.Type)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Interop.ComInteropProxy::AddProxy(System.IntPtr,Mono.Interop.ComInteropProxy)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void Mono.Security.BitConverterLE::UIntFromBytes(System.Byte*,System.Byte[],System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void Mono.Security.BitConverterLE::ULongFromBytes(System.Byte*,System.Byte[],System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void Mono.Security.BitConverterLE::UShortFromBytes(System.Byte*,System.Byte[],System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.__ComObject::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System._AppDomain::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System._AppDomain::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System._AppDomain::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Activator::System.Runtime.InteropServices._Activator.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Activator::System.Runtime.InteropServices._Activator.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Activator::System.Runtime.InteropServices._Activator.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.AppDomain::System._AppDomain.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.AppDomain::System._AppDomain.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.AppDomain::System._AppDomain.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.Void*' as a parameter type
+SC-M: System.Void System.ArgIterator::.ctor(System.RuntimeArgumentHandle,System.Void*)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.ArgIterator::Setup(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Attribute::System.Runtime.InteropServices._Attribute.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Attribute::System.Runtime.InteropServices._Attribute.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Attribute::System.Runtime.InteropServices._Attribute.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.BitConverter::PutBytes(System.Byte*,System.Byte[],System.Int32,System.Int32)

# arglist
+SC-M: System.Void System.Console::Write(System.String,System.Object,System.Object,System.Object,System.Object)

# arglist
+SC-M: System.Void System.Console::WriteLine(System.String,System.Object,System.Object,System.Object,System.Object)

# localloc
+SC-M: System.Void System.DateTimeUtils::ZeroPad(System.Text.StringBuilder,System.Int32,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Diagnostics.SymbolStore.ISymbolWriter::Initialize(System.IntPtr,System.String,System.Boolean)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Diagnostics.SymbolStore.ISymbolWriter::SetUnderlyingWriter(System.IntPtr)

# using 'System.Void*' as a parameter type
+SC-M: System.Void System.Globalization.TextInfo::.ctor(System.Globalization.CultureInfo,System.Int32,System.Void*,System.Boolean)

# [VISIBLE] using 'System.Void*' as a parameter type
+SC-M: System.Void System.IntPtr::.ctor(System.Void*)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.IO.FileStream::.ctor(System.IntPtr,System.IO.FileAccess,System.Boolean,System.Int32,System.Boolean,System.Boolean)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.IO.MonoIO::Lock(System.IntPtr,System.Int64,System.Int64,System.IO.MonoIOError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.IO.MonoIO::Unlock(System.IntPtr,System.Int64,System.Int64,System.IO.MonoIOError&)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.IO.UnmanagedMemoryStream::.ctor(System.Byte*,System.Int64)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.IO.UnmanagedMemoryStream::.ctor(System.Byte*,System.Int64,System.Int64,System.IO.FileAccess)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.ModuleHandle::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.AssemblyName::System.Runtime.InteropServices._AssemblyName.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.AssemblyName::System.Runtime.InteropServices._AssemblyName.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.AssemblyName::System.Runtime.InteropServices._AssemblyName.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.ConstructorInfo::System.Runtime.InteropServices._ConstructorInfo.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.ConstructorInfo::System.Runtime.InteropServices._ConstructorInfo.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.ConstructorInfo::System.Runtime.InteropServices._ConstructorInfo.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.AssemblyBuilder::System.Runtime.InteropServices._AssemblyBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.AssemblyBuilder::System.Runtime.InteropServices._AssemblyBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.AssemblyBuilder::System.Runtime.InteropServices._AssemblyBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ConstructorBuilder::System.Runtime.InteropServices._ConstructorBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ConstructorBuilder::System.Runtime.InteropServices._ConstructorBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ConstructorBuilder::System.Runtime.InteropServices._ConstructorBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.CustomAttributeBuilder::System.Runtime.InteropServices._CustomAttributeBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.CustomAttributeBuilder::System.Runtime.InteropServices._CustomAttributeBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.CustomAttributeBuilder::System.Runtime.InteropServices._CustomAttributeBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.EnumBuilder::System.Runtime.InteropServices._EnumBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.EnumBuilder::System.Runtime.InteropServices._EnumBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.EnumBuilder::System.Runtime.InteropServices._EnumBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.EventBuilder::System.Runtime.InteropServices._EventBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.EventBuilder::System.Runtime.InteropServices._EventBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.EventBuilder::System.Runtime.InteropServices._EventBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.FieldBuilder::System.Runtime.InteropServices._FieldBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.FieldBuilder::System.Runtime.InteropServices._FieldBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.FieldBuilder::System.Runtime.InteropServices._FieldBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ILGenerator::System.Runtime.InteropServices._ILGenerator.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ILGenerator::System.Runtime.InteropServices._ILGenerator.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ILGenerator::System.Runtime.InteropServices._ILGenerator.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.LocalBuilder::System.Runtime.InteropServices._LocalBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.LocalBuilder::System.Runtime.InteropServices._LocalBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.LocalBuilder::System.Runtime.InteropServices._LocalBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.MethodBuilder::System.Runtime.InteropServices._MethodBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.MethodBuilder::System.Runtime.InteropServices._MethodBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.MethodBuilder::System.Runtime.InteropServices._MethodBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ModuleBuilder::System.Runtime.InteropServices._ModuleBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ModuleBuilder::System.Runtime.InteropServices._ModuleBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ModuleBuilder::System.Runtime.InteropServices._ModuleBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ModuleBuilder::WriteToFile(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ParameterBuilder::System.Runtime.InteropServices._ParameterBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ParameterBuilder::System.Runtime.InteropServices._ParameterBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.ParameterBuilder::System.Runtime.InteropServices._ParameterBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.PropertyBuilder::System.Runtime.InteropServices._PropertyBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.PropertyBuilder::System.Runtime.InteropServices._PropertyBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.PropertyBuilder::System.Runtime.InteropServices._PropertyBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.SignatureHelper::System.Runtime.InteropServices._SignatureHelper.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.SignatureHelper::System.Runtime.InteropServices._SignatureHelper.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.SignatureHelper::System.Runtime.InteropServices._SignatureHelper.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.TypeBuilder::System.Runtime.InteropServices._TypeBuilder.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.TypeBuilder::System.Runtime.InteropServices._TypeBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Emit.TypeBuilder::System.Runtime.InteropServices._TypeBuilder.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.EventInfo::System.Runtime.InteropServices._EventInfo.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.EventInfo::System.Runtime.InteropServices._EventInfo.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.EventInfo::System.Runtime.InteropServices._EventInfo.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.FieldInfo::System.Runtime.InteropServices._FieldInfo.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.FieldInfo::System.Runtime.InteropServices._FieldInfo.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.FieldInfo::System.Runtime.InteropServices._FieldInfo.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.MemberInfo::System.Runtime.InteropServices._MemberInfo.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.MemberInfo::System.Runtime.InteropServices._MemberInfo.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.MemberInfo::System.Runtime.InteropServices._MemberInfo.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.MethodBase::System.Runtime.InteropServices._MethodBase.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.MethodBase::System.Runtime.InteropServices._MethodBase.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.MethodBase::System.Runtime.InteropServices._MethodBase.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.MethodInfo::System.Runtime.InteropServices._MethodInfo.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.MethodInfo::System.Runtime.InteropServices._MethodInfo.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.MethodInfo::System.Runtime.InteropServices._MethodInfo.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Module::GetPEKind(System.IntPtr,System.Reflection.PortableExecutableKinds&,System.Reflection.ImageFileMachine&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Module::System.Runtime.InteropServices._Module.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Module::System.Runtime.InteropServices._Module.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.Module::System.Runtime.InteropServices._Module.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.MonoMethodInfo::get_method_info(System.IntPtr,System.Reflection.MonoMethodInfo&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.ParameterInfo::System.Runtime.InteropServices._ParameterInfo.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.ParameterInfo::System.Runtime.InteropServices._ParameterInfo.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.ParameterInfo::System.Runtime.InteropServices._ParameterInfo.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.PropertyInfo::System.Runtime.InteropServices._PropertyInfo.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.PropertyInfo::System.Runtime.InteropServices._PropertyInfo.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Reflection.PropertyInfo::System.Runtime.InteropServices._PropertyInfo.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(System.Array,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.CompilerServices.RuntimeHelpers::RunClassConstructor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.CompilerServices.RuntimeHelpers::RunModuleConstructor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Activator::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Activator::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Activator::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._AssemblyBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._AssemblyBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._AssemblyBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._AssemblyName::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._AssemblyName::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._AssemblyName::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Attribute::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Attribute::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Attribute::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ConstructorBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ConstructorBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ConstructorBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ConstructorInfo::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ConstructorInfo::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ConstructorInfo::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._CustomAttributeBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._CustomAttributeBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._CustomAttributeBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._EnumBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._EnumBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._EnumBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._EventBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._EventBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._EventBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._EventInfo::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._EventInfo::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._EventInfo::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._FieldBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._FieldBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._FieldBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._FieldInfo::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._FieldInfo::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._FieldInfo::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ILGenerator::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ILGenerator::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ILGenerator::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._LocalBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._LocalBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._LocalBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MemberInfo::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MemberInfo::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MemberInfo::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MethodBase::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MethodBase::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MethodBase::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MethodBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MethodBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MethodBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MethodInfo::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MethodInfo::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._MethodInfo::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Module::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Module::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Module::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ModuleBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ModuleBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ModuleBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ParameterBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ParameterBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ParameterBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ParameterInfo::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ParameterInfo::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._ParameterInfo::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._PropertyBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._PropertyBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._PropertyBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._PropertyInfo::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._PropertyInfo::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._PropertyInfo::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._SignatureHelper::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._SignatureHelper::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._SignatureHelper::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Thread::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Thread::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Thread::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Type::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Type::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._Type::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._TypeBuilder::GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._TypeBuilder::GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices._TypeBuilder::Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.ComTypes.ITypeInfo::GetDllEntry(System.Int32,System.Runtime.InteropServices.ComTypes.INVOKEKIND,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.ComTypes.ITypeInfo::Invoke(System.Object,System.Int32,System.Int16,System.Runtime.InteropServices.ComTypes.DISPPARAMS&,System.IntPtr,System.IntPtr,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.ComTypes.ITypeInfo::ReleaseFuncDesc(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.ComTypes.ITypeInfo::ReleaseTypeAttr(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.ComTypes.ITypeInfo::ReleaseVarDesc(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.ComTypes.ITypeLib::ReleaseTLibAttr(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.CriticalHandle::.ctor(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.CriticalHandle::SetHandle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.GCHandle::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.HandleRef::.ctor(System.Object,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.ICustomMarshaler::CleanUpNativeData(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::ClearAnsi(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::ClearBSTR(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::ClearUnicode(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.Byte[],System.Int32,System.IntPtr,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.Char[],System.Int32,System.IntPtr,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.Double[],System.Int32,System.IntPtr,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.Int16[],System.Int32,System.IntPtr,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.Int32[],System.Int32,System.IntPtr,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.Int64[],System.Int32,System.IntPtr,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.IntPtr,System.Char[],System.Int32,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.IntPtr,System.Double[],System.Int32,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.IntPtr,System.Int16[],System.Int32,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.IntPtr,System.Int32[],System.Int32,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.IntPtr,System.Int64[],System.Int32,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.IntPtr,System.IntPtr[],System.Int32,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.IntPtr,System.Single[],System.Int32,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.IntPtr[],System.Int32,System.IntPtr,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::Copy(System.Single[],System.Int32,System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::copy_from_unmanaged(System.IntPtr,System.Int32,System.Array,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::copy_to_unmanaged(System.Array,System.Int32,System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::DestroyStructure(System.IntPtr,System.Type)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::FreeBSTR(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::FreeCoTaskMem(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::FreeHGlobal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::GetNativeVariantForObject(System.Object,System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::PtrToStructure(System.IntPtr,System.Object)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::StructureToPtr(System.Object,System.IntPtr,System.Boolean)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::ThrowExceptionForHR(System.Int32,System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteByte(System.IntPtr,System.Byte)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteByte(System.IntPtr,System.Int32,System.Byte)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteInt16(System.IntPtr,System.Char)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteInt16(System.IntPtr,System.Int16)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteInt16(System.IntPtr,System.Int32,System.Char)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteInt16(System.IntPtr,System.Int32,System.Int16)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteInt32(System.IntPtr,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteInt32(System.IntPtr,System.Int32,System.Int32)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteInt64(System.IntPtr,System.Int32,System.Int64)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteInt64(System.IntPtr,System.Int64)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteIntPtr(System.IntPtr,System.Int32,System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteIntPtr(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::WriteIntPtr(System.Object,System.Int32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::ZeroFreeBSTR(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::ZeroFreeCoTaskMemAnsi(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::ZeroFreeCoTaskMemUnicode(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::ZeroFreeGlobalAllocAnsi(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.Marshal::ZeroFreeGlobalAllocUnicode(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.SafeHandle::.ctor(System.IntPtr,System.Boolean)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.SafeHandle::SetHandle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.UCOMITypeInfo::ReleaseFuncDesc(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.UCOMITypeInfo::ReleaseTypeAttr(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.UCOMITypeInfo::ReleaseVarDesc(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.InteropServices.UCOMITypeLib::ReleaseTLibAttr(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.Remoting.Proxies.RealProxy::.ctor(System.Type,System.IntPtr,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Runtime.Remoting.Proxies.RealProxy::SetCOMIUnknown(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.RuntimeFieldHandle::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.RuntimeMethodHandle::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.RuntimeTypeHandle::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Cryptography.CspParameters::.ctor(System.Int32,System.String,System.String,System.Security.AccessControl.CryptoKeySecurity,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Cryptography.CspParameters::set_ParentWindowHandle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Cryptography.RNGCryptoServiceProvider::RngClose(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Cryptography.X509Certificates.X509Certificate::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Cryptography.X509Certificates.X509Certificate::InitFromHandle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Principal.WindowsIdentity::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Principal.WindowsIdentity::.ctor(System.IntPtr,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Principal.WindowsIdentity::.ctor(System.IntPtr,System.String,System.Security.Principal.WindowsAccountType)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Principal.WindowsIdentity::.ctor(System.IntPtr,System.String,System.Security.Principal.WindowsAccountType,System.Boolean)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Principal.WindowsIdentity::SetToken(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.Principal.WindowsImpersonationContext::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.SecurityManager::InternalDemand(System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.SecurityManager::InternalDemandChoice(System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.SecurityManager::LinkDemandSecurityException(System.Int32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Security.SecurityManager::MethodAccessException(System.IntPtr,System.IntPtr)

# [VISIBLE] using 'System.Char*' as a parameter type
+SC-M: System.Void System.String::.ctor(System.Char*)

# [VISIBLE] using 'System.Char*' as a parameter type
+SC-M: System.Void System.String::.ctor(System.Char*,System.Int32,System.Int32)

# [VISIBLE] using 'System.SByte*' as a parameter type
+SC-M: System.Void System.String::.ctor(System.SByte*)

# using 'System.SByte*' as a parameter type
+SC-M: System.Void System.String::.ctor(System.SByte*,System.Int32,System.Int32)

# using 'System.SByte*' as a parameter type
+SC-M: System.Void System.String::.ctor(System.SByte*,System.Int32,System.Int32,System.Text.Encoding)

# using 'System.Char*' as a parameter type
+SC-M: System.Void System.String::CharCopy(System.Char*,System.Char*,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Void System.String::CharCopyReverse(System.Char*,System.Char*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.String::memcpy(System.Byte*,System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.String::memcpy1(System.Byte*,System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.String::memcpy2(System.Byte*,System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.String::memcpy4(System.Byte*,System.Byte*,System.Int32)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.String::memset(System.Byte*,System.Int32,System.Int32)

# using 'System.Char*' as a parameter type
+SC-M: System.Void System.Text.Decoder::CheckArguments(System.Char*,System.Int32,System.Byte*,System.Int32)

# [VISIBLE] using 'System.Byte*' as a parameter type
+SC-M: System.Void System.Text.Decoder::Convert(System.Byte*,System.Int32,System.Char*,System.Int32,System.Boolean,System.Int32&,System.Int32&,System.Boolean&)

# using 'System.Char*' as a parameter type
+SC-M: System.Void System.Text.Encoder::CheckArguments(System.Char*,System.Int32,System.Byte*,System.Int32)

# [VISIBLE] using 'System.Char*' as a parameter type
+SC-M: System.Void System.Text.Encoder::Convert(System.Char*,System.Int32,System.Byte*,System.Int32,System.Boolean,System.Int32&,System.Int32&,System.Boolean&)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.Text.UnicodeEncoding::CopyChars(System.Byte*,System.Byte*,System.Int32,System.Boolean)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.Text.UTF8Encoding::Fallback(System.Object,System.Text.DecoderFallbackBuffer&,System.Byte[]&,System.Byte*,System.Int64,System.UInt32,System.Char*,System.Int32&)

# [VISIBLE] using 'System.Threading.NativeOverlapped*' as a parameter type
+SC-M: System.Void System.Threading.IOCompletionCallback::Invoke(System.UInt32,System.UInt32,System.Threading.NativeOverlapped*)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Threading.Mutex::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Threading.NativeEventCalls::CloseEvent_internal(System.IntPtr)

# [VISIBLE] using 'System.Threading.NativeOverlapped*' as a parameter type
+SC-M: System.Void System.Threading.Overlapped::Free(System.Threading.NativeOverlapped*)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Threading.Thread::System.Runtime.InteropServices._Thread.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Threading.Thread::System.Runtime.InteropServices._Thread.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Threading.Thread::System.Runtime.InteropServices._Thread.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Threading.Thread::Thread_free_internal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Threading.Thread::VolatileWrite(System.IntPtr&,System.IntPtr)

# using 'System.UIntPtr' as a parameter type
+SC-M: System.Void System.Threading.Thread::VolatileWrite(System.UIntPtr&,System.UIntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Threading.WaitHandle::set_Handle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Type::System.Runtime.InteropServices._Type.GetIDsOfNames(System.Guid&,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Type::System.Runtime.InteropServices._Type.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Type::System.Runtime.InteropServices._Type.Invoke(System.UInt32,System.Guid&,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# [VISIBLE] using 'System.Void*' as a parameter type
+SC-M: System.Void System.UIntPtr::.ctor(System.Void*)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void* System.IntPtr::op_Explicit(System.IntPtr)

# [VISIBLE] using 'System.Void*' as return type
+SC-M: System.Void* System.IntPtr::ToPointer()

# [VISIBLE] using 'System.UIntPtr' as a parameter type
+SC-M: System.Void* System.UIntPtr::op_Explicit(System.UIntPtr)

# [VISIBLE] using 'System.Void*' as return type
+SC-M: System.Void* System.UIntPtr::ToPointer()

