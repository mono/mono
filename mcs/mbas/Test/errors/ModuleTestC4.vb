REM LineNo: 4
REM ExpectedError: BC30738
REM ErrorMessage: 'Sub Main' is declared more than once in 'ModuleTestC4': M1.Main(), M2.Main()

Module M1
	Sub Main ()
	End Sub
End Module

Module M2
	Sub Main ()
	End Sub
End Module
