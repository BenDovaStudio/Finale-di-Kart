using System;
using _Scripts.Controllers;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace _Scripts.Managers
{
    public class UnityServicesManager : MonoBehaviour
    {
        #region Variables

        [SerializeField] private NetworkController networkController;

        public static UnityServicesManager Instance { get; private set; }

        public bool ServiceInitialized { get; private set; }

        #endregion


        #region Custom Events

        public delegate void NetworkStatus(NetworkReachability networkReachability);

        public static NetworkStatus OnNetworkCheck;

        private string playerName = "User";

        private InitializationOptions initializationOptions;


        #endregion


        #region Builtin Methods

        private void Awake()
        {
            playerName += DateTime.Now.Second.ToString();
            initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(playerName);

            if (Instance is null) Instance = this;
            else
            {
                Destroy(gameObject);
                Debug.LogWarning("Should not have two Unity Service Managers!!!");
            }


            InitializeServices();
        }

        #endregion



        #region Custom Methods

        private async void InitializeServices()
        {

            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                try
                {
                    await UnityServices.InitializeAsync(initializationOptions);
                    AuthenticateUser();
                }
                catch (ServicesInitializationException e)
                {
                    Debug.LogWarning(e);
                }
            }
            else
            {
                OnNetworkCheck?.Invoke(NetworkReachability.NotReachable);
            }
        }


        private async void AuthenticateUser()
        {
            AuthenticationService.Instance.SignedIn += OnUserSignIn;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private void OnUserSignIn()
        {
            Debug.Log($"User has signed in with PlayerId: {AuthenticationService.Instance.PlayerId}");
            InitNetworkController();
            AuthenticationService.Instance.SignedIn -= OnUserSignIn;
        }

        private void InitNetworkController() {
            if (networkController == null) return;
            Debug.Log("Network Controller Instantiated");
            Instantiate(networkController);
        }

        #endregion
    }
}