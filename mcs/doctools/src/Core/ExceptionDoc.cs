// ExceptionDoc.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
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
	public class ExceptionDoc
	{
		private string cref = null;
		private string description = null;

		public ExceptionDoc()
		{
		}

		public ExceptionDoc(string pCref, string pDescription)
		{
			cref = pCref;
			description = pDescription;
		}

		public string Cref
		{
			get { return cref;  }
			set { cref = value; }
		}

		public string Description
		{
			get { return description;  }
			set { description = value; }
		}
	}
}
