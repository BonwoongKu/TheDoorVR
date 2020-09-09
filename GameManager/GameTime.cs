using System;
//using System.Timers;
using System.Collections;
using UnityEngine;

namespace GameManagers
{
    public class GameTime : SingletoneMono<GameTime>
    {
        private int m_iClientTime;
        private int m_iMinutes;
        private int m_iSeconds;

        public int ClientTime { get{ return m_iClientTime; } }
        public int Minutes { get { return m_iMinutes; } }
        public int Seconds { get { return m_iSeconds; } }

        private bool m_bIsTimeover;
        public bool IsTimeOver { get { return m_bIsTimeover; } set { m_bIsTimeover = value; } }

        private WaitForSeconds m_OneSecondWaiting;

        public static event Action onStartTimer;
        public static event Action<int> onTimerUpdate;

        private bool m_bIsPause = false;
        IEnumerator m_TimerProc;
        protected override void Awake()
        {
            base.Awake();
            m_iClientTime = GameConfig.Instance.Play.playTime;
            GameScene.onChangeScene_SelectServer += GameScene_onChangeScene_SelectServer;
        }

        private void GameScene_onChangeScene_SelectServer()
        {
            m_iClientTime = GameConfig.Instance.Play.playTime;
            if (m_TimerProc != null) StopCoroutine(m_TimerProc);
        }

        private void OnDestroy()
        {
            GameScene.onChangeScene_SelectServer -= GameScene_onChangeScene_SelectServer;
        }

        public static void DebugCall()
        {
            MyUtility.InvokeMethoName(onTimerUpdate);
        }
        public IEnumerator TimerProc()
        {
            if(m_OneSecondWaiting == null)
                m_OneSecondWaiting = new WaitForSeconds(1);

            int t = GameConfig.Instance.Play.playTime;
            while (t > 0)
            {
                if (!m_bIsPause)
                {
                    t--;
                    MyNetworkManager.SendToServer_GameTime(t);
                }
                yield return m_OneSecondWaiting;
            }
            m_TimerProc = null;
        }
        public void ParseMinuteAndSecond(int _totalSecond, out int _minute, out int _second)
        {
            if (_totalSecond == 0)
            {
                _minute = 0;
                _second = 0;
            }
            else
            {
                _minute = Mathf.FloorToInt(_totalSecond / 60f);
                _second = Mathf.FloorToInt(_totalSecond - _minute * 60);
            }
        }
        public void ReceiveTime(MyNetworkManager.NetMessage _msg)
        {
            m_iClientTime = 0;
            _msg.ResetPosition();
            _msg.Sub(out m_iClientTime);
            m_iMinutes = Mathf.FloorToInt(m_iClientTime / 60f);
            m_iSeconds = Mathf.FloorToInt(m_iClientTime - m_iMinutes * 60);

            if (onTimerUpdate != null)
                onTimerUpdate(m_iClientTime);

            if (m_iClientTime == 0)
            {
                m_bIsTimeover = true;
                GameState.Instance.ChangeGameState(eGameState.Finish);
            }
        }

        public void StartTimer()
        {
            StopTimer();
            m_bIsPause = false;
            m_iClientTime = GameConfig.Instance.Play.playTime;
            if (MyNetworkManager.IsServer)
            {
                Debug.Log("Start Timer On Server");
                m_TimerProc = TimerProc();
                StartCoroutine(m_TimerProc);
            }
        }
        public void PauseTimer() { m_bIsPause = true; }
        public void ContinueTimer() { m_bIsPause = false; }
        public void StopTimer()
        {
            if (MyNetworkManager.IsServer)
            {
                Debug.Log("Stop Timer Timer On Server");
                if (m_TimerProc != null )
                    StopCoroutine(m_TimerProc);
            }
        }
    }
}
