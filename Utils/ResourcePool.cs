using System.Collections.Generic;

namespace Depravity
{
    public class ResourcePool<T>
    {
        public delegate T Builder();

        private readonly Stack<T> free = new Stack<T>();
        private readonly Builder builder;

        public ResourcePool(Builder builder){
            this.builder = builder;
        }

        public T Allocate()
        {
            return free.Count == 0 ? builder.Invoke() : free.Pop();
        }

        public void Release(T t)
        {
            free.Push(t);
        }
    }
}