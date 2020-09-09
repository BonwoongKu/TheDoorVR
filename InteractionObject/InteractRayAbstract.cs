using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    public class InteractRayAbstract : InteractBasicAbstract
    {

        protected override void Awake()
        {
            base.Awake();
            ListenInternalMessage(eInternalInteractMessage.Disable);
        }

        protected override void Restore()
        {
            base.Restore();
            ListenInternalMessage(eInternalInteractMessage.Disable);
        }

        public override void ListenExternalMessage(stExternalInteractData _externalInteractData)
        {
            base.ListenExternalMessage(_externalInteractData);
            switch (_externalInteractData.InteractMessage)
            {
                case eExternalInteractMessage.EnterHMD: OnRayEnter(); break;
                case eExternalInteractMessage.StayHMD: OnRayStay(); break;
                case eExternalInteractMessage.ExitHmd: OnRayExit(); break;
            }
        }

        protected virtual void OnRayEnter() { }
        protected virtual void OnRayStay() { }
        protected virtual void OnRayExit() { }
    }
}

