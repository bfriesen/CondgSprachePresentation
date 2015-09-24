﻿using System;
using System.Dynamic;
using Sprache;

namespace JsonParser
{
    internal static class Parser
    {
        public static Func<string, object> GetJsonParser()
        {
            var literalParser = GetLiteralParser();
            var stringParser = GetStringParser();
            var numberParser = GetNumberParser();
            var objectParser = GetObjectParser();

            var mainParser = literalParser
                .Or(stringParser)
                .Or(numberParser)
                .Or(objectParser);

            return mainParser.Parse;
        }

        private static Parser<object> GetObjectParser()
        {
            return
                from openBrace in Parse.Char('{')
                from closeBrace in Parse.Char('}')
                select new ExpandoObject();
        }

        private static Parser<object> GetNumberParser()
        {
            var doubleParser =
                from negative in Parse.Char('-').Optional()
                from digits in Parse.Digit.AtLeastOnce().Text()
                from dot in Parse.Char('.')
                from decimalPlaces in Parse.Digit.AtLeastOnce().Text()
                select (object)(double.Parse(digits + dot + decimalPlaces) * (negative.IsDefined ? -1 : 1));

            var intParser =
                from negative in Parse.Char('-').Optional()
                from digits in Parse.Digit.AtLeastOnce().Text()
                select (object)(int.Parse(digits) * (negative.IsDefined ? -1 : 1));

            return doubleParser.Or(intParser);
        }

        private static Parser<string> GetStringParser()
        {
            var escapedQuoteParser =
                from backslash in Parse.Char('\\')
                from quote in Parse.Char('"')
                select quote;

            var escapedBackslashParser =
                from firstBackslash in Parse.Char('\\')
                from secondBackslash in Parse.Char('\\')
                select secondBackslash;

            return
                from openQuote in Parse.Char('"')
                from value in escapedBackslashParser
                    .Or(escapedQuoteParser)
                    .Or(Parse.CharExcept('"'))
                    .Many().Text()
                from closeQuote in Parse.Char('"')
                select value;
        }

        private static Parser<object> GetLiteralParser()
        {
            var trueParser =
                from t in Parse.String("true")
                select (object)true;

            var falseParser =
                from f in Parse.String("false")
                select (object)false;

            var nullParser =
                from n in Parse.String("null")
                select (object)null;

            return trueParser.Or(falseParser).Or(nullParser);
        }
    }
}