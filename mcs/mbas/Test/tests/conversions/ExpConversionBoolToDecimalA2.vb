Module ExpConversionofBoolToDecimal
        Sub Main()
                Dim a as Boolean = True
                Dim b as Decimal = CDec(a)
                if b <> -1 then
                        Throw New System.Exception("Explicit Conversion of Bool(True) to Decimal has Failed. Expected -1, but got " & b)
                End if
        End Sub
End Module
