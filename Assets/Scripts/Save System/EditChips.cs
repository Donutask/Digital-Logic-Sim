using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine;
using SFB;

public class EditChips : MonoBehaviour
{
    [Header("References")]
    public Transform implementationHolder;
    public Manager manager;
    public ChipSignal InputSignalPrefab;
    public ChipSignal OutputSignalPrefab;
    public Wire wirePrefab;
    GameObject InputBar;
    GameObject OutputBar;

    Vector2 chipPos;

    public void OpenFileBrowser()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Load Chip Save File", SaveSystem.GlobalDirectoryPath, "txt", false);
        if (paths.Length == 1)
            DisplayChips(paths[0]);
    }


    void DisplayChips(string chipPath)
    {
        ChipInteraction chipInteraction = GameObject.Find("Interaction").transform.Find("Chip Interaction").gameObject.GetComponent<ChipInteraction>();
        SavedChip savedChip;
        Chip loadingChip;
        List<Chip> loadedChips = new List<Chip>();

        using (StreamReader reader = new StreamReader(chipPath))
        {
            string chipSaveString = reader.ReadToEnd();
            savedChip = JsonUtility.FromJson<SavedChip>(chipSaveString);
        }

        string originalChipName = savedChip.name;
        InputBar = GameObject.Find("Input Bar");
        OutputBar = GameObject.Find("Output Bar");

        foreach (SavedComponentChip componentChip in savedChip.savedComponentChips)
        {
            string chipName = componentChip.chipName;
            if ((chipName != "SIGNAL IN") && (chipName != "SIGNAL OUT"))
            {
                if (IsBuiltInChipName(chipName))
                {
                    for (int i = 0; i < manager.builtinChips.Length; i++)
                    {
                        //skip null chips
                        if (manager.builtinChips[i] == null)
                        {
                            continue;
                        }

                        if (chipName == manager.builtinChips[i].name)
                        {
                            chipPos = new Vector2((float)componentChip.posX, (float)componentChip.posY);
                            loadingChip = (GameObject.Instantiate(manager.builtinChips[i], chipPos, Quaternion.identity, implementationHolder).GetComponent<Chip>());
                            loadedChips.Add(loadingChip);
                            chipInteraction.allChips.Add(loadingChip);
                        }
                    }
                }
                else
                {
                    List<GameObject> inactiveGameObjects = new List<GameObject>();

                    foreach (GameObject gameObject in FindInActiveObjectsByName(chipName))
                    {
                        if (!gameObject.activeSelf)
                        {
                            inactiveGameObjects.Add(gameObject);
                            gameObject.SetActive(true);
                        }
                    }

                    GameObject original = GameObject.Find("/Manager/" + chipName);

                    chipPos = new Vector2((float)componentChip.posX, (float)componentChip.posY);
                    loadingChip = (GameObject.Instantiate(original, chipPos, Quaternion.identity, implementationHolder).GetComponent<Chip>());
                    loadedChips.Add(loadingChip);
                    chipInteraction.allChips.Add(loadingChip);

                    foreach (GameObject gameObject in inactiveGameObjects)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
            else
            {

                if (chipName == "SIGNAL IN")
                {
                    chipPos = new Vector2((float)componentChip.posX, (float)componentChip.posY);
                    ChipSignal spawnedSignal = Instantiate(InputSignalPrefab, chipPos, Quaternion.identity, implementationHolder.Find("Inputs"));
                    loadedChips.Add(spawnedSignal.GetComponent<Chip>());
                    InputBar.GetComponent<ChipInterfaceEditor>().signals.Add(spawnedSignal);
                    spawnedSignal.side = ChipSignal.Side.Left;
                }
                else if (chipName == "SIGNAL OUT")
                {
                    chipPos = new Vector2((float)componentChip.posX, (float)componentChip.posY);
                    ChipSignal spawnedSignal = Instantiate(OutputSignalPrefab, chipPos, Quaternion.identity, implementationHolder.Find("Inputs"));
                    loadedChips.Add(spawnedSignal.GetComponent<Chip>());
                    OutputBar.GetComponent<ChipInterfaceEditor>().signals.Add(spawnedSignal);
                    spawnedSignal.side = ChipSignal.Side.Right;
                }
            }
        }

        Dictionary<SavedWire, int[]> savedWires = new Dictionary<SavedWire, int[]>();

        string wirePath = SaveSystem.GetPathToWireSaveFile(originalChipName);

        //only add wires if there are any
        if (File.Exists(wirePath))
        {
            SavedWireLayout savedWireLayout;

            using (StreamReader reader = new StreamReader(wirePath))
            {
                string wireSaveString = reader.ReadToEnd();
                savedWireLayout = JsonUtility.FromJson<SavedWireLayout>(wireSaveString);
            }


            foreach (SavedWire savedWire in savedWireLayout.serializableWires)
            {
                savedWires.Add(savedWire, new int[] { savedWire.parentChipIndex, savedWire.parentChipOutputIndex });
            }
        }

        //Code from ChipLoader.cs arranged to work here
        for (int chipIndex = 0; chipIndex < savedChip.savedComponentChips.Length; chipIndex++)
        {

            loadedChips.ToArray();
            Chip loadedComponentChip = loadedChips[chipIndex];
            for (int inputPinIndex = 0; inputPinIndex < loadedComponentChip.inputPins.Length; inputPinIndex++)
            {
                SavedInputPin savedPin = savedChip.savedComponentChips[chipIndex].inputPins[inputPinIndex];
                Pin pin = loadedComponentChip.inputPins[inputPinIndex];

                // If this pin should receive input from somewhere, then wire it up to that pin
                if (savedPin.parentChipIndex != -1)
                {
                    Pin connectedPin = loadedChips[savedPin.parentChipIndex].outputPins[savedPin.parentChipOutputIndex];
                    pin.cyclic = savedPin.isCylic;
                    Pin.TryConnect(connectedPin, pin);

                    if (Pin.TryConnect(connectedPin, pin))
                    {
                        Wire loadedWire = GameObject.Instantiate(wirePrefab, parent: implementationHolder.Find("Wires"));
                        loadedWire.Connect(connectedPin, loadedComponentChip.inputPins[inputPinIndex]);

                        /*foreach (SavedWire savedWire in savedWires.Keys)                      DOESN'T WORK FOR NOW
                        {
                            if (savedWires[savedWire][0] == chipIndex && savedWires[savedWire][1] == inputPinIndex)
                            {
                                foreach (Vector2 anchorPoint in savedWire.anchorPoints)
                                {
                                    if (anchorPoint.x != -7.243164539337158 && anchorPoint.x != 7.243164539337158)
                                    {
                                        loadedWire.AddAnchorPoint(anchorPoint);
                                    }
                                }
                            }
                        }*/
                    }
                }
            }
        }
    }

    //Piece of code I found on stackoverflow by https://stackoverflow.com/users/3785314/programmer
    GameObject[] FindInActiveObjectsByName(string name)
    {
        List<GameObject> validTransforms = new List<GameObject>();
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].gameObject.name == name)
                {
                    validTransforms.Add(objs[i].gameObject);
                }
            }
        }
        return validTransforms.ToArray();
    }

    /// <summary>
    /// True if chipName is in the Manager.builtinchips list
    /// </summary>
    bool IsBuiltInChipName(string chipName)
    {
        foreach (var item in manager.builtinChips)
        {
            if (item == null)
            {
                continue;
            }
            if (chipName == item.chipName)
            {
                return true;
            }
        }
        return false;
    }
}