REM LineNo: 10
REM ExpectedError: BC30179
REM ErrorMessage: module 'NameSpaceTest' and module 'NameSpaceTest' conflict in namespace 'NS1'.

Namespace NS1
	Module NameSpaceTest
	End Module
End Namespace
Namespace NS1
	Module NameSpaceTest
		Sub Main()
		End Sub
	End Module
End Namespace
