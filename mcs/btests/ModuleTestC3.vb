REM LineNo: 6
REM ExpectedError: BC31089
REM ErrorMessage: Types declared 'Private' must be inside another type.

NameSpace N
	Private Module M1
		Sub Main()
		End Sub
	End Module

	Public Module M2
		Sub Main ()
		End Sub
	End Module
End NameSpace
