using UnityEngine;

namespace Depravity
{
    public class ConversationInterface : MonoBehaviour
    {
        [SerializeField]
        private ConversationInterfaceProvider conversationInterfaceProvider;

        private static ConversationInterface instance;

        public static void SetCaption(string text, ConversationReplyTextProvider replies = null, ConversationReplySelected selected = null)
        {
            Debug.Assert(instance != null, "No ConversationInterface present in scene");
            instance.conversationInterfaceProvider.SetCaption(text, replies, selected);
        }

        public void Awake()
        {
            instance = this;
        }
    }
}