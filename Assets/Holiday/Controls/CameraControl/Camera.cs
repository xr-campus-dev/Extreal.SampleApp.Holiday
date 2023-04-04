using System;
using Cinemachine;
using UniRx;
using Extreal.Core.Common.System;
using UnityEngine;
using Extreal.Core.Logging;

namespace Extreal.SampleApp.Holiday.Controls.CameraControl
{
    public class Camera : DisposableBase
    {
        private readonly Cinemachine3rdPersonFollow thirdPersonFollow;
        private GameObject avatarPrefab;

        public IObservable<bool> IsFpv => isFpv;
        private readonly BoolReactiveProperty isFpv = new BoolReactiveProperty();

        private readonly Vector3 initDamping;
        private readonly Vector3 initShoulderOffset;
        private readonly float initCameraDistance;

        private static readonly ELogger Logger = LoggingManager.GetLogger(nameof(Camera));

        public Camera(CinemachineVirtualCamera virtualCamera)
        {
            thirdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

            initDamping = thirdPersonFollow.Damping;
            initShoulderOffset = thirdPersonFollow.ShoulderOffset;
            initCameraDistance = thirdPersonFollow.CameraDistance;
        }

        protected override void ReleaseManagedResources()
            => isFpv.Dispose();

        public void SetAvatarPrefab(GameObject avatarPrefab)
        {
            this.avatarPrefab = avatarPrefab;
            if (avatarPrefab != null)
            {
                SetPerspective(isFpv.Value);
            }
        }

        public void TogglePerspective()
        {
            isFpv.Value = !isFpv.Value;
            SetPerspective(isFpv.Value);
        }

        private void SetPerspective(bool value)
        {
            if (value)
            {
                if (Logger.IsDebug())
                {
                    Logger.LogDebug("1st-person perspective");
                }
                thirdPersonFollow.Damping = Vector3.zero;
                thirdPersonFollow.ShoulderOffset = Vector3.zero;
                thirdPersonFollow.CameraDistance = 0f;
                avatarPrefab.SetActive(false);
            }
            else
            {
                if (Logger.IsDebug())
                {
                    Logger.LogDebug("3st-person perspective");
                }
                thirdPersonFollow.Damping = initDamping;
                thirdPersonFollow.ShoulderOffset = initShoulderOffset;
                thirdPersonFollow.CameraDistance = initCameraDistance;
                avatarPrefab.SetActive(true);
            }
        }
    }
}
