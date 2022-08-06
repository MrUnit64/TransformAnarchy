namespace RotationAnarchy.Internal
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IUnityCallbacksReceiver
    {
        void UnityUpdate();
        void UnityLateUpdate();
        void UnityFixedUpdate();
    }

    public class UnityCallbacksHandler : MonoBehaviour
    {
        private IUnityCallbacksReceiver receiver;

        public void SetReceiver(IUnityCallbacksReceiver receiver)
        {
            this.receiver = receiver;
        }

        private void Update()
        {
            receiver?.UnityUpdate();
        }

        private void LateUpdate()
        {
            receiver?.UnityLateUpdate();
        }

        private void FixedUpdate()
        {
            receiver?.UnityFixedUpdate();
        }
    }
}