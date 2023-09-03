using UnityEngine;

namespace _Scripts {
    public static class SystemInitializer {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Execute() => Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("Systems")));
    }
}