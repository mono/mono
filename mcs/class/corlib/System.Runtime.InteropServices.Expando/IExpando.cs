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
	//[Guid("")]
	public interface IExpando : IReflect
	{
		FieldInfo AddField (string name);

		MethodInfo AddMethod (string name, Delegate method);

		PropertyInfo AddProperty(string name);

		void RemoveMember(MemberInfo m);
	}
}
