/*
  Copyright (C) 2009 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class EventBuilder : EventInfo
	{
		private readonly TypeBuilder typeBuilder;
		private readonly string name;
		private EventAttributes attributes;
		private readonly int eventtype;
		private MethodBuilder addOnMethod;
		private MethodBuilder removeOnMethod;
		private MethodBuilder fireMethod;
		private List<MethodBuilder> otherMethods;
		private int lazyPseudoToken;

		internal EventBuilder(TypeBuilder typeBuilder, string name, EventAttributes attributes, Type eventtype)
		{
			this.typeBuilder = typeBuilder;
			this.name = name;
			this.attributes = attributes;
			this.eventtype = typeBuilder.ModuleBuilder.GetTypeTokenForMemberRef(eventtype);
		}

		public void SetAddOnMethod(MethodBuilder mdBuilder)
		{
			addOnMethod = mdBuilder;
		}

		public void SetRemoveOnMethod(MethodBuilder mdBuilder)
		{
			removeOnMethod = mdBuilder;
		}

		public void SetRaiseMethod(MethodBuilder mdBuilder)
		{
			fireMethod = mdBuilder;
		}

		public void AddOtherMethod(MethodBuilder mdBuilder)
		{
			if (otherMethods == null)
			{
				otherMethods = new List<MethodBuilder>();
			}
			otherMethods.Add(mdBuilder);
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			Universe u = typeBuilder.ModuleBuilder.universe;
			if (customBuilder.Constructor.DeclaringType == u.System_Runtime_CompilerServices_SpecialNameAttribute)
			{
				attributes |= EventAttributes.SpecialName;
			}
			else
			{
				if (lazyPseudoToken == 0)
				{
					lazyPseudoToken = typeBuilder.ModuleBuilder.AllocPseudoToken();
				}
				typeBuilder.ModuleBuilder.SetCustomAttribute(lazyPseudoToken, customBuilder);
			}
		}

		public override EventAttributes Attributes
		{
			get { return attributes; }
		}

		public override MethodInfo GetAddMethod(bool nonPublic)
		{
			return nonPublic || (addOnMethod != null && addOnMethod.IsPublic) ? addOnMethod : null;
		}

		public override MethodInfo GetRemoveMethod(bool nonPublic)
		{
			return nonPublic || (removeOnMethod != null && removeOnMethod.IsPublic) ? removeOnMethod : null;
		}

		public override MethodInfo GetRaiseMethod(bool nonPublic)
		{
			return nonPublic || (fireMethod != null && fireMethod.IsPublic) ? fireMethod : null;
		}

		public override MethodInfo[] GetOtherMethods(bool nonPublic)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			if (otherMethods != null)
			{
				foreach (MethodInfo method in otherMethods)
				{
					if (nonPublic || method.IsPublic)
					{
						list.Add(method);
					}
				}
			}
			return list.ToArray();
		}

		public override Type DeclaringType
		{
			get { return typeBuilder; }
		}

		public override string Name
		{
			get { return name; }
		}

		public override Module Module
		{
			get { return typeBuilder.ModuleBuilder; }
		}

		public EventToken GetEventToken()
		{
			if (lazyPseudoToken == 0)
			{
				lazyPseudoToken = typeBuilder.ModuleBuilder.AllocPseudoToken();
			}
			return new EventToken(lazyPseudoToken);
		}

		public override Type EventHandlerType
		{
			get { return typeBuilder.ModuleBuilder.ResolveType(eventtype); }
		}

		internal void Bake()
		{
			EventTable.Record rec = new EventTable.Record();
			rec.EventFlags = (short)attributes;
			rec.Name = typeBuilder.ModuleBuilder.Strings.Add(name);
			rec.EventType = eventtype;
			int token = 0x14000000 | typeBuilder.ModuleBuilder.Event.AddRecord(rec);

			if (lazyPseudoToken != 0)
			{
				typeBuilder.ModuleBuilder.RegisterTokenFixup(lazyPseudoToken, token);
			}

			if (addOnMethod != null)
			{
				AddMethodSemantics(MethodSemanticsTable.AddOn, addOnMethod.MetadataToken, token);
			}
			if (removeOnMethod != null)
			{
				AddMethodSemantics(MethodSemanticsTable.RemoveOn, removeOnMethod.MetadataToken, token);
			}
			if (fireMethod != null)
			{
				AddMethodSemantics(MethodSemanticsTable.Fire, fireMethod.MetadataToken, token);
			}
			if (otherMethods != null)
			{
				foreach (MethodBuilder method in otherMethods)
				{
					AddMethodSemantics(MethodSemanticsTable.Other, method.MetadataToken, token);
				}
			}
		}

		private void AddMethodSemantics(short semantics, int methodToken, int propertyToken)
		{
			MethodSemanticsTable.Record rec = new MethodSemanticsTable.Record();
			rec.Semantics = semantics;
			rec.Method = methodToken;
			rec.Association = propertyToken;
			typeBuilder.ModuleBuilder.MethodSemantics.AddRecord(rec);
		}

		internal override bool IsPublic
		{
			get
			{
				if ((addOnMethod != null && addOnMethod.IsPublic) || (removeOnMethod != null && removeOnMethod.IsPublic) || (fireMethod != null && fireMethod.IsPublic))
				{
					return true;
				}
				if (otherMethods != null)
				{
					foreach (MethodBuilder method in otherMethods)
					{
						if (method.IsPublic)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		internal override bool IsStatic
		{
			get
			{
				if ((addOnMethod != null && addOnMethod.IsStatic) || (removeOnMethod != null && removeOnMethod.IsStatic) || (fireMethod != null && fireMethod.IsStatic))
				{
					return true;
				}
				if (otherMethods != null)
				{
					foreach (MethodBuilder method in otherMethods)
					{
						if (method.IsStatic)
						{
							return true;
						}
					}
				}
				return false;
			}
		}
	}
}
