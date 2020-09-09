using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameManagers;
using InteractSystem;
public class InteractiveItemPieceOfMemory : InteractiveItem
{
    public static event System.Action<ePlayArea, ePieceOfMemoryType> onFirstGrabPieceOfMemory;
    public enum ePieceOfMemoryType
    {
        A_1 = 1 << 0, A_2 = 1 << 1, A_3 = 1 << 2, A_4 = 1 << 3,
        B_1 = 1 << 4, B_2 = 1 << 5, B_3 = 1 << 6, B_4 = 1 << 7,
    }

    [SerializeField] private ePieceOfMemoryType pieceType = ePieceOfMemoryType.A_1;
    public ePieceOfMemoryType PieceType { get { return pieceType; } }

    [SerializeField] private RendererColorFader chineseCharacterRenderer = null;


    protected override void Awake()
    {
        base.Awake();
        InteractObjectCollection<InteractiveItemPieceOfMemory>.Instance.AddItem(this);
        
        switch (pieceType)
        {
            case ePieceOfMemoryType.A_1:
            case ePieceOfMemoryType.B_1:
                InteractivePieceOfMemoryHole.onInstalledPieceOfMemory += InteractivePieceOfMemoryHole_onInstalledPieceOfMemory;
                GameSound.onVoiceEnd += GameSound_onVoiceEnd1;
                break;
            case ePieceOfMemoryType.A_3:
            case ePieceOfMemoryType.B_3:
                ListenInternalMessage(eInternalInteractMessage.Disable);
                AWTG.Raft.onArriveFrontOfPieceOfMemory += Raft_onArriveFrontOfPieceOfMemory;
                MyNetworkManager.OnReceiveSimpleNetMessageClient += ReceiveRaftMoveToCliff;
                InteractivePieceOfMemoryHole.onInstalledPieceOfMemory += InteractivePieceOfMemoryHole_onInstalledPieceOfMemory;
                GameSound.onVoiceEnd += GameSound_onVoiceEnd;
                break;
            case ePieceOfMemoryType.A_4:
            case ePieceOfMemoryType.B_4:
                InteractivePieceOfMemoryHole.onInstalledPieceOfMemory += InteractivePieceOfMemoryHole_onInstalledPieceOfMemory;
                AWTG.SafeReleaseDevice.onReleaseAllDevice += SafeReleaseDevice_onReleaseAllDevice;
                break;
        }
    }

    
    #region 초군문
    private void GameSound_onVoiceEnd1(eNPCSoundType arg1, int arg2)
    {
        if (arg1 == eNPCSoundType.Reaper_woman)
        {
            eNPCSoundID_Reaper_woman id = (eNPCSoundID_Reaper_woman)arg2;

            if (id == eNPCSoundID_Reaper_woman.A_6)
            {
                ListenInternalMessage(eInternalInteractMessage.Enable);
                if (GameArea.Instance.IsMyArea(InteractiveArea))
                {
                    
                    m_refHintBasic.SetSecondsToActive(10);
                    m_refHintBasic.StartCounting();
                    m_refHintBasic.onActiveHint += M_refHintBasic_onActiveHint;
                }
            }
        }
    }

    private void M_refHintBasic_onActiveHint()
    {
        GameSound.Instance.PlayNPCLoop(eNPCSoundID_Reaper_woman_Hint._105, 0, 0, 10);
    }

    private void InteractivePieceOfMemoryHole_onInstalledPieceOfMemory(ePieceOfMemoryType obj)
    {
        if (obj == pieceType)
        {
            if (GameArea.Instance.IsMyArea(InteractiveArea))
            {
                GameSound.Instance.ClearReservedNPCAudio(eNPCSoundType.Reaper_woman_Hint);
                GameSound.Instance.StopNPCSource(eNPCSoundType.Reaper_woman_Hint);
            }
        }
    }
    #endregion

    #region 나태지옥
    private void GameSound_onVoiceEnd(eNPCSoundType arg1, int arg2)
    {
        if (arg1 != eNPCSoundType.Reaper_woman)
            return;

        eNPCSoundID_Reaper_woman id = (eNPCSoundID_Reaper_woman)arg2;
        if (id == eNPCSoundID_Reaper_woman.C_19)
        {
            GameSound.onVoiceEnd -= GameSound_onVoiceEnd;

            if (GameArea.Instance.IsMyArea(InteractiveArea))
            {
                m_refHintBasic.SetSecondsToActive(10);
                m_refHintBasic.StartCounting();
            }

            chineseCharacterRenderer.FadeIn();
        }
    }
    private void ReceiveRaftMoveToCliff(MyNetworkManager.NET_GAME_MESSAGE obj)
    {
        switch (obj)
        {
            case MyNetworkManager.NET_GAME_MESSAGE.AWTG_SOMEONE_TOUCH_LAZYHELL_PIECE_OF_MEMORY:
                MyNetworkManager.OnReceiveSimpleNetMessageClient -= ReceiveRaftMoveToCliff;
                m_refHintBasic.UnActiveHint();
                break;
        }
    }
    private void Raft_onArriveFrontOfPieceOfMemory(ePlayArea obj)
    {
        if (GameArea.Instance.IsIncludedArea(obj, InteractiveArea))
        {
            chineseCharacterRenderer.FadeIn();
            ListenInternalMessage(eInternalInteractMessage.Enable);
            


            if (GameArea.Instance.IsMyArea(InteractiveArea))
            {
                m_refHintBasic.SetSecondsToActive(10);
                m_refHintBasic.StartCounting();
                //GameSound.Instance.PlayNPCLoop(eNPCSoundID_Reaper_woman_Hint._105, 0, 0, 10);
            }
        }
    }

   
    #endregion

    #region 천륜지옥

    private void SafeReleaseDevice_onReleaseAllDevice()
    {
        chineseCharacterRenderer.FadeIn(() => ListenInternalMessage(eInternalInteractMessage.Enable));

        if (GameArea.Instance.IsMyArea(InteractiveArea))
        {
            m_refHintBasic.SetSecondsToActive(10);
            m_refHintBasic.StartCounting();
            GameSound.Instance.PlayNPCLoop(eNPCSoundID_Reaper_woman_Hint._105, 0, 0, 10);
        }
    }
    #endregion








    

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InteractObjectCollection<InteractiveItemPieceOfMemory>.Instance.RemoveItem(this);
    }

    

    protected override void OnFirstGrab()
    {
        base.OnFirstGrab();
        onFirstGrabPieceOfMemory?.Invoke(InteractiveArea, pieceType);
    }

}
