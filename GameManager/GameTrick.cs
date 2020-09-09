using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameManagers
{
    public class GameTrick : SingletoneMono<GameTrick>
    {
        public static event System.Action<eTrick> onChangeTrick;
        public static event System.Action<eTrick> onFinishTrick;

        private eTrick m_StartTrick = eTrick.None;
        private eTrick m_CurrentTrick;
        public eTrick CurrentTrick { get { return m_CurrentTrick; } }

        private int m_iCurrentTrickIntValue;
        public int CurrentTrickIntValue { get { return m_iCurrentTrickIntValue; } }

        private readonly static eTrick[] Tricks;

        static GameTrick()
        {
            Tricks = (eTrick[])System.Enum.GetValues(typeof(eTrick));
        }
        public static eTrick[] GetTricksFromMaskValue(eTrick _usingTrick)
        {
            List<eTrick> trick = new List<eTrick>();

            for (int i = 0; i < Tricks.Length; i++)
            {
                if( (_usingTrick & Tricks[i]) != 0)
                    trick.Add(Tricks[i]);
            }

            return trick.ToArray();
        }

        public static void DebugCall()
        {
            MyUtility.InvokeMethoName(onChangeTrick);
            MyUtility.InvokeMethoName(onFinishTrick);
        }
        public GameTrick()
        {
            m_CurrentTrick = m_StartTrick;
            m_iCurrentTrickIntValue = (int)m_CurrentTrick;
        }
        public void ResetTrick()
        {
            m_CurrentTrick = m_StartTrick;
            m_iCurrentTrickIntValue = (int)m_CurrentTrick;
        }

        public void FinishTrick(eTrick _trickType)
        {
            if (onFinishTrick != null)
                onFinishTrick(_trickType);
        }
        public void ChangeTrick(eTrick _trickType)
        {
            m_CurrentTrick = _trickType;
            m_iCurrentTrickIntValue = (int)m_CurrentTrick;
            if (onChangeTrick != null)
                onChangeTrick(_trickType);
        }
    }
}
