Module ExpConversionBoolToDecimal
	Sub Main()
		Dim a as Boolean = False
		Dim b as Decimal = CDec(a)
		if b <> 0
			Throw new System.Exception("Boolean to Decimal Conversion failed. Expected 0 but got " & b)
		End if	
	End Sub
End Module