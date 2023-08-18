using Unity.Netcode;
using UnityEngine;

namespace _Scripts {
    public class Singleton<T> : NetworkBehaviour where T : Component {
        private static T _instance;

        public static T Instance {
            get {
                if (_instance == null) {
                    var objs = FindObjectsOfType(typeof(T)) as T[];
                    if (objs is { Length: > 0 }) {
                        _instance = objs[0];
                    }

                    if (objs is { Length: > 1 }) {
                        Debug.LogError("Theres of more than one " + typeof(T).Name + " in the scene.");
                    }

                    if (_instance == null) {
                        GameObject obj = new GameObject();
                        obj.name = $"_{typeof(T).Name}";
                        _instance = obj.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

    }
}