using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zoom : MonoBehaviour
{
    [SerializeField] float scaleChange;
    public Vector3 scale { get; private set; }
    [SerializeField] float min;
    [SerializeField] float max;
    float interactionNum = 0.25f;
    public GameObject[] objectsToZoom;
    public ChipInteraction _chipInteraction;
    [SerializeField] bool useScrollwheel;

    private void Start()
    {
        _chipInteraction = GameObject.FindWithTag("Interaction").GetComponent<ChipInteraction>();
        scale = new Vector3(1, 1, 1);
    }

    void Update()
    {
        float zoom = scale.x;
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown("="))
        {
            zoom += scaleChange;
            interactionNum += 0.1f;
        }

        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown("-"))
        {
            zoom -= scaleChange;
            interactionNum -= 0.1f;
        }

        //scroll with the scroll wheel
        if (useScrollwheel)
        {
            zoom += scaleChange * Input.mouseScrollDelta.y;
        }

        zoom = Mathf.Clamp(zoom, min, max);
        var newScale = new Vector3(zoom, zoom, zoom);

        //only update zoom objects if zoom has changed
        if (newScale != scale)
        {
            scale = newScale;

            objectsToZoom = GameObject.FindGameObjectsWithTag("Zoom");
            _chipInteraction.selectionBoundsBorderPadding = interactionNum;
            for (int i = 0; i < objectsToZoom.Length; i++)
            {
                objectsToZoom[i].transform.localScale = scale;
            }
        }
    }
}
