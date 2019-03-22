//
// System.Configuration.SectionInformation.cs
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

using System.Collections;
using System.Xml;

namespace System.Configuration
{
	public sealed class SectionInformation
	{
		ConfigurationSection associatedConfigSection;
		ConfigurationSection parent;
		
		ConfigurationAllowDefinition allow_definition = ConfigurationAllowDefinition.Everywhere;
		ConfigurationAllowExeDefinition allow_exe_definition = ConfigurationAllowExeDefinition.MachineToApplication;
		bool allow_location, allow_override;
		bool inherit_on_child_apps;
		bool restart_on_external_changes;
		bool require_permission;

		string config_source = String.Empty;
		bool force_update;
		string name, type_name;
		
		ProtectedConfigurationProvider protection_provider;
		

		[MonoTODO ("default value for require_permission")]
		internal SectionInformation (ConfigurationSection associatedConfigSection)
		{
			this.associatedConfigSection = associatedConfigSection;

			allow_definition = ConfigurationAllowDefinition.Everywhere;
			allow_location = true;
			allow_override = true;
			inherit_on_child_apps = true;
			restart_on_external_changes = true;
		}

		internal string ConfigFilePath {
			get;
			set;
		}
		
		public ConfigurationAllowDefinition AllowDefinition {
			get { return allow_definition; }
			set { allow_definition = value; }
		}

		public ConfigurationAllowExeDefinition AllowExeDefinition {
			get { return allow_exe_definition; }
			set { allow_exe_definition = value; }
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
			set {
				if (value == null)
					value = String.Empty;

				config_source = value; 
			}
		}

		public bool ForceSave {
			get { return force_update; }
			set { force_update = value; }
		}

		public bool InheritInChildApplications {
			get { return inherit_on_child_apps; }
			set { inherit_on_child_apps = value; }
		}

		[MonoTODO]
		public bool IsDeclarationRequired {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool IsDeclared {
			get { return false; }
		}

		[MonoTODO]
		public bool IsLocked {
			get { return false; }
		}

		public bool IsProtected {
			get { return protection_provider != null; }
		}

		public string Name {
			get { return name; }
		}

		public ProtectedConfigurationProvider ProtectionProvider {
			get { return protection_provider; }
		}

		[MonoTODO]
		public bool RequirePermission {
			get { return require_permission; }
			set { require_permission = value; }
		}

		[MonoTODO]
		public bool RestartOnExternalChanges {
			get { return restart_on_external_changes; }
			set { restart_on_external_changes = value; }
		}

		[MonoTODO]
		public string SectionName {
			get { return name; }
		}

		public string Type {
			get { return type_name; }
			set {
				if (value == null || value.Length == 0)
					throw new ArgumentException ("Value cannot be null or empty.");

				type_name = value; 
			}
		}

		public ConfigurationSection GetParentSection ()
		{
			return parent;
		}

		internal void SetParentSection (ConfigurationSection parent)
		{
			this.parent = parent;
		}

		public string GetRawXml ()
		{
			return associatedConfigSection.RawXml;
		}

		public void SetRawXml (string rawXml)
		{
			associatedConfigSection.SetRawXmlAndDeserialize (rawXml, String.Empty);
		}

		public void ProtectSection (string protectionProvider)
		{
			protection_provider = ProtectedConfiguration.GetProvider (protectionProvider, true);
		}

		[MonoTODO]
		public void ForceDeclaration (bool force)
		{
		}

		public void ForceDeclaration ()
		{
			ForceDeclaration (true);
		}

		[MonoTODO]
		public void RevertToParent ()
		{
			throw new NotImplementedException ();
		}

		public void UnprotectSection ()
		{
			protection_provider = null;
		}

		internal void SetName (string name)
		{
			this.name = name;
		}
	}
}

