using System.Collections.Generic;
using System.Threading.Tasks;
using _Scripts.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace _Scripts.Controllers {
	public class NetworkController : MonoBehaviour {
		#region Variables

		public static NetworkController Instance { private set; get; }

		public NodeType nodeType = NodeType.None;

		private string relayJoinCode = "";

		private Lobby createdLobby;
		private Lobby selectedLobby;
		private Lobby joinedLobby;
		
		private float heartbeatTimer;
		private readonly float heartbeatCooldown = AppConstants.HeartbeatCooldown;


		private readonly KeyCode[] serverSecretCode = {
			KeyCode.C, KeyCode.R, KeyCode.E, KeyCode.A, KeyCode.T, KeyCode.E, KeyCode.S, KeyCode.E, KeyCode.R, KeyCode.V, KeyCode.E, KeyCode.R
		};

		private List<KeyCode> enteredCode = new List<KeyCode>();
		private readonly float secretCodeCooldown = AppConstants.SecretCodeCooldown;
		private float lastKeyPressTime;

		#endregion

		
		#region Builtin Methods

		private void Awake() {
			if (Instance is null) Instance = this;
			else Destroy(gameObject);

			
		}

		private void OnEnable() {
			ServerListElement.OnLobbySelect += OnLobbySelected;
			NetworkManager.Singleton.OnServerStopped += OnServerDisconnect;
			NetworkManager.Singleton.OnClientStopped += OnClientDisconnect;
		}

		private void OnDisable() {
			ServerListElement.OnLobbySelect -= OnLobbySelected;
			NetworkManager.Singleton.OnServerStopped -= OnServerDisconnect;
			NetworkManager.Singleton.OnClientStopped -= OnClientDisconnect;
		}

		private void Start() {
			heartbeatTimer = 0;
		}
        
		private void Update() {
			// LobbyHeartbeat();
			HandleLobbyHeartbeat();
			HandleSecretCode();
		}

		#endregion

		
		#region Custom Methods

		public void CreateServer() {
			GameUIController.Instance.HandleServerStartUI();
			CreateRelay();
		}
        
		private async void JoinServer(Lobby passedLobby) {
			try {
				passedLobby = await LobbyService.Instance.GetLobbyAsync(passedLobby.Id);
				Debug.Log($"Joining Lobby: {passedLobby.Name} by ID");
				joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(passedLobby.Id);

				JoinRelay(joinedLobby.Data[AppConstants.RelayCode].Value);

			}
			catch (LobbyServiceException e) {
				Debug.Log(e);
			}
		}

		private async void HandleLobbyHeartbeat() {
			if (createdLobby == null) return;
			heartbeatTimer -= Time.deltaTime;
			if (heartbeatTimer < 0) {
				heartbeatTimer = heartbeatCooldown;
				Debug.Log("Sending Heartbeat");
				await LobbyService.Instance.SendHeartbeatPingAsync(createdLobby.Id);
			}
		}

		private async void CreateRelay() {
			try {
				Allocation allocation = await RelayService.Instance.CreateAllocationAsync(10);

				relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
				Debug.Log("Relay Created with join code: " + relayJoinCode);
				RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
				NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
				NetworkManager.Singleton.StartServer();
				QueryResponse lobbyResponse = await QueryLobbies();
				int serverNumber = lobbyResponse.Results.Count;
				CreateLobby($"Server {serverNumber:D2}", 11);
			}
			catch (RelayServiceException e) {
				Debug.Log(e);
			}
		}

		private async void JoinRelay(string joinCode) {
			try {
				Debug.Log($"Joining relay with: {joinCode}");
				JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

				RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

				NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

				GameUIController.Instance.HandlePlayerStartUI();
				nodeType = NodeType.Client;
				NetworkManager.Singleton.StartClient();

			}
			catch (RelayServiceException e) {
				Debug.Log(e);
			}
		}

		private async void CreateLobby(string lobbyName, int maxPlayersInclusiveOfSelf) {
			try {
				var lobbyOptions = new CreateLobbyOptions() {
					IsPrivate = false,
					Data = new Dictionary<string, DataObject> {
						{
							AppConstants.RelayCode,
							new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)
						}
					}
				};
				nodeType = NodeType.Server;
				Debug.Log("Creating Lobby");
				createdLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayersInclusiveOfSelf, lobbyOptions);
			}
			catch (LobbyServiceException e) {
				Debug.Log(e);
			}
		}

		public static async Task<QueryResponse> QueryLobbies() {
			try {
				Debug.Log("Querying For Lobby");
				QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
				return queryResponse;
			}
			catch (LobbyServiceException e) {
				Debug.Log(e);
				return null;
			}
		}

		public static async Task<QueryResponse> QueryLobbies(QueryLobbiesOptions options) {
			try {
				Debug.Log("Querying Lobby with options");
				QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
				return queryResponse;
			}
			catch (LobbyServiceException e) {
				Debug.Log(e);
				return null;
			}
		}
        
		private void HandleSecretCode() {
			foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode))) {
				if (Input.GetKeyDown(keyCode)) {
					lastKeyPressTime = Time.time; // Update the last key press time

					enteredCode.Add(keyCode);

					if (enteredCode.Count > serverSecretCode.Length) {
						enteredCode.RemoveAt(0);
					}

					if (enteredCode.Count == serverSecretCode.Length) {
						CheckSecretCode();
					}
				}
			}

			// Check if the input has timed out
			if (Time.time - lastKeyPressTime > secretCodeCooldown) {
				enteredCode.Clear(); // Reset the input after timeout
			}
		}

		private void CheckSecretCode() {
			if (enteredCode.Count == serverSecretCode.Length) {
				// Debug.Log($"Code Entered: {enteredCode}");
				bool codeMatch = true;
				for (int i = 0; i < serverSecretCode.Length; i++) {
					if (enteredCode[i] != serverSecretCode[i]) {
						codeMatch = false;
						break;
					}

					Debug.Log($"match at index {i}");
				}

				if (codeMatch) {
					Debug.Log("Code Matched!");
					CreateServer();
				}
			}

			enteredCode.Clear();
		}

		public async void KillServer() {
			await DeleteLobby();
			NetworkManager.Singleton.Shutdown();
			GameUIController.Instance.HandleServerStopUI();
		}

		private async Task DeleteLobby() {
			if (createdLobby != null) {
				try {
					await LobbyService.Instance.DeleteLobbyAsync(createdLobby.Id);
					createdLobby = null;
				}
				catch (LobbyServiceException e) {
					Debug.Log(e);
				}
			}
		}

		public void ConnectToServer() {
			if (selectedLobby == null) {
				Debug.Log("No Server Selected");
				return;
			}
			JoinServer(selectedLobby);
		}

		public async void DisconnectFromServer() {
			try {
				string playerId = AuthenticationService.Instance.PlayerId;
				joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions());
				await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
				GameUIController.Instance.HandlePlayerEndUI();
			}
			catch (LobbyServiceException e) {
				Debug.Log(e);
			}
		}

		#endregion


		#region Custom Event Methods

		private async void OnApplicationQuit() {
			await DeleteLobby();
			if (nodeType != NodeType.None) {
				NetworkManager.Singleton.Shutdown();
			}
		}
		
		private void OnLobbySelected(Lobby lobby) {
			Debug.Log($"Server Selected: {lobby.Name} [{lobby.Id}]");
			selectedLobby = lobby;
		}

		private void OnNetworkInstanceShutdown() {
			GameUIController.Instance.SetDefaultUI();
		}

		private void OnServerDisconnect(bool value) {
			Debug.Log($"Server Disconnected : ({value})");
		}

		private void OnClientDisconnect(bool value) {
			Debug.Log($"Client Disconnected : ({value})");
		}

		#endregion
	}
}