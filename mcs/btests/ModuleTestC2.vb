REM LineNo: 16
REM ExpectedError: BC30625
REM ErrorMessage: 'Module' statement must end with a matching 'End Module'.

REM LineNo: 18
REM ExpectedError: BC30289
REM ErrorMessage: Statement cannot appear within a method body. End of method assumed.

REM LineNo: 19
REM ExpectedError: BC30429
REM ErrorMessage: 'End Sub' must be preceded by a matching 'Sub'.

Module M
End Module

Module ModuleTest
	Sub Main()
		Module M = new Module		
	End Sub
End Module
