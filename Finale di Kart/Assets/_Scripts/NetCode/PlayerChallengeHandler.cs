using System;
using System.Collections;
using _Scripts.Controllers;
using _Scripts.Track;
using Unity.Netcode;
using UnityEngine;

namespace _Scripts.NetCode {
    public class PlayerChallengeHandler : NetworkBehaviour {

        [SerializeField] private int lockedClientId = -1;
    
        [SerializeField] private bool isWaitingForResponse = false;
        [SerializeField] private bool isBeingWaitedUpon = false;
    
        private int currentInitiatedId = -1;
        private int currentTargetId = -1;



        private readonly ulong[] targetClientArray = new ulong[1];
    

        /*[SerializeField] private List<int> beingWaitedUponClientIds = new List<int>();

    [SerializeField] private List<int> initiatedRequestClientIds = new List<int>();*/


        #region Builtin Methods

        private void Update() {
            if (lockedClientId > -1) {
                if (!isWaitingForResponse) {
                    if (Input.GetKeyDown(KeyCode.G)) {
                        InitiateChallengeServerRpc((ulong)lockedClientId);
                    }
                }
            }

            if (isBeingWaitedUpon) {
                // Debug.Log("Press Y or N");
                if (Input.GetKeyDown(KeyCode.Y)) {
                    // Debug.Log("Y Pressed");
                    ReplyChallengeServerRpc(ChallengeResponse.Accept, (ulong)lockedClientId);
                    GameUIController.Instance.DisablePrompt();
                    isBeingWaitedUpon = false;
                }

                if (Input.GetKeyDown(KeyCode.N)) {
                    // Debug.Log("N Pressed");
                    ReplyChallengeServerRpc(ChallengeResponse.Reject, (ulong)lockedClientId);
                    GameUIController.Instance.DisablePrompt();
                    isBeingWaitedUpon = false;
                }

                // isBeingWaitedUpon = false;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!IsOwner) return;
            if (!other.CompareTag("Player")) return;
            if (lockedClientId >= 0) return;
            var otherDude = other.transform.GetComponentInParent<NetworkObject>();
            if (otherDude != null) {
                lockedClientId = (int)otherDude.OwnerClientId;
                GameUIController.Instance.PromptInitiateRace();
                // Call ServerRPC to initiate challenge request
            }
        }

        private void OnTriggerExit(Collider other) {
            if (!IsOwner) return;
            if (!other.CompareTag("Player")) return;
            if (lockedClientId < 0) return;
            var otherDude = other.transform.GetComponentInParent<NetworkObject>();
            if (otherDude != null) {
                if ((int)otherDude.OwnerClientId == lockedClientId) {
                    GameUIController.Instance.DisablePrompt();
                    lockedClientId = -1;
                }
            }
        }

        #endregion


        #region ServerRPCs


        // Will Get called upon challenge initiate request
        [ServerRpc]
        private void InitiateChallengeServerRpc(ulong targetClientId, ServerRpcParams serverRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> InitiateChallengeServerRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");
            if (!IsServer) return;
            float requestTimeout = 10f;
            var senderClientId = serverRpcParams.Receive.SenderClientId;

            Debug.Log($"Initiated req from client: {senderClientId} to {targetClientId}");

            targetClientArray[0] = senderClientId;
            currentInitiatedId = (int)senderClientId;
            ClientRpcParams initiatorClientRpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = targetClientArray
                }
            };
            targetClientArray[0] = targetClientId;
            currentTargetId = (int)targetClientId;
            ClientRpcParams targetClientRpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = targetClientArray
                }
            };


            if (currentTargetId >= 0) {
                Debug.Log("Server Busy");
                return;
            }

            if (currentInitiatedId >= 0) {
                Debug.Log("Server busy");
                return;
            }


            /*
        if (HasInitiated((int)senderClientId)) {
            Debug.Log("Cannot Request More than once");
            return;
        }

        if (HasInitiated((int)targetClientId)) {
            Debug.Log("Cannot challenge already engaged player. Already initiated with another");
            return;
        }

        if (IsBeingWeightedUpon((int)targetClientId)) {
            Debug.Log("Cannot receive multiple requests at a time");
            return;
        }
        initiatedRequestClientIds.Add((int)senderClientId);
        beingWaitedUponClientIds.Add((int)targetClientId);
        */

            StartCoroutine(TimerRoutine(requestTimeout, () => {
                ResetChallengeClientRpc(initiatorClientRpcParams);
                ResetChallengeClientRpc(targetClientRpcParams);
            }));

            // Letting the initiator know about the processing of the challenge request
            InitiatedChallengeRequestClientRpc(requestTimeout, initiatorClientRpcParams);

            // inform the target about the challenge request
            ReceiveChallengeRequestClientRpc(senderClientId, requestTimeout, targetClientRpcParams);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReplyChallengeServerRpc(ChallengeResponse response, ulong challengerId, ServerRpcParams serverRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> ReplyChallengeServerRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");
            if (!IsServer) return;
            ClientRpcParams initiatorClientRpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = new ulong[] { challengerId }
                }
            };

            ClientRpcParams targetClientRpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                }
            };
            switch (response) {
                case ChallengeResponse.Accept: {
                    // challengePairs[requestPairIndex].challengeState = ChallengeState.Accepted;
                    Debug.Log($"Challenge Accepted by client: {serverRpcParams.Receive.SenderClientId}");
                    TrackGenerator.Instance.GenerateTrack();
                    TeleportThemMFersClientRpc(new Vector3(-1.5f, 2, 0), targetClientRpcParams);
                    TeleportThemMFersClientRpc(new Vector3(1.5f, 2, 0), initiatorClientRpcParams);
                    break;
                }
                case ChallengeResponse.Reject: {
                    Debug.Log("Challenge Rejected");
                    // challengePairs[requestPairIndex].challengeState = ChallengeState.Rejected;
                    break;
                }
            }

            currentInitiatedId = -1;
            currentTargetId = -1;
        }



        #endregion


        #region ClientRPCs

        [ClientRpc]
        private void InitiatedChallengeRequestClientRpc(float timeoutDuration, ClientRpcParams clientRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> InitiatedChallengeRequestClientRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");

            if (!IsOwner) return;
            isWaitingForResponse = true;
            GameUIController.Instance.PromptWaiting(timeoutDuration);
        }

        [ClientRpc]
        private void ReceiveChallengeRequestClientRpc(ulong challengerId, float timeoutDuration, ClientRpcParams clientRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> RecieveChallengeRequestClientRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");
            isBeingWaitedUpon = true;
            if (IsOwner) return;
            lockedClientId = (int)challengerId;
            Debug.Log("Waiting for user response");
            GameUIController.Instance.PromptAffirmation(timeoutDuration);
        }

        [ClientRpc]
        private void ResetChallengeClientRpc(ClientRpcParams clientRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> ResetChallengeClientRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");
            GameUIController.Instance.PromptDeactivateWhole();
            isBeingWaitedUpon = false;
            isWaitingForResponse = false;
            lockedClientId = -1;
            // clientRpcParams.Receive.
        }

        [ClientRpc]
        private void TeleportThemMFersClientRpc(Vector3 pos, ClientRpcParams clientRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> TeleportThemMFersClientRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");

            // transform.position = location.position;
            // NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.transform.position = pos;
            Debug.Log("Hello " + OwnerClientId);
            transform.position = pos;
        }

        #endregion


        #region Coroutines

        private delegate void CoroutineEnds();

        private IEnumerator TimerRoutine(float duration, Action onCompleteAction) {
            float elapsedTime = 0;
            while (elapsedTime < duration) {
                elapsedTime += Time.unscaledDeltaTime;
                yield return new WaitForEndOfFrame();
            }

            onCompleteAction?.Invoke();
            // _onTimerEnd?.Invoke();
        }


        #endregion


        #region Custom Methods

        /*private bool HasInitiated(int clientId) {
        return initiatedRequestClientIds.Contains(clientId);
    }


    private bool IsBeingWeightedUpon(int clientId) {
        return beingWaitedUponClientIds.Contains(clientId);
    }*/


        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsServer && (IsOwnedByServer || IsOwner))
                gameObject.name = "ServerObj";
            if (IsClient && IsOwner)
                gameObject.name = $"Self_Client_{OwnerClientId}";
            if (IsClient && !IsOwner)
                gameObject.name = $"Other_Client_{OwnerClientId}";
            if(IsClient && IsOwnedByServer)
                gameObject.name = $"Other(ServerOwned)_Client_{OwnerClientId}";
        }

        #endregion


        struct PlayerData : INetworkSerializable {
            public ulong Id;
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
                serializer.SerializeValue(ref Id);
            }
        }
    }
}