// System.ComponentModel.Design.IResourceService.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Globalization;
using System.Resources;

namespace System.ComponentModel.Design
{
	public interface IResourceService
	{
		IResourceReader GetResourceReader (CultureInfo info);

		IResourceWriter GetResourceWriter (CultureInfo info);
	}
}
