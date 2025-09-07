using System;
using System.Collections.Generic;
using UnityEngine;

// 语言类型（以后加语言，这里加枚举就行，比如Japanese=2）
public enum LanguageType
{
    ChineseSimplified, // 简体中文
    English            // 英文
}

// 语言包数据（不用管，JSON解析用）
[Serializable]
public class LanguagePackage
{
    public LanguageType Language;
    public List<TextEntry> Entries;
}
[Serializable]
public class TextEntry
{
    public string TextId; // 文本唯一ID（比如“技能_火球术_名字”）
    public string Text;   // 对应语言的文字
}

// 多语言管理器（全局唯一，负责找文字）
public class I18nManager : MonoBehaviour
{
    public static I18nManager Instance { get; private set; }

    [Header("语言包配置")]
    public List<TextAsset> LanguagePackageAssets; // 拖入语言JSON文件
    public LanguageType DefaultLanguage = LanguageType.ChineseSimplified;

    private Dictionary<string, string> _textMap = new(); // 存ID和文字的对应关系

    private void Awake()
    {
        // 确保只有一个管理器
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 加载默认语言（中文）
        LoadLanguage(DefaultLanguage);
    }

    // 加载指定语言（切换语言时调用）
    public void LoadLanguage(LanguageType lang)
    {
        _textMap.Clear();
        // 找对应的语言文件
        TextAsset targetFile = null;
        foreach (var asset in LanguagePackageAssets)
        {
            LanguagePackage pack = JsonUtility.FromJson<LanguagePackage>(asset.text);
            if (pack.Language == lang) { targetFile = asset; break; }
        }
        // 没找到就用默认中文
        if (targetFile == null)
        {
            foreach (var asset in LanguagePackageAssets)
            {
                LanguagePackage pack = JsonUtility.FromJson<LanguagePackage>(asset.text);
                if (pack.Language == DefaultLanguage) { targetFile = asset; break; }
            }
        }
        // 解析文件，存ID和文字
        LanguagePackage parsed = JsonUtility.FromJson<LanguagePackage>(targetFile.text);
        foreach (var entry in parsed.Entries)
        {
            if (!_textMap.ContainsKey(entry.TextId))
                _textMap.Add(entry.TextId, entry.Text);
        }
        Debug.Log($"加载了{lang}语言，共{_textMap.Count}段文字");
    }

    // 对外接口：通过ID获取文字（重点！其他脚本用这个找文字）
    public string GetText(string textId, params object[] args)
    {
        // 没找到ID就返回ID本身（方便调试）
        if (!_textMap.TryGetValue(textId, out string text))
        {
            Debug.LogWarning($"没找到文本ID：{textId}");
            return textId;
        }
        // 替换占位符（比如“{0}造成{1}伤害”里的{0}和{1}）
        if (args != null && args.Length > 0)
            text = string.Format(text, args);
        return text;
    }

    // 切换语言的事件（UI刷新用）
    public event Action<LanguageType> OnLanguageChanged;
    public void SwitchLanguage(LanguageType lang)
    {
        LoadLanguage(lang);
        OnLanguageChanged?.Invoke(lang);
    }
}

