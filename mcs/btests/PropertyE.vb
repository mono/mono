Imports system

MustInherit Class C1
	public MustOverride Property p() as Integer
End Class


Class C2
	Inherits C1

	private i as integer

	public shadows Property p() as Integer
		SET (ByVal val as Integer)
			i = val
		End SET

		GET
			return i
		END GET
	End Property
End Class


Module M
	Sub Main()
	End Sub

End Module
