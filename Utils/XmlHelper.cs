using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Depravity
{
    [AttributeUsage(AttributeTargets.Class)]
    public class XmlTag : Attribute
    {
        public readonly string name;

        public XmlTag(string name)
        {
            this.name = name;
        }
    }

    public interface IXmlTagImpl
    {
        void Close(XmlTagStack stack, Substring body);
    }

    public abstract class BodylessXmlTag : IXmlTagImpl
    {
        public void Close(XmlTagStack stack, Substring body) { }
    }

    public struct XmlTagStack
    {
        public Stack<IXmlTagImpl> stack;

        public XmlTagStack(Stack<IXmlTagImpl> stack)
        {
            this.stack = stack;
        }

#if UNITY_EDITOR
        private static string GetName(Type type)
        {
            return ((XmlTag)Attribute.GetCustomAttribute(type, typeof(XmlTag))).name;
        }

        private void CheckStack(Type t)
        {
            Debug.Assert(
                stack.Count != 0,
                $"No parent element. Expected {GetName(t)}"
            );
            Debug.Assert(
                stack.Peek().GetType() == t,
                $"Expected {GetName(t)} but got {GetName(stack.Peek().GetType())}"
            );
        }

        public T GetParent<T>() where T : IXmlTagImpl
        {
            CheckStack(typeof(T));
            return (T)stack.Peek();
        }
#else
        public T GetParent<T>() where T : IXmlTagImpl
        {
            return (T)stack.Peek();
        }
#endif
    }


    public class XmlHelper
    {
        private static readonly Type[] emptyTypeArray = new Type[0];
        private static readonly object[] emptyObjectArray = new object[0];
#if UNITY_EDITOR
        private static readonly Type tagType = typeof(IXmlTagImpl);
#endif

        private readonly Dictionary<Substring, ConstructorInfo> tagTypes;
        private readonly Stack<IXmlTagImpl> stack = new Stack<IXmlTagImpl>();
        private readonly Stack<bool> valid = new Stack<bool>();
        private readonly XmlParser parser;

        private IXmlTagImpl root;

        public XmlHelper(params Type[] tags)
        {
            int len = tags.Length;
            tagTypes = new Dictionary<Substring, ConstructorInfo>(len);
            for (int i = 0; i != len; AddTag(tags[i++])) ;
            parser = new XmlParser
            {
                OnOpenElement = OpenElement,
                OnCloseElement = CloseElement,
                OnAddAttribute = AddAttribute
            };
        }

        private void AddTag(Type tag)
        {
#if UNITY_EDITOR
            Debug.Assert(tagType.IsAssignableFrom(tag), "XmlHelper can only parse tags implementing IXmlTagImpl");
#endif
            var constr = tag.GetConstructor(emptyTypeArray);
#if UNITY_EDITOR
            Debug.Assert(constr != null, "XmlHelper can only parse tags with an empty constructor");
#endif
            tagTypes[new Substring(GetName(tag))] = constr;
        }

        private static string GetName(Type tag)
        {
            var attr = (XmlTag)Attribute.GetCustomAttribute(tag, typeof(XmlTag));
            return attr == null ? tag.Name : attr.name;
        }

        private void AddAttribute(Substring name, Substring value)
        {
            if (!valid.Peek())
            {
                return;
            }
            var tag = stack.Peek();
            var type = tag.GetType();
            var info = type.GetField(name.ToString());
            if (info != null && info.FieldType == typeof(string))
            {
                info.SetValue(tag, value.ToString());
            }
        }

        private void OpenElement(Substring name)
        {
            if (tagTypes.TryGetValue(name, out var constr))
            {
                stack.Push((IXmlTagImpl)constr.Invoke(emptyObjectArray));
                valid.Push(true);
            }
            else
            {
                valid.Push(false);
            }
        }

        private void CloseElement(Substring name, Substring body)
        {
            valid.Pop();
            if (!tagTypes.ContainsKey(name))
            {
                return;
            }
            root = stack.Pop();
            root.Close(new XmlTagStack(stack), body);
        }

        public T Parse<T>(string xml) where T : IXmlTagImpl
        {
            parser.Parse(xml);
            return (T)root;
        }
    }
}
