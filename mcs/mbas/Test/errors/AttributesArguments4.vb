REM LineNo: 15
REM ExpectedError: BC30661 
REM ErrorMessage: Field or property 'z' is not found.

Imports System
<AttributeUsage(AttributeTargets.All)> _
Public Class GeneralAttribute
	Inherits Attribute
	Public Sub New(ByVal A As Type)
	End Sub
	Public y As Type
End Class


<General(GetType(GeneralAttribute), z := 10)> _
Class Bar
End Class


Module Test
	Sub Main ()
		Dim x 
	End Sub
End Module 