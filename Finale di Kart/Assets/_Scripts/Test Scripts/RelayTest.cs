using System;
using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Rendering;

public class RelayTest : MonoBehaviour {

	private string playerName = "User";
	private void Awake() {
		playerName += DateTime.Now.Second.ToString(); 
	}

	private void Start() {
		
		Authenticate(playerName);
	}

	private async void Authenticate(string pName) {
		Debug.Log("Authentication Initiated");
		playerName = pName;
		InitializationOptions initializationOptions = new InitializationOptions();
		initializationOptions.SetProfile(playerName);
		
		await UnityServices.InitializeAsync(initializationOptions);

		AuthenticationService.Instance.SignedIn += OnUserSignIn;
		
		await AuthenticationService.Instance.SignInAnonymouslyAsync();


		Debug.Log("Authentication Complete");
	}

	private void OnUserSignIn() {
		//
		AuthenticationService.Instance.SignedIn -= OnUserSignIn;
	}


	[ConsoleMethod("CreateRelay", "Creates a new Relay and prints it's join code")]
	public static async void CreateRelay() {
		try {
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(10);

			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			Debug.Log("Relay Created with join code: " + joinCode);
		}
		catch (RelayServiceException e) {
			Debug.Log(e);
		}
	}
}