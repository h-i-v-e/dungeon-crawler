using System.Collections.Generic;
using System.Text;

namespace Depravity
{

    public class SloppyScriptParser
    {
        private readonly Dictionary<Substring, Action> actions = new Dictionary<Substring, Action>();
        private readonly ResourcePool<StringBuilder> stringBuilders = new ResourcePool<StringBuilder>(() => new StringBuilder());

        private StringBuilder buffer;

        public delegate string Action(string content);

        public void SetAction(string key, Action action)
        {
            actions.Add(new Substring(key), action);
        }

        private static void Close(ref Substring substring)
        {
            bool escaped = false;
            int offset = substring.from, end = substring.to;
            for (int depth = 1; offset < end && depth != 0; ++offset)
            {
                char c = substring[offset];
                switch (c)
                {
                    case '#':
                        if (escaped)
                        {
                            escaped = false;
                        }
                        else
                        {
                            escaped = true;
                        }
                        continue;
                    case '{':
                        if (!escaped)
                        {
                            ++depth;
                        }
                        else
                        {
                            escaped = false;
                        }
                        continue;
                    case '}':
                        if (!escaped)
                        {
                            --depth;
                        }
                        else
                        {
                            escaped = false;
                        }
                        continue;
                }
            }
            substring.to = offset;
        }

        private void Call(Substring action, StringBuilder buffer, ref Substring substring)
        {
            if (actions.TryGetValue(action, out var func))
            {
                Close(ref substring);
                buffer.Append(func(Parse(substring)));
            }
            buffer.Append(substring.Parent, substring.from, substring.Length);
        }

        public string Parse(Substring substring)
        {
            buffer = stringBuilders.Allocate();
            for (int i = substring.Find('#'); i != -1; i = substring.Find('#'))
            {
                int next = i + 1;
                if (next < substring.to)
                {
                    char c = substring[next];
                    switch (c)
                    {
                        case '#':
                        case '{':
                        case '}':
                            buffer.Append(c);
                            substring.from = next;
                            continue;
                    }
                }
                buffer.Append(substring.Parent, substring.from, i - substring.from);
                substring.from = i + 1;
                int bracket = substring.Find('{');
                if (bracket != -1)
                {
                    var action = substring.Range(substring.from, bracket);
                    var body = substring.Range(bracket + 1, substring.to);
                    Call(action, buffer, ref body);
                    substring.from = body.to + 1;
                }
            }
            var output = buffer.ToString();
            buffer.Clear();
            stringBuilders.Release(buffer);
            return output;
        }
    }
}
