// 
// System.Web.Services.Protocols.SoapExtension.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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

using System.IO;
using System.Collections;
using System.Web.Services.Configuration;

namespace System.Web.Services.Protocols {
	public abstract class SoapExtension {

		#region Fields

		Stream stream;

		#endregion

		#region Constructors

		protected SoapExtension ()
		{
		}

		#endregion // Constructors

		#region Methods

		public virtual Stream ChainStream (Stream stream)
		{
			return stream;
		}

		public abstract object GetInitializer (Type serviceType);
		public abstract object GetInitializer (LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute);
		public abstract void Initialize (object initializer);
		public abstract void ProcessMessage (SoapMessage message);


#if !TARGET_JVM
		static ArrayList[] globalExtensions;
#else
		static ArrayList[] globalExtensions {
			get {
				return (ArrayList[])AppDomain.CurrentDomain.GetData("SoapExtension.globalExtensions");
			}
			set {
				AppDomain.CurrentDomain.SetData("SoapExtension.globalExtensions", value);
			}
		}
#endif

		internal static SoapExtension[] CreateExtensionChain (SoapExtensionRuntimeConfig[] extensionConfigs)
		{
			if (extensionConfigs == null) return null;
			SoapExtension[] res = new SoapExtension [extensionConfigs.Length];
			CreateExtensionChain (extensionConfigs, res, 0);
			return res;
		}

		internal static SoapExtension[] CreateExtensionChain (SoapExtensionRuntimeConfig[] hiPrioExts, SoapExtensionRuntimeConfig[] medPrioExts, SoapExtensionRuntimeConfig[] lowPrioExts)
		{
			int len = 0;
			if (hiPrioExts != null) len += hiPrioExts.Length;
			if (medPrioExts != null) len += medPrioExts.Length;
			if (lowPrioExts != null) len += lowPrioExts.Length;
			if (len == 0) return null;

			SoapExtension[] res = new SoapExtension [len];
			int pos = 0;
			if (hiPrioExts != null) pos = CreateExtensionChain (hiPrioExts, res, pos);
			if (medPrioExts != null) pos = CreateExtensionChain (medPrioExts, res, pos);
			if (lowPrioExts != null) pos = CreateExtensionChain (lowPrioExts, res, pos);
			return res;
		}

		static int CreateExtensionChain (SoapExtensionRuntimeConfig[] extensionConfigs, SoapExtension[] destArray, int pos)
		{
			for (int n=0; n<extensionConfigs.Length; n++)
			{
				SoapExtensionRuntimeConfig econf = extensionConfigs [n];
				SoapExtension ext = (SoapExtension) Activator.CreateInstance (econf.Type);
				ext.Initialize (econf.InitializationInfo);
				destArray [pos++] = ext;
			}
			return pos;
		}

#if !MOBILE
		static void InitializeGlobalExtensions ()
		{
			globalExtensions = new ArrayList[2];
#if NET_2_0 
			if (WebServicesSection.Current == null) return;

			SoapExtensionTypeElementCollection exts = WebServicesSection.Current.SoapExtensionTypes;
#else
			ArrayList exts = WSConfig.Instance.ExtensionTypes;
#endif
			if (exts == null) return;

#if NET_2_0
			foreach (SoapExtensionTypeElement econf in exts)
#else
			foreach (WSExtensionConfig econf in exts)
#endif
			{
				if (globalExtensions [(int)econf.Group] == null) globalExtensions [(int)econf.Group] = new ArrayList ();
				ArrayList destList = globalExtensions [(int) econf.Group];
				bool added = false;
				for (int n=0; n<destList.Count && !added; n++)
#if NET_2_0
					if (((SoapExtensionTypeElement)destList [n]).Priority > econf.Priority) {
#else
					if (((WSExtensionConfig)destList [n]).Priority > econf.Priority) {
#endif
						destList.Insert (n, econf);
						added = true;
					}
				if (!added) destList.Add (econf);
			}
		}

		internal static SoapExtensionRuntimeConfig[][] GetTypeExtensions (Type serviceType)
		{
			if (globalExtensions == null) InitializeGlobalExtensions();
			
			SoapExtensionRuntimeConfig[][] exts = new SoapExtensionRuntimeConfig[2][];

			for (int group = 0; group < 2; group++)
			{
				ArrayList globList = globalExtensions[group];
				if (globList == null) continue;
				exts [group] = new SoapExtensionRuntimeConfig [globList.Count];
				for (int n=0; n<globList.Count; n++)
				{
#if NET_2_0
					SoapExtensionTypeElement econf = (SoapExtensionTypeElement) globList [n];
#else
					WSExtensionConfig econf = (WSExtensionConfig) globList [n];
#endif
					SoapExtensionRuntimeConfig typeconf = new SoapExtensionRuntimeConfig ();
					typeconf.Type = econf.Type;
					SoapExtension ext = (SoapExtension) Activator.CreateInstance (econf.Type);
					typeconf.InitializationInfo = ext.GetInitializer (serviceType);
					exts [group][n] = typeconf;
				}
			}
			return exts;
		}
#endif
	
		internal static SoapExtensionRuntimeConfig[] GetMethodExtensions (LogicalMethodInfo method)
		{
			object[] ats = method.GetCustomAttributes (typeof (SoapExtensionAttribute));
			SoapExtensionRuntimeConfig[] exts = new SoapExtensionRuntimeConfig [ats.Length];
			int[] priorities = new int[ats.Length];

			for (int n=0; n<ats.Length; n++)
			{
				SoapExtensionAttribute at = (SoapExtensionAttribute) ats[n];
				SoapExtensionRuntimeConfig econf = new SoapExtensionRuntimeConfig ();
				econf.Type = at.ExtensionType;
				priorities [n] = at.Priority;
				SoapExtension ext = (SoapExtension) Activator.CreateInstance (econf.Type);
				econf.InitializationInfo = ext.GetInitializer (method, at);
				exts [n] = econf;
			}
			Array.Sort (priorities, exts);
			return exts;
		}

		internal static Stream ExecuteChainStream (SoapExtension[] extensions, Stream stream)
		{
			if (extensions == null) return stream;

			Stream newStream = stream;
			foreach (SoapExtension ext in extensions)
				newStream = ext.ChainStream (newStream);
			return newStream;
		}

		internal static void ExecuteProcessMessage(SoapExtension[] extensions, SoapMessage message, Stream stream, bool inverseOrder) 
		{
			if (extensions == null) return;

			message.InternalStream = stream;

			if (inverseOrder)
			{
				for (int n = extensions.Length-1; n >= 0; n--)
					extensions[n].ProcessMessage (message);
			}
			else
			{
				for (int n = 0; n < extensions.Length; n++)
					extensions[n].ProcessMessage (message);
			}
		}

		#endregion // Methods
	}

	internal class SoapExtensionRuntimeConfig
	{
		public Type Type;
		public int Priority;
		public object InitializationInfo;
	}
}
