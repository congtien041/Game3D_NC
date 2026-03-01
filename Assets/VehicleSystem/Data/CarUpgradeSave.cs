namespace VehicleSystem.Data
{
    [System.Serializable]
    public class CarUpgradeSave
    {
        public string carID;
        public bool isUnlocked;
        
        // Lưu lại 4 cột tương ứng trong ảnh
        public int topSpeedLevel = 0;
        public int accelerationLevel = 0;
        public int handlingLevel = 0;
        public int nitroLevel = 0;
    }
}