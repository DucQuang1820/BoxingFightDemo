using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
    public void OneVOne()
    {
        SceneManager.LoadScene("1vs1");
    }
    public void OneVMany()
    {
        SceneManager.LoadScene("1vsMany");
    }
    public void ManyVMany()
    {
        SceneManager.LoadScene("ManyVMany");
    }
}
