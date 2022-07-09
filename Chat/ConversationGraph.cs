using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Depravity
{
    public class ConversationGraph : MonoBehaviour
    {
        private static readonly Regex whitespaceCleaner = new Regex("\\s+", RegexOptions.Compiled);
        private static readonly Regex lineBreak = new Regex("<br\\s*/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string Purge(Substring xmlString)
        {
            return lineBreak.Replace(whitespaceCleaner.Replace(XmlParser.TrimTagBody(xmlString).ToString(), " "), System.Environment.NewLine);
        }

        [XmlTag("option")]
        public class Option : IXmlTagImpl
        {
            public string label, reference;

            public Statement node;

            public void Close(XmlTagStack stack, Substring body)
            {
                if (label == null)
                {
                    label = Purge(body);
                }
                stack.GetParent<Replies>().options.Add(this);
            }
        }

        [XmlTag("statement")]
        public class Statement : IXmlTagImpl
        {
            public string dialogue, action, id, forward;

            public Option[] replies;

            public void Close(XmlTagStack stack, Substring body)
            {
                stack.GetParent<ConversationTag>().statements.Add(this);
            }
        }

        [XmlTag("replies")]
        public class Replies : IXmlTagImpl
        {
            public readonly List<Option> options = new List<Option>();

            public void Close(XmlTagStack stack, Substring body)
            {
                stack.GetParent<Statement>().replies = options.ToArray();
            }
        }

        [XmlTag("dialogue")]
        public class Dialogue : IXmlTagImpl
        {
            public void Close(XmlTagStack stack, Substring body)
            {
                stack.GetParent<Statement>().dialogue = Purge(body);
            }
        }

        [XmlTag("conversation")]
        public class ConversationTag : BodylessXmlTag
        {
            public readonly List<Statement> statements = new List<Statement>();
            public string onenter, onexit;

            private void BindOptions(Dictionary<string, Statement> dict, Option[] options)
            {
                for (int i = 0, j = options.Length; i != j; ++i)
                {
                    var option = options[i];
                    Debug.Assert(
                        dict.TryGetValue(option.reference, out option.node),
                        $"No statement with id {option.reference} exists"
                    );
                }
            }

            public Dictionary<string, Statement> ResolveReferences()
            {
                int len = statements.Count;
                var dict = new Dictionary<string, Statement>(len);
                for (int i = 0; i != len; ++i)
                {
                    var state = statements[i];
                    dict[state.id] = state;
                }
                for (int i = 0; i != len; ++i)
                {
                    var replies = statements[i].replies;
                    if (replies != null)
                    {
                        BindOptions(dict, replies);
                    }
                }
                return dict;
            }
        }

        private struct Conversation
        {
            public string onEnter, onExit;
            public Statement root;
            public Dictionary<string, Statement> nodes;
        }

        private static readonly XmlHelper xmlHelper = new XmlHelper(
            typeof(Option), typeof(Statement), typeof(Replies),
            typeof(Dialogue), typeof(ConversationTag)
        );
        private static readonly Dictionary<string, Conversation> cache = new Dictionary<string, Conversation>();

        private static bool Parse(TextAsset xmlFile, out Conversation conversation)
        {
            var conv = xmlHelper.Parse<ConversationTag>(xmlFile.text);
            conversation.nodes = conv.ResolveReferences();
            if (conversation.nodes.TryGetValue("root", out conversation.root))
            {
                conversation.onEnter = conv.onenter;
                conversation.onExit = conv.onexit;
                return true;
            }
            Debug.Assert(false, "conversation must have a statement with an id of \"root\"");
            conversation = default;
            return false;
        }

        [SerializeField]
        private TextAsset xmlFile;

        [SerializeField]
        private Monster monster;

        [SerializeField]
        private string message = "Talk";

        private Statement active;
        private Conversation conversation;
        private PlayerController playerController;
        private bool inRange = false;

        public string ActiveNode
        {
            get
            {
                return active?.id;
            }
            set
            {
                if ((active == null || active.id != value) &&
                    conversation.nodes.TryGetValue(value, out var statement))
                {
                    active = statement;
                    ShowNode();
                }
            }
        }

        private void Awake()
        {
            var name = xmlFile.name;
            if (!cache.TryGetValue(name, out conversation))
            {
                if (Parse(xmlFile, out conversation))
                {
                    cache[name] = conversation;
                }
            }
        }

        private void Start()
        {
            playerController = Controller.Player.GetComponent<PlayerController>();
        }

        private string GetReply(int offset)
        {
            var r = active.replies;
            return r == null || offset >= r.Length ? null : r[offset].label;
        }

        private void ReplySelected(int offset)
        {
            if (offset >= active.replies.Length)
            {
                return;
            }
            active = active.replies[offset].node;
            ShowNode();
        }

        private void ShowNode()
        {
            if (active.action != null)
            {
                SendMessage(active.action);
            }
            if (active.forward != null)
            {
                ActiveNode = active.forward;
            }
            else if (active.dialogue != null) {
                ConversationInterface.SetCaption(active.dialogue, GetReply, ReplySelected);
            }
        }

        private void Activate(string name)
        {
            if (name != null)
            {
                SendMessage(name);
            }
        }

        private void SetAnimation()
        {
            Controller.Player.AnimationManager.SetActivity(
                active == null ? MonsterAnimationManager.Activity.ATTACKING : MonsterAnimationManager.Activity.SHOPPING
            );
        }

        private IEnumerator CheckForActivate()
        {
            while (inRange)
            {
                if (Input.GetButtonDown("Fire3"))
                {
                    if (active == null)
                    {
                        Controller.ShowMessage("Exit conversation");
                        active = conversation.root;
                        playerController.ChatWith = monster;
                        Activate(conversation.onEnter);
                        ShowNode();
                    }
                    else
                    {
                        Activate(conversation.onExit);
                        active = null;
                        playerController.ChatWith = null;
                        ConversationInterface.SetCaption(null);
                        Controller.ShowMessage(message);
                    }
                    SetAnimation();
                }
                yield return null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == Controller.Player.gameObject)
            {
                Controller.ShowMessage(message);
                inRange = true;
                StartCoroutine(CheckForActivate());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == Controller.Player.gameObject)
            {
                Controller.HideMessage();
                inRange = false;
            }
        }
    }
}