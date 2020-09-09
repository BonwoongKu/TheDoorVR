using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManagers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InteractSystem
{
    public abstract class InteractBasicAbstract : MonoBehaviour
    {
        [Header("InteractObjectAbstract")]
        [SerializeField, ReadOnly] private uint m_iUniqueID = 0;
        public uint UID { get { return m_iUniqueID; } }
        public const uint InvalidUID = uint.MaxValue - 10;

        [SerializeField, EnumFlag] private ePlayArea interactiveArea = (ePlayArea)(-1);
        protected ePlayArea InteractiveArea { get { return interactiveArea; } }

        [SerializeField, EnumFlag] private eTrick m_eUsingTrick;
        public eTrick UsingTrick { get { return m_eUsingTrick; } }


        [Header("로컬에서만 사용될때"), SerializeField]
        protected bool m_bIsOnlyLocalObject = false;

        private bool m_bSavedIsEnabled = true;
        [Header("손과 상호작용 활성화/비활성화"), SerializeField]
        private bool m_bIsEnabled = true;
        public bool IsEnabled { get { return m_bIsEnabled; } }

        [Header("손 감지 영역 (손과 콜리더의 가까운 위치 감지)"), SerializeField]
        protected Collider detectionArea;
        public Collider DetectionArea { get { return detectionArea; } }

        [SerializeField] private Transform m_tm_ObjectPivot;
        public Transform Pivot { get { return m_tm_ObjectPivot; } }

        [Header("오브젝트 아웃라인"),SerializeField]
        protected GlowObjectCmd m_GlowCmd;
        private Coroutine m_ProcessingCoroutine;

        [SerializeField,EnumFlag] private VRHand.eHandType m_eEnteredHand;


        private Transform m_refTmRecordedParent;
        private Vector3 m_vRecordedPosition;
        private Quaternion m_qRecordedRotation;
        private Vector3 m_vRecordedScale;

        protected bool IsEnteredAnyHand() { return (m_eEnteredHand != 0); }
        
        private void EnterHand(VRHand.eHandType _handType)
        {
            if (!IsEnteredAnyHand())
                ShowOutLine();
            m_eEnteredHand |= _handType;
        }
        private void ExitHand(VRHand.eHandType _handType)
        {
            m_eEnteredHand &= ~_handType;
            if (!IsEnteredAnyHand())
                HideOutLine();
        }

        protected virtual void Awake()
        {
            m_bSavedIsEnabled = m_bIsEnabled;

            if (m_tm_ObjectPivot == null) m_tm_ObjectPivot = transform;

            if (detectionArea == null)
            {
                detectionArea = GetComponent<Collider>();
                if (detectionArea == null)
                    detectionArea = GetComponentInChildren<Collider>();
            }

            if (detectionArea != null)
                ExtentsSize = detectionArea.bounds.extents.magnitude;

            RecordTransform();

            EventObjectHandler.Add_EventAbleObject(this);
            GameState.onChangeGameState += GameState_onChangeGameState;
        }

        public void RecordTransform()
        {
        
            m_refTmRecordedParent = Pivot.parent;
            m_vRecordedPosition = Pivot.localPosition;
            m_qRecordedRotation = Pivot.localRotation;
            m_vRecordedScale = Pivot.localScale;
        }
        public void SetTransformRecorded()
        {
            Pivot.parent = m_refTmRecordedParent;
            Pivot.localPosition = m_vRecordedPosition;
            Pivot.localRotation = m_qRecordedRotation;
            Pivot.localScale = m_vRecordedScale;
        }
        private void GameState_onChangeGameState(eGameState arg1, eGameState arg2)
        {
            switch (arg2)
            {
                case eGameState.Standby:
                    Restore();
                    break;
            }
        }


        protected bool IsUsingTrick(eTrick _trick)
        {
            return (( m_eUsingTrick & _trick)  != 0 );
        }


        protected virtual void OnDestroy()
        {
            EventObjectHandler.Remove_EventAbleObject(this);
            GameState.onChangeGameState -= GameState_onChangeGameState;
        }

        protected virtual void Restore()
        {
            StopPivotProcessing();
            m_eEnteredHand = 0;
            m_bIsEnabled = m_bSavedIsEnabled;
            HideOutLine();


            SetTransformRecorded();
        }

        public bool IsEqual(InteractBasicAbstract _target) { if (_target == null)    return false; else return (m_iUniqueID == _target.UID); }
        public Vector3 ClosetPoints(Vector3 _handPosition)
        {
            if (detectionArea == null)
                return Vector3.zero;
            else
                return detectionArea.ClosestPoint(_handPosition);
        }

        
        public float ExtentsSize { get; set; }

        public void StartPivotProcessing(Transform _parent, Vector3 _localPos, Quaternion _localRot, float _duration, System.Action _callBack)
        {
            Vector3 originScale = m_tm_ObjectPivot.lossyScale;
            Pivot.SetParent(_parent);

            if (_parent != null)
            {
                StopPivotProcessing();
                m_ProcessingCoroutine = StartCoroutine(MovingTransform(_parent, _localPos, _localRot, originScale, _duration, _callBack));
            }
        }
        public void StartPivotProcessing(Transform _parent)
        {
            StartPivotProcessing(_parent, Vector3.zero, Quaternion.identity, 0.2f, null);
        }
        public void StartPivotProcessing(Transform _parent, System.Action _callBack)
        {
            StartPivotProcessing(_parent, Vector3.zero, Quaternion.identity, 0.2f, _callBack);
        }
        public void StartPivotProcessing(Transform _parent, float _duration)
        {
            StartPivotProcessing(_parent, Vector3.zero, Quaternion.identity, _duration, null);
        }
        public void StartPivotProcessing(Transform _parent, float _duration, System.Action _callBack)
        {
            StartPivotProcessing(_parent, Vector3.zero, Quaternion.identity, _duration, _callBack);
        }
        public void StopPivotProcessing()
        {
            if (m_ProcessingCoroutine != null)
            {
                StopCoroutine(m_ProcessingCoroutine);
                m_ProcessingCoroutine = null;
            }
        }

        public bool IsFinishPivotProcessing()
        {
            return (m_ProcessingCoroutine == null);
        }

        private IEnumerator MovingTransform(Transform _parent, Vector3 _localPos, Quaternion _localRot, Vector3 _originScale, float _duration,System.Action _callBack)
        {
            MyUtility.CoroutineDelay delay = new MyUtility.CoroutineDelay( _duration);
            Vector3 startPos = m_tm_ObjectPivot.localPosition;
            Vector3 endPos = _localPos;
            Quaternion startRot = m_tm_ObjectPivot.localRotation;
            Quaternion endRot = _localRot;
            Vector3 startScale = m_tm_ObjectPivot.localScale;
            Vector3 endScale = MyUtility.GetParentScales(_parent, _originScale);

            delay.Reset();
            while (!delay.IsEnd)
            {
                m_tm_ObjectPivot.localPosition = Vector3.Lerp(startPos, endPos, delay.NormalizedTime);
                m_tm_ObjectPivot.localRotation = Quaternion.Slerp(startRot, endRot, delay.NormalizedTime);
                m_tm_ObjectPivot.localScale = Vector3.Lerp(startScale, endScale, delay.NormalizedTime);
                yield return null;
            }

            //Vector3 dir = endPos - startPos;
            //Vector3 xEndPos = startPos + Vector3.right * dir.x;
            //delay.Reset();
            //while (!delay.IsEnd)
            //{
            //    m_tm_ObjectPivot.localPosition = Vector3.Lerp(startPos, xEndPos, delay.NormalizedTime);
            //    yield return null;
            //}
            //delay.Reset();
            //Vector3 yEndPos = xEndPos + Vector3.up * dir.y;
            //while (!delay.IsEnd)
            //{
            //    m_tm_ObjectPivot.localPosition = Vector3.Lerp(xEndPos, yEndPos, delay.NormalizedTime);
            //    yield return null;
            //}
            //delay.Reset();
            //Vector3 zEndPos = yEndPos + Vector3.forward * dir.z;
            //while (!delay.IsEnd)
            //{
            //    m_tm_ObjectPivot.localPosition = Vector3.Lerp(yEndPos, zEndPos, delay.NormalizedTime);
            //    yield return null;
            //}
            yield return null;

            _callBack?.Invoke();
            m_ProcessingCoroutine = null;
        }

        protected void ShowOutLine() { if (m_GlowCmd != null) m_GlowCmd.ShowGlow = true; }
        protected void HideOutLine() { if (m_GlowCmd != null) m_GlowCmd.ShowGlow = false; }

        public virtual void ListenExternalMessage(stExternalInteractData _externalInteractData)
        {
            switch (_externalInteractData.InteractMessage)
            {
                case eExternalInteractMessage.EnterHand: EnterHand(_externalInteractData.HandType); break;
                case eExternalInteractMessage.ExitHand: ExitHand(_externalInteractData.HandType); break;
            }
        }

        public virtual void ListenInternalMessage(eInternalInteractMessage _internalInteractMsg)
        {
            switch (_internalInteractMsg)
            {
                case eInternalInteractMessage.Disable: m_bIsEnabled = false; break;
                case eInternalInteractMessage.Enable: m_bIsEnabled = true; break;
            }
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(InteractSystem.InteractObjectAbstract))]
[CanEditMultipleObjects]
public class InteractObjectAbstract : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif







