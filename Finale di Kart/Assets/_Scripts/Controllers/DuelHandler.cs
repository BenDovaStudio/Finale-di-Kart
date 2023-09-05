using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.NetCode;
using _Scripts.Track;
using _Scripts.Utilities;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Controllers {
    public class DuelHandler : NetworkBehaviour {
        #region Variables

        [SerializeField] private TrackGenerator trackGenerator;

        [SerializeField] private List<int> activeChallengers;
        [SerializeField] private List<int> activeTargets;

        [SerializeField] private ChallengePairs pairs = new ChallengePairs();
        
        

        #endregion


        #region Builtin Methods (Network)

        public override void OnNetworkSpawn() {
            if (!IsServer) {
                // Debug.Log("Instantiated but getting deleted :(");
                Destroy(gameObject);
                return;
            }


            SetUpEventListeners();

            trackGenerator = FindObjectOfType<TrackGenerator>();

            // Debug.Log("Instantiated on server");

        }


        public override void OnNetworkDespawn() {
            RemoveListeners();
        }

        #endregion

        #region Custom Events (Network)

        public static event Action<ulong, ulong, float> OnBroadcastChallengeRequest;

        public static event Action<ulong, float, ChallengeState> OnChallengeStatusUpdate;

        public static event Action<ulong, ulong, Vector3, Vector3, Quaternion> OnDuelBegin;



        public static event Action<ulong, RaceConclusion> RaceConclusionEvent;

        #endregion

        #region Custom Event Methods (Network)

        private void OnChallengeInitiateRequestEvent(ulong challengerId, ulong targetId) {
            if (activeChallengers.Contains((int)challengerId)) {
                Debug.Log("Client should wait for the previous");
                OnChallengeStatusUpdate?.Invoke(challengerId, 0f, ChallengeState.Rejected);
                return;
            }
            if (activeTargets.Contains((int)targetId)) {
                Debug.Log("Client should wait for the target to become available");
                OnChallengeStatusUpdate?.Invoke(challengerId, 0f, ChallengeState.Rejected);
                return;
            }
            
            if (pairs.HasPair(challengerId, targetId)) {
                Debug.Log($"Pair {challengerId + targetId} Already Exist");
                return;
            }
            
            OnChallengeStatusUpdate?.Invoke(challengerId, AppConstants.ChallengeRequestTimeout, ChallengeState.Accepted);
            OnBroadcastChallengeRequest?.Invoke(challengerId, targetId, AppConstants.ChallengeRequestTimeout);
            var routine = StartCoroutine(DuelTimeoutTimer(10f, () => {
                HandleRemovePlayers(challengerId, targetId);
                OnChallengeStatusUpdate?.Invoke(challengerId, 0f, ChallengeState.Rejected);
            }));
            pairs.AddPair(challengerId, targetId, routine);
            
            activeChallengers.Add((int)challengerId);
            activeTargets.Add((int)targetId);
        }


        private void OnChallengeResponseEvent(ulong challengerId, ulong targetId, ChallengeResponse response) {
            if (!pairs.HasPair(challengerId, targetId)) return;
            Duel duelPair = new Duel(challengerId, targetId);
            switch (response) {
                case ChallengeResponse.Accept: {
                    // TODO - Instantiate track and spawn them mfrs

                    // Player Spawn Offset +-1.5x, 0.15y, 2.5z
                    var duelTrackIndex = trackGenerator.GenerateServerTrack(out var playerSpawn);

                    if (duelTrackIndex < 0) return;
                    
                    Debug.Log("Spawning them on their respected tracks");
                    
                    var vectNegPos = new Vector3(playerSpawn.x - 1.5f, playerSpawn.y + 0.15f, playerSpawn.z + 2.5f);
                    var vectPosPos = new Vector3(playerSpawn.x + 1.5f, playerSpawn.y + 0.15f, playerSpawn.z + 2.5f);
                    
                    
                    OnDuelBegin?.Invoke(challengerId, targetId, vectPosPos, vectNegPos, quaternion.identity);
                    
                    
                    // OnDuelBegin
                    break;
                }
                case ChallengeResponse.Reject: {
                    var routine = pairs.GetTimerRoutine(challengerId, targetId);
                    if (routine != null) {
                        Debug.Log("Stopping Routine");
                        StopCoroutine(routine);
                    }
                    OnChallengeStatusUpdate?.Invoke(challengerId, targetId, ChallengeState.Rejected);
                    HandleRemovePlayers(challengerId, targetId);
                    break;
                }
            }
        }


        private void OnRaceFinishEvent(ulong winner, ulong loser) {
            RaceConclusionEvent?.Invoke(winner, RaceConclusion.Win);
            RaceConclusionEvent?.Invoke(loser, RaceConclusion.Lose);
        }

        #endregion


        #region Custom Methods

        private void SetUpEventListeners() {
            PlayerChallengeHandler.OnChallengeInitiateRequest += OnChallengeInitiateRequestEvent;
            PlayerChallengeHandler.OnChallengeResponse += OnChallengeResponseEvent;
            PlayerChallengeHandler.OnRaceFinish += OnRaceFinishEvent;
        }

        private void RemoveListeners() {
            PlayerChallengeHandler.OnChallengeInitiateRequest -= OnChallengeInitiateRequestEvent;
            PlayerChallengeHandler.OnChallengeResponse -= OnChallengeResponseEvent;
        }

        private void HandleRemovePlayers(ulong challenger, ulong target) {
            activeChallengers.Remove((int)challenger);
            activeTargets.Remove((int)target);
            Duel duel = new(challenger, target);
            var index = pairs.GetPairIndex(duel);
            pairs.RemovePairAt(index);
        }

        #endregion


        #region Coroutines

        private IEnumerator DuelTimeoutTimer(float duration, Action action) {
            float timeElapsed = 0;
            Debug.Log("Server: Timer Started");

            while (timeElapsed < duration) {
                timeElapsed += Time.unscaledDeltaTime;
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("Server: Challenge Timeout");
            action.Invoke();
        }

        #endregion
    }
}