using System;
using Unity.Services.Core;
using UnityEngine;

namespace _Scripts.Managers {
    public class UnityServicesManager : MonoBehaviour {
        #region Variables

        public static UnityServicesManager Instance { get; private set; }
        
        public bool ServiceInitialized { get; private set; }

        #endregion


        #region Custom Events

        public delegate void NetworkStatus(NetworkReachability networkReachability);

        public static NetworkStatus OnNetworkCheck;

        #endregion
        
        
        #region Builtin Methods

        private void Awake() {
            if (Instance is null) Instance = this;
            else {
                Destroy(gameObject);
                Debug.LogWarning("Should not have two Unity Service Managers!!!");
                
                    ....
                        /// Implement the new scene and set up a proper main menu and game sequence
            }


            InitializeServices();
        }

        #endregion



        #region Custom Methods

        public async void InitializeServices() {
            if (Application.internetReachability != NetworkReachability.NotReachable) {
                try {
                    await UnityServices.InitializeAsync();
                }
                catch (Exception e) {
                    Debug.LogWarning(e);
                }
            }
            else {
                OnNetworkCheck?.Invoke(NetworkReachability.NotReachable);
            }
        }

        #endregion
    }
}