Module ExpConversionofBoolToDouble
        Sub Main()
                Dim a as Boolean = False
                Dim b as Double = CDbl(a)
                if b <> 0 then
                        Throw New System.Exception("Explicit Conversion of Bool(False) to Double has Failed. Expected 0, but got " & b)
                End if
        End Sub
End Module
