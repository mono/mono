REM LineNo: 16
REM ExpectedError: BC30390
REM ErrorMessage: 'C1.a' is not accessible in this context because it is 'Private'.

Imports System 'Should generate five compilation errors
Class C1
	Private a As Integer=10
	Protected b As Integer=30
	Private Sub S()
	End Sub
End Class

Class C2
	Inherits C1
	Public Sub S1()
		Console.WriteLine (a)
	End Sub
End Class

