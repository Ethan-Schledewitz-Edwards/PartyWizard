using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject basePanel;
    [SerializeField] private GameObject spellPanel;
    [SerializeField] private GameObject itemPanel;

    public List<Enemy> currentEnemies;

    private void Start()
    {
        UI.SetActive(true);

        basePanel.SetActive(true);
        spellPanel.SetActive(false);
        itemPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!basePanel.activeInHierarchy)
            {
                basePanel.SetActive(true);
                spellPanel.SetActive(false);
                itemPanel.SetActive(false);
            }
        }
    }

    #region Base Panel

    public void Attack()
    {
        // TO-DO: Attack
    }

    public void Spell()
    {
        basePanel.SetActive(false);
        spellPanel.SetActive(true);
        itemPanel.SetActive(false);
    }

    public void Guard()
    {
        // TO-DO: Guard
    }

    public void Item()
    {
        basePanel.SetActive(false);
        spellPanel.SetActive(false);
        itemPanel.SetActive(true);
    }

    #endregion
    
    public void Cast(string spell)
    {
        // TO-DO: cast spells
    }

    public void Back()
    {
        basePanel.SetActive(true);
        spellPanel.SetActive(false);
        itemPanel.SetActive(false);
    }
}
