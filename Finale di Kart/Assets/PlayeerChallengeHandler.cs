using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayeerChallengeHandler : MonoBehaviour {
    [SerializeField] private List<Transform> players;


    private void Start() {
        InvokeRepeating();
    }




    private ulong CalculatePlayerDistances() {
        
    }


    [ServerRpc]
    private List<Transform> GetPlayersServerRpc() {
        // NetworkManager.Singleton.ConnectedClients.
    }
}
