using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Depravity
{
    public class UIConversationInterfaceProvider : ConversationInterfaceProvider
    {
        [SerializeField]
        private UIConversationBlock blurb, optionPrototype;

        [SerializeField]
        private Color activeReplyColour;

        [SerializeField, Range(0.0f, 1.0f)]
        private float optionPollDelay = 0.1f;

        [SerializeField, Range(0.0f, 1.0f)]
        private float optionChangeThreshold = 0.3f;

        private readonly List<UIConversationBlock> replies = new List<UIConversationBlock>(16);
        private readonly ResourcePool<UIConversationBlock> options;
        private int activeNode = -1;
        private ConversationReplySelected selected;
        private float nextPollTime = 0.0f;

        private UIConversationBlock MakeOption()
        {
            var opt = Instantiate(optionPrototype.gameObject);
            opt.transform.parent = blurb.transform.parent;
            return opt.GetComponent<UIConversationBlock>();
        }

        public UIConversationInterfaceProvider()
        {
           options = new ResourcePool<UIConversationBlock>(MakeOption);
        }

        private void Clear()
        {
            for (int i = 0, j = replies.Count; i != j; ++i)
            {
                var reply = replies[i];
                reply.Colour = optionPrototype.Colour;
                reply.gameObject.SetActive(false);
                options.Release(reply);
            }
            replies.Clear();
        }

        private int LoadOptions(ConversationReplyTextProvider options)
        {
            Clear();
            int i = 0;
            for (string option = options(i); option != null; option = options(++i))
            {
                var opt = this.options.Allocate();
                opt.Text = option;
                replies.Add(opt);
            }
            return i;
        }

        private void Close()
        {
            Clear();
            blurb.gameObject.SetActive(false);
        }

        private void ShowReplies(int num)
        {
            float optionHeight = optionPrototype.Height;
            float offset = num * optionHeight;
            blurb.Offset = offset + blurb.Height * 0.5f;
            blurb.gameObject.SetActive(true);
            offset -= optionHeight * 0.5f;
            for (int i = 0; i != num; ++i)
            {
                var reply = replies[i];
                reply.Offset = offset;
                reply.gameObject.SetActive(true);
                offset -= optionHeight;
            }
            if (num > 0)
            {
                SetActive(0);
            }
        }

        public override void SetCaption(string text, ConversationReplyTextProvider replies = null, ConversationReplySelected selected = null)
        {
            if (text == null)
            {
                Close();
            }
            else
            {
                this.selected = selected;
                blurb.Text = text;
                ShowReplies(LoadOptions(replies));
            }
        }

        private void SetActive(int node)
        {
            var img = replies[node].GetComponent<Image>();
            img.color = activeReplyColour;
            activeNode = node;
        }

        private void ToggleOn(int node)
        {
            replies[activeNode].Colour = optionPrototype.Colour;
            SetActive(node);
        }

        private void Poll()
        {
            float axis = Input.GetAxis("Vertical");
            if (axis > optionChangeThreshold)
            {
                if (activeNode > 0)
                {
                    ToggleOn(activeNode - 1);
                }
            }
            else if (axis < -optionChangeThreshold)
            {
                if (activeNode < replies.Count - 1)
                {
                    ToggleOn(activeNode + 1);
                }
            }
        }

        private void Update()
        {
            int len = replies.Count;
            if (len == 0)
            {
                return;
            }
            float time = Time.fixedUnscaledTime;
            if (time >= nextPollTime)
            {
                Poll();
                nextPollTime = time + optionPollDelay;
            }
            if (Input.GetButtonDown("Fire1"))
            {
                selected(activeNode);
            }
        }
    }
}
