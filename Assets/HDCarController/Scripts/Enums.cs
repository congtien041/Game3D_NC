
public class Enums
{
    public enum TurnDirection
    {
        cw, //Clock Wise
        ccw //Counter Clock Wise
    }
    public enum Axis
    {
        x,
        y,
        z
    }

    public enum FrictionType
    {
        Side,
        Forward
    }

    public enum TorqueType
    {
        Engine,
        Wheel
    }

    public enum CalledFunction
    {
        Update,
        FixedUpdate,
        LateUpdate
    }

    public enum CarDirection
    {
        Forward,
        Backward,
        None
    }

    public enum LaunchControlMode
    {
        Idle,
        LaunchControl,
        ReadyToGo

    }

    public enum GearBoxControlMode
    {
        Auto,
        Manual
    }

    public enum ClutchControlMode
    {
        Auto,
        Manual
    }

    public enum ShiftDirection
    {
        Up,
        Down,
        Empty
    }

    public enum TurnSignal
    {
        Off,
        Left,
        Right,
        All
    }

    public enum SoundType
    {
        None,
        ExhaustPat,
        TurboCoolDown,
        ShiftUp,
        ShiftDown,
        HardCrash,
        SoftCrash,
        Scratch
    }

    public enum SubSoundType
    {
        None,
        Asphalt,
        Stone,
        Wood,
        Metal,
        Terrain,
        Foliage
    }

    public enum SpeedType
    {
        Km,
        Mile
    }
}
