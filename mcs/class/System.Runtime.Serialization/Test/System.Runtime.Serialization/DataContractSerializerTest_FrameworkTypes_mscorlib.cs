//
// DataContractSerializerTest_FrameworkTypes_mscorlib.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft.co http://www.mainsoft.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//
// This test code contains tests for attributes in System.Runtime.Serialization
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.Globalization;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	[Category ("NotWorking")]
	public partial class DataContractSerializerTest_FrameworkTypes_mscorlib
		: DataContractSerializerTest_FrameworkTypes
	{
		[Test]
		public void System_Object () {
			Test<global::System.Object> ();
		}
		[Test]
		public void System_StringSplitOptions () {
			Test<global::System.StringSplitOptions> ();
		}
		[Test]
		public void System_StringComparison () {
			Test<global::System.StringComparison> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Text_StringBuilder () {
			Test<global::System.Text.StringBuilder> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Exception () {
			Test<global::System.Exception> ();
		}
		[Test]
		public void System_DateTime () {
			Test<global::System.DateTime> ();
		}
		[Test]
		public void System_DateTimeKind () {
			Test<global::System.DateTimeKind> ();
		}
		[Test]
		public void System_DateTimeOffset () {
			Test<global::System.DateTimeOffset> ();
		}
		[Test]
		public void System_SystemException () {
			Test<global::System.SystemException> ();
		}
		[Test]
		public void System_OutOfMemoryException () {
			Test<global::System.OutOfMemoryException> ();
		}
		[Test]
		public void System_StackOverflowException () {
			Test<global::System.StackOverflowException> ();
		}
		[Test]
		public void System_DataMisalignedException () {
			Test<global::System.DataMisalignedException> ();
		}
		[Test]
		public void System_ExecutionEngineException () {
			Test<global::System.ExecutionEngineException> ();
		}
		[Test]
		public void System_MemberAccessException () {
			Test<global::System.MemberAccessException> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_AccessViolationException () {
			Test<global::System.AccessViolationException> ();
		}
		[Test]
		public void System_ApplicationException () {
			Test<global::System.ApplicationException> ();
		}
		[Test]
		public void System_EventArgs () {
			Test<global::System.EventArgs> ();
		}
		[Test]
		public void System_AppDomainManagerInitializationOptions () {
			Test<global::System.AppDomainManagerInitializationOptions> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_AppDomainSetup () {
			Test<global::System.AppDomainSetup> ();
		}
		[Test]
		public void System_LoaderOptimization () {
			Test<global::System.LoaderOptimization> ();
		}
		[Test]
		public void System_AppDomainUnloadedException () {
			Test<global::System.AppDomainUnloadedException> ();
		}
		[Test]
		public void System_ActivationContext_ContextForm () {
			Test<global::System.ActivationContext.ContextForm> ();
		}
		[Test]
		public void System_ArgumentException () {
			Test<global::System.ArgumentException> ();
		}
		[Test]
		public void System_ArgumentNullException () {
			Test<global::System.ArgumentNullException> ();
		}
		[Test]
		public void System_ArgumentOutOfRangeException () {
			Test<global::System.ArgumentOutOfRangeException> ();
		}
		[Test]
		public void System_ArithmeticException () {
			Test<global::System.ArithmeticException> ();
		}
		[Test]
		public void System_ArrayTypeMismatchException () {
			Test<global::System.ArrayTypeMismatchException> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_AttributeTargets () {
			Test<global::System.AttributeTargets> ();
		}
		[Test]
		public void System_BadImageFormatException () {
			Test<global::System.BadImageFormatException> ();
		}
		[Test]
		public void System_Boolean () {
			Test<global::System.Boolean> ();
		}
		[Test]
		public void System_Byte () {
			Test<global::System.Byte> ();
		}
		[Test]
		public void System_CannotUnloadAppDomainException () {
			Test<global::System.CannotUnloadAppDomainException> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Char () {
			Test<global::System.Char> ();
		}
		[Test]
		public void System_TypeUnloadedException () {
			Test<global::System.TypeUnloadedException> ();
		}
		[Test]
		public void System_ConsoleColor () {
			Test<global::System.ConsoleColor> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_ConsoleModifiers () {
			Test<global::System.ConsoleModifiers> ();
		}
		[Test]
		public void System_ConsoleSpecialKey () {
			Test<global::System.ConsoleSpecialKey> ();
		}
		[Test]
		public void System_ContextMarshalException () {
			Test<global::System.ContextMarshalException> ();
		}
		[Test]
		public void System_Base64FormattingOptions () {
			Test<global::System.Base64FormattingOptions> ();
		}
		[Test]
		public void System_ContextStaticAttribute () {
			Test<global::System.ContextStaticAttribute> ();
		}
		[Test]
		public void System_DayOfWeek () {
			Test<global::System.DayOfWeek> ();
		}
		[Test]
		public void System_Decimal () {
			Test<global::System.Decimal> ();
		}
		[Test]
		public void System_DivideByZeroException () {
			Test<global::System.DivideByZeroException> ();
		}
		[Test]
		public void System_Double () {
			Test<global::System.Double> ();
		}
		[Test]
		public void System_DuplicateWaitObjectException () {
			Test<global::System.DuplicateWaitObjectException> ();
		}
		[Test]
		public void System_TypeLoadException () {
			Test<global::System.TypeLoadException> ();
		}
		[Test]
		public void System_EntryPointNotFoundException () {
			Test<global::System.EntryPointNotFoundException> ();
		}
		[Test]
		public void System_DllNotFoundException () {
			Test<global::System.DllNotFoundException> ();
		}
		[Test]
		public void System_EnvironmentVariableTarget () {
			Test<global::System.EnvironmentVariableTarget> ();
		}
		[Test]
		public void System_Environment_SpecialFolder () {
			Test<global::System.Environment.SpecialFolder> ();
		}
		[Test]
		public void System_FieldAccessException () {
			Test<global::System.FieldAccessException> ();
		}
		[Test]
		public void System_FlagsAttribute () {
			Test<global::System.FlagsAttribute> ();
		}
		[Test]
		public void System_FormatException () {
			Test<global::System.FormatException> ();
		}
		[Test]
		public void System_GCCollectionMode () {
			Test<global::System.GCCollectionMode> ();
		}
		[Test]
		public void System_Guid () {
			Test<global::System.Guid> ();
		}
		[Test]
		public void System_IndexOutOfRangeException () {
			Test<global::System.IndexOutOfRangeException> ();
		}
		[Test]
		public void System_InsufficientMemoryException () {
			Test<global::System.InsufficientMemoryException> ();
		}
		[Test]
		public void System_Int16 () {
			Test<global::System.Int16> ();
		}
		[Test]
		public void System_Int32 () {
			Test<global::System.Int32> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Int64 () {
			Test<global::System.Int64> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_IntPtr () {
			Test<global::System.IntPtr> ();
		}
		[Test]
		public void System_InvalidCastException () {
			Test<global::System.InvalidCastException> ();
		}
		[Test]
		public void System_InvalidOperationException () {
			Test<global::System.InvalidOperationException> ();
		}
		[Test]
		public void System_InvalidProgramException () {
			Test<global::System.InvalidProgramException> ();
		}
		[Test]
		public void System_MethodAccessException () {
			Test<global::System.MethodAccessException> ();
		}
		[Test]
		public void System_MidpointRounding () {
			Test<global::System.MidpointRounding> ();
		}
		[Test]
		public void System_MissingMemberException () {
			Test<global::System.MissingMemberException> ();
		}
		[Test]
		public void System_MissingFieldException () {
			Test<global::System.MissingFieldException> ();
		}
		[Test]
		public void System_MissingMethodException () {
			Test<global::System.MissingMethodException> ();
		}
		[Test]
		public void System_MulticastNotSupportedException () {
			Test<global::System.MulticastNotSupportedException> ();
		}
		[Test]
		public void System_NotFiniteNumberException () {
			Test<global::System.NotFiniteNumberException> ();
		}
		[Test]
		public void System_NotImplementedException () {
			Test<global::System.NotImplementedException> ();
		}
		[Test]
		public void System_NotSupportedException () {
			Test<global::System.NotSupportedException> ();
		}
		[Test]
		public void System_NullReferenceException () {
			Test<global::System.NullReferenceException> ();
		}
		[Test]
		public void System_ObsoleteAttribute () {
			Test<global::System.ObsoleteAttribute> ();
		}
		[Test]
		public void System_OperationCanceledException () {
			Test<global::System.OperationCanceledException> ();
		}
		[Test]
		public void System_OverflowException () {
			Test<global::System.OverflowException> ();
		}
		[Test]
		public void System_PlatformID () {
			Test<global::System.PlatformID> ();
		}
		[Test]
		public void System_PlatformNotSupportedException () {
			Test<global::System.PlatformNotSupportedException> ();
		}
		//[Test]
		//[Category ("NotWorking")]
		//public void System_Random () {
		//    Test<global::System.Random> ();
		//}
		[Test]
		public void System_RankException () {
			Test<global::System.RankException> ();
		}
		[Test]
		public void System_SByte () {
			Test<global::System.SByte> ();
		}
		[Test]
		public void System_Single () {
			Test<global::System.Single> ();
		}
		[Test]
		public void System_TimeoutException () {
			Test<global::System.TimeoutException> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_TimeSpan () {
			Test<global::System.TimeSpan> ();
		}
		[Test]
		public void System_TypeCode () {
			Test<global::System.TypeCode> ();
		}
		[Test]
		public void System_UInt16 () {
			Test<global::System.UInt16> ();
		}
		[Test]
		public void System_UInt32 () {
			Test<global::System.UInt32> ();
		}
		[Test]
		public void System_UInt64 () {
			Test<global::System.UInt64> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_UIntPtr () {
			Test<global::System.UIntPtr> ();
		}
		[Test]
		public void System_UnauthorizedAccessException () {
			Test<global::System.UnauthorizedAccessException> ();
		}
		[Test]
		public void System_Version () {
			Test<global::System.Version> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Threading_AbandonedMutexException () {
			Test<global::System.Threading.AbandonedMutexException> ();
		}
		[Test]
		public void System_Threading_EventResetMode () {
			Test<global::System.Threading.EventResetMode> ();
		}
		[Test]
		public void System_Threading_SynchronizationLockException () {
			Test<global::System.Threading.SynchronizationLockException> ();
		}
		[Test]
		public void System_Threading_ThreadInterruptedException () {
			Test<global::System.Threading.ThreadInterruptedException> ();
		}
		[Test]
		public void System_Threading_ThreadPriority () {
			Test<global::System.Threading.ThreadPriority> ();
		}
		[Test]
		public void System_Threading_ThreadState () {
			Test<global::System.Threading.ThreadState> ();
		}
		[Test]
		public void System_Threading_ThreadStateException () {
			Test<global::System.Threading.ThreadStateException> ();
		}
		[Test]
		public void System_ThreadStaticAttribute () {
			Test<global::System.ThreadStaticAttribute> ();
		}
		[Test]
		public void System_Threading_WaitHandleCannotBeOpenedException () {
			Test<global::System.Threading.WaitHandleCannotBeOpenedException> ();
		}
		[Test]
		public void System_Threading_ApartmentState () {
			Test<global::System.Threading.ApartmentState> ();
		}
		[Test]
		public void System_Collections_ArrayList () {
			Test<global::System.Collections.ArrayList> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Collections_CaseInsensitiveComparer () {
			Test<global::System.Collections.CaseInsensitiveComparer> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Collections_CaseInsensitiveHashCodeProvider () {
			Test<global::System.Collections.CaseInsensitiveHashCodeProvider> ();
		}
		[Test]
		public void System_Collections_DictionaryEntry () {
			Test<global::System.Collections.DictionaryEntry> ();
		}
		[Test]
		public void System_Collections_Hashtable () {
			Test<global::System.Collections.Hashtable> ();
		}
		[Test]
		public void System_Collections_Queue () {
			Test<global::System.Collections.Queue> ();
		}
		[Test]
		public void System_Collections_SortedList () {
			Test<global::System.Collections.SortedList> ();
		}
		[Test]
		public void System_Collections_Stack () {
			Test<global::System.Collections.Stack> ();
		}
		[Test]
		public void System_Collections_Generic_KeyNotFoundException () {
			Test<global::System.Collections.Generic.KeyNotFoundException> ();
		}
		[Test]
		public void System_Diagnostics_DebuggerStepThroughAttribute () {
			Test<global::System.Diagnostics.DebuggerStepThroughAttribute> ();
		}
		[Test]
		public void System_Diagnostics_DebuggerStepperBoundaryAttribute () {
			Test<global::System.Diagnostics.DebuggerStepperBoundaryAttribute> ();
		}
		[Test]
		public void System_Diagnostics_DebuggerHiddenAttribute () {
			Test<global::System.Diagnostics.DebuggerHiddenAttribute> ();
		}
		[Test]
		public void System_Diagnostics_DebuggerNonUserCodeAttribute () {
			Test<global::System.Diagnostics.DebuggerNonUserCodeAttribute> ();
		}
		[Test]
		public void System_Diagnostics_DebuggableAttribute_DebuggingModes () {
			Test<global::System.Diagnostics.DebuggableAttribute.DebuggingModes> ();
		}
		[Test]
		public void System_Diagnostics_DebuggerBrowsableState () {
			Test<global::System.Diagnostics.DebuggerBrowsableState> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Runtime_Serialization_ObjectIDGenerator () {
			Test<global::System.Runtime.Serialization.ObjectIDGenerator> ();
		}
		[Test]
		public void System_Runtime_Serialization_SerializationException () {
			Test<global::System.Runtime.Serialization.SerializationException> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Runtime_Serialization_StreamingContext () {
			Test<global::System.Runtime.Serialization.StreamingContext> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Runtime_Serialization_StreamingContextStates () {
			Test<global::System.Runtime.Serialization.StreamingContextStates> ();
		}
		[Test]
		public void System_Globalization_CalendarAlgorithmType () {
			Test<global::System.Globalization.CalendarAlgorithmType> ();
		}
		[Test]
		public void System_Globalization_CalendarWeekRule () {
			Test<global::System.Globalization.CalendarWeekRule> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Globalization_ChineseLunisolarCalendar () {
			Test<global::System.Globalization.ChineseLunisolarCalendar> ();
		}
		[Test]
		public void System_Globalization_CompareOptions () {
			Test<global::System.Globalization.CompareOptions> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Globalization_CultureTypes () {
			Test<global::System.Globalization.CultureTypes> ();
		}
		[Test]
		public void System_Globalization_DateTimeStyles () {
			Test<global::System.Globalization.DateTimeStyles> ();
		}
		[Test]
		public void System_Globalization_DigitShapes () {
			Test<global::System.Globalization.DigitShapes> ();
		}
		[Test]
		public void System_Globalization_GregorianCalendar () {
			Test<global::System.Globalization.GregorianCalendar> ();
		}
		[Test]
		public void System_Globalization_HebrewCalendar () {
			Test<global::System.Globalization.HebrewCalendar> ();
		}
		[Test]
		public void System_Globalization_HijriCalendar () {
			Test<global::System.Globalization.HijriCalendar> ();
		}
		[Test]
		public void System_Globalization_PersianCalendar () {
			Test<global::System.Globalization.PersianCalendar> ();
		}
		[Test]
		public void System_Globalization_JulianCalendar () {
			Test<global::System.Globalization.JulianCalendar> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Globalization_KoreanLunisolarCalendar () {
			Test<global::System.Globalization.KoreanLunisolarCalendar> ();
		}
		[Test]
		public void System_Globalization_StringInfo () {
			Test<global::System.Globalization.StringInfo> ();
		}
		[Test]
		public void System_Globalization_NumberFormatInfo () {
			Test<global::System.Globalization.NumberFormatInfo> ();
		}
		[Test]
		public void System_Globalization_NumberStyles () {
			Test<global::System.Globalization.NumberStyles> ();
		}
		[Test]
		public void System_Globalization_UmAlQuraCalendar () {
			Test<global::System.Globalization.UmAlQuraCalendar> ();
		}
		[Test]
		public void System_Globalization_UnicodeCategory () {
			Test<global::System.Globalization.UnicodeCategory> ();
		}
		[Test]
		public void System_Text_DecoderExceptionFallback () {
			Test<global::System.Text.DecoderExceptionFallback> ();
		}
		[Test]
		public void System_Text_DecoderFallbackException () {
			Test<global::System.Text.DecoderFallbackException> ();
		}
		[Test]
		public void System_Text_DecoderReplacementFallback () {
			Test<global::System.Text.DecoderReplacementFallback> ();
		}
		[Test]
		public void System_Text_EncoderExceptionFallback () {
			Test<global::System.Text.EncoderExceptionFallback> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Text_EncoderFallbackException () {
			Test<global::System.Text.EncoderFallbackException> ();
		}
		[Test]
		public void System_Text_EncoderReplacementFallback () {
			Test<global::System.Text.EncoderReplacementFallback> ();
		}
		[Test]
		public void System_Resources_MissingManifestResourceException () {
			Test<global::System.Resources.MissingManifestResourceException> ();
		}
		[Test]
		public void System_Resources_MissingSatelliteAssemblyException () {
			Test<global::System.Resources.MissingSatelliteAssemblyException> ();
		}
		[Test]
		public void System_Resources_UltimateResourceFallbackLocation () {
			Test<global::System.Resources.UltimateResourceFallbackLocation> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_Policy_AllMembershipCondition () {
			Test<global::System.Security.Policy.AllMembershipCondition> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_Policy_ApplicationDirectoryMembershipCondition () {
			Test<global::System.Security.Policy.ApplicationDirectoryMembershipCondition> ();
		}
		[Test]
		public void System_Security_Policy_ApplicationVersionMatch () {
			Test<global::System.Security.Policy.ApplicationVersionMatch> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_Policy_ApplicationTrust () {
			Test<global::System.Security.Policy.ApplicationTrust> ();
		}
		[Test]
		public void System_Security_Policy_Evidence () {
			Test<global::System.Security.Policy.Evidence> ();
		}
		[Test]
		public void System_Security_Policy_TrustManagerUIContext () {
			Test<global::System.Security.Policy.TrustManagerUIContext> ();
		}
		[Test]
		public void System_Security_Policy_PolicyException () {
			Test<global::System.Security.Policy.PolicyException> ();
		}
		[Test]
		public void System_Security_Policy_PolicyStatementAttribute () {
			Test<global::System.Security.Policy.PolicyStatementAttribute> ();
		}
		[Test]
		public void System_Security_Policy_GacInstalled () {
			Test<global::System.Security.Policy.GacInstalled> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_Policy_GacMembershipCondition () {
			Test<global::System.Security.Policy.GacMembershipCondition> ();
		}
		[Test]
		public void System_Security_Principal_PrincipalPolicy () {
			Test<global::System.Security.Principal.PrincipalPolicy> ();
		}
		[Test]
		public void System_Security_Principal_WindowsAccountType () {
			Test<global::System.Security.Principal.WindowsAccountType> ();
		}
		[Test]
		public void System_Security_Principal_TokenImpersonationLevel () {
			Test<global::System.Security.Principal.TokenImpersonationLevel> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_Principal_TokenAccessLevels () {
			Test<global::System.Security.Principal.TokenAccessLevels> ();
		}
		[Test]
		public void System_Runtime_ConstrainedExecution_Consistency () {
			Test<global::System.Runtime.ConstrainedExecution.Consistency> ();
		}
		[Test]
		public void System_Runtime_ConstrainedExecution_Cer () {
			Test<global::System.Runtime.ConstrainedExecution.Cer> ();
		}
		[Test]
		public void System_IO_SearchOption () {
			Test<global::System.IO.SearchOption> ();
		}
		[Test]
		public void System_IO_IOException () {
			Test<global::System.IO.IOException> ();
		}
		[Test]
		public void System_IO_DirectoryNotFoundException () {
			Test<global::System.IO.DirectoryNotFoundException> ();
		}
		[Test]
		public void System_IO_DriveType () {
			Test<global::System.IO.DriveType> ();
		}
		[Test]
		public void System_IO_DriveNotFoundException () {
			Test<global::System.IO.DriveNotFoundException> ();
		}
		[Test]
		public void System_IO_EndOfStreamException () {
			Test<global::System.IO.EndOfStreamException> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_IO_FileAccess () {
			Test<global::System.IO.FileAccess> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_IO_FileLoadException () {
			Test<global::System.IO.FileLoadException> ();
		}
		[Test]
		public void System_IO_FileNotFoundException () {
			Test<global::System.IO.FileNotFoundException> ();
		}
		[Test]
		public void System_IO_FileOptions () {
			Test<global::System.IO.FileOptions> ();
		}
		[Test]
		public void System_IO_FileShare () {
			Test<global::System.IO.FileShare> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_IO_FileAttributes () {
			Test<global::System.IO.FileAttributes> ();
		}
		[Test]
		public void System_IO_MemoryStream () {
			Test<global::System.IO.MemoryStream> ();
		}
		[Test]
		public void System_IO_PathTooLongException () {
			Test<global::System.IO.PathTooLongException> ();
		}
		[Test]
		public void System_IO_SeekOrigin () {
			Test<global::System.IO.SeekOrigin> ();
		}
		//[Test]
		//public void System_Runtime_GCLatencyMode () {
		//    Test<global::System.Runtime.GCLatencyMode> ();
		//}
		[Test]
		public void System_Security_XmlSyntaxException () {
			Test<global::System.Security.XmlSyntaxException> ();
		}
		[Test]
		public void System_Security_Permissions_EnvironmentPermissionAccess () {
			Test<global::System.Security.Permissions.EnvironmentPermissionAccess> ();
		}
		[Test]
		public void System_Security_Permissions_FileDialogPermissionAccess () {
			Test<global::System.Security.Permissions.FileDialogPermissionAccess> ();
		}
		[Test]
		public void System_Security_Permissions_FileIOPermissionAccess () {
			Test<global::System.Security.Permissions.FileIOPermissionAccess> ();
		}
		[Test]
		public void System_Security_Permissions_HostProtectionResource () {
			Test<global::System.Security.Permissions.HostProtectionResource> ();
		}
		[Test]
		public void System_Security_Permissions_HostProtectionAttribute () {
			Test<global::System.Security.Permissions.HostProtectionAttribute> ();
		}
		[Test]
		public void System_Security_Permissions_IsolatedStorageContainment () {
			Test<global::System.Security.Permissions.IsolatedStorageContainment> ();
		}
		[Test]
		public void System_Security_Permissions_PermissionState () {
			Test<global::System.Security.Permissions.PermissionState> ();
		}
		[Test]
		public void System_Security_Permissions_ReflectionPermissionFlag () {
			Test<global::System.Security.Permissions.ReflectionPermissionFlag> ();
		}
		[Test]
		public void System_Security_Permissions_SecurityPermissionFlag () {
			Test<global::System.Security.Permissions.SecurityPermissionFlag> ();
		}
		[Test]
		public void System_Security_Permissions_UIPermissionWindow () {
			Test<global::System.Security.Permissions.UIPermissionWindow> ();
		}
		[Test]
		public void System_Security_Permissions_UIPermissionClipboard () {
			Test<global::System.Security.Permissions.UIPermissionClipboard> ();
		}
		[Test]
		public void System_Security_Permissions_GacIdentityPermission () {
			Test<global::System.Security.Permissions.GacIdentityPermission> ();
		}
		[Test]
		public void System_Security_Permissions_KeyContainerPermissionFlags () {
			Test<global::System.Security.Permissions.KeyContainerPermissionFlags> ();
		}
		[Test]
		public void System_Security_Permissions_RegistryPermissionAccess () {
			Test<global::System.Security.Permissions.RegistryPermissionAccess> ();
		}
		[Test]
		public void System_Security_SecurityCriticalScope () {
			Test<global::System.Security.SecurityCriticalScope> ();
		}
		[Test]
		public void System_Security_HostSecurityManagerOptions () {
			Test<global::System.Security.HostSecurityManagerOptions> ();
		}
		[Test]
		public void System_Security_HostSecurityManager () {
			Test<global::System.Security.HostSecurityManager> ();
		}
		[Test]
		public void System_Security_PolicyLevelType () {
			Test<global::System.Security.PolicyLevelType> ();
		}
		[Test]
		public void System_Security_SecurityZone () {
			Test<global::System.Security.SecurityZone> ();
		}
		[Test]
		public void System_Security_VerificationException () {
			Test<global::System.Security.VerificationException> ();
		}
		[Test]
		public void System_Runtime_Remoting_Channels_ServerProcessing () {
			Test<global::System.Runtime.Remoting.Channels.ServerProcessing> ();
		}
		[Test]
		public void System_Runtime_Remoting_Channels_TransportHeaders () {
			Test<global::System.Runtime.Remoting.Channels.TransportHeaders> ();
		}
		[Test]
		public void System_Runtime_Remoting_Lifetime_LeaseState () {
			Test<global::System.Runtime.Remoting.Lifetime.LeaseState> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_SoapOption () {
			Test<global::System.Runtime.Remoting.Metadata.SoapOption> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_XmlFieldOrderOption () {
			Test<global::System.Runtime.Remoting.Metadata.XmlFieldOrderOption> ();
		}
		[Test]
		public void System_Runtime_Remoting_CustomErrorsModes () {
			Test<global::System.Runtime.Remoting.CustomErrorsModes> ();
		}
		[Test]
		public void System_Runtime_Remoting_RemotingException () {
			Test<global::System.Runtime.Remoting.RemotingException> ();
		}
		[Test]
		public void System_Runtime_Remoting_ServerException () {
			Test<global::System.Runtime.Remoting.ServerException> ();
		}
		[Test]
		public void System_Runtime_Remoting_RemotingTimeoutException () {
			Test<global::System.Runtime.Remoting.RemotingTimeoutException> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapTime () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapTime> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapDate () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapYearMonth () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapYearMonth> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapYear () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapYear> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapMonthDay () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapMonthDay> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapDay () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDay> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapMonth () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapMonth> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapHexBinary () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapBase64Binary () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapBase64Binary> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapInteger () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapInteger> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapPositiveInteger () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapPositiveInteger> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapNonPositiveInteger () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonPositiveInteger> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapNonNegativeInteger () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonNegativeInteger> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapNegativeInteger () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNegativeInteger> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapAnyUri () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapAnyUri> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapQName () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapQName> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapNotation () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNotation> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapNormalizedString () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNormalizedString> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapToken () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapToken> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapLanguage () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapLanguage> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapName () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapName> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapIdrefs () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapIdrefs> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapEntities () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapEntities> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapNmtoken () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNmtoken> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapNmtokens () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNmtokens> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapNcName () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNcName> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapId () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapId> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapIdref () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapIdref> ();
		}
		[Test]
		public void System_Runtime_Remoting_Metadata_W3cXsd2001_SoapEntity () {
			Test<global::System.Runtime.Remoting.Metadata.W3cXsd2001.SoapEntity> ();
		}
		[Test]
		public void System_Runtime_Remoting_Contexts_SynchronizationAttribute () {
			Test<global::System.Runtime.Remoting.Contexts.SynchronizationAttribute> ();
		}
		[Test]
		public void System_IO_IsolatedStorage_IsolatedStorageScope () {
			Test<global::System.IO.IsolatedStorage.IsolatedStorageScope> ();
		}
		[Test]
		public void System_IO_IsolatedStorage_IsolatedStorageException () {
			Test<global::System.IO.IsolatedStorage.IsolatedStorageException> ();
		}
		[Test]
		public void System_Runtime_Serialization_Formatters_FormatterTypeStyle () {
			Test<global::System.Runtime.Serialization.Formatters.FormatterTypeStyle> ();
		}
		[Test]
		public void System_Runtime_Serialization_Formatters_FormatterAssemblyStyle () {
			Test<global::System.Runtime.Serialization.Formatters.FormatterAssemblyStyle> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Runtime_Serialization_Formatters_SoapMessage () {
			Test<global::System.Runtime.Serialization.Formatters.SoapMessage> ();
		}
		[Test]
		public void System_Runtime_Serialization_Formatters_SoapFault () {
			Test<global::System.Runtime.Serialization.Formatters.SoapFault> ();
		}
		[Test]
		public void System_Configuration_Assemblies_AssemblyHash () {
			Test<global::System.Configuration.Assemblies.AssemblyHash> ();
		}
		[Test]
		public void System_Configuration_Assemblies_AssemblyHashAlgorithm () {
			Test<global::System.Configuration.Assemblies.AssemblyHashAlgorithm> ();
		}
		[Test]
		public void System_Security_Cryptography_CryptographicException () {
			Test<global::System.Security.Cryptography.CryptographicException> ();
		}
		[Test]
		public void System_Security_Cryptography_CryptographicUnexpectedOperationException () {
			Test<global::System.Security.Cryptography.CryptographicUnexpectedOperationException> ();
		}
		[Test]
		public void System_Security_Cryptography_FromBase64TransformMode () {
			Test<global::System.Security.Cryptography.FromBase64TransformMode> ();
		}
		[Test]
		public void System_Security_Cryptography_CspProviderFlags () {
			Test<global::System.Security.Cryptography.CspProviderFlags> ();
		}
		[Test]
		public void System_Security_Cryptography_CryptoStreamMode () {
			Test<global::System.Security.Cryptography.CryptoStreamMode> ();
		}
		[Test]
		public void System_Security_Cryptography_DSAParameters () {
			Test<global::System.Security.Cryptography.DSAParameters> ();
		}
		[Test]
		public void System_Security_Cryptography_RSAParameters () {
			Test<global::System.Security.Cryptography.RSAParameters> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509ContentType () {
			Test<global::System.Security.Cryptography.X509Certificates.X509ContentType> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509KeyStorageFlags () {
			Test<global::System.Security.Cryptography.X509Certificates.X509KeyStorageFlags> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_Cryptography_X509Certificates_X509Certificate () {
			Test<global::System.Security.Cryptography.X509Certificates.X509Certificate> ();
		}
		[Test]
		public void System_Security_AccessControl_AceType () {
			Test<global::System.Security.AccessControl.AceType> ();
		}
		[Test]
		public void System_Security_AccessControl_AceFlags () {
			Test<global::System.Security.AccessControl.AceFlags> ();
		}
		[Test]
		public void System_Security_AccessControl_AceQualifier () {
			Test<global::System.Security.AccessControl.AceQualifier> ();
		}
		[Test]
		public void System_Security_AccessControl_ObjectAceFlags () {
			Test<global::System.Security.AccessControl.ObjectAceFlags> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_AccessControl_CryptoKeyRights () {
			Test<global::System.Security.AccessControl.CryptoKeyRights> ();
		}
		[Test]
		public void System_Security_AccessControl_InheritanceFlags () {
			Test<global::System.Security.AccessControl.InheritanceFlags> ();
		}
		[Test]
		public void System_Security_AccessControl_PropagationFlags () {
			Test<global::System.Security.AccessControl.PropagationFlags> ();
		}
		[Test]
		public void System_Security_AccessControl_AuditFlags () {
			Test<global::System.Security.AccessControl.AuditFlags> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_AccessControl_SecurityInfos () {
			Test<global::System.Security.AccessControl.SecurityInfos> ();
		}
		[Test]
		public void System_Security_AccessControl_ResourceType () {
			Test<global::System.Security.AccessControl.ResourceType> ();
		}
		[Test]
		public void System_Security_AccessControl_AccessControlSections () {
			Test<global::System.Security.AccessControl.AccessControlSections> ();
		}
		[Test]
		public void System_Security_AccessControl_AccessControlActions () {
			Test<global::System.Security.AccessControl.AccessControlActions> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_AccessControl_EventWaitHandleRights () {
			Test<global::System.Security.AccessControl.EventWaitHandleRights> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_AccessControl_FileSystemRights () {
			Test<global::System.Security.AccessControl.FileSystemRights> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_AccessControl_MutexRights () {
			Test<global::System.Security.AccessControl.MutexRights> ();
		}
		[Test]
		public void System_Security_AccessControl_AccessControlModification () {
			Test<global::System.Security.AccessControl.AccessControlModification> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_AccessControl_PrivilegeNotHeldException () {
			Test<global::System.Security.AccessControl.PrivilegeNotHeldException> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_AccessControl_RegistryRights () {
			Test<global::System.Security.AccessControl.RegistryRights> ();
		}
		[Test]
		public void System_Security_AccessControl_AccessControlType () {
			Test<global::System.Security.AccessControl.AccessControlType> ();
		}
		[Test]
		public void System_Security_AccessControl_ControlFlags () {
			Test<global::System.Security.AccessControl.ControlFlags> ();
		}
		[Test]
		public void System_Security_Principal_WellKnownSidType () {
			Test<global::System.Security.Principal.WellKnownSidType> ();
		}
		[Test]
		public void System_Security_Principal_IdentityNotMappedException () {
			Test<global::System.Security.Principal.IdentityNotMappedException> ();
		}
		[Test]
		public void System_Runtime_Versioning_ResourceScope () {
			Test<global::System.Runtime.Versioning.ResourceScope> ();
		}
	}
}
