using UnityEngine;

public class wallPlayer : MonoBehaviour

{
    public float speed = 5f;
    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * speed * Time.deltaTime;
        transform.position += move;
        float h = Input.GetAxis("Horizontal");
        if (h > 0) transform.eulerAngles = new Vector3(0, 0, 0);
        if (h < 0) transform.eulerAngles = new Vector3(0, -180, 0);
    }
}