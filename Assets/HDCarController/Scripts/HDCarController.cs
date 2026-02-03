using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static Enums;
using Random = UnityEngine.Random;
using HDCarControl;
using System.Reflection.Emit;
using TMPro;
//using UnityEditor.UIElements;

public class HDCarController : MonoBehaviour
{
    public int exhaustCount = 2;
    public ParticleSystem[] exhaustPS;
    public Canvas IndicatorPanel;

    private HDCarControlFunctions HDCarControlFunctions = new HDCarControlFunctions();

    public class DynamicDataClass
    {
        public WheelProperties wheelProperties = new WheelProperties();
        private const int WheelCount = WheelProperties.AxleCount* 2;
        public int maxEngineTorque = 0;
        public float EngineTorque = 0;
        public int maxEngineRpm = 0;
        public float EngineRpm = 0;
        public float[] WheelsRpm = new float[WheelCount];
        public float PowerHP = 0;
        public float PowerKw = 0;
        public float[] WheelsMotorTorque = new float[WheelCount];
        public float[] WheelsBrakeTorque = new float[WheelCount];

        public Vector3 Speed = new Vector3();
        public Transform Position;
        public Quaternion Rotation;
        public int CurrenShift = 0;
        public float ClutchRate = 0;

        public float DownForce = 0;
        public float AirFriction = 0;
        public float RollingResistance = 0;
        public float InnerResistance = 0;

        public bool IsCarOnAir = false;
        public float ForwardSkid = 0;
        public float SideSkid = 0;
        public WheelHit[] WheelHits = new WheelHit[WheelCount];
        public Material SkidMaterial;

        public void ClearArrays()
        {
            for (int i = 0; i < WheelCount; i++)
            {
                WheelsMotorTorque[i] = 0;
                WheelsBrakeTorque[i] = 0;
            }
        }
    }
    
    [Serializable]
    public class GroundMatrial
    {
        public string Name;
        public PhysicsMaterial PhysicMaterial;
        public Material TireDustMaterial;
        public Sound SkidSound;
        [HideInInspector]
        public int TiresCountOnTheGround;
        public bool EnableTireBurnout = true;
    }

    [Serializable]
    public class TireParticleSytem
    {
        public ParticleSystem[] PSs = new ParticleSystem[3];
        [HideInInspector]
        public float[] TimeOfUpdate = new float[3];
    }

    [Serializable]
    public class WheelProperties
    {
        public const int AxleCount = 2;
        private const int wheelCount = AxleCount * 2;

        public GameObject[] Axles = new GameObject[AxleCount];
        public WheelCollider[] wheelColliders = new WheelCollider[wheelCount];
        [HideInInspector]
        public float[] ForwardStiffness = new float[wheelCount];
        [HideInInspector]
        public float[] SideStiffness = new float[wheelCount];
        public bool[] isPowered = new bool[wheelCount];
        public TireParticleSytem[] TireParticleSytems = new TireParticleSytem[wheelCount];
        public Transform[] TireMeshes = new Transform[4];
        [HideInInspector]
        public int[] LastIndex = new int[wheelCount];
        [HideInInspector]
        public float[] TireTemprature = new float[wheelCount];
        [HideInInspector]
        public float[] BrakeTemprature = new float[wheelCount];
        [HideInInspector]
        public float[] SideSpeedDiff = new float[wheelCount];
        [HideInInspector]
        public float[] ForwardSpeedDif = new float[wheelCount];
        [HideInInspector]
        public float[] SignedForwardSpeedDif = new float[wheelCount];
        [HideInInspector]
        public float[] SideSkidIntensitiy = new float[wheelCount];
        [HideInInspector]
        public float[] ForwardSkidIntensitiy = new float[wheelCount];
        [HideInInspector]
        public float[] SprungMassRate = new float[wheelCount];

        [HideInInspector]
        public float WheelDistance = 0;
        [HideInInspector]
        public float AxleDistance = 0;

        public void CalculateSpeedDiffsAndIntensities(Rigidbody rb)
        {
            short index = 0;
            Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.linearVelocity);
            foreach (WheelCollider wc in wheelColliders)
            {
                //Side Speed Diff : Meters in Second.
                SideSpeedDiff[index] = Mathf.Abs(localVelocity.x);

                //Forward Speed Diff : Meters in Second.
                float WRpmBasedRadialSpeed = 2 * Mathf.PI * wc.radius * Mathf.Abs(wc.rpm) / 60;
                ForwardSpeedDif[index] = Mathf.Abs(WRpmBasedRadialSpeed - Mathf.Abs(localVelocity.z));
                SignedForwardSpeedDif[index] = WRpmBasedRadialSpeed - Mathf.Abs(localVelocity.z);
                //Sprung Mass Rate For Each Wheel
                SprungMassRate[index] = wc.sprungMass / rb.mass;

                //Forward Intensity Multiplier
                float ForwardMultiplier = 0;
                float SideMultiplier = 1f;

                if (wc.motorTorque > 10 && wc.brakeTorque <= 0)
                    ForwardMultiplier = 1;
                else if (wc.motorTorque <= 0 && wc.brakeTorque > 10)
                    ForwardMultiplier = 0.2f;
                else
                    ForwardMultiplier = 0;

                //Skid Intensity
                ForwardSkidIntensitiy[index] = Mathf.Clamp(ForwardSpeedDif[index] * ForwardMultiplier * 10 * SprungMassRate[index], 0, 15) / 15;
                SideSkidIntensitiy[index] = Mathf.Clamp(SideSpeedDiff[index] * SideMultiplier * 10 * SprungMassRate[index], 0, 30) / 30;

                index++;
            }
        }

        public void initialize()
        {
            short index = 0;
            foreach (GameObject axle in Axles)
            {
                foreach (WheelCollider wc in axle.GetComponentsInChildren<WheelCollider>())
                {
                    wheelColliders[index] = wc;
                    wheelColliders[index].motorTorque = 0;
                    wheelColliders[index].brakeTorque = 0;
                    index++;
                }
            }

            for (int i = 0; i < wheelCount; i++)
            {
                WheelCollider item = wheelColliders[i];
                ForwardStiffness[i] = item.forwardFriction.stiffness;
                SideStiffness[i] = item.sidewaysFriction.stiffness;
                LastIndex[i] = 0;
                TireTemprature[i] = 0;
                BrakeTemprature[i] = 0;
            }

            AxleDistance = Mathf.Abs(Axles[0].transform.localPosition.z - Axles[1].transform.localPosition.z);
            WheelCollider[] wcs = new WheelCollider[2];
            wcs = Axles[0].GetComponentsInChildren<WheelCollider>();
            WheelDistance = Mathf.Abs(wcs[0].transform.localPosition.x - wcs[1].transform.localPosition.x);
        }

        public void TireTempratureControl(int i, float HeatGained, float HeatLosed)
        {
            TireTemprature[i] += HeatGained - HeatLosed;
            TireTemprature[i] = Mathf.Clamp01(TireTemprature[i]);
        }

        public void BrakeTempratureControl(int i, float HeatGained, float HeatLosed)
        {
            BrakeTemprature[i] += HeatGained - HeatLosed;
            BrakeTemprature[i] = Mathf.Clamp01(BrakeTemprature[i]);
        }
    }

    [System.Serializable]
    public class WavPart
    {
        public int centerRPM;
        public AudioSource audioSource = new AudioSource();
        public void CalculateCenterRpm()
        {
            string name = audioSource.clip.name;
            centerRPM = int.Parse(name.Substring(name.LastIndexOf("_") + 1));
        }
    }

    [System.Serializable]
    public class SoundFilters
    {
        [Range(0, 1), Tooltip("Engine Master Volume.")]
        public float EngineMasterVolume = 1;
        [Range(0, 22000), Tooltip("Low Pass Filter Low Cutt Off Freq.")]
        public float EngineLowPassCutOffLow = 3800;
        [Range(0, 22000), Tooltip("Low Pass Filter High Cutt Off Freq.")]
        public float EngineLowPassCutOffHigh = 22000;
        [Tooltip("Distortion Level.")]
        public AnimationCurve EngineDistortionLevel;
        [Range(0, 1), Tooltip("Skid Sound Level.")]
        public float OtherSoundsVolume = 1;
        [Range(0, 22000), Tooltip("Low Pass Filter Cutt Off Freq.")]
        public float OtherLowPassCutOff = 3800;
        [Tooltip("Distortion Level.")]
        public AnimationCurve OtherDistortionLevel;
    }

    [System.Serializable]
    public class SoundFilterSets
    {
        public SoundFilters[] SoundsAtCamPositions;
    }

    public Skids SkidMarkController;
    public GameObject TireBurnOutParticleSystem;
    [Range(0, 10)]
    public float TireBurnOutParticleSystemIntensity;

    public GameObject SteeringWheel;
    public Axis SteeringWheelAxis;
    [Range(0,10)]
    public float SteeringWheelMultiplier = 1.5f;
    private Vector3 SOP; //Steering Wheel Original Position

    public GameObject LeftBrakeSystem, RightBrakeSystem;
    public Axis BrakeAxis;

    public GameObject RpmNeedle, SpeedNeedle;
    public Axis RpmNeedleAxis, SpeedNeedleAxis;
    public float SpeedNeedleRange, RpmNeedleRange, MaxSpeedForSpeedNeedle, MaxRpmForSpeedNeedle;
    public TurnDirection SpeedNeedleDirection, RpmNeedleDirection;
    public SpeedType SpeedType;

    private Vector3 LFBOP; //Left Front Brake Original Position
    private Vector3 RFBOP; //Right Front Brake Original Position
    private Vector3 RPMNEEDLEOP; //RPM Needle Original Position
    private Vector3 SPEEDNEEDLEOP; //Speed Needle Original Position


    public GroundMatrial[] GroundMatrials = new GroundMatrial[3];

    public WheelProperties wheelProperties = new WheelProperties();
    public DynamicDataClass DynamicData = new DynamicDataClass();
    public Transform centerOfMass;
    public AnimationCurve HpCurve;
    public AnimationCurve GasPeddalEfficiency;
    public AnimationCurve CRCurve;

    private AudioManager audioManager;

    private Rigidbody rb;
    private Vector3 localVelocity;

    #region Lights
    public Light BrakeLightLeft, BrakeLightRight, 
                 MainLightLeft, MainLightRight, RearSideLightLeft, RearSideLightRight,
                 ReverseLightLeft, ReverseLightRight, SignalLightFrontLeft, SignalLightFrontRight, SignalLightRearLeft, SignalLightRearRight;

    public GameObject BrakeLightObject, MainLightObject, RearSideLightObject, ReverseLightObject, LeftSignalLightObject, RightSignalLightObject;
    public Material LightsOffMaterial, LightsOnMaterial;

    //Light Key Codes
    public KeyCode frontLightKey, turnSignalLeftKey, turnSignalRightKey, turnSignalAllFour, turnSignalOff;
    //Car Control Key Codes
    //public KeyCode Left, Right, Gas, Brake, LaunchControl, ShiftDown, ShiftUp, HandBrake;
    public KeyCode LaunchControl, ShiftDown, ShiftUp, HandBrake;
    private TurnSignal TurnSignalStatus;
    public LaunchControlMode LaunchControlMode;
    public GearBoxControlMode GearBoxControlMode;
    public ClutchControlMode ClutchControlMode;
    #endregion

    #region Screen and Particle Properties
    public float MinSideSpeedForSkidVolume, MinForwardSpeedForSkidVolume, MinForwardSpeedForSkids, MinSideSpeedForSkids, tireDustParticleCount;
    private float TireTemprature;
    [Range(0f, 0.01f)]
    public float TireHeatingCoEff = 0.01f, BrakeHeatingCoEff = 0.01f;
    [Range(0f, 0.01f)]
    public float TireCoolingCoEff = 0.005f, BrakeCoolingCoEff = 0.005f;
    #endregion

    #region Engine Properties
    [Range(0, 10)]
    public float[] GearRatios = new float[7];
    [Range(0, 20)]
    public float rearRatio, finalRatio;

    [Range(0, 2)]
    public float TorkMultiplier = 1;
    [Range(0, 5000)]
    public float MaxEnginePower = 1000;
    [Range(0, 2)]
    public float HPMultiplier = 1;
    [Range(0, 0.5f)]
    public float CRSensitivity = 0.05f;

    [Range(0, 12000)]
    public float MaxEngineRpm = 0f;
    [Range(0, 1000)]
    public float MinEngineRpm = 0f;
    [Range(0, 1)]
    public float AccCoEff = 0.2f;
    [Range(0, 0.1f)]
    public float DecCoEff = 0.002f;
    [Range(0.00001f, 0.5f)]
    public float ReverseGearCoEff = 0.01f;
    [Range(0.1f, 10f)]
    public float InitialDampingRate = 1f;
    [Range(0f, 2f)]
    public float ForwardSkidEffectToSideFriction = 0.5f;
    [Range(0f, 2f)]
    public float FrictionMultiplier = 0.5f;
    [Range(0f, 1f)]
    public float SideForceMultiplier = 0.5f;
    [Range(0f, 1000f)]
    public float MinEngineRpmOffsett = 200f;
    [Range(0f, 1000)]
    public float MaxEngineRpmOffsett = 500f;
    [Range(0f, 60)]
    public float MaxSteerAngle = 40;
    [Range(0, 100), Tooltip("0:Close, 100:Open")]
    public float ClutchRate = 0;
    private float MaxTorqueERpm = 0;
    private float MaxTorque = 0;
    private float EngineRpm = 0;
    private float exhaustPatCoEff;

    //Speed Based Values
    private float velocityBasedEngineRpm, velocityBasedWheelRpm, currentShiftMaxSpeed, currentShiftMinSpeed, forwardSpeed, currentShiftMaxWheelRpm, wheelCircumference;

    //Car and Engine Properties
    [Range(0, 3)]
    public float SteeringSensitivity = 0.7f;
    public float gearShiftTime, cutOffRpm , Throttle;

    private float wheelRadius, massOfCar, gas, wheelTork, wheelRpm, highGearRpm, lowGearRpm;
    
    public float MaxBrakeTorque, ActiveBrakeRate, ActiveEngineTorque;
    private int currentGear, poweredWheelCount;

    private bool handBrakeActive, isGearShifting;
    #endregion

    #region Air Resistance
    public float accelerationOfGravity, cDrag, airDensity, crosSection, cInner, cWheel;
    [Range(0, 5)]
    public float CDownForce = 1;
    private float totalResistance, airResistance, rollingResistance, innerResistance, downForce;

    private bool isCarOnAir;
    #endregion

    #region Sound Properties
    /// <summary>
    /// This Section is related with engine Rpm Sound.
    /// </summary>

    private SoundFilters SelectedSoundFilterSet;
    public SoundFilterSets CamSetup = new SoundFilterSets();
    [HideInInspector]
    public int SelectedCamPositionIndex = 0;

    public AudioClip[] EngineAudioClips;
    private WavPart[] EngineAudioSources;
    public AnimationCurve volumeCurve;
    public int RPMInterval = 1000;
    private float soundRpm;
    private AudioMixer EngineVolumeMixer;
    private AudioMixer OtherVolumeMixer;
    private AudioMixerSnapshot[] mixerSnapShots;

    /// <summary>
    /// This Section is related with other sounds of the car.
    /// </summary>
    private AudioSource NOSAudioSource = new AudioSource();
    private AudioSource EngineFalureAudioSource = new AudioSource();
    private AudioSource OneShotAudioSource = new AudioSource();
    private AudioSource TireSkidAudioSource = new AudioSource();

    /// <summary>
    /// Tire Skid Sound.
    /// </summary>
    private float MinSleepVelocity = 3;
    [Range(0, 1)]
    public float ExsausthPathRandomizer = 0.5f;
    [Range(0, 1)]
    public float TurboSoundRandomizer = 0.5f;
    #endregion

    #region Calculation Variables
    private Queue SpeedHistory = new Queue();
    private Queue AxisAccelarations = new Queue();
    private float RpmBasedMaxSpeed = 0;

    private float CR, GBERpm, AvgERpm, WBERpm;
    private float GearRate = 0;
    private CarDirection CarDirection = CarDirection.None;

    [Range(0,1), Tooltip("Percent Of Max Engine Rpm")]
    public float highGearRpmRate;
    [Range(0, 1), Tooltip("Percent Of Max Engine Rpm")]
    public float lowGearRpmRate;
    /// <summary>
    /// ABS Setup
    /// </summary>
    public bool ABS = true;
    [Range(0, 1f)]
    public float ABSFrequency = 0.002f;
    [Range(0, 1)]
    private float ABSTempBrakeRate = 0;
    private bool ABSReleaseActive = false;

    /// <summary>
    /// Traction Control Setup
    /// </summary>
    public bool TractionControl= true;
    [Range(0.1f, 1f)]
    public float TCAggressivness = 0.9f;
    private bool TCSControlActive = false;
    #endregion

    private void Awake()
    {
		lastFixedUpdateTime = Time.time;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;

        #region initials
        SetInitialvariables();

        SetSoundSystem();

        #endregion

        MinEngineRpm = EngineAudioSources[0].centerRPM + MinEngineRpmOffsett;
        MaxEngineRpm = EngineAudioSources[EngineAudioSources.Length - 1].centerRPM + MaxEngineRpmOffsett;
        highGearRpm = MaxEngineRpm * highGearRpmRate;
        lowGearRpm = MaxEngineRpm * lowGearRpmRate;
        StartCoroutine(TurnSignalLight());
        StartCoroutine(AdjustClutch());
    }

    void FixedUpdate()
    {
        #region Computer Control
        lastFixedUpdateTime = Time.time;

        gas = Input.GetAxis("Vertical");
        steering = Input.GetAxis("Horizontal") * MaxSteerAngle;

        ClutchRate = Input.GetAxis("ClutchRate");
        // ClutchRate = Input.GetKey(KeyCode.LeftShift) ? 1f : 0f;

        CalculateSteerAngles();
        CalculateThrottleAndTorques(gas);

        ControlLights();
        #endregion

        for (int i = 0; i < wheelProperties.wheelColliders.Length; i++)
        {
            WheelCollider wc = wheelProperties.wheelColliders[i];
            Vector3 position;
            Quaternion rotation;
            wc.GetWorldPose(out position, out rotation);

            wheelProperties.TireMeshes[i].transform.position = position;
            wheelProperties.TireMeshes[i].transform.rotation = rotation;
        }
        CalculateDynamicData();
        AddForces();
        VelocityBasedVariables();

        SetEngineRpm();
        FindBestShift();
    }

    private CarData carData = new CarData();
    public void CarDataClassUpdate()
    {
        
    }



    private void LateUpdate()
    {
        SetParticleSystemsAndFrictions();
    }
  
    private float steering = 0;
    private void CalculateSteerAngles()
    {
        float InnerSteer = 0, OuterSteer = 0;
        float l = wheelProperties.AxleDistance;
        float w = wheelProperties.WheelDistance;
        InnerSteer = steering;
        float ri = l / Mathf.Tan(Mathf.PI * steering / 180);
        OuterSteer = Mathf.Atan(l / (ri + w)) * 180 / Mathf.PI;

        wheelProperties.wheelColliders[0].steerAngle = OuterSteer;
        wheelProperties.wheelColliders[1].steerAngle = InnerSteer;

        switch (SteeringWheelAxis)
        {
            case Axis.x:
                SteeringWheel.transform.localEulerAngles = new Vector3(-steering * SteeringWheelMultiplier, SOP.y, SOP.z);
                break;
            case Axis.y:
                SteeringWheel.transform.localEulerAngles = new Vector3(SOP.x, -steering * SteeringWheelMultiplier, SOP.z);
                break;
            case Axis.z:
                SteeringWheel.transform.localEulerAngles = new Vector3(SOP.x, SOP.y, -steering * SteeringWheelMultiplier);
                break;
            default:
                break;
        }
        switch (BrakeAxis)
        {
            case Axis.x:
                LeftBrakeSystem.transform.localEulerAngles = new Vector3(OuterSteer, LFBOP.y, LFBOP.z);
                RightBrakeSystem.transform.localEulerAngles = new Vector3(InnerSteer, RFBOP.y, RFBOP.z);
                break;
            case Axis.y:
                LeftBrakeSystem.transform.localEulerAngles = new Vector3(LFBOP.x, OuterSteer, LFBOP.z);
                RightBrakeSystem.transform.localEulerAngles = new Vector3(RFBOP.x, InnerSteer, RFBOP.z);
                break;
            case Axis.z:
                LeftBrakeSystem.transform.localEulerAngles = new Vector3(LFBOP.x, LFBOP.y, OuterSteer);
                RightBrakeSystem.transform.localEulerAngles = new Vector3(RFBOP.x, RFBOP.y, InnerSteer);
                break;
            default:
                break;
        }

        float calibratedSpeed = 0;
        if (SpeedType == SpeedType.Km)
            calibratedSpeed = localVelocity.z * 3.6f;
        else if (SpeedType == SpeedType.Mile)
            calibratedSpeed = localVelocity.z * 2.23693f;

        if (SpeedNeedleDirection == TurnDirection.cw)
            calibratedSpeed = -SpeedNeedleRange * calibratedSpeed / MaxSpeedForSpeedNeedle;
        else if (SpeedNeedleDirection == TurnDirection.ccw)
            calibratedSpeed = SpeedNeedleRange * calibratedSpeed / MaxSpeedForSpeedNeedle;

        switch (SpeedNeedleAxis)
        {
            case Axis.x:
                SpeedNeedle.transform.localEulerAngles = new Vector3(calibratedSpeed, SPEEDNEEDLEOP.y, SPEEDNEEDLEOP.z);
                break;
            case Axis.y:
                SpeedNeedle.transform.localEulerAngles = new Vector3(SPEEDNEEDLEOP.x, calibratedSpeed, SPEEDNEEDLEOP.z);
                break;
            case Axis.z:
                SpeedNeedle.transform.localEulerAngles = new Vector3(SPEEDNEEDLEOP.x, SPEEDNEEDLEOP.y, calibratedSpeed);
                break;
            default:
                break;
        }

        float calibratedRpm = 0;
        if (RpmNeedleDirection == TurnDirection.cw)
            calibratedRpm = -RpmNeedleRange * soundRpm / MaxRpmForSpeedNeedle;
        else if (RpmNeedleDirection == TurnDirection.ccw)
            calibratedRpm = RpmNeedleRange * soundRpm / MaxRpmForSpeedNeedle;

        switch (RpmNeedleAxis)
        {
            case Axis.x:
                RpmNeedle.transform.localEulerAngles = new Vector3(RPMNEEDLEOP.x + calibratedRpm, RPMNEEDLEOP.y, RPMNEEDLEOP.z);
                break;
            case Axis.y:
                RpmNeedle.transform.localEulerAngles = new Vector3(RPMNEEDLEOP.x, RPMNEEDLEOP.y + calibratedRpm, RPMNEEDLEOP.z);
                break;
            case Axis.z:
                RpmNeedle.transform.localEulerAngles = new Vector3(RPMNEEDLEOP.x, RPMNEEDLEOP.y, RPMNEEDLEOP.z + calibratedRpm);
                break;
            default:
                break;
        }
    }

    private float tempCutOffFrequency = 0;
    private void CalculateThrottleAndTorques(float gas)
    {
        if (gas > 0)
        {
            ActiveBrakeRate = 0;
            Throttle = Mathf.Clamp01(Mathf.Abs(gas));
            CarDirection = CarDirection.Forward;
        }

        if (gas == 0)
        {
            CarDirection = CarDirection.None;
            Throttle = 0;
            ActiveBrakeRate = 0;
        }

        if (gas < 0)
        {
            if (localVelocity.z > MinSleepVelocity)
            {
                ActiveBrakeRate = Mathf.Clamp01(Mathf.Abs(gas));
                Throttle = 0;
                CarDirection = CarDirection.Forward;
            }
            else if (localVelocity.z <= MinSleepVelocity)
            {
                ActiveBrakeRate = 0;
                Throttle = Mathf.Clamp01(Mathf.Abs(gas));
                CarDirection = CarDirection.Backward;
            }
        }

        if (isGearShifting == true)
        {
            Throttle = 0;
        }
        
        if (LaunchControlMode == LaunchControlMode.LaunchControl)
        {
            if (gas > 0)
            {
                ActiveBrakeRate = 10;
                Throttle = Mathf.Clamp01(Mathf.Abs(gas));
            } else
            {
                ActiveBrakeRate = 1;
                Throttle = 0;
            }
        }

        if (handBrakeActive)
        {
            ActiveBrakeRate = 1;
            Throttle = 0;
        }

        exhaustPatCoEff += Throttle;

        if (CarDirection == CarDirection.Forward || CarDirection == CarDirection.None)
        {
            if (currentGear == -1)
            {
                currentGear = 0;
            }
            GearRate = GearRatios[currentGear] * finalRatio;
        }
        else if (CarDirection == CarDirection.Backward)
        {
            currentGear = 0;
            GearRate = -rearRatio * finalRatio;
        }

        float distortion;

        if (Throttle > 0)
        {
            tempCutOffFrequency = Mathf.Lerp(tempCutOffFrequency, SelectedSoundFilterSet.EngineLowPassCutOffHigh, 0.05f);
            distortion = Mathf.Lerp(0, 1, soundRpm / MaxEngineRpm);
        }
        else
        {
            tempCutOffFrequency = Mathf.Lerp(tempCutOffFrequency, SelectedSoundFilterSet.EngineLowPassCutOffLow, 0.95f);
            distortion = Mathf.Lerp(0, 1, 0);
        }

        EngineVolumeMixer.SetFloat("CuttOffFrequency", tempCutOffFrequency);

        EngineVolumeMixer.SetFloat("DistortionLevel", SelectedSoundFilterSet.EngineDistortionLevel.Evaluate(distortion));

        OtherVolumeMixer.SetFloat("DistortionLevel", SelectedSoundFilterSet.EngineDistortionLevel.Evaluate(distortion));
    }

    void Update()
    {
        if (SelectedSoundFilterSet != CamSetup.SoundsAtCamPositions[SelectedCamPositionIndex])
        {
            SelectedSoundFilterSet = CamSetup.SoundsAtCamPositions[SelectedCamPositionIndex];
        }

        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(kcode) && (Input.GetKeyDown(frontLightKey) || 
            Input.GetKeyDown(turnSignalRightKey) || Input.GetKeyDown(turnSignalLeftKey) ||
            Input.GetKeyDown(turnSignalAllFour) || Input.GetKeyDown(turnSignalOff)))
                ControlLights(kcode);

            if (Input.GetKey(kcode) && (Input.GetKeyDown(LaunchControl) || Input.GetKeyDown(ShiftUp) || Input.GetKeyDown(ShiftDown) || Input.GetKeyUp(HandBrake)))
            {
                if (kcode == LaunchControl)
                {
                    if (LaunchControlMode == Enums.LaunchControlMode.Idle)
                        LaunchControlMode = Enums.LaunchControlMode.LaunchControl;
                    else if (LaunchControlMode == Enums.LaunchControlMode.LaunchControl)
                        LaunchControlMode = Enums.LaunchControlMode.ReadyToGo;
                    else if (LaunchControlMode == Enums.LaunchControlMode.ReadyToGo)
                        LaunchControlMode = Enums.LaunchControlMode.Idle;
                }

                if (GearRate > 0)
                {
                    if (currentGear > 0 && kcode == ShiftDown)
                    {
                        int tempGear = currentGear;
                        float tempERpm = GearRatios[tempGear - 1] * finalRatio * velocityBasedWheelRpm;
                        if (tempERpm <= MaxEngineRpm)
                        {
                            StartCoroutine(ChangeShift(Enums.ShiftDirection.Down, gearShiftTime));
                        }
                    }
                    if (currentGear < GearRatios.Length - 1 && kcode == ShiftUp)
                    {
                        int tempGear = currentGear;
                        float tempERpm = GearRatios[tempGear + 1] * finalRatio * velocityBasedWheelRpm;
                        if (tempERpm >= MinEngineRpm)
                        {
                            StartCoroutine(ChangeShift(Enums.ShiftDirection.Up, gearShiftTime));
                        }
                    }
                }
            }

            if (Input.GetKeyDown(HandBrake))
            {
                handBrakeActive = true;
            }
            if (Input.GetKeyUp(HandBrake))
            {
                handBrakeActive = false;
            }
        }

        float minInterval = (soundRpm - RPMInterval / 2);
        float maxInterval = (soundRpm + RPMInterval / 2);

        foreach (var item in EngineAudioSources)
        {
            if (item.centerRPM >= minInterval || item.centerRPM <= maxInterval)
            {
                float intervalPos = HDCarControlFunctions.IntervalPos(soundRpm, RPMInterval, item.centerRPM);
                item.audioSource.volume = volumeCurve.Evaluate(intervalPos) * SelectedSoundFilterSet.EngineMasterVolume;
                item.audioSource.pitch = soundRpm / item.centerRPM;
            }
        }
    }

    private void SetEngineRpm()
    {
        CalculatePoweredWheelsAvgRpm();
        
        switch (LaunchControlMode)
        {
            case LaunchControlMode.Idle:
                if (ClutchControlMode == ClutchControlMode.Auto)
                {
                    TargetCR = 0;
                }
                else
                {
                    TargetCR = CRCurve.Evaluate(ClutchRate);
                }
                if (Throttle > 0)
                    EngineRpm += Throttle * AccCoEff * GetEngineHp(EngineRpm);
                else
                    EngineRpm -= DecCoEff * EngineRpm;
                break;
            case LaunchControlMode.LaunchControl:
                if (ClutchControlMode == ClutchControlMode.Auto)
                {
                    if (Throttle > 0)
                    {
                        if (EngineRpm > MaxTorqueERpm)
                        {
                            TargetCR += CRSensitivity;
                        }
                        else
                        {
                            TargetCR -= CRSensitivity;
                        }
                        TargetCR = Mathf.Clamp01(TargetCR);

                        EngineRpm = Mathf.Lerp(MinEngineRpm, MaxEngineRpm, 1 - CR);
                    }
                    else
                    {
                        TargetCR = 0;
                        EngineRpm -= DecCoEff * EngineRpm;
                    }
                }
                else
                {
                    TargetCR = CRCurve.Evaluate(ClutchRate);
                    if (Throttle > 0)
                    {
                        EngineRpm = Mathf.Lerp(MinEngineRpm, MaxEngineRpm, 1 - CR);
                    }
                    else
                    {
                        EngineRpm -= DecCoEff * EngineRpm;
                    }
                }
                break;
            case LaunchControlMode.ReadyToGo:
                if (ClutchControlMode == ClutchControlMode.Auto)
                {
                    if (!isGearShifting)
                    {
                        if (WBERpm < EngineRpm * 0.95f && TargetCR <= 1)
                        {
                            TargetCR += Mathf.Abs(EngineRpm - WBERpm) * 0.1f / MaxEngineRpm;
                            Mathf.Clamp01(TargetCR);
                        }
                        else
                        {
                            TargetCR = 1;
                        }
                    }
                    else
                    {
                        TargetCR = 0;
                    }
                }
                else
                {
                    if (!isGearShifting)
                    {
                        TargetCR = CRCurve.Evaluate(ClutchRate);
                    }
                    else
                    {
                        TargetCR = 0;
                    }
                }

                if (Throttle > 0)
                    GBERpm += Throttle * AccCoEff * GetEngineHp(EngineRpm);
                else
                    GBERpm -= DecCoEff * EngineRpm;

                GBERpm = Mathf.Clamp(GBERpm, 0.9f * MinEngineRpm, 1.05f * MaxEngineRpm);

                WBERpm = Mathf.Clamp(Mathf.Abs(wheelRpm * GearRate), 0.9f * MinEngineRpm, 1.05f * MaxEngineRpm);

                EngineRpm = Mathf.Lerp(GBERpm, WBERpm, CR);
                break;
            default:
                break;
        }

        EngineRpm = Mathf.Clamp(EngineRpm, 0.9f * MinEngineRpm, 1.05f * MaxEngineRpm);

        soundRpm = Mathf.Lerp(soundRpm, EngineRpm, 0.05f);
        if (soundRpm > MaxEngineRpm && Throttle > 0)
        {
            soundRpm -= cutOffRpm;
        }

        ApplyTorques();
    }

    void ApplyTorques()
    {
        wheelTork = GetTork(EngineRpm, TorqueType.Wheel);

        if (LaunchControlMode == Enums.LaunchControlMode.Idle)
        {
            for (int i = 0; i < wheelProperties.wheelColliders.Length; i++)
            {
                wheelProperties.wheelColliders[i].brakeTorque = MaxBrakeTorque * ActiveBrakeRate;
                wheelProperties.wheelColliders[i].motorTorque = 0;
            }
        }
        if (LaunchControlMode == Enums.LaunchControlMode.LaunchControl)
        {
            for (int i = 0; i < wheelProperties.wheelColliders.Length; i++)
            {
                wheelProperties.wheelColliders[i].brakeTorque = MaxBrakeTorque;
                wheelProperties.wheelColliders[i].motorTorque = wheelTork / poweredWheelCount;
            }
        }
        if (LaunchControlMode == Enums.LaunchControlMode.ReadyToGo)
        {
            for (int i = 0; i < wheelProperties.wheelColliders.Count(); i++)
            {
                WheelCollider wc = wheelProperties.wheelColliders[i];

                //Clear Wheel Damping Rates.
                wheelProperties.wheelColliders[i].wheelDampingRate = InitialDampingRate;

                WheelHit hit;
                if (wheelProperties.wheelColliders[i].GetGroundHit(out hit))
                {
                    wheelProperties.wheelColliders[i].forwardFriction =
                        GetFrictionCurve(localVelocity, i, FrictionType.Forward, wheelProperties.ForwardStiffness[i] * hit.collider.material.dynamicFriction,
                        wheelProperties.wheelColliders[i].forwardFriction.extremumSlip, wheelProperties.wheelColliders[i].forwardFriction.extremumValue,
                        wheelProperties.wheelColliders[i].forwardFriction.asymptoteSlip, wheelProperties.wheelColliders[i].forwardFriction.asymptoteValue);

                    wheelProperties.wheelColliders[i].sidewaysFriction =
                        GetFrictionCurve(localVelocity, i, FrictionType.Side, wheelProperties.SideStiffness[i] * hit.collider.material.dynamicFriction,
                        wheelProperties.wheelColliders[i].sidewaysFriction.extremumSlip, wheelProperties.wheelColliders[i].sidewaysFriction.extremumValue,
                        wheelProperties.wheelColliders[i].sidewaysFriction.asymptoteSlip, wheelProperties.wheelColliders[i].sidewaysFriction.asymptoteValue);
                }

                #region Calculate Brakes
                float CurrentBrakeForce = 0;
                if (ActiveBrakeRate > 0)
                {
                    TCSControlActive = false;

                    ABSTempBrakeRate = 1;
                }

                if (Throttle > 0)
                {
                    ABSReleaseActive = false;
                }

                if (ActiveBrakeRate > 0)
                {
                    if (ABS && CarDirection == CarDirection.Forward)
                    {
                        if (wc.rpm < velocityBasedWheelRpm * 0.3f)
                        {
                            ABSReleaseActive = true;
                        }

                        if (wc.rpm > velocityBasedWheelRpm * 0.8f)
                        {
                            ABSReleaseActive = false;
                        }

                        ABSTempBrakeRate += HDCarControlFunctions.ABSBrakeRate(ABSFrequency, ABSReleaseActive, localVelocity.z);

                        ABSTempBrakeRate = Mathf.Clamp01(ABSTempBrakeRate);

                        CurrentBrakeForce = ABSTempBrakeRate * MaxBrakeTorque;
                    }
                    else
                    {
                        CurrentBrakeForce = ActiveBrakeRate * MaxBrakeTorque;
                        ABSTempBrakeRate = 0;
                        ABSReleaseActive = true;
                    }
                }
                else
                {
                    CurrentBrakeForce = 0;
                    ABSTempBrakeRate = 0;
                    ABSReleaseActive = false;
                }

                wheelProperties.wheelColliders[i].brakeTorque = CurrentBrakeForce;
                wheelProperties.BrakeTempratureControl(i, ActiveBrakeRate * (wc.rpm / 60) * BrakeHeatingCoEff, BrakeCoolingCoEff * (1 + Mathf.Abs(localVelocity.z / 10)));
                #endregion

                #region Calculate Torques
                if (Throttle > 0 && wheelProperties.isPowered[i])
                {
                    if (TractionControl == true && wc.rpm > 0 && velocityBasedWheelRpm / wc.rpm < TCAggressivness)
                    {
                        wheelProperties.wheelColliders[i].motorTorque = HDCarControlFunctions.MotorTorque(true, velocityBasedWheelRpm, wc.rpm, wheelTork, poweredWheelCount);
                        TCSControlActive = true;
                    }
                    else
                    {
                        wheelProperties.wheelColliders[i].motorTorque = HDCarControlFunctions.MotorTorque(false, velocityBasedWheelRpm, wc.rpm, wheelTork, poweredWheelCount);
                        TCSControlActive = false;
                    }
                }
                else
                {
                    TCSControlActive = false;
                    wheelProperties.wheelColliders[i].motorTorque = 0;
                }
                wheelProperties.TireTempratureControl(i, totalintensity * TireHeatingCoEff, TireCoolingCoEff * (1 + Mathf.Abs(localVelocity.z) / 10));
                #endregion
            }
        }
    }

    void FindBestShift()
    {
        if (GearBoxControlMode == GearBoxControlMode.Auto)
        {
            if (localVelocity.z >= currentShiftMaxSpeed && currentGear < GearRatios.Length - 1 && isGearShifting == false)
            {
                StartCoroutine(ChangeShift(ShiftDirection.Up, gearShiftTime));
                if (TurboSoundRandomizer > Random.value)
                {
                    int index = Random.Range(0, audioManager.sounds.Where(x => x.Type == SoundType.TurboCoolDown).Count());
                    AudioClip clip = audioManager.sounds.Where(x => x.Type == SoundType.TurboCoolDown).Skip(index).First().clip;
                    OneShotAudioSource.PlayOneShot(clip);
                }

                StartCoroutine(ExhaustPat());
            }
            if (forwardSpeed <= currentShiftMinSpeed && currentGear > 0 && isGearShifting == false)
            {
                StartCoroutine(ChangeShift(ShiftDirection.Down, gearShiftTime));
            }
        }
        else if (GearBoxControlMode == GearBoxControlMode.Manual)
        {
            //handled in update section.
        }
    }
    
    private void AddForces()
    {
        localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        rb.AddRelativeForce(new Vector3(0, 0, -totalResistance));
        rb.AddRelativeForce(new Vector3(0, -downForce, 0));
        
        //Side Force
        if (localVelocity.z > 0)
        {
            float totalforce = 0;
            for (int i = 0; i < wheelProperties.wheelColliders.Length; i++)
            {
                WheelCollider wc = wheelProperties.wheelColliders[i];
                WheelHit hit;
                if (wc.GetGroundHit(out hit) && localVelocity.x < steering)
                {
                    totalforce += SideForceMultiplier * rb.mass * 10 *
                               Throttle * (steering / MaxSteerAngle) *
                               wheelProperties.ForwardSkidIntensitiy[i];
                }
            }
            Vector3 forceVector = totalforce * transform.forward;
            rb.AddForce(forceVector);
        }

        rb.angularDamping = Mathf.Clamp(localVelocity.z * 0.05f, 0, 2);

        if (localVelocity.z < -10 && CarDirection == CarDirection.Backward)
        {
            rb.linearDamping = (Mathf.Abs(localVelocity.z) - 10) * 0.1f;
        } else
        {
            rb.linearDamping = 0;    
        }
    }

    private void CalculatePoweredWheelsAvgRpm()
    {
        wheelRpm = 0;
        for (int i = 0; i < wheelProperties.wheelColliders.Length; i++)
        {
            if (wheelProperties.isPowered[i])
            {
                wheelRpm += Mathf.Abs(wheelProperties.wheelColliders[i].rpm);
            }
        }
        wheelRpm /= poweredWheelCount;
    }

    void VelocityBasedVariables()
    {
        forwardSpeed = localVelocity.z;
        currentShiftMaxSpeed = wheelCircumference * highGearRpm / (GearRatios[currentGear] * finalRatio * 60);
        currentShiftMinSpeed = wheelCircumference * lowGearRpm / (GearRatios[currentGear] * finalRatio * 60);
        velocityBasedWheelRpm = Math.Abs(60 * forwardSpeed / 2.08f);
        velocityBasedEngineRpm = velocityBasedWheelRpm * GearRate;
        currentShiftMaxWheelRpm = MaxEngineRpm / (GearRate);

        wheelProperties.CalculateSpeedDiffsAndIntensities(rb);
    }
    
    void CalculateDynamicData()
    {
        airResistance = 0.5f * cDrag * crosSection * airDensity * localVelocity.sqrMagnitude;
        rollingResistance = Mathf.Abs(cWheel * massOfCar * accelerationOfGravity * wheelRpm);
        innerResistance = cInner * AvgERpm;
        downForce = CDownForce * localVelocity.magnitude * localVelocity.magnitude/4;
        totalResistance = airResistance + innerResistance + rollingResistance;

        DynamicData.wheelProperties = wheelProperties;
        DynamicData.EngineRpm = soundRpm;

        for (int i = 0; i < wheelProperties.wheelColliders.Length; i++)
        {
            WheelCollider item = wheelProperties.wheelColliders[i];
            DynamicData.WheelsRpm[i] = item.rpm;
            DynamicData.WheelsMotorTorque[i] = item.motorTorque;
            DynamicData.WheelsBrakeTorque[i] = item.brakeTorque;
            WheelHit hit = new WheelHit();
            if (item.GetGroundHit(out hit))
                DynamicData.WheelHits[i] = hit;
        }

        DynamicData.PowerHP = GetEngineHp(AvgERpm);
        DynamicData.PowerKw = DynamicData.PowerHP * 0.745f;
        DynamicData.EngineTorque = ActiveEngineTorque;
        DynamicData.Speed = localVelocity;
        DynamicData.Position = rb.transform;
        DynamicData.Rotation = rb.transform.rotation;
        DynamicData.CurrenShift = currentGear;
        DynamicData.ClutchRate = CR;
        DynamicData.DownForce = downForce;
        DynamicData.AirFriction = airResistance;
        DynamicData.RollingResistance = rollingResistance;
        DynamicData.InnerResistance = innerResistance;
        DynamicData.IsCarOnAir = isCarOnAir;
        DynamicData.ForwardSkid = 0;
        DynamicData.SideSkid = 0;
        DynamicData.SkidMaterial = null;
    }

    private float TargetCR = 0;
    IEnumerator AdjustClutch()
    {
        while (true)
        {
            CR = Mathf.Lerp(CR, TargetCR, 0.2f);
            yield return null;
        }
    }

    private void SetSoundSystem()
    {
        SelectedSoundFilterSet = CamSetup.SoundsAtCamPositions[SelectedCamPositionIndex];

        EngineVolumeMixer = Resources.Load("EngineVolume") as AudioMixer;
        OtherVolumeMixer = Resources.Load("OtherSounds") as AudioMixer;

        TireSkidAudioSource.outputAudioMixerGroup = OtherVolumeMixer.FindMatchingGroups("Master")[0];
        OneShotAudioSource.outputAudioMixerGroup = OtherVolumeMixer.FindMatchingGroups("Master")[0];
        OneShotAudioSource.volume = SelectedSoundFilterSet.OtherSoundsVolume;

        audioManager = gameObject.GetComponent<AudioManager>();

        EngineAudioSources = new WavPart[EngineAudioClips.Length];

        for (int i = 0; i < EngineAudioSources.Length; i++)
        {
            EngineAudioSources[i] = new WavPart();
            EngineAudioSources[i].audioSource = gameObject.AddComponent<AudioSource>();
            EngineAudioSources[i].audioSource.outputAudioMixerGroup = EngineVolumeMixer.FindMatchingGroups("Master")[0];
        }

        foreach (AudioClip ac in EngineAudioClips)
        {
            int orderInList = int.Parse(ac.name.Substring(0, ac.name.LastIndexOf("_"))) - 1;

            EngineAudioSources[orderInList].audioSource.clip = ac;
            EngineAudioSources[orderInList].audioSource.loop = true;
            EngineAudioSources[orderInList].audioSource.Play();
            EngineAudioSources[orderInList].audioSource.volume = 0;
            EngineAudioSources[orderInList].CalculateCenterRpm();
        }
    }

    IEnumerator ChangeShift(ShiftDirection shiftDirection, float shiftTime)
    {
        TargetCR = 0;
        isGearShifting = true;
        if (shiftDirection == ShiftDirection.Up)
        {
            currentGear++;
            yield return new WaitForSeconds(shiftTime);
        }

        if (shiftDirection == ShiftDirection.Down)
        {
            currentGear--;
            yield return new WaitForSeconds(shiftTime);
        }

        AudioClip ac = audioManager.sounds.Where(x => x.Type == SoundType.ShiftUp).First().clip;
        float volumeScale = audioManager.sounds.Where(x => x.Type == SoundType.ShiftUp).First().volume;
        OneShotAudioSource.PlayOneShot(audioManager.sounds.Where(x => x.Type == SoundType.ShiftUp).First().clip, volumeScale);

        TargetCR = 1;
        isGearShifting = false;
    }

    float GetTork(float rpm, TorqueType torqueType)
    {
        float rpmRate = rpm / MaxEngineRpm;
        float torqueReturn;
        if (torqueType == TorqueType.Engine)
        {
            torqueReturn = GasPeddalEfficiency.Evaluate(rpmRate) * TorkMultiplier * 12629 * GetEngineHp(rpm) / rpm;
        }
        else
        {
            torqueReturn = GearRate * CR * GasPeddalEfficiency.Evaluate(rpmRate) * TorkMultiplier * 12629 * GetEngineHp(rpm) / rpm;
        }

        return torqueReturn;
    }

    private float GetEngineHp(float rpm)
    {
        float rpmRate = rpm / MaxEngineRpm;
        float engineHp = MaxEnginePower * Mathf.Abs(HpCurve.Evaluate(rpmRate)) * HPMultiplier * Throttle;
        return engineHp;
    }

    private bool runAgain = false;
    IEnumerator ExhaustPat()
    {
        if ((exhaustPatCoEff > 100 && Throttle == 0 && ExsausthPathRandomizer > Random.value) || runAgain)
        {
            runAgain = false;
            int index = Random.Range(0, audioManager.sounds.Where(x => x.Type == SoundType.ExhaustPat).Count() - 1);
            AudioClip clip = audioManager.sounds.Where(x => x.Type == SoundType.ExhaustPat).Skip(index).First().clip;
            OneShotAudioSource.volume = SelectedSoundFilterSet.OtherSoundsVolume;
            OneShotAudioSource.PlayOneShot(clip);
            exhaustPatCoEff = 0;

            exhaustPS[0].Play();
            exhaustPS[1].Play();

            yield return new WaitForSeconds(clip.length);

            if (ExsausthPathRandomizer/2 > Random.value && Throttle == 0)
            {
                runAgain = true;
                yield return new WaitForSeconds(clip.length + Random.value);
                StartCoroutine(ExhaustPat());
            }

            exhaustPS[0].Stop();
            exhaustPS[1].Stop();
        }
    }

    IEnumerator TurnSignalLight()
    {
        //-1: Off, 0:All Four, 1:Right Two, 2: Left Two
        while (true)
        {
            if (TurnSignalStatus == Enums.TurnSignal.Left)
            {
                LeftSignalLightObject.GetComponent<Renderer>().material = LightsOnMaterial;
                RightSignalLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
                SignalLightFrontLeft.enabled = true;
                SignalLightRearLeft.enabled = true;
                SignalLightFrontRight.enabled = false;
                SignalLightRearRight.enabled = false;
            }
            else if (TurnSignalStatus == Enums.TurnSignal.Right)
            {
                LeftSignalLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
                RightSignalLightObject.GetComponent<Renderer>().material = LightsOnMaterial;
                SignalLightFrontLeft.enabled = false;
                SignalLightRearLeft.enabled = false;
                SignalLightFrontRight.enabled = true;
                SignalLightRearRight.enabled = true;
            }
            else if (TurnSignalStatus == Enums.TurnSignal.All)
            {
                LeftSignalLightObject.GetComponent<Renderer>().material = LightsOnMaterial;
                RightSignalLightObject.GetComponent<Renderer>().material = LightsOnMaterial;
                SignalLightFrontLeft.enabled = true;
                SignalLightRearLeft.enabled = true;
                SignalLightFrontRight.enabled = true;
                SignalLightRearRight.enabled = true;
            }
            else if (TurnSignalStatus == Enums.TurnSignal.Off)
            {
                LeftSignalLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
                RightSignalLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
                SignalLightFrontLeft.enabled = false;
                SignalLightRearLeft.enabled = false;
                SignalLightFrontRight.enabled = false;
                SignalLightRearRight.enabled = false;
            }
            yield return new WaitForSeconds(0.5f);
            LeftSignalLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
            RightSignalLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
            SignalLightFrontLeft.enabled = false;
            SignalLightRearLeft.enabled = false;
            SignalLightFrontRight.enabled = false;
            SignalLightRearRight.enabled = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public WheelFrictionCurve GetFrictionCurve(Vector3 localvelocity, int index, FrictionType type, float stiffness, float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue)
    {
        WheelFrictionCurve wfc = new WheelFrictionCurve();
        float wheelSteerAngle = Mathf.Abs(wheelProperties.wheelColliders[index].steerAngle);

        if (type == FrictionType.Forward)
        {
            float addOn = FrictionMultiplier *  localvelocity.z/RpmBasedMaxSpeed;
            wfc.stiffness = stiffness + addOn;
            wfc.asymptoteValue = asymptoteValue;
            wfc.asymptoteSlip = asymptoteSlip;
            wfc.extremumSlip = extremumSlip;
            wfc.extremumValue = extremumValue;

        } else if(type == FrictionType.Side) 
        {
            float addOn = -ForwardSkidEffectToSideFriction * wheelProperties.ForwardSkidIntensitiy[index] +
                        FrictionMultiplier * wheelSteerAngle / MaxSteerAngle +
                        FrictionMultiplier * (localvelocity.z / RpmBasedMaxSpeed);
            //float addOn = FrictionMultiplier * relativeSteerAngle + localvelocity.z / RpmBasedMaxSpeed - ForwardSkidEffectToSideFriction * wheelProperties.ForwardSkidIntensitiy[index];
            wfc.stiffness = stiffness + addOn;
            wfc.asymptoteValue = asymptoteValue;
            wfc.asymptoteSlip = asymptoteSlip;
            wfc.extremumSlip = extremumSlip;
            wfc.extremumValue = extremumValue;
        }
        return wfc;
    }

    private void ControlLights()
    {
        if (gas < 0 && localVelocity.z > 0)
        {
            BrakeLightObject.GetComponent<Renderer>().material = LightsOnMaterial;
            ReverseLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
            BrakeLightRight.enabled = true;
            BrakeLightLeft.enabled = true;
            ReverseLightRight.enabled = false;
            ReverseLightLeft.enabled = false;
        }
        else if (gas < 0 && localVelocity.z < 0)
        {
            BrakeLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
            ReverseLightObject.GetComponent<Renderer>().material = LightsOnMaterial;
            BrakeLightRight.enabled = false;
            BrakeLightLeft.enabled = false;
            ReverseLightRight.enabled = true;
            ReverseLightLeft.enabled = true;
        }
        else
        {
            BrakeLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
            ReverseLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
            BrakeLightRight.enabled = false;
            BrakeLightLeft.enabled = false;
            ReverseLightRight.enabled = false;
            ReverseLightLeft.enabled = false;
        }
    }

    private void ControlLights(KeyCode keyCode)
    {
        if (keyCode == frontLightKey)
        {
            Material m = MainLightObject.GetComponent<Renderer>().material;
            if (m.name.Remove(LightsOnMaterial.name.Length) == LightsOnMaterial.name)
            {
                MainLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
                RearSideLightObject.GetComponent<Renderer>().material = LightsOffMaterial;
                MainLightLeft.enabled = false;
                MainLightRight.enabled = false;
                RearSideLightLeft.enabled = false;
                RearSideLightRight.enabled = false;
            }
            else
            {
                MainLightObject.GetComponent<Renderer>().material = LightsOnMaterial;
                RearSideLightObject.GetComponent<Renderer>().material = LightsOnMaterial;
                MainLightLeft.enabled = true;
                MainLightRight.enabled = true;
                RearSideLightLeft.enabled = true;
                RearSideLightRight.enabled = true;
            }
        }

        if (keyCode == turnSignalLeftKey)
        {
            TurnSignalStatus = Enums.TurnSignal.Left;
        }
        else if (keyCode == turnSignalRightKey)
        {
            TurnSignalStatus = Enums.TurnSignal.Right;
        }
        else if (keyCode == turnSignalAllFour)
        {
            TurnSignalStatus = Enums.TurnSignal.All;
        }
        else if (keyCode == turnSignalOff)
        {
            TurnSignalStatus = Enums.TurnSignal.Off;
        }
    }

    void SetInitialvariables()
    {
        #region Set WheelProperties initials.
        wheelProperties.initialize();
        SOP = SteeringWheel.transform.localEulerAngles;
        LFBOP = LeftBrakeSystem.transform.localEulerAngles;
        RFBOP = RightBrakeSystem.transform.localEulerAngles;
        RPMNEEDLEOP = RpmNeedle.transform.localEulerAngles;
        SPEEDNEEDLEOP = SpeedNeedle.transform.localEulerAngles;
        #endregion

        #region General Variables
        TireTemprature = 0;
        exhaustPatCoEff = 0;
        TurnSignalStatus = Enums.TurnSignal.Off;
        soundRpm = 0;
        isGearShifting = false;
        isCarOnAir = false;
        currentShiftMaxSpeed = 0f;
        currentShiftMinSpeed = 0f;
        velocityBasedWheelRpm = 0f;
        velocityBasedEngineRpm = 0;
        EngineRpm = MinEngineRpm;
        AvgERpm = MinEngineRpm;
        GBERpm = MinEngineRpm;
        wheelRpm = 0f;
        wheelTork = 0f;
        currentGear = 0;
        airResistance = 0f;
        forwardSpeed = 0f;
        downForce = 0f;
        wheelRadius = wheelProperties.wheelColliders[0].radius;
        wheelCircumference = 2 * (float)Math.PI * wheelRadius;
        float Rps = MaxEngineRpm / 60; //Rotation in per second.
        RpmBasedMaxSpeed = wheelCircumference * Rps / (GearRatios[GearRatios.Length - 1] * finalRatio);
        poweredWheelCount = wheelProperties.isPowered.Where(x => x == true).Count();
        rollingResistance = 0f;
        innerResistance = 0f;
        totalResistance = 0f;
        massOfCar = rb.mass;
        CR = 0;
        #endregion

        #region AudioSources
        TireSkidAudioSource = gameObject.AddComponent<AudioSource>();
        OneShotAudioSource = gameObject.AddComponent<AudioSource>();
        #endregion

        #region Set Max Torque Engine Rpm
        MaxTorqueERpm = 0;
        float rpm = MinEngineRpm;
        float maxTorqueERpmRate = 0;
        float i = rpm / MaxEngineRpm;
        while (i <= 1)
        {
            float engineHp = MaxEnginePower * Mathf.Abs(HpCurve.Evaluate(i)) * HPMultiplier;
            float torque = GasPeddalEfficiency.Evaluate(i) * TorkMultiplier * 12629 * engineHp / rpm;
            if (torque > MaxTorque)
            {
                MaxTorque = torque;
                maxTorqueERpmRate = i;
            }
            rpm += 50;
            i = rpm / MaxEngineRpm;
        }
        MaxTorqueERpm = maxTorqueERpmRate * MaxEngineRpm;
        #endregion
    }
    
    private float IntensityForSkidVolume = 0;
    private float lastFixedUpdateTime = 0;
    private float totalintensity = 0;
    void SetParticleSystemsAndFrictions()
    {
        int groundThatHasMaxValue;

        #region Detect Ground
        foreach (var groundMaterial in GroundMatrials)
        {
            groundMaterial.TiresCountOnTheGround = 0;
        }

        for (int i = 0; i < wheelProperties.wheelColliders.Length; i++)
        {
            WheelCollider wc = wheelProperties.wheelColliders[i];
            isCarOnAir = true;
            wc.GetGroundHit(out WheelHit hit);
            foreach (var groundMaterial in GroundMatrials)
            {
                if (hit.collider != null && hit.collider.material.name.Contains(groundMaterial.PhysicMaterial.name))
                {
                    groundMaterial.TiresCountOnTheGround++;
                    isCarOnAir = false;
                }
            }
        }
        #endregion

        #region  Set Tire Skid Volume
        float sideSpeedDiffForVolume = 0;
        float forwardSpeedDiffForVolume = 0;
        int wheelCount = wheelProperties.wheelColliders.Count();

        for (int i = 0; i < wheelCount; i++)
        {
            sideSpeedDiffForVolume += wheelProperties.SideSpeedDiff[i];
            forwardSpeedDiffForVolume += wheelProperties.ForwardSpeedDif[i];
        }

        sideSpeedDiffForVolume /= wheelCount;
        forwardSpeedDiffForVolume /= wheelCount;

        float SideIntensityForSkidVolume = Mathf.Clamp(sideSpeedDiffForVolume - MinSideSpeedForSkidVolume, 0, 10) / 10;
        float ForwardIntensityForSkidVolume = Mathf.Clamp(forwardSpeedDiffForVolume - MinForwardSpeedForSkidVolume, 0, 10) / 10;

        float TempIntensityForSkidVolume = Mathf.Clamp01((ForwardIntensityForSkidVolume + SideIntensityForSkidVolume) / 2);

        IntensityForSkidVolume = Mathf.Lerp(IntensityForSkidVolume, TempIntensityForSkidVolume, 0.7f);

        if ((sideSpeedDiffForVolume > MinSideSpeedForSkidVolume || forwardSpeedDiffForVolume > MinForwardSpeedForSkidVolume) && !isCarOnAir)
        {
            TireTemprature += TireHeatingCoEff;
            TireTemprature = Mathf.Clamp(TireTemprature, -1, 1);
            Sound sound;
            groundThatHasMaxValue = 0;
            foreach (GroundMatrial gm in GroundMatrials)
            {
                if (gm.TiresCountOnTheGround > groundThatHasMaxValue)
                    groundThatHasMaxValue = gm.TiresCountOnTheGround;
            }
            sound = GroundMatrials.First(x => x.TiresCountOnTheGround == groundThatHasMaxValue).SkidSound;
            TireSkidAudioSource.volume = IntensityForSkidVolume * SelectedSoundFilterSet.OtherSoundsVolume;
            PlaySound(TireSkidAudioSource, sound, true, Random.Range(0, 0.5f));
        }
        else
        {
            StopSound(TireSkidAudioSource);
        }
        #endregion

        #region Tire Dust and Skid Strength
        for (int i = 0; i < wheelProperties.wheelColliders.Length; i++)
        {
            WheelCollider wc = wheelProperties.wheelColliders[i];
            ParticleSystem[] tbps = TireBurnOutParticleSystem.GetComponentsInChildren<ParticleSystem>();

            GroundMatrial gm = null;
            if (wheelProperties.wheelColliders[i].GetGroundHit(out WheelHit wheelHit))
            {
                string groundPhysicalMaterialName = wheelHit.collider.material.name.Substring(0, wheelHit.collider.material.name.IndexOf(" (Instance)"));
                gm = GroundMatrials.Where(x => x.PhysicMaterial.name == groundPhysicalMaterialName).FirstOrDefault();
            }
            
            if (gm != null && gm.EnableTireBurnout)
            {
                foreach (ParticleSystem ps in tbps)
                {
                    if (ps.transform.parent == TireBurnOutParticleSystem.transform)
                    {
                        var emission = ps.emission;

                        if (wheelProperties.SignedForwardSpeedDif[i] > MinForwardSpeedForSkids && CarDirection == CarDirection.Forward)
                        {
                            emission.enabled = true;
                            emission.rateOverTime = TireBurnOutParticleSystemIntensity * wheelProperties.ForwardSpeedDif[i];
                        }
                        else
                        {
                            emission.enabled = false;

                        }
                    }
                }
            } else
            {
                foreach (ParticleSystem ps in tbps)
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                    emission.rateOverTime = 0;
                }
            }


            if (wheelProperties.wheelColliders[i].GetGroundHit(out WheelHit hit))
            {
                float forwardintensity = 0;
                float sideintensity = 0;
                if (wheelProperties.ForwardSpeedDif[i] > MinForwardSpeedForSkids)
                {
                    forwardintensity = wheelProperties.ForwardSkidIntensitiy[i];
                }

                if (wheelProperties.SideSpeedDiff[i] > MinSideSpeedForSkids)
                {
                    sideintensity = wheelProperties.SideSkidIntensitiy[i];
                }

                totalintensity = (forwardintensity + sideintensity) / 2;

                Vector3 skidPoint = hit.point + (rb.linearVelocity * (Time.time - lastFixedUpdateTime));
                wheelProperties.LastIndex[i] = SkidMarkController.AddSkidMark(skidPoint, hit.normal, totalintensity, wheelProperties.LastIndex[i], 1.1f - wheelProperties.SideSkidIntensitiy[i] / 3);

                foreach (GroundMatrial groundMaterial in GroundMatrials)
                {
                    if (hit.collider.material.name.Contains(groundMaterial.PhysicMaterial.name))
                    {
                        bool MaterialAlreadyInUse = false;
                        int PsIndexOfExistedPS = -1;
                        float FirstUpdatedTime = int.MaxValue;
                        float LastUpdatedTime = int.MinValue;
                        int FirstUpdatedPS = -1;
                        int LastUpdatedPS = -1;

                        for (int j = 0; j < wheelProperties.TireParticleSytems[i].PSs.Length; j++)
                        {
                            if (wheelProperties.TireParticleSytems[i].TimeOfUpdate[j] < FirstUpdatedTime)
                            {
                                FirstUpdatedTime = wheelProperties.TireParticleSytems[i].TimeOfUpdate[j];
                                FirstUpdatedPS = j;
                            }

                            if (wheelProperties.TireParticleSytems[i].TimeOfUpdate[j] > LastUpdatedTime)
                            {
                                LastUpdatedTime = wheelProperties.TireParticleSytems[i].TimeOfUpdate[j];
                                LastUpdatedPS = j;
                            }

                            ParticleSystem ps = wheelProperties.TireParticleSytems[i].PSs[j];
                            if (ps.GetComponent<ParticleSystemRenderer>().material.name.Contains(groundMaterial.TireDustMaterial.name))
                            {
                                MaterialAlreadyInUse = true;
                                PsIndexOfExistedPS = j;
                            }

                            var emission = wheelProperties.TireParticleSytems[i].PSs[j].emission;
                            emission.rateOverTime = wheelProperties.TireTemprature[i] * totalintensity * tireDustParticleCount;
                            var main = wheelProperties.TireParticleSytems[i].PSs[FirstUpdatedPS].main;
                            main.startSpeed = 3 + Mathf.Clamp(Mathf.Log10(Mathf.Abs(rb.linearVelocity.magnitude)), 0, 10);
                        }

                        if (MaterialAlreadyInUse == true)
                        {
                            wheelProperties.TireParticleSytems[i].TimeOfUpdate[PsIndexOfExistedPS] = Time.unscaledTime;
                            wheelProperties.TireParticleSytems[i].PSs[PsIndexOfExistedPS].Play();

                            for (int j = 0; j < wheelProperties.TireParticleSytems[i].PSs.Length; j++)
                            {
                                if (j != PsIndexOfExistedPS)
                                {
                                    var emission = wheelProperties.TireParticleSytems[i].PSs[j].emission;
                                    emission.rateOverTime = 0;
                                    var main = wheelProperties.TireParticleSytems[i].PSs[FirstUpdatedPS].main;
                                    main.startSpeed = 3 + Mathf.Clamp(Mathf.Log10(Mathf.Abs(rb.linearVelocity.magnitude)), 0, 10);
                                }
                            }
                        }
                        else
                        {
                            wheelProperties.TireParticleSytems[i].PSs[FirstUpdatedPS].Clear();
                            wheelProperties.TireParticleSytems[i].PSs[FirstUpdatedPS].GetComponent<ParticleSystemRenderer>().material = groundMaterial.TireDustMaterial;
                            wheelProperties.TireParticleSytems[i].TimeOfUpdate[FirstUpdatedPS] = Time.unscaledTime;
                            var emission = wheelProperties.TireParticleSytems[i].PSs[FirstUpdatedPS].emission;
                            var main = wheelProperties.TireParticleSytems[i].PSs[FirstUpdatedPS].main;
                            emission.rateOverTime = wheelProperties.TireTemprature[i] * totalintensity * tireDustParticleCount;
                            main.startSpeed = 3 + Mathf.Clamp(Mathf.Log10(Mathf.Abs(rb.linearVelocity.magnitude)), 0, 10);
                            wheelProperties.TireParticleSytems[i].PSs[FirstUpdatedPS].Play();
                        }
                    }
                }
            }
            else if (hit.collider == null)
            {
                wheelProperties.LastIndex[i] = -1;
                
                if (hit.collider == null || gas <= 0 && totalintensity < 0.1f)
                {
                    foreach (ParticleSystem ps in wheelProperties.TireParticleSytems[i].PSs)
                    {
                        var emission = ps.emission;
                        emission.rateOverTime = 0;
                    }
                }
            }
        }
        #endregion

        #region Exhaust particle Systems
        StartCoroutine(ExhaustPat());
        #endregion
    }

    private int PlaySound(AudioSource audioSource, Sound sound, bool loop, float time)
    {
        if (audioSource.clip == null)
        {
            audioSource.clip = sound.clip;
            audioSource.loop = loop;
            audioSource.time = audioSource.clip.length * time;
            audioSource.Play();
        }
        else if (audioSource.clip != null)
        {
            if (audioSource.clip.name == sound.clip.name && audioSource.isPlaying == false)
            {
                audioSource.time = 0;
                audioSource.time = audioSource.clip.length * time;
                audioSource.Play();
            }
            else if (audioSource.clip.name != sound.clip.name)
            {
                audioSource.time = 0;
                audioSource.clip = sound.clip;
                audioSource.Play();
            }
        }
        return audioSource.GetInstanceID();
    }

    private void StopSound(AudioSource audioSource, int id)
    {
        audioSource.Stop();
        audioSource.clip = null;
    }

    private void StopSound(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.clip = null;
    }

    void OnGUI()
    {
        string gear = "";
        if ((LaunchControlMode == LaunchControlMode.LaunchControl || LaunchControlMode == LaunchControlMode.ReadyToGo))
        {
            if (gas > 0)
            {
                if (LaunchControlMode == LaunchControlMode.LaunchControl)
                {
                    gear = "LC";
                } else
                {
                    if (localVelocity.z > -MinSleepVelocity)
                    {
                        gear = (currentGear + 1).ToString();
                    }
                    else
                    {
                        gear = "R";
                    }
                }
            }
            else if (gas <= 0)
            {
                if (localVelocity.z > -MinSleepVelocity)
                {
                    gear = (currentGear + 1).ToString();
                }
                else
                {
                    gear = "R";
                }
            }
        }

        if (LaunchControlMode == LaunchControlMode.Idle)
        {
            gear = "N";
        }

        IndicatorPanelSerie2 ip = IndicatorPanel.GetComponent<IndicatorPanelSerie2>();
        ip.maxHp = (int)MaxEnginePower; //Integer Value
        ip.maxTorque = (int)MaxTorque; //Integer Value
        ip.torque =  (int)GetTork(Mathf.Clamp(soundRpm, 0, 20000), TorqueType.Engine); //Integer from 0 to Max Torque
        ip.hp = (int)GetEngineHp(Mathf.Clamp(soundRpm, 0, 20000)); //Integer from 0 to Max HP
        ip.speed = (int)Mathf.Abs(localVelocity.z * 3.6f); //Integer
        ip.rpm = (int)soundRpm; //Integer
        ip.absEnable = ABS; //Boolean
        ip.absInUse = ABSReleaseActive; //Boolean
        ip.tcsEnable = TractionControl; //Boolean
        ip.tcsInUse = TCSControlActive; //Boolean
        ip.fuelLevel = 50; //int Between 0 - 100
        ip.oilLevel = 10; //int Between 0 - 100
        ip.engineError = true; //Boolean
        ip.gear = gear; //String
        ip.mainLong = MainLightLeft.isActiveAndEnabled; //Boolean
        ip.mainShort = MainLightRight.isActiveAndEnabled; //Boolean
        ip.mainFog = MainLightRight.isActiveAndEnabled; //Boolean
        ip.electiricalError = true; //Boolean
    }
}