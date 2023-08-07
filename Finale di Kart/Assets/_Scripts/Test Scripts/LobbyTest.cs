using System.Collections.Generic;
using System.Threading.Tasks;
using IngameDebugConsole;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace _Scripts.Test_Scripts {
	public class LobbyTest : MonoBehaviour {

		#region Variables

		public static LobbyTest Singleton {
			private set;
			get;
		}

		#endregion


		#region Custom Methods

		#region Console Methods

		[ConsoleMethod("Master_CreateLobby", "Creates a Lobby with specified number of players")]
		public static async void CreateLobby(string lobbyName, int maxPlayersExclusiveOfSelf) {
			try {
				Lobby lobby =
					await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayersExclusiveOfSelf);
			}
			catch (LobbyServiceException e) {
				Debug.Log(e);
			}
		}
		
		
		// public async void CreateLobby

		#endregion


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
		

		#endregion
	}
}