using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class IndicatorPanelSerie2 : MonoBehaviour
{
    [Serializable]
    public class panelSection
    {
        private int speedSubSegCount, rpmSubSegCount, speedSubTextCount, rpmSubTextCount, maxSpeed, maxRpm, radialDistance;
        public GameObject[] speedSectionsImg;
        public GameObject[] rpmSectionsImg;
        public Color[] speedSegmentColors;
        public Color[] rpmSegmentColors;
        public GameObject[] speedSubs;
        public GameObject[] rpmSubs;

        public void CreateFields(Image speedBaseSegmentImage, Image rpmBaseSegmentImage, Color textColor, TMP_FontAsset fontAsset, int fontSize,
                                int speedSubTextCount, int rpmSubTextCount, int speedSubSegCount, int rpmSubSegCount,
                                int distanceFromCenter, int rpmRate, Color[] speedColors, Color[] rpmColors,
                                int maxSpeed, int maxRpm, int radialDistance)
        {
            this.speedSubSegCount = speedSubSegCount;
            this.speedSubTextCount = speedSubTextCount;
            this.rpmSubSegCount = rpmSubSegCount;
            this.rpmSubTextCount = rpmSubTextCount;
            this.maxSpeed = maxSpeed;
            this.maxRpm = maxRpm;
            this.radialDistance = radialDistance;

            rpmSegmentColors = rpmColors;
            speedSegmentColors = speedColors;
            speedSectionsImg = new GameObject[speedSubSegCount];
            rpmSectionsImg = new GameObject[rpmSubSegCount];
            speedSubs = new GameObject[speedSubTextCount + 1];
            rpmSubs = new GameObject[rpmSubTextCount + 1];

            for (int i = 0; i <= speedSubTextCount; i++)
            {
                speedSubs[i] = new GameObject();
                TextMeshProUGUI speedSub = speedSubs[i].AddComponent<TextMeshProUGUI>();
                speedSub.transform.SetParent(speedBaseSegmentImage.transform, false);
                speedSub.autoSizeTextContainer = true;
                speedSub.color = textColor;
                speedSub.font = fontAsset;
                speedSub.fontSize = fontSize;
                speedSub.UpdateFontAsset();

                int speedUnit = i * maxSpeed / speedSubTextCount;
                float radialSpeedUnit = i * radialDistance / speedSubTextCount;
                float speedAngle = (-90 - radialSpeedUnit) * Mathf.PI / 180f;
                speedSub.transform.localPosition = new Vector2(distanceFromCenter * Mathf.Cos(speedAngle), distanceFromCenter * Mathf.Sin(speedAngle));
                speedSub.text = speedUnit.ToString();
            }

            for (int i = 0; i <= rpmSubTextCount; i++)
            {
                rpmSubs[i] = new GameObject();
                TextMeshProUGUI rpmSub = rpmSubs[i].AddComponent<TextMeshProUGUI>();
                rpmSub.transform.SetParent(rpmBaseSegmentImage.transform, false);
                rpmSub.autoSizeTextContainer = true;
                rpmSub.color = textColor;
                rpmSub.font = fontAsset;
                rpmSub.fontSize = fontSize;
                rpmSub.UpdateFontAsset();

                int rpmUnit = (i * maxRpm / rpmSubTextCount) / rpmRate;
                float radialRpmUnit = i * radialDistance / rpmSubTextCount;
                float rpmAngle = (-90 + radialRpmUnit) * Mathf.PI / 180f;
                rpmSub.transform.localPosition = new Vector2(distanceFromCenter * Mathf.Cos(rpmAngle), distanceFromCenter * Mathf.Sin(rpmAngle));
                rpmSub.text = rpmUnit.ToString();
            }

            for (int i = speedSubSegCount - 1; i >= 0; i--)
            {
                speedSectionsImg[i] = new GameObject();
                speedSectionsImg[i].transform.SetParent(speedBaseSegmentImage.transform.parent, false);
                Image img = speedSectionsImg[i].AddComponent<Image>();
                img.transform.SetParent(speedBaseSegmentImage.transform, false);
                img.transform.localPosition = speedBaseSegmentImage.transform.localPosition;
                img.rectTransform.localScale = new Vector3(1, 1, 0);
                img.sprite = Resources.Load<Sprite>("Images/InnerUnColored");
                img.type = Image.Type.Filled;
                img.fillAmount = 0f;
                img.color = speedSegmentColors[i];
                img.GraphicUpdateComplete();
            }

            for (int i = rpmSubSegCount - 1; i >= 0; i--)
            {
                rpmSectionsImg[i] = new GameObject();
                rpmSectionsImg[i].transform.SetParent(rpmBaseSegmentImage.transform.parent, false);
                Image img = rpmSectionsImg[i].AddComponent<Image>();
                img.transform.SetParent(rpmBaseSegmentImage.transform, false);
                img.transform.localPosition = rpmBaseSegmentImage.transform.localPosition;
                img.rectTransform.localScale = new Vector3(1, 1, 0);
                img.sprite = Resources.Load<Sprite>("Images/InnerUnColored");
                img.type = Image.Type.Filled;
                img.fillAmount = 0f;
                img.fillClockwise = false;
                img.color = rpmSegmentColors[i];
                img.GraphicUpdateComplete();
            }
        }

        public void update(int speed, int rpm, int radialDistance)
        {
            float radialSpeedUnit = radialDistance / speedSubSegCount;
            float scaledSpeed = speed * radialDistance / maxSpeed;

            for (int i = 1; i <= speedSubSegCount; i++)
            {
                Image img = speedSectionsImg[i - 1].GetComponent<Image>();

                if (scaledSpeed < i * radialSpeedUnit)
                    img.fillAmount = scaledSpeed / 360;
                else
                    img.fillAmount = i * radialSpeedUnit / 360;
            }

            float radialRpmUnit = radialDistance / rpmSubSegCount;
            float scaledRpm = rpm * radialDistance / maxRpm;

            for (int i = 1; i <= rpmSubSegCount; i++)
            {
                Image img = rpmSectionsImg[i - 1].GetComponent<Image>();

                if (scaledRpm < i * radialRpmUnit)
                    img.fillAmount = scaledRpm / 360;
                else
                    img.fillAmount = i * radialRpmUnit / 360;
            }
        }
    }

    [Range(0, 1000)]
    public float dataUpdateSpeed = 100; //Miliseconds
    private System.DateTime lastUpdateTime;

    public int rpmMultiplier = 100;
    public Color deactiveColor, activeColor, inUseColor, longLightColor, fogLightColor, warningColor, normalColor, carColor;

    [HideInInspector]
    public int maxTorque, maxHp;
    public int maxRpm, maxSpeed;
    [HideInInspector]
    public int speed, rpm, torque, hp, oilLevel, fuelLevel;
    [HideInInspector]
    public string gear;
    [HideInInspector]
    public bool mainLong, mainShort, mainFog, engineError, electiricalError, absEnable, tcsEnable, absInUse, tcsInUse;

    public Image absIndImg, tcsIndImg, longIndImg, shortIndImg, fogIndImg, motorIndImg, electricIndImg, hpImg, torqueImg;
    public Image oilImg, fuelImg, lightsIcon, carImg;

    public TextMeshProUGUI OilTxt, FuelTxt, gearTxt;
    public TextMeshProUGUI speedMainTxt, rpmMainTxt, speedUnit, rpmUnit;

    public TMP_FontAsset fontAsset;
    public Color textColor;
    public PanelEnums.SpeedUnits SpeedUnits;
    public Image positionReferanceForSpeedTexts, positionReferanceForRpmTexts;
    public int speedSubTextCount, speedSubSegCount, rpmSubTextCount, rpmSubSegCount, distanceFromCenter, rpmRate, radialDistance, fontSize;
    public Color[] speedColors;
    public Color[] rpmColors;
    private panelSection panel = new panelSection();

    void Start()
    {
        initialize();
    }

    float torqueLerp = 0;
    float hpLerp = 0;
    void Update()
    {
        panel.update(speed, rpm, radialDistance);

        torqueLerp = Mathf.Lerp(torque, torqueLerp, 0.95f);
        hpLerp = Mathf.Lerp(hp, hpLerp, 0.95f);

        hpImg.fillAmount = (float)hpLerp / maxHp;
        torqueImg.fillAmount =(float)torqueLerp / maxTorque;
        oilImg.fillAmount = (float)oilLevel / 100;
        fuelImg.fillAmount = (float)fuelLevel / 100;

        System.TimeSpan ts = DateTime.Now - lastUpdateTime;
        if (ts.Milliseconds > dataUpdateSpeed)
        {
            lastUpdateTime = DateTime.Now;

            speedMainTxt.text = speed.ToString("0");
            rpmMainTxt.text = rpm.ToString("0");
            gearTxt.text = gear;

            if (oilLevel < 10)
                OilTxt.color = warningColor;
            else
                OilTxt.color = normalColor;
            if (fuelLevel < 10)
                FuelTxt.color = warningColor;
            else
                FuelTxt.color = normalColor;
        }

        checkIndicators();
    }

    void checkIndicators()
    {
        if (!absEnable)
            absIndImg.color = deactiveColor;
        if (absEnable && !absInUse)
            absIndImg.color = activeColor;
        if (absEnable && absInUse)
            absIndImg.color = inUseColor;

        if (!tcsEnable)
            tcsIndImg.color = deactiveColor;
        if (tcsEnable && !tcsInUse)
            tcsIndImg.color = activeColor;
        if (tcsEnable && tcsInUse)
            tcsIndImg.color = inUseColor;

        if (mainLong)
            longIndImg.color = longLightColor;
        else
            longIndImg.color = deactiveColor;

        if (mainShort)
            shortIndImg.color = activeColor;
        else
            shortIndImg.color = deactiveColor;

        if (mainFog)
            fogIndImg.color = fogLightColor;
        else
            fogIndImg.color = deactiveColor;

        if (mainLong || mainShort || mainFog)
            lightsIcon.transform.gameObject.SetActive(true);
        else
            lightsIcon.transform.gameObject.SetActive(false);

        if (engineError)
            motorIndImg.color = warningColor;
        else
            motorIndImg.color = deactiveColor;

        if (electiricalError)
            electricIndImg.color = warningColor;
        else
            electricIndImg.color = deactiveColor;

    }

    private void initialize()
    {
        lastUpdateTime = DateTime.Now;
        panel.CreateFields(positionReferanceForSpeedTexts, positionReferanceForRpmTexts, textColor, fontAsset, fontSize, speedSubTextCount, rpmSubTextCount,
            speedSubSegCount, rpmSubSegCount, distanceFromCenter, rpmRate, speedColors, rpmColors, maxSpeed, maxRpm, radialDistance);

        switch (SpeedUnits)
        {
            case PanelEnums.SpeedUnits.Km:
                speedUnit.text = "Km/H";
                break;
            case PanelEnums.SpeedUnits.Mile:
                speedUnit.text = "MPH";
                break;
            default:
                break;
        }

        rpmUnit.text = "x" + rpmRate.ToString();
        torque = 0;
        hp = 0;
        gear = "N";
        speed = 0;
        maxTorque = 1;
        maxHp = 1;
        rpm = 0;

        speedMainTxt.text = "0";
        rpmMainTxt.text = "0";
        gearTxt.text = "N";

        absInUse = false;
        tcsInUse = false;
        mainLong = false;
        mainShort = false;
        engineError = false;
        fuelImg.fillAmount = 0;
        oilImg.fillAmount = 0;
        torqueImg.fillAmount = 0;
        hpImg.fillAmount = 0;

        carImg.color = carColor;
    }
}
