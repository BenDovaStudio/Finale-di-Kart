using System;
using UnityEngine;

namespace _Scripts {
	public class GameController : MonoBehaviour {


		private static GameController Singleton;
		
		
		[SerializeField]
		private Transform playerVehicle;

		[SerializeField]
		private RCC_CarControllerV3 rccComponent;


		[SerializeField]
		private RCC_Camera rccCamera;


		private void Awake() {
			if (Singleton != null) Destroy(gameObject);
			else Singleton = this;
		}

		private void OnEnable() {
			RCC_CarControllerV3.OnCarSpawned += OnCarSpawn;
		}

		private void OnDisable() {
			RCC_CarControllerV3.OnCarSpawned -= OnCarSpawn;
		}

		private void OnCarSpawn(RCC_CarControllerV3 rccController, bool isOwner) {
			if (!isOwner) return;
			rccCamera.cameraTarget.playerVehicle = rccController;
			rccComponent = rccController;
			playerVehicle = rccController.transform;
		}
	}
}