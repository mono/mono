//
// System.ComponentModel.RunInstallerAttribute.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//

using System;

namespace System.ComponentModel
{
	[AttributeUsageAttribute(AttributeTargets.Class)]
	public class RunInstallerAttribute : Attribute
	{
		public static readonly RunInstallerAttribute Yes = new RunInstallerAttribute (true);
		public static readonly RunInstallerAttribute No = new RunInstallerAttribute (false);
		public static readonly RunInstallerAttribute Default = new RunInstallerAttribute (false);

		private bool runInstaller;

		public RunInstallerAttribute (bool runInstaller)
		{
			this.runInstaller = runInstaller;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is RunInstallerAttribute))
				return false;

			return ((RunInstallerAttribute) obj).RunInstaller.Equals (runInstaller);
		}

		public override int GetHashCode ()
		{
			return runInstaller.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return Equals (Default);
		}

		public bool RunInstaller
		{
			get { return runInstaller; }
		}
	}
}

