REM LineNo: 15
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

REM LineNo: 16
REM ExpectedError: BC30001
REM ErrorMessage: Statement is not valid in a namespace.

REM LineNo: 17
REM ExpectedError: BC30678
REM ErrorMessage: 'End' statement not valid.

Imports system

Public Struct s
		public MustOverride Property p() as Integer
End Struct


Module M
	private i as integer

	
	Sub Main()
	End Sub

End Module
