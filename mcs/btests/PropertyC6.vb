Imports system

Module M
	private i as integer

	public ReadOnly Property p() as Integer
		GET
			return i
		END GET

		SET (ByVal val as Integer)
			i = val
		End SET
	End Property

	Sub Main()
	End Sub

End Module
