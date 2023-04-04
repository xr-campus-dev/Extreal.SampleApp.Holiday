using Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Extreal.SampleApp.Holiday.Controls.CameraControl
{
    public class CameraControlScope : LifetimeScope
    {
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private CameraControlView cameraControlView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(virtualCamera);
            builder.RegisterComponent(cameraControlView);

            builder.Register<Camera>(Lifetime.Singleton);

            builder.RegisterEntryPoint<CameraControlPresenter>();
        }
    }
}
