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


        #region Custom Events
        
        // Lock variable
        private static bool _eventLocked = false;

        public delegate void LobbySelection(Lobby lobby);

        public static event LobbySelection OnLobbySelect;

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
            playersTextObject.text = $"{currentPlayers - 1}/{maximumPlayers - 1}";
        }

        public void SetHighlight(bool yes = true) {
            highlight.gameObject.SetActive(yes);
        }

        #endregion


        #region Custom Event Methods

        private void OnClickMethod() {
            if(!_eventLocked) {
                _eventLocked = true;
                // Debug.Log($"Clicked on {gameObject.name}");
                GameUIController.Instance.ResetHighlight();
                OnLobbySelect?.Invoke(myLobby);
                SetHighlight();
                _eventLocked = false;
            }
        }

        #endregion
    }
}