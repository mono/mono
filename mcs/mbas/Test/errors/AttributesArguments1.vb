REM LineNo: 21
REM ExpectedError: BC30060 
REM ErrorMessage: Conversion from 'Integer' to 'String' cannot occur in a constant expression.

Imports System
<AttributeUsage(AttributeTargets.All)> _
Public Class GeneralAttribute
	Inherits Attribute
	Public Sub New(ByVal x As string)
	End Sub
	Public y As Type
	Public Property z As Integer
		Get
		End Get
		Set
		End Set
	End Property
End Class


<General(10, z := 30, y := GetType(Integer))> _
Class Foo
End Class

<General(10.5, z := 10)> _
Class Bar
End Class


Module Test
Sub Main ()
End Sub
End Module 