using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Scripts.Track {
    public class TrackGenerator : NetworkSingleton<TrackGenerator> {
        #region Variables

        // [SerializeField] private bool generateTrack;

        // [SerializeField] private NetworkVariable<bool> generateTrack = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // private bool changeValue;
        // private bool callRPC;

        [SerializeField] private List<Track> tracks;

        private List<Track> currentTrack = new List<Track>();

        [SerializeField] private int trackLength;

        [SerializeField] private Button generateTrackButton;

        #endregion


        #region Builtin Methods

        private void Update() {
            // if (!IsOwner) return;
            // if (generateTrack.Value) {
            //     generateTrack.Value = false;
            //     GenerateTrackServerRpc(10);
            // }
            // if (!callRPC) return;
            // callRPC = false;
            // GenerateTrackServerRpc(10);

            // if (changeValue) {
            //     generateTrack.Value = !generateTrack.Value;
            // }
        }

        #endregion


        #region Custom Methods

        // [ServerRpc(RequireOwnership = false)]
        // [ServerRpc]
        private void GenerateTrack() {
            if (!IsServer) return;
            Debug.Log($"GenerateTrack Called {IsServer} : {IsOwner} : {OwnerClientId} : {IsOwnedByServer}");
            DeletePreviousTrack();
            var selfTransform = transform;
            var firstNode = Instantiate(tracks[0], selfTransform.position, Quaternion.identity, selfTransform);
            firstNode.GetComponent<NetworkObject>().Spawn(true);
            currentTrack.Add(firstNode);


            for (int itr = 0; itr < trackLength; ++itr) {

                int trackBlock = Random.Range(0, tracks.Count);
                var spawnAtTransform = currentTrack[^1].GetEndNode();       // getting last spawned track from the list

                var possibleEndPoint = tracks[trackBlock].GetEndNode();

                Ray ray = new Ray(possibleEndPoint.position, possibleEndPoint.forward);
                
                // Debug.Log($"");
                // Debug.DrawRay(possibleEndPoint.position, possibleEndPoint.forward);
                
                // .03f
                var spawnedTrack = Instantiate(tracks[trackBlock], spawnAtTransform);
                spawnedTrack.GetComponent<NetworkObject>().Spawn(true);

                currentTrack.Add(spawnedTrack);
            }
        }


        private void DeletePreviousTrack() {
            foreach (var track in currentTrack) {
                Destroy(track.gameObject);
            }

            currentTrack.Clear();
        }

        /*private void PrintRepeating() {
            Debug.Log(transform.position.ToString());
        }*/

        #endregion


        #region Network Events

        public override void OnNetworkSpawn() {
            // Debug.Log(IsOwnedByServer + " <- Server Ownership");
            // Debug.Log(owne);
            // InvokeRepeating(nameof(PrintRepeating), 1.5f, 1.5f);
            // generateTrack.OnValueChanged += (value, newValue) => {
            //     Debug.Log(OwnerClientId + " ; " + generateTrack.Value + " -:- " + value + " , " + newValue);
            // };

            generateTrackButton.onClick.AddListener(GenerateTrack);
            
            Debug.Log($"Is owner: {IsOwner}, Is Server: {IsServer}");
            if(generateTrackButton) generateTrackButton.gameObject.SetActive(IsServer);
            
            
        }

        public override void OnNetworkDespawn() {
            generateTrackButton.onClick.RemoveListener(GenerateTrack);
            base.OnNetworkDespawn();
            
            Debug.Log($"Going down: {gameObject}");
            Destroy(gameObject);
        }

        #endregion
    }
}