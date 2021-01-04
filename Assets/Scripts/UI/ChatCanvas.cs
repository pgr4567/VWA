﻿using System.Collections.Generic;
using Chat;
using General;
using Mirror;
using Player;
using TMPro;
using UnityEngine;

namespace UI {
    public class ChatCanvas : MonoBehaviour {
        public static ChatCanvas instance;
        [SerializeField] private GameObject chatUI;
        [SerializeField] private int maxMessagesCount = 100;
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private Transform messageParent;
        [SerializeField] private TMP_InputField msgInput;
        private readonly Queue<GameObject> _messageObjs = new Queue<GameObject> ();
        private readonly Queue<ChatMessage> _messages = new Queue<ChatMessage> ();

        private void Awake () {
            if (instance != null) {
                Debug.LogWarning ("There must only ever be one ChatCanvas!");
                Destroy (gameObject);
                return;
            }

            instance = this;
        }

        public void ToggleChat () {
            chatUI.SetActive(!chatUI.activeSelf);
            if (chatUI.activeSelf) {
                GameManager.instance.isInGUI = true;
                MouseController.instance.ShowCursor();
            }
        }

        public new void SendMessage (string message) {
            if (message.Trim () == "") {
                return;
            }

            msgInput.text = string.Empty;
            ChatMessage msg = new ChatMessage { sender = GameManager.instance.username, message = message };
            NetworkClient.Send (msg);
            AppendMessage (msg);
        }

        public void AppendMessage (ChatMessage msg) {
            GameObject go = Instantiate (messagePrefab, messageParent);
            go.GetComponent<TMP_Text> ().text = "<" + msg.sender + ">: " + msg.message;
            _messages.Enqueue (msg);
            _messageObjs.Enqueue (go);
            if (_messages.Count > maxMessagesCount) {
                _messages.Dequeue ();
                Destroy (_messageObjs.Dequeue ());
            }
        }
    }
}