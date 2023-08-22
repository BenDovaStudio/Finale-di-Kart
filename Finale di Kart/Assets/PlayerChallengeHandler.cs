using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerChallengeHandler : NetworkBehaviour {
    [SerializeField] private List<Transform> players;

    [SerializeField] private ulong clientId;
    
    [SerializeField] private bool callRpc;

    private void Start() {
        // InvokeRepeating();
    }

    private void Update() {
        /*if (callRpc) {
            callRpc = false;
            // List<Transform> obtainedList = GetPlayersServerRpc(clientId);
            // foreach (var playerTransform in obtainedList) {
            //     Debug.Log(playerTransform);
            // }
        }*/
    }


    // private ulong CalculatePlayerDistances() {
    //     
    // }


    /*[ServerRpc]
    private void GetPlayersServerRpc(ulong clientIdParam) {
        // Debug.Log($"Am Server: {IsServer}, Am Client:{IsClient}, Am Owner{IsOwner}, Am Owned by Server {IsOwnedByServer}, Executing from ID: {clientId}");
        var client = NetworkManager.Singleton.ConnectedClients[clientIdParam];


        // client.PlayerObject.transform.GetComponent<>()= Vector3.zero;
        // var clientTransform = client.PlayerObject.transform;
        // Debug.Log($"{clientTransform}");
        // List<Transform> playersList = new List<Transform>();
        // foreach (var connectedClient in NetworkManager.Singleton.ConnectedClients) {
        //     playersList.Add(connectedClient.Value.PlayerObject.transform);
        // }
        // return playersList;
    }*/

    // private void GetInfoFromServer(Transform playerTransform) {
    //     
    // }


    private void OnTriggerEnter(Collider other) {
        if(!IsOwner) return;
        if(!other.CompareTag("Player")) return;
        var otherDude = other.transform.GetComponentInParent<NetworkObject>();
        if (otherDude != null) {
            Debug.Log($"{otherDude.OwnerClientId}");
            // Call the server RPC to notify the other client;
            NotifyClientServerRpc(otherDude.OwnerClientId);

        }
        else {
            Debug.Log($"Did not get the NetworkObject off of: {other.name}");
        }
        
        
    }


    [ServerRpc]
    private void NotifyClientServerRpc(ulong targetClientId, ServerRpcParams rpcParams = default) {
        if (!IsServer) return;
        var senderClientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"Notifying Client: {targetClientId}");

        ClientRpcParams clientParams = new ClientRpcParams() {
            Send = new ClientRpcSendParams() {
                TargetClientIds = new ulong[] { targetClientId }
            }
        };

        ReceiveChallengeRequestClientRpc($"{senderClientId} says you suck... :/", senderClientId, clientParams);
    }



    [ClientRpc]
    private void ReceiveChallengeRequestClientRpc(string message, ulong initiatorClientId, ClientRpcParams rpcParams = default) {
        // Display the UI and based on whether user clicks on the accept button or the timer runs out first; accept or reject the idea
        
    }



    public override void OnNetworkSpawn() {
        if (IsServer) {
            transform.position = new Vector3(5, 3, 10);
        }
    }
}
