using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace VScrollBGTest
{
    [System.Serializable]
    /// </summary>
    //This script is used for background vertical scrolling demo play.
    //Used in the editor only.
    //This is a sample script and stability and optimization are not guaranteed.
    //The standard resolution of the demo scene is 1920x1280 pixels
    /// </summary>

    public class VScrollBGCtrl : MonoBehaviour
    {
        //Test UI view settings
        public GameObject TestUI;
        public GameObject TestUIOn;
        public GameObject TestUIOff;
        public Text TxtThemeCount;
        public Text TxtBGCount;

        //Background
        public Transform BG_Object;
        Texture2D BGtexture = null;

        //Dir(Path) & Name-List
        public Object BGImageDir;
        private string BGDirPath;
        private string ThemeDirName;
        public List<string> ThemeNameStr;
        public List<string> BGNameStr;

        //Theme & BGImage Count & Set-Value
        private int ThemeNum;
        private int BGImageNum;
        public int ThemeCount;
        public int BGImageCount;

        //BGGrid 
        public Transform BGGrid_Object;
        public Texture[] BGGridTexture;
        private int BGGridNum;
        public Text TxtBGGridName;

        //Camera
        public Transform CamPos;
        public GameObject CamPerspective;
        public GameObject CamOthographics;
        public Vector3 CamSetPos = new Vector3(0, -6.0f, -7.0f);//start camera position
        public float CamOffsetWidth = 0.7f;
        public float CamOffsetStrength = 0.4f;

        //Character
        public Transform CharPos;
        public float CharSpeed = 2.4f;
        private float CharOffsetX = 0.0f;
        private float CharOffsetY = -4.5f;
        public float WidthMax = 1.5f;
        public float WidthMin = -1.5f;
        public float HeightMax = -3.5f;
        public float HeightMin = -5.5f;

        //Character size values by camera projection
        public Vector3 CharPosScalePer = new Vector3(0.4f, 0.4f, 1.0f);
        public Vector3 CharPosScaleOtho = new Vector3(0.4f, 0.4f, 1.0f);

        //Scrolling Speeds
        public float ScrollSpeed = 0.24f;
        private float VOffset = 0.0f;

        //Renderer
        private MeshRenderer RenBG;
        private MeshRenderer RenGrid;

        //UI
        public Text ThemeNameTxt;
        public Text BGNameTxt;
        public Toggle ToggleCamProjection;
        public GameObject Btn_UPArrow, Btn_DownArrow, Btn_LArrow, Btn_RArrow;
        public GameObject Btn_A, Btn_S, Btn_D, Btn_W;

        //Set keyboard input time interval
        bool IsBGChange;
        public float BGChangeBtnSpeed = 2.0f;
        private float CurBGChangeBtnSpeed = 0.0f;

        private string ArrowKeyV;

        public bool _IsAutoPlay;


        //#if UNITY_EDITOR
        void Start()
        {
            TestUISetting(true);
            CharPos.position = new Vector3(CharOffsetX, CharOffsetY, 0.0f);
            ThemeNum = 0;
            BGImageNum = 0;
            RenBG = BG_Object.GetComponent<MeshRenderer>();
            RenGrid = BGGrid_Object.GetComponent<MeshRenderer>();
            IsBGChange = false;
            ThemeGetting();
            BGGridTextureSetting(0);

            ToggleCamProjection.onValueChanged.AddListener(delegate
            {
                ToggleCamProjectionSet(ToggleCamProjection);
            });
            ToggleCamProjection.isOn = true;
        }

        //TestUI On/off
        public void TestUISetting(bool _IsOn)
        {
            TestUI.SetActive(_IsOn);
            TestUIOn.SetActive(!_IsOn);
            TestUIOff.SetActive(_IsOn);
        }

        //Specify the background folder path and retrieve theme information
        public void ThemeGetting()
        {
            //Specify background folder path
            BGDirPath = UnityEditor.AssetDatabase.GetAssetPath(BGImageDir);

            //Get the names (=themes) of subfolders from the background folder
            if (System.IO.Directory.Exists(BGDirPath))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(BGDirPath);
                foreach (System.IO.DirectoryInfo Dir in di.GetDirectories())
                {
                    //Get name of subfolder
                    ThemeDirName = Dir.Name;

                    //Add theme names to the list
                    ThemeNameStr.Add(ThemeDirName);
                }

                //Number of themes 
                ThemeCount = ThemeNameStr.Count - 1;

                //theme settings
                ThemeNumSet(0);
            }
            else
            {
                Debug.Log("!!Need to check if 'Background' folder is link to 'BGDirPath'!!");
            }
        }

        //Get BGimages(=background image texture)
        public void BGImageGetting()
        {
            //Get the address of the subfolder (=theme folder) of the 'background' folder
            DirectoryInfo BGDir = new DirectoryInfo(BGDirPath + "/" + ThemeNameStr[ThemeNum]);

            //Add BGImage names to the list
            BGNameStr.Clear();
            foreach (FileInfo File in BGDir.GetFiles("*.png"))
            {
                BGNameStr.Add($"{Path.GetFileNameWithoutExtension(File.Name)}");
            }

            //Number of BGimages
            BGImageCount = BGNameStr.Count - 1;

            //BGimage setting
            BGNumSet(0);
        }

        public void ThemeNumSetAdd(int AddNum)
        {
            if (!_IsAutoPlay)
            {
                ThemeNum += AddNum;
                ThemeNumSet(ThemeNum);
            }
        }

        //Theme setting
        public void ThemeNumSet(int Num)
        {
            ThemeNum = Num;

            if (ThemeNum > ThemeCount)
                ThemeNum = 0;

            if (ThemeNum < 0)
                ThemeNum = ThemeCount;

            ThemeNameTxt.text = ThemeNameStr[ThemeNum];
            TxtThemeCount.text = (ThemeNum + 1) + " / " + (ThemeCount + 1);
            BGImageNum = 0;
            BGImageGetting();
        }

        public void BGNumSetAdd(int AddNum)
        {
            if (!_IsAutoPlay)
            {
                BGNumSet(AddNum);
            }
        }
        //BGImage Setting

        public void BGNumSet(int Num)
        {
            BGImageNum += Num;
            if (BGImageNum > BGImageCount)
                BGImageNum = 0;

            if (BGImageNum < 0)
                BGImageNum = BGImageCount;

            BGNameTxt.text = BGNameStr[BGImageNum];
            TxtBGCount.text = (BGImageNum + 1) + " / " + (BGImageCount + 1);
            string CurPath = BGDirPath + "/" + ThemeNameStr[ThemeNum] + "/" + BGNameStr[BGImageNum] + ".png";

            //Apply texture
            byte[] byteTexture = System.IO.File.ReadAllBytes(CurPath);
            if (byteTexture.Length > 0)
            {
                BGtexture = new Texture2D(0, 0);
                BGtexture.LoadImage(byteTexture);
                RenBG.material.SetTexture("_MainTex", BGtexture);
            }
            IsBGChange = true;
        }

        //GridImage Setting
        public void BGGridTextureSetting(int _BGGridNum)
        {
            BGGridNum += _BGGridNum;
            if (BGGridNum > BGGridTexture.Length - 1)
                BGGridNum = 0;

            RenGrid.material.SetTexture("_MainTex", BGGridTexture[BGGridNum]);
            TxtBGGridName.text = BGGridTexture[BGGridNum].name;
            IsBGChange = true;
        }

        void Update()
        {
            //move upward
            if (Input.GetKey(KeyCode.W) || ArrowKeyV == "W" || _IsAutoPlay)
            {
                CharOffsetY += CharSpeed * Time.deltaTime;
                if (CharOffsetY >= HeightMax)
                    CharOffsetY = HeightMax;

                CharPos.position = new Vector3(CharOffsetX, CharOffsetY, 0.0f);

                if (CharPos.position.y == HeightMax)
                {
                    VOffset += ScrollSpeed * Time.deltaTime;
                    RenBG.material.mainTextureOffset = new Vector2(0, VOffset);
                    RenGrid.material.mainTextureOffset = new Vector2(0, VOffset * 8.0f);
                }
            }

            //move down
            if (Input.GetKey(KeyCode.S) || ArrowKeyV == "S")
            {
                if (!_IsAutoPlay)
                {
                    CharOffsetY -= CharSpeed * Time.deltaTime;
                    if (CharOffsetY <= HeightMin)
                        CharOffsetY = HeightMin;

                    CharPos.position = new Vector3(CharOffsetX, CharOffsetY, 0.0f);

                    if (CharPos.position.y == HeightMin)
                    {
                        VOffset -= ScrollSpeed * Time.deltaTime;
                        RenBG.material.mainTextureOffset = new Vector2(0, VOffset);
                        RenGrid.material.mainTextureOffset = new Vector2(0, VOffset * 8.0f);
                    }
                }

            }

            //move right
            if (Input.GetKey(KeyCode.D) || ArrowKeyV == "D")
            {
                CharOffsetX += CharSpeed * Time.deltaTime;
                if (CharOffsetX >= WidthMax)
                    CharOffsetX = WidthMax;

                CharPos.position = new Vector3(CharOffsetX, CharOffsetY, 0.0f);
                CamPosSet();
            }

            //move left
            if (Input.GetKey(KeyCode.A) || ArrowKeyV == "A")
            {
                CharOffsetX -= CharSpeed * Time.deltaTime;
                if (CharOffsetX <= WidthMin)
                    CharOffsetX = WidthMin;

                CharPos.position = new Vector3(CharOffsetX, CharOffsetY, 0.0f);
                CamPosSet();
            }


            //change grid image
            if (Input.GetKey(KeyCode.Space) && !IsBGChange)
            {
                BGGridTextureSetting(1);
            }

            //change Theme & BGImage
            if (!IsBGChange && !_IsAutoPlay)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    ThemeNum += 1;
                    ThemeNumSet(ThemeNum);
                }

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    ThemeNum -= 1;
                    ThemeNumSet(ThemeNum);
                }

                if (Input.GetKey(KeyCode.RightArrow))
                    BGNumSet(1);

                if (Input.GetKey(KeyCode.LeftArrow))
                    BGNumSet(-1);

                CurBGChangeBtnSpeed = 0.0f;
            }

            //Background switching time interval
            if (IsBGChange)
            {
                CurBGChangeBtnSpeed += BGChangeBtnSpeed * Time.deltaTime;
                if (CurBGChangeBtnSpeed > 1.0f)
                {
                    CurBGChangeBtnSpeed = 0.0f;
                    IsBGChange = false;
                }
            }
        }

        //Camera Position Control
        public void CamPosSet()
        {
            if (CharOffsetX >= CamOffsetWidth)
            {
                CamPos.position = new Vector3(CamSetPos.x + ((CharOffsetX - CamOffsetWidth) * CamOffsetStrength), CamSetPos.y, CamSetPos.z);
            }
            if (CharOffsetX <= -CamOffsetWidth)
            {
                CamPos.position = new Vector3(CamSetPos.x + ((CharOffsetX + CamOffsetWidth) * CamOffsetStrength), CamSetPos.y, CamSetPos.z);
            }
        }


        //Camera projection Setting
        public void ToggleCamProjectionSet(Toggle _IsPerspective)
        {
            CamPerspective.SetActive(ToggleCamProjection.isOn);
            CamOthographics.SetActive(!ToggleCamProjection.isOn);

            if (ToggleCamProjection.isOn)
                CharPos.localScale = CharPosScalePer;

            if (!ToggleCamProjection.isOn)
                CharPos.localScale = CharPosScaleOtho;

        }

        //Arrow Key setting
        public void ArrowSetting(string _Key)
        {
            ArrowKeyV = _Key;
        }

        public void _IsAutoPlayModeSetting(bool _IsAutoPlayMode)
        {
            _IsAutoPlay = _IsAutoPlayMode;
            Btn_UPArrow.SetActive(!_IsAutoPlayMode);
            Btn_DownArrow.SetActive(!_IsAutoPlayMode);
            Btn_LArrow.SetActive(!_IsAutoPlayMode);
            Btn_RArrow.SetActive(!_IsAutoPlayMode);

            //Btn_A.SetActive(!_IsAutoPlayMode);
            Btn_S.SetActive(!_IsAutoPlayMode);
            //Btn_D.SetActive(!_IsAutoPlayMode);
            Btn_W.SetActive(!_IsAutoPlayMode);
        }
        //#endif
    }
}