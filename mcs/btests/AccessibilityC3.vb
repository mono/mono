REM LineNo: 23
REM ExpectedError: BC30451
REM ErrorMessage: Name 'S2' is not declared.

REM LineNo: 24
REM ExpectedError: BC30451
REM ErrorMessage: Name 'C' is not declared.

REM LineNo: 31
REM ExpectedError: BC30456
REM ErrorMessage: 'S2' is not a member of 'C1'.

REM LineNo: 32
REM ExpectedError: BC30456
REM ErrorMessage: 'C' is not a member of 'C1'.

Class C1
End Class

Class C2
	Inherits C1
	Public Sub S1()
		S2()
		C = 10
	End Sub
End Class

Class C3
	Public Sub S()
		Dim myC As New C1()
		myC.S2()
		myC.C = 20
	End Sub
End Class

Module Accessibility
	Sub Main()
	End Sub
End Module
