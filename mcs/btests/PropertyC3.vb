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
