using UnityEngine;

namespace _Scripts.Track {
    public class Track : MonoBehaviour {
        #region Variables

        [SerializeField] private Transform endNode;

        public Transform GetEndNode() {
            return endNode;
        }

        #endregion
    }
}