
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class UnboundIconReport_Robust : EditorWindow
{
    private string spriteFolder = "Assets/Sprite/SkillIcon";
    private string outputCsvPath = "Assets/SkillData/unbound_icons_report_robust.csv";

    // classID -> className mapping (same as importer)
    private static readonly Dictionary<string, string> ClassNameMap = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase) {
        {"CL001","Knight1"},
        {"CL002","Knight2"},
        {"CL003","Knight3"},
        {"CL011","Samurai1"},
        {"CL012","Samurai2"},
        {"CL013","Samurai3"},
        {"CL021","Warrior1"},
        {"CL022","Warrior2"},
        {"CL031","Pirate1"},
        {"CL032","Pirate2"},
        {"CL041","Alchemist1"},
        {"CL042","Alchemist2"},
        {"CL051","Master1"},
        {"CL052","Master2"},
        {"CL053","Master3"},
        {"CL071","Artist1"}
    };

    [MenuItem("Tools/Skill/Report Unbound Skill Icons (Robust)")]
    public static void OpenWindow()
    {
        GetWindow<UnboundIconReport_Robust>("Unbound Icon Report (Robust)");
    }

    void OnGUI()
    {
        GUILayout.Label("Unbound Skill Icon Report (Robust)", EditorStyles.boldLabel);
        spriteFolder = EditorGUILayout.TextField("Sprite folder", spriteFolder);
        outputCsvPath = EditorGUILayout.TextField("Output CSV path", outputCsvPath);

        if (GUILayout.Button("Generate Robust Report"))
        {
            GenerateReport();
        }
        if (GUILayout.Button("Open output CSV"))
        {
            if (File.Exists(outputCsvPath)) EditorUtility.RevealInFinder(PathGetFull(outputCsvPath));
            else EditorUtility.DisplayDialog("Not found", "Output CSV not found. Generate report first.", "OK");
        }
        EditorGUILayout.HelpBox("This tool searches all SkillDefinition assets in project and lists those without a Sprite assigned to the 'icon' field (using strong-typed load).", MessageType.Info);
    }

    void GenerateReport()
    {
        // Find all SkillDefinition assets anywhere in project
        var guids = AssetDatabase.FindAssets("t:SkillDefinition");
        var sbConsole = new StringBuilder();
        var sbCsv = new StringBuilder();
        sbCsv.AppendLine("skillID,classID,skillName,assetPath,suggestedIconNames,existingSpriteMatches");

        int count = 0;

        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            // Try load as SkillDefinition type
            var sd = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            if (sd == null)
            {
                // Possibly type mismatch: log for debug
                Debug.LogWarning($"Found asset at {path} but could not load as SkillDefinition (null). It may be another type or compilation issue.");
                continue;
            }

            string skillID = sd.skillID ?? "";
            string classID = sd.classID ?? "";
            string skillName = sd.skillName ?? "";
            bool unbound = sd.icon == null;

            if (unbound)
            {
                count++;
                string className = "";
                ClassNameMap.TryGetValue(classID, out className);
                var candidates = BuildIconCandidates(skillID, className, skillName);
                string candidatesJoined = string.Join(" | ", candidates);

                // search for any existing sprite matches in spriteFolder for quick hint
                var foundMatches = new List<string>();
                foreach (var c in candidates)
                {
                    var exts = new[] { ".png", ".jpg", ".psd", ".tga" };
                    foreach (var e in exts)
                    {
                        string p = PathCombine(spriteFolder, c + e);
                        if (File.Exists(PathGetFull(p)))
                        {
                            foundMatches.Add(p);
                        }
                    }
                    var results = AssetDatabase.FindAssets(c + " t:Sprite", new[] { spriteFolder });
                    foreach (var guid in results)
                    {
                        var p2 = AssetDatabase.GUIDToAssetPath(guid);
                        if (!foundMatches.Contains(p2)) foundMatches.Add(p2);
                    }
                }

                string foundJoined = foundMatches.Count > 0 ? string.Join(" | ", foundMatches) : "";

                sbConsole.AppendLine($"UNBOUND -> {skillID} | class:{classID} | name:{skillName} | asset:{path}");
                sbConsole.AppendLine($"    Suggested: {candidatesJoined}");
                if (!string.IsNullOrEmpty(foundJoined)) sbConsole.AppendLine($"    Existing matches: {foundJoined}");
                else sbConsole.AppendLine($"    Existing matches: NONE");

                string csvRow = $"{EscapeCsv(skillID)},{EscapeCsv(classID)},{EscapeCsv(skillName)},{EscapeCsv(path)},{EscapeCsv(candidatesJoined)},{EscapeCsv(foundJoined)}";
                sbCsv.AppendLine(csvRow);
            }
            else
            {
                // optionally log bound items in verbose mode - commented out to reduce spam
                // Debug.Log($"Bound -> {skillID} icon:{sd.icon.name}");
            }
        }

        sbConsole.AppendLine($"Unbound icon count: {count}");
        Debug.Log(sbConsole.ToString());

        // copy to clipboard
        EditorGUIUtility.systemCopyBuffer = sbConsole.ToString();
        Debug.Log("Report copied to clipboard.");

        // write csv
        try
        {
            var folder = Path.GetDirectoryName(outputCsvPath);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            File.WriteAllText(outputCsvPath, sbCsv.ToString(), System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log($"CSV exported to: {outputCsvPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to write CSV: {ex.Message}");
        }

        EditorUtility.DisplayDialog("Report generated", $"Found {count} unbound skills. Results copied to clipboard and exported to:\n{outputCsvPath}", "OK");
    }

    static string EscapeCsv(string s)
    {
        if (s == null) return "";
        if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
        return s;
    }

    static string PathCombine(string a, string b)
    {
        return a.TrimEnd('/', '\\') + "/" + b.TrimStart('/', '\\');
    }

    static string PathGetFull(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return assetPath;
        return Path.GetFullPath(assetPath);
    }

    static List<string> BuildIconCandidates(string skillID, string className, string skillName)
    {
        var candidates = new List<string>();
        className = className ?? "";
        skillName = skillName ?? "";
        string sanitized = SanitizeName(skillName);
        string underscore = sanitized.Replace(" ", "_");
        string nospace = sanitized.Replace(" ", "");
        string dash = sanitized.Replace(" ", "-");

        if (!string.IsNullOrEmpty(className))
        {
            candidates.Add($"Skill_{className}_{sanitized}");
            candidates.Add($"Skill_{className}_{underscore}");
            candidates.Add($"Skill_{className}_{nospace}");
            candidates.Add($"Skill_{className}_{dash}");
            candidates.Add($"Skill_{className}_{skillID}");
            candidates.Add($"Skill_{className}_{skillID.ToLower()}");
        }
        candidates.Add($"Skill_{sanitized}");
        candidates.Add($"Skill_{underscore}");
        candidates.Add($"Skill_{nospace}");
        candidates.Add($"{sanitized}");
        candidates.Add($"{underscore}");
        candidates.Add(skillID);
        candidates.Add($"Skill_{skillID}");

        return candidates.Distinct(System.StringComparer.OrdinalIgnoreCase).ToList();
    }

    static string SanitizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        string cleaned = Regex.Replace(name, @"[^\w\s\-]", ""); // remove punctuation except underscore/dash/space
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();
        return cleaned;
    }
}
