using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LocalizationTestWithoutIsReady : MonoBehaviour
{
    public string tableReference = "SkillDescription";
    public string entryReference = "skill_SK001_description";
    public Text displayText;

    void Start()
    {
        // 等待一段时间，确保Localization初始化（具体时间视情况而定）
        StartCoroutine(WaitAndGetContent());
    }

    IEnumerator WaitAndGetContent()
    {
        // 可选择等待初始化操作完成，调用事件（如果你导入了Localization表或在导入后即可跳过）
        while (!LocalizationSettings.InitializationOperation.IsDone && !LocalizationSettings.InitializationOperation.IsDone)
        {
            yield return null; //每帧检测
        }
        // 也可以不检测，直接等待几秒（比如0.5秒）
        // yield return new WaitForSeconds(0.5f);

        // 直接读取内容
        var table = LocalizationSettings.StringDatabase.GetTable(tableReference);
        if (table == null)
        {
            Debug.LogError($"找不到Localization Table：{tableReference}");
            yield break;
        }
        var entry = table.GetEntry(entryReference);
        if (entry != null)
        {
            string content = entry.GetLocalizedString();
            Debug.Log($"【内容】：{content}");
            if (displayText != null)
                displayText.text = content;
        }
        else
        {
            Debug.LogError($"未找到Entry：{entryReference}在表：{tableReference}");
        }
    }
}
