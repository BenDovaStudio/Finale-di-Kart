using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerNameCard : NetworkBehaviour {
    [SerializeField] private Transform lookAtTarget;

    [SerializeField] private LookAtConstraint lookAtConstraintComponent;

    [SerializeField] private Transform theCard;


    public override void OnNetworkSpawn() {
        if (IsServer) {
            
        }
        
    }
}