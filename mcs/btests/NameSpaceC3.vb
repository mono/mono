REM LineNo: 8
REM ExpectedError: BC30420
REM ErrorMessage: 'Sub Main' was not found in 'NameSpaceC3'.

REM LineNo: 14
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
