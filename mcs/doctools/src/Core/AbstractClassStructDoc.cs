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
using System.Reflection;
using System.Xml.Serialization;

namespace Mono.Doc.Core
{
	public abstract class AbstractClassStructDoc : AbstractTypeDoc
	{
		protected ValueConstrainedArrayList constructors = new ValueConstrainedArrayList(typeof(ConstructorDoc));
		protected ValueConstrainedArrayList       events = new ValueConstrainedArrayList(typeof(EventDoc));
		protected ValueConstrainedArrayList       fields = new ValueConstrainedArrayList(typeof(FieldDoc));
		protected ValueConstrainedArrayList      methods = new ValueConstrainedArrayList(typeof(MethodDoc));
		protected ValueConstrainedArrayList    operators = new ValueConstrainedArrayList(typeof(OperatorDoc));
		protected ValueConstrainedArrayList   properties = new ValueConstrainedArrayList(typeof(PropertyDoc));

		// nested items
		protected ValueConstrainedArrayList      classes = new ValueConstrainedArrayList(typeof(ClassDoc));
		protected ValueConstrainedArrayList    delegates = new ValueConstrainedArrayList(typeof(DelegateDoc));
		protected ValueConstrainedArrayList        enums = new ValueConstrainedArrayList(typeof(EnumDoc));
		protected ValueConstrainedArrayList   interfaces = new ValueConstrainedArrayList(typeof(InterfaceDoc));
		protected ValueConstrainedArrayList      structs = new ValueConstrainedArrayList(typeof(StructDoc));

		public AbstractClassStructDoc(string name) : base(name)
		{
		}

		public AbstractClassStructDoc() : this(string.Empty)
		{
		}

		public AbstractClassStructDoc(Type t, AssemblyLoader loader) : base(t, loader)
		{
			// constructors
			foreach (ConstructorInfo cons in loader.GetConstructors(t))
			{
				this.Constructors.Add(new ConstructorDoc(cons, loader));
			}

			// events
			foreach (EventInfo ev in loader.GetEvents(t))
			{
				this.Events.Add(new EventDoc(ev, loader));
			}

			// fields
			foreach (FieldInfo field in loader.GetFields(t))
			{
				this.Fields.Add(new FieldDoc(TypeNameHelper.GetName(field)));
			}

			// methods
			foreach (MethodInfo m in loader.GetMethods(t))
			{
				this.Methods.Add(new MethodDoc(m, loader));
			}

			// operators
			foreach (MethodInfo o in loader.GetOperators(t))
			{
				this.Operators.Add(new OperatorDoc(o, loader));
			}

			// properties
			foreach (PropertyInfo prop in loader.GetProperties(t))
			{
				this.Properties.Add(new PropertyDoc(prop, loader));
			}

			// TODO: nested types
		}

		[XmlElement(ElementName = "constructor", Type = typeof(ConstructorDoc))]
		public ValueConstrainedArrayList Constructors
		{
			get { return this.constructors;  }
		}

		[XmlElement(ElementName = "event", Type = typeof(EventDoc))]
		public ValueConstrainedArrayList Events
		{
			get { return this.events;  }
		}

		[XmlElement(ElementName = "field", Type = typeof(FieldDoc))]
		public ValueConstrainedArrayList Fields
		{
			get { return this.fields;  }
		}

		[XmlElement(ElementName = "method", Type = typeof(MethodDoc))]
		public ValueConstrainedArrayList Methods
		{
			get { return this.methods;  }
		}

		[XmlElement(ElementName = "operator", Type = typeof(OperatorDoc))]
		public ValueConstrainedArrayList Operators
		{
			get { return this.operators;  }
		}

		[XmlElement(ElementName = "property", Type = typeof(PropertyDoc))]
		public ValueConstrainedArrayList Properties
		{
			get { return this.properties;  }
		}

		[XmlElement(ElementName = "class", Type = typeof(ClassDoc))]
		public ValueConstrainedArrayList Classes
		{
			get { return this.classes;  }
		}

		[XmlElement(ElementName = "delegate", Type = typeof(DelegateDoc))]
		public ValueConstrainedArrayList Delegates
		{
			get { return this.delegates;  }
		}

		[XmlElement(ElementName = "enum", Type = typeof(EnumDoc))]
		public ValueConstrainedArrayList Enums
		{
			get { return this.enums;  }
		}

		[XmlElement(ElementName = "interface", Type = typeof(InterfaceDoc))]
		public ValueConstrainedArrayList Interfaces
		{
			get { return this.interfaces;  }
		}

		[XmlElement(ElementName = "struct", Type = typeof(StructDoc))]
		public ValueConstrainedArrayList Structs
		{
			get { return this.structs;  }
		}
	}
}
