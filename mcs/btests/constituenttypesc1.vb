Imports System
Public Class C1
	Private Class C2
	End Class
	Public Function F() As C2 'Function F exposes private class C2
	End Function

	Private Function F1() As C2
	End Function
End Class
Module ConstituentTypes
	Sub Main()
		Dim myC As New C1()
		
	End Sub
End Module
