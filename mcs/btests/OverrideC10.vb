REM LineNo: 6
REM ExpectedError: BC30501
REM ErrorMessage: 'Shared' cannot be combined with 'Overridable' on a method declaration.

Class C1
	Shared Overridable Sub F() 'Overridable methods should not be shared
	End Sub
End Class
Module OverrideC2
	Sub Main()
	End Sub
End Module
