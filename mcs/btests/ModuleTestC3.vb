REM LineNo: 8
REM ExpectedError: BC30420
REM ErrorMessage: 'Sub Main' was not found in 'ModuleTestC3'.

REM LineNo: 10
REM ExpectedError: BC31089
REM ErrorMessage: Types declared 'Private' must be inside another type.

NameSpace N
Private Module M
	Sub Main()
	End Sub
End Module
End NameSpace
