// EnumMember.cs
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

namespace Mono.Doc.Core
{
	public class EnumMember
	{
		private string name;
		private string description;

		public EnumMember()
		{
			this.name        = string.Empty;
			this.description = string.Empty;
		}

		public EnumMember(string name, string description)
		{
			this.name        = name;
			this.description = description;
		}

		public string Name
		{
			get { return this.name;  }
			set { this.name = value; }
		}

		public string Description
		{
			get { return this.description;  }
			set { this.description = value; }
		}
	}
}
