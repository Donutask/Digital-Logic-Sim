using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadProjectMenu : MonoBehaviour
{
    [SerializeField] Button projectButtonPrefab;
    [SerializeField] Transform scrollHolder;
    [SerializeField] GameObject noProjectsIndicator;

    [SerializeField, HideInInspector]
    List<Button> loadButtons;
    void OnEnable()
    {
        string[] projectNames = SaveSystem.GetSaveNames();

        //show a text saying that there are no projects
        if (projectNames.Length <= 0)
        {
            noProjectsIndicator.SetActive(true);
            return;
        }
        else
            noProjectsIndicator.SetActive(false);


        for (int i = 0; i < projectNames.Length; i++)
        {
            string projectName = projectNames[i];
            if (i >= loadButtons.Count)
            {
                loadButtons.Add(Instantiate(projectButtonPrefab, parent: scrollHolder));
            }
            Button loadButton = loadButtons[i];
            loadButton.GetComponent<ProjectButton>().name = projectName.Trim();
            loadButton.GetComponentInChildren<TMPro.TMP_Text>().text = projectName.Trim();
            loadButton.onClick.AddListener(() => LoadProject(projectName));
        }
    }

    public void LoadProject(string projectName)
    {
        SaveSystem.SetActiveProject(projectName);
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

}