REM LineNo: 8
REM ExpectedError: BC31088
REM ErrorMessage: 'NotOverridable' cannot be specified for methods that do not override another method.

Class C1
	'methods that do not override any other 
	'method should not be declared 'notoverridable'
	Public Notoverridable Sub S() 
	End Sub
End Class
Module OverrideC2
	Sub Main()
	End Sub
End Module
