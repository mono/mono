//
// System.Web.Hosting.BareApplicationHost
// 
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
#if NET_2_0
using System;
using System.IO;
using System.Collections.Generic;

namespace System.Web.Hosting {
	class RegisteredItem {
		public IRegisteredObject Item;
		public bool AutoClean;
		public RegisteredItem (IRegisteredObject item, bool autoclean)
		{
			this.Item = item;
			this.AutoClean = autoclean;
		}
	}

	sealed class BareApplicationHost : MarshalByRefObject {
		string vpath;
		string phys_path;
		Dictionary<Type, RegisteredItem> hash;
		internal ApplicationManager Manager;
		internal string AppID;

		public BareApplicationHost ()
		{
			Init ();
		}

		void Init ()
		{
			hash = new Dictionary<Type, RegisteredItem> ();
			HostingEnvironment.Host = this;
			AppDomain current = AppDomain.CurrentDomain;
			current.DomainUnload += OnDomainUnload;
			phys_path = (string) current.GetData (".appPath");
			vpath = (string) current.GetData (".appVPath");
		}

		public string VirtualPath {
			get { return vpath; }
		}

		public string PhysicalPath {
			get { return phys_path; }
		}

		public AppDomain Domain {
			get { return AppDomain.CurrentDomain; }
		}

		public void Shutdown ()
		{
			HostingEnvironment.InitiateShutdown ();
		}

		public void StopObject (Type type)
		{
			if (!hash.ContainsKey (type))
				return;

			RegisteredItem reg = hash [type];
			reg.Item.Stop (false);
		}

		public IRegisteredObject CreateInstance (Type type)
		{
			return (IRegisteredObject) Activator.CreateInstance (type, null);
		}

		public void RegisterObject (IRegisteredObject obj, bool auto_clean)
		{
			hash [obj.GetType ()] = new RegisteredItem (obj, auto_clean);
		}

		public bool UnregisterObject (IRegisteredObject obj)
		{
			return hash.Remove (obj.GetType ());
		}

		public IRegisteredObject GetObject (Type type)
		{
			if (hash.ContainsKey (type))
				return hash [type].Item;

			return null;
		}

		public string GetCodeGenDir ()
		{
			return AppDomain.CurrentDomain.SetupInformation.DynamicBase;
		}

		void OnDomainUnload (object sender, EventArgs args)
		{
			Manager.RemoveHost (AppID);
			ICollection<RegisteredItem> values = hash.Values;
			RegisteredItem [] objects = new RegisteredItem [hash.Count];
			values.CopyTo (objects, 0);

			foreach (RegisteredItem reg in objects) {
				try {
					reg.Item.Stop (true); // Stop should call Unregister. It's ok if not.
				} catch {
					// Ignore or throw?
				}
			}
			hash.Clear ();
		}
	}
}

#endif

