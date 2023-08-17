using UnityEngine;

namespace _Scripts.Controllers {
	public class GameController : MonoBehaviour {


		public static GameController Instance;
		
		
		[SerializeField]
		private Transform playerVehicle;

		[SerializeField]
		private RCC_CarControllerV3 rccComponent;


		[SerializeField]
		private RCC_Camera rccCamera;


		private void Awake() {
			if (Instance != null) Destroy(gameObject);
			else Instance = this;
		}

		private void OnEnable() {
			RCC_CarControllerV3.OnCarSpawned += OnCarSpawn;
		}

		private void OnDisable() {
			RCC_CarControllerV3.OnCarSpawned -= OnCarSpawn;
		}

		private void OnCarSpawn(RCC_CarControllerV3 rccController, bool isOwner) {
			// if (!isOwner) return;
			rccCamera.cameraTarget.playerVehicle = rccController;
			rccComponent = rccController;
			playerVehicle = rccController.transform;
		}
	}
}