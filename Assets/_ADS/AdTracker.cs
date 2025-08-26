using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKTEST
{
    [RequireComponent(typeof(AdPlacement), typeof(Renderer), typeof(MeshFilter))]
    public class AdTracker : MonoBehaviour
    {
        [Tooltip("how long the ad must be visible(seconds) to count as an impression")]
        [SerializeField] private float timeForValidImpression = 2.0f;

        [Tooltip("controls how directly the camera must face the ad. 0 is a 90 degree angle, 0.7 is  around 45 degrees")]
        [Range(0f, 1f)]
        [SerializeField] private float requiredViewAngle = 0.7f;

        [Tooltip("how much the ad's surface must face the camera")]
        [SerializeField] private float requiredSurfaceAlignment = 1f;

        [Range(0f, 10f)]
        [SerializeField] private float minScreenCoveragePercent = 0.5f;

        [Tooltip("Which layers can block the view of the ad")]
        [SerializeField] private LayerMask occlusionLayers;

        [Header("Component References")]
        [SerializeField] private AdPlacement m_AdPlacement;
        [SerializeField] private Renderer m_AdRenderer;
        [SerializeField] private MeshFilter m_MeshFilter;

        private Vector3[] localCorners;
        private Vector3[] pointsToCheck;
        private Vector3[] worldCorners;
        private bool impressionLogged = false;
        private bool isContinuouslyVisible = false;
        private float visibilityStartTime = 0f;

        void Awake()
        {
            if (m_AdPlacement == null || m_AdRenderer == null || m_MeshFilter == null)
            {
                Debug.LogError("A component reference on the AdTracker is not assigned! Please assign it in the Inspector", this);
                this.enabled = false;
                return;
            }
            pointsToCheck = new Vector3[3];
            worldCorners = new Vector3[4];
            Bounds localBounds = GetComponent<MeshFilter>().mesh.bounds;
            Vector3 min = localBounds.min;
            Vector3 max = localBounds.max;
            localCorners = new Vector3[4];
            localCorners[0] = new Vector3(min.x, max.y, 0); // top left
            localCorners[1] = new Vector3(max.x, max.y, 0); // top right
            localCorners[2] = new Vector3(min.x, min.y, 0); // bottom left
            localCorners[3] = new Vector3(max.x, min.y, 0); // bottom right
        }


        public void ProcessVisibilityCheck(Camera mainCam, bool debugMode)
        {
            if (impressionLogged) return;

            bool isCurrentlyVisible = IsVisible(mainCam, debugMode);

            if (isCurrentlyVisible)
            {
                if (!isContinuouslyVisible)
                {
                    isContinuouslyVisible = true;
                    visibilityStartTime = Time.time;
                }
                if (Time.time - visibilityStartTime >= timeForValidImpression)
                {
                    m_AdPlacement.LogImpression();
                    impressionLogged = true;
                }
            }
            else
            {
                isContinuouslyVisible = false;
            }
        }
        public bool IsVisible(Camera mainCam, bool debugMode)
        {
            if (!m_AdRenderer.isVisible) return false;
            // check to ensure the camera is looking at the banner
            if (Vector3.Dot(mainCam.transform.forward, (m_AdRenderer.bounds.center - mainCam.transform.position).normalized) < requiredViewAngle) return false;

            //make sure the ad is facing camera and not viewed from extreme angle
            if (Vector3.Dot(transform.forward, mainCam.transform.forward) > requiredSurfaceAlignment) return false;

            if (!HasSufficientScreenSize(mainCam)) return false;

            int visiblePoints = 0;
            pointsToCheck[0] = m_AdRenderer.bounds.center;
            pointsToCheck[1] = transform.TransformPoint(localCorners[2]); // bottom left
            pointsToCheck[2] = transform.TransformPoint(localCorners[1]); // top right
            // casting 3 raycasts in total to ensure that banner isnt blocked
            foreach (Vector3 point in pointsToCheck)
            {
                Vector3 direction = point - mainCam.transform.position;
                bool isOccluded = Physics.Raycast(mainCam.transform.position, direction, direction.magnitude, occlusionLayers);

                if (!isOccluded)
                {
                    visiblePoints++;
                }

                if (debugMode)
                {
                    Debug.DrawRay(mainCam.transform.position, direction, isOccluded ? Color.red : Color.green);
                }
            }
            return visiblePoints == pointsToCheck.Length;
        }

        private bool HasSufficientScreenSize(Camera cam)
        {
            for (int i = 0; i < 4; i++)
            {
                worldCorners[i] = transform.TransformPoint(localCorners[i]);
            }
            Vector2 minScreenPos = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxScreenPos = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < 4; i++)
            {
                Vector3 screenPos = cam.WorldToScreenPoint(worldCorners[i]);
                if (screenPos.z < 0) return false;

                minScreenPos.x = Mathf.Min(minScreenPos.x, screenPos.x);
                minScreenPos.y = Mathf.Min(minScreenPos.y, screenPos.y);
                maxScreenPos.x = Mathf.Max(maxScreenPos.x, screenPos.x);
                maxScreenPos.y = Mathf.Max(maxScreenPos.y, screenPos.y);
            }
            float screenArea = (maxScreenPos.x - minScreenPos.x) * (maxScreenPos.y - minScreenPos.y);
            float totalScreenArea = Screen.width * Screen.height;
            float coverage = (screenArea / totalScreenArea) * 100f;
            return coverage >= minScreenCoveragePercent;
        }

        private void OnEnable() => AdTrackingManager.Instance?.RegisterTracker(this);

        private void OnDisable() => AdTrackingManager.Instance?.UnRegisterTracker(this);
    }
}