REM LineNo: 6
REM ExpectedError: BC30001
REM ErrorMessage: Statement is not valid in a namespace.

Namespace NS1
	Dim a As Integer
	Module NamespaceTest
		Sub Main()
		End Sub
	End Module
End Namespace
