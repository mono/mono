Imports System
Module Scope1
	Public Function S() As Integer
		Return 1
	End Function
End Module
Module Scope
	Sub Main()
		Dim a As Integer=S()
		If a<>1 Then
			Throw New Exception("ScopeA:Failed-public method should be visible in other modules too")
		End If
	End Sub
End Module
