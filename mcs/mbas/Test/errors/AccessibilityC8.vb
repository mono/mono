REM LineNo: 16
REM ExpectedError: BC30390
REM ErrorMessage: 'C1.Private Sub S()' is not accessible in this context because it is 'Private'.

Imports System 'Should generate five compilation errors
Class C1
	Private a As Integer=10
	Protected b As Integer=30
	Private Sub S()
	End Sub
End Class

Class C3
	Public Sub S()
		Dim myC As New C1()
		myC.S()
	End Sub
End Class
Module Accessibility
	Sub Main()
	End Sub
End Module
