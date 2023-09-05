using System;
using System.Collections;
using System.Net;
using _Scripts.Controllers;
using DG.Tweening;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using ClientRpcParams = Unity.Netcode.ClientRpcParams;

namespace _Scripts.NetCode {
    public class PlayerChallengeHandler : NetworkBehaviour {

        #region Multiplayer Inputs

        private bool useMultiplayerInputs = false;

        [SerializeField] private PlayerInput multiplayerInputs;
        private InputAction initiateChallenge;
        private InputAction acceptChallenge;
        private InputAction rejectChallenge;

        #endregion


        private Vector3 posBeforeSpawnForRace;
        private Quaternion rotBeforeSpawnForRace;




        [SerializeField] private LayerMask terrainMask;

        [SerializeField] private int lockedClientId = -1;

        [SerializeField] private bool isWaitingForResponse = false;
        [SerializeField] private bool isBeingWaitedUpon = false;


        private ulong challengerId = ulong.MaxValue;


        private ulong raceOpponentId = ulong.MaxValue;

        private Tween tween;
        
        private readonly ulong[] targetClientArray = new ulong[1];



        // private int currentInitiatedId = -1;

        // private int currentTargetId = -1;

        /*[SerializeField] private List<int> beingWaitedUponClientIds = new List<int>();

    [SerializeField] private List<int> initiatedRequestClientIds = new List<int>();*/


        #region Builtin Methods

        private void Awake() {
            initiateChallenge = multiplayerInputs.actions.FindAction(AppConstants.InitiateChallenge, true);
            acceptChallenge = multiplayerInputs.actions.FindAction(AppConstants.AcceptChallenge, true);
            rejectChallenge = multiplayerInputs.actions.FindAction(AppConstants.RejectChallenge, true);
        }

        private void OnEnable() {
            InputEventSetup(true);
        }

        private void OnDisable() {
            InputEventSetup(false);
        }

        private void OnTriggerEnter(Collider other) {
            if (IsServer) {
                if (other.CompareTag(AppConstants.FinishTrigger)) {
                    // Debug.Log();
                    OnRaceFinish?.Invoke(OwnerClientId, raceOpponentId);
                }
            }

            if (!IsOwner) return;
            if (!other.CompareTag("Player")) return;
            if (lockedClientId >= 0) return;
            var otherDude = other.transform.GetComponentInParent<NetworkObject>();
            
            if (otherDude == null) return;
            lockedClientId = (int)otherDude.OwnerClientId;
            if (isBeingWaitedUpon || isWaitingForResponse) return;
            GameUIController.Instance.PromptInitiateRace();
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
        /*[ServerRpc]
        private void InitiateChallengeServerRpc(ulong targetClientId, ServerRpcParams serverRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> InitiateChallengeServerRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");
            if (!IsServer) return;
            float requestTimeout = 10f;
            var senderClientId = serverRpcParams.Receive.SenderClientId;

            Debug.Log($"Initiated req from client: {senderClientId} to {targetClientId}");

            targetClientArray[0] = senderClientId;
            // currentInitiatedId = (int)senderClientId;
            ClientRpcParams initiatorClientRpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = targetClientArray
                }
            };
            targetClientArray[0] = targetClientId;
            // currentTargetId = (int)targetClientId;
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
        #1#

            StartCoroutine(TimerRoutine(requestTimeout, () => {
                ResetChallengeClientRpc(initiatorClientRpcParams);
                ResetChallengeClientRpc(targetClientRpcParams);
            }));

            // Letting the initiator know about the processing of the challenge request
            InitiatedChallengeRequestClientRpc(requestTimeout, initiatorClientRpcParams);

            // inform the target about the challenge request
            ReceiveChallengeRequestClientRpc(senderClientId, requestTimeout, targetClientRpcParams);
        }*/

        /*[ServerRpc(RequireOwnership = false)]
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
        }*/

        [ServerRpc]
        private void InitiateChallengeBeginServerRpc(ulong targetId, ServerRpcParams serverParams = default) {
            if (!IsServer) return;
            var challenger = serverParams.Receive.SenderClientId;
            DuelHandler.OnChallengeStatusUpdate += OnChallengeStatus;
            OnChallengeInitiateRequest?.Invoke(challenger, targetId);
        }


        [ServerRpc]
        private void ChallengeResponseServerRpc(ulong challenger,ChallengeResponse response, ServerRpcParams serverParams = default) {
            if (!IsServer) return;
            var target = serverParams.Receive.SenderClientId;
            OnChallengeResponse?.Invoke(challenger, target, response);
        }


        #endregion


        #region ClientRPCs

        #region Old Mess

        /*[ClientRpc]
        private void InitiatedChallengeRequestClientRpc(float timeoutDuration, ClientRpcParams clientRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> InitiatedChallengeRequestClientRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");

            if (!IsOwner) return;
            isWaitingForResponse = true;
            GameUIController.Instance.PromptWaiting(timeoutDuration);
        }*/

        /*[ClientRpc]
        private void ReceiveChallengeRequestClientRpc(ulong challengerId, float timeoutDuration, ClientRpcParams clientRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> RecieveChallengeRequestClientRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");
            isBeingWaitedUpon = true;
            if (IsOwner) return;
            lockedClientId = (int)challengerId;
            Debug.Log("Waiting for user response");
            GameUIController.Instance.PromptAffirmation(timeoutDuration);
        }*/

        /*[ClientRpc]
        private void ResetChallengeClientRpc(ClientRpcParams clientRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> ResetChallengeClientRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");
            GameUIController.Instance.PromptDeactivateWhole();
            isBeingWaitedUpon = false;
            isWaitingForResponse = false;
            lockedClientId = -1;
            // clientRpcParams.Receive.
        }*/

        /*[ClientRpc]
        private void TeleportThemMFersClientRpc(Vector3 pos, ClientRpcParams clientRpcParams = default) {
            Debug.Log(
                $"{gameObject.name}:-> TeleportThemMFersClientRpc >> IsServer: {IsServer}, IsOwnedByServer: {IsOwnedByServer}, IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}, IsClient: {IsClient}, IsSpawned: {IsSpawned}");

            // transform.position = location.position;
            // NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.transform.position = pos;
            Debug.Log("Hello " + OwnerClientId);
            transform.position = pos;
        }*/


        #endregion

        [ClientRpc]
        private void ReceiveChallengeClientRpc(ulong challenger, float timeout, ClientRpcParams clientParams = default) {
            if (!IsOwner) return;
            Debug.Log($"Received Challenge Req from -> Player {challenger:D2}");
            isBeingWaitedUpon = true;
            challengerId = challenger;
            GameUIController.Instance.PromptAffirmation(timeout);
            tween = DOVirtual.DelayedCall(timeout, () => {
                Debug.Log("Disabling prompt from tween");
                GameUIController.Instance.DisablePrompt();
                isBeingWaitedUpon = false;
            });
        }
        
        [ClientRpc]
        private void ChallengeStatusUpdateClientRpc(ChallengeState status, float timeoutDuration, ClientRpcParams clientParams = default) {
            // Debug.Log("Executing RPC");
            if (!IsOwner) return;
            // Debug.Log("Is RPC owner");
            switch (status) {
                case ChallengeState.Accepted: {
                    // Debug.Log("Challenge Initiated, wait for response");
                    // GameUIController.Instance.DisablePrompt();
                    GameUIController.Instance.PromptWaiting(timeoutDuration);
                    break;
                }
                case ChallengeState.Rejected: {
                    // Debug.Log("Challenge Rejected");
                    GameUIController.Instance.DisablePrompt();
                    lockedClientId = -1;
                    isWaitingForResponse = false;
                    DuelHandler.OnChallengeStatusUpdate -= OnChallengeStatus;
                    break;
                }
            }
        }

        [ClientRpc]
        private void SpawnAtTrackClientRpc(Vector3 spawnPos, Quaternion spawnRot, ulong opponent, ClientRpcParams clientParams = default) {
            if (!IsOwner) return;
            raceOpponentId = opponent;
            transform.SetPositionAndRotation(spawnPos, spawnRot);
        }


        [ClientRpc]
        private void RaceConclusionClientRpc(Vector3 oldPos, Quaternion oldRot, RaceConclusion conclusion, ClientRpcParams clientParams = default) {
            if (!IsOwner) return;
            switch (conclusion) {
                case _Scripts.RaceConclusion.Win: {
                    GameUIController.Instance.DisplayWinMessage();
                    break;
                }
                case _Scripts.RaceConclusion.Lose: {
                    GameUIController.Instance.DisplayLoseMessage();
                    break;
                }
            }
            DOVirtual.DelayedCall(4f, () => {
                transform.SetPositionAndRotation(oldPos, oldRot);
            });
        }

        #endregion


        #region Coroutines

        // private delegate void CoroutineEnds();

        // private IEnumerator TimerRoutine(float duration, Action onCompleteAction) {
        //     float elapsedTime = 0;
        //     while (elapsedTime < duration) {
        //         elapsedTime += Time.unscaledDeltaTime;
        //         yield return new WaitForEndOfFrame();
        //     }
        //
        //     onCompleteAction?.Invoke();
        //     // _onTimerEnd?.Invoke();
        // }


        #endregion


        /*private bool HasInitiated(int clientId) {
        return initiatedRequestClientIds.Contains(clientId);
    }


    private bool IsBeingWeightedUpon(int clientId) {
        return beingWaitedUponClientIds.Contains(clientId);
    }*/

        #region Builtin Methods (Network)

        public override void OnNetworkSpawn() {
            #region Player Game Object name setup

            if (IsServer && (IsOwnedByServer || IsOwner)) gameObject.name = "ServerObj";
            if (IsClient && IsOwner) gameObject.name = $"Self_Client_{OwnerClientId}";
            if (IsClient && !IsOwner) gameObject.name = $"Other_Client_{OwnerClientId}";
            if (IsClient && IsOwnedByServer) gameObject.name = $"Other(ServerOwned)_Client_{OwnerClientId}";
            if (IsServer && (!IsOwner || !IsOwnedByServer)) gameObject.name = $"Other_Client_{OwnerClientId}";

            #endregion

            if (IsServer) {
                SetUpListeners();
            }

            if (IsOwner) {
                useMultiplayerInputs = true;
                /*  Spawn Location Randomizer, currently it is spawning on top of the water... like... come on... :(
                 var randomPosition = new Vector3(Random.Range(0, 100f), 0, Random.Range(0f, 100f));
                // randomPosition.y = 1000;
                var rayPos = randomPosition;
                rayPos.y = 1500f;
                var rotation = Vector3.zero;
                Ray ray = new Ray(rayPos, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, 5000f, terrainMask)) {
                    randomPosition.y = hit.point.y + 1;
                    rotation = hit.normal;
                }

                transform.SetPositionAndRotation(randomPosition, Quaternion.Euler(rotation));*/
            }
        }

        public override void OnNetworkDespawn() {
            if (IsServer) {
                RemoveListeners();
            }

            useMultiplayerInputs = false;
        }

        #endregion

        #region Custom Events (Network)

        public static event Action<ulong, ulong> OnChallengeInitiateRequest;
        public static event Action<ulong, ulong, ChallengeResponse> OnChallengeResponse;
        public static event Action<ulong, ulong> OnRaceFinish;

        #endregion

        #region Custom Event Methods

        private void OnInitiateInput(InputAction.CallbackContext context) {
            if (!useMultiplayerInputs || isBeingWaitedUpon || isWaitingForResponse) return;

            isWaitingForResponse = true;
            InitiateChallengeBeginServerRpc((ulong)lockedClientId);
        }

        private void OnAcceptInput(InputAction.CallbackContext context) {
            if (!useMultiplayerInputs || !isBeingWaitedUpon || isWaitingForResponse) return;
            tween?.Kill();
            GameUIController.Instance.DisablePrompt();
            // Debug.Log("Challenge Accepted");
            if(challengerId < 11) {
                ChallengeResponseServerRpc(challengerId, ChallengeResponse.Accept);
                // isBeingWaitedUpon = false;
            }
        }

        private void OnRejectInput(InputAction.CallbackContext context) {
            if (!useMultiplayerInputs || !isBeingWaitedUpon || isWaitingForResponse) return;
            tween?.Kill();
            GameUIController.Instance.DisablePrompt();
            // Debug.Log("Challenge Rejected");
            if(challengerId < 11) {
                ChallengeResponseServerRpc(challengerId, ChallengeResponse.Reject);
                challengerId = ulong.MaxValue;
                isBeingWaitedUpon = false;
            }
        }

        #endregion

        #region Custom Event Methods (Network)

        private void ChallengeRequestBroadcastEvent(ulong challenger, ulong targetId, float timeout) {
            if (targetId == OwnerClientId) {
                targetClientArray[0] = targetId;
                ClientRpcParams targetClientParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = targetClientArray } };
                ReceiveChallengeClientRpc(challenger, timeout, targetClientParams);
            }
        }


        private void OnChallengeStatus(ulong challenger, float timeoutDuration, ChallengeState status) {
            if (OwnerClientId != challenger) return;
            // Debug.Log("Received Challenge status update");
            targetClientArray[0] = challenger;
            var challengeClientParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = targetClientArray } };
            ChallengeStatusUpdateClientRpc(status, timeoutDuration, challengeClientParams);
            // DuelHandler.OnChallengeStatusUpdate -= OnChallengeStatus;
        }


        private void OnDuelBeginEvent(ulong challenger, ulong target, Vector3 challengerSpawn, Vector3 targetSpawn, Quaternion spawnRotation) {
            var myTransform = transform;
            if (challenger == OwnerClientId) {
                StoreWorldPosAndRot(myTransform);
                raceOpponentId = target;
                targetClientArray[0] = challenger;
                ClientRpcParams targetClientParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = targetClientArray } };
                // spawn challenger
                SpawnAtTrackClientRpc(challengerSpawn, spawnRotation, target, targetClientParams);
            }
            else if (target == OwnerClientId) {
                StoreWorldPosAndRot(myTransform);
                raceOpponentId = challenger;
                targetClientArray[0] = target;
                ClientRpcParams targetClientParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = targetClientArray } };
                // spawn target
                SpawnAtTrackClientRpc(targetSpawn, spawnRotation, challenger, targetClientParams);
            }
        }

        private void OnRaceConclusionEvent(ulong id, RaceConclusion conclusion) {
            if (id != OwnerClientId) return;
            targetClientArray[0] = id;
            ClientRpcParams targetClientParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = targetClientArray } };
            RaceConclusionClientRpc(posBeforeSpawnForRace, rotBeforeSpawnForRace, conclusion, targetClientParams);
        }

        #endregion


        #region Custom Methods

        private void SetUpListeners() {
            DuelHandler.OnBroadcastChallengeRequest += ChallengeRequestBroadcastEvent;
            DuelHandler.OnDuelBegin += OnDuelBeginEvent;
            DuelHandler.RaceConclusionEvent += OnRaceConclusionEvent;
        }

        private void RemoveListeners() {
            DuelHandler.OnBroadcastChallengeRequest -= ChallengeRequestBroadcastEvent;
            DuelHandler.OnDuelBegin -= OnDuelBeginEvent;
            DuelHandler.RaceConclusionEvent -= OnRaceConclusionEvent;
        }

        private void InputEventSetup(bool isEnable) {
            if (isEnable) {
                initiateChallenge.performed += OnInitiateInput;
                acceptChallenge.performed += OnAcceptInput;
                rejectChallenge.performed += OnRejectInput;
            }
            else {
                initiateChallenge.performed -= OnInitiateInput;
                acceptChallenge.performed -= OnAcceptInput;
                rejectChallenge.performed -= OnRejectInput;
            }
        }

        private void StoreWorldPosAndRot(Transform myTransform) {
            posBeforeSpawnForRace = myTransform.position;
            rotBeforeSpawnForRace = myTransform.rotation;
        }

        #endregion


        /*struct PlayerData : INetworkSerializable {
            public ulong Id;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
                serializer.SerializeValue(ref Id);
            }
        }*/
    }
}