#if UNITY_EDITOR
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace OneHitDown.EditorTools
{
    /// <summary>
    /// Kiểm tra nhanh TMP đã sẵn sàng: TMP Settings có tồn tại, có font mặc định,
    /// và prefab Cổng Thần render được chữ hay không. Chạy qua batchmode -executeMethod.
    /// </summary>
    internal static class TmpReadinessCheck
    {
        [MenuItem("Tools/One Hit Down/Check TMP Readiness", false, 41)]
        private static void CheckMenu()
        {
            Run();
        }

        /// <summary>Điểm vào batchmode: -executeMethod OneHitDown.EditorTools.TmpReadinessCheck.RunFromCommandLine</summary>
        public static void RunFromCommandLine()
        {
            Run();
        }

        private static void Run()
        {
            StringBuilder sb = new StringBuilder();
            int errors = 0;

            TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings == null)
            {
                sb.AppendLine("FAIL  TMP Settings chưa có trong Assets/TextMesh Pro/Resources/");
                errors++;
            }
            else
            {
                sb.AppendLine("PASS  TMP Settings đã nạp được.");
                TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
                if (defaultFont == null)
                {
                    sb.AppendLine("FAIL  TMP Settings.defaultFontAsset = null");
                    errors++;
                }
                else
                {
                    sb.AppendLine($"PASS  defaultFontAsset = '{defaultFont.name}' ({defaultFont.characterLookupTable?.Count ?? 0} ký tự).");
                }
            }

            GameObject gatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ExitGate.prefab");
            if (gatePrefab == null)
            {
                sb.AppendLine("FAIL  Không nạp được Assets/Prefabs/ExitGate.prefab");
                errors++;
            }
            else
            {
                TMP_Text label = gatePrefab.GetComponentInChildren<TMP_Text>(true);
                if (label == null)
                {
                    sb.AppendLine("FAIL  ExitGate.prefab không có TMP_Text nào.");
                    errors++;
                }
                else
                {
                    // TextMeshPro.LoadFontAsset() tự gán font mặc định khi m_fontAsset == null.
                    sb.AppendLine($"PASS  ExitGate có TMP_Text '{label.name}', font hiện tại = " +
                                  (label.font == null ? "null (sẽ tự lấy font mặc định khi chạy)" : label.font.name));

                    // Kiểm chứng thật: dựng thử một bản sao và bắt TMP sinh mesh cho chuỗi [LOCKED 0/3].
                    errors += VerifyRenders(sb, label);
                }
            }

            if (errors == 0)
            {
                Debug.Log($"<b>[TmpReadinessCheck]</b> <color=green>PASS</color>\n{sb}");
            }
            else
            {
                Debug.LogError($"<b>[TmpReadinessCheck]</b> <color=red>FAIL ({errors} lỗi)</color>\n{sb}");
            }
        }

        /// <summary>
        /// Dựng bản sao TMP_Text trong Scene tạm, ép sinh mesh cho chuỗi "[LOCKED 0/3]"
        /// rồi đếm số đỉnh — nếu > 0 nghĩa là chữ thật sự vẽ được, không chỉ "có font".
        /// </summary>
        private static int VerifyRenders(StringBuilder sb, TMP_Text source)
        {
            GameObject probe = Object.Instantiate(source.gameObject);
            probe.hideFlags = HideFlags.HideAndDontSave;
            TMP_Text probeText = probe.GetComponent<TMP_Text>();

            try
            {
                probeText.text = "[LOCKED 0/3]";
                probeText.ForceMeshUpdate();

                int vertexCount = 0;
                if (probeText.textInfo != null && probeText.textInfo.meshInfo != null)
                {
                    for (int i = 0; i < probeText.textInfo.meshInfo.Length; i++)
                    {
                        vertexCount += probeText.textInfo.meshInfo[i].vertexCount;
                    }
                }

                int characterCount = probeText.textInfo != null ? probeText.textInfo.characterCount : 0;
                if (vertexCount > 0)
                {
                    sb.AppendLine($"PASS  Sinh mesh cho '[LOCKED 0/3]': {vertexCount} đỉnh, {characterCount} ký tự — chữ vẽ được.");
                    return 0;
                }

                sb.AppendLine("FAIL  ForceMeshUpdate sinh 0 đỉnh — chữ sẽ không hiện.");
                return 1;
            }
            finally
            {
                Object.DestroyImmediate(probe);
            }
        }
    }
}
#endif
