Module ExpConversionBytetoBool
	Sub Main()
		Dim a as Byte = 123 
		Dim b as Boolean
		b = CBool(a)
		if b <> True
			Throw new System.Exception("Byte to Bool Conversion is not working properly. Expected True but got " &b)
		End if	
	End Sub
End Module
