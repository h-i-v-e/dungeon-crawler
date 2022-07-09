using System.Collections.Generic;

namespace Depravity
{
    public class XmlParser
    {
        private const string CDATA_BEGIN = "<![CDATA[", CDATA_END = "]]>";

        public delegate void OpenElement(Substring name);

        public delegate void CloseElement(Substring name, Substring body);

        public delegate void AddAttribute(Substring name, Substring value);

        private string content;
        private readonly Stack<int> openTags = new Stack<int>();

        public OpenElement OnOpenElement { set; private get; }
        public CloseElement OnCloseElement { set; private get; }
        public AddAttribute OnAddAttribute { set; private get; }

        public static Substring TrimTagBody(Substring body)
        {
            var output = new Substring(body);
            output.Trim();
            if (output.Find(CDATA_BEGIN, out var ss))
            {
                output.from = ss.to;
                if (output.Find(CDATA_END, out ss))
                {
                    output.to = ss.from;
                    output.Trim();
                }
                else
                {
                    output.from = output.to;
                }
            }
            return output;
        }

        private int Find(int offset, char c)
        {
            return new Substring(content, offset, content.Length).Find(c);
        }

        private bool HasSlash(int offset, out int slashPos)
        {
            slashPos = offset;
            while (slashPos >= 0)
            {
                char d = content[slashPos];
                if (d == '/')
                {
                    return true;
                }
                if (d > ' ')
                {
                    return false;
                }
                --slashPos;
            }
            return false;
        }

        private int FindWhiteSpace(int offset, int end)
        {
            while (offset != end)
            {
                if (content[offset] <= ' ')
                {
                    return offset;
                }
                ++offset;
            }
            return end;
        }

        private Substring ExtractTagName(int offset, int end)
        {
            return new Substring(content, offset, FindWhiteSpace(offset, end));
        }

        private int OpenTag(int offset, int end)
        {
            var name = ExtractTagName(offset, end);
            OnOpenElement?.Invoke(name);
            return name.to;
        }

        private int SkipWhiteSpace(int offset, int end)
        {
            while (offset != end)
            {
                if (content[offset] > ' ')
                {
                    return offset;
                }
                ++offset;
            }
            return end;
        }

        private int ExtractAttribute(int offset, int end)
        {
            offset = SkipWhiteSpace(offset, end);
            for (int i = offset; i < end; ++i)
            {
                char c = content[i];
                if (c <= ' ')
                {
                    OnAddAttribute?.Invoke(new Substring(content, offset, i), new Substring(content, i, i));
                    return i + 1;
                }
                if (c == '=')
                {
                    int start = Find(i + 1, '"');
                    int stop = Find(++start, '"');
                    OnAddAttribute?.Invoke(new Substring(content, offset, i), new Substring(content, start, stop));
                    return stop + 1;
                }
            }
            return end;
        }

        private int ReadAttributes(int offset, int end)
        {
            for (offset = ExtractAttribute(offset, end); offset < end; offset = ExtractAttribute(offset, end)) ;
            return offset;
        }

        private int ReadTag(int offset, int end)
        {
            return ReadAttributes(OpenTag(offset, end), end);
        }

        private bool IsCDATA(ref int i)
        {
            int end = i + CDATA_BEGIN.Length;
            if (end < content.Length && CDATA_BEGIN == new Substring(content, i, end))
            {
                i = content.IndexOf(CDATA_END, end) + CDATA_END.Length;
                return true;
            }
            return false;
        }

        public void Parse(string xml)
        {
            content = xml;
            int len = xml.Length;
            for (int i = Find(0, '<'); i < len; i = Find(i, '<'))
            {
                if (IsCDATA(ref i))
                {
                    continue;
                }
                int blockEnd = i++;
                int end = Find(i, '>');
                if (content[i] == '/')
                {
                    ++i;
                    OnCloseElement?.Invoke(ExtractTagName(i, end), new Substring(content, openTags.Pop(), blockEnd));
                    i = end + 1;
                    continue;
                }
                if (HasSlash(end - 1, out var slashPos))
                {
                    var name = ExtractTagName(i, slashPos);
                    OnOpenElement?.Invoke(name);
                    ReadAttributes(name.to, slashPos);
                    OnCloseElement?.Invoke(name, new Substring(content, slashPos, slashPos));
                    continue;
                }
                openTags.Push(end + 1);
                ReadTag(i, end);
            }
        }
    }
}