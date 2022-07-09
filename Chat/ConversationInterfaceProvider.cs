using UnityEngine;

namespace Depravity
{
    public delegate string ConversationReplyTextProvider(int offset);
    public delegate void ConversationReplySelected(int offset);

    public abstract class ConversationInterfaceProvider : MonoBehaviour
    {
        public abstract void SetCaption(string text, ConversationReplyTextProvider replies = null, ConversationReplySelected selected = null);
    }
}