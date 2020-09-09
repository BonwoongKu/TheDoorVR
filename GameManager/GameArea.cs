using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameManagers
{
    public class GameArea : SingletoneMono<GameArea>
    {
        public bool IsMyArea(ePlayArea _playArea) { return IsIncludedArea(myPlayArea, _playArea); }
        public bool IsMyAreaIsA { get {  return IsIncludedArea(ePlayArea.A, myPlayArea); } }
        public bool IsMyAreaIsB { get {  return IsIncludedArea(ePlayArea.B, myPlayArea); } }


        public bool IsIncludedArea(ePlayArea _lhs, ePlayArea _rhs)
        {
            return (_lhs & _rhs) != 0;
        }

        [SerializeField] private ePlayArea myPlayArea = ePlayArea.A;
        public ePlayArea MyPlayArea => myPlayArea;

        public void SelectPlayArea(ePlayArea _selectPlayArea)
        {
            myPlayArea = _selectPlayArea;
        }

        protected override void Awake()
        {
            base.Awake();
            if (GameConfig.Instance.Play.playArea == "a" || GameConfig.Instance.Play.playArea == "A")
            {
                myPlayArea = ePlayArea.A;
            }
            else if (GameConfig.Instance.Play.playArea == "b" || GameConfig.Instance.Play.playArea == "B")
            {
                myPlayArea = ePlayArea.B;
            }
        }


        
    }
}
