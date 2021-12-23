using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using General;
using Networking.RequestMessages;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class FriendCanvas : MonoBehaviour {
        [SerializeField] private TMP_InputField makeRequestField;

        [SerializeField] private GameObject friendScreen;
        [SerializeField] private Transform friendsParent;
        [SerializeField] private GameObject friendsPrefab;
        [SerializeField] private Transform requestsParent;
        [SerializeField] private GameObject requestsPrefab;
        [SerializeField] private Transform sentParent;
        [SerializeField] private GameObject sentPrefab;
        [SerializeField] private Transform inviteParent;
        [SerializeField] private GameObject invitePrefab;
        [SerializeField] private float inviteDuration = 300;

        private readonly Dictionary<string, GameObject> _friendsList = new Dictionary<string, GameObject> ();
        private readonly Dictionary<string, GameObject> _requestsList = new Dictionary<string, GameObject> ();
        private readonly Dictionary<string, GameObject> _sentRequestsList = new Dictionary<string, GameObject> ();
        private readonly Dictionary<float, GameObject> _invitesList = new Dictionary<float, GameObject> ();

        private async void Start () {
            await Task.Delay (200);
            if (GameManager.instance.isServer) {
                Destroy(gameObject);
                return;
            }
            RequestManagerClient.instance.onResponse += (req, res, args) => {
                switch (req) {
                    case "friends":
                        OnFriendsChanged (res);
                        break;
                    case "requests":
                        OnRequestsChanged (res);
                        break;
                    case "sentRequests":
                        OnSentChanged (res);
                        break;
                    case "notifyRequest":
                        AddRequest (args[0]);
                        break;
                    case "notifyAccept":
                        AddFriend (args[0]);
                        RemoveSentRequestUI (args[0]);
                        break;
                    case "notifyDecline":
                        RemoveSentRequestUI (args[0]);
                        break;
                    case "notifyRequestRemove":
                        RemoveRequestUI (args[0]);
                        break;
                    case "notifyRemove":
                        RemoveFriendUI (args[0]);
                        break;
                    case "notifyInvite":
                        AddInvite (args[0], args[1]);
                        break;
                }
            };
            GetData();
        }

        public void ToggleFriends () {
            //TODO: TEMP
            //if (GameManager.instance.isInGUI && !friendScreen.activeSelf || GameManager.instance.isServer) {
            //   return;
            //}

            friendScreen.SetActive (!friendScreen.activeSelf);
            if (friendScreen.activeSelf) {
                GameManager.instance.isInGUI = true;
                MouseController.instance.ShowCursor ();

                GetData ();
            } else {
                GameManager.instance.isInGUI = false;
                MouseController.instance.HideCursor ();
            }
        }

        public void MakeFriendRequest() {
            RequestManagerClient.instance.SendRequest("addRequest", new string[] { makeRequestField.text });
            GetData();
        }

        private void Update () {
            //TODO: USE NEW INPUT SYSTEM
            if (Input.GetKeyDown (KeyCode.F)) {
                ToggleFriends ();
            }

            CheckInvites ();
        }

        private void OnFriendsChanged (string res) {
            string[] friends = res.Split (';');
            //TODO: This is order dependent.
            foreach (string friend in friends.Where (f => !_friendsList.ContainsKey (f))) {
                if (friend.Trim() == "") {
                    continue;
                }
                AddFriend (friend);
            }
        }

        private void OnRequestsChanged (string res) {
            string[] friends = res.Split (';');
            //TODO: This is order dependent.
            foreach (string friend in friends.Where (f => !_requestsList.ContainsKey (f))) {
                if (friend.Trim() == "") {
                    continue;
                }
                AddRequest (friend);
            }
        }

        private void AddRequest (string friend) {
            GameObject go = Instantiate (requestsPrefab, requestsParent);
            go.transform.GetChild (0).GetComponent<TMP_Text> ().text = friend;
            go.transform.GetChild (1).GetComponent<Button> ().onClick.AddListener (() => { AcceptRequest (friend); });
            go.transform.GetChild (2).GetComponent<Button> ().onClick.AddListener (() => { DeclineRequest (friend); });
            _requestsList.Add (friend, go);
        }

        private void AddFriend (string friend) {
            GameObject go = Instantiate (friendsPrefab, friendsParent);
            go.transform.GetChild (0).GetComponent<TMP_Text> ().text = friend;
            go.transform.GetChild (1).GetComponent<Button> ().onClick.AddListener (() => { InviteFriend (friend); });
            go.transform.GetChild (2).GetComponent<Button> ().onClick.AddListener (() => { RemoveFriend (friend); });
            _friendsList.Add (friend, go);
        }

        private void OnSentChanged (string res) {
            string[] friends = res.Split (';');
            //TODO: This is order dependent.
            foreach (string friend in friends.Where (f => !_sentRequestsList.ContainsKey (f))) {
                if (friend.Trim() == "") {
                    continue;
                }
                GameObject go = Instantiate (sentPrefab, sentParent);
                go.transform.GetChild (0).GetComponent<TMP_Text> ().text = friend;
                go.transform.GetChild (1).GetComponent<Button> ().onClick
                    .AddListener (() => { RemoveSentRequest (friend); });
                _sentRequestsList.Add (friend, go);
            }
        }

        private void AddInvite (string friend, string gameID) {
            GameObject go = Instantiate (invitePrefab, inviteParent);
            go.transform.GetChild (0).GetComponent<TMP_Text> ().text = friend;
            go.transform.GetChild (1).GetComponent<Button> ().onClick.AddListener (() => { JoinGame (gameID); });
            _invitesList.Add (Time.time, go);
        }

        private void CheckInvites () {
            List<float> toRemove = new List<float> ();
            foreach (KeyValuePair<float, GameObject> invite in _invitesList.Where (invite => 
                Time.time - invite.Key > inviteDuration)) {
                Destroy (invite.Value);
                toRemove.Add (invite.Key);
            }

            foreach (float time in toRemove) {
                _invitesList.Remove (time);
            }
        }

        private void RemoveFriendUI (string friend) {
            Destroy (_friendsList[friend]);
            _friendsList.Remove (friend);
        }

        private void RemoveRequestUI (string friend) {
            Destroy (_requestsList[friend]);
            _requestsList.Remove (friend);
        }

        private void RemoveSentRequestUI (string friend) {
            Destroy (_sentRequestsList[friend]);
            _sentRequestsList.Remove (friend);
        }

        public void RemoveFriend (string friend) {
            RequestManagerClient.instance.SendRequest ("removeFriend", friend);
            RemoveFriendUI (friend);
        }

        public void InviteFriend (string friend) { 
            if (GameManager.instance.currentGame == "") {
                return;
            }
            RequestManagerClient.instance.SendRequest ("inviteFriend", friend, GameManager.instance.currentGame); 
        }

        public void AcceptRequest (string friend) {
            RequestManagerClient.instance.SendRequest ("acceptRequest", friend);
            RemoveRequestUI (friend);
            AddFriend(friend);
        }

        public void DeclineRequest (string friend) {
            RequestManagerClient.instance.SendRequest ("declineRequest", friend);
            RemoveRequestUI (friend);
        }

        public void JoinGame (string gameID) {
            GameManager.instance.LeaveMinigame();
            GameManager.instance.JoinMinigame(gameID);
        }

        public void RemoveSentRequest (string friend) {
            RequestManagerClient.instance.SendRequest ("removeRequest", friend);
            RemoveSentRequestUI (friend);
        }

        private void GetData () {
            RequestManagerClient.instance.SendRequest ("friends");
            RequestManagerClient.instance.SendRequest ("requests");
            RequestManagerClient.instance.SendRequest ("sentRequests");
        }
    }
}