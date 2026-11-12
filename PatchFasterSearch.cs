using HarmonyLib;
using Il2Cpp;

namespace FasterSearching
{
    [HarmonyPatch(typeof(ContainerInteraction))]
    internal static class PatchFasterSearch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ContainerInteraction.PerformHold))]
        private static void PerformHold_Postfix(ContainerInteraction __instance, ref bool __result)
        {
            if (!__result || __instance == null || __instance.m_Container == null || FasterSearchingSettings.Instance == null)
            {
                return;
            }

            // Не вмешиваемся во взлом сейфа
            if (__instance.m_IsSafeCracking || __instance.m_Container.IsSafeLocked())
            {
                return;
            }

            Container container = __instance.m_Container;
            bool isInspected = container.IsInspected();
            bool isEmpty = container.IsEmpty();

            float multiplier;

            if (!isInspected && isEmpty && FasterSearchingSettings.Instance.instantlySearchEmpty)
            {
                multiplier = 0f;
            }
            else if (isInspected)
            {
                multiplier = FasterSearchingSettings.Instance.openTimeMultiplier;
            }
            else
            {
                multiplier = FasterSearchingSettings.Instance.searchTimeMultiplier;
            }

            // Игра внутри PerformHold() уже записала актуальное базовое время в HoldTime
            float targetTime = __instance.HoldTime * multiplier;

            if (targetTime <= 0.01f)
            {
                // Ставим минимальный порог > 0, чтобы избежать деления на 0 в NormalizedProgress,
                // и сразу доводим таймер до конца — контейнер откроется мгновенно на следующем кадре
                __instance.HoldTime = 0.01f;
                __instance.m_Timer = 0.01f;
            }
            else
            {
                __instance.HoldTime = targetTime;
            }
        }
    }
}
