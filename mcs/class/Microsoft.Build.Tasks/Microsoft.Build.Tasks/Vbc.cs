//
//// UpdateManifest.cs
/////
///// Author:
/////      Leszek Ciesielski  <skolima@gmail.com>
/////
///// Copyright (C) 2006 Forcom (http://www.forcom.com.pl/)
/////
///// Permission is hereby granted, free of charge, to any person obtaining
///// a copy of this software and associated documentation files (the
///// "Software"), to deal in the Software without restriction, including
///// without limitation the rights to use, copy, modify, merge, publish,
///// distribute, sublicense, and/or sell copies of the Software, and to
///// permit persons to whom the Software is furnished to do so, subject to
///// the following conditions:
///// 
///// The above copyright notice and this permission notice shall be
///// included in all copies or substantial portions of the Software.
///// 
///// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
///// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
///// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
///// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
///// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
///// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
///// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
/////
///

#if NET_2_0

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks
{
	public class Vbc : ManagedCompiler
	{
		[MonoTODO]
		protected override bool ValidateParameters ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string RootNamespace {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string SdkPath {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool TargetCompactFramework {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool UseHostCompilerIfAvailable {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string Verbosity {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string WarningsAsErrors {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string WarningsNotAsErrors {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		protected override string ToolName {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		protected internal override void AddResponseFileCommands (
				CommandLineBuilderExtension commandLine )
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool CallHostObjectToExecute ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override string GenerateFullPathToTool ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override HostObjectInitializationStatus InitializeHostObject ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Vbc ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string BaseAddress {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string DisabledWarnings  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string DocumentationFile {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string ErrorReport {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool GenerateDocumentation {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public ITaskItem[] Imports {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool NoStandardLib {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool NoWarnings {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string OptionCompare {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool OptionExplicit {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool OptionStrict {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string OptionStrictType {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string Platform {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool RemoveIntegerChecks {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	}
}

#endif
