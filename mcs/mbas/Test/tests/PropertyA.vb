Module M
	private i as integer

	public Property p() as Integer
		GET
			return i
		END GET

		SET (ByVal val as Integer)
			i = val
		End SET

	End Property

	Sub Main()
		p = 10
		System.Console.WriteLine(p)
	End Sub

End Module
