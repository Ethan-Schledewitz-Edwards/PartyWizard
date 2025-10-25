using UnityEngine;
using UnityEngine.UI;

public class FirstButton : MonoBehaviour
{
    void OnEnable() => GetComponent<Button>().Select();
}
