REM LineNo: 17
REM ExpectedError: BC31066
REM ErrorMessage: Method in a Module cannot be declared 'Protected' or 'Protected Friend'.


Module ClsModule
	Sub New()
	End Sub

	Public a As Integer
	Public Const b as integer = 10

	Class C1
	End Class

	' inherited members can be declared as protected
	protected Sub Finalize()
	end sub
End Module

Module MainModule
	Sub Main()
		ClsModule.a=20
	End Sub
End Module
