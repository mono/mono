// DelegateDoc.cs
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
using System.Xml.Serialization;

namespace Mono.Doc.Core
{
	[XmlType(TypeName = "delegate")]
	public class DelegateDoc : AbstractTypeDoc
	{
		private ValueConstrainedArrayList parameters = new ValueConstrainedArrayList(typeof(ParameterDoc));

		public DelegateDoc(string name) : base(name)
		{
		}

		public DelegateDoc() : this (string.Empty)
		{
		}

		[XmlElement(ElementName = "param", Type = typeof(ParameterDoc))]
		public ValueConstrainedArrayList Parameters
		{
			get { return this.parameters; }
		}
	}
}
