REM LineNo: 6
REM ExpectedError: BC31408
REM ErrorMessage: 'Private' and 'Overridable' cannot be combined.

Class C1
	Private Overridable Sub S() 'Overridable methods should not be private
	End Sub
End Class
Module OverrideC2
	Sub Main()
	End Sub
End Module
