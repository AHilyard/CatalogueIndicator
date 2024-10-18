using System;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using System.Collections.Generic;
using StardewValley.GameData.Shops;
using StardewValley.Extensions;
using StardewValley.Internal;

namespace CatalogueIndicator
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod
	{
		public static IMonitor M = null;

		public static Texture2D catalogueIcon = null;

		public static readonly Dictionary<string, List<ISalable>> resolvedShopCache = new();

		/*********
		** Public methods
		*********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			M = Monitor;

			// Load the catalogue icon.
			catalogueIcon = helper.ModContent.Load<Texture2D>("assets/catalogue_icon.png");

			Harmony harmony = new Harmony(ModManifest.UniqueID);

			// Patch items's draw menu items method to add icons.
			harmony.Patch(original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.draw), new Type[] { typeof(SpriteBatch) }),
						  postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopMenuPatches.draw_Postfix)));

			helper.Events.GameLoop.SaveLoaded += (s, e) => {

				// If this is the first time a game is loaded, resolve the catalogue cache now.
				if (resolvedShopCache.Count == 0)
				{
					Dictionary<string, ShopData> shopDictionary = new(DataLoader.Shops(Game1.content));

					// Prune the dictionary down to only catalogues.
					shopDictionary.RemoveWhere(shop => !shop.Key.Contains("Catalogue"));

					// Fully resolve all catalogue shops.
					foreach (var shopPair in shopDictionary)
					{
						List<ISalable> shopItems = new();
						ShopData shop = shopPair.Value;
						foreach (ShopItemData shopEntry in shop.Items)
						{
							IEnumerable<ItemQueryResult> queryResults = ItemQueryResolver.TryResolve(shopEntry, null, ItemQuerySearchMode.All, true);
							foreach (ItemQueryResult result in queryResults)
							{
								shopItems.Add(result.Item);
							}
						}
						resolvedShopCache.Add(shopPair.Key, shopItems);
					}
				}
			};
		}
	}
}