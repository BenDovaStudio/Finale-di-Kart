using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace _Scripts {
    public class PlayerNameCard : NetworkBehaviour {
        [SerializeField] private Transform lookAtTarget;

        [SerializeField] private LookAtConstraint lookAtConstraintComponent;

        [SerializeField] private Transform theCard;

        // [SerializeField] private Image cardImage;
        [SerializeField] private SpriteRenderer cardSprite;


        [SerializeField] private TextMeshPro playerNameText;

        private NetworkVariable<PlayerNameNv> playerName = new NetworkVariable<PlayerNameNv>();
        private bool playerNameSet = false;


        public override void OnNetworkSpawn() {
            if (Camera.main) {
                lookAtTarget = Camera.main.transform;
                ConstraintSource source = new ConstraintSource();
                source.sourceTransform = lookAtTarget;
                source.weight = 1;
                lookAtConstraintComponent.AddSource(source);
                lookAtConstraintComponent.constraintActive = true;
            }
            if (IsServer) {
                playerName.Value = $"Player {OwnerClientId}";
            }
        }

        private void SetPlayerName() {
            playerNameText.text = playerName.Value;
        }


        private void Update() {
            if (!playerNameSet && !string.IsNullOrEmpty(playerName.Value)) {
                SetPlayerName();
            }
        }
    }
}