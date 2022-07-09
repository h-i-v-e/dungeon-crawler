using UnityEngine;
using System;

namespace Depravity
{

    public class DummyConversationInterfaceProvider : ConversationInterfaceProvider
    {
        private static readonly string[] spliters =
        {
            Environment.NewLine, "\n"
        };
        private ConversationReplySelected selected;

        private static void Log(string text)
        {
            foreach (var str in text.Split(spliters, StringSplitOptions.RemoveEmptyEntries))
            {
                Debug.Log(str);
            }
            Debug.Log(Environment.NewLine);
        }

        public override void SetCaption(string text, ConversationReplyTextProvider replies = null, ConversationReplySelected selected = null)
        {
            if (text == null)
            {
                this.selected = null;
                return;
            }
            this.selected = selected;
            Log(text);
            int i = 0;
            for (string reply = replies(i); reply != null; reply = replies(++i))
            {
                Log(reply);
            }
        }

        public void Update()
        {
            if (selected == null)
            {
                return;
            }
            if (Input.anyKeyDown)
            {
                if (int.TryParse(Input.inputString, out int selected))
                {
                    this.selected(selected);
                }
            }
        }
    }
}