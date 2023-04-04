using Extreal.Core.StageNavigation;
using Extreal.SampleApp.Holiday.App;
using Extreal.SampleApp.Holiday.App.Common;
using Extreal.SampleApp.Holiday.App.Config;
using UniRx;

namespace Extreal.SampleApp.Holiday.Controls.CameraControl
{
    public class CameraControlPresenter : StagePresenterBase
    {
        private readonly CameraControlView cameraControlView;
        private readonly Camera camera;
        private readonly AppState appState;

        public CameraControlPresenter
        (
            CameraControlView cameraControlView,
            Camera camera,
            AppState appState,
            StageNavigator<StageName, SceneName> stageNavigator
        ) : base(stageNavigator)
        {
            this.cameraControlView = cameraControlView;
            this.camera = camera;
            this.appState = appState;
        }

        protected override void Initialize
        (
            StageNavigator<StageName, SceneName> stageNavigator,
            CompositeDisposable sceneDisposables
        )
        {
            cameraControlView.OnPerspectiveButtonClicked
                .Subscribe(_ => camera.TogglePerspective())
                .AddTo(sceneDisposables);

            camera.IsFpv
                .Subscribe(cameraControlView.SetPerspectiveLabel)
                .AddTo(sceneDisposables);

            appState.MyAvatarPrefab
                .Subscribe(camera.SetAvatarPrefab)
                .AddTo(sceneDisposables);
        }

        protected override void OnStageEntered(StageName stageName, CompositeDisposable stageDisposables)
        {
            if (AppUtils.IsSpace(stageName))
            {
                cameraControlView.Show();
            }
        }

        protected override void OnStageExiting(StageName stageName)
            => cameraControlView.Hide();
    }
}
