using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GameManagers;


#if UNITY_EDITOR
using UnityEditor;
#endif

using InteractSystem;
public class InteractiveItem : InteractGrabAbstract
{
    [Header("EventItem"), Space(10)]
    [SerializeField] protected Transform m_refTmRoot;
    [SerializeField] protected Rigidbody m_Rigidbody;
    
    private bool        m_InitRigidbody_useGravity;
    private bool        m_InitRigidbody_isKinematic;
    
    protected MaterialPropertyBlock m_MaterialPropertyBlock;

    [SerializeField] protected Renderer[] m_rnrRimlights;
    
    private IEnumerator m_ProcCheckGround;
    //[SerializeField] private float m_fTargetEmissionMultiply = 0;
    [SerializeField] private float m_fEmissionMultiplyOnGround = 0.5f;
    [SerializeField] private float m_fEmissionMultiplyWhenGrab = 0.07f;

    [SerializeField] private Color m_colRimColor = new Color(1.0f, 0.5f, 0, 1);
    [SerializeField] private float m_fRimPower = 3;
    private const string _TexturColorMultiplyFadeData = "_TexturColorMultiplyFadeData";
    private const string _RimEffectFadeData = "_RimEffectFadeData";

    [SerializeField] protected NoticeHintBasic m_refHintBasic = null;


    private Vector4 GetFadeInterpolateData(Vector4 _prevData, float _newFadeValue)
    {
        Vector4 fadeData = _prevData;
        float sceneLoadTime = Time.timeSinceLevelLoad;
        fadeData.z = 0.6f;
        float duration = fadeData.z;
        float startTime = fadeData.w;
        float passedTime = sceneLoadTime - startTime;
        float normalizeTime = Mathf.Clamp01(passedTime / duration);

        fadeData.x = Mathf.Lerp(fadeData.x, fadeData.y, normalizeTime);
        fadeData.y = _newFadeValue;

        fadeData.w = sceneLoadTime;
        return fadeData;
    }

    private void SetRimFactor( float _rimFactor )
    {
        for (int i = 0; i < m_rnrRimlights.Length; i++)
        {
            m_rnrRimlights[i].GetPropertyBlock(m_MaterialPropertyBlock);
            Vector4 fadeData = m_MaterialPropertyBlock.GetVector(_RimEffectFadeData);
            fadeData = GetFadeInterpolateData(fadeData, _rimFactor);
            m_MaterialPropertyBlock.SetColor(ShaderPropertyDefine._RimColor, m_colRimColor);
            m_MaterialPropertyBlock.SetFloat(ShaderPropertyDefine._RimPower, m_fRimPower);
            m_MaterialPropertyBlock.SetVector(_RimEffectFadeData, fadeData);
            m_rnrRimlights[i].SetPropertyBlock(m_MaterialPropertyBlock);
        }
    }
    
    protected void SetTextureMultiply( float _multiplyValue)
    {
        for (int i = 0; i < m_rnrRimlights.Length; i++)
        {
            m_rnrRimlights[i].GetPropertyBlock(m_MaterialPropertyBlock);
            Vector4 fadeData = m_MaterialPropertyBlock.GetVector(_TexturColorMultiplyFadeData);
            fadeData = GetFadeInterpolateData(fadeData, _multiplyValue);
            m_MaterialPropertyBlock.SetVector(_TexturColorMultiplyFadeData, fadeData);
            m_rnrRimlights[i].SetPropertyBlock(m_MaterialPropertyBlock);
        }
    }

    private IEnumerator ProcCheckGround()
    {
        bool _isPlayedSound = false;
        yield return new WaitForFixedUpdate();
        MyUtility.CoroutineDelay delay = new MyUtility.CoroutineDelay(0.1f);

        float groundOffsetY = 0;

        if (m_refTmRoot != null)
            groundOffsetY = m_refTmRoot.position.y;
        while (true)
        {
            delay.Reset();
            if (m_Rigidbody.velocity.sqrMagnitude < 0.001f)
            {
                EventPullDeskDrawerHandler.AddObjectInsideDrawer(this);
                if (Pivot.position.y - groundOffsetY < 0.2f + ExtentsSize * 1.1f)
                {
                    if (GameBoundary.IsPointInBoundary(Pivot.position))
                    {
                        //m_fTargetEmissionMultiply = m_fEmissionMultiplyOnGround;
                        SetTextureMultiply(m_fEmissionMultiplyOnGround);
                    }
                    else
                    {
                        SetTextureMultiply(0);
                        //m_fTargetEmissionMultiply = 0f;
                    }
                    
                    GameSound.Instance.Play(eSoundID_Common.ItemDrop, Pivot.position);
                    break;
                }
                else
                {
                    if (!_isPlayedSound)
                    {
                        _isPlayedSound = true;
                        GameSound.Instance.Play(eSoundID_Common.ItemDrop, Pivot.position);
                    }
                }
            }
            if (!delay.IsEnd)
                yield return null;
        }
    }
  
    protected override void Awake()
    {
        base.Awake();
        if (m_rnrRimlights == null || m_rnrRimlights.Length == 0)
            m_rnrRimlights = GetComponentsInChildren<Renderer>(true);
        InteractObjectCollection<InteractiveItem>.Instance.AddItem(this);
        RecordTransform();

        m_InitRigidbody_isKinematic = m_Rigidbody.isKinematic;
        m_InitRigidbody_useGravity = m_Rigidbody.useGravity;
        
        m_MaterialPropertyBlock = new MaterialPropertyBlock();

        //m_refHintBasic.Initialize();
        m_refHintBasic.onActiveHint += M_refHintBasic_onActiveHint;
        m_refHintBasic.onUnActiveHint += M_refHintBasic_onUnActiveHint;

        GameState.onChangeGameState += GameState_onChangeGameState;
        SetTextureMultiply(0);
        SetRimFactor(0);
    }
    private void GameState_onChangeGameState(eGameState arg1, eGameState arg2)
    {
        switch (arg2)
        {
            case eGameState.Standby:
                StartCheckGround();
                break;
        }
    }
    //
    private void M_refHintBasic_onUnActiveHint() {  SetRimFactor(0);}
    private void M_refHintBasic_onActiveHint() {SetRimFactor(1);}
    //

    private void Start()
    {
        StartCheckGround();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InteractObjectCollection<InteractiveItem>.Instance.RemoveItem(this);
        m_refHintBasic.Release();
        GameState.onChangeGameState -= GameState_onChangeGameState;
        EventPullDeskDrawerHandler.RemoveObjectInsideDrawer(this);
    }
    protected override void Restore()
    {
        base.Restore();
        
        SetTextureMultiply(0);
        SetRimFactor(0);
        SetRigidbodyValue(m_InitRigidbody_useGravity, m_InitRigidbody_isKinematic);
        m_Rigidbody.detectCollisions = true;
        SetTransformRecorded();

        m_bIsPlayedGrabVoice = false;
    }

    protected override void OnFirstGrab()
    {
        m_refHintBasic.UnActiveHint();
    }
    protected override void OnGrab()
    {
        EventPullDeskDrawerHandler.RemoveObjectInsideDrawer(this);
        StopCheckGround();
        PlayGrabVoice();

        if (MyNetworkManager.IsMyID(m_GrabData.GrabberID))
            SetTextureMultiply(m_fEmissionMultiplyWhenGrab);
        else
            SetTextureMultiply(0);

        Pivot.parent = m_refTmRoot;
        SetRigidbodyValue(false, false);
    }
    protected override void OnUnGrab()
    {
        StopPivotProcessing();
        
        SetTextureMultiply(0);
        Pivot.parent = m_refTmRoot;

        Vector3 velocity = m_GrabData.Velocity;

        Pivot.position = m_GrabData.Position;
        Pivot.rotation = m_GrabData.Rotation;
        
        StartCheckGround();

        SetRigidbodyValue(true, false, m_GrabData.Velocity, Vector3.zero);
    }
    protected override void OnFixedGrabbing()
    {

        if (m_GrabData.GrabHandTransform == null)
        {
            m_Rigidbody.velocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            return;
        }
        //float maxDistanceDelta = 10;
        //Vector3 positionDelta = m_GrabData.GrabHandTransform.TransformPoint(m_GrabData.Position) - Pivot.position;
        //Quaternion rotationDelta = m_GrabData.GrabHandTransform.rotation * m_GrabData.Rotation * Quaternion.Inverse(Pivot.rotation);

        //rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
        //angle = ((angle > 180) ? angle -= 360 : angle);

        //float angularVelocityLimit = float.PositiveInfinity;
        //float velocityLimit = float.PositiveInfinity;

        //Vector3 velocityTarget = positionDelta / Time.smoothDeltaTime;
        ////Debug.Log(velocityTarget.magnitude);
        //Vector3 calculatedVelocity = Vector3.MoveTowards(m_Rigidbody.velocity, velocityTarget, maxDistanceDelta);

        //if (velocityLimit == float.PositiveInfinity || calculatedVelocity.sqrMagnitude < velocityLimit)
        //{
        //    m_Rigidbody.velocity = calculatedVelocity;
        //}

        //if (angle != 0)
        //{
        //    Vector3 angularTarget = angle * axis;
        //    Vector3 calculatedAngularVelocity = Vector3.MoveTowards(m_Rigidbody.angularVelocity, angularTarget, 360);

        //    if (angularVelocityLimit == float.PositiveInfinity || calculatedAngularVelocity.sqrMagnitude < angularVelocityLimit)
        //    {
        //        m_Rigidbody.angularVelocity = calculatedAngularVelocity;
        //    }
        //}

        float maxDistanceDelta = 10f;
        Vector3 positionDelta = m_GrabData.GrabHandTransform.TransformPoint(m_GrabData.Position) - Pivot.position;
        Quaternion rotationDelta = m_GrabData.GrabHandTransform.rotation * m_GrabData.Rotation * Quaternion.Inverse(Pivot.rotation);

        float angle;
        Vector3 axis;
        rotationDelta.ToAngleAxis(out angle, out axis);

        angle = ((angle > 180) ? angle -= 360 : angle);

        float angularVelocityLimit = float.PositiveInfinity;
        float velocityLimit = float.PositiveInfinity;
        if (angle != 0)
        {
            Vector3 angularTarget = angle * axis;
            Vector3 calculatedAngularVelocity = Vector3.MoveTowards(m_Rigidbody.angularVelocity, angularTarget, 1000);
            if (angularVelocityLimit == float.PositiveInfinity || calculatedAngularVelocity.sqrMagnitude < angularVelocityLimit)
            {
                m_Rigidbody.angularVelocity = calculatedAngularVelocity;
            }
        }

        Vector3 velocityTarget = positionDelta / Time.fixedDeltaTime;
        Vector3 calculatedVelocity = Vector3.MoveTowards(m_Rigidbody.velocity, velocityTarget, maxDistanceDelta);

        if (velocityLimit == float.PositiveInfinity || calculatedVelocity.sqrMagnitude < velocityLimit)
        {
            m_Rigidbody.velocity = calculatedVelocity;
        }
    }

    protected void StartCheckGround()
    {
        StopCheckGround();        
        m_ProcCheckGround = ProcCheckGround();
        StartCoroutine(m_ProcCheckGround);
    }
    protected void StopCheckGround()
    {
        if (m_ProcCheckGround != null)
        {
            StopCoroutine(m_ProcCheckGround);
            m_ProcCheckGround = null;
        }
    }

    protected void SetRigidbodyValue(bool _useGravity, bool _isKinematic)
    {
        SetRigidbodyValue(_useGravity, _isKinematic, Vector3.zero, Vector3.zero);
    }
    protected void SetRigidbodyValue(bool _useGravity, bool _isKinematic, Vector3 _velocity, Vector3 _angularVelocity)
    {
        m_Rigidbody.useGravity = _useGravity;
        m_Rigidbody.isKinematic = _isKinematic;
        m_Rigidbody.angularVelocity = _angularVelocity;
        m_Rigidbody.velocity = _velocity;
    }

    public override void ListenInternalMessage(eInternalInteractMessage _internalInteractMsg)
    {
        base.ListenInternalMessage(_internalInteractMsg);
        switch (_internalInteractMsg)
        {
            case eInternalInteractMessage.Disable:
                //m_fTargetEmissionMultiply = 0;
                SetTextureMultiply(0);
                StopCheckGround();
                break;
            case eInternalInteractMessage.Enable_Physics: SetRigidbodyValue(true, false); break;
            case eInternalInteractMessage.Disable_Physics: SetRigidbodyValue(false, true); break;
            case eInternalInteractMessage.Detect_Collision: DetectCollision(); break;
            case eInternalInteractMessage.UnDetect_Collision: UnDetectCollision(); break;
            case eInternalInteractMessage.Freeze_Rigidbody_Rotation: FreezeRigidbodyRotation(); break;
            case eInternalInteractMessage.UnFreeze_Rigidbody_Rotation: UnFreezeRigidbodyRotation(); break;
            case eInternalInteractMessage.Start_Check_Ground: StartCheckGround(); break;
            case eInternalInteractMessage.Stop_Check_Ground: StopCheckGround(); break;
        }
    }

    private void FreezeRigidbodyRotation() { if (m_Rigidbody != null) m_Rigidbody.freezeRotation = true; }
    private void UnFreezeRigidbodyRotation() { if (m_Rigidbody != null) m_Rigidbody.freezeRotation = false; }

    private void DetectCollision() { m_Rigidbody.detectCollisions = true; }
    private void UnDetectCollision() { m_Rigidbody.detectCollisions = false; }


    #region GrabVoice
    [HideInInspector] [SerializeField] protected bool m_bUseGrabVoice = false;
    [HideInInspector] [SerializeField] protected bool m_bIsPlayedGrabVoice;
    [HideInInspector] [SerializeField] protected eNPCSoundType m_GrabSoundType;
    [HideInInspector] [SerializeField] protected int m_GrabSoundID;
    protected void PlayGrabVoice()
    {
        if (m_bUseGrabVoice)
        {
            if (!m_bIsPlayedGrabVoice)
            {
                m_bIsPlayedGrabVoice = true;
                GameManagers.GameSound.Instance.PlayNPCbyID(m_GrabSoundType, m_GrabSoundID);
            }
        }
    }
    protected void PlayForceGrabVoice()
    {
        if (!m_bIsPlayedGrabVoice)
        {
            m_bIsPlayedGrabVoice = true;
            GameManagers.GameSound.Instance.PlayNPCForcebyID(m_GrabSoundType, m_GrabSoundID, 0);
        }
    }
    #endregion


}


#if UNITY_EDITOR
[CustomEditor(typeof(InteractiveItem),true), CanEditMultipleObjects]
public class EventAbleItemInspector : Editor
{
    //public override void OnInspectorGUI()
    //{

    //    base.OnInspectorGUI();
        
    //    EditorGUILayout.Separator();
        
    //    SerializedProperty playVoice = serializedObject.FindProperty("m_bUseGrabVoice");
    //    EditorGUILayout.PropertyField(playVoice, new GUIContent("NPC 보이스 사용"));
    //    if (playVoice.boolValue)
    //    {

    //        SerializedProperty grabVoiceType = serializedObject.FindProperty("m_GrabSoundType");
    //        SerializedProperty grabSoundID = serializedObject.FindProperty("m_GrabSoundID");
    //        EditorGUILayout.PropertyField(grabVoiceType, new GUIContent("NPC 사운드 타입"));
    //        eNPCSoundType npcSoundType = (eNPCSoundType)grabVoiceType.intValue;
    //        System.Type type = null;
    //        if (npcSoundType == eNPCSoundType.Doctor)
    //        {
    //            type = typeof(eNPCSoundID_Doctor);
    //        }
    //        else if (npcSoundType == eNPCSoundType.Amy)
    //        {
    //            type = typeof(eNPCSoundID_Amy);
    //        }

    //        object enumObj = null;
    //        if (type != null)
    //        {
    //            if(System.Enum.IsDefined(type, grabSoundID.intValue))
    //                enumObj = System.Enum.ToObject(type, grabSoundID.intValue);
    //            else
    //                enumObj = System.Enum.ToObject(type, 0);
    //        }
    //        if (enumObj != null)
    //        {
    //            System.Enum e = (System.Enum)enumObj;
    //            e = EditorGUILayout.EnumPopup(type.Name, e);
    //            object obj2 = (object)e;
    //            grabSoundID.intValue = (int)obj2;
    //        }
    //        EditorGUILayout.Separator();
    //    }
    //    serializedObject.ApplyModifiedProperties();
    //}
}
#endif