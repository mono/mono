REM LineNo: 44
REM ExpectedError: BC30390
REM ErrorMessage: 'C1.Private Sub S()' is not accessible in this context because it is 'Private'.

REM LineNo: 45
REM ExpectedError: BC30390
REM ErrorMessage: 'C1.a' is not accessible in this context because it is 'Private'.

REM LineNo: 52
REM ExpectedError: BC30390
REM ErrorMessage: 'C1.Private Sub S()' is not accessible in this context because it is 'Private'.

REM LineNo: 53
REM ExpectedError: BC30390
REM ErrorMessage: 'C1.a' is not accessible in this context because it is 'Private'.

REM LineNo: 54
REM ExpectedError: BC30390
REM ErrorMessage: 'C1.b' is not accessible in this context because it is 'Protected'.

REM LineNo: 60
REM ExpectedError: BC30390
REM ErrorMessage: 'C1.Private Sub S()' is not accessible in this context because it is 'Private'.

REM LineNo: 61
REM ExpectedError: BC30390
REM ErrorMessage: 'C1.a' is not accessible in this context because it is 'Private'.

REM LineNo: 62
REM ExpectedError: BC30390
REM ErrorMessage: 'C1.b' is not accessible in this context because it is 'Protected'.

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
		S()
		Console.WriteLine(a)
	End Sub
End Class

Class C3
	Public Sub S()
		Dim myC As New C1()
		myC.S()
		Console.WriteLine(myC.a)
		Console.WriteLine(myC.b)
	End Sub
End Class
Module Accessibility
	Sub Main()
		Dim myC1 As New C1()
		myC1.S()
		Console.WriteLine(myC1.a)	
		Console.WriteLine(myC1.b)

		Dim myC2 As New C2()
		myC2.S1()

		Dim myC3 As New C3()
		myC3.S()
	End Sub
End Module
