using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Xaml;

namespace Test.Elements
{
    [System.Windows.Markup.ContentProperty("Value")]
    public class Number
    {
        public Number() { }

        public Number(Number other)
        {
            Value = other.Value;
        }

        [TypeConverter(typeof(NameReferenceExpressionToDoubleConverter))]
        public Double Value { get; set; }
    }

    [TypeConverter(typeof(NameReferenceExpressionToNumberConverter))]
    public class ConvertibleNumber : Number
    {
    }

    [System.Windows.Markup.UsableDuringInitialization(true)]
    public class NumberTopDown : Number
    { }

    public class NestedNumber : Number, ISupportInitialize
    {
        public Number Number1 { get; set; }
        public Number Number2 { get; set; }
        public Number Number3 { get; set; }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
            Value = GetValue(Number1) + GetValue(Number2) + GetValue(Number3);
        }

        private static double GetValue(Number n)
        {
            return (n == null) ? 0 : n.Value;
        }
    }

    // ----------------------------------------------
    // Type Converter
    //

    public class NameReferenceExpressionToNumberConverter : NameReferenceExpressionToDoubleConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            object result = base.ConvertFrom(context, culture, value);
            if (!(result is Double))
            {
                return result;
            }
            var number = new ConvertibleNumber();
            number.Value = (Double)result;
            return number;
        }
   }

    public class NameReferenceExpressionToDoubleConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string text = (string)value;
            if (text == null)
            {
                throw new ArgumentException("Missing String Value", "value");
            }
            var nameResolver = (IXamlNameResolver)context;
            if (nameResolver == null)
            {
                throw new ArgumentException("Missing IXamlNameResolver", "context");
            }

            
            var names = new List<string>();
            var scanner = new MathExpressionScanner(text);
            foreach (MathExpressionToken scannerTok in scanner.Scan())
            {
                switch(scannerTok.TokenType)
                {
                    case MathTokenType.Name:
                        if(!names.Contains(scannerTok.Name))
                        {
                            names.Add(scannerTok.Name);
                        }
                        break;
                }
            }

            var unknownNameList = new List<string>();
            var nameValueTable = ExpressionServices.GetNameValueTable(nameResolver, text, out unknownNameList);

            if (unknownNameList != null && unknownNameList.Count > 0)
            {
                object fixup = nameResolver.GetFixupToken(unknownNameList);
                return fixup;
            }

            // if we have the values of all the names,
            // hand it to the expresssion parser and let 'er rip!
            //
            MathExpressionParser parser = new MathExpressionParser(nameValueTable);
            Double number = parser.Parse(text);
            return number;
        }
    }

    // ----------------------------------------------------
    // Markup Extension

    public class MathExtension : System.Windows.Markup.MarkupExtension
    {
        public MathExtension() { }

        public MathExtension(string expression)
        {
            Expression = expression;
        }

        public string Expression { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Expression == null)
            {
                return 0.0;
            }
            var nameResolver = (IXamlNameResolver)serviceProvider;
            if (nameResolver == null)
            {
                throw new ArgumentException("Missing IXamlNameResolver", "context");
            }

            List<string> unknownNameList;
            var nameValueTable = ExpressionServices.GetNameValueTable(nameResolver, Expression, out unknownNameList);

            if (unknownNameList != null && unknownNameList.Count > 0)
            {
                object fixup = nameResolver.GetFixupToken(unknownNameList);
                return fixup;
            }

            // if we have the values of all the names,
            // hand it to the expresssion parser and let 'er rip!
            //
            MathExpressionParser parser = new MathExpressionParser(nameValueTable);
            Double number = parser.Parse(Expression);
            return number;

        }
    }

    // ----------------------------------------------------
    // Markup Extension (that wraps and uses the TC version)

    public class MathTcExtension : System.Windows.Markup.MarkupExtension
    {
        NameReferenceExpressionToDoubleConverter _tc;

        public MathTcExtension()
        {
            _tc = new NameReferenceExpressionToDoubleConverter();
        }

        public MathTcExtension(string expression)
            :this()
        {
            Expression = expression;
        }

        public string Expression { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Expression == null)
            {
                return 0.0;
            }
            return _tc.ConvertFrom((ITypeDescriptorContext)serviceProvider, System.Globalization.CultureInfo.InvariantCulture, Expression);
        }
    }

    // -----------------------------------------------------------------------------------------
    // Markup Extension (that has a Number property to test forward refs on properties of an ME)
    public class MathPlusExtension : MathExtension
    {
        public Number AdditionalValue { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object result = base.ProvideValue(serviceProvider);
            if (!(result is double))
            {
                return result;
            }
            return new Number { Value = (double)result + AdditionalValue.Value };
        }
    }

    // ---------------------------------------------------------------------------------
    //   Common code for TC and ME implementations.
    //
    static class ExpressionServices
    {
        static public Dictionary<string, Double> GetNameValueTable(IXamlNameResolver nameResolver,
                                                    string expression, out List<string> unknownNames)
        {
            unknownNames = new List<string>();

            var allNames = new List<string>();
            var nameValueTable = new Dictionary<string, Double>();

            var scanner = new MathExpressionScanner(expression);
            foreach (MathExpressionToken scannerTok in scanner.Scan())
            {
                switch(scannerTok.TokenType)
                {
                    case MathTokenType.Name:
                        if(!allNames.Contains(scannerTok.Name))
                        {
                            allNames.Add(scannerTok.Name);
                        }
                        break;
                }
            }

            foreach (string name in allNames)
            {
                bool isFullyInit;
                object obj = nameResolver.Resolve(name, out isFullyInit);
                if (obj == null)
                {
                    if (nameResolver.IsFixupTokenAvailable)
                    {
                        unknownNames.Add(name);
                    }
                    else
                    {
                        throw new InvalidOperationException("Could not resolve name {0}");
                    }
                }
                else if (!isFullyInit)
                {
                    throw new InvalidOperationException(String.Format("Name {0} is not fully initialized", name));
                }
                else
                {
                    var num = obj as Number;
                    if (num == null)
                    {
                        throw new InvalidOperationException(String.Format("The Name {0} is of type {1} does not resolve to a Number", name, obj.GetType()));
                    }
                    nameValueTable.Add(name, num.Value);
                }
            }
            if(unknownNames.Count == 0)
            {
                unknownNames = null;
            }

            return nameValueTable;
        }
    }

    // ----------------------------------------------------
    //  Expression parser (used by all implementations)
    //
    enum MathTokenType { NONE, Num, Name, AddOp, MultiOp, Open, Close, CompOp }
    
    class MathExpressionToken
    {
        public readonly MathTokenType TokenType;
        public readonly Double Value;
        public readonly char Operator;
        public readonly string Name;

        public MathExpressionToken(MathTokenType tokenType, Double value)
        {
            TokenType = tokenType;
            Value = value;
        }

        public MathExpressionToken(MathTokenType tokenType, string name)
        {
            TokenType = tokenType;
            Name = name;
        }

        public MathExpressionToken(MathTokenType tokenType, char op)
        {
            TokenType = tokenType;
            Operator = op;
        }

        public MathExpressionToken(MathTokenType tokenType)
        {
            TokenType = tokenType;
        }
    }

    class MathExpressionScanner
    {
        string _expression;
        int _currentOffset;

        public MathExpressionScanner(string expression)
        {
            _expression = expression;
            _currentOffset = 0;
        }

        public IEnumerable<MathExpressionToken> Scan()
        {
            MathExpressionToken returnToken = null;

            while(_currentOffset < _expression.Length && returnToken == null)
            {
                char cur = _expression[_currentOffset++];
                if (cur != ' ')
                {
                    if (Char.IsDigit(cur))
                    {
                        returnToken = GetNumber();
                    }
                    else if (Char.IsLetter(cur) || cur == '_')
                    {
                        returnToken = GetName();
                    }
                    else if (cur == '+' || cur == '-')
                    {
                        returnToken = new MathExpressionToken(MathTokenType.AddOp, cur);
                    }
                    else if (cur == '*' || cur == '/')
                    {
                        returnToken = new MathExpressionToken(MathTokenType.MultiOp, cur);
                    }
                    else if (cur == '(')
                    {
                        returnToken = new MathExpressionToken(MathTokenType.Open);
                    }
                    else if (cur == ')')
                    {
                        returnToken = new MathExpressionToken(MathTokenType.Close);
                    }
                    else if (cur == '=' || cur == '!')
                    {
                        returnToken = GetCompOp(cur);
                    }
                    else
                    {
                        throw new InvalidOperationException(String.Format("Invalid Character '{0}'", cur));
                    }
                }
                if (returnToken != null)
                {
                    yield return returnToken;
                }
                returnToken = null;
            }
        }

        private MathExpressionToken GetCompOp(char first)
        {
            if (_currentOffset < _expression.Length)
            {
                char second = _expression[_currentOffset++];
                if (second == '=')
                {
                    return new MathExpressionToken(MathTokenType.CompOp, first);
                }
                throw new InvalidOperationException(String.Format("Unknown Expression operator {0}{1}", first, second));
            }
            throw new InvalidOperationException(String.Format("Unknown Expression operator {0}", first));
        }

        private MathExpressionToken GetNumber()
        {
            int start = _currentOffset-1;
            char cur;
            while(_currentOffset < _expression.Length)
            {
                cur = _expression[_currentOffset];
                if (!(Char.IsDigit(cur) || cur == '.'))
                {
                    break;
                }
                _currentOffset += 1;
            }

            string numString = _expression.Substring(start, _currentOffset - start);
            Double value = 0;
            try
            {
                 value = Double.Parse(numString);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Double.Parse('{0}') failed", numString), ex);
            }
            return new MathExpressionToken(MathTokenType.Num, value);
        }

        private MathExpressionToken GetName()
        {
            int start = _currentOffset-1;
            char cur;
            while(_currentOffset < _expression.Length)
            {
                cur = _expression[_currentOffset];
                if(!(Char.IsLetter(cur) || Char.IsDigit(cur) || cur == '_'))
                {
                    break;
                }
                _currentOffset += 1;
            }
            string name = _expression.Substring(start, _currentOffset - start);
            return new MathExpressionToken(MathTokenType.Name, name);
        }
    }
    class MathExpressionParser
    {
        MathExpressionScanner _scanner;
        IEnumerator<MathExpressionToken> _tokens;
        private MathExpressionToken CurrentToken { get; set; }
        private Dictionary<string, Double> _nameValueTable;

        public MathExpressionParser(Dictionary<string, Double> nameValueTable)
        {
            _nameValueTable = nameValueTable;
        }

        // Comp ::= Exp
        // Comp ::= Exp == Exp

        // Exp ::= Term
        // Exp ::= Term (+/-) Exp

        // Term ::= Fac
        // Term ::= Fac (*|/) Term
        // Fac ::= Number

        // Fac ::= ( Comp )

        public Double Parse(string expression)
        {
            _scanner = new MathExpressionScanner(expression);
            _tokens = _scanner.Scan().GetEnumerator();
            _tokens.MoveNext();
            CurrentToken = _tokens.Current;
            Double value = P_Comparison();
            return value;
        }

        // Comp ::= Exp
        // Comp ::= Exp = Exp

        private Double P_Comparison()
        {
            Double value = 0;
            switch (CurrentToken.TokenType)
            {
                case MathTokenType.Open:
                case MathTokenType.Num:
                case MathTokenType.Name:
                    value = P_Expression();
                    if (CurrentToken.TokenType == MathTokenType.CompOp)
                    {
                        char op = CurrentToken.Operator;
                        Consume_EqualsOp();
                        Double temp = P_Expression();
                        if (op == '=')
                        {
                            if (value == temp)
                            {
                                return 1;
                            }
                            throw new InvalidOperationException(String.Format("ERROR {0} != {1}", value, temp));
                        }
                        else if (op == '!')
                        {
                            if (value != temp)
                            {
                                return 1;
                            }
                            throw new InvalidOperationException(String.Format("ERROR {0} != {1}", value, temp));
                        }
                    }
                    break;

                default:
                    string err = String.Format("Parse error in P_Term unexpected '{0}'", CurrentToken.TokenType.ToString());
                    throw new InvalidOperationException(err);
            }
            return value;
        }

        // Exp ::= Term
        // Exp ::= Term (+/-) Exp

        private Double P_Expression()
        {
            Double value = 0;
            switch (CurrentToken.TokenType)
            {
                case MathTokenType.Open:
                case MathTokenType.Num:
                case MathTokenType.Name:
                    value = P_Term();
                    if (CurrentToken.TokenType == MathTokenType.AddOp)
                    {
                        char op = CurrentToken.Operator;
                        Consume_AddOp();
                        Double temp = P_Expression();
                        if (op == '+')
                        {
                            value += temp;
                        }
                        else if (op == '-')
                        {
                            value -= temp;
                        }
                    }
                    break;

                default:
                    string err = String.Format("Parse error in P_Term unexpected '{0}'", CurrentToken.TokenType.ToString());
                    throw new InvalidOperationException(err);
            }
            return value;
        }

        // Term ::= Fac
        // Term ::= Fac (*|/) Term

        private Double P_Term()
        {
            Double value = 0;
            switch (CurrentToken.TokenType)
            {
                case MathTokenType.Open:
                case MathTokenType.Num:
                case MathTokenType.Name:
                    value = P_Factor();
                    if (CurrentToken.TokenType == MathTokenType.MultiOp)
                    {
                        char op = CurrentToken.Operator;
                        Consume_MultiOp();
                        Double temp = P_Term();
                        if (op == '*')
                        {
                            value *= temp;
                        }
                        else if (op == '/')
                        {
                            value /= temp;
                        }
                    }
                    break;


                default:
                    string err = String.Format("Parse error in P_Term unexpected '{0}'", CurrentToken.TokenType.ToString());
                    throw new InvalidOperationException(err);
            }
            return value;
        }

        // Fac ::= Number
        // Fac ::= Name
        // Fac ::= ( Exp )

        private Double P_Factor()
        {
            Double value = 0;
            switch (CurrentToken.TokenType)
            {
                case MathTokenType.Num:
                    value = Consume_Number();
                    break;

                case MathTokenType.Name:
                    value = Consume_Name();
                    break;

                case MathTokenType.Open:
                    Consume_OpenParen();
                    value = P_Expression();
                    Consume_CloseParen();
                    break;

                default:
                    string err = String.Format("Parse error in P_Term unexpected '{0}'", CurrentToken.TokenType.ToString());
                    throw new InvalidOperationException(err);
            }
            return value;
        }

        private void Consume_EqualsOp()
        {
            _tokens.MoveNext();
            CurrentToken = _tokens.Current;
        }

        private void Consume_AddOp()
        {
            _tokens.MoveNext();
            CurrentToken = _tokens.Current;
        }

        private void Consume_MultiOp()
        {
            _tokens.MoveNext();
            CurrentToken = _tokens.Current;
        }

        private void Consume_OpenParen()
        {
            _tokens.MoveNext();
            CurrentToken = _tokens.Current;
        }

        private void Consume_CloseParen()
        {
            _tokens.MoveNext();
            CurrentToken = _tokens.Current;
        }

        private Double Consume_Number()
        {
            Double value = CurrentToken.Value;
            _tokens.MoveNext();
            CurrentToken = _tokens.Current;
            return value;
        }

        private Double Consume_Name()
        {
            string name = CurrentToken.Name;
            _tokens.MoveNext();
            CurrentToken = _tokens.Current;
            Double val = _nameValueTable[name];
            return val;
        }

    }

}
