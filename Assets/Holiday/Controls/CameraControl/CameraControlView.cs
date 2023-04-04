using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Extreal.SampleApp.Holiday.Controls.CameraControl
{
    public class CameraControlView : MonoBehaviour
    {
        [SerializeField] private GameObject canvas;
        [SerializeField] private Button perspectiveButton;
        [SerializeField] private TMP_Text perspectiveLabel;

        public IObservable<Unit> OnPerspectiveButtonClicked
            => perspectiveButton.OnClickAsObservable().TakeUntilDestroy(this);

        public void Show()
            => canvas.SetActive(true);

        public void Hide()
            => canvas.SetActive(false);

        public void SetPerspectiveLabel(bool isFpv)
            => perspectiveLabel.text = isFpv ? "1" : "3";
    }
}
