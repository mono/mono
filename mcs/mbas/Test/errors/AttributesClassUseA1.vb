REM LineNo: 25
REM ExpectedError: BC30045
REM ErrorMessage: Positional parameters must be ByVal and may not specify ByRef,


Imports System

<AttributeUsage(AttributeTargets.Class, AllowMultiple := True, _
                Inherited := True )> _
Class MultiUseAttribute 
	Inherits System.Attribute

	Public Sub New(ByRef Value As Integer)
	End Sub
End Class

<AttributeUsage(AttributeTargets.Class, Inherited := True)> _
Class SingleUseAttribute
	Inherits Attribute

	Public Sub New(ByRef Value As Integer)
	End Sub
End Class

<SingleUse(1), MultiUse(1)> Class Base
End Class



<SingleUse(0), MultiUse(0)> _
Class Derived
	Inherits Base
End Class

Module Test 
	Sub Main()
		Dim type As Type = GetType(Base)
		Dim arr() As Object = _
                type.GetCustomAttributes(GetType(SingleUseAttribute), True)
	End Sub
End Module 
