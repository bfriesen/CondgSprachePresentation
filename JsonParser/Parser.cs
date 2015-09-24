using System;
using Sprache;

namespace JsonParser
{
    internal static class Parser
    {
        public static Func<string, object> GetJsonParser()
        {
            var literalParser = GetLiteralParser();
            var stringParser = GetStringParser();

            var mainParser = literalParser.Or(stringParser);

            return mainParser.Parse;
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
                    .Or(Parse.CharExcept('"')).Many().Text()
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