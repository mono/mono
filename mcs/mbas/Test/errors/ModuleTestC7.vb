REM LineNo: 10
REM ExpectedError: BC31066
REM ErrorMessage: Method in a Module cannot be declared 'Protected' or 'Protected Friend'.

Module ClsModule
	'Protected Class C1
	Class C1
	End Class

	Protected Sub s()
	End Sub
End Module 

Module MainModule
	Sub Main()
	End Sub
End Module
