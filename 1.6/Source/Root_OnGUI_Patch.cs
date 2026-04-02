using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace ImmersiveOpening
{
	[HarmonyPatch(typeof(Root), nameof(Root.OnGUI))]
	public static class Root_OnGUI_Patch
	{
		public static bool isImmersiveOpeningActive;
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var uiRootOnGUIMethod = AccessTools.Method(typeof(UIRoot), nameof(UIRoot.UIRootOnGUI));
			var uiRootMethod = AccessTools.Method(typeof(Root_OnGUI_Patch), nameof(UIRootOnGUI));
			var patched = false;

			foreach (var inst in instructions)
			{
				if (!patched && inst.Calls(uiRootOnGUIMethod))
				{
					yield return new CodeInstruction(OpCodes.Call, uiRootMethod);
					patched = true;
				}
				else
				{
					yield return inst;
				}
			}
		}

		public static void UIRootOnGUI(UIRoot uiRoot)
		{
			if (isImmersiveOpeningActive)
			{
				Text.StartOfOnGUI();
				uiRoot.windows.HandleEventsHighPriority();
				uiRoot.windows.WindowStackOnGUI();
			}
			else
			{
				uiRoot.UIRootOnGUI();
			}
		}
	}
}
