using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GameManagers;

namespace InteractSystem
{
    public abstract class InteractPullAbstract : InteractGrabAbstract
    {
        protected enum PullType { Position, Rotation, }

        [Header("EventPullAbleObject")]
        [SerializeField] private bool isUnLimited = false;
        [SerializeField] protected PullType m_PullType = PullType.Position;
        [SerializeField] protected Transform m_tmPullingTarget = null;
        [SerializeField] private Vector3 m_vLocalPullingAxis = Vector3.zero;
        private Vector3 m_vWorldPullingAxis;


        [SerializeField] protected bool m_bIsAblePulling = true;
        private bool m_Saved_bIsAbleDetectMotion;

        private Vector3 m_vPrevPullingPosition;
        private Vector3 m_vCurrentPullingPosition;
        private Vector3 prevPullingTargetPosition;
        [SerializeField] private float m_fPrevPulledvalue = 0;
        [SerializeField] protected float m_fPulledValue = 0;
        [SerializeField] private float autoMinPullValue = 0;
        [SerializeField] private float autoMaxPullValue = 0;

        [SerializeField] private float minPullValue = 0;
        [SerializeField] private float maxPullValue = 0;

        public float MinPullValue { get { return minPullValue; } }
        public float MaxPullValue { get { return maxPullValue; } }
        public float CurrentPulledValue { get { return m_fPulledValue; } }
        public float PreviewPulledValue { get { return m_fPrevPulledvalue; } }

        private Vector3 m_vOriginPullValue;
    
        [SerializeField] private float m_fToMaxDuration = 1;
        [SerializeField] private float m_fToMinDuration = 1;
    

        protected event System.Action onLimitMax;
        protected event System.Action onLimitMin;
        protected event System.Action onChangePullValue;
        protected event System.Action onTranslatedTarget;
        private NetworkComunicationData m_comunicationData;
        protected override void Awake()
        {
            base.Awake();
            m_comunicationData = new NetworkComunicationData();
            m_vWorldPullingAxis = m_tmPullingTarget.TransformDirection(m_vLocalPullingAxis).normalized;

            m_Saved_bIsAbleDetectMotion = m_bIsAblePulling;


            if (m_PullType == PullType.Position) { m_vOriginPullValue = m_tmPullingTarget.position; }
            else { m_vOriginPullValue = m_tmPullingTarget.rotation.eulerAngles; }

            MyNetworkManager.OnReceiveNetMessageClient += MyNetworkManager_OnReceiveNetMessageClient;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            MyNetworkManager.OnReceiveNetMessageClient -= MyNetworkManager_OnReceiveNetMessageClient;
        }

        private void MyNetworkManager_OnReceiveNetMessageClient(MyNetworkManager.NetMessage obj)
        {
            if (!obj.IsSameReceiverID(UID))
                return;
            
            switch (obj.MsgType)
            {
                case MyNetworkManager.NET_GAME_MESSAGE.MSG_SYNC_PULL_VALUE:
                    if (!MyNetworkManager.IsMyID(obj.SenderID))
                    {
                        obj.Sub(out float newPullValue);
                        m_fPrevPulledvalue = m_fPulledValue;
                        m_fPulledValue = newPullValue;
                        m_comunicationData.Receive();

                        if (!m_GrabData.IsGrabbing)
                        {
                            TranslatePullingTarget(m_fPulledValue);
                        }
                    }
                    break;
            }
        }

    
        protected override void Restore()
        {
            base.Restore();
            m_bIsAblePulling = m_Saved_bIsAbleDetectMotion;
        
            if (m_PullType == PullType.Position)    { m_tmPullingTarget.position = m_vOriginPullValue; }
            else                                    { m_tmPullingTarget.rotation = Quaternion.Euler( m_vOriginPullValue ); }
        }

        public override void ListenInternalMessage(eInternalInteractMessage _internalInteractMsg)
        {
            base.ListenInternalMessage(_internalInteractMsg);
            switch (_internalInteractMsg)
            {
                case eInternalInteractMessage.Pull_Start_To_Max: StartProcToMax(null); break;
                case eInternalInteractMessage.Pull_Start_To_Min: StartProcToMin(null); break;
            }
        }

        protected override void OnChangedGrab(VRHand.eHandType _prevGrabbedHand, VRHand.eHandType _curGrabbedHand)
        {
        }
        protected override void OnGrabbing()
        {
            
            if (m_GrabData.AmIGrabbing)
            {
                if (!m_bIsAblePulling)
                {
                    Vector3 handWorldPosition = VRInputManager.GetControllerPosition(m_GrabData.HandType, VRInputManager.PositionType.CONTROLLER);

                    //m_vCurrentPullingPosition = handWorldPosition;
                    m_vPrevPullingPosition = handWorldPosition;
                    return;
                }
                if (m_PullType == PullType.Position) { CalculatePosition(); }
                else if (m_PullType == PullType.Rotation) { CalculateRotation(); }
                CheckLimitVlaue();
                onChangePullValue?.Invoke();
                TranslatePullingTarget(m_fPulledValue);

                if (m_comunicationData.IsAbleSend)
                {
                    SendPullData();
                }
            }
            else
            {
                float pullValue = Mathf.Lerp(m_fPrevPulledvalue, m_fPulledValue, m_comunicationData.UpdateDelta);
                TranslatePullingTarget(pullValue);
            }
        }

        private void SendPullData()
        {
            MyNetworkManager.SendToServer_FloatValue(MyNetworkManager.NET_GAME_MESSAGE.MSG_SYNC_PULL_VALUE, UID, m_fPulledValue);
            m_comunicationData.Send();
        }

        protected override void OnGrab()
        {
            base.OnGrab();
            if (MyNetworkManager.IsMyID(m_GrabData.GrabberID))
            {
                Vector3 handWorldPosition = VRInputManager.GetControllerPosition(m_GrabData.HandType, VRInputManager.PositionType.CONTROLLER);
                m_vPrevPullingPosition = /*m_vCurrentPullingPosition =*/ handWorldPosition;
                prevPullingTargetPosition = m_tmPullingTarget.position;
                MyNetworkManager.SendToServer_FloatValue(MyNetworkManager.NET_GAME_MESSAGE.MSG_SYNC_PULL_VALUE, UID, m_fPulledValue);
                m_comunicationData.Send();
            }
        }
        protected override void OnUnGrab()
        {
            if (m_GrabData.WasIGrabbing)
            {
                MyNetworkManager.SendToServer_FloatValue(MyNetworkManager.NET_GAME_MESSAGE.MSG_SYNC_PULL_VALUE, UID, m_fPulledValue);
                m_comunicationData.Send();
            }
        }

        private void CheckLimitVlaue( )
        {
            if (!isUnLimited)
            {
                if (isAutoTranslation)
                    m_fPulledValue = Mathf.Clamp(m_fPulledValue, autoMinPullValue, autoMaxPullValue);
                else
                    m_fPulledValue = Mathf.Clamp(m_fPulledValue, minPullValue, maxPullValue);

                if (m_fPrevPulledvalue < maxPullValue && m_fPulledValue > maxPullValue)
                {
                    onLimitMax?.Invoke();
                    SendPullData();
                    //m_bIsAblePulling = false;
                    //MyNetworkManager.SendToServer_GameMessage(MyNetworkManager.NET_GAME_MESSAGE.MSG_PULL_LIMIT_TO_MAX, UID);
                }
                else if (m_fPrevPulledvalue > minPullValue && m_fPulledValue < minPullValue)
                {
                    onLimitMin?.Invoke();
                    SendPullData();
                    //m_bIsAblePulling = false;
                    //MyNetworkManager.SendToServer_GameMessage(MyNetworkManager.NET_GAME_MESSAGE.MSG_PULL_LIMIT_TO_MIN, UID);
                }

               
            }
            
        }
        [SerializeField] private bool isAutoTranslation = false;
        private void TranslatePullingTarget(float _pulledValue)
        {
            if (m_PullType == PullType.Position) { m_tmPullingTarget.position = m_vOriginPullValue + m_vWorldPullingAxis * _pulledValue; }
            else { m_tmPullingTarget.rotation = Quaternion.AngleAxis(_pulledValue, m_vWorldPullingAxis) * Quaternion.Euler(m_vOriginPullValue);  }

            onTranslatedTarget?.Invoke();
        }
        
        private void CalculateRotation()
        {
            

            Vector3 currentHandPosition = m_vCurrentPullingPosition = VRInputManager.GetControllerPosition(m_GrabData.HandType, VRInputManager.PositionType.CONTROLLER);

            Vector3 prevDirection = m_vPrevPullingPosition - prevPullingTargetPosition;
            Vector3 currentDirection = currentHandPosition - m_tmPullingTarget.position;

            Vector3 crossFromAxisToPrev = Vector3.Cross(prevDirection, m_vWorldPullingAxis);

            Vector3 crossFromAxisToCurrent = Vector3.Cross(currentDirection, m_vWorldPullingAxis);
            Vector3 crossPrevCurrent = Vector3.Cross(crossFromAxisToPrev, crossFromAxisToCurrent).normalized;
            float crossAngle = Vector3.Angle(crossFromAxisToPrev, crossFromAxisToCurrent);

            float dot = Vector3.Dot(crossPrevCurrent, m_vWorldPullingAxis);

            float finalAngle = crossAngle * dot;
            m_fPrevPulledvalue = m_fPulledValue;
            m_vPrevPullingPosition = currentHandPosition;
            prevPullingTargetPosition = m_tmPullingTarget.position;
            m_fPulledValue += finalAngle;
            
        }

        
        private void CalculatePosition()
        {
            m_fPrevPulledvalue = m_fPulledValue;
            //m_vPrevPullingPosition = m_vCurrentPullingPosition;
            Vector3 currentHandPosition = m_vCurrentPullingPosition = VRInputManager.GetControllerPosition(m_GrabData.HandType, VRInputManager.PositionType.CONTROLLER);
            Vector3 delta = currentHandPosition - m_vPrevPullingPosition;

            float dot = Vector3.Dot(m_vWorldPullingAxis, delta);
            m_vPrevPullingPosition = currentHandPosition;
            m_fPulledValue += dot;
        }

        protected void StartProcTargetValue(float _pulledValue, float _duration, System.Action _callBack)
        {
            m_bIsAblePulling = false;
            StartCoroutine(Proc_ToTargetValue(_pulledValue, _duration, _callBack));
        }
        protected void StartProcToMax( System.Action _callBack)
        {
            m_bIsAblePulling = false;
            StartProcTargetValue(autoMaxPullValue, m_fToMaxDuration, _callBack);
        }
        protected void StartProcToMin( System.Action _callBack)
        {
            m_bIsAblePulling = false;
            StartProcTargetValue(autoMinPullValue, m_fToMinDuration, _callBack);
        }

        private IEnumerator Proc_ToTargetValue(float _targetPullValue, float _duration, System.Action _callBack)
        {
            MyUtility.CoroutineDelay delay = new MyUtility.CoroutineDelay(_duration);
            delay.SetDelay(_duration);
            float targetPulledValue = Mathf.Clamp(_targetPullValue, autoMinPullValue, autoMaxPullValue);
            
            while (!delay.IsEnd)
            {
                float pullValue = Mathf.Lerp(m_fPulledValue, targetPulledValue, delay.NormalizedTime);
                TranslatePullingTarget(pullValue);
                yield return null;
            }

            m_fPulledValue = targetPulledValue;

            _callBack?.Invoke();
        }
    }
}