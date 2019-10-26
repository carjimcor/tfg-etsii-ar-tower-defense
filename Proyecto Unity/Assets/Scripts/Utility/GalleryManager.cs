using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GalleryManager : MonoBehaviour
{

    public GameObject prefabs;
    public Vector3 position;
    public Text text;

    int currentPrefabsIdx = 0;
    int showPrefabIdx = 0;
    List<Transform> parentPrefabs = new List<Transform>();
    List<Transform> currentPrefabs = new List<Transform>();

    Transform showedObject = default;

    void Start()
    {
        Init();
    }

    void Init()
    {
        InitParentPrefabs();
        NextGallery(0);
    }

    void InitParentPrefabs()
    {
        for (int i = 0; i < prefabs.transform.childCount; i++)
        {
            Transform current = prefabs.transform.GetChild(i);
            parentPrefabs.Add(current);
        }
    }

    public void NextGallery(int increment)
    {
        currentPrefabsIdx += increment;
        if (currentPrefabsIdx >= parentPrefabs.Count)
            currentPrefabsIdx = 0;

        if (currentPrefabsIdx < 0)
            currentPrefabsIdx = parentPrefabs.Count - 1;

        showPrefabIdx = 0;
        ResetCurrentPrefabs();

        ShowNextObject(0);
    }

    void ResetCurrentPrefabs()
    {
        Transform currentPrefabParent = parentPrefabs[currentPrefabsIdx].transform;
        currentPrefabs.Clear();

        text.text = currentPrefabParent.name;

        for (int i = 0; i < currentPrefabParent.childCount; i++)
        {
            Transform aux = currentPrefabParent.GetChild(i);
            currentPrefabs.Add(aux);
        }
    }

    public void ShowNextObject(int increment)
    {
        showPrefabIdx += increment;
        if (showPrefabIdx >= currentPrefabs.Count)
            showPrefabIdx = 0;
        if (showPrefabIdx < 0)
            showPrefabIdx = currentPrefabs.Count - 1;

        if (showedObject != null)
            showedObject.gameObject.SetActive(false);

        showedObject = currentPrefabs[showPrefabIdx];
        showedObject.position = position;
        showedObject.gameObject.SetActive(true);
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
}
