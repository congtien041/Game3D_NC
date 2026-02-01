using System;
using UnityEngine;
using UnityEngine.UIElements;
using static Enums;

[System.Serializable]
public class Sound
{
    public SoundType Type;
    public SubSoundType SubType;
    public AudioClip clip;

    [Range(0f,1f)]
    public float volume;

    [Range(1f, 3f), HideInInspector]
    public float pitch;

    public bool loop = true;
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
public class OtherMaterial
{
    public string name;
    public Material material;
}

[System.Serializable]
public class PhysicalMaterial
{
    public string name;
    public PhysicsMaterial material;
}

[Serializable]
public class TireParticleSytem
{
    public ParticleSystem[] PSs = new ParticleSystem[3];
    [HideInInspector]
    public float[] TimeOfUpdate = new float[3];
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
    [Range(0, 10)]
    public float rollingRessistanceCoEff = 0.5f;
    public bool EnableTireBurnout = true;
}

[Serializable]
public class Axle
{
    private const int wheelCount = 2;
    public GameObject AxelBody;
    public bool isPowered = false;
    public WheelCollider[] wheels = new WheelCollider[wheelCount];
    public TireParticleSytem[] TireParticleSytems = new TireParticleSytem[wheelCount];
    public Transform[] TireMeshes = new Transform[2];

    public float[] ForwardStiffness = new float[wheelCount];
    public float[] SideStiffness = new float[wheelCount];
    public int[] LastIndex = new int[wheelCount];
    public float[] TireTemprature = new float[wheelCount];
    public float[] BrakeTemprature = new float[wheelCount];
    public float[] WheelRpms = new float[wheelCount];
    public float[] SideSpeedDiff = new float[wheelCount];
    public float[] ForwardSpeedDif = new float[wheelCount];
    public float[] SignedForwardSpeedDif = new float[wheelCount];
    public float[] SideSkidIntensitiy = new float[wheelCount];
    public float[] ForwardSkidIntensitiy = new float[wheelCount];
    public float[] SprungMassRate = new float[wheelCount];
    public float[] rollingResistance = new float[wheelCount];
    public float WheelCircumference;

    public void Initialize(Rigidbody rb)
    {
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.linearVelocity);
        short wheelIndex = 0;
        foreach (WheelCollider wc in wheels)
        {
            wc.motorTorque = 0;
            wc.brakeTorque = 0;
            ForwardStiffness[wheelIndex] = wc.forwardFriction.stiffness;
            SideStiffness[wheelIndex] = wc.sidewaysFriction.stiffness;
            LastIndex[wheelIndex] = 0;
            TireTemprature[wheelIndex] = 0;
            BrakeTemprature[wheelIndex] = 0;

            WheelCircumference = 2 * (float)Math.PI * wc.radius;

            //Wheel Rpms
            WheelRpms[wheelIndex] = Mathf.Lerp(WheelRpms[wheelIndex], wc.rpm, 0.2f);
            //---------------------------------------------------------------------------------

            //Side Speed Diff : Meters in Second.
            SideSpeedDiff[wheelIndex] = Mathf.Abs(localVelocity.x);
            //---------------------------------------------------------------------------------

            //Forward Speed Diff : Meters in Second.
            float WRpmBasedRadialSpeed = 2 * Mathf.PI * wc.radius * Mathf.Abs(wc.rpm) / 60;
            ForwardSpeedDif[wheelIndex] = Mathf.Abs(WRpmBasedRadialSpeed - Mathf.Abs(localVelocity.z));
            SignedForwardSpeedDif[wheelIndex] = WRpmBasedRadialSpeed - Mathf.Abs(localVelocity.z);
            //---------------------------------------------------------------------------------

            //Forward Intensity Multiplier
            float ForwardMultiplier = 0;
            float SideMultiplier = 1f;

            if (wc.motorTorque > 10 && wc.brakeTorque <= 0)
                ForwardMultiplier = 1;
            else if (wc.motorTorque <= 0 && wc.brakeTorque > 10)
                ForwardMultiplier = 0.2f;
            else
                ForwardMultiplier = 0;
            //---------------------------------------------------------------------------------

            //Skid Intensity
            ForwardSkidIntensitiy[wheelIndex] = Mathf.Clamp(ForwardSpeedDif[wheelIndex] * ForwardMultiplier * 10 * SprungMassRate[wheelIndex], 0, 15) / 15;
            SideSkidIntensitiy[wheelIndex] = Mathf.Clamp(SideSpeedDiff[wheelIndex] * SideMultiplier * 10 * SprungMassRate[wheelIndex], 0, 30) / 30;
            //---------------------------------------------------------------------------------

            wheelIndex++;
        }
    }

    public void CalculatedynamicData(Rigidbody rb)
    {
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.linearVelocity);
        short wheelIndex = 0;
        foreach (WheelCollider wc in wheels)
        {
            //Wheel Rpms
            WheelRpms[wheelIndex] = Mathf.Lerp(WheelRpms[wheelIndex], wc.rpm, 0.2f);
            //---------------------------------------------------------------------------------

            //Side Speed Diff : Meters in Second.
            SideSpeedDiff[wheelIndex] = Mathf.Abs(localVelocity.x);
            //---------------------------------------------------------------------------------

            //Forward Speed Diff : Meters in Second.
            float WRpmBasedRadialSpeed = 2 * Mathf.PI * wc.radius * Mathf.Abs(wc.rpm) / 60;
            ForwardSpeedDif[wheelIndex] = Mathf.Abs(WRpmBasedRadialSpeed - Mathf.Abs(localVelocity.z));
            SignedForwardSpeedDif[wheelIndex] = WRpmBasedRadialSpeed - Mathf.Abs(localVelocity.z);
            //---------------------------------------------------------------------------------

            //Forward Intensity Multiplier
            float ForwardMultiplier = 0;
            float SideMultiplier = 1f;

            if (wc.motorTorque > 10 && wc.brakeTorque <= 0)
                ForwardMultiplier = 1;
            else if (wc.motorTorque <= 0 && wc.brakeTorque > 10)
                ForwardMultiplier = 0.2f;
            else
                ForwardMultiplier = 0;
            //---------------------------------------------------------------------------------

            //Skid Intensity
            ForwardSkidIntensitiy[wheelIndex] = Mathf.Clamp(ForwardSpeedDif[wheelIndex] * ForwardMultiplier * 10 * SprungMassRate[wheelIndex], 0, 15) / 15;
            SideSkidIntensitiy[wheelIndex] = Mathf.Clamp(SideSpeedDiff[wheelIndex] * SideMultiplier * 10 * SprungMassRate[wheelIndex], 0, 30) / 30;
            //---------------------------------------------------------------------------------

            Vector3 position;
            Quaternion rotation;
            wc.GetWorldPose(out position, out rotation);

            TireMeshes[wheelIndex].transform.position = position;
            TireMeshes[wheelIndex].transform.rotation = rotation;
            wheelIndex++;
        }
    }

    #region Get Functions
    public float GetRollingRessistance(int wheelIndex)
    {
        return rollingResistance[wheelIndex];
    }
    public float GetWheelDistance()
    {
        float distance = Vector3.Distance(wheels[0].transform.position, wheels[1].transform.position);
        return distance;
    }
    public float GetDistanceFromOtherAxle(GameObject from, GameObject to)
    {
        return Vector3.Distance(from.transform.position, to.transform.position);
    }
    public float GetForwardStiffness(int wheelIndex)
    {
        return ForwardStiffness[wheelIndex];
    }
    public float GetSideStiffness(int wheelIndex)
    {
        return SideStiffness[wheelIndex];
    }
    public float GetLastIndex(int wheelIndex)
    {
        return LastIndex[wheelIndex];
    }
    public float GetTireTemprature(int wheelIndex)
    {
        return TireTemprature[wheelIndex];
    }
    public float GetBrakeTemprature(int wheelIndex)
    {
        return BrakeTemprature[wheelIndex];
    }
    public float GetWheelRpms(int wheelIndex)
    {
        return WheelRpms[wheelIndex];
    }
    public float GetSideSpeedDiff(int wheelIndex)
    {
        return SideSpeedDiff[wheelIndex];
    }
    public float GetForwardSpeedDif(int wheelIndex)
    {
        return ForwardSpeedDif[wheelIndex];
    }
    public float GetSignedForwardSpeedDif(int wheelIndex)
    {
        return SignedForwardSpeedDif[wheelIndex];
    }
    public float GetSideSkidIntensitiy(int wheelIndex)
    {
        return SideSkidIntensitiy[wheelIndex];
    }
    public float GetForwardSkidIntensitiy(int wheelIndex)
    {
        return ForwardSkidIntensitiy[wheelIndex];
    }
    public float GetSprungMassRate(int wheelIndex)
    {
        return SprungMassRate[wheelIndex];
    }
    public float GetWheelCircumference()
    {
        return WheelCircumference;
    }
    #endregion

    #region Set Functions
    public void SetRollingResistance(GroundMatrial[] groundMaterials)
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            //if (wheels[i].isGrounded)
            //{
            //    foreach (GroundMatrial groundMaterial in groundMaterials)
            //    {
            //        if (wheels[i].material.name.Contains(groundMaterial.PhysicMaterial.name))
            //        {
            //            rollingResistance[i] = wheels[i].sprungMass * groundMaterial.rollingRessistanceCoEff / wheels[i].radius;
            //        }
            //    }
            //}
        }
    }
    public void SetLastIndex(int wheelIndex, int value)
    {
        LastIndex[wheelIndex] = value;
    }
    public void SetTireTemprature(int wheelIndex, float value)
    {
        TireTemprature[wheelIndex] = value;
    }
    public void SetBrakeTemprature(int wheelIndex, float value)
    {
        BrakeTemprature[wheelIndex] = value;
    }
    #endregion
}

[Serializable]
public class EngineAndGearProperties
{
    public float[] GearRatios = new float[7];
    public const int finalRatio = 0;
    public const int rearRatio = 0;
    [Range(0, 3)]
    public float SteeringSensitivity = 0.7f;
    public float gearShiftTime, cutOffRpm, Throttle;

    //Constant Values
    public float maxEngineRpm;
    public float minEngineRpm;
    public float highGearRpm;
    public float lowGearRpm;

    //Current Values
    public int currentGear;
    public float currentEngineRpm;
    public float currentEngineTorque;
    public float currentWheelTorque;
    public float currentShiftMaxWheelTorque;

    //Max Power Values
    public float maxEnginePower;
    public float maxEngineTorque;
    public float maxTorqueRpm;
    public float maxSpeed;
    public float maxWheelTorque;

    #region Get Functions
    public float GetMaxEngineRpm()
    {
        return maxEngineRpm;
    }
    public float GetMinEngineRpm()
    {
        return minEngineRpm;
    }
    public float GetHighGearRpm()
    {
        return highGearRpm;
    }
    public float GetLowGearRpm()
    {
        return lowGearRpm;
    }
    public int GetCurrentGear()
    {
        return currentGear;
    }
    public float GetCurrentEngineRpm()
    {
        return currentEngineRpm;
    }
    public float GetCurrentEngineTorque()
    {
        return currentEngineTorque;
    }
    public float GetCurrentWheelTorque()
    {
        return currentWheelTorque;
    }
    public float GetCurrentShiftMaxwheelTorque()
    {
        return currentShiftMaxWheelTorque;
    }
    public float GetMaxEnginePower()
    {
        return maxEnginePower;
    }
    public float GetMaxEngineTorque()
    {
        return maxEngineTorque;
    }
    public float GetMaxTorqueRpm()
    {
        return maxTorqueRpm;
    }
    public float GetMaxSpeed()
    {
        return maxSpeed;
    }
    public float GetMaxWheelTorque()
    {
        return maxWheelTorque;
    }
    public float GetCurrentGearRatio()
    {
        if (currentGear > 0)
        {
            return GearRatios[currentGear] * finalRatio;
        }
        else if (currentGear < 0)
        {
            return -rearRatio * finalRatio;
        } else
        {
            return 0;
        }
    }
    public float GetCurrentGearRatio(int currentGear)
    {
        if (currentGear > 0)
        {
            return GearRatios[currentGear] * finalRatio;
        }
        else if (currentGear < 0)
        {
            return -rearRatio * finalRatio;
        }
        else
        {
            return 0;
        }
    }
    #endregion


    #region Set Functions
    public void SetMaxEngineRpm(float value)
    {
        maxEngineRpm = value;
    }
    public void SetMinEngineRpm(float value)
    {
        minEngineRpm = value;
    }
    public void SetHighGearRpm(float value)
    {
        highGearRpm = value;
    }
    public void SetLowGearRpm(float value)
    {
        lowGearRpm = value;
    }
    public void SetCurrentGear(int value)
    {
        currentGear = value;
    }
    public void SetCurrentEngineRpm(float value)
    {
        currentEngineRpm = value;
    }
    public void SetCurrentEngineTorque(float value)
    {
        currentEngineTorque = value;
    }
    public void SetCurrentWheelTorque(float value)
    {
        currentWheelTorque = value;
    }
    public void SetCurrentShiftMaxwheelTorque(float value)
    {
        currentShiftMaxWheelTorque = value;
    }
    public void SetMaxEnginePower(float value)
    {
        maxEnginePower = value;
    }
    public void SetMaxEngineTorque(float value)
    {
        maxEngineTorque = value;
    }
    public void SetMaxTorqueRpm(float value)
    {
        maxTorqueRpm = value;
    }
    public void SetMaxSpeed(float value)
    {
        maxSpeed = value;
    }
    public void SetMaxWheelTorque(float value)
    {
        maxWheelTorque = value;
    }
    #endregion

}

[Serializable]
public class CarData
{
    private const int AxleCount = 2;
    public Axle[] Axles = new Axle[AxleCount];

    public EngineAndGearProperties EGP = new EngineAndGearProperties();

    //Resistance Values
    [Range(0, 1)]
    public float rollingResistanceCoEff = 0.01f;
    [Range(0, 1)]
    public float airFrictionCoEff = 0.23f;
    [Range(0, 1)]
    public float downForceCoEff = 0.8f;
    [Range(0, 10)]
    public float crossSection = 1f;
    [Range(0, 5)]
    public float airDensity = 1.222f;
    [Range(0, 1)]
    public float spoilerWingArea = 0.2f;
    //---------------------------------------------------------------------------------

    //Car Properties
    [Range(0, 1), Tooltip("Percent Of Max Engine Rpm")]
    public float highGearRpmRate = 0.95f;
    [Range(0, 1), Tooltip("Percent Of Min Engine Rpm")]
    public float lowGearRpmRate = 0.65f;
    [Range(-1000, 1000)]
    public float MinEngineRpmOffsett = 0f;
    [Range(-1000, 1000)]
    public float MaxEngineRpmOffsett = 0f;
    //---------------------------------------------------------------------------------

    //Resistances
    public float innerResistance, airFriction, downForce, totalRollingResistance, totalResistance;
    //---------------------------------------------------------------------------------

    //Current Car, Engine and Wheel Properties
    public float biggestWheelCircumference;
    public float forwardSpeed;
    public float currentShiftMaxSpeed;
    public float currentShiftMinSpeed;
    public float velocityBasedWheelRpm;
    public float velocityBasedEngineRpm;
    public float currentShiftMaxWheelRpm;
    public float currentShiftMinWheelRpm;
    public int poweredAxlesCount;
    public Vector3 localVelocity;
    //---------------------------------------------------------------------------------

    public void initialize(Rigidbody rb, WavPart[] EngineAudioSources)
    {
        localVelocity = rb.transform.InverseTransformDirection(rb.linearVelocity);

        innerResistance = 0;
        airFriction = 0;
        downForce = 0;
        totalResistance = 0;
        totalRollingResistance = 0;

        biggestWheelCircumference = 0;
        poweredAxlesCount = 0;
        foreach (Axle axle in Axles)
        {
            axle.Initialize(rb);
            if (axle.GetWheelCircumference() > biggestWheelCircumference)
                biggestWheelCircumference = axle.GetWheelCircumference();

            if (axle.isPowered)
                poweredAxlesCount++;
        }

        forwardSpeed = 0;
        currentShiftMaxSpeed = 0;
        currentShiftMinSpeed = 0;
        velocityBasedWheelRpm = 0;
        velocityBasedEngineRpm = 0;
        currentShiftMaxWheelRpm = 0;
        currentShiftMinWheelRpm = 0;
        EGP.SetCurrentGear(0);

        EGP.SetMinEngineRpm(EngineAudioSources[0].centerRPM + MinEngineRpmOffsett);
        EGP.SetMaxEngineRpm(EngineAudioSources[EngineAudioSources.Length - 1].centerRPM + MaxEngineRpmOffsett);
        EGP.SetHighGearRpm(EGP.GetMaxEngineRpm() * highGearRpmRate);
        EGP.SetLowGearRpm(EGP.GetMaxEngineRpm() * lowGearRpmRate);
        EGP.SetCurrentEngineRpm(EGP.GetMinEngineRpm());
    }

    public void CalculateDynamicData(Rigidbody rb, GroundMatrial[] groundMaterials)
    {
        localVelocity = rb.transform.InverseTransformDirection(rb.linearVelocity);

        #region Resistance Calculation 
        innerResistance = rollingResistanceCoEff * EGP.GetCurrentEngineRpm();
        airFriction = airDensity * (airFrictionCoEff + downForceCoEff) * crossSection * localVelocity.magnitude * localVelocity.magnitude / 2;
        downForce = airDensity * localVelocity.z * localVelocity.z * spoilerWingArea * downForceCoEff / 2;
        #endregion

        totalRollingResistance = 0;
        for (int axleIndex = 0; axleIndex < Axles.Length; axleIndex++)
        {
            Axle axle = Axles[axleIndex];
            axle.CalculatedynamicData(rb);
            axle.SetRollingResistance(groundMaterials);

            for (int wheelIndex = 0; wheelIndex < axle.wheels.Length; wheelIndex++)
            {
                totalRollingResistance += axle.GetRollingRessistance(wheelIndex);
            }
        }

        totalResistance = innerResistance + airDensity;

        #region MyRegion
        forwardSpeed = localVelocity.z;

        if (EGP.GetCurrentGear() != 0)
        {
            currentShiftMaxSpeed = biggestWheelCircumference * (EGP.GetMaxEngineRpm() / 60) / EGP.GetCurrentGearRatio(EGP.GetCurrentGear());
            currentShiftMinSpeed = biggestWheelCircumference * (EGP.GetMinEngineRpm() / 60) / EGP.GetCurrentGearRatio(EGP.GetCurrentGear());
            velocityBasedWheelRpm = Math.Abs(60 * forwardSpeed / 2.08f);
            velocityBasedEngineRpm = velocityBasedWheelRpm * EGP.GetCurrentGearRatio(EGP.GetCurrentGear());
            currentShiftMaxWheelRpm = EGP.GetMaxEngineRpm() / EGP.GetCurrentGearRatio(EGP.GetCurrentGear());
            currentShiftMinWheelRpm = EGP.GetMinEngineRpm() / EGP.GetCurrentGearRatio(EGP.GetCurrentGear());
        }
        else
        {
            currentShiftMaxSpeed = 0;
            currentShiftMinSpeed = 0;
            velocityBasedWheelRpm = Math.Abs(60 * forwardSpeed / 2.08f);
            velocityBasedEngineRpm = 0;
            currentShiftMaxWheelRpm = 0;
            currentShiftMinWheelRpm = 0;
        }
        
        #endregion
    }

    #region Get Functions
    public Vector3 GetLocalVelocity()
    {
        return localVelocity;
    }
    public float GetDownForce()
    {
        return downForce;
    }
    public float GetTotalRessistance()
    {
        return totalResistance;
    }
    public int GetPoweredWheelCount()
    {
        return poweredAxlesCount * 2;
    }
    #endregion

    #region Set Functions
    
    #endregion
}
