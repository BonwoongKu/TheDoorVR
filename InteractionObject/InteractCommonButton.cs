using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManagers;
using InteractSystem;

public class InteractCommonButton : InteractBasicAbstract
{
    private enum eButtonType { Click, Toggle, }
    [Header("공용 버튼")]
    [Space(10)]

    [SerializeField] private eButtonType m_eButtonType = eButtonType.Click;
    private bool m_bIsToggleOn = false;
    private bool m_bRecordedIsToggleOn = false;
    private float m_fLastClickedTime = 0;

    protected float ClickedTime { get { return m_fLastClickedTime; } set { m_fLastClickedTime = value; } }
    protected event System.Action onClickButton;
    protected event System.Action<bool> onToggleChange;
    protected uint ClickedPlayerID { get; set; }
    [SerializeField] private UnityEngine.Events.UnityEvent onClickButton_UnityEvent;
    [SerializeField] private UnityEngine.Events.UnityEvent<bool> onToggleChanged_UnityEvent;

    protected override void Awake()
    {
        base.Awake();
        m_bRecordedIsToggleOn = m_bIsToggleOn;
        MyNetworkManager.OnReceiveNetMessageClient += MyNetworkManager_OnReceiveNetMessageClient;
    }


    protected override void Restore()
    {
        base.Restore();
        m_bIsToggleOn = m_bRecordedIsToggleOn;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        onClickButton = null;
        onToggleChange = null;
        MyNetworkManager.OnReceiveNetMessageClient -= MyNetworkManager_OnReceiveNetMessageClient;
    }

    private void MyNetworkManager_OnReceiveNetMessageClient(MyNetworkManager.NetMessage obj)
    {
        switch (obj.MsgType)
        {
            case MyNetworkManager.NET_GAME_MESSAGE.MSG_PUSH_BUTTON:
                if (obj.IsSameReceiverID(UID))
                {
                    ClickedPlayerID = obj.SenderID;
                    m_fLastClickedTime = obj.ServerTime;
                    PushedButton();
                }
                break;
        }
    }

    private void PushedButton()
    {
        ListenInternalMessage(eInternalInteractMessage.Disable);
        
        switch (m_eButtonType)
        {
            case eButtonType.Click:
                onClickButton?.Invoke();
                break;
            case eButtonType.Toggle:
                m_bIsToggleOn = !m_bIsToggleOn;
                onToggleChange?.Invoke(m_bIsToggleOn);
                break;
        }
    }

    public override void ListenExternalMessage(stExternalInteractData _externalInteractData)
    {
        base.ListenExternalMessage(_externalInteractData);
        switch (_externalInteractData.InteractMessage)
        {
            case eExternalInteractMessage.TriggerDown:
                TriggerDown();
                break;
        }
    }


    private void TriggerDown()
    {
        ListenInternalMessage(eInternalInteractMessage.Disable);
        MyNetworkManager.SendToServer_GameMessage(MyNetworkManager.NET_GAME_MESSAGE.MSG_PUSH_BUTTON, UID);
    }
}
