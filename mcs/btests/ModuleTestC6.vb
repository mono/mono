REM LineNo: 14
REM ExpectedError: BC30593
REM ErrorMessage: Variables in Modules cannot be declared 'Protected'.

REM LineNo: 15
REM ExpectedError: BC30593
REM ErrorMessage: Variables in Modules cannot be declared 'Protected'.

REM LineNo: 21
REM ExpectedError: BC31066
REM ErrorMessage: Method in a Module cannot be declared 'Protected' or 'Protected Friend'.

Module ClsModule
	Protected a As Integer
	Protected Const b as integer = 10

	'Protected Class C1
	Class C1
	End Class

	protected Sub s()
	end sub
End Module 

Module MainModule
	Sub Main()
	End Sub
End Module
