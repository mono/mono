/*
  Copyright (C) 2009-2011 Jeroen Frijters

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
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	sealed class ResourceModule : NonPEModule
	{
		private readonly ModuleReader manifest;
		private readonly int index;
		private readonly string location;

		internal ResourceModule(ModuleReader manifest, int index, string location)
			: base(manifest.universe)
		{
			this.manifest = manifest;
			this.index = index;
			this.location = location;
		}

		public override int MDStreamVersion
		{
			get { throw new NotSupportedException(); }
		}

		public override bool IsResource()
		{
			return true;
		}

		public override Assembly Assembly
		{
			get { return manifest.Assembly; }
		}

		public override string FullyQualifiedName
		{
			get { return location ?? "<Unknown>"; }
		}

		public override string Name
		{
			get { return location == null ? "<Unknown>" : System.IO.Path.GetFileName(location); }
		}

		public override string ScopeName
		{
			get { return manifest.GetString(manifest.File.records[index].Name); }
		}

		public override Guid ModuleVersionId
		{
			get { throw new NotSupportedException(); }
		}

		public override byte[] __ModuleHash
		{
			get
			{
				int blob = manifest.File.records[index].HashValue;
				return blob == 0 ? Empty<byte>.Array : manifest.GetBlobCopy(blob);
			}
		}

		internal override Type FindType(TypeName typeName)
		{
			return null;
		}

		internal override Type FindTypeIgnoreCase(TypeName lowerCaseName)
		{
			return null;
		}

		internal override void GetTypesImpl(List<Type> list)
		{
		}

		protected override Exception ArgumentOutOfRangeException()
		{
			return new NotSupportedException();
		}
	}
}
