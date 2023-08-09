using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
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

		private Lobby serverLobby;
		private float heartbeatTimer;


		private const string ServerSecretCode = "createserver0110";
		private string enteredSecretCode = "";
		private float cooldown;

		#endregion

		#region Builtin Methods

		private void Awake() {
			if (Instance is null) Instance = this;
			else Destroy(gameObject);
		}

		private void Start() {
			heartbeatTimer = 0;
		}


		private void Update() {
			LobbyHeartbeat();
			HandleSecretCode();
		}

		#endregion

		#region Custom Methods


		private void CreateServer() {
			CreateRelay();
		}

		public async void JoinServer(Lobby passedLobby) {
			try {
				await LobbyService.Instance.JoinLobbyByIdAsync(passedLobby.Id);
				
				JoinRelay(passedLobby.Data[AppConstants.RelayCode].Value);
				
			}
			catch (LobbyServiceException e) {
				Debug.Log(e);
			}
		}

		private async void LobbyHeartbeat() {
			if (serverLobby == null) return;
			heartbeatTimer -= Time.deltaTime;
			if (heartbeatTimer < 0) {
				heartbeatTimer = AppConstants.HeartbeatCooldown;
				await LobbyService.Instance.SendHeartbeatPingAsync(serverLobby.Id);
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
				CreateLobby($"Server {serverNumber:D2}", 10);
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

				nodeType = NodeType.Client;
				NetworkManager.Singleton.StartClient();

			}
			catch (RelayServiceException e) {
				Debug.Log(e);
			}
		}
		private async void CreateLobby(string lobbyName, int maxPlayersExclusiveOfSelf) {
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
				serverLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayersExclusiveOfSelf, lobbyOptions);
			}
			catch (LobbyServiceException e) {
				Debug.Log(e);
			}
		}


		public static async Task<QueryResponse> QueryLobbies() {
			try {
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
				QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
				return queryResponse;
			}
			catch (LobbyServiceException e) {
				Debug.Log(e);
				return null;
			}
		}


		private void HandleSecretCode() {
			if (nodeType is not NodeType.None) return;
			if (enteredSecretCode == ServerSecretCode) {
				CreateServer();
				return;
			}
			cooldown -= Time.deltaTime;
			if (Input.GetKeyUp(KeyCode.C)) {
				ConcatCode('c');
			}
			else if (Input.GetKeyUp(KeyCode.R)) {
				ConcatCode('r');
			}
			else if (Input.GetKeyUp(KeyCode.E)) {
				ConcatCode('e');
			}
			else if (Input.GetKeyUp(KeyCode.A)) {
				ConcatCode('a');
			}
			else if (Input.GetKeyUp(KeyCode.T)) {
				ConcatCode('t');
			}
			else if (Input.GetKeyUp(KeyCode.S)) {
				ConcatCode('s');
			}
			else if (Input.GetKeyUp(KeyCode.V)) {
				ConcatCode('v');
			}
			else if (Input.GetKeyUp(KeyCode.Keypad0) || Input.GetKeyUp(KeyCode.Alpha0)) {
				ConcatCode('0');
			}
			else if (Input.GetKeyUp(KeyCode.Keypad1) || Input.GetKeyUp(KeyCode.Alpha1)) {
				ConcatCode('1');
			}
		}

		private void ConcatCode(char character) {
			if (cooldown > 0) {
				enteredSecretCode += character;
				cooldown = AppConstants.SecretCodeCooldown;
			}
			else enteredSecretCode = "";
		}

		#endregion
	}
}