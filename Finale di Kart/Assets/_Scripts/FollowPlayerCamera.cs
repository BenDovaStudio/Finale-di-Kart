using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour {
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform mainDirectionalLightTransform;
    [SerializeField] private Transform sunObjectTransform;

    private void Update() {
        var playerCameraPos = playerCameraTransform.position;
        var selfTransform = transform;
        var selfPos = selfTransform.position;

        Vector3 newPos = new Vector3(playerCameraPos.x, selfPos.y, playerCameraPos.z);


        selfTransform.position = newPos; // setting horizontal position of the sky-dome to player's camera's position. y axis is locked to initial
        sunObjectTransform.rotation = mainDirectionalLightTransform.rotation; // aligning sun to the directional light
    }
}