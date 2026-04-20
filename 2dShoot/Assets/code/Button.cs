using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    public int SN = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(SN);
        }
    }
    public void ChangeS(int sn)
    {
        SceneManager.LoadScene(sn);
    }
}