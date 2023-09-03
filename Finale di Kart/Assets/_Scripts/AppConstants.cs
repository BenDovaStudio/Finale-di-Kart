using UnityEngine;

namespace _Scripts
{
    public static class AppConstants
    {
        public const string RelayCode = "RelayCode";
        
        public const float HeartbeatCooldown = 20;

        public const float SecretCodeCooldown = 1.2f;


        public const float ChallengeRequestTimeout = 10f;



        #region Custom Input Actions

        public const string InitiateChallenge = "InitiateChallenge";
        public const string AcceptChallenge = "AcceptChallenge";
        public const string RejectChallenge = "RejectChallenge";

        #endregion
    }
}