REM LineNo: 7
REM ExpectedError: BC30617
REM ErrorMessage: 'Module' statements can occur only at file or namespace level.

Imports System
Module ModuleTest
	Module ModuleInner
	End Module
	Sub Main()
	End Sub
End Module
