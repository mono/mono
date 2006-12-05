//
//// GenerateDeploymentManifest.cs
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
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks
{
	public sealed class GenerateDeploymentManifest : GenerateManifestBase
	{
		[MonoTODO]
		public GenerateDeploymentManifest ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string DeploymentUrl {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool DisallowUrlActivation {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool Install {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool MapFileExtensions {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string MinimumRequiredVersion {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string Product {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string Publisher {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string SupportUrl {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool TrustUrlParameters {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool UpdateEnabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public int UpdateInterval {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string UpdateMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string UpdateUnit {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		protected internal override bool ValidateInputs ()
		{
			throw new NotImplementedException ();
		}
		
		protected override Type GetObjectType ()
		{
			throw new NotImplementedException ();
		}
		
		protected override bool OnManifestLoaded (Manifest manifest)
		{
			throw new NotImplementedException ();
		}
		
		protected override bool OnManifestResolved (Manifest manifest)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
