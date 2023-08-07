using System;
using IngameDebugConsole;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace _Scripts.Test_Scripts {
	public class RelayTest : MonoBehaviour {

		private string playerName = "User";

		private void Awake() {
			playerName += DateTime.Now.Second.ToString();
		}

		/*private void OnEnable() {
			
		}*/

		private void Start() {
			Authenticate(playerName);
		}

		private async void InitializeUnityService(string plName) {
			InitializationOptions initializationOptions = new InitializationOptions();
			initializationOptions.SetProfile(plName);
			try {
				await UnityServices.InitializeAsync(initializationOptions);
			}
			catch (ServicesInitializationException e) {
				Debug.Log(e);
			}
		}

		private async void Authenticate(string plName) {
			Debug.Log("Authentication Initiated");
			playerName = plName;
			InitializeUnityService(playerName);

			AuthenticationService.Instance.SignedIn += OnUserSignIn;

			await AuthenticationService.Instance.SignInAnonymouslyAsync();


			Debug.Log("Authentication Complete");
		}

		private void OnUserSignIn() {
			//
			Debug.Log($"User has signed in with PlayerId: {AuthenticationService.Instance.PlayerId}");
			AuthenticationService.Instance.SignedIn -= OnUserSignIn;
		}


		[ConsoleMethod("CreateRelay", "Creates a new Relay and prints it's join code")]
		public static async void CreateRelay() {
			try {
				Allocation allocation = await RelayService.Instance.CreateAllocationAsync(10);

				string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

				Debug.Log("Relay Created with join code: " + joinCode);

				RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

				NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

				NetworkManager.Singleton.StartServer();
			}
			catch (RelayServiceException e) {
				Debug.Log(e);
			}
		}


		[ConsoleMethod("JoinLobby", "Join a lobby by providing a join code")]
		public static async void JoinLobby(string joinCode) {
			try {
				Debug.Log($"Joining relay with: {joinCode}");
				JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

				RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

				NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

				NetworkManager.Singleton.StartClient();

			}
			catch (RelayServiceException e) {
				Debug.Log(e);
			}
		}
	}
}