// AbstractDoc.cs
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
using System.Collections.Specialized;
using System.Reflection;
using System.Xml.Serialization;

namespace Mono.Doc.Core
{
	public abstract class AbstractDoc
	{
		protected string           name;
		protected string           summary;
		protected string           remarks;
		protected StringCollection seeAlso;

		public static readonly string TODO = "Documentation forthcoming.";

		public AbstractDoc(string name)
		{
			this.name     = name;
			this.summary  = string.Empty;
			this.remarks  = string.Empty;
			this.seeAlso  = new StringCollection();
		}

		public AbstractDoc() : this(string.Empty)
		{
		}

		public AbstractDoc(MemberInfo m, AssemblyLoader loader) : this(TypeNameHelper.GetName(m))
		{
			this.summary = AbstractDoc.TODO;
			this.remarks = AbstractDoc.TODO;
		}

		[XmlElement(ElementName = "summary")]
		public string Summary
		{
			get { return this.summary;  }
			set { this.summary = value; }
		}

		[XmlElement(ElementName = "remarks")]
		public string Remarks
		{
			get { return this.remarks;  }
			set { this.remarks = value; }
		}

		[XmlAttribute(AttributeName = "name")]
		public string Name
		{
			get { return this.name;  }
			set { this.name = value; }
		}

		[XmlElement(ElementName = "seealso")]
		public StringCollection SeeAlso
		{
			get { return this.seeAlso; }
		}
	}
}
