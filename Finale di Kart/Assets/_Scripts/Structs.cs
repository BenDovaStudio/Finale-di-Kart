using System;
using System.Dynamic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

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


    [Serializable]
    public struct Duel {
        [SerializeField] private int id;
        [SerializeField] private ulong challenger;
        [SerializeField] private ulong target;
        [SerializeField] private int duelTrackIndex;
        public Coroutine TimerRoutine;

        public Duel(ulong challengerId, ulong targetId) {
            challenger = challengerId;
            target = targetId;
            id = (int)(challengerId + targetId);
            TimerRoutine = null;
            duelTrackIndex = -1;
        }

        public Duel(ulong challengerId, ulong targetId, Coroutine timerRoutine) {
            challenger = challengerId;
            target = targetId;
            id = (int)(challengerId + targetId);
            TimerRoutine = timerRoutine;
            duelTrackIndex = -1;
        }

        public int GetDuelId() {
            return id;
        }

        public ulong GetChallenger() {
            return challenger;
        }

        public ulong GetTarget() {
            return target;
        }

        public int GetTrackIndex() {
            return duelTrackIndex;
        }

        #region Overrides for equality comparison

        public override bool Equals(object obj) {
            if (obj is Duel other) {
                return id == other.GetDuelId();
            }

            return false;
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }

        #endregion
    }
}