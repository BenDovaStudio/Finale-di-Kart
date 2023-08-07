using TMPro;
using UnityEngine;

namespace _Scripts.UI {
    
    
    public class ServerListElement : MonoBehaviour {
        #region Serialized Data Members

        [SerializeField]
        private TextMeshProUGUI nameTextObject;

        [SerializeField]
        private TextMeshProUGUI regionTextObject;

        [SerializeField]
        private TextMeshProUGUI playersTextObject;

        #endregion


        #region Public Functions

        public void UpdateValues(string serverName, string serverRegion, int currentPlayers, int maximumPlayers) {
            nameTextObject.text = serverName;
            regionTextObject.text = serverRegion;
            playersTextObject.text = $"{currentPlayers}/{maximumPlayers}";
        }

        #endregion
    }
}