using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Collections;
using TMPro;

public static class SaveRuntime
{
    // Lightweight place to stash state after selecting/creating a save.
    public static int CurrentSlotIndex = -1;
    public static SaveSlotsController.SaveMeta CurrentMeta;
    public static string CurrentMetaJson;
}

public class SaveSlotsController : MonoBehaviour
{
    [Header("Slots (left → right)")]
    public Button[] slotButtons;           // FILE 1 / FILE 2 / FILE 3
    public TMP_Text[] slotLabels;          // optional; purely cosmetic
    public Button deleteButton;            // DELETE FILE

    [Header("Routing")]
    [Tooltip("Panel to show after creating a NEW save (e.g., Character Select).")]
    public GameObject newSaveTargetPanel;

    [Tooltip("Panel to show after LOADING an EXISTING save (e.g., Continue/Load panel or straight to Gameplay Setup).")]
    public GameObject existingSaveTargetPanel;

    [Tooltip("Your menu router component (the one with FadeTo(GameObject) or similar).")]
    public MonoBehaviour menuRouter;

    [Tooltip("Router method name that accepts a single GameObject (default: FadeTo).")]
    public string routerMethod = "FadeTo";

    [Header("Timings")]
    public float routeDelay = 0.05f;       // tiny delay before routing (lets UI click SFX finish)

    [Header("Save Storage")]
    public string filePrefix = "save_slot_";
    public string fileExt = ".json";

    int selectedIndex = -1;

    // ---------- lifecycle ----------
    void OnEnable()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int ix = i;
            slotButtons[i].onClick.RemoveAllListeners();
            slotButtons[i].onClick.AddListener(() => OnClickSlot(ix));
        }

        if (deleteButton)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteSelected);
        }

        RefreshAll();
        AutoSelectFirst();
    }

    // ---------- UI / state ----------
    void RefreshAll()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            slotButtons[i].interactable = true;

            // optional label styling
            if (slotLabels != null && i < slotLabels.Length && slotLabels[i])
            {
                slotLabels[i].text = $"FILE {i+1}";
                slotLabels[i].alpha = 1f;
            }

            // keep keyboard/gamepad nav alive
            var nav = slotButtons[i].navigation;
            nav.mode = Navigation.Mode.Automatic;
            slotButtons[i].navigation = nav;
        }

        deleteButton.interactable = (selectedIndex >= 0) && HasSave(selectedIndex);
    }

    void AutoSelectFirst()
    {
        if (selectedIndex < 0) selectedIndex = 0;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, slotButtons.Length - 1);
        SetSelected(slotButtons[selectedIndex].gameObject);
        deleteButton.interactable = HasSave(selectedIndex);
    }

    void SetSelected(GameObject go)
    {
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(go);
    }

    // ---------- interactions ----------
    public void OnClickSlot(int index)
    {
        if (index < 0 || index >= slotButtons.Length) return;

        selectedIndex = index;
        SetSelected(slotButtons[index].gameObject);
        deleteButton.interactable = HasSave(index);

        if (HasSave(index))
        {
            // LOAD existing save and route to EXISTING panel
            var meta = LoadSave(index);
            if (meta == null)
            {
                Debug.LogWarning($"[SaveSlots] Slot {index+1} had a file but failed to load. Treating as empty.");
                CreateNewSaveAndGo(index);
                return;
            }

            SaveRuntime.CurrentSlotIndex = index;
            SaveRuntime.CurrentMeta = meta;
            SaveRuntime.CurrentMetaJson = ReadAllTextSafe(PathFor(index));

            StartCoroutine(RouteTo(existingSaveTargetPanel));
        }
        else
        {
            // EMPTY → create and route to NEW panel
            CreateNewSaveAndGo(index);
        }
    }

    void CreateNewSaveAndGo(int index)
    {
        var meta = CreateOrOverwriteSave(index);
        SaveRuntime.CurrentSlotIndex = index;
        SaveRuntime.CurrentMeta = meta;
        SaveRuntime.CurrentMetaJson = ReadAllTextSafe(PathFor(index));
        StartCoroutine(RouteTo(newSaveTargetPanel));
    }

    public void OnDeleteSelected()
    {
        if (selectedIndex < 0) return;
        if (!HasSave(selectedIndex)) return;

        try
        {
            string path = PathFor(selectedIndex);
            if (File.Exists(path)) File.Delete(path);
            Debug.Log($"[SaveSlots] Deleted slot {selectedIndex+1}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSlots] Delete failed: {e}");
        }

        RefreshAll();
        // keep focus on same slot
        SetSelected(slotButtons[selectedIndex].gameObject);
        deleteButton.interactable = HasSave(selectedIndex);
    }

    IEnumerator RouteTo(GameObject targetPanel)
    {
        yield return new WaitForSecondsRealtime(routeDelay);

        if (targetPanel == null)
        {
            Debug.LogWarning("[SaveSlots] Target panel not assigned.");
            yield break;
        }

        if (menuRouter != null && !string.IsNullOrEmpty(routerMethod))
        {
            var m = menuRouter.GetType().GetMethod(routerMethod);
            if (m != null)
            {
                m.Invoke(menuRouter, new object[] { targetPanel });
                yield break;
            }
        }

        // Fallback: manual toggle
        targetPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    // ---------- save I/O ----------
    [Serializable]
    public class SaveMeta
    {
        public int slotIndex;
        public string createdUtc;
        public int version = 1;

        // Add anything you want to reference on next panel,
        // e.g., lastLevel, difficulty, characterId, etc.
    }

    SaveMeta CreateOrOverwriteSave(int index)
    {
        var meta = new SaveMeta
        {
            slotIndex = index,
            createdUtc = DateTime.UtcNow.ToString("o"),
            version = 1
        };

        string json = JsonUtility.ToJson(meta, true);
        WriteAllTextSafe(PathFor(index), json);
        Debug.Log($"[SaveSlots] Created/Overwrote slot {index+1}\n{json}");

        RefreshAll();
        return meta;
    }

    SaveMeta LoadSave(int index)
    {
        try
        {
            string path = PathFor(index);
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            var meta = JsonUtility.FromJson<SaveMeta>(json);
            if (meta == null) return null;

            // defensive: ensure slot matches file name
            meta.slotIndex = index;
            Debug.Log($"[SaveSlots] Loaded slot {index+1}\n{json}");
            return meta;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSlots] Load failed for slot {index+1}: {e}");
            return null;
        }
    }

    bool HasSave(int index) => File.Exists(PathFor(index));

    string PathFor(int index)
    {
        string file = $"{filePrefix}{index+1}{fileExt}";
        return Path.Combine(Application.persistentDataPath, file);
    }

    // ---------- IO helpers ----------
    void WriteAllTextSafe(string path, string text)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, text);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSlots] WriteAllTextSafe error: {e}");
        }
    }

    string ReadAllTextSafe(string path)
    {
        try { return File.Exists(path) ? File.ReadAllText(path) : null; }
        catch (Exception e) { Debug.LogError($"[SaveSlots] ReadAllTextSafe error: {e}"); return null; }
    }
}
