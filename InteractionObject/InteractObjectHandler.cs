using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManagers;


namespace InteractSystem
{
    public class InteractObjectHandler
    {
        private static Dictionary<int, InteractObjectAbstract> m_dicRegistedRayTargets;
        private static Dictionary<eTrick, List<InteractObjectAbstract>> m_dicRegistedEventObject;
        private static Dictionary<uint, InteractObjectAbstract> m_dicRegistedNetworkInteractObjects;

        static InteractObjectHandler()
        {
            m_dicRegistedRayTargets = new Dictionary<int, InteractObjectAbstract>();
            m_dicRegistedEventObject = new Dictionary<eTrick, List<InteractObjectAbstract>>();
            m_dicRegistedNetworkInteractObjects = new Dictionary<uint, InteractObjectAbstract>();
        }

        public static void Add_EventAbleObject( InteractObjectAbstract _eventAbleObj, bool _addAll = false)
        {
            if (_eventAbleObj.GetType().IsSubclassOf(typeof(EventRayAbstract)))
            {
                if (_eventAbleObj.DetectionArea != null)
                {
                    RegistRayTargetEventObject(_eventAbleObj.DetectionArea.GetInstanceID(), _eventAbleObj);
                }
                if(!_addAll)
                    return;
            }

            eTrick[] usingTricks = GameTrick.GetTricksFromMaskValue(_eventAbleObj.UsingTrick);

            for (int i = 0; i < usingTricks.Length; i++)
            {
                AddToLocalDictionary(usingTricks[i], _eventAbleObj);
            }

            RegistNetworkEventObject(_eventAbleObj);
        }
        public static void Remove_EventAbleObject(InteractObjectAbstract _eventAbleObj, bool _removeAll = false)
        {
            if (_eventAbleObj.GetType().IsSubclassOf(typeof(EventRayAbstract)))
            {
                if (_eventAbleObj.DetectionArea != null)
                {
                    UnRegistRayTargetObject(_eventAbleObj.DetectionArea.GetInstanceID());
                }

                if (!_removeAll)
                    return;
            }

            eTrick[] usingTricks = GameTrick.GetTricksFromMaskValue(_eventAbleObj.UsingTrick);

            for (int i = 0; i < usingTricks.Length; i++)
            {
                RemoveToLocalDictionary(usingTricks[i], _eventAbleObj);
            }
            UnRegistNetworkEventObject(_eventAbleObj);
        }

        private static void AddToLocalDictionary(eTrick _trickType, InteractObjectAbstract _eventBasicAbstract)
        {
            if (!m_dicRegistedEventObject.ContainsKey(_trickType))
                m_dicRegistedEventObject.Add(_trickType, new List<InteractObjectAbstract>());

            if (!m_dicRegistedEventObject[_trickType].Contains(_eventBasicAbstract))
                m_dicRegistedEventObject[_trickType].Add(_eventBasicAbstract);
        }
        private static void RemoveToLocalDictionary(eTrick _trickType, InteractObjectAbstract _eventBasicAbstract)
        {
            if (m_dicRegistedEventObject.ContainsKey(_trickType))
            {
                if (m_dicRegistedEventObject[_trickType].Contains(_eventBasicAbstract))
                    m_dicRegistedEventObject[_trickType].Remove(_eventBasicAbstract);

                if (m_dicRegistedEventObject[_trickType].Count == 0)
                    m_dicRegistedEventObject.Remove(_trickType);
            }
        }
        public static void RegistNetworkEventObject(InteractObjectAbstract _eventBasic)
        {
            uint uid = _eventBasic.UID;
            if (!m_dicRegistedNetworkInteractObjects.ContainsKey(uid)) { m_dicRegistedNetworkInteractObjects.Add(uid, _eventBasic); }
        }
        public static void UnRegistNetworkEventObject(InteractObjectAbstract _eventBasic)
        {
            uint uid = _eventBasic.UID;
            if (m_dicRegistedNetworkInteractObjects.ContainsKey(uid)) { m_dicRegistedNetworkInteractObjects.Remove(uid); }
        }

        private static void RegistRayTargetEventObject(int _instanceID, InteractObjectAbstract _eventBasic)
        {
            if (!m_dicRegistedRayTargets.ContainsKey(_instanceID))
            {
                m_dicRegistedRayTargets.Add(_instanceID, _eventBasic);
            }
        }
        private static void UnRegistRayTargetObject(int _instanceID)
        {
            if (m_dicRegistedRayTargets.ContainsKey(_instanceID))
            {
                m_dicRegistedRayTargets.Remove(_instanceID);
            }
        }
        public static bool GetRayTargetObject(int _instanceID, out InteractObjectAbstract _rayTargetObject)
        {
            return m_dicRegistedRayTargets.TryGetValue(_instanceID, out _rayTargetObject);
        }

        public static bool GetEventObjectByNetID(uint _netID, out InteractObjectAbstract _netObject)
        {
            return m_dicRegistedNetworkInteractObjects.TryGetValue(_netID, out _netObject);
        }

        public static List<InteractObjectAbstract> GetCurrentEventAbleObjectList(eTrick _trickType)
        {
            if (m_dicRegistedEventObject.ContainsKey(_trickType))
            {
                return m_dicRegistedEventObject[_trickType];
            }
            else
            {
                m_dicRegistedEventObject.Add(_trickType, new List<InteractObjectAbstract>());
                return m_dicRegistedEventObject[_trickType];
            }
        }
    }
}








