REM LineNo: 15
REM ExpectedError: BC31065
REM ErrorMessage: 'Set' parameter cannot be declared 'ByRef'.

Imports system

Module M
	private i as integer

	public Property p() as Integer
		GET
			return i
		END GET

		SET (ByRef val as Integer)
			i = val
		End SET

	End Property

	Sub Main()
		p = 10
		Console.WriteLine(p)
	End Sub

End Module
