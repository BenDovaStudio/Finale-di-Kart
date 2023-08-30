using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.NetCode;
using _Scripts.Track;
using Unity.Netcode;
using UnityEngine;

namespace _Scripts {
    public class DuelHandler : NetworkBehaviour {
        #region Variables

        [SerializeField] private TrackGenerator trackGenerator;

        [SerializeField] private List<int> activeChallengers;
        [SerializeField] private List<int> activeTargets;

        #endregion


        #region Builtin Methods (Network)

        public override void OnNetworkSpawn() {
            if (!IsServer) {
                Debug.Log("Instantiated but getting deleted :(");
                Destroy(gameObject);
                return;
            }


            SetUpEventListeners();

            trackGenerator = FindObjectOfType<TrackGenerator>();

            Debug.Log("Instantiated on server");

        }


        public override void OnNetworkDespawn() {
            RemoveListeners();
        }

        #endregion

        #region Custom Events (Network)

        public static event Action<ulong, ulong> OnBroadcastChallengeRequest;

        public static event Action<ulong, ChallengeState> OnChallengeStatusUpdate;

        #endregion

        #region Custom Event Methods (Network)

        private void OnChallengeInitiateRequestEvent(ulong challengerId, ulong targetId) {
            if (activeChallengers.Contains((int)challengerId)) {
                Debug.Log("Client should wait for the previous");
                OnChallengeStatusUpdate?.Invoke(challengerId, ChallengeState.Rejected);
                return;
            }
            if (activeTargets.Contains((int)targetId)) {
                Debug.Log("Client should wait for the target to become available");
                OnChallengeStatusUpdate?.Invoke(challengerId, ChallengeState.Rejected);
                return;
            }
            
            OnChallengeStatusUpdate?.Invoke(challengerId, ChallengeState.Accepted);
            OnBroadcastChallengeRequest?.Invoke(challengerId, targetId);
            StartCoroutine(DuelTimeoutTimer(10f, () => {
            // TODO ------------------------------------------Add the pair class that will keep record of the challenge routines and such stuff-------
            }));
            
            // TODO - remove these from the list upon challenge rejection or timeout
            activeChallengers.Add((int)challengerId);
            activeTargets.Add((int)targetId);
        }

        #endregion


        #region Custom Methods

        private void SetUpEventListeners() {
            PlayerChallengeHandler.OnChallengeInitiateRequest += OnChallengeInitiateRequestEvent;
        }

        private void RemoveListeners() {
            PlayerChallengeHandler.OnChallengeInitiateRequest -= OnChallengeInitiateRequestEvent;
        }

        #endregion


        #region Coroutines

        private IEnumerator DuelTimeoutTimer(float duration, Action action) {
            float timeElapsed = 0;

            while (timeElapsed < duration) {
                timeElapsed += Time.unscaledDeltaTime;
                yield return new WaitForEndOfFrame();
            }
            action.Invoke();
        }

        #endregion
    }
}