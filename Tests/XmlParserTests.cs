using NUnit.Framework;
using Depravity;
using System.Collections.Generic;

public class Test
{
    public const string
        ATTR_NAME = "type",
        ATTR_VALUE = "test attribute",
        BODIED_TAG = "WithBody",
        EMPTY_TAG = "EmptyTag",
        IGNORE_TAG = "IShouldBeIgnored";
    public static string body = $"werty terty\nWiddle didley\n do<{EMPTY_TAG}/>Hello,\r\nHow may I help you?<![CDATA[\n<{IGNORE_TAG}>]]>";
    public static string withCDATA = $"<{BODIED_TAG}>\n\t<![CDATA[\n\t\t{ATTR_VALUE}\n\t]]>\n</{BODIED_TAG}>";
    public static string xml = $"<{BODIED_TAG} {ATTR_NAME}=\"{ATTR_VALUE}\">{body}</{BODIED_TAG}>";

    private readonly List<Substring>
        opens = new List<Substring> (),
        closes = new List<Substring>(),
        bodies = new List<Substring>();

    public void Open(Substring name)
    {
        Assert.IsFalse(name == IGNORE_TAG, "CDATA should not be parsed");
        opens.Add(name);
    }

    public void Close(Substring name, Substring body)
    {
        closes.Add(name);
        bodies.Add(body);
    }

    public void Attribute(Substring key, Substring value)
    {
        Assert.IsTrue(key == ATTR_NAME, $"{key} should be {ATTR_NAME}");
        Assert.IsTrue(value == ATTR_VALUE, $"{value} should be {ATTR_VALUE}");
    }

    private static void Check(List<Substring> list, int i, string value)
    {
        Assert.IsTrue(list[i] == value, $"{list[i]} should be {value}");
    }

    public void CheckOffset(int i, string tag, string body)
    {
        Check(opens, i, tag);
        int back = closes.Count - (i + 1);
        Check(closes, back, tag);
        Check(bodies, back, body);
    }

    public static void CheckCDATA(Substring _, Substring body)
    {
        body.Trim();
        Assert.IsTrue(body == ATTR_VALUE, $"\"{body}\" != \"{ATTR_VALUE}\"");
    }
}

public class XmlParserTests
{
    [Test]
    public void ParseTest()
    {
        var test = new Test();
        new XmlParser {
            OnOpenElement = test.Open,
            OnCloseElement = test.Close,
            OnAddAttribute = test.Attribute,
        }.Parse(Test.xml);
        test.CheckOffset(0, Test.BODIED_TAG, Test.body);
        test.CheckOffset(1, Test.EMPTY_TAG, "");
    }

    [Test]
    public void TrimTest()
    {
        new XmlParser
        {
            OnCloseElement = Test.CheckCDATA
        }.Parse(Test.withCDATA);
    }
}
