using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKTEST
{
    public class AdTrackingManager : MonoBehaviour
    {
        public static AdTrackingManager Instance { get; private set; }

        private readonly List<AdTracker> activeTrackers = new List<AdTracker>();
        private int currentIndex = 0;
        [SerializeField] private int trackersToCheckPerFrame = 2;
        [Tooltip("Character Camera")]
        [SerializeField] private Camera mainCamera;
        [Tooltip("Debug Mode")]
        [SerializeField] private bool globalDebugMode = false;
        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            if (mainCamera == null)
                Debug.LogError("AdTrackingManager: No Camera Attached!");
        }

        void Update()
        {
            if (mainCamera == null || activeTrackers.Count == 0) return;
            for (int i = 0; i < trackersToCheckPerFrame; i++)
            {
                if (currentIndex >= activeTrackers.Count)
                {
                    currentIndex = 0;
                }
                if (activeTrackers[currentIndex] != null)
                {
                    activeTrackers[currentIndex].ProcessVisibilityCheck(mainCamera, globalDebugMode);
                }
                currentIndex++;
            }
        }

        public void RegisterTracker(AdTracker tracker)
        {
            if (!activeTrackers.Contains(tracker))
                activeTrackers.Add(tracker);
        }

        public void UnRegisterTracker(AdTracker tracker)
        {
            if (activeTrackers.Contains(tracker))
                activeTrackers.Remove(tracker);
        }
    }
}