using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Scripts.UI;
using DG.Tweening;
using Unity.Multiplayer.Tools.NetStatsMonitor;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace _Scripts.Controllers {



	public class GameUIController : MonoBehaviour {
		#region Variables

		public static GameUIController Instance;

		#region Profiling

		[SerializeField] private RuntimeNetStatsMonitor networkStatsMonitor; 

		#endregion


		#region ServerList

		[Header("ServerList")] [SerializeField]
		private GameObject serverListParent;

		[SerializeField] private Transform serverListTransform;

		[SerializeField] private ServerListElement serverListElementPrefab;

		// [Space] 
		[SerializeField] private Transform createServerButtonTransform;
		// [Space]


		private static List<ServerListElement> _serverList = new List<ServerListElement>();


		private float lastUpdateTime;
		private bool canUpdateLobby = true;
		[SerializeField] private bool shouldUpdateLobby = true;

		[SerializeField] private float lobbyQueryCooldown = 2.5f;

		#endregion


		#region GameLoop

		[Space] [Header("Game Loop")] [SerializeField]
		private Transform playerButtonGrpTransform;

		[FormerlySerializedAs("serverButtonsTransform")] [SerializeField]
		private Transform serverButtonGrpTransform;

		#endregion


		#region ChallengeUI
		
		[Space] [Header("Challenge Section")]

		[SerializeField] private RectTransform promptTransform;


		[SerializeField] private Image fillBar;
		[SerializeField] private Transform basicPrompt;
		[SerializeField] private Transform waitingPrompt;
		[SerializeField] private Transform affirmationPrompt;

		#endregion



		#region Race UI

		[SerializeField] private Transform winContainer;
		[SerializeField] private Transform loseContainer;

		#endregion

		#endregion


		#region Builtin Methods

		private void Awake() {
			if (Instance != null) {
				Debug.Log($"{gameObject.name}: Please no double GameUI controller");
				Destroy(gameObject);
			}
			else {
				Instance = this;
			}
		}

		private void OnEnable() {
			SetDefaultUI();
		}

		private void Update() {
			UpdateServerList();
			if (Input.GetKeyDown(KeyCode.F8)) {
				Debug.Log("KeyPressed");
				networkStatsMonitor.Visible = !networkStatsMonitor.Visible;
			}
		}

		#endregion


		#region Custom Methods

		public void SetDefaultUI() {
			// if (createServerButtonTransform) createServerButtonTransform.gameObject.SetActive(IsServer);
			if (serverListTransform) serverListTransform.gameObject.SetActive(true);
			if (serverButtonGrpTransform) serverButtonGrpTransform.gameObject.SetActive(false);
			if (playerButtonGrpTransform) playerButtonGrpTransform.gameObject.SetActive(false);
			if (promptTransform) promptTransform.gameObject.SetActive(false);
		}

		public void SetLobbyUpdate(bool shouldUpdate) {
			shouldUpdateLobby = shouldUpdate;
		}

		private async Task UpdateLobbyList() {
			lastUpdateTime = Time.unscaledTime;
			QueryResponse response = await NetworkController.QueryLobbies();
			foreach (ServerListElement serverListElem in _serverList) {
				Destroy(serverListElem.gameObject);
			}

			_serverList.Clear();
			if (response != null) {
				var resultList = response.Results;
				foreach (Lobby lobby in resultList) {
					ServerListElement tempHolder = Instantiate(serverListElementPrefab, serverListParent.transform);
					tempHolder.transform.SetParent(serverListParent.transform);
					tempHolder.UpdateValues(lobby.Name, lobby.HostId, lobby.Players.Count, lobby.MaxPlayers, lobby);
					_serverList.Add(tempHolder);
				}
			}
		}

		private async void UpdateServerList() {
			if (serverListTransform.gameObject.activeInHierarchy && canUpdateLobby && shouldUpdateLobby) {
				if (Time.unscaledTime - lastUpdateTime > lobbyQueryCooldown) {
					canUpdateLobby = false;
					await UpdateLobbyList();
					lastUpdateTime = Time.unscaledTime;
					canUpdateLobby = true;
				}
			}
		}

		public void ResetHighlight() {
			foreach (var serverListElement in _serverList) {
				serverListElement.SetHighlight(false);
			}
		}

		public void HandleServerStartUI() {
			if (serverListTransform) serverListTransform.gameObject.SetActive(false);
			if (serverButtonGrpTransform) serverButtonGrpTransform.gameObject.SetActive(true);
		}

		public void HandleServerStopUI() {
			if (serverButtonGrpTransform) serverButtonGrpTransform.gameObject.SetActive(false);
			if (serverListTransform) serverListTransform.gameObject.SetActive(true);
		}

		public void HandlePlayerStartUI() {
			if (serverListTransform) serverListTransform.gameObject.SetActive(false);
			if (playerButtonGrpTransform) playerButtonGrpTransform.gameObject.SetActive(true);
		}

		public void HandlePlayerEndUI() {
			if (playerButtonGrpTransform) playerButtonGrpTransform.gameObject.SetActive(false);
			if (serverListTransform) serverListTransform.gameObject.SetActive(true);
		}


		public void EnableCountdownFillBar(float time) {
			fillBar.gameObject.SetActive(true);
			StartCoroutine(FillBarCR(fillBar, time));
		}

		public void PromptInitiateRace(bool setActive = true) {
			promptTransform.gameObject.SetActive(setActive);
			ResetPrompt();
			basicPrompt.gameObject.SetActive(true);
		}

		public void DisablePrompt() {
			promptTransform.gameObject.SetActive(false);
		}
		public void PromptWaiting(float time) {
			promptTransform.gameObject.SetActive(true);
			ResetPrompt();
			EnableCountdownFillBar(time);
			waitingPrompt.gameObject.SetActive(true);
		}
		public void PromptAffirmation(float time) {
			promptTransform.gameObject.SetActive(true);
			ResetPrompt();
			EnableCountdownFillBar(time);
			affirmationPrompt.gameObject.SetActive(true);
		}
		

		private void ResetPrompt() {
			basicPrompt.gameObject.SetActive(false);
			affirmationPrompt.gameObject.SetActive(false);
			waitingPrompt.gameObject.SetActive(false);
			fillBar.gameObject.SetActive(false);
		}

		public void PromptDeactivateWhole() {
			promptTransform.gameObject.SetActive(false);
		}



		public void DisplayWinMessage() {
			winContainer.gameObject.SetActive(true);
			DOVirtual.DelayedCall(3f, () => {
				winContainer.gameObject.SetActive(false);
			});
		}
		public void DisplayLoseMessage() {
			loseContainer.gameObject.SetActive(true);
			DOVirtual.DelayedCall(3f, () => {
				loseContainer.gameObject.SetActive(false);
			});
		}

	#endregion



	#region Coroutines

	private IEnumerator FillBarCR(Image image, float duration) {
		image.fillMethod = Image.FillMethod.Horizontal;
		image.fillAmount = 1;

		float timeElapsed = 0;

		while (timeElapsed < duration) {
			var t = timeElapsed / duration;
			image.fillAmount = Mathf.Lerp(1, 0, t);
			timeElapsed += Time.unscaledDeltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	#endregion
	
	
	
	
	}
}