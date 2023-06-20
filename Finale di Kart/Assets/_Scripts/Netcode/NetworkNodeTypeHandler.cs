using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Netcode {
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
			if (clientBtn) clientBtn.onClick.AddListener(() => {
				NetworkManager.Singleton.StartClient();
				if (instanceLabel) instanceLabel.text = "Client";
				SetInitializationButtons(false);
				if(stopInstanceButton)stopInstanceButton.gameObject.SetActive(true);
			});
			if (hostBtn) hostBtn.onClick.AddListener(() => {
				NetworkManager.Singleton.StartHost();
				if (instanceLabel) instanceLabel.text = "Host";
				SetInitializationButtons(false);
				if(stopInstanceButton)stopInstanceButton.gameObject.SetActive(true);
			});
			if (serverBtn) serverBtn.onClick.AddListener(() => {
				NetworkManager.Singleton.StartServer();
				if (instanceLabel) instanceLabel.text = "Server";
				SetInitializationButtons(false);
				if(stopInstanceButton)stopInstanceButton.gameObject.SetActive(true);
			});

			if (stopInstanceButton) stopInstanceButton.onClick.AddListener(() => {
				NetworkManager.Singleton.Shutdown();
				if (instanceLabel) instanceLabel.text = "Waiting to Initialize";
				stopInstanceButton.gameObject.SetActive(false);
				SetInitializationButtons(true);
			});
		}

		private void SetInitializationButtons(bool active) {
			if (clientBtn) clientBtn.gameObject.SetActive(active);
			if (hostBtn) hostBtn.gameObject.SetActive(active);
			if (serverBtn) serverBtn.gameObject.SetActive(active);
		}
	}
}