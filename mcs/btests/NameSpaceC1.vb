REM LineNo: 7
REM ExpectedError: BC30618
REM ErrorMessage: 'Namespace' statements can occur only at file or namespace level.

Namespace NS1
	Module NamespaceTest
		Namespace NS2
		End Namespace
		Sub Main()
		End Sub
	End Module
End Namespace
