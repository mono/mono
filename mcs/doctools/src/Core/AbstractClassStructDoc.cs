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
		protected ValueConstrainedArrayList constructors;
		protected ValueConstrainedArrayList       events;
		protected ValueConstrainedArrayList       fields;
		protected ValueConstrainedArrayList      methods;
		protected ValueConstrainedArrayList    operators;
		protected ValueConstrainedArrayList    properties;

		// nested items
		protected ValueConstrainedArrayList       classes;
		protected ValueConstrainedArrayList    delegates;
		protected ValueConstrainedArrayList        enums;
		protected ValueConstrainedArrayList   interfaces;
		protected ValueConstrainedArrayList      structs;

		public AbstractClassStructDoc(string name) : base(name)
		{
			this.constructors = new ValueConstrainedArrayList(Type.GetType("Mono.Doc.Core.ConstructorDoc", true));
			this.events       = new ValueConstrainedArrayList(Type.GetType("Mono.Doc.Core.EventDoc", true));
			this.fields       = new ValueConstrainedArrayList(Type.GetType("Mono.Doc.Core.FieldDoc", true));
			this.methods      = new ValueConstrainedArrayList(Type.GetType("Mono.Doc.Core.MethodDoc", true));
			this.operators    = new ValueConstrainedArrayList(Type.GetType("Mono.Doc.Core.OperatorDoc", true));
			this.properties   = new ValueConstrainedArrayList(Type.GetType("Mono.Doc.Core.PropertyDoc", true));
			this.classes      = new ValueConstrainedArrayList(Type.GetType("Mono.Doc.Core.ClassDoc", true));
			this.enums        = new ValueConstrainedArrayList(Type.GetType("Mono.Doc.Core.EnumDoc", true));
			this.interfaces   = new ValueConstrainedArrayList(Type.GetType("Mono.Doc.Core.InterfaceDoc", true));
			this.structs      = new ValueConstrainedArrayList(Type.GetType("Mono.Doc.Core.StructDoc", true));
		}

		public AbstractClassStructDoc() : this(string.Empty)
		{
		}

		public ValueConstrainedArrayList Constructors
		{
			get { return this.constructors;  }
		}

		public ValueConstrainedArrayList Events
		{
			get { return this.events;  }
		}

		public ValueConstrainedArrayList Fields
		{
			get { return this.fields;  }
		}

		public ValueConstrainedArrayList Methods
		{
			get { return this.methods;  }
		}

		public ValueConstrainedArrayList Operators
		{
			get { return this.operators;  }
		}

		public ValueConstrainedArrayList Properties
		{
			get { return this.properties;  }
		}

		public ValueConstrainedArrayList NestedClasses
		{
			get { return this.classes;  }
		}

		public ValueConstrainedArrayList NestedDelegates
		{
			get { return this.delegates;  }
		}

		public ValueConstrainedArrayList NestedEnums
		{
			get { return this.enums;  }
		}

		public ValueConstrainedArrayList NestedInterfaces
		{
			get { return this.interfaces;  }
		}

		public ValueConstrainedArrayList NestedStructs
		{
			get { return this.structs;  }
		}
	}
}
