//
// System.Drawing.PrintingPermissionAttribute.cs
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
	/// Summary description for PrintingPermissionAttribute.
	/// </summary>
	/// 
	[AttributeUsage(AttributeTargets.All)]
	public sealed class PrintingPermissionAttribute : CodeAccessSecurityAttribute
	{
		private PrintingPermissionLevel _Level;
		
		public PrintingPermissionAttribute(SecurityAction action) : base(action)
		{
			// seems to always assign PrintingPermissionLevel.NoPrinting ...
			Level = PrintingPermissionLevel.NoPrinting;
		}
		
		public PrintingPermissionLevel Level {
			get{
				return _Level;
			}
			set{
				_Level = value;
			}
		}
		
		public override IPermission CreatePermission(){
			return new PrintingPermission(this.Level);
		}
	}
}
