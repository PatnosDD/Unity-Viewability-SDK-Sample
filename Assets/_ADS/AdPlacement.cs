using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SDKTEST
{
    [RequireComponent(typeof(Collider))]
    internal class AdPlacement : MonoBehaviour
    {
        public string adUnitId = "rameSadme123";

        public UnityEvent OnImpression;
        public UnityEvent OnClick;

        private void OnMouseDown()
        {
            Debug.Log($"Ad '{adUnitId}' was clicked!");
            OnClick?.Invoke();
        }

        internal void LogImpression()
        {
            Debug.Log($"Valid impression logged for ad unit : {adUnitId}");
            OnImpression?.Invoke();
        }
    }

}
