REM LineNo: 9
REM ExpectedError: BC30678
REM ErrorMessage: 'End' statement not valid.

Imports system

Public Structure s
		Dim a As Integer
		public MustOverride Property p() as Integer
End Structure


Module M
	private i as integer

	
	Sub Main()
	End Sub

End Module
