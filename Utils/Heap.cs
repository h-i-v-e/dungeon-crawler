using System.Collections.Generic;

namespace Depravity
{
    public class Heap<T>
    {
        public delegate bool Less(T a, T b);

        private readonly Less less;
        private readonly List<T> items = new List<T>();

        public Heap(Less less)
        {
            this.less = less;
        }

        public T Pop()
        {
            T top = items[0];
            int len = items.Count;
            if (len == 1)
            {
                items.Clear();
                return top;
            }
            int last = len - 1;
            items[0] = items[last];
            items.RemoveAt(len - 1);
            BubbleDown();
            return top;
        }

        public T Peek()
        {
            return items[0];
        }

        private void BubbleUp()
        {
            int offset = items.Count - 1;
            T item = items[offset];
            while (offset > 0)
            {
                int parent = (offset - 1) >> 1;
                T parentItem = items[parent];
                if (less(parentItem, item))
                {
                    break;
                }
                items[offset] = parentItem;
                offset = parent;
            }
            items[offset] = item;
        }

        private void BubbleDown()
        {
            int swapTo = 0;
            T item = items[0];
            for (int len = items.Count; swapTo < len;)
            {
                int depth = swapTo << 1;
                int left = depth + 1, right = depth + 2;
                if (left >= len)
                {
                    break;
                }
                int swapWith = right >= len || less(items[left], items[right]) ? left : right;
                if (less(items[swapWith], item))
                {
                    items[swapTo] = items[swapWith];
                    swapTo = swapWith;
                }
                else
                {
                    break;
                }
            }
            items[swapTo] = item;
        }

        public void Push(T item)
        {
            items.Add(item);
            int len = items.Count;
            if (len == 1)
            {
                return;
            }
            BubbleUp();
        }

        public bool IsEmpty
        {
            get
            {
                return items.Count == 0;
            }
        }

        public void Clear()
        {
            items.Clear();
        }
    }
}
