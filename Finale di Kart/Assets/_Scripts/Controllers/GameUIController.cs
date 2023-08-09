using System.Collections.Generic;
using _Scripts.UI;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Controllers {
	
	
	
	public class GameUIController : MonoBehaviour {
		#region Serialized Data Members

		[FormerlySerializedAs("ServerListParent")] [SerializeField]
		private GameObject serverListParent;

		[SerializeField]
		private ServerListElement serverListElementPrefab;

		#endregion


		#region Variables

		private static List<ServerListElement> _serverList = new List<ServerListElement>();

		#endregion



		#region Builtin Methods

		private void Start() {
			InvokeRepeating(nameof(UpdateServerList), 2f, 2f);
		}

		#endregion


		#region MyRegion

		public async void UpdateServerList() {
			QueryResponse response = await NetworkController.QueryLobbies();
			// Debug.Log($"Server List size: {_serverList.Count}");
			// bool firstElement = true;
			foreach (ServerListElement serverListElem in _serverList) {
				/*if (firstElement) {
					firstElement = false;
					continue;
				}*/
				// Debug.Log($"Deleting Element");
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

		#endregion
	}
}