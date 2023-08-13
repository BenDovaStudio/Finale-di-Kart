using System.Collections.Generic;
using System.Threading.Tasks;
using _Scripts.UI;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Controllers {



	public class GameUIController : MonoBehaviour {
		#region Variables

		public static GameUIController Instance;


		#region ServerList

		[Header("ServerList")] [SerializeField]
		private GameObject serverListParent;

		[SerializeField] private Transform serverListTransform;

		[SerializeField] private ServerListElement serverListElementPrefab;


		private static List<ServerListElement> _serverList = new List<ServerListElement>();


		private float lastUpdateTime;
		private bool canUpdateLobby = true;
		[SerializeField] private bool shouldUpdateLobby;

		[SerializeField] private float lobbyQueryCooldown = 2.5f;

		#endregion


		#region GameLoop

		[Space] [Header("Game Loop")] [SerializeField]
		private Transform playerButtonGrpTransform;

		[FormerlySerializedAs("serverButtonsTransform")] [SerializeField]
		private Transform serverButtonGrpTransform;

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
		}

		#endregion


		#region Custom Methods

		public void SetDefaultUI() {
			if (serverListTransform) serverListTransform.gameObject.SetActive(true);
			if (serverButtonGrpTransform) serverButtonGrpTransform.gameObject.SetActive(false);
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

		#endregion
	}
}