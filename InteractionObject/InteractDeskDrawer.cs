using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using InteractSystem;
namespace InteractSystem
{
    public class InteractDeskDrawer : InteractPullAbstract
    {
        [Header("InteractDeskDrawer")]
        [SerializeField] private bool m_bIsOpen = false;

        [SerializeField] private SoundSource m_refSoundSourceOpen = null;
        [SerializeField] private SoundSource m_refSoundSourceClose = null;

        [SerializeField] private List<InteractBasicAbstract> m_IncludedObject = null;
        [SerializeField] private List<InteractBasicAbstract> m_SavedIncludedObject = null;
        [SerializeField] private Collider m_colDrawerCollider = null;

        [SerializeField] private UnityEngine.Events.UnityEvent onStartOpen = null;
        [SerializeField] private UnityEngine.Events.UnityEvent m_onOpen = null;

        protected override void Awake()
        {
            base.Awake();
            if (m_colDrawerCollider == null)
                return;
            onLimitMax += EventPullDeskDrawer_onLimitToMax;
            onLimitMin += EventPullDeskDrawer_onLimitToMin;
            InteractDeskDrawerHandler.RegistDrawer(this);
            m_SavedIncludedObject = new List<InteractBasicAbstract>();
            for (int i = 0; i < m_IncludedObject.Count; i++)
            {
                m_SavedIncludedObject.Add(m_IncludedObject[i]);
            }
            MyNetworkManager.OnReceiveNetMessageClient += MyNetworkManager_OnReceiveNetMessageClient;
        }
        private void EventPullDeskDrawer_onLimitToMin()
        {
            m_bIsAblePulling = false;
            MyNetworkManager.SendToServer_GameMessage(MyNetworkManager.NET_GAME_MESSAGE.MSG_DESK_CLOSE, UID);
        }

        private void EventPullDeskDrawer_onLimitToMax()
        {
            m_bIsAblePulling = false;
            MyNetworkManager.SendToServer_GameMessage(MyNetworkManager.NET_GAME_MESSAGE.MSG_DESK_OPEN, UID);
        }

        private void MyNetworkManager_OnReceiveNetMessageClient(MyNetworkManager.NetMessage _netMessage)
        {
            if (!_netMessage.IsSameReceiverID(UID))
                return;

            switch (_netMessage.MsgType)
            {
                case MyNetworkManager.NET_GAME_MESSAGE.MSG_DESK_OPEN:
                    if (m_refSoundSourceOpen != null)
                        m_refSoundSourceOpen.Play(Pivot.position);
                    if (onStartOpen != null)
                        onStartOpen.Invoke();
                    StartProcToMax(OpenDesk);

                    break;
                case MyNetworkManager.NET_GAME_MESSAGE.MSG_DESK_CLOSE:
                    if (m_refSoundSourceClose != null)
                        m_refSoundSourceClose.Play(Pivot.position);

                    DisableIncludeObject();
                    StartProcToMin(CloseDesk);
                    break;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            InteractDeskDrawerHandler.UnRegistDrawer(this);
            MyNetworkManager.OnReceiveNetMessageClient -= MyNetworkManager_OnReceiveNetMessageClient;
        }
        protected override void Restore()
        {
            base.Restore();
            m_IncludedObject.Clear();

            for (int i = 0; i < m_SavedIncludedObject.Count; i++)
            {
                m_IncludedObject.Add(m_SavedIncludedObject[i]);
            }
        }

        
        private void OpenDesk()
        {
            m_bIsOpen = true;
            if (m_onOpen != null)
                m_onOpen.Invoke();
            EnableIncludeObject();
            m_bIsAblePulling = true;
        }

        private void CloseDesk()
        {
            m_bIsOpen = false;
            m_bIsAblePulling = true;
        }

        public bool AddObjectInsideDrawer(InteractBasicAbstract _target)
        {
            if (IsInsideDrawer(_target))
                return false;

            Vector3 pos = _target.Pivot.position;
            if (m_colDrawerCollider.bounds.Contains(pos))
            {
                if (m_PullType == InteractPullAbstract.PullType.Position)
                {
                    _target.Pivot.parent = m_tmPullingTarget;
                }

                m_IncludedObject.Add(_target);
                _target.ListenInternalMessage(eInternalInteractMessage.Disable_Physics);
                if (m_bIsOpen)
                    _target.ListenInternalMessage(eInternalInteractMessage.Enable);
                else
                    _target.ListenInternalMessage(eInternalInteractMessage.Disable);

                return true;
            }
            else
                return false;
        }
        public bool RemoveObjectInsideDrawer(InteractBasicAbstract _target)
        {
            if (IsInsideDrawer(_target))
            {
                m_IncludedObject.Remove(_target);
                return true;
            }
            else
                return false;
        }
        private bool IsInsideDrawer(InteractBasicAbstract _target)
        {
            return m_IncludedObject.Contains(_target);
        }

        private void DisableIncludeObject()
        {
            for (int i = 0; i < m_IncludedObject.Count; i++)
            {
                m_IncludedObject[i].ListenInternalMessage(eInternalInteractMessage.Disable);
            }
        }
        private void EnableIncludeObject()
        {
            for (int i = 0; i < m_IncludedObject.Count; i++)
            {
                m_IncludedObject[i].ListenInternalMessage(eInternalInteractMessage.Enable);
            }
        }
    }
}
public class InteractDeskDrawerHandler
{   
    private static List<InteractDeskDrawer> m_RegistedDrawer;

    static InteractDeskDrawerHandler()
    {
        if (m_RegistedDrawer == null)
            m_RegistedDrawer = new List<InteractDeskDrawer>();
    }
    public static void RegistDrawer(InteractDeskDrawer _drawer)
    {
        if (!m_RegistedDrawer.Contains(_drawer)) m_RegistedDrawer.Add(_drawer);
    }
    public static void UnRegistDrawer(InteractDeskDrawer _drawer)
    {
        if (m_RegistedDrawer.Contains(_drawer)) m_RegistedDrawer.Remove(_drawer);
    }

    public static void AddObjectInsideDrawer(InteractBasicAbstract _target)
    {
        int count = m_RegistedDrawer.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_RegistedDrawer[i].AddObjectInsideDrawer(_target))
                return;
        }
    }
    public static void RemoveObjectInsideDrawer(InteractBasicAbstract _target)
    {
        int count = m_RegistedDrawer.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_RegistedDrawer[i].RemoveObjectInsideDrawer(_target))
                return;
        }
    }
}


