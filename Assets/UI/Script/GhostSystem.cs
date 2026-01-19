using UnityEngine;
using System.Collections.Generic;

// Cấu trúc dữ liệu để lưu Input từng khung hình
[System.Serializable]
public struct GhostFrame
{
    public float throttle;   // Ga/Phanh (-1 đến 1)
    public float steering;   // Lái (-1 đến 1)
    public bool handbrake;   // Phanh tay
}

public class GhostSystem : MonoBehaviour
{
    public enum Mode { Recorder, Replayer }
    
    [Header("CHẾ ĐỘ")]
    public Mode currentMode = Mode.Recorder;

    [Header("CẤU HÌNH")]
    public PrometeoNPCController carController;
    
    // Dữ liệu tĩnh để chia sẻ giữa xe Player và xe AI
    // Khi Player ghi xong, dữ liệu sẽ nằm ở đây để AI lấy dùng
    public static List<GhostFrame> recordedClip = new List<GhostFrame>();

    // Biến nội bộ
    private bool isRecording = false;
    private bool isReplaying = false;
    private int currentFrameIndex = 0;

    void Start()
    {
        if (carController == null) 
            carController = GetComponent<PrometeoNPCController>();

        if (currentMode == Mode.Recorder)
        {
            // Bắt đầu ghi
            recordedClip.Clear();
            isRecording = true;
            Debug.Log("ĐANG GHI HÌNH TRÌNH CỦA BẠN...");
        }
        else if (currentMode == Mode.Replayer)
        {
            // Bắt đầu phát lại
            if (recordedClip.Count > 0)
            {
                isReplaying = true;
                currentFrameIndex = 0;
                // Tắt vật lý va chạm của xe AI để nó không bị lệch khi đụng Player (tùy chọn)
                // GetComponent<Rigidbody>().isKinematic = true; 
                Debug.Log("AI ĐANG CHẠY LẠI DỮ LIỆU CỦA BẠN!");
            }
            else
            {
                Debug.LogError("Chưa có dữ liệu ghi âm! Hãy chạy xe Player trước.");
            }
        }
    }

    // Dùng FixedUpdate để đồng bộ với vật lý (50 lần/giây)
    void FixedUpdate()
    {
        if (currentMode == Mode.Recorder && isRecording)
        {
            RecordFrame();
        }
        else if (currentMode == Mode.Replayer && isReplaying)
        {
            ReplayFrame();
        }
    }

    void RecordFrame()
    {
        GhostFrame frame = new GhostFrame();
        
        // Lấy Input từ bàn phím (WSAD / Mũi tên)
        frame.throttle = Input.GetAxis("Vertical");   // W/S
        frame.steering = Input.GetAxis("Horizontal"); // A/D
        frame.handbrake = Input.GetKey(KeyCode.Space);

        // Lưu vào danh sách
        recordedClip.Add(frame);
    }

    void ReplayFrame()
    {
        if (currentFrameIndex < recordedClip.Count)
        {
            // Lấy dữ liệu từ bộ nhớ
            GhostFrame frame = recordedClip[currentFrameIndex];

            // Nạp dữ liệu vào xe AI thông qua hàm ta vừa viết thêm
            carController.SetRawInput(frame.throttle, frame.steering, frame.handbrake);

            currentFrameIndex++;
        }
        else
        {
            // Hết băng ghi âm -> Dừng xe
            carController.SetRawInput(0, 0, true);
            isReplaying = false;
            Debug.Log("KẾT THÚC REPLAY");
        }
    }
}