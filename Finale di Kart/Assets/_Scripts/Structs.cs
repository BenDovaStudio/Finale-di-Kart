using Unity.Collections;
using Unity.Netcode;

namespace _Scripts {
    public struct PlayerNameNv : INetworkSerializable {
        private FixedString32Bytes playerNumber;


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref playerNumber);
        }

        public override string ToString() {
            return playerNumber.ToString();
        }
        
        
        // Some random methods that are copy-pasta :)
        public static implicit operator string(PlayerNameNv pn) => pn.ToString();
        public static implicit operator PlayerNameNv(string s) => new PlayerNameNv() {
            playerNumber = new FixedString32Bytes(s)
        };
    }
}