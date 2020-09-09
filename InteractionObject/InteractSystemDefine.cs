using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManagers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InteractSystem
{
    public enum eExternalInteractMessage
    {
        EnterHand,
        ExitHand,
        StayHand,

        TriggerDown,
        TriggerUp,

        EnterHMD,
        StayHMD,
        ExitHmd,
    }
    public enum eInternalInteractMessage
    {
        Disable,
        Enable,

        Pull_Start_To_Min,
        Pull_Start_To_Max,

        Enable_Physics,
        Disable_Physics,

        Detect_Collision,
        UnDetect_Collision,

        Freeze_Rigidbody_Rotation,
        UnFreeze_Rigidbody_Rotation,

        Enable_Detect_Motion,
        Disable_Detect_Motion,

        Start_Check_Ground,
        Stop_Check_Ground,

    }
    public struct stExternalInteractData
    {
        public VRHand.eHandType HandType;
        public eExternalInteractMessage InteractMessage;
        public uint GrabbingObjectID;
    }
}
    






