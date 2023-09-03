using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Utilities {
    [Serializable]
    public class ChallengePairs {
        [SerializeField] private List<Duel> duelList = new();

        public bool HasPair(ulong challenger, ulong target) {
            Duel duelPair = new Duel(challenger, target);
            int id = duelPair.GetDuelId();
            foreach (var duel in duelList) {
                if (duel.Equals(duelPair)) {
                    return true;
                }
            }
            return false;
        }

        public void AddPair(ulong challengerId, ulong targetId) {
            duelList.Add(new Duel(challengerId, targetId));
        }
        public void AddPair(ulong challengerId, ulong targetId, Coroutine timerRoutine) {
            duelList.Add(new Duel(challengerId, targetId, timerRoutine));
        }

        public void RemovePair(ulong challengerId, ulong targetId) {
            duelList.Remove(new Duel(challengerId, targetId));
        }

        public void RemovePair(Duel duel) {
            duelList.Remove(duel);
        }

        public void RemovePairAt(int index) {
            duelList.RemoveAt(index);
        }

        public int GetPairIndex(Duel duelPair) {
            return duelList.IndexOf(duelPair);
        }

        public Coroutine GetTimerRoutine(ulong challengerId, ulong targetId) {
            var duelPair = new Duel(challengerId, targetId);
            var index = duelList.IndexOf(duelPair);
            return duelList[index].TimerRoutine;
        }
    }
}