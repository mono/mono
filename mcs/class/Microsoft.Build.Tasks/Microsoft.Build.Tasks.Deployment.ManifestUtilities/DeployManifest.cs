//
// DeployManifest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities {
	
	[ComVisible (false)]
	public sealed class DeployManifest : Manifest {
	
		string			deploymentUrl;
		bool			disallowUrlActivation;
		AssemblyReference	entryPoint;
		bool			install;
		bool			mapFileExtensions;
		string			minimumRequiredVersion;
		string			product;
		string			publisher;
		string			supportUrl;
		bool			trustUrlParameters;
		bool			updateEnabled;
		int			updateInterval;
		UpdateMode		updateMode;
		UpdateUnit		updateUnit;
		string			xmlDeploymentUrl;
		string			xmlDisallowUrlActivation;
		string			xmlInstall;
		string			xmlMapFileExtensions;
		string			xmlMinimumRequiredVersion;
		string			xmlProduct;
		string			xmlPublisher;
		string			xmlSupportUrl;
		string			xmlTrustUrlParameters;
		string			xmlUpdateEnabled;
		string			xmlUpdateInterval;
		string			xmlUpdateMode;
		string			xmlUpdateUnit;
	
		[MonoTODO]
		public DeployManifest ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void Validate ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string DeploymentUrl {
			get { return deploymentUrl; }
			set { deploymentUrl = value; }
		}
		
		[MonoTODO]
		public bool DisallowUrlActivation {
			get { return disallowUrlActivation; }
			set { disallowUrlActivation = value; }
		}
		
		[MonoTODO]
		public override AssemblyReference EntryPoint {
			get { return entryPoint; }
			set { entryPoint = value; }
		}
		
		[MonoTODO]
		public bool Install {
			get { return install; }
			set { install = value; }
		}
		
		[MonoTODO]
		public bool MapFileExtensions {
			get { return mapFileExtensions; }
			set { mapFileExtensions = value; }
		}
		
		[MonoTODO]
		public string MinimumRequiredVersion {
			get { return minimumRequiredVersion; }
			set { minimumRequiredVersion = value; }
		}
		
		[MonoTODO]
		public string Product {
			get { return product; }
			set { product = value; }
		}
		
		[MonoTODO]
		public string Publisher {
			get { return publisher; }
			set { publisher = value; }
		}
		
		[MonoTODO]
		public string SupportUrl {
			get { return supportUrl; }
			set { supportUrl = value; }
		}
		
		[MonoTODO]
		public bool TrustUrlParameters {
			get { return trustUrlParameters; }
			set { trustUrlParameters = value; }
		}
		
		[MonoTODO]
		public bool UpdateEnabled {
			get { return updateEnabled; }
			set { updateEnabled = value; }
		}
		
		[MonoTODO]
		public int UpdateInterval {
			get { return updateInterval; }
			set { updateInterval = value; }
		}
		
		[MonoTODO]
		public UpdateMode UpdateMode {
			get { return updateMode; }
			set { updateMode = value; }
		}
		
		[MonoTODO]
		public UpdateUnit UpdateUnit {
			get { return updateUnit; }
			set { updateUnit = value; }
		}
		
		[MonoTODO]
		public string XmlDeploymentUrl {
			get { return xmlDeploymentUrl; }
			set { xmlDeploymentUrl = value; }
		}
		
		[MonoTODO]
		public string XmlDisallowUrlActivation {
			get { return xmlDisallowUrlActivation; }
			set { xmlDisallowUrlActivation = value; }
		}
		
		[MonoTODO]
		public string XmlInstall {
			get { return xmlInstall; }
			set { xmlInstall = value; }
		}
		
		[MonoTODO]
		public string XmlMapFileExtensions {
			get { return xmlMapFileExtensions; }
			set { xmlMapFileExtensions = value; }
		}
		
		[MonoTODO]
		public string XmlMinimumRequiredVersion {
			get { return xmlMinimumRequiredVersion; }
			set { xmlMinimumRequiredVersion = value; }
		}
		
		[MonoTODO]
		public string XmlProduct {
			get { return xmlProduct; }
			set { xmlProduct = value; }
		}
		
		[MonoTODO]
		public string XmlPublisher {
			get { return xmlPublisher; }
			set { xmlPublisher = value; }
		}
		
		[MonoTODO]
		public string XmlSupportUrl {
			get { return xmlSupportUrl; }
			set { xmlSupportUrl = value; }
		}
		
		[MonoTODO]
		public string XmlTrustUrlParameters {
			get { return xmlTrustUrlParameters; }
			set { xmlTrustUrlParameters = value; }
		}
		
		[MonoTODO]
		public string XmlUpdateEnabled {
			get { return xmlUpdateEnabled; }
			set { xmlUpdateEnabled = value; }
		}
		
		[MonoTODO]
		public string XmlUpdateInterval {
			get { return xmlUpdateInterval; }
			set { xmlUpdateInterval = value; }
		}
		
		[MonoTODO]
		public string XmlUpdateMode {
			get { return xmlUpdateMode; }
			set { xmlUpdateMode = value; }
		}
		
		[MonoTODO]
		public string XmlUpdateUnit {
			get { return xmlUpdateUnit; }
			set { xmlUpdateUnit = value; }
		}
	}
}

#endif
