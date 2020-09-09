using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteractSystem;
namespace AWTG
{

    public class PictureDiscControlButton : InteractCommonButton
    {
        public static event System.Action<GameManagers.ePlayArea> onPressedButton;
        public static event System.Action<GameManagers.ePlayArea, uint> onUnPressedButton;

        [SerializeField] private TransformFader buttonFader;
        [SerializeField] private RendererColorFader buttonEmissionFader;
        protected override void Awake()
        {
            base.Awake();
            PictureDisc.onFinishPictureDisc += PictureDisc_onFinishPictureDisc;
            PictureDisc.onNotMatchPictureDisc += PictureDisc_onNotMatchPictureDisc;
            PictureDisc.onhMatchPictureDisc += PictureDisc_onhMatchPictureDisc;
            PictureDisc.onNotFinishPictureDisc += PictureDisc_onNotFinishPictureDisc;
            FamilyRelationshipsHellRoomSetting.onFinishRisePuzzle += FamilyRelationshipsHellRoomSetting_onFinishRisePuzzle;
            ListenInternalMessage(eInternalInteractMessage.Disable);
            onToggleChange += PictureDiscControlButton_onToggleChange;
        }

        private void PictureDisc_onNotFinishPictureDisc(GameManagers.ePlayArea obj)
        {
            if (GameManagers.GameArea.Instance.IsIncludedArea(obj, InteractiveArea))
            {
                buttonFader.FadeOut(() => ListenInternalMessage(eInternalInteractMessage.Enable));
                buttonEmissionFader.StartFadeAnimation();
            }
        }

        private void PictureDisc_onhMatchPictureDisc(GameManagers.ePlayArea obj)
        {
            if (GameManagers.GameArea.Instance.IsIncludedArea(obj, InteractiveArea))
            {
                buttonFader.FadeOut(() => ListenInternalMessage(eInternalInteractMessage.Enable));
                buttonEmissionFader.StartFadeAnimation();
            }
        }

        private void PictureDisc_onNotMatchPictureDisc(GameManagers.ePlayArea obj)
        {
            if (GameManagers.GameArea.Instance.IsIncludedArea(obj, InteractiveArea))
            {
                buttonFader.FadeOut(() => ListenInternalMessage(eInternalInteractMessage.Enable));
                buttonEmissionFader.StartFadeAnimation();
            }
        }

        private void PictureDiscControlButton_onToggleChange(bool obj)
        {
            PlayClickSound();
            buttonFader.FadeIn();
            buttonEmissionFader.FadeIn();
            onUnPressedButton?.Invoke(InteractiveArea, ClickedPlayerID);
        }
        private void PlayClickSound()
        {
            if (GameManagers.GameArea.Instance.IsMyArea(InteractiveArea))
            {
                GameManagers.GameSound.Instance.Play(GameManagers.eSoundID_FX.cheon_1, transform.position);
            }
        }
        private void FamilyRelationshipsHellRoomSetting_onFinishRisePuzzle()
        {
            StartCoroutine(EnableButtonProcess());
        }
        private IEnumerator EnableButtonProcess()
        {
            MyUtility.CoroutineDelay delay = new MyUtility.CoroutineDelay(2.0f);
            while (!delay.IsEnd) yield return null;
            buttonEmissionFader.StartFadeAnimation();
            ListenInternalMessage(eInternalInteractMessage.Enable);

            if (GameManagers.GameArea.Instance.IsMyArea(InteractiveArea))
            {
                GameManagers.GameSound.Instance.PlayNPC(GameManagers.eNPCSoundID_Reaper_woman_Hint._121, 0, 30);
            }
        }
        private void PictureDisc_onFinishPictureDisc(GameManagers.ePlayArea obj)
        {
            if (GameManagers.GameArea.Instance.IsIncludedArea(obj, InteractiveArea))
            {
                ListenInternalMessage(eInternalInteractMessage.Disable);
                buttonFader.FadeOut();
                buttonEmissionFader.FadeOut();
            }
        }
    }

}
