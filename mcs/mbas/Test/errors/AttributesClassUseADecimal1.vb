REM LineNo: 31 
REM ExpectedError: BC30659
REM ErrorMessage: 'A' does not have a valid attribute type


Imports System

<AttributeUsage(AttributeTargets.Class, AllowMultiple := True, _
                Inherited := True )> _
Class MultiUseAttribute 
	Inherits System.Attribute

	Public Sub New(ByVal Value As Integer)
	End Sub
End Class

<AttributeUsage(AttributeTargets.Class, Inherited := True)> _
Class SingleUseAttribute
	Inherits Attribute
	Property A () As Decimal
	Get
	End Get
	Set
	End Set
	End Property

	Public Sub New(ByVal Value As Integer)
	End Sub
End Class

<SingleUse(1,A:=1.1), MultiUse(1)> Class Base
End Class

<SingleUse(0,A:=1.1), MultiUse(0)> _
Class Derived
    Inherits Base
End Class

Module Test 
	Sub Main()
		Dim type As Type = GetType(Base)
		Dim arr() As Object = _
            type.GetCustomAttributes(GetType(SingleUseAttribute), True)
		'Console.WriteLine(arr.Length)
	End Sub
End Module 
