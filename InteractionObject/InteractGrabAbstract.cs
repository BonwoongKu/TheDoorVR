using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace InteractSystem
{
    public struct stGrabData
    {
        
        public uint PreviousGrabberID;
        public uint GrabberID;

        public VRHand.eHandType PreviousHandType;
        public VRHand.eHandType HandType;

        public float PrevReceiveTime;
        public float ReceiveTime;

        public Quaternion Rotation;
        public Vector3 Position;
        public Vector3 Velocity;
        


        private int grabbedCount;
        public Transform GrabHandTransform { get; set; }

        private void SetDefaultData(uint _id, VRHand.eHandType _handType, Vector3 _position, Quaternion _rotation, float _receiveTime)
        {
            ReceiveTime = _receiveTime;
            GrabberID = _id;
            HandType = _handType;
            Position = _position;
            Rotation = _rotation;
        }
        public void SetGrabData(uint _id, VRHand.eHandType _handType, Vector3 _position, Quaternion _rotation, float _receiveTime)
        {
            RecordData();
            grabbedCount++;
            SetDefaultData(_id, _handType, _position, _rotation, _receiveTime);
            if (MyNetworkManager.IsMyID(GrabberID))
            {
                GrabHandTransform = VRHandEventHandler.GetHandGrabPivot(HandType);
            }
            else
            {
                GrabHandTransform = NetCharacterHandler.GetHandGrabPivot(GrabberID, HandType);
            }
            
        }
        public void SetUnGrabData(VRHand.eHandType _handType, Vector3 _position, Quaternion _rotation, Vector3 _velocity, float _receiveTime)
        {
            RecordData();
            SetDefaultData(MyNetworkManager.INVALIDID, _handType, _position, _rotation, _receiveTime);
            Velocity = _velocity;
        }
        private void RecordData()
        {
            PrevReceiveTime = ReceiveTime;
            PreviousHandType = HandType;
            PreviousGrabberID = GrabberID;
        }
        public bool IsChangedGrabHand()
        {
            return (PreviousGrabberID == GrabberID && WasGrabbing == IsGrabbing && PreviousHandType != HandType);
        }
        public void ResetData()
        {
            grabbedCount = 0;
            GrabHandTransform = null;
            GrabberID = MyNetworkManager.INVALIDID;
        }

        public bool IsFirstGrab  { get { return grabbedCount == 1; } }
        public bool AmIGrabbing     { get { return MyNetworkManager.IsMyID(GrabberID); } }
        public bool WasIGrabbing    { get { return MyNetworkManager.IsMyID(PreviousGrabberID); }  }
        public bool IsGrabbing      { get { return MyNetworkManager.INVALIDID != GrabberID; } }
        public bool WasGrabbing     { get { return MyNetworkManager.INVALIDID != PreviousGrabberID; } }
    }
    public abstract class InteractGrabAbstract : InteractObjectBasicAbstract
    {
        [Header("EventGrabAbstract")]

        protected stGrabData m_GrabData;

        public static event System.Action<stGrabData, InteractObjectBasicAbstract> onBroadcast_Grab;
        public static event System.Action<stGrabData, InteractObjectBasicAbstract> onBroadcast_UnGrab;
        [SerializeField] private UnityEngine.Events.UnityEvent m_pUnityEvent_OnFirstGrab = null;
        [SerializeField] private UnityEngine.Events.UnityEvent m_pUnityEvent_OnGrab = null;
        [SerializeField] private UnityEngine.Events.UnityEvent m_pUnityEvent_OnUnGrab = null;
        protected override void Restore()
        {
            base.Restore();
            
            GameManagers.GameLoop.onUpdate -= OnGrabbing;
            GameManagers.GameLoop.onFixedUpdate -= OnFixedGrabbing;
            m_GrabData.ResetData();
        }
        protected override void Awake()
        {
            base.Awake();
            
            m_GrabData = new stGrabData();
            MyNetworkManager.OnReceiveNetMessageClient += MyNetworkManager_OnReceiveNetMessageClient;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            MyNetworkManager.OnReceiveNetMessageClient -= MyNetworkManager_OnReceiveNetMessageClient;
            GameManagers.GameLoop.onUpdate -= OnGrabbing;
            GameManagers.GameLoop.onFixedUpdate -= OnFixedGrabbing;
        }

        private void MyNetworkManager_OnReceiveNetMessageClient(MyNetworkManager.NetMessage obj)
        {
            if (obj.ReceiverID != UID)
                return;

            obj.ResetPosition();
            switch (obj.MsgType)
            {
                case MyNetworkManager.NET_GAME_MESSAGE.MSG_GRAB:
                    ReceiveGrab(obj);
                    break;
                case MyNetworkManager.NET_GAME_MESSAGE.MSG_UNGRAB:
                    ReceiveUnGrab(obj);
                    break;
            }
        }

        public override void ListenExternalMessage(stExternalInteractData _externalInteractData)
        {
            base.ListenExternalMessage(_externalInteractData);
            switch (_externalInteractData.InteractMessage)
            {
                case eExternalInteractMessage.TriggerDown:
                    TriggerDown(_externalInteractData);
                    break;
                case eExternalInteractMessage.TriggerUp:
                    TriggerUp(_externalInteractData);
                    break;
            }
        }
        public override void ListenInternalMessage(eInternalInteractMessage _internalInteractMsg)
        {
            base.ListenInternalMessage(_internalInteractMsg);
            switch (_internalInteractMsg)
            {
                case eInternalInteractMessage.Disable:
                    GameManagers.GameLoop.onUpdate -= OnGrabbing;
                    GameManagers.GameLoop.onFixedUpdate -= OnFixedGrabbing;
                    break;
            }
        }
        [SerializeField] private bool isRequestedGrabMessage = false;
        private void TriggerDown(stExternalInteractData _externalInteractData)
        {
            Debug.Log("Trigger Down : " + name + " " + _externalInteractData.HandType);

            if (isRequestedGrabMessage)
                return;
            isRequestedGrabMessage = true;
            Transform grabTm = VRHandEventHandler.GetHandGrabPivot(_externalInteractData.HandType);
            Vector3 pos = grabTm.InverseTransformPoint(Pivot.position);
            Quaternion rot = Quaternion.Inverse(grabTm.rotation) * Pivot.rotation;

            if (m_GrabData.AmIGrabbing && m_GrabData.HandType != _externalInteractData.HandType)
            {
                VRHandEventHandler.UnGrabInteractObject(m_GrabData.HandType, this);
            }

            if (m_bIsOnlyLocalObject)
            {
                m_GrabData.SetGrabData(MyNetworkManager.MyNetID, _externalInteractData.HandType, pos, rot, Time.time);
                GrabSomething();
            }
            else
            {
                //Debug.Log("Send Grab : " + name + " " + _externalInteractData.HandType);
                MyNetworkManager.SendToServer_Grab(UID, _externalInteractData.HandType, pos, rot);
            }
        }
        private void TriggerUp(stExternalInteractData _externalInteractData)
        {
            //Debug.Log("Trigger Up : " + name + " " + _externalInteractData.HandType);
            if (isRequestedGrabMessage)
                return;

            bool isGrab = VRHandEventHandler.IsGrab(_externalInteractData.HandType, this);
            if (isGrab)
            {

                isRequestedGrabMessage = true;
                Vector3 pos = Pivot.position;
                Quaternion rot = Pivot.rotation;
                Vector3 velocity = VRInputManager.GetVelocity(_externalInteractData.HandType, VRInputManager.PositionType.CONTROLLER);

                if (m_bIsOnlyLocalObject)
                {
                    m_GrabData.SetUnGrabData(_externalInteractData.HandType, pos, rot, velocity, Time.time);
                    UnGrabSomething();
                }
                else
                {
                    //Debug.Log("Send UnGrab : " + name + " " + _externalInteractData.HandType);
                    MyNetworkManager.SendToServer_UnGrab(UID, _externalInteractData.HandType, pos, rot, velocity);
                }
            }
        }

        private void ReceiveGrab(MyNetworkManager.NetMessage _netMessage)
        {
            _netMessage.Sub(out int handTypeVal).Sub(out Vector3 grabPos).Sub(out Quaternion grabRot);
            m_GrabData.SetGrabData(_netMessage.SenderID, (VRHand.eHandType)handTypeVal, grabPos, grabRot, _netMessage.ServerTime);
            GrabSomething();
        }
        private void ReceiveUnGrab(MyNetworkManager.NetMessage _netMessage)
        {
            _netMessage.Sub(out int handTypeVal).Sub(out Vector3 unGrabPos).Sub(out Quaternion unGrabRot).Sub(out Vector3 velocity);
            m_GrabData.SetUnGrabData((VRHand.eHandType)handTypeVal, unGrabPos, unGrabRot, velocity, _netMessage.ServerTime);
            UnGrabSomething();
        }

        private void GrabSomething()
        {
            //DebugManager.LogCalledFunctionName();
            //Debug.Log("Receive Grab : " + name + "  " + m_GrabData.HandType);
            onBroadcast_Grab?.Invoke(m_GrabData, this);

            if (m_GrabData.IsFirstGrab)
            {
                OnFirstGrab();
                if (m_pUnityEvent_OnFirstGrab != null)
                    m_pUnityEvent_OnFirstGrab.Invoke();
            }
            OnGrab();
            if (m_pUnityEvent_OnGrab != null)
                m_pUnityEvent_OnGrab.Invoke();
            GameManagers.GameLoop.onUpdate -= OnGrabbing;
            GameManagers.GameLoop.onUpdate += OnGrabbing;

            GameManagers.GameLoop.onFixedUpdate -= OnFixedGrabbing;
            GameManagers.GameLoop.onFixedUpdate += OnFixedGrabbing;
            
            isRequestedGrabMessage = false;
        }

        

        private void UnGrabSomething()
        {
            //Debug.Log("Receive UnGrab : " + name + "  " + m_GrabData.HandType);
            GameManagers.GameLoop.onUpdate -= OnGrabbing;
            GameManagers.GameLoop.onFixedUpdate -= OnFixedGrabbing;
            onBroadcast_UnGrab?.Invoke(m_GrabData, this);

            OnUnGrab();
            if (m_pUnityEvent_OnUnGrab != null)
                m_pUnityEvent_OnUnGrab.Invoke();
            
            isRequestedGrabMessage = false;


        }
        protected virtual void OnChangedGrab(VRHand.eHandType _prevGrabbedHand, VRHand.eHandType _curGrabbedHand) { }
        protected virtual void OnGrab() { }
        protected virtual void OnUnGrab() { }
        protected virtual void OnGrabbing() { }
        protected virtual void OnFirstGrab() { }
        protected virtual void OnFixedGrabbing() { }
    }

}
