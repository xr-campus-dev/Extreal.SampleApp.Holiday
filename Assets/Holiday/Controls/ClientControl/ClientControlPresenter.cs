﻿using Cysharp.Threading.Tasks;
using Extreal.Core.StageNavigation;
using Extreal.Integration.Chat.Vivox;
using Extreal.Integration.Multiplay.NGO;
using Extreal.SampleApp.Holiday.App;
using Extreal.SampleApp.Holiday.App.AssetWorkflow;
using Extreal.SampleApp.Holiday.App.Common;
using Extreal.SampleApp.Holiday.App.Config;
using UniRx;
using VivoxUnity;

namespace Extreal.SampleApp.Holiday.Controls.ClientControl
{
    public class ClientControlPresenter : StagePresenterBase
    {
        private readonly AppState appState;
        private readonly AssetHelper assetHelper;
        // private readonly VivoxClient vivoxClient;
        private readonly NgoClient ngoClient;

        public ClientControlPresenter(
            StageNavigator<StageName, SceneName> stageNavigator,
            AppState appState,
            AssetHelper assetHelper,
            // VivoxClient vivoxClient,
            NgoClient ngoClient) : base(stageNavigator)
        {
            this.appState = appState;
            this.assetHelper = assetHelper;
            // this.vivoxClient = vivoxClient;
            this.ngoClient = ngoClient;
        }

        protected override void Initialize(
            StageNavigator<StageName, SceneName> stageNavigator, CompositeDisposable sceneDisposables)
        {
            InitializeNgoClient(stageNavigator, sceneDisposables);
            // InitializeVivoxClient(sceneDisposables);
        }

        private void InitializeNgoClient(
            StageNavigator<StageName, SceneName> stageNavigator, CompositeDisposable sceneDisposables)
        {
            ngoClient.OnConnectionApprovalRejected
                .Subscribe(_ =>
                {
                    appState.Notify(assetHelper.MessageConfig.MultiplayConnectionApprovalRejectedMessage);
                    stageNavigator.ReplaceAsync(StageName.AvatarSelectionStage);
                })
                .AddTo(sceneDisposables);

            ngoClient.OnConnectRetrying
                .Subscribe(retryCount => AppUtils.NotifyRetrying(
                    appState,
                    assetHelper.MessageConfig.MultiplayConnectRetryMessage,
                    retryCount))
                .AddTo(sceneDisposables);

            ngoClient.OnConnectRetried
                .Subscribe(result => AppUtils.NotifyRetried(
                    appState,
                    result,
                    assetHelper.MessageConfig.MultiplayConnectRetrySuccessMessage,
                    assetHelper.MessageConfig.MultiplayConnectRetryFailureMessage))
                .AddTo(sceneDisposables);

            ngoClient.OnUnexpectedDisconnected
                .Subscribe(_ =>
                    appState.Notify(assetHelper.MessageConfig.MultiplayUnexpectedDisconnectedMessage))
                .AddTo(sceneDisposables);
        }

        // private void InitializeVivoxClient(CompositeDisposable sceneDisposables)
        // {
        //     vivoxClient.OnConnectRetrying
        //         .Subscribe(retryCount => AppUtils.NotifyRetrying(
        //             appState,
        //             assetHelper.MessageConfig.ChatConnectRetryMessage,
        //             retryCount))
        //         .AddTo(sceneDisposables);

        //     vivoxClient.OnConnectRetried
        //         .Subscribe(result => AppUtils.NotifyRetried(
        //             appState,
        //             result,
        //             assetHelper.MessageConfig.ChatConnectRetrySuccessMessage,
        //             assetHelper.MessageConfig.ChatConnectRetryFailureMessage))
        //         .AddTo(sceneDisposables);

        //     vivoxClient.OnRecoveryStateChanged
        //         .Where(recoveryState => recoveryState == ConnectionRecoveryState.FailedToRecover)
        //         .Subscribe(_ => appState.Notify(assetHelper.MessageConfig.ChatUnexpectedDisconnectedMessage))
        //         .AddTo(sceneDisposables);

        //     vivoxClient.LoginAsync(new VivoxAuthConfig(nameof(Holiday))).Forget();
        // }

        protected override void OnStageEntered(StageName stageName, CompositeDisposable stageDisposables)
        {
        }

        protected override void OnStageExiting(StageName stageName)
        {
        }
    }
}
