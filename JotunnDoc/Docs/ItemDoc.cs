﻿using System;
using Jotunn.Managers;
using UnityEngine;

namespace JotunnDoc.Docs
{
    public class ItemDoc : Doc
    {
        public ItemDoc() : base("JotunnDoc/Docs/conceptual/objects/item-list.md")
        {
            ItemManager.OnItemsRegistered += DocItems;
        }

        private void DocItems()
        {
            Debug.Log("Documenting items");

            AddHeader(1, "Item list");
            AddText("All of the items currently in the game, with English localizations applied");
            AddText("This file is automatically generated from Valheim using the JotunnDoc mod found on our GitHub.");
            AddTableHeader("Prefab", "Name", "Type", "Description");

            foreach (GameObject obj in ObjectDB.instance.m_items)
            {
                ItemDrop item = obj.GetComponent<ItemDrop>();
                ItemDrop.ItemData.SharedData shared = item.m_itemData.m_shared;

                AddTableRow(
                    obj.name,
                    JotunnDoc.Localize(shared.m_name),
                    shared.m_itemType.ToString(),
                    JotunnDoc.Localize(shared.m_description));
            }

            Save();
            Debug.Log("\t-> Done");
        }
    }
}
