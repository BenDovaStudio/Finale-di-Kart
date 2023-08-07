using System.Threading.Tasks;
using _Scripts.Prompts;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.NetCode {
	public class NetworkNodeTypeHandler : MonoBehaviour {
		[SerializeField]
		private Button clientBtn;

		[SerializeField]
		private Button hostBtn;

		[SerializeField]
		private Button serverBtn;


		[SerializeField]
		private Button stopInstanceButton;


		[SerializeField]
		private TextMeshProUGUI instanceLabel;


		private void Awake() {
			if (clientBtn) clientBtn.onClick.AddListener(OnCreateClient);
			if (hostBtn) hostBtn.onClick.AddListener(OnCreateHost);
			if (serverBtn) serverBtn.onClick.AddListener(OnCreateServer);

			if (stopInstanceButton)
				stopInstanceButton.onClick.AddListener(() => {
					NetworkManager.Singleton.Shutdown();
					if (instanceLabel) instanceLabel.text = "Waiting to Initialize";
					SetInitializationButtons(true);
				});
		}

		private void SetInitializationButtons(bool active) {
			if (clientBtn) clientBtn.gameObject.SetActive(active);
			if (hostBtn) hostBtn.gameObject.SetActive(active);
			if (serverBtn) serverBtn.gameObject.SetActive(active);

			if (stopInstanceButton) stopInstanceButton.gameObject.SetActive(!active);
		}

		private async Task<string> CreateRelay(int maxPlayersExclusiveOfSelf) {
			string joinCode = null;
			try {
				Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayersExclusiveOfSelf);
				joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
				SetServerData(allocation);
			}
			catch (RelayServiceException e) {
				Debug.Log(e);
			}

			return joinCode;
		}

		private void SetServerData(Allocation allocation) {
			RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

			if (NetworkManager.Singleton.TryGetComponent(out UnityTransport unityTransport)) {
				unityTransport.SetRelayServerData(relayServerData);
			}
		}

		private void SetServerData(JoinAllocation allocation) {
			RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

			if (NetworkManager.Singleton.TryGetComponent(out UnityTransport unityTransport)) {
				unityTransport.SetRelayServerData(relayServerData);
			}
		}

		private async void OnCreateServer() {
			// NetworkManager.Singleton.StartServer();
			string joinCode = await CreateRelay(10);
			if (joinCode is null) return;
			NetworkManager.Singleton.StartServer();

			if (instanceLabel) instanceLabel.text = $"Acting as Server. Join code: {joinCode}";
			SetInitializationButtons(false);

		}

		private async void OnCreateHost() {
			// NetworkManager.Singleton.StartHost();
			string joinCode = await CreateRelay(9);
			if (joinCode is null) return;
			NetworkManager.Singleton.StartHost();


			if (instanceLabel) instanceLabel.text = $"Acting as Host. Join code: {joinCode}";
			SetInitializationButtons(false);
		}

		private void OnCreateClient() {
			/*RelayServerData relayServerData = new RelayServerData();
			if (NetworkManager.Singleton.TryGetComponent(out UnityTransport unityTransport)) {
				unityTransport.SetRelayServerData(relayServerData);
			}

			NetworkManager.Singleton.StartClient();*/

			JoinCodePrompt.Singleton.OnTextCompleted += OnTextEntered;
			JoinCodePrompt.Singleton.GetInput();

		}


		private async void OnTextEntered(string text) {
			try {
				Debug.Log($"Entered Text: {text}");
				JoinCodePrompt.Singleton.OnTextCompleted -= OnTextEntered;

				JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(text);

				// RelayServerData serverData = new RelayServerData(joinAllocation, "dtls");

				SetServerData(joinAllocation);

				NetworkManager.Singleton.StartClient();

				if (instanceLabel) instanceLabel.text = "Acting as Client";
				SetInitializationButtons(false);
				if (stopInstanceButton) stopInstanceButton.gameObject.SetActive(true);
			}
			catch (RelayServiceException e) {
				Debug.Log(e);
			}
		}
	}
}