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

using System;

namespace System.Configuration
{
	public class ConfigurationSectionGroup
	{
		bool require_declaration;
		string name, type_name;

		ConfigurationSectionCollection sections;
		ConfigurationSectionGroupCollection groups;
		Configuration config;
		SectionGroupInfo group;
		
		public ConfigurationSectionGroup ()
		{
		}

		Configuration Config {
			get {
				if (config == null)
					throw new InvalidOperationException ("ConfigurationSectionGroup cannot be edited until it is added to a Configuration instance as its descendant");
				return config;
			}
		}

		bool initialized;

		internal void Initialize (Configuration config, SectionGroupInfo group)
		{
			if (initialized)
				throw new SystemException ("INTERNAL ERROR: this configuration section is being initialized twice: " + GetType ());
			initialized = true;
			this.config = config;
			this.group = group;
		}
		
		internal void SetName (string name)
		{
			this.name = name;
		}

		[MonoTODO]
		public void ForceDeclaration (bool require)
		{
			this.require_declaration = require;
		}

		public void ForceDeclaration ()
		{
			ForceDeclaration (true);
		}
		
		[MonoTODO]
		public bool IsDeclared {
			get { return false; }
		}

		[MonoTODO]
		public bool IsDeclarationRequired {
			get { return require_declaration; }
		}

		public string Name {
			get { return name; }
		}

		[MonoInternalNote ("Check if this is correct")]
		public string SectionGroupName {
			get { return group.XPath; }
		}

		public ConfigurationSectionGroupCollection SectionGroups {
			get {
				if (groups == null) groups = new ConfigurationSectionGroupCollection (Config, group);
				return groups;
			}
		}

		public ConfigurationSectionCollection Sections {
			get {
				if (sections == null) sections = new ConfigurationSectionCollection (Config, group);
				return sections;
			}
		}

		public string Type {
			get { return type_name;}
			set { type_name = value; }
		}
	}
}

