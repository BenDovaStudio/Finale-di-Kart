using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace _Scripts {
    public class PlayerNameCard : NetworkBehaviour {
        [SerializeField] private Transform lookAtTarget;

        [SerializeField] private LookAtConstraint lookAtConstraintComponent;

        [SerializeField] private Transform theCard;

        [SerializeField] private Image cardImage;
        [SerializeField] private SpriteRenderer cardSprite;


        public override void OnNetworkSpawn() {
            if (IsServer) {
            
            }
        }
    }
}