#if !UNITY_IOS && !UNITY_ANDROID
using System.Diagnostics.CodeAnalysis;
#endif
using UnityEngine;

namespace Extreal.SampleApp.Holiday.Controls.MultiplayControl
{
    public class MobileView : MonoBehaviour
    {
        [SerializeField] private GameObject joysticksCanvas;

#if !UNITY_IOS && !UNITY_ANDROID
        [SuppressMessage("Style", "IDE0051")]
        private void Awake()
            => joysticksCanvas.SetActive(false);
#endif
    }
}
