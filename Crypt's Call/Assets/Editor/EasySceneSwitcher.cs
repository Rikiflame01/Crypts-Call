#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class EasySceneSwitcher : EditorWindow
{
    // List of selected folders to search for scenes
    private List<string> selectedFolders = new List<string>();

    // Dictionary to hold categorized scenes
    private Dictionary<string, List<string>> categorizedScenes = new Dictionary<string, List<string>>();

    // Dictionary to hold category colors
    private Dictionary<string, Color> categoryColors = new Dictionary<string, Color>();

    // Scroll positions for the UI
    private Vector2 scrollPositionFolders;
    private Vector2 scrollPositionScenes;

    // EditorPrefs keys for saving data
    private const string EditorPrefsFoldersKey = "EasySceneSwitcher_SelectedFolders";
    private const string EditorPrefsColorsKey = "EasySceneSwitcher_CategoryColors";

    /// <summary>
    /// Adds a menu item named "Easy Scene Switcher" to the Tools menu
    /// </summary>
    [MenuItem("Tools/Easy Scene Switcher")]
    public static void ShowWindow()
    {
        // Show existing window instance. If one doesn't exist, make one.
        EasySceneSwitcher window = GetWindow<EasySceneSwitcher>("Scene Switcher");
        window.minSize = new Vector2(400, 600); // Adjusted size for better layout
        window.Show();
    }

    /// <summary>
    /// Called when the window is enabled (opened or recompiled)
    /// </summary>
    private void OnEnable()
    {
        LoadSelectedFolders();
        LoadCategoryColors();
        RefreshSceneList();
    }

    /// <summary>
    /// Main GUI rendering function
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("Easy Scene Switcher", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Section: Selected Folders
        GUILayout.Label("Selected Folders", EditorStyles.label);
        EditorGUILayout.Space();

        // Begin ScrollView for folders
        scrollPositionFolders = EditorGUILayout.BeginScrollView(scrollPositionFolders, GUILayout.Height(150));

        if (selectedFolders.Count == 0)
        {
            EditorGUILayout.HelpBox("No folders selected. Click 'Add Folder' to include scenes.", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < selectedFolders.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(selectedFolders[i], GUILayout.MaxWidth(250));
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    RemoveFolder(i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // Add Folder Button
        if (GUILayout.Button("Add Folder", GUILayout.Height(30)))
        {
            AddFolder();
        }

        EditorGUILayout.Space();

        // Refresh Scene List Button
        if (GUILayout.Button("Refresh Scene List", GUILayout.Height(35)))
        {
            RefreshSceneList();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Section: Scene List
        GUILayout.Label("Scenes", EditorStyles.label);
        EditorGUILayout.Space();

        // Begin ScrollView for scenes
        scrollPositionScenes = EditorGUILayout.BeginScrollView(scrollPositionScenes);

        if (categorizedScenes.Count == 0)
        {
            EditorGUILayout.HelpBox("No scenes found in the selected folders.", MessageType.Info);
        }
        else
        {
            foreach (var category in categorizedScenes)
            {
                // Ensure the category has a color
                if (!categoryColors.ContainsKey(category.Key))
                {
                    categoryColors[category.Key] = Color.white; // Default color
                }

                // Begin Horizontal Layout for Category Header and Color Picker
                EditorGUILayout.BeginHorizontal();

                // Display category name with current color
                GUI.color = categoryColors[category.Key];
                GUILayout.Label("---- " + category.Key + " ----", EditorStyles.boldLabel);
                GUI.color = Color.white; // Reset color

                // Inline Color Picker
                Color newColor = EditorGUILayout.ColorField(categoryColors[category.Key], GUILayout.Width(100));
                if (newColor != categoryColors[category.Key])
                {
                    categoryColors[category.Key] = newColor;
                    SaveCategoryColors();
                }

                EditorGUILayout.EndHorizontal();

                // Scenes under the category
                foreach (var scene in category.Value)
                {
                    // Set button background color to match category color
                    GUI.backgroundColor = categoryColors[category.Key];
                    if (GUILayout.Button(scene, GUILayout.Height(25)))
                    {
                        string scenePath = GetScenePath(category.Key, scene);
                        if (!string.IsNullOrEmpty(scenePath))
                        {
                            SwitchScene(scenePath);
                        }
                        else
                        {
                            Debug.LogError($"Scene path not found for scene: {scene}");
                        }
                    }
                    // Reset background color
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Loads the selected folders from EditorPrefs
    /// </summary>
    private void LoadSelectedFolders()
    {
        string json = EditorPrefs.GetString(EditorPrefsFoldersKey, "");
        if (!string.IsNullOrEmpty(json))
        {
            FolderList folderList = JsonUtility.FromJson<FolderList>(json);
            if (folderList != null && folderList.folders != null)
            {
                selectedFolders = folderList.folders;
            }
        }

        // If no folders are selected, add a default "Assets/Scenes" folder
        if (selectedFolders.Count == 0)
        {
            string defaultFolder = "Assets/Scenes";
            if (AssetDatabase.IsValidFolder(defaultFolder))
            {
                selectedFolders.Add(defaultFolder);
                SaveSelectedFolders();
            }
        }
    }

    /// <summary>
    /// Loads the category colors from EditorPrefs
    /// </summary>
    private void LoadCategoryColors()
    {
        string json = EditorPrefs.GetString(EditorPrefsColorsKey, "");
        if (!string.IsNullOrEmpty(json))
        {
            ColorList colorList = JsonUtility.FromJson<ColorList>(json);
            if (colorList != null && colorList.categories != null && colorList.colors != null)
            {
                for (int i = 0; i < colorList.categories.Count && i < colorList.colors.Count; i++)
                {
                    if (!categoryColors.ContainsKey(colorList.categories[i]))
                    {
                        categoryColors.Add(colorList.categories[i], colorList.colors[i]);
                    }
                    else
                    {
                        categoryColors[colorList.categories[i]] = colorList.colors[i];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Saves the selected folders to EditorPrefs
    /// </summary>
    private void SaveSelectedFolders()
    {
        FolderList folderList = new FolderList { folders = selectedFolders };
        string json = JsonUtility.ToJson(folderList);
        EditorPrefs.SetString(EditorPrefsFoldersKey, json);
    }

    /// <summary>
    /// Saves the category colors to EditorPrefs
    /// </summary>
    private void SaveCategoryColors()
    {
        ColorList colorList = new ColorList
        {
            categories = categoryColors.Keys.ToList(),
            colors = categoryColors.Values.ToList()
        };
        string json = JsonUtility.ToJson(colorList);
        EditorPrefs.SetString(EditorPrefsColorsKey, json);
    }

    /// <summary>
    /// Adds a new folder to the selectedFolders list
    /// </summary>
    private void AddFolder()
    {
        string selectedPath = EditorUtility.OpenFolderPanel("Select Folder to Include", Application.dataPath, "");

        if (string.IsNullOrEmpty(selectedPath))
        {
            // User canceled the folder selection
            return;
        }

        // Convert absolute path to relative Asset path
        string relativePath = ConvertToAssetPath(selectedPath);

        if (string.IsNullOrEmpty(relativePath))
        {
            EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder within the project's Assets directory.", "OK");
            return;
        }

        // Check if the folder exists in the project
        if (!AssetDatabase.IsValidFolder(relativePath))
        {
            EditorUtility.DisplayDialog("Invalid Folder", "The selected folder does not exist in the project.", "OK");
            return;
        }

        // Avoid duplicates
        if (selectedFolders.Contains(relativePath))
        {
            EditorUtility.DisplayDialog("Folder Exists", "The selected folder is already included.", "OK");
            return;
        }

        selectedFolders.Add(relativePath);
        SaveSelectedFolders();
        RefreshSceneList();
    }

    /// <summary>
    /// Removes a folder from the selectedFolders list by index
    /// </summary>
    /// <param name="index">Index of the folder to remove</param>
    private void RemoveFolder(int index)
    {
        if (index >= 0 && index < selectedFolders.Count)
        {
            string removedFolder = selectedFolders[index];
            selectedFolders.RemoveAt(index);
            SaveSelectedFolders();
            RefreshSceneList();

            // Also remove category color if no longer present
            string categoryName = GetParentFolderName(removedFolder);
            if (!selectedFolders.Any(f => GetParentFolderName(f) == categoryName))
            {
                if (categoryColors.ContainsKey(categoryName))
                {
                    categoryColors.Remove(categoryName);
                    SaveCategoryColors();
                }
            }
        }
    }

    /// <summary>
    /// Converts an absolute system path to a Unity Asset path
    /// </summary>
    /// <param name="absolutePath">Absolute path selected by the user</param>
    /// <returns>Relative Asset path or null if invalid</returns>
    private string ConvertToAssetPath(string absolutePath)
    {
        string dataPath = Application.dataPath.Replace("\\", "/");
        absolutePath = absolutePath.Replace("\\", "/");

        if (absolutePath.StartsWith(dataPath))
        {
            return "Assets" + absolutePath.Substring(dataPath.Length);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Refreshes the list of scenes based on selected folders
    /// </summary>
    private void RefreshSceneList()
    {
        categorizedScenes.Clear();

        foreach (string folder in selectedFolders)
        {
            // Find all scene GUIDs in the current folder
            string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { folder });

            // Convert GUIDs to asset paths
            string[] scenePaths = sceneGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray();

            foreach (string scenePath in scenePaths)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                string parentFolderName = GetParentFolderName(folder);

                if (!categorizedScenes.ContainsKey(parentFolderName))
                {
                    categorizedScenes[parentFolderName] = new List<string>();
                }

                categorizedScenes[parentFolderName].Add(sceneName);
            }
        }

        // Sort categories and scenes alphabetically
        var sortedCategories = categorizedScenes.Keys.ToList();
        sortedCategories.Sort();

        Dictionary<string, List<string>> sortedCategorizedScenes = new Dictionary<string, List<string>>();
        foreach (var category in sortedCategories)
        {
            sortedCategorizedScenes[category] = categorizedScenes[category].OrderBy(s => s).ToList();

            // Ensure each category has a color
            if (!categoryColors.ContainsKey(category))
            {
                categoryColors[category] = Color.white; // Default color
            }
        }

        categorizedScenes = sortedCategorizedScenes;

        // Save category colors in case new categories were added
        SaveCategoryColors();

        Debug.Log($"Found {scenePathsTotal()} scene(s) in the selected folders.");
    }

    /// <summary>
    /// Gets the parent folder name from a given folder path
    /// </summary>
    /// <param name="folderPath">Asset folder path</param>
    /// <returns>Parent folder name</returns>
    private string GetParentFolderName(string folderPath)
    {
        string trimmedPath = folderPath.TrimEnd('/');
        int lastSlash = trimmedPath.LastIndexOf('/');

        if (lastSlash >= 0 && lastSlash < trimmedPath.Length - 1)
        {
            return trimmedPath.Substring(lastSlash + 1);
        }
        else
        {
            return folderPath;
        }
    }

    /// <summary>
    /// Calculates the total number of scenes found
    /// </summary>
    /// <returns>Total scene count</returns>
    private int scenePathsTotal()
    {
        int total = 0;
        foreach (var category in categorizedScenes.Values)
        {
            total += category.Count;
        }
        return total;
    }

    /// <summary>
    /// Retrieves the full scene path based on category and scene name
    /// </summary>
    /// <param name="category">Category name (parent folder)</param>
    /// <param name="sceneName">Scene name</param>
    /// <returns>Full Asset path to the scene</returns>
    private string GetScenePath(string category, string sceneName)
    {
        // Find the folder path that matches the category
        string folderPath = selectedFolders.FirstOrDefault(f => GetParentFolderName(f) == category);
        if (string.IsNullOrEmpty(folderPath))
            return null;

        // Construct the scene path
        string scenePath = Path.Combine(folderPath, sceneName + ".unity").Replace("\\", "/");

        if (AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scenePath) != null)
        {
            return scenePath;
        }
        else
        {
            // Scene asset not found
            return null;
        }
    }

    /// <summary>
    /// Switches to the specified scene, prompting to save changes first
    /// </summary>
    /// <param name="scenePath">Asset path to the scene</param>
    private void SwitchScene(string scenePath)
    {
        // Check for unsaved changes in the current scene
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // Open the new scene
            EditorSceneManager.OpenScene(scenePath);
            Debug.Log($"Switched to scene: {scenePath}");
        }
        else
        {
            Debug.LogWarning("Scene switch cancelled. Unsaved changes were not saved.");
        }
    }

    /// <summary>
    /// Serializable class to hold the list of folders
    /// </summary>
    [System.Serializable]
    private class FolderList
    {
        public List<string> folders = new List<string>();
    }

    /// <summary>
    /// Serializable class to hold category colors
    /// </summary>
    [System.Serializable]
    private class ColorList
    {
        public List<string> categories = new List<string>();
        public List<Color> colors = new List<Color>();
    }
}
#endif