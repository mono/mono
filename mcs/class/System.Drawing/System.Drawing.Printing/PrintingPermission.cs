//
// System.Drawing.PrintingPermission.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//
// (C) 2002 Ximian, Inc
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Security;
using System.Security.Permissions;

namespace System.Drawing.Printing
{
	/// <summary>
	/// Summary description for PrintingPermission.
	/// </summary>
	/// 
	[Serializable]
	public sealed class PrintingPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		private PrintingPermissionLevel _Level;
		
		public PrintingPermission(PermissionState state) {
			switch (state)
			{
				case PermissionState.None:
					Level = PrintingPermissionLevel.NoPrinting;
					break;
				case PermissionState.Unrestricted:
					Level = PrintingPermissionLevel.AllPrinting;
					break;
				default:
					// should never happen
					throw new ArgumentException("state");
			}
		}
		public PrintingPermission(PrintingPermissionLevel printingLevel) {
			Level = printingLevel;
		}
		
// properties
		public PrintingPermissionLevel Level{
			get{
				return _Level;
			}
			set{
				_Level = value;
			}
		}

// methods
		public override IPermission Copy(){
			return new PrintingPermission(this.Level);
		}
		
		[MonoTODO("PrintingPermission.FromXml")]
		public override void FromXml(SecurityElement esd)
		{
			throw new NotImplementedException();
		}
		
		public override IPermission Intersect(IPermission target)
		{
			if (this.IsSubsetOf(target))
				return this.Copy();
			else
				return target.Copy();
		}
		
		public override bool IsSubsetOf(IPermission target)
		{
			if (!(target is PrintingPermission))
				throw new ArgumentException("target");
			
			return this.Level <= (target as PrintingPermission).Level;
		}
		
		public bool IsUnrestricted()
		{
			return (this.Level == PrintingPermissionLevel.AllPrinting);
		}
		
		[MonoTODO("PrintingPermission.ToXml")]
		public override SecurityElement ToXml()
		{
			throw new NotImplementedException();
		}
		
		public override IPermission Union(IPermission target)
		{
			if (this.IsSubsetOf(target))
				return target.Copy();
			else
				return this.Copy();
		}
	}
}
