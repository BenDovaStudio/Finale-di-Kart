using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Track {
    public class TrackGenerator : NetworkBehaviour {
        #region Variables

        // [SerializeField] private bool generateTrack;

        [SerializeField] private NetworkVariable<bool> generateTrack = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private bool changeValue;
        private bool callRPC;

        [SerializeField] private List<Track> tracks;

        private List<Track> currentTrack = new List<Track>();

        [SerializeField] private int trackLength;

        #endregion


        #region Builtin Methods

        private void Update() {
            // if (!IsOwner) return;
            // if (generateTrack.Value) {
            //     generateTrack.Value = false;
            //     GenerateTrackServerRpc(10);
            // }
            if (callRPC) {
                callRPC = false;
                GenerateTrackServerRpc(10);
            }

            if (changeValue) {
                generateTrack.Value = !generateTrack.Value;
            }
        }

        #endregion


        #region Custom Methods

        // [ServerRpc(RequireOwnership = false)]
        [ServerRpc]
        public void GenerateTrackServerRpc(int trackLengthParam) {
            Debug.Log("ServerRpc Called" + OwnerClientId);
            DeletePreviousTrack();
            var selfTransform = transform;
            var firstNode = Instantiate(tracks[0], selfTransform.position, Quaternion.identity, selfTransform);
            firstNode.GetComponent<NetworkObject>().Spawn(true);
            currentTrack.Add(firstNode);


            for (int itr = 0; itr < trackLengthParam; ++itr) {

                int trackBlock = Random.Range(0, tracks.Count);
                var spawnAtTransform = currentTrack[^1].GetEndNode();
                var spawnedTrack = Instantiate(tracks[trackBlock], spawnAtTransform);

                currentTrack.Add(spawnedTrack);
            }
        }


        private void DeletePreviousTrack() {
            foreach (var track in currentTrack) {
                Destroy(track.gameObject);
            }

            currentTrack.Clear();
        }

        private void PrintRepeating() {
            Debug.Log(transform.position.ToString());
        }

        #endregion


        #region Network Events

        public override void OnNetworkSpawn() {
            Debug.Log(IsOwnedByServer + " <- Server Ownership");
            // Debug.Log(owne);
            InvokeRepeating(nameof(PrintRepeating), 1.5f, 1.5f);
            generateTrack.OnValueChanged += (value, newValue) => {
                Debug.Log(OwnerClientId + " ; " + generateTrack.Value + " -:- " + value + " , " + newValue);
            };
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            Debug.Log($"Going down: {gameObject}");
            Destroy(gameObject);
        }

        #endregion
    }
}