//
// System.Drawing.PrintingPermission.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//
// (C) 2002 Ximian, Inc
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
