//
// System.Configuration.ConfigurationSection.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//  Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0
#if XML_DEP
using System.Collections;
using System.Xml;

namespace System.Configuration
{
	public abstract class ConfigurationSection : ConfigurationElement
	{
		ConfigurationSection parent;
		
		ConfigurationAllowDefinition allow_definition;
		bool allow_location, allow_override;
		bool inherit_on_child_apps;
		bool restart_on_external_changes;

		string config_source;
		bool force_update, is_declared, is_locked, is_protected;
		string name, path, type_name;

		ProtectedConfigurationProvider protected_provider;
		
		public ConfigurationSection ()
		{
			allow_definition = ConfigurationAllowDefinition.Everywhere;
			allow_location = true;
			allow_override = true;
			inherit_on_child_apps = true;
			restart_on_external_changes = true;
		}
		
		public ConfigurationAllowDefinition AllowDefinition {
			get { return allow_definition; }
			set { allow_definition = value; }
		}

		public bool AllowLocation {
			get { return allow_location; }
			set { allow_location = value; }
		}

		public bool AllowOverride {
			get { return allow_override; }
			set { allow_override = value; }
		}

		public string ConfigSource {
			get { return config_source; }
			set { config_source = value; }
		}

		public bool ForceUpdate {
			get { return force_update; }
			set { force_update = value; }
		}

		public bool InheritInChildApplications {
			get { return inherit_on_child_apps; }
			set { inherit_on_child_apps = value; }
		}

		[MonoTODO]
		public bool IsDeclared {
			get { return is_declared; }
		}

		[MonoTODO]
		public bool IsLocked {
			get { return is_locked; }
		}

		[MonoTODO]
		public bool IsProtected {
			get { return is_protected; }
		}

		public string Name {
			get { return name; }
		}

		[MonoTODO]
		public string Path {
			get { return path; }
		}

		[MonoTODO]
		public ProtectedConfigurationProvider ProtectionProvider {
			get { return protected_provider; }
		}

		[MonoTODO]
		public bool RestartOnExternalChanges {
			get { return restart_on_external_changes; }
			set { restart_on_external_changes = value; }
		}

		public string TypeName {
			get { return type_name; }
			set { type_name = value; }
		}

		public ConfigurationSection GetParentSection ()
		{
			return parent;
		}

		[MonoTODO]
		public XmlNode GetRawXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual object GetRuntimeObject ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override bool IsModified ()
		{
			return base.IsModified ();
		}

		[MonoTODO]
		public void ProtectSection (string provider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RequireDeclaration (bool require)
		{
		}

		[MonoTODO]
		protected internal override void ResetModified ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RevertToParent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void UnProtectSection ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void UpdateRawXml (string xml)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		internal void SetName (string name)
		{
			this.name = name;
		}
	}
}
#endif
#endif
