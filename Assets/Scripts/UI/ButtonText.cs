﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ButtonText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Button button;
    public TMPro.TMP_Text buttonText;
    public Color normalCol = Color.white;
    public Color nonInteractableCol = Color.grey;
    public Color highlightedCol = Color.white;
    bool highlighted;

    new string name;

    public ChipDelete _chipDelete;

    void Start()
    {
        var manager = GameObject.FindWithTag("Manager");
        if (manager != null)
        {
            _chipDelete = manager.GetComponent<ChipDelete>();
        }
        name = gameObject.name;
    }

    void Update()
    {
        Color col = (highlighted) ? highlightedCol : normalCol;
        buttonText.color = (button.interactable) ? col : nonInteractableCol;

        if (Input.GetKeyDown(KeyCode.Delete) && highlighted && _chipDelete != null)
        {
            _chipDelete.ConfirmDelete(name);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
        {
            highlighted = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlighted = false;
    }

    void OnEnable()
    {
        highlighted = false;
    }

    void OnValidate()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        if (buttonText == null)
        {
            buttonText = transform.GetComponentInChildren<TMPro.TMP_Text>();
        }
    }
}