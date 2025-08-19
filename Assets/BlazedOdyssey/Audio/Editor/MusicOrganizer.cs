using System.IO;
using UnityEditor;
using UnityEngine;

namespace BlazedOdyssey.Audio.Editor
{
    public static class MusicOrganizer
    {
        internal const string MusicFolder = "Assets/Resources/Music";
        private const string ReadmePath = MusicFolder + "/README_Music.txt";
        private const string LicensesPath = MusicFolder + "/LICENSES.txt";

        [MenuItem("BlazedOdyssey/Audio/Create Music Folder Structure")]
        [MenuItem("Tools/BlazedOdyssey/Audio/Create Music Folder Structure")]
        public static void CreateFolders()
        {
            EnsureFolders();
            EditorUtility.DisplayDialog("Music Folders", "Created/verified: \n- Assets/Resources/Music\n- README_Music.txt\n- LICENSES.txt", "OK");
        }

        [MenuItem("BlazedOdyssey/Audio/Import Music Files (copy)")]
        [MenuItem("Tools/BlazedOdyssey/Audio/Import Music Files (copy)")]
        public static void ImportMusic()
        {
            EnsureFolders();
            string folder = EditorUtility.OpenFolderPanel("Select folder containing .mp3/.ogg/.wav", "", "");
            if (string.IsNullOrEmpty(folder)) return;

            string[] files = Directory.GetFiles(folder);
            int copied = 0;
            foreach (var f in files)
            {
                string ext = Path.GetExtension(f).ToLowerInvariant();
                if (ext != ".mp3" && ext != ".ogg" && ext != ".wav") continue;
                string name = Path.GetFileName(f);
                string dest = Path.Combine(MusicFolder, name).Replace('\\', '/');
                try
                {
                    File.Copy(f, dest, true);
                    copied++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Music import: failed to copy {name}: {e.Message}");
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Music Import", $"Copied {copied} file(s) into {MusicFolder}.", "OK");
        }

        [MenuItem("BlazedOdyssey/Audio/Open Curated Free Music Sources")]
        [MenuItem("Tools/BlazedOdyssey/Audio/Open Curated Free Music Sources")]
        public static void OpenCuratedSources()
        {
            // Oldschool/retro MMO-friendly, CC0/CC-BY sources
            Application.OpenURL("https://opengameart.org/content/retro-game-music-pack"); // Juhani Junkala (CC0)
            Application.OpenURL("https://opengameart.org/content/8-bit-music-pack");
            Application.OpenURL("https://opengameart.org/content/rpg-music-pack-loopable");
            Application.OpenURL("https://peritune.com/category/music/loop/"); // PeriTune (CC-BY)
        }

        [MenuItem("BlazedOdyssey/Audio/Batch Download Music (URLs)")]
        [MenuItem("Tools/BlazedOdyssey/Audio/Batch Download Music (URLs)")]
        public static void OpenDownloader()
        {
            MusicDownloaderWindow.ShowWindow();
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(MusicFolder))
                AssetDatabase.CreateFolder("Assets/Resources", "Music");

            // README
            if (!File.Exists(ReadmePath))
            {
                File.WriteAllText(ReadmePath,
                    "Place loop-friendly background music for your game in this folder.\n" +
                    "They will be auto-loaded by GameAudioManager as the default playlist.\n\n" +
                    "Recommended format: .ogg or .mp3 (loop points optional).\n" +
                    "Naming suggestion: 01_Town_Day.ogg, 02_Field.ogg, 03_Dungeon.ogg, 04_Boss.ogg, 05_Night_Ambience.ogg\n");
            }

            // LICENSES
            if (!File.Exists(LicensesPath))
            {
                File.WriteAllText(LicensesPath,
                    "Record the source and license of each track you import here. Examples:\n\n" +
                    "Town_Day.ogg — Juhani Junkala — Retro Game Music Pack — CC0 — https://opengameart.org/content/retro-game-music-pack\n" +
                    "Field.ogg — PeriTune (Sakuya) — CC-BY 4.0 — https://peritune.com/\n");
            }

            AssetDatabase.ImportAsset(ReadmePath);
            AssetDatabase.ImportAsset(LicensesPath);
        }
    }

    public class MusicDownloaderWindow : EditorWindow
    {
        private const string Help = "Paste one URL per line. Optional filename after a pipe: URL | filename. Files will be saved into Assets/Resources/Music.";
        private string _urls = "";
        private Vector2 _scroll;

        public static void ShowWindow()
        {
            var w = GetWindow<MusicDownloaderWindow>(true, "Batch Download Music");
            w.minSize = new Vector2(640, 360);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(Help, MessageType.Info);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _urls = EditorGUILayout.TextArea(_urls, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Download", GUILayout.Height(28)))
                {
                    DownloadAll();
                }
                if (GUILayout.Button("Open Curated Sources", GUILayout.Height(28)))
                {
                    MusicOrganizer.OpenCuratedSources();
                }
            }
        }

        private void DownloadAll()
        {
            MusicOrganizer.CreateFolders();
            var lines = _urls.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                EditorUtility.DisplayDialog("Batch Download Music", "No URLs provided.", "OK");
                return;
            }

            int ok = 0, fail = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var raw = lines[i];
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                string url = line;
                string name = null;
                int pipe = line.IndexOf('|');
                if (pipe >= 0)
                {
                    url = line.Substring(0, pipe).Trim();
                    name = line.Substring(pipe + 1).Trim();
                }
                if (string.IsNullOrEmpty(url)) continue;

                try
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        name = Path.GetFileName(new System.Uri(url).LocalPath);
                        if (string.IsNullOrEmpty(name)) name = "track_" + ok + ".ogg";
                    }
                    string destPath = Path.Combine(MusicOrganizer.MusicFolder, name).Replace('\\', '/');
                    using (var wc = new System.Net.WebClient())
                    {
                        EditorUtility.DisplayProgressBar("Downloading", name, (float)i / Mathf.Max(1, lines.Length));
                        wc.DownloadFile(url, destPath);
                    }
                    ok++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to download: {url} — {e.Message}");
                    fail++;
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Batch Download Music", $"Completed. Success: {ok}, Failed: {fail}.", "OK");
        }
    }
}


