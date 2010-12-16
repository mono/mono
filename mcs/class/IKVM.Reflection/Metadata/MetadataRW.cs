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
using System.Text;

namespace IKVM.Reflection.Metadata
{
	// base class for MetadataReader and MetadataWriter
	abstract class MetadataRW
	{
		internal readonly bool bigStrings;
		internal readonly bool bigGuids;
		internal readonly bool bigBlobs;
		internal readonly bool bigResolutionScope;
		internal readonly bool bigTypeDefOrRef;
		internal readonly bool bigMemberRefParent;
		internal readonly bool bigHasCustomAttribute;
		internal readonly bool bigCustomAttributeType;
		internal readonly bool bigMethodDefOrRef;
		internal readonly bool bigHasConstant;
		internal readonly bool bigHasSemantics;
		internal readonly bool bigHasFieldMarshal;
		internal readonly bool bigHasDeclSecurity;
		internal readonly bool bigTypeOrMethodDef;
		internal readonly bool bigMemberForwarded;
		internal readonly bool bigImplementation;
		internal readonly bool bigField;
		internal readonly bool bigMethodDef;
		internal readonly bool bigParam;
		internal readonly bool bigTypeDef;
		internal readonly bool bigProperty;
		internal readonly bool bigEvent;
		internal readonly bool bigGenericParam;
		internal readonly bool bigModuleRef;

		protected MetadataRW(Module module, bool bigStrings, bool bigGuids, bool bigBlobs)
		{
			this.bigStrings = bigStrings;
			this.bigGuids = bigGuids;
			this.bigBlobs = bigBlobs;
			this.bigField = module.Field.IsBig;
			this.bigMethodDef = module.MethodDef.IsBig;
			this.bigParam = module.Param.IsBig;
			this.bigTypeDef = module.TypeDef.IsBig;
			this.bigProperty = module.Property.IsBig;
			this.bigEvent = module.Event.IsBig;
			this.bigGenericParam = module.GenericParam.IsBig;
			this.bigModuleRef = module.ModuleRef.IsBig;
			this.bigResolutionScope = IsBig(2, module.ModuleTable, module.ModuleRef, module.AssemblyRef, module.TypeRef);
			this.bigTypeDefOrRef = IsBig(2, module.TypeDef, module.TypeRef, module.TypeSpec);
			this.bigMemberRefParent = IsBig(3, module.TypeDef, module.TypeRef, module.ModuleRef, module.MethodDef, module.TypeSpec);
			this.bigMethodDefOrRef = IsBig(1, module.MethodDef, module.MemberRef);
			this.bigHasCustomAttribute = IsBig(5, module.MethodDef, module.Field, module.TypeRef, module.TypeDef, module.Param, module.InterfaceImpl, module.MemberRef,
				module.ModuleTable, /*module.Permission,*/ module.Property, module.Event, module.StandAloneSig, module.ModuleRef, module.TypeSpec, module.AssemblyTable,
				module.AssemblyRef, module.File, module.ExportedType, module.ManifestResource);
			this.bigCustomAttributeType = IsBig(3, module.MethodDef, module.MemberRef);
			this.bigHasConstant = IsBig(2, module.Field, module.Param, module.Property);
			this.bigHasSemantics = IsBig(1, module.Event, module.Property);
			this.bigHasFieldMarshal = IsBig(1, module.Field, module.Param);
			this.bigHasDeclSecurity = IsBig(2, module.TypeDef, module.MethodDef, module.AssemblyTable);
			this.bigTypeOrMethodDef = IsBig(1, module.TypeDef, module.MethodDef);
			this.bigMemberForwarded = IsBig(1, module.Field, module.MethodDef);
			this.bigImplementation = IsBig(2, module.File, module.AssemblyRef, module.ExportedType);
		}

		private static bool IsBig(int bitsUsed, params Table[] tables)
		{
			int limit = 1 << (16 - bitsUsed);
			foreach (Table table in tables)
			{
				if (table.RowCount >= limit)
				{
					return true;
				}
			}
			return false;
		}
	}
}
