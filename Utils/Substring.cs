namespace Depravity
{
    public struct Substring
    {
        private readonly string parent;
        public int from, to;

        public Substring(string parent, int from, int to)
        {
            this.parent = parent;
            this.from = from;
            this.to = to;
        }

        public Substring(string parent)
        {
            this.parent = parent;
            from = 0;
            to = parent.Length;
        }

        public Substring(Substring substring)
        {
            parent = substring.parent;
            from = substring.from;
            to = substring.to;
        }

        public Substring Range(int from, int to)
        {
            return new Substring(parent, from, to);
        }

        public string Parent
        {
            get
            {
                return parent;
            }
        }

        public int Length
        {
            get
            {
                return to - from;
            }
        }

        public char this[int idx]
        {
            get
            {
                return parent[from + idx];
            }
        }

        public char Last
        {
            get
            {
                return parent[to - 1];
            }
        }

        public int Find(char c)
        {
            for (int i = from; i < to; ++i)
            {
                if (parent[i] == c)
                {
                    return i;
                }
            }
            return to;
        }

        public bool Find(Substring substring, out Substring result)
        {
            result = new Substring(parent, from, from + substring.Length);
            while (result.to <= to)
            {
                if (result == substring)
                {
                    return true;
                }
                ++result.from;
                ++result.to;
            }
            return false;
        }

        public bool Find(string str, out Substring result)
        {
            return Find(new Substring(str), out result);
        }

        public static bool operator ==(Substring substring, string comp)
        {
            if (comp.Length != substring.Length)
            {
                return false;
            }
            for (int i = substring.from, j = 0; i < substring.to; ++i, ++j)
            {
                if (substring.parent[i] != comp[j])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(Substring substring, string comp)
        {
            return !(comp == substring);
        }

        public static bool operator ==(string comp, Substring xmlString)
        {
            return xmlString == comp;
        }

        public static bool operator !=(string comp, Substring xmlString)
        {
            return xmlString != comp;
        }

        public override string ToString()
        {
            return parent.Substring(from, Length);
        }

        public override bool Equals(object obj)
        {
            return (obj is Substring @ss && @ss == this) || (obj is string @string && this == @string);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            for (int i = from; i != to; hash = hash * 37 + parent[i++]) ;
            return hash;
        }

        public static bool operator ==(Substring a, Substring b)
        {
            int len = a.Length;
            if (len != b.Length)
            {
                return false;
            }
            for (int i = 0; i != len; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(Substring a, Substring b)
        {
            return !(a == b);
        }

        public void LeftTrim(char c = ' ')
        {
            for (; from != to && parent[from] <= c; ++from) ;
        }

        public void RightTrim(char c = ' ')
        {
            for (int i = to - 1; to > from && parent[i] <= c; --i)
            {
                to = i;
            }
        }

        public void Trim(char c = ' ')
        {
            LeftTrim(c);
            RightTrim(c);
        }
    }
}
