REM LineNo: 6
REM ExpectedError: BC30593
REM ErrorMessage: Variables in Modules cannot be declared 'Protected'.

Module ClsModule
	Protected Const b as integer = 10

	'Protected Class C1
	Class C1
	End Class
End Module 

Module MainModule
	Sub Main()
	End Sub
End Module
