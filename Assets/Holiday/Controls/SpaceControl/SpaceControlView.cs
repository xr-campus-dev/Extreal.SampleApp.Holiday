﻿using System;
using System.Diagnostics.CodeAnalysis;
using Extreal.SampleApp.Holiday.App.AssetWorkflow;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Extreal.SampleApp.Holiday.Controls.SpaceControl
{
    public class SpaceControlView : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text backButtonLabel;

        [Inject] private AssetHelper assetHelper;

        public IObservable<Unit> OnBackButtonClicked
            => backButton.OnClickAsObservable().TakeUntilDestroy(this);

        [SuppressMessage("Usage", "IDE0051")]
        private void Awake() => backButtonLabel.text = assetHelper.MessageConfig.VirtualSpaceBackButtonLabel;
    }
}
