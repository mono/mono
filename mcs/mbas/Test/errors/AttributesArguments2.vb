REM LineNo: 22
REM ExpectedError: BC30390 
REM ErrorMessage: 'GeneralAttribute.Private Sub New(x As Integer)' is not accessible in this context because it is 'Private'.


Imports System
<AttributeUsage(AttributeTargets.All)> _
Public Class GeneralAttribute
	Inherits Attribute
	Private Sub New(ByVal x As Integer)
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