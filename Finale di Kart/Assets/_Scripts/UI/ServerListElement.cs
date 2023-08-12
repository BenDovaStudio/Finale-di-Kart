using System;
using _Scripts.Controllers;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI {


    public class ServerListElement : MonoBehaviour {
        #region Variables

        [SerializeField] private TextMeshProUGUI nameTextObject;

        [SerializeField] private TextMeshProUGUI regionTextObject;

        [SerializeField] private TextMeshProUGUI playersTextObject;

        private Lobby myLobby;

        [SerializeField] private Button myButton;

        [SerializeField] private Image highlight;

        #endregion


        #region Builtin Methods

        private void OnEnable() {
            myButton.onClick.AddListener(OnClickMethod);
        }

        private void OnDisable() {
            myButton.onClick.RemoveListener(OnClickMethod);
        }

        #endregion


        #region Public Functions

        public void UpdateValues(string serverName, string serverRegion, int currentPlayers, int maximumPlayers, Lobby lobby) {
            myLobby = lobby;
            nameTextObject.text = serverName;
            // regionTextObject.text = serverRegion;
            playersTextObject.text = $"{currentPlayers}/{maximumPlayers}";
        }

        private void OnClickMethod() {
            // TODO - Join Lobby + Server code here
            // NetworkController.Instance.JoinServer(myLobby);
            Debug.Log($"Clicked on {gameObject.name}");
            GameUIController.Instance.ResetHighlight();
            SetHighlight();
        }


        public void SetHighlight(bool yes = true) {
            highlight.gameObject.SetActive(yes);
        }

        #endregion
    }
}