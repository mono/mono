REM LineNo: 10
REM ExpectedError: BC31408
REM ErrorMessage: 'Private' and 'Overridable' cannot be combined.

REM LineNo: 12
REM ExpectedError: BC30501
REM ErrorMessage: 'Shared' cannot be combined with 'Overridable' on a method declaration.

Class C1
	Private Overridable Sub S() 'Overridable methods should not be private
	End Sub
	Shared Overridable Sub F() 'Overridable methods should not be shared
	End Sub
End Class
Module OverrideC2
	Sub Main()
	End Sub
End Module
