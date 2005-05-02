REM LineNo: 29
REM ExpectedError: BC30663 
REM ErrorMessage: Attribute 'SingleUseAttribute' cannot be applied multiple times.

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



<SingleUse(False), SingleUse(False)> _
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
