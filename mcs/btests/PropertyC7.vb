REM LineNo: 14
REM ExpectedError: BC30503
REM ErrorMessage: Properties in a Module cannot be declared 'MustOverride'.

REM LineNo: 14
REM ExpectedError: BC30124
REM ErrorMessage: Property without a 'ReadOnly' or 'WriteOnly' specifier must provide both a 'Get' and a 'Set'.

Imports system

Module M
	private i as integer

	public MustOverride Property p() as Integer

	Sub Main()
	End Sub

End Module
