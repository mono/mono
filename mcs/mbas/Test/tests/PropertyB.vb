IMPORTS SYSTEM
Module M
	private i as integer

	public Property p() as Integer
		GET
			return i
		END GET

		SET
			i = Value
		End SET

	End Property

	Sub Main()
		p = 10
		Console.WriteLine(p)
	End Sub

End Module
