'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell Inc. (http://www.novell.com)
'Base single-use 
 
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
		Dim type As Type = GetType(Base)
		Dim arr() As Object = _
		type.GetCustomAttributes(GetType(SingleuseAttribute), True)
		if arr.Length <> 1 Then 
			Throw New Exception ("The Base Class should get one attribute . but got " & arr.Length)
		End If	
	End Sub
End Module 
