// AbstractClassStructDoc.cs
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
	public abstract class AbstractClassStructDoc : AbstractTypeDoc
	{
		// constructor | event | field | method | operator | property
		protected ConstructorDoc[] constructors = null;
		protected EventDoc[]       events       = null;
		protected FieldDoc[]       fields       = null;
		protected MethodDoc[]      methods      = null;
		protected OperatorDoc[]    operators    = null;
		protected PropertyDoc[]    properties   = null;

		// nested items
		protected ClassDoc[]       classes      = null;
		protected DelegateDoc[]    delegates    = null;
		protected EnumDoc[]        enums        = null;
		protected InterfaceDoc[]   interfaces   = null;
		protected StructDoc[]      structs      = null;

		public AbstractClassStructDoc() : base()
		{
		}

		public ConstructorDoc[] Constructors
		{
			get { return constructors;  }
			set { constructors = value; }
		}

		public EventDoc[] Events
		{
			get { return events;  }
			set { events = value; }
		}

		public FieldDoc[] Fields
		{
			get { return fields;  }
			set { fields = value; }
		}

		public MethodDoc[] Methods
		{
			get { return methods;  }
			set { methods = value; }
		}

		public OperatorDoc[] Operators
		{
			get { return operators;  }
			set { operators = value; }
		}

		public PropertyDoc[] Properties
		{
			get { return properties;  }
			set { properties = value; }
		}

		public ClassDoc[] NestedClasses
		{
			get { return classes;  }
			set { classes = value; }
		}

		public DelegateDoc[] NestedDelegates
		{
			get { return delegates;  }
			set { delegates = value; }
		}

		public EnumDoc[] NestedEnums
		{
			get { return enums;  }
			set { enums = value; }
		}

		public InterfaceDoc[] NestedInterfaces
		{
			get { return interfaces;  }
			set { interfaces = value; }
		}

		public StructDoc[] NestedStructs
		{
			get { return structs;  }
			set { structs = value; }
		}
	}
}
