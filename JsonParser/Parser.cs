﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Sprache;

namespace JsonParser
{
    internal static class Parser
    {
        public static Func<string, object> GetJsonParser()
        {
            var mainParser = new MainParser();

            var literalParser = GetLiteralParser();
            var stringParser = GetStringParser();
            var numberParser = GetNumberParser();
            var objectParser = GetObjectParser(stringParser, mainParser);
            var arrayParser = GetArrayParser(mainParser);

            mainParser.Value = literalParser
                .Or(stringParser)
                .Or(numberParser)
                .Or(objectParser)
                .Or(arrayParser)
                .Token();

            return mainParser.Value.Parse;
        }

        private static Parser<object> GetArrayParser(MainParser mainParser)
        {
            var remainderParser =
                from comma in Parse.Char(',').Token()
                from item in Parse.Ref(() => mainParser.Value)
                select item;

            return
                from openBracket in Parse.Char('[').Token()
                from item in Parse.Ref(() => mainParser.Value).Optional()
                from rest in remainderParser.Many()
                from closeBracket in Parse.Char(']').Token()
                select GetObjectArray(item, rest);
        }

        private static object[] GetObjectArray(IOption<object> item, IEnumerable<object> rest)
        {
            if (!item.IsDefined)
            {
                return new object[0];
            }

            return new[] { item.Get() }.Concat(rest).ToArray();
        }

        private static Parser<object> GetObjectParser(
            Parser<string> stringParser,
            MainParser mainParser)
        {
            var memberParser =
                from name in stringParser
                from colon in Parse.Char(':').Token()
                from value in Parse.Ref(() => mainParser.Value)
                select new Member { Name = name, Value = value };

            var remainderParser =
                from comma in Parse.Char(',').Token()
                from member in memberParser
                select member;

            return
                from openBrace in Parse.Char('{').Token()
                from member in memberParser.Optional()
                from rest in remainderParser.Many()
                from closeBrace in Parse.Char('}').Token()
                select GetExpandoObject(member, rest);
        }

        private static ExpandoObject GetExpandoObject(
            IOption<Member> member,
            IEnumerable<Member> rest)
        {
            var expandoObject = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expandoObject;

            if (member.IsDefined)
            {
                dictionary.Add(member.Get().Name, member.Get().Value);

                foreach (var m in rest)
                {
                    dictionary.Add(m.Name, m.Value);
                }
            }

            return expandoObject;
        }

        private class MainParser
        {
            public Parser<object> Value;
        }

        private class Member
        {
            public string Name;
            public object Value;
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