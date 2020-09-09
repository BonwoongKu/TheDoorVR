using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameManagers
{
    public class GameLoop : SingletoneMono<GameLoop>
    {
        public static event Action onUpdateStandby;
        public static event Action onUpdatePrologue;
        public static event Action onUpdatePlay;
        public static event Action onUpdatePause;
        public static event Action onUpdateFinish;

        private static event Action _onUpdate;
        public static event Action onUpdate { add { _onUpdate -= value; _onUpdate += value; } remove { _onUpdate -= value; } }
        public static event Action onLateUpdate;
        public static event Action onFixedUpdate;

        private Dictionary<eGameState, Action> m_GameStateUpdate;

        private eGameState m_eCurrentState;
        static GameLoop()
        {
            CreateInstance();
        }
        protected override void Awake()
        {
            base.Awake();
            GameState.onChangeGameState += GameState_onChangeGameState;

            m_eCurrentState = GameState.Instance.CurrentGameState;
            m_GameStateUpdate = new Dictionary<eGameState, Action>(new Compares.GameStateComparer())
            {
                { eGameState.Standby,   new Action( ()=> { onUpdateStandby?.Invoke(); } ) },
                { eGameState.Prologue,  new Action( ()=> { onUpdatePrologue?.Invoke(); } ) },
                { eGameState.Play,      new Action( ()=> { onUpdatePlay?.Invoke(); } )   },
                { eGameState.Pause,     new Action( ()=> { onUpdatePause?.Invoke(); } )   },
                { eGameState.Finish,    new Action( ()=> { onUpdateFinish?.Invoke(); } )       },
            };
        }
        private void GameState_onChangeGameState(eGameState _prevState, eGameState _currentState)
        {
            m_eCurrentState = _currentState;
        }
        public static void DebugCall()
        {
            MyUtility.InvokeMethoName(onUpdateStandby);
            MyUtility.InvokeMethoName(onUpdatePause);
            MyUtility.InvokeMethoName(onUpdatePrologue);
            MyUtility.InvokeMethoName(onUpdatePlay);
            MyUtility.InvokeMethoName(onUpdateFinish);
            MyUtility.InvokeMethoName(_onUpdate);
            MyUtility.InvokeMethoName(onLateUpdate);
            MyUtility.InvokeMethoName(onFixedUpdate);
        }
        private void Update()
        {
            _onUpdate?.Invoke();

            m_GameStateUpdate[m_eCurrentState]();
        }
        private void LateUpdate()
        {
            onLateUpdate?.Invoke();
        }
        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke();
        }
    }

    
}

