Module ExpConversionDecimaltoBool
	Sub Main()
		Dim a as Decimal = 123 
		Dim b as Boolean
		b = CBool(a)
		if b <> True
			Throw new System.Exception("Decimal to Boolean Conversion is not working properly. Expected True but got " &b)
		End if	
	End Sub
End Module
