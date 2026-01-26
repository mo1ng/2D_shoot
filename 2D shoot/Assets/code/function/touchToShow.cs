using UnityEngine;

public class touchToShow : MonoBehaviour

{
    public GameObject eObject;

    void Start() => eObject?.SetActive(false);
    void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) eObject?.SetActive(true); }
    void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) eObject?.SetActive(false); }
}