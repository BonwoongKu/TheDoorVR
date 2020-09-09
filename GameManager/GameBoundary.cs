using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameManagers
{
    public class GameBoundary : SingletoneMono<GameBoundary>
    {
        private class BoundaryDetector
        {
            public int ID { get; set; }
            private Vector3[] m_arrVecBoundaryPoint;
            private GameBoundaryPointData boundaryPointData;
            private Vector3 m_vecMinBoundaryPoint;
            private Vector3 m_vecMaxBoundaryPoint;

            public void SetDetectorData(int _key, Vector3[] _boundaryPoint)
            {
                ID = _key;
                m_arrVecBoundaryPoint = _boundaryPoint;

                m_vecMinBoundaryPoint.x = m_arrVecBoundaryPoint[0].x;
                m_vecMinBoundaryPoint.z = m_arrVecBoundaryPoint[0].z;
                m_vecMaxBoundaryPoint.x = m_arrVecBoundaryPoint[0].x;
                m_vecMaxBoundaryPoint.z = m_arrVecBoundaryPoint[0].z;

                for (int i = 1; i < m_arrVecBoundaryPoint.Length; i++)
                {
                    Vector3 point = m_arrVecBoundaryPoint[i];
                    m_vecMinBoundaryPoint.x = Mathf.Min(point.x, m_vecMinBoundaryPoint.x);
                    m_vecMinBoundaryPoint.z = Mathf.Min(point.z, m_vecMinBoundaryPoint.z);

                    m_vecMaxBoundaryPoint.x = Mathf.Max(point.x, m_vecMaxBoundaryPoint.x);
                    m_vecMaxBoundaryPoint.z = Mathf.Max(point.z, m_vecMaxBoundaryPoint.z);
                }
            }
            public void SetDetectorData_Renewal(int _key, GameBoundaryPointData _boundaryPointData)
            {
                ID = _key;
                boundaryPointData = _boundaryPointData;
                RecalculateMaxAndMin();
            }
            public BoundaryDetector(int _key, GameBoundaryPointData _boundaryPointData)
            {
                SetDetectorData_Renewal(_key, _boundaryPointData);
            }
            public BoundaryDetector(int _key, Vector3[] _boundaryPoint)
            {
                SetDetectorData(_key, _boundaryPoint);
            }
            private void RecalculateMaxAndMin()
            {
                m_vecMinBoundaryPoint.x = boundaryPointData.BoundaryPoints[0].position.x;
                m_vecMinBoundaryPoint.z = boundaryPointData.BoundaryPoints[0].position.z;
                m_vecMaxBoundaryPoint.x = boundaryPointData.BoundaryPoints[0].position.x;
                m_vecMaxBoundaryPoint.z = boundaryPointData.BoundaryPoints[0].position.z;

                for (int i = 1; i < boundaryPointData.BoundaryPoints.Length; i++)
                {
                    Vector3 point = boundaryPointData.BoundaryPoints[i].position;
                    m_vecMinBoundaryPoint.x = Mathf.Min(point.x, m_vecMinBoundaryPoint.x);
                    m_vecMinBoundaryPoint.z = Mathf.Min(point.z, m_vecMinBoundaryPoint.z);

                    m_vecMaxBoundaryPoint.x = Mathf.Max(point.x, m_vecMaxBoundaryPoint.x);
                    m_vecMaxBoundaryPoint.z = Mathf.Max(point.z, m_vecMaxBoundaryPoint.z);
                }
            }
            public bool IsPointInBoundary_Renewal(Vector3 _point)
            {
                bool isInside = false;
                RecalculateMaxAndMin();
                Vector3 maxBoundaryPoint = m_vecMaxBoundaryPoint;
                Vector3 minBoundaryPoint = m_vecMinBoundaryPoint;
                if (_point.x < minBoundaryPoint.x || _point.x > maxBoundaryPoint.x || _point.z < minBoundaryPoint.z || _point.z > maxBoundaryPoint.z)
                {
                    return false;
                }

                for (int i = 0, j = boundaryPointData.BoundaryPoints.Length - 1; i < boundaryPointData.BoundaryPoints.Length; j = i++)
                {
                    Vector3 boundaryPoint_J = boundaryPointData.BoundaryPoints[j].position;
                    Vector3 boundaryPoint_I = boundaryPointData.BoundaryPoints[i].position;

                    if ((boundaryPoint_I.z > _point.z) != (boundaryPoint_J.z > _point.z) &&
                         _point.x < (boundaryPoint_J.x - boundaryPoint_I.x) * (_point.z - boundaryPoint_I.z) / (boundaryPoint_J.z - boundaryPoint_I.z) + boundaryPoint_I.x)
                    {
                        isInside = !isInside;
                    }
                }

                return isInside;
            }
            public bool IsPointInBoundary(Vector3 _point)
            {
                bool isInside = false;

                Vector3 maxBoundaryPoint = m_vecMaxBoundaryPoint + GameBoundary.BoundaryOffset;
                Vector3 minBoundaryPoint = m_vecMinBoundaryPoint + GameBoundary.BoundaryOffset;
                if (_point.x < minBoundaryPoint.x || _point.x > maxBoundaryPoint.x || _point.z < minBoundaryPoint.z || _point.z > maxBoundaryPoint.z)
                {
                    return false;
                }

                for (int i = 0, j = m_arrVecBoundaryPoint.Length - 1; i < m_arrVecBoundaryPoint.Length; j = i++)
                {
                    Vector3 boundaryPoint_J = m_arrVecBoundaryPoint[j] + GameBoundary.BoundaryOffset;
                    Vector3 boundaryPoint_I = m_arrVecBoundaryPoint[i]+ GameBoundary.BoundaryOffset;

                    if ( (boundaryPoint_I.z > _point.z) != (boundaryPoint_J.z > _point.z) &&
                         _point.x < (boundaryPoint_J.x - boundaryPoint_I.x) * (_point.z - boundaryPoint_I.z) / (boundaryPoint_J.z - boundaryPoint_I.z) + boundaryPoint_I.x)
                    {
                        isInside = !isInside;
                    }
                }

                return isInside;
            }
        }
        private class BoundaryRenderer
        {
            private readonly Vector3 C_IGNORE_HAND_POSITION = new Vector3(0, 1000, 0);
            private const string C_LEFT_HAND_PROPERTY = "_HitPosition";
            private const string C_RIGHT_HAND_PROPERTY = "_HitPosition2";

            private Transform boundaryRoot;
            private Transform m_refTmBoundary;
            private MeshFilter m_refMeshFilterBoundary;
            private MeshRenderer m_refRendererBoundary;
            private Material m_refMatBoundary;
            private Mesh m_refMeshBoundary;

            public void SetParent(Transform _parent)
            {
                m_refTmBoundary.parent = _parent;
            }
            public BoundaryRenderer(Transform _parent)
            {
                GameObject go = new GameObject("BoundaryRenderer");
                m_refTmBoundary = go.transform;

                m_refTmBoundary.parent = _parent;
                m_refTmBoundary.localPosition = Vector3.zero;
                m_refTmBoundary.localRotation = Quaternion.identity;
                m_refTmBoundary.localScale = Vector3.one;

                m_refMeshFilterBoundary = go.AddComponent<MeshFilter>();
                m_refRendererBoundary = go.AddComponent<MeshRenderer>();

                //m_refMatBoundary = Resources.Load<Material>("Shader/SafeZone/matSafeZone");
                m_refMatBoundary = new Material(Shader.Find("Hidden/Common/SafeZone"));
                Texture2D tex = new Texture2D(25, 25,TextureFormat.RGBA32, true);
                
                for (int y = 0; y < 25; y++)
                {
                    for (int x = 0; x < 25; x++)
                    {
                        if (x == 0 || x == 24 || y == 0 || y == 24)
                        {
                            tex.SetPixel(x, y, Color.white);
                        }
                        else
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                }
                
                //tex.alphaIsTransparency = true;
                tex.Apply();
                m_refMatBoundary.SetTexture("_TextureSample0", tex);
                m_refMatBoundary.SetFloat("_Int0", 50);
                m_refMatBoundary.SetFloat("_HitSize", 0.15f);
                m_refMatBoundary.SetColor("_HitColor", Color.red);

                m_refRendererBoundary.receiveShadows = false;
                m_refRendererBoundary.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                m_refRendererBoundary.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                m_refRendererBoundary.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                m_refRendererBoundary.material = m_refMatBoundary;
                m_refMeshBoundary = m_refMeshFilterBoundary.mesh;
            }


            private float height = 6;
            public void SetBoundaryMesh(Vector3[] _points)
            {

                m_refMeshBoundary.Clear();
                int resX = _points.Length; // 2 minimum
                int resY = 2;
                #region Vertices		
                Vector3[] vertices;
                vertices = new Vector3[(resX) * (resY)];
                int vertexIndex = 0;
                for (int y = 0; y < resY; y++)
                {
                    for (int x = 0; x < resX; x++)
                    {
                        vertices[vertexIndex] = new Vector3(_points[x].x, _points[x].y + y * height, _points[x].z);
                        vertexIndex++;
                    }
                }
                #endregion

                #region UVs		
                Vector2[] uvs;
                uvs = new Vector2[vertices.Length];
                Vector3 lastPosition = _points[0];
                for (int v = 0; v < resY; v++)
                {
                    float calculateDistance = 0;
                    for (int u = 0; u < resX; u++)
                    {

                        float dist = Vector3.Distance(_points[u], lastPosition);
                        calculateDistance += dist;
                        uvs[u + v * resX] = new Vector2(calculateDistance, v * height);
                        lastPosition = _points[u];
                    }
                }

                #endregion
                #region Triangles

                int triangleCount = (resX - 1) * (resY - 1);
                int[] triangles = new int[triangleCount * 6];
                for (int i = 0; i < triangleCount; i++)
                {
                    int triangleIndex = i * 6;

                    triangles[triangleIndex] = i;
                    triangles[triangleIndex + 1] = i + resX;
                    triangles[triangleIndex + 2] = i + 1;

                    triangles[triangleIndex + 3] = i + 1;
                    triangles[triangleIndex + 4] = i + resX;
                    triangles[triangleIndex + 5] = resX + i + 1;

                }
                #endregion

                m_refMeshBoundary.vertices = vertices;
                m_refMeshBoundary.uv = uvs;
                m_refMeshBoundary.triangles = triangles;

                m_refMeshBoundary.RecalculateBounds();
            }

            public void SetBoundaryMesh_Renewal( GameBoundaryPointData _pointData)
            {
                boundaryRoot = _pointData.BoundaryRoot;
                GameLoop.onLateUpdate -= GameLoop_onLateUpdate;
                if (_pointData.BoundaryRoot != null)
                {
                    boundaryRoot = _pointData.BoundaryRoot;
                    m_refTmBoundary.position = boundaryRoot.position;
                    m_refTmBoundary.rotation = boundaryRoot.rotation;
                    GameLoop.onLateUpdate += GameLoop_onLateUpdate;
                }
                else
                {
                    m_refTmBoundary.position = Vector3.zero;
                    m_refTmBoundary.rotation = Quaternion.identity;
                }

                m_refMeshBoundary.Clear();
                

                Vector3[] _points = new Vector3[_pointData.BoundaryPoints.Length];
                for (int i = 0; i < _points.Length; i++)
                {
                    _points[i] = _pointData.BoundaryPoints[i].position - m_refTmBoundary.position;
                }
                int resX = _points.Length; // 2 minimum
                int resY = 2;
                #region Vertices		
                Vector3[] vertices;
                vertices = new Vector3[(resX) * (resY)];
                int vertexIndex = 0;
                for (int y = 0; y < resY; y++)
                {
                    for (int x = 0; x < resX; x++)
                    {
                        vertices[vertexIndex] = new Vector3(_points[x].x, _points[x].y + y * height, _points[x].z);
                        vertexIndex++;
                    }
                }
                #endregion

                #region UVs		
                Vector2[] uvs;
                uvs = new Vector2[vertices.Length];
                Vector3 lastPosition = _points[0];
                for (int v = 0; v < resY; v++)
                {
                    float calculateDistance = 0;
                    for (int u = 0; u < resX; u++)
                    {

                        float dist = Vector3.Distance(_points[u], lastPosition);
                        calculateDistance += dist;
                        uvs[u + v * resX] = new Vector2(calculateDistance, v * height);
                        lastPosition = _points[u];
                    }
                }

                #endregion
                #region Triangles

                int triangleCount = (resX - 1) * (resY - 1);
                int[] triangles = new int[triangleCount * 6];
                for (int i = 0; i < triangleCount; i++)
                {
                    int triangleIndex = i * 6;

                    triangles[triangleIndex] = i;
                    triangles[triangleIndex + 1] = i + resX;
                    triangles[triangleIndex + 2] = i + 1;

                    triangles[triangleIndex + 3] = i + 1;
                    triangles[triangleIndex + 4] = i + resX;
                    triangles[triangleIndex + 5] = resX + i + 1;

                }
                #endregion

                m_refMeshBoundary.vertices = vertices;
                m_refMeshBoundary.uv = uvs;
                m_refMeshBoundary.triangles = triangles;

                m_refMeshBoundary.RecalculateBounds();
            }

            private void GameLoop_onLateUpdate()
            {
                if (boundaryRoot != null)
                {
                    m_refTmBoundary.position = boundaryRoot.position;
                    m_refTmBoundary.rotation = boundaryRoot.rotation;
                }
            }

            public void SetLeftHandPosition(bool _isInIgnoreBoundary, Vector3 _rightHand)
            {
                if (_isInIgnoreBoundary)
                {
                    m_refMatBoundary.SetVector(C_LEFT_HAND_PROPERTY, C_IGNORE_HAND_POSITION);
                }
                else
                {
                    Vector3 inverseRightHandPosition = m_refTmBoundary.InverseTransformPoint(_rightHand);
                    m_refMatBoundary.SetVector(C_LEFT_HAND_PROPERTY, inverseRightHandPosition);
                }
            }
            public void SetRightHandPosition(bool _isInIgnoreBoundary, Vector3 _rightHand)
            {
                if (_isInIgnoreBoundary)
                {
                    m_refMatBoundary.SetVector(C_RIGHT_HAND_PROPERTY, C_IGNORE_HAND_POSITION);
                }
                else
                {
                    Vector3 inverseRightHandPosition = m_refTmBoundary.InverseTransformPoint(_rightHand);
                    m_refMatBoundary.SetVector(C_RIGHT_HAND_PROPERTY, inverseRightHandPosition);
                }

            }
        }

        protected override void Awake()
        {
            base.Awake();

            GameLoop.onUpdatePrologue += GameLoop_onUpdatePrologue;
            GameLoop.onUpdatePlay += GameState_onGamePlayUpdate;
            GameState.onChangeGameState += GameState_onChangeGameState;
        }

        private void GameLoop_onUpdatePrologue()
        {
            ExecuteIsDeviceOutOfBoundary();
        }

        private void GameState_onChangeGameState(eGameState arg1, eGameState arg2)
        {
            switch (arg2)
            {
                case eGameState.Standby:
                    SelectBoundary(0);
                    break;
                case eGameState.Finish:
                    g_refBoundaryRenderer.SetLeftHandPosition(true, Vector3.zero);
                    g_refBoundaryRenderer.SetRightHandPosition(true, Vector3.zero);
                    break;
            }
        }

        private void OnDestroy()
        {
            GameLoop.onUpdatePrologue -= GameLoop_onUpdatePrologue;
            GameLoop.onUpdatePlay -= GameState_onGamePlayUpdate;
            GameState.onChangeGameState -= GameState_onChangeGameState;
        }

        private void GameState_onGamePlayUpdate() { ExecuteIsDeviceOutOfBoundary(); }


        private Dictionary<int, Vector3[]> m_dicBoundaryPoints;
        private Dictionary<int, GameBoundaryPointData> m_dicBoundaryPointData;
        private static BoundaryDetector g_refSelectedDetector;
        private static BoundaryRenderer g_refBoundaryRenderer;
        private static Bounds g_stIgnoreBoundaryArea = new Bounds(Vector3.zero, Vector3.zero);

        private bool m_bWasHeadInsideOfBoundary = true;
        private bool m_bWasLeftHandInsideOfBoundary = true;
        private bool m_bWasRightHandInsideOfBoundary = true;
        private bool m_bWasAllInsideOfBoundary = true;

        public static event System.Action<bool> onBoundHead;
        public static event System.Action<bool> onBoundLeftHand;
        public static event System.Action<bool> onBoundRightHand;
        public static event System.Action<bool> onBoundAll;

        public static Vector3 BoundaryOffset { get; set; }

        private bool IsRegistedBoundaryDetector(int _key)
        {
            if (m_dicBoundaryPoints == null)
                m_dicBoundaryPoints = new Dictionary<int, Vector3[]>();

            if (m_dicBoundaryPointData == null)
                m_dicBoundaryPointData = new Dictionary<int, GameBoundaryPointData>();

            return m_dicBoundaryPointData.ContainsKey(_key);
            //return m_dicBoundaryPoints.ContainsKey(_key);
        }

        public void SetIgnoreBoundary(Bounds _bounds)
        {
            g_stIgnoreBoundaryArea = _bounds;
        }

        public void RegistBoundaryPointData(int _key, Vector3[] _points)
        {
            if (!IsRegistedBoundaryDetector(_key)) m_dicBoundaryPoints.Add(_key, _points);

            if (_key == 0)
                SelectBoundary(0);
        }
        public void RegistBoundaryPointData(int _key, GameBoundaryPointData _boundaryPointData)
        {
            if (!IsRegistedBoundaryDetector(_key)) m_dicBoundaryPointData.Add(_key, _boundaryPointData);

            if (_key == 0)
                SelectBoundary(0);
        }
        public void UnRegistBoundaryPointData(int _key)
        {
            if (IsRegistedBoundaryDetector(_key))
            {
                m_dicBoundaryPoints.Remove(_key);
                m_dicBoundaryPointData.Remove(_key);
            }
            

            if (g_refSelectedDetector != null && g_refSelectedDetector.ID == _key)
            {
                g_refSelectedDetector = null;
            }
        }

        public void SelectBoundary(int _index)
        {
            if (!IsRegistedBoundaryDetector(_index)) return;

            if (g_refSelectedDetector != null) g_refSelectedDetector.SetDetectorData_Renewal(_index, m_dicBoundaryPointData[_index]);
            else g_refSelectedDetector = new BoundaryDetector(_index, m_dicBoundaryPointData[_index]);

            if (g_refBoundaryRenderer == null)
                g_refBoundaryRenderer = new BoundaryRenderer(transform);

            g_refBoundaryRenderer.SetBoundaryMesh_Renewal(m_dicBoundaryPointData[_index]);
        }

        public static bool IsPointInBoundary(Vector3 _point)
        {
            if (g_refSelectedDetector == null)
                return false;
            else
                return g_refSelectedDetector.IsPointInBoundary_Renewal(_point);
        }

        private void ExecuteIsDeviceOutOfBoundary()
        {
            if (g_refSelectedDetector == null)
                return;

            Vector3 head = VRInputManager.GetDevicePosition(VRInputManager.PositionType.HMD);
            Vector3 leftHand = VRInputManager.GetControllerPosition(VRHand.eHandType.Left, VRInputManager.PositionType.CONTROLLER);
            Vector3 RightHand = VRInputManager.GetControllerPosition(VRHand.eHandType.Right, VRInputManager.PositionType.CONTROLLER);
            bool isInIgnoreZoneHead = false;
            bool isInIgnoreZoneLeftHand = false;
            bool isInIgnoreZoneRightHand = false;
            if (g_stIgnoreBoundaryArea != null)
            {
                isInIgnoreZoneHead = g_stIgnoreBoundaryArea.Contains(head);
                isInIgnoreZoneLeftHand = g_stIgnoreBoundaryArea.Contains(leftHand);
                isInIgnoreZoneRightHand = g_stIgnoreBoundaryArea.Contains(RightHand);
            }
            
            

            bool isInHead = g_refSelectedDetector.IsPointInBoundary_Renewal(head);
            bool isInLeftHand = g_refSelectedDetector.IsPointInBoundary_Renewal(leftHand);
            bool isInRightHand = g_refSelectedDetector.IsPointInBoundary_Renewal(RightHand);

            g_refBoundaryRenderer.SetLeftHandPosition(isInIgnoreZoneLeftHand, leftHand);
            g_refBoundaryRenderer.SetRightHandPosition(isInIgnoreZoneRightHand, RightHand);

            isInHead = isInHead || isInIgnoreZoneHead;
            isInLeftHand = isInLeftHand || isInIgnoreZoneLeftHand;
            isInRightHand = isInRightHand || isInIgnoreZoneRightHand;

            bool isInAll = isInHead && isInLeftHand && isInRightHand;

            if (m_bWasHeadInsideOfBoundary != isInHead) { onBoundHead?.Invoke(isInHead); }
            if (m_bWasLeftHandInsideOfBoundary != isInLeftHand) { onBoundLeftHand?.Invoke(isInLeftHand); }
            if (m_bWasRightHandInsideOfBoundary != isInRightHand) { onBoundRightHand?.Invoke(isInRightHand); }
            if (m_bWasAllInsideOfBoundary != isInAll) { onBoundAll?.Invoke(isInAll); }


            m_bWasHeadInsideOfBoundary = isInHead;
            m_bWasLeftHandInsideOfBoundary = isInLeftHand;
            m_bWasRightHandInsideOfBoundary = isInRightHand;
            m_bWasAllInsideOfBoundary = isInAll;
        }
    }
}

