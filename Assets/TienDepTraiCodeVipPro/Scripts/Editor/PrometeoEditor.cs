using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrometeoCarController))]
[System.Serializable]
public class PrometeoEditor : Editor
{
    private PrometeoCarController prometeo;
    private SerializedObject SO;

    // CAR SETUP
    private SerializedProperty maxSpeed;
    private SerializedProperty maxReverseSpeed;
    private SerializedProperty accelerationMultiplier;
    private SerializedProperty maxSteeringAngle;
    private SerializedProperty steeringSpeed;
    private SerializedProperty brakeForce;
    private SerializedProperty decelerationMultiplier;
    private SerializedProperty handbrakeDriftMultiplier;
    private SerializedProperty bodyMassCenter;

    // ARCADE VIP
    private SerializedProperty useNitro;
    private SerializedProperty maxNitro;
    private SerializedProperty nitroConsumeRate;
    private SerializedProperty nitroRegenRate;
    private SerializedProperty nitroForce;
    private SerializedProperty useDriftBoost;
    private SerializedProperty driftDurationForBoost;
    private SerializedProperty miniBoostForce;

    // WHEELS
    private SerializedProperty frontLeftMesh;
    private SerializedProperty frontLeftCollider;
    private SerializedProperty frontRightMesh;
    private SerializedProperty frontRightCollider;
    private SerializedProperty rearLeftMesh;
    private SerializedProperty rearLeftCollider;
    private SerializedProperty rearRightMesh;
    private SerializedProperty rearRightCollider;

    // EFFECTS
    private SerializedProperty useEffects;
    private SerializedProperty RLWParticleSystem;
    private SerializedProperty RRWParticleSystem;
    private SerializedProperty RLWTireSkid;
    private SerializedProperty RRWTireSkid;

    // UI
    private SerializedProperty useUI;
    private SerializedProperty carSpeedText;
    private SerializedProperty nitroText;

    // SOUNDS
    private SerializedProperty useSounds;
    private SerializedProperty carEngineSound;
    private SerializedProperty tireScreechSound;

    // TOUCH CONTROLS
    private SerializedProperty useTouchControls;
    private SerializedProperty throttleButton;
    private SerializedProperty reverseButton;
    private SerializedProperty turnRightButton;
    private SerializedProperty turnLeftButton;
    private SerializedProperty handbrakeButton;
    private SerializedProperty nitroButton;

    private void OnEnable()
    {
        prometeo = (PrometeoCarController)target;
        SO = new SerializedObject(target);

        maxSpeed = SO.FindProperty("maxSpeed");
        maxReverseSpeed = SO.FindProperty("maxReverseSpeed");
        accelerationMultiplier = SO.FindProperty("accelerationMultiplier");
        maxSteeringAngle = SO.FindProperty("maxSteeringAngle");
        steeringSpeed = SO.FindProperty("steeringSpeed");
        brakeForce = SO.FindProperty("brakeForce");
        decelerationMultiplier = SO.FindProperty("decelerationMultiplier");
        handbrakeDriftMultiplier = SO.FindProperty("handbrakeDriftMultiplier");
        bodyMassCenter = SO.FindProperty("bodyMassCenter");

        useNitro = SO.FindProperty("useNitro");
        maxNitro = SO.FindProperty("maxNitro");
        nitroConsumeRate = SO.FindProperty("nitroConsumeRate");
        nitroRegenRate = SO.FindProperty("nitroRegenRate");
        nitroForce = SO.FindProperty("nitroForce");
        useDriftBoost = SO.FindProperty("useDriftBoost");
        driftDurationForBoost = SO.FindProperty("driftDurationForBoost");
        miniBoostForce = SO.FindProperty("miniBoostForce");

        frontLeftMesh = SO.FindProperty("frontLeftMesh");
        frontLeftCollider = SO.FindProperty("frontLeftCollider");
        frontRightMesh = SO.FindProperty("frontRightMesh");
        frontRightCollider = SO.FindProperty("frontRightCollider");
        rearLeftMesh = SO.FindProperty("rearLeftMesh");
        rearLeftCollider = SO.FindProperty("rearLeftCollider");
        rearRightMesh = SO.FindProperty("rearRightMesh");
        rearRightCollider = SO.FindProperty("rearRightCollider");

        useEffects = SO.FindProperty("useEffects");
        RLWParticleSystem = SO.FindProperty("RLWParticleSystem");
        RRWParticleSystem = SO.FindProperty("RRWParticleSystem");
        RLWTireSkid = SO.FindProperty("RLWTireSkid");
        RRWTireSkid = SO.FindProperty("RRWTireSkid");

        useUI = SO.FindProperty("useUI");
        carSpeedText = SO.FindProperty("carSpeedText");
        nitroText = SO.FindProperty("nitroText");

        useSounds = SO.FindProperty("useSounds");
        carEngineSound = SO.FindProperty("carEngineSound");
        tireScreechSound = SO.FindProperty("tireScreechSound");

        useTouchControls = SO.FindProperty("useTouchControls");
        throttleButton = SO.FindProperty("throttleButton");
        reverseButton = SO.FindProperty("reverseButton");
        turnRightButton = SO.FindProperty("turnRightButton");
        turnLeftButton = SO.FindProperty("turnLeftButton");
        handbrakeButton = SO.FindProperty("handbrakeButton");
        nitroButton = SO.FindProperty("nitroButton");
    }

    public override void OnInspectorGUI()
    {
        SO.Update();

        GUILayout.Space(20);
        GUILayout.Label("CAR SETUP", EditorStyles.boldLabel);
        
        maxSpeed.intValue = EditorGUILayout.IntSlider("Max Speed:", maxSpeed.intValue, 20, 300);
        maxReverseSpeed.intValue = EditorGUILayout.IntSlider("Max Reverse Speed:", maxReverseSpeed.intValue, 10, 120);
        accelerationMultiplier.intValue = EditorGUILayout.IntSlider("Acceleration Multiplier:", accelerationMultiplier.intValue, 1, 10);
        maxSteeringAngle.intValue = EditorGUILayout.IntSlider("Max Steering Angle:", maxSteeringAngle.intValue, 10, 45);
        steeringSpeed.floatValue = EditorGUILayout.Slider("Steering Speed:", steeringSpeed.floatValue, 0.1f, 1f);
        brakeForce.intValue = EditorGUILayout.IntSlider("Brake Force:", brakeForce.intValue, 100, 600);
        decelerationMultiplier.intValue = EditorGUILayout.IntSlider("Deceleration Multiplier:", decelerationMultiplier.intValue, 1, 10);
        handbrakeDriftMultiplier.intValue = EditorGUILayout.IntSlider("Drift Multiplier:", handbrakeDriftMultiplier.intValue, 1, 10);
        EditorGUILayout.PropertyField(bodyMassCenter, new GUIContent("Mass Center of Car: "));

        GUILayout.Space(20);
        GUILayout.Label("ARCADE FEATURES (VIP)", EditorStyles.boldLabel);
        
        useNitro.boolValue = EditorGUILayout.Toggle("Enable Nitro System", useNitro.boolValue);
        if (useNitro.boolValue)
        {
            EditorGUI.indentLevel++;
            maxNitro.floatValue = EditorGUILayout.FloatField("Max Nitro Capacity:", maxNitro.floatValue);
            nitroConsumeRate.floatValue = EditorGUILayout.FloatField("Burn Rate (/sec):", nitroConsumeRate.floatValue);
            nitroRegenRate.floatValue = EditorGUILayout.FloatField("Regen Rate (/sec):", nitroRegenRate.floatValue);
            nitroForce.floatValue = EditorGUILayout.FloatField("Nitro Force:", nitroForce.floatValue);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(5);
        useDriftBoost.boolValue = EditorGUILayout.Toggle("Enable Drift Mini-Boost", useDriftBoost.boolValue);
        if (useDriftBoost.boolValue)
        {
            EditorGUI.indentLevel++;
            driftDurationForBoost.floatValue = EditorGUILayout.FloatField("Drift Time for Boost (s):", driftDurationForBoost.floatValue);
            miniBoostForce.floatValue = EditorGUILayout.FloatField("Boost Force:", miniBoostForce.floatValue);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(20);
        GUILayout.Label("WHEELS", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(frontLeftMesh, new GUIContent("Front Left Mesh"));
        EditorGUILayout.PropertyField(frontLeftCollider, new GUIContent("Front Left Collider"));
        EditorGUILayout.PropertyField(frontRightMesh, new GUIContent("Front Right Mesh"));
        EditorGUILayout.PropertyField(frontRightCollider, new GUIContent("Front Right Collider"));
        EditorGUILayout.PropertyField(rearLeftMesh, new GUIContent("Rear Left Mesh"));
        EditorGUILayout.PropertyField(rearLeftCollider, new GUIContent("Rear Left Collider"));
        EditorGUILayout.PropertyField(rearRightMesh, new GUIContent("Rear Right Mesh"));
        EditorGUILayout.PropertyField(rearRightCollider, new GUIContent("Rear Right Collider"));

        GUILayout.Space(20);
        GUILayout.Label("EFFECTS", EditorStyles.boldLabel);
        useEffects.boolValue = EditorGUILayout.BeginToggleGroup("Use effects (particle systems)?", useEffects.boolValue);
        EditorGUILayout.PropertyField(RLWParticleSystem, new GUIContent("Rear Left Particle System"));
        EditorGUILayout.PropertyField(RRWParticleSystem, new GUIContent("Rear Right Particle System"));
        EditorGUILayout.PropertyField(RLWTireSkid, new GUIContent("Rear Left Trail Renderer"));
        EditorGUILayout.PropertyField(RRWTireSkid, new GUIContent("Rear Right Trail Renderer"));
        EditorGUILayout.EndToggleGroup();

        GUILayout.Space(20);
        GUILayout.Label("UI", EditorStyles.boldLabel);
        useUI.boolValue = EditorGUILayout.BeginToggleGroup("Use UI (Speed & Nitro)?", useUI.boolValue);
        EditorGUILayout.PropertyField(carSpeedText, new GUIContent("Speed Text (UI)"));
        if(useNitro.boolValue)
            EditorGUILayout.PropertyField(nitroText, new GUIContent("Nitro Text (UI)"));
        EditorGUILayout.EndToggleGroup();

        GUILayout.Space(20);
        GUILayout.Label("SOUNDS", EditorStyles.boldLabel);
        useSounds.boolValue = EditorGUILayout.BeginToggleGroup("Use sounds?", useSounds.boolValue);
        EditorGUILayout.PropertyField(carEngineSound, new GUIContent("Car Engine Sound"));
        EditorGUILayout.PropertyField(tireScreechSound, new GUIContent("Tire Screech Sound"));
        EditorGUILayout.EndToggleGroup();

        GUILayout.Space(20);
        GUILayout.Label("TOUCH CONTROLS", EditorStyles.boldLabel);
        useTouchControls.boolValue = EditorGUILayout.BeginToggleGroup("Use touch controls?", useTouchControls.boolValue);
        EditorGUILayout.PropertyField(throttleButton, new GUIContent("Throttle Button"));
        EditorGUILayout.PropertyField(reverseButton, new GUIContent("Brakes/Reverse Button"));
        EditorGUILayout.PropertyField(turnLeftButton, new GUIContent("Turn Left Button"));
        EditorGUILayout.PropertyField(turnRightButton, new GUIContent("Turn Right Button"));
        EditorGUILayout.PropertyField(handbrakeButton, new GUIContent("Handbrake Button"));
        if(useNitro.boolValue)
            EditorGUILayout.PropertyField(nitroButton, new GUIContent("Nitro Button"));
        EditorGUILayout.EndToggleGroup();

        GUILayout.Space(10);
        SO.ApplyModifiedProperties();
    }
}