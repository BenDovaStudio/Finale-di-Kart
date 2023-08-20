using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TestRaycast : MonoBehaviour {
    [SerializeField] private LayerMask raycastLayerMask;

    // Update is called once per frame
    void Update() {
        Ray ray = new Ray(transform.position, transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit,2000f, raycastLayerMask)) {
            Debug.Log($"Hit on: {hit.collider.gameObject.name}");
        }
    }
}
