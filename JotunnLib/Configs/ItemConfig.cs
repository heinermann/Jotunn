﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Jotunn.Entities;
using Jotunn.Utils;

namespace Jotunn.Configs
{
    /// <summary>
    ///     Configuration class for adding custom items. Automatically creates a recipe for this item.<br />
    ///     Use this in a constructor of <see cref="CustomItem"/> and 
    ///     Jötunn resolves the references to the game objects at runtime.
    /// </summary>
    public class ItemConfig
    {
        /// <summary>
        ///     The unique name for your item. May be tokenized.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     The description of your item. May be tokenized.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     The name of the item prefab. Is automatically set in <see cref="CustomItem"/>.
        /// </summary>
        public string Item { get; internal set; }

        /// <summary>
        ///     The amount of <see cref="Item"/> that will be created when crafting this item. Defaults to <c>1</c>.
        /// </summary>
        public int Amount { get; set; } = 1;

        /// <summary>
        ///     Whether this item is craftable or not. Defaults to <c>true</c>.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     The name of the piece table prefab this item uses to build pieces.
        /// </summary>
        public string PieceTable { get; set; } = string.Empty;

        /// <summary>
        ///     The name of the crafting station prefab where this recipe can be crafted.<br/>
        ///     Can be set to <c>null</c> to have the recipe be craftable without a crafting station.
        /// </summary>
        public string CraftingStation { get; set; } = string.Empty;

        /// <summary>
        ///     The name of the crafting station prefab where this item can be repaired.<br/>
        ///     Can be set to <c>null</c> to have the item be repairable without a crafting station.
        /// </summary>
        public string RepairStation { get; set; } = string.Empty;

        /// <summary>
        ///     The minimum required level for the crafting station. Defaults to <c>1</c>.
        /// </summary>
        public int MinStationLevel { get; set; } = 1;

        /// <summary>
        ///     Icons for this item. If more than one icon is added, this item automatically has variants.
        /// </summary>
        public Sprite[] Icons { get; set; } = null;

        /// <summary>
        ///     Texture holding the variants different styles.
        /// </summary>
        public Texture2D StyleTex { get; set; } = null;
        
        /// <summary>
        ///     Array of <see cref="RequirementConfig"/>s for all crafting materials it takes to craft the recipe.
        /// </summary>
        public RequirementConfig[] Requirements { get; set; } = Array.Empty<RequirementConfig>();

        /// <summary>
        ///     Apply this config's values to a GameObject's ItemDrop.
        /// </summary>
        /// <param name="prefab"></param>
        public void Apply(GameObject prefab)
        {
            var itemDrop = prefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                Logger.LogError($"GameObject has no ItemDrop attached");
                return;
            }

            var shared = itemDrop.m_itemData.m_shared;
            if (shared == null)
            {
                Logger.LogError($"ItemDrop has no SharedData component");
                return;
            }

            // Set the Item to the prefab name
            Item = prefab.name;

            // Set the name and description if provided
            if (!string.IsNullOrEmpty(Name))
            {
                shared.m_name = Name;
            }
            if (!string.IsNullOrEmpty(Description))
            {
                shared.m_description = Description;
            }

            // If there is still no m_name, add a token from the prefabs name
            if (string.IsNullOrEmpty(shared.m_name))
            {
                shared.m_name = $"${prefab.name}";
            }

            // Add a piece table if provided
            if (!string.IsNullOrEmpty(PieceTable))
            {
                shared.m_buildPieces = Mock<PieceTable>.Create(PieceTable);
            }

            // Set icons if provided
            if (Icons != null && Icons.Length > 0)
            {
                itemDrop.m_itemData.m_shared.m_icons = Icons;

                // Set variants if a StyleTex is provided
                if (StyleTex != null)
                {
                    ExtEquipment.Enable();
                    foreach (var rend in ShaderHelper.GetRenderers(prefab))
                    {
                        foreach (var mat in rend.materials)
                        {
                            if (mat.shader != Shader.Find("Custom/Creature"))
                            {
                                mat.shader = Shader.Find("Custom/Creature");
                            }

                            if (mat.HasProperty("_StyleTex"))
                            {
                                itemDrop.m_itemData.m_shared.m_variants = Icons.Length;
                                rend.gameObject.GetOrAddComponent<ItemStyle>();
                                mat.EnableKeyword("_USESTYLES_ON");
                                mat.SetFloat("_Style", 0f);
                                mat.SetFloat("_UseStyles", 1f);
                                mat.SetTexture("_StyleTex", StyleTex);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Converts the RequirementConfigs to Valheim style Piece.Requirements
        /// </summary>
        /// <returns>The Valheim Piece.Requirement array</returns>
        public Piece.Requirement[] GetRequirements()
        {
            Piece.Requirement[] reqs = new Piece.Requirement[Requirements.Length];

            for (int i = 0; i < reqs.Length; i++)
            {
                reqs[i] = Requirements[i].GetRequirement();
            }

            return reqs;
        }

        /// <summary>
        ///     Converts the ItemConfig to a Valheim style Recipe.
        /// </summary>
        /// <returns>The Valheim recipe</returns>
        public Recipe GetRecipe()
        {
            if (Item == null)
            {
                Logger.LogError($"No item set in item config");
                return null;
            }

            // If there are no requirements, don't create a recipe.
            if (Requirements == null || Requirements.Length == 0)
            {
                return null;
            }

            var recipe = ScriptableObject.CreateInstance<Recipe>();

            recipe.name = "Recipe_" + Item;
            recipe.m_item = Mock<ItemDrop>.Create(Item);
            recipe.m_amount = Amount;
            recipe.m_enabled = Enabled;

            if (!string.IsNullOrEmpty(CraftingStation))
            {
                recipe.m_craftingStation = Mock<CraftingStation>.Create(CraftingStation);
            }

            if (!string.IsNullOrEmpty(RepairStation))
            {
                recipe.m_repairStation = Mock<CraftingStation>.Create(RepairStation);
            }

            recipe.m_minStationLevel = MinStationLevel;
            recipe.m_resources = GetRequirements();

            return recipe;
        }

        /// <summary>
        ///     Loads a single ItemConfig from a JSON string
        /// </summary>
        /// <param name="json">JSON text</param>
        /// <returns>Loaded ItemConfig</returns>
        public static ItemConfig FromJson(string json)
        {
            return SimpleJson.SimpleJson.DeserializeObject<ItemConfig>(json);
        }

        /// <summary>
        ///     Loads a list of ItemConfigs from a JSON string
        /// </summary>
        /// <param name="json">JSON text</param>
        /// <returns>Loaded list of ItemConfigs</returns>
        public static List<ItemConfig> ListFromJson(string json)
        {
            return SimpleJson.SimpleJson.DeserializeObject<List<ItemConfig>>(json);
        }
    }
}
