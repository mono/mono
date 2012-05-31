/*
  Copyright (C) 2009-2012 Jeroen Frijters

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
using IKVM.Reflection.Reader;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection
{
	public sealed class ManifestResourceInfo
	{
		private readonly ModuleReader module;
		private readonly int index;

		internal ManifestResourceInfo(ModuleReader module, int index)
		{
			this.module = module;
			this.index = index;
		}

		public ResourceAttributes __ResourceAttributes
		{
			get { return (ResourceAttributes)module.ManifestResource.records[index].Flags; }
		}

		public int __Offset
		{
			get { return module.ManifestResource.records[index].Offset; }
		}

		public ResourceLocation ResourceLocation
		{
			get
			{
				int implementation = module.ManifestResource.records[index].Implementation;
				if ((implementation >> 24) == AssemblyRefTable.Index)
				{
					Assembly asm = ReferencedAssembly;
					if (asm == null || asm.__IsMissing)
					{
						return ResourceLocation.ContainedInAnotherAssembly;
					}
					return asm.GetManifestResourceInfo(module.GetString(module.ManifestResource.records[index].Name)).ResourceLocation | ResourceLocation.ContainedInAnotherAssembly;
				}
				else if ((implementation >> 24) == FileTable.Index)
				{
					if ((implementation & 0xFFFFFF) == 0)
					{
						return ResourceLocation.ContainedInManifestFile | ResourceLocation.Embedded;
					}
					return 0;
				}
				else
				{
					throw new BadImageFormatException();
				}
			}
		}

		public Assembly ReferencedAssembly
		{
			get
			{
				int implementation = module.ManifestResource.records[index].Implementation;
				if ((implementation >> 24) == AssemblyRefTable.Index)
				{
					return module.ResolveAssemblyRef((implementation & 0xFFFFFF) - 1);
				}
				return null;
			}
		}

		public string FileName
		{
			get
			{
				int implementation = module.ManifestResource.records[index].Implementation;
				if ((implementation >> 24) == FileTable.Index)
				{
					if ((implementation & 0xFFFFFF) == 0)
					{
						return null;
					}
					else
					{
						return module.GetString(module.File.records[(implementation & 0xFFFFFF) - 1].Name);
					}
				}
				return null;
			}
		}
	}
}
