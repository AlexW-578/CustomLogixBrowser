using System;
using System.Collections.Generic;
using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;
using HarmonyLib;
using NeosModLoader;
using SpecialItemsLib;

namespace CustomLogixBrowser{
	public class CustomLogixBrowser : NeosMod {
		public override string Name => "CustomLogixBrowser";
		public override string Author => "AlexW-578";
		public override string Version => "1.1.0";
		public override string Link => "https://github.com/AlexW-578/CustomLogixBrowser";
		
		private static ModConfiguration Config;

		[AutoRegisterConfigKey]
		private static readonly ModConfigurationKey<bool> Enabled = new ModConfigurationKey<bool>("enabled", "Enables the mod", () => true);

		public override void OnEngineInit() {
			Config = GetConfiguration();
			Config.Save(true);
			Harmony harmony = new Harmony("co.uk.alexw-578.CustomLogixBrowser");
			LogixBrowserObject = SpecialItemsLib.SpecialItemsLib.RegisterItem(LOGIX_BROWSER_TAG);
			harmony.PatchAll();
		}
		private static string LOGIX_BROWSER_TAG { get { return "custom_logix_browser"; } }
		private static CustomSpecialItem LogixBrowserObject;

		[HarmonyPatch(typeof(SlotHelper), "GenerateTags", new Type[] { typeof(Slot), typeof(HashSet<string>) })]
		class SlotHelper_GenerateTags_Patch {
			static void Postfix(Slot slot, HashSet<string> tags) {
				if (slot.GetComponent<LogixNodeSelector>() != null) {
					tags.Add(LOGIX_BROWSER_TAG);
				}
			}
		}

		[HarmonyPatch(typeof(LogixTip), "ToggleNodeSelector")]
		class NeosLogixBrowser_Patch {
			static bool Prefix(LogixTip __instance,SlotCleanupRef<LogixNodeSelector> ____nodeSelector) {
				if (!Config.GetValue(Enabled)) { return true; }
				if (LogixBrowserObject.Uri == null) { return true; }

				if (____nodeSelector.Target == null)
				{
					Slot slot = __instance.LocalUserSpace.AddSlot("NodeMenu");
					slot.StartTask(async delegate()
					{
						await slot.LoadObjectAsync(LogixBrowserObject.Uri);
						InventoryItem component = slot.GetComponent<InventoryItem>();
						slot = ((component != null) ? component.Unpack() : null) ?? slot;
						____nodeSelector.Target = (LogixNodeSelector)slot.GetComponent(typeof(LogixNodeSelector));
						slot.LocalScale = float3.One * 0.5f;
						slot.PositionInFrontOfUser(float3.Backward);
					});
				}
				else
				{
					____nodeSelector.Target.Slot.Destroy();
				}

				__instance.ActiveTool?.CloseContextMenu();
				return false;
			}
		}
	}
}