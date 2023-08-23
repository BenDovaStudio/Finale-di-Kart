using System;
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

        [FormerlySerializedAs("tracks")] [SerializeField] private List<Track> trackPrefabs;

        // private List<Track> currentTrack = new List<Track>();

        [SerializeField] private List<List<Track>> tracks = new List<List<Track>>();
        

        [SerializeField] private int trackLength;

        [SerializeField] private Button generateTrackButton;

        [SerializeField] private LayerMask trackMask;

        [SerializeField] private Transform[] trackLocations;

        [SerializeField] private bool[] trackBusy = new bool[5];


        private List<Track> currentTrack;

        #endregion


        #region Builtin Methods

        private void Start() {
            tracks.Add(new List<Track>());
            tracks.Add(new List<Track>());
            tracks.Add(new List<Track>());
            tracks.Add(new List<Track>());
            tracks.Add(new List<Track>());
            // Debug.Log($"Count: {tracks.Count}");
            
            // Spawn Offset x1.5, y0.15, z2.5
        }

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
        public void GenerateTrack() {
            if (!IsServer) return;
            Debug.Log("Generating");
            var indexTrack = GetFreeTrack();
            // var indexTrack = 0;
            currentTrack = tracks[indexTrack];
            Transform spawnTransform = trackLocations[indexTrack];
            trackBusy[indexTrack] = true;
            // Debug.Log($"GenerateTrack Called {IsServer} : {IsOwner} : {OwnerClientId} : {IsOwnedByServer}");
            DeletePreviousTrack();
            // var selfTransform = transform;
            var firstNode = Instantiate(trackPrefabs[0], spawnTransform.position, Quaternion.identity);
            firstNode.GetComponent<NetworkObject>().Spawn(true);
            firstNode.transform.SetParent(spawnTransform);
            currentTrack.Add(firstNode);


            for (int itr = 0; itr < trackLength; ++itr) {

                int trackBlock = Random.Range(0, trackPrefabs.Count);
                var spawnAtTransform = currentTrack[^1].GetEndNode();       // getting last spawned track from the list

                bool shouldSpawn = true;

                
                // Vector3 leftPoint =new Vector3(possibleEndPoint.pos, )
                
                // Ray rayleft = new Ray(possibleEndPoint.position)
                
                // Debug.Log($"");
                // Debug.DrawRay(possibleEndPoint.position, possibleEndPoint.forward);
                
                // .03f
                var spawnedTrack = Instantiate(trackPrefabs[trackBlock], spawnAtTransform);
                
                spawnedTrack.gameObject.SetActive(false);


                var tempHolder = trackPrefabs[trackBlock].GetEndNode().position;

                var spawnPosition = new Vector3(tempHolder.x, tempHolder.y + 0.03f, tempHolder.z);
                var possibleEndPoint = trackPrefabs[trackBlock].GetEndNode();
                

                Ray ray = new Ray(spawnPosition, possibleEndPoint.forward);
                RaycastHit hit0, hit1, hit2;
                if (Physics.Raycast(ray, out hit0, 1500f, trackMask)) {
                    shouldSpawn = false;
                }

                spawnPosition = new Vector3(tempHolder.x - 4, tempHolder.y + 0.03f, tempHolder.z);
                ray = new Ray(spawnPosition, possibleEndPoint.forward);
                
                if (Physics.Raycast(ray, out hit1, 1500f, trackMask)) {
                    shouldSpawn = false;
                }
                spawnPosition = new Vector3(tempHolder.x + 4, tempHolder.y + 0.03f, tempHolder.z);
                ray = new Ray(spawnPosition, possibleEndPoint.forward);
                
                if (Physics.Raycast(ray, out hit2, 1500f, trackMask)) {
                    shouldSpawn = false;
                }




                if (shouldSpawn) {
                    spawnedTrack.gameObject.SetActive(true);
                    spawnedTrack.GetComponent<NetworkObject>().Spawn(true);
                    spawnedTrack.transform.SetParent(spawnTransform);
                    currentTrack.Add(spawnedTrack);    
                }
                else {
                    Destroy(spawnedTrack.gameObject);
                    itr--;
                }
                
            }

            // return indexTrack;
        }
        
        
        
        private void DeletePreviousTrack() {
            foreach (var track in currentTrack) {
                Destroy(track.gameObject);
            }

            currentTrack.Clear();
        }


        private int GetFreeTrack() {
            for (int itr = 0; itr < trackBusy.Length; ++itr) {
                if (!trackBusy[itr]) {
                    Debug.Log($"Track Found at: {itr}");
                    return itr;
                }
            }
            Debug.Log($"Track Found at: {-1}");
            return -1;
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
            
            // Debug.Log($"Is owner: {IsOwner}, Is Server: {IsServer}");
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