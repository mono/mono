Imports system

Module M
	private i as integer

	public WriteOnly Property p() as Integer
		SET (ByVal val as Integer)
			i = val
		End SET
	End Property

	public ReadOnly Property p1() as Integer
		GET
			return i
		END GET
	End Property


	Sub Main()
	End Sub

End Module
