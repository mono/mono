'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell Inc. (http://www.novell.com)
'Derived multi-use
 
Imports System

<AttributeUsage(AttributeTargets.Class, AllowMultiple := True, _
                Inherited := True )> _
Class MultiUseAttribute 
	Inherits System.Attribute
	Public Sub New(ByVal Value As Boolean)
	End Sub
End Class

<AttributeUsage(AttributeTargets.Class, Inherited := True)> _
Class SingleUseAttribute
	Inherits Attribute
	Public Sub New(ByVal Value As Boolean)
	End Sub
End Class

<SingleUse(True), MultiUse(True)> Class Base
End Class

<SingleUse(False), MultiUse(False)> _
Class Derived
	Inherits Base
End Class

Module Test 
	Sub Main()
		Dim type As Type = GetType(Derived)
		Dim arr() As Object = _
		type.GetCustomAttributes(GetType(MultiUseAttribute), True)
		if arr.Length <> 2 Then 
			Throw New Exception ("The Base Class should get single-use multiuse attribute .Expected 2 but got " & arr.Length)
		End If	
	End Sub
End Module 
