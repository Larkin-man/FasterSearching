using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;

namespace FasterSearching
{
    [HarmonyPatch(typeof(Container), nameof(Container.BeginContainerOpen))]
    internal static class PatchFasterSearch
    {
        // Запоминаем оригинальное время обыска для каждого контейнера по его InstanceID,
        // чтобы при повторном открытии множитель не применялся несколько раз подряд
        private static readonly Dictionary<int, float> OriginalSearchTimes = new Dictionary<int, float>();

        private static void Prefix(Container __instance)
        {
            if (__instance == null) return;

            int instanceId = __instance.GetInstanceID();

            if (!OriginalSearchTimes.TryGetValue(instanceId, out float baseSearchTime))
            {
                baseSearchTime = __instance.m_ContainerSearchSeconds;
                OriginalSearchTimes[instanceId] = baseSearchTime;
            }

            bool isEmpty = __instance.IsEmpty();
            bool isInspected = __instance.IsInspected();

            if (isEmpty && FasterSearchingSettings.options.instantEmptyContainers)
            {
                __instance.m_ContainerSearchSeconds = 0f;
            }
            else if (isInspected)
            {
                __instance.m_ContainerSearchSeconds = baseSearchTime * FasterSearchingSettings.options.searchedContainerSpeedMultiplier;
            }
            else
            {
                __instance.m_ContainerSearchSeconds = baseSearchTime * FasterSearchingSettings.options.searchSpeedMultiplier;
            }
        }
    }
}
