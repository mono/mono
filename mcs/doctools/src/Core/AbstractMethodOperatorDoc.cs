// AbstractMethodOperatorDoc.cs
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
	public abstract class AbstractMethodOperatorDoc : AbstractDoc
	{
		protected ExceptionDoc[] exceptions = null;
		protected ParameterDoc[] parameters = null;
		protected string         returns    = null;

		public AbstractMethodOperatorDoc() : base()
		{
		}

		public ExceptionDoc[] Exceptions
		{
			get { return exceptions;  }
			set { exceptions = value; }
		}

		public ParameterDoc[] Parameters
		{
			get { return parameters;  }
			set { parameters = value; }
		}

		public string Returns
		{
			get { return returns;  }
			set { returns = value; }
		}
	}
}
