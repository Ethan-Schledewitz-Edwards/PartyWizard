using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TextScroll : MonoBehaviour
{
    [SerializeField] private TMP_Text textLabel;

    [SerializeField] private string[] text;
    [SerializeField] private float textSpeed;

    private readonly int currentLine = 0;

    private void Start()
    {
        textLabel.SetText("");
    }

    public void PlayText()
    {
        StartCoroutine(ScrollText(currentLine));
    }

    private IEnumerator ScrollText(int line)
    {
        for (int i = 0; i < text[line].Length; i++)
        {
            textLabel.SetText(text[line][..i]);

            yield return new WaitForSeconds(textSpeed);
        }
    }
}
