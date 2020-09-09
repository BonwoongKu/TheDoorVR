using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteractSystem;
public class InteractivePieceOfMemoryHole : InteractBasicAbstract
{
    public static event System.Action<InteractiveItemPieceOfMemory.ePieceOfMemoryType> onInstalledPieceOfMemory;
    public static event System.Action<InteractiveItemPieceOfMemory.ePieceOfMemoryType, Vector3> onInstalledPieceOfMemoryWithPosition;
    [SerializeField] private InteractiveItemPieceOfMemory.ePieceOfMemoryType installTargetPieceType = InteractiveItemPieceOfMemory.ePieceOfMemoryType.A_1;

    [SerializeField] private ParticleSystem equipParticle = null;

    [SerializeField] private GameObject installedPieceObject;

    protected override void Awake()
    {
        base.Awake();
        MyNetworkManager.OnReceiveNetMessageClient += MyNetworkManager_OnReceiveNetMessageClient;
        installedPieceObject.SetActive(false);
        ListenInternalMessage(eInternalInteractMessage.Disable);

        InteractiveItemPieceOfMemory.onFirstGrabPieceOfMemory += InteractiveItemPieceOfMemory_onFirstGrabPieceOfMemory;
    }

    private void InteractiveItemPieceOfMemory_onFirstGrabPieceOfMemory(GameManagers.ePlayArea arg1, InteractiveItemPieceOfMemory.ePieceOfMemoryType arg2)
    {
        if (arg2 == installTargetPieceType)
        {
            ListenInternalMessage(eInternalInteractMessage.Enable);
        }
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
            case MyNetworkManager.NET_GAME_MESSAGE.AWTG_INSTALL_PIECE_OF_MEMORY:
                obj.Sub(out uint pieceID);
                GameManagers.GameSound.Instance.Play(GameManagers.eSoundID_FX.c_3, transform.position);
                ReceiveInstallPiece(pieceID);
                break;
        }
        
    }


    public override void ListenExternalMessage(stExternalInteractData _externalInteractData)
    {
        base.ListenExternalMessage(_externalInteractData);

        switch (_externalInteractData.InteractMessage)
        {
            case eExternalInteractMessage.TriggerUp:
                RequestInstallPiece(_externalInteractData.GrabbingObjectID);
                break;
        }
    }

    private void RequestInstallPiece(uint _grabbingObjectID)
    {
        if (InteractObjectCollection<InteractiveItemPieceOfMemory>.Instance.TryGetItem(_grabbingObjectID, out InteractiveItemPieceOfMemory piece))
        {
            if (piece.PieceType == installTargetPieceType)
            {
                this.ListenInternalMessage(eInternalInteractMessage.Disable);
                piece.ListenInternalMessage(eInternalInteractMessage.Disable);

                MyNetworkManager.SendToServer_UIntValue(MyNetworkManager.NET_GAME_MESSAGE.AWTG_INSTALL_PIECE_OF_MEMORY, UID, piece.UID);
            }
        }
    }

    private void ReceiveInstallPiece(uint _receivedPieceID)
    {
        if (InteractObjectCollection<InteractiveItemPieceOfMemory>.Instance.TryGetItem(_receivedPieceID, out InteractiveItemPieceOfMemory piece))
        {

            this.ListenInternalMessage(eInternalInteractMessage.Disable);
            piece.ListenInternalMessage(eInternalInteractMessage.Disable);
            piece.ListenInternalMessage(eInternalInteractMessage.Disable_Physics);

            Vector3 holePosition = Pivot.position;
            piece.StartPivotProcessing(Pivot, ()=> 
            {
                installedPieceObject.SetActive(true);
                Pivot.gameObject.SetActive(false);
                equipParticle.Play();  
                onInstalledPieceOfMemory?.Invoke(installTargetPieceType); 
                onInstalledPieceOfMemoryWithPosition?.Invoke(installTargetPieceType, holePosition);  
            });
            
        }
    }
}
