using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace _Scripts.Managers
{
    public class UnityServicesManager : MonoBehaviour
    {
        #region Variables

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

        public async void InitializeServices()
        {

            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                try
                {
                    await UnityServices.InitializeAsync();
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
            AuthenticationService.Instance.SignedIn -= OnUserSignIn;
        }

        #endregion
    }
}