using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using StardewValley.Menus;

namespace CatalogueIndicator
{
	public class ShopMenuPatches
	{
		private static readonly Dictionary<string, bool> itemAvailabilityCache = new();

		public static void draw_Postfix(ref ShopMenu __instance, SpriteBatch b)
		{
			for (int i = 0; i < __instance.forSaleButtons.Count; i++)
			{
				if (__instance.currentItemIndex + i < __instance.forSale.Count)
				{
					ISalable item = __instance.forSale[__instance.currentItemIndex + i];
					ItemStockInformation stockInfo = __instance.itemPriceAndStock[item];
					StackDrawType stackDrawType = __instance.GetStackDrawType(stockInfo, item);

					if (item.ShouldDrawIcon())
					{
						Vector2 drawPos = new Vector2((float)(__instance.forSaleButtons[i].bounds.X + 32 - 8), (float)(__instance.forSaleButtons[i].bounds.Y + 24));

						// First check if we've already cached this item.
						if (!itemAvailabilityCache.TryGetValue(item.QualifiedItemId, out bool presentInCatalogue))
						{
							// Check if the given item appears in any catalogues.
							foreach (var shopPair in ModEntry.resolvedShopCache)
							{
								foreach (ISalable shopEntry in shopPair.Value)
								{
									if (shopEntry.QualifiedItemId == item.QualifiedItemId)
									{
										presentInCatalogue = true;

										// Add this item to the cache.
										itemAvailabilityCache.Add(item.QualifiedItemId, true);
										break;
									}
								}

								if (presentInCatalogue)
								{
									break;
								}
							}

							if (!presentInCatalogue)
							{
								// Add this item to the cache.
								itemAvailabilityCache.Add(item.QualifiedItemId, false);
							}
						}
					
						// If the item is in a catalogue, draw the icon.
						if (presentInCatalogue)
						{
							// Draw the catalogue icon.
							b.Draw(ModEntry.catalogueIcon, drawPos + new Vector2(-6f, -10f), new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.9f + 0.0001f);
						}
					}
				}
			}
		}
	}
}