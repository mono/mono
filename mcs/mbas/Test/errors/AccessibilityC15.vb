REM LineNo: 11
REM ExpectedError: BC30451
REM ErrorMessage: Name 'S2' is not declared.

Class C1
End Class

Class C2
	Inherits C1
	Public Sub S1()
		S2()
	End Sub
End Class

Module Accessibility
	Sub Main()
	End Sub
End Module
