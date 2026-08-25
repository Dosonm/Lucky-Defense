using UnityEngine;
using UnityEngine.UI;

namespace VScrollBGTest
{
    [System.Serializable]
    /// </summary>
    //This script is used for background vertical scrolling demo play.
    //Used in the editor only.
    //This is a sample script and stability and optimization are not guaranteed.
    //The standard resolution of the demo scene is 1920x1280 pixels
    /// </summary>
    public class VScrollBGDemoPlayCtrl : MonoBehaviour
    {
        private bool IsAutoPlay;
        private float CurTime = 0;

        //Set how many seconds each background images should be displayed
        public float AutoPlayShowTime = 1.2f;
        public Toggle ToggleAutoPlay;
        private int CurThemeCount = 0;
        public VScrollBGCtrl _VScrollBGCtrl;
        private int BGViewCount = 0;//background image count
        private int BGImageCount = 0;//Number of images per theme
        public int BGImageStep = 1;//Number of images skipped
        public int BGImageMax = 4;//Maximum number of background images per theme
        public bool _IsShowFullImage;//Show all background images

        // Start is called before the first frame update
        void Start()
        {
            //Auto (Scrolling) play
            ToggleAutoPlay.onValueChanged.AddListener(delegate
            {
                ToggleAutoPlaySet(ToggleAutoPlay);
            });
        }

        // Update is called once per frame
        void Update()
        {
            if (IsAutoPlay)
            {
                CurTime += Time.unscaledDeltaTime;
                if (CurTime > AutoPlayShowTime)
                {
                    if (BGImageStep > 0)
                        BGViewCount += BGImageStep;
                    else
                        BGViewCount += 1;

                    _VScrollBGCtrl.BGNumSet(BGImageStep);
                    CurTime = 0.0f;

                    if ((BGViewCount / BGImageStep) > BGImageCount)
                    {
                        CurThemeCount += 1;

                        if (CurThemeCount <= _VScrollBGCtrl.ThemeCount)
                        {
                            _VScrollBGCtrl.ThemeNumSet(CurThemeCount);
                        }
                        if (CurThemeCount > _VScrollBGCtrl.ThemeCount)
                        {
                            IsAutoPlay = false;
                            ToggleAutoPlay.isOn = false;
                        }
                        BGViewCount = 0;
                        BGImageCountSetting();
                    }
                }
            }
        }

        //Maximum number of background images per theme
        public void BGImageCountSetting()
        {
            if (_IsShowFullImage)
                BGImageCount = _VScrollBGCtrl.BGImageCount;
            else
                BGImageCount = BGImageMax;

        }

        //Automatically shows background image during play
        public void ToggleAutoPlaySet(Toggle _IsAutoPlay)
        {
            IsAutoPlay = ToggleAutoPlay.isOn;
            _VScrollBGCtrl._IsAutoPlayModeSetting(ToggleAutoPlay.isOn);
            BGImageCountSetting();

            if (ToggleAutoPlay.isOn)
            {
                BGViewCount = 0;
                _VScrollBGCtrl.ThemeNumSet(0);
                _VScrollBGCtrl._IsAutoPlayModeSetting(true);
            }

            if (!ToggleAutoPlay.isOn)
            {
                CurThemeCount = 0;
                CurTime = 0;
                _VScrollBGCtrl._IsAutoPlayModeSetting(false);
            }
        }
    }
}
