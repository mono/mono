REM LineNo: 10
REM ExpectedError: BC30124
REM ErrorMessage: Property without a 'ReadOnly' or 'WriteOnly' specifier must provide both a 'Get' and a 'Set'.

Imports system

Module M
	private i as integer

	public Property p() as Integer
		Get
			return i
		END Get
	End Property

	Sub Main()
		p = 10
		Console.WriteLine(p)
	End Sub

End Module
