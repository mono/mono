// EnumDoc.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
//
// This file is part of Monodoc, a multilingual API documentation tool.
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Reflection;
using System.Xml.Serialization;

namespace Mono.Doc.Core
{
	[XmlType(TypeName = "enum")]
	public class EnumDoc : AbstractTypeDoc
	{
		private ValueConstrainedArrayList members = new ValueConstrainedArrayList(typeof(EnumDoc.Member));
		
		public EnumDoc(string name) : base(name)
		{
		}

		public EnumDoc() : this(string.Empty)
		{
		}

		public EnumDoc(Type t, AssemblyLoader loader) : base(t, loader)
		{
			if (!t.IsEnum)
			{
				throw new ArgumentException("EnumDoc Type must be an enum.", "t");
			}

			foreach (FieldInfo m in t.GetFields())
			{
				this.Members.Add(new EnumDoc.Member(m.Name, AbstractDoc.TODO));
			}
		}

		[XmlElement(ElementName = "member", Type = typeof(EnumDoc.Member))]
		public ValueConstrainedArrayList Members
		{
			get { return this.members; }
		}


		[XmlType(TypeName = "member")]
		public struct Member
		{
			[XmlAttribute(AttributeName = "name")]
			public string Name;
			[XmlText]
			public string Description;

			public Member(string name, string description)
			{
				Name        = name;
				Description = description;
			}
		}
	}
}
