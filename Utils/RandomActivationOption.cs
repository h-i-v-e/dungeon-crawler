using System;
using System.ComponentModel;

namespace Depravity
{
    public interface RandomActivationOption
    {
        int GetChances();
    }

    public static class RandomActivationOptions
    {
        public static T SelectFrom<T>(T[] options) where T : RandomActivationOption
        {
            int num = 0, opts = options.Length, idx = 0;
            for (int i = 0; i != opts; num += options[i++].GetChances()) ;
            num = UnityEngine.Random.Range(0, num);
            for (int i = 0; i != opts; ++i)
            {
                var opt = options[i];
                int next = idx + opt.GetChances();
                if (num < next)
                {
                    return opt;
                }
                idx = next;
            }
            return default;
        }
    }
}
