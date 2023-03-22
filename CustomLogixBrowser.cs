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
		public override string Version => "0.0.1";
		public override string Link => "https://github.com/AlexW-578/CustomLogixBrowser";

		private static ModConfiguration Config;

		[AutoRegisterConfigKey]
		private static readonly ModConfigurationKey<bool> enabled = new ModConfigurationKey<bool>("enabled", "Enables the mod", () => true);

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
					Warn("AlexW-578: Adding Logix Browser Tag");
				}
			}
		}

		[HarmonyPatch(typeof(LogixNodeSelector), "OnAttach")]
		class NeosLogixBrowser_Patch {
			static bool Prefix(LogixNodeSelector __instance) {
				if (!Config.GetValue(enabled)) { return true; }
				if (LogixBrowserObject.Uri == null) { return true; }

				var slot = __instance.Slot;

				slot.StartTask(async delegate () {
					await slot.LoadObjectAsync(LogixBrowserObject.Uri);
					InventoryItem component = slot.GetComponent<InventoryItem>();
					slot = ((component != null) ? component.Unpack() : null) ?? slot;
					__instance.Enabled = false;
					slot.LocalScale = float3.One * 0.5f;
					__instance.Destroy();
					slot.PositionInFrontOfUser(float3.Backward);
				});
				return false;
			}
		}
	}
}