﻿using System.Collections.Generic;
using System.Dynamic;
using NUnit.Framework;

namespace JsonParser.Tests
{
    public class JsonParseTests
    {
        [Test]
        public void TrueReturnsTrue()
        {
            var json = "true";

            var result = Json.Parse(json);

            Assert.That(result, Is.True);
        }

        [Test]
        public void FalseReturnsFalse()
        {
            var json = "false";

            var result = Json.Parse(json);

            Assert.That(result, Is.False);
        }

        [Test]
        public void NullReturnsNull()
        {
            var json = "null";

            var result = Json.Parse(json);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void StringWithNoEscapeSequencesReturnsString()
        {
            var json = @"""Hello, world!""";

            var result = Json.Parse(json);

            Assert.That(result, Is.EqualTo("Hello, world!"));
        }

        [Test]
        public void StringWithEscapedQuoteReturnsString()
        {
            var json = @"""\""Ow.\"" -my pancreas""";

            var result = Json.Parse(json);

            Assert.That(result, Is.EqualTo(@"""Ow."" -my pancreas"));
        }

        [Test]
        public void StringWithEscapedBackslashReturnsString()
        {
            var json = @"""c:\\temp\\file.txt""";

            var result = Json.Parse(json);

            Assert.That(result, Is.EqualTo(@"c:\temp\file.txt"));
        }

        [Test]
        public void NumberWithoutDecimalPlaceReturnsInt()
        {
            var json = @"123";

            var result = Json.Parse(json);

            Assert.That(result, Is.EqualTo(123));
            Assert.That(result, Is.InstanceOf<int>());
        }

        [Test]
        public void NegativeNumberWithoutDecimalPlaceReturnsInt()
        {
            var json = @"-123";

            var result = Json.Parse(json);

            Assert.That(result, Is.EqualTo(-123));
            Assert.That(result, Is.InstanceOf<int>());
        }

        [Test]
        public void NumberWithDecimalPlaceReturnsDouble()
        {
            var json = @"123.45";

            var result = Json.Parse(json);

            Assert.That(result, Is.EqualTo(123.45));
            Assert.That(result, Is.InstanceOf<double>());
        }

        [Test]
        public void NegativeNumberWithDecimalPlaceReturnsDouble()
        {
            var json = @"-123.45";

            var result = Json.Parse(json);

            Assert.That(result, Is.EqualTo(-123.45));
            Assert.That(result, Is.InstanceOf<double>());
        }

        [Test]
        public void AnEmptyJsonObjectReturnsExpandoObject()
        {
            var json = @"{}";

            var result = Json.Parse(json);

            Assert.That(result, Is.InstanceOf<ExpandoObject>());
            var dictionary = (IDictionary<string, object>)result;
            Assert.That(dictionary.Count, Is.EqualTo(0));
        }

        [Test]
        public void AnJsonObjectWithOneMemberReturnsExpandoObject()
        {
            var json = @"{""foo"":123}";

            var result = Json.Parse(json);

            Assert.That(result, Is.InstanceOf<ExpandoObject>());
            var dictionary = (IDictionary<string, object>)result;
            Assert.That(dictionary.Count, Is.EqualTo(1));
            Assert.That(result.foo, Is.EqualTo(123));
        }

        [Test, Ignore]
        public void AnJsonObjectWithMultipleMemberReturnsExpandoObject()
        {
            var json = @"{""foo"":123,""bar"":true,""baz"":false}";

            var result = Json.Parse(json);

            Assert.That(result, Is.InstanceOf<ExpandoObject>());
            var dictionary = (IDictionary<string, object>)result;
            Assert.That(dictionary.Count, Is.EqualTo(3));
            Assert.That(result.foo, Is.EqualTo(123));
            Assert.That(result.bar, Is.EqualTo(true));
            Assert.That(result.baz, Is.EqualTo(false));
        }
    }
}