//
// System.Runtime.InteropServices.Expando.IExpando.cs
//
// Author:
//    Alejandro Sánchez Acosta (raciel@es.gnu.org)
// 
// (C) Alejandro Sánchez Acosta
// 

using System.Reflection;
using System.Reflection.Emit;

namespace System.Runtime.InteropServices.Expando
{
	[Guid("afbf15e6-c37c-11d2-b88e-00a0c9b471b8")]
	public interface IExpando : IReflect
	{
		FieldInfo AddField (string name);

		MethodInfo AddMethod (string name, Delegate method);

		PropertyInfo AddProperty(string name);

		void RemoveMember(MemberInfo m);
	}
}
