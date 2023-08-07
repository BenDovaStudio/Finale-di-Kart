using System;
using System.Collections.Generic;
using _Scripts.Test_Scripts;
using _Scripts.UI;
using IngameDebugConsole;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace _Scripts.Controllers {
	
	
	
	public class GameUIController : MonoBehaviour {
		#region Serialized Data Members

		[SerializeField]
		private GameObject ServerListParent;

		[SerializeField]
		private ServerListElement serverListElementPrefab;

		#endregion


		#region Variables

		private static List<ServerListElement> _serverList = new List<ServerListElement>();

		#endregion



		#region Builtin Methods

		private void Start() {
			InvokeRepeating(nameof(UpdateServerList), 3f, 3f);
		}

		#endregion


		#region MyRegion

		public async void UpdateServerList() {
			QueryResponse response = await LobbyTest.QueryLobbies();
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
					ServerListElement tempHolder = Instantiate(serverListElementPrefab, ServerListParent.transform);
					tempHolder.transform.SetParent(ServerListParent.transform);
					tempHolder.UpdateValues(lobby.Name, lobby.HostId, lobby.Players.Count, lobby.MaxPlayers);
					_serverList.Add(tempHolder);
				}
			}
		}

		#endregion
	}
}