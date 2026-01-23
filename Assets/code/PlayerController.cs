using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float speed = 7f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [System.Obsolete]
    void Update() {
        // Di chuyển
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        transform.position += new Vector3(x, 0, z) * speed * Time.deltaTime;

        // Quay mặt theo chuột
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            Vector3 lookDir = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            transform.LookAt(lookDir);
        }

        // Bắn
        if (Input.GetMouseButtonDown(0)) {
            GameObject b = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            b.GetComponent<Rigidbody>().velocity = firePoint.forward * 20f;
            Destroy(b, 2f);
        }
    }
}