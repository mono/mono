Imports system

Module M
	private i as integer

	public Property p(ByVal x as Integer) as Integer
		GET
			return i 
		END GET

		SET (ByVal val as Integer)
			i = val
		End SET

	End Property

	Sub Main()
		p(5) = 10
                'Console.WriteLine(p)
	End Sub

End Module
