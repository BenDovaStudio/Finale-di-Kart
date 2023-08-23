using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts;
using _Scripts.Controllers;
using _Scripts.Track;
using Unity.Netcode;
using UnityEngine;

public class PlayerChallengeHandler : NetworkBehaviour {

    [SerializeField] private int lockedClientId = -1;


    [SerializeField] private bool isWaitingForResponse = false;

    [SerializeField] private bool isBeingWaitedUpon = false;



    private int currentInitiatedId = -1;
    private int currentTargetId = -1;



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
                ReplyChallengeServerRpc(ChallengeResponse.Accept);
                isBeingWaitedUpon = false;
            }

            if (Input.GetKeyDown(KeyCode.N)) {
                // Debug.Log("N Pressed");
                ReplyChallengeServerRpc(ChallengeResponse.Reject);
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
        if (!IsServer) return;
        float requestTimeout = 10f;
        var senderClientId = serverRpcParams.Receive.SenderClientId;

        ClientRpcParams initiatorClientRpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = new ulong[] { senderClientId }
            }
        };

        ClientRpcParams targetClientRpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = new ulong[] { targetClientId }
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
        ReceiveChallengeRequestClientRpc(requestTimeout, targetClientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReplyChallengeServerRpc(ChallengeResponse response, ServerRpcParams serverRpcParams = default) {
        if (!IsServer) return;
        switch (response) {
            case ChallengeResponse.Accept: {
                // challengePairs[requestPairIndex].challengeState = ChallengeState.Accepted;
                Debug.Log($"Challenge Accepted by client: {serverRpcParams.Receive.SenderClientId}");
                TrackGenerator.Instance.GenerateTrack();
                break;
            }
            case ChallengeResponse.Reject: {
                Debug.Log("Challenge Rejected");
                // challengePairs[requestPairIndex].challengeState = ChallengeState.Rejected;
                break;
            }
        }
    }
    
    

    #endregion


    #region ClientRPCs

    [ClientRpc]
    private void InitiatedChallengeRequestClientRpc(float timeoutDuration, ClientRpcParams clientRpcParams = default) {
        if (!IsOwner) return;
        isWaitingForResponse = true;
        GameUIController.Instance.PromptWaiting(timeoutDuration);
    }

    [ClientRpc]
    private void ReceiveChallengeRequestClientRpc(float timeoutDuration, ClientRpcParams clientRpcParams = default) {
        isBeingWaitedUpon = true;
        if (IsOwner) return;
        Debug.Log("Waiting for user response");
        GameUIController.Instance.PromptAffirmation(timeoutDuration);
    }
    
    [ClientRpc]
    private void ResetChallengeClientRpc(ClientRpcParams clientRpcParams = default) {
        GameUIController.Instance.PromptDeactivateWhole();
        isBeingWaitedUpon = false;
        isWaitingForResponse = false;
        lockedClientId = -1;
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

    #endregion


}