// AbstractClassStructDoc.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

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
