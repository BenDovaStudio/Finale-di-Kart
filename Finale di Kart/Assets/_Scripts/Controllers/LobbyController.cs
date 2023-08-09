// using System;
// using System.Threading.Tasks;
// using IngameDebugConsole;
// using Unity.Services.Lobbies;
// using Unity.Services.Lobbies.Models;
// using UnityEngine;
//
// namespace _Scripts.Controllers {
// 	public class LobbyController : MonoBehaviour {
//
// 		#region Variables
//
// 		public static LobbyController Instance {
// 			private set;
// 			get;
// 		}
//
// 		#endregion
//
//
// 		#region Builtin Methods
//
// 		private void Awake()
// 		{
// 			if (Instance is null) Instance = this;
// 			else Destroy(gameObject);
// 		}
//
// 		#endregion
//
//
// 		#region Custom Methods
//
// 		#region Console Methods
//
// 		// [ConsoleMethod("Master_CreateLobby", "Creates a Lobby with specified number of players")]
//
// 		// public async void CreateLobby
//
// 		#endregion
//
//
// 		
// 		
//
// 		#endregion
// 	}
// }