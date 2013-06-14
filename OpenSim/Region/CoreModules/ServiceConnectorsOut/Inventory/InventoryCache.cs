﻿using System;
using System.Collections.Generic;
using System.Threading;
using OpenSim.Framework;
using OpenMetaverse;

namespace OpenSim.Region.CoreModules.ServiceConnectorsOut.Inventory
{
    /// <summary>
    /// Cache root and system inventory folders to reduce number of potentially remote inventory calls and associated holdups.
    /// </summary>
    public class InventoryCache
    {
        private const double CACHE_EXPIRATION_SECONDS = 3600.0; // 1 hour

        private static ExpiringCache<UUID, InventoryFolderBase> m_RootFolders = new ExpiringCache<UUID, InventoryFolderBase>();
        private static ExpiringCache<UUID, Dictionary<AssetType, InventoryFolderBase>> m_FolderTypes = new ExpiringCache<UUID, Dictionary<AssetType, InventoryFolderBase>>();
        private static ExpiringCache<UUID, InventoryCollection> m_Inventories = new ExpiringCache<UUID, InventoryCollection>();

        public void Cache(UUID userID, InventoryFolderBase root)
        {
            m_RootFolders.AddOrUpdate(userID, root, CACHE_EXPIRATION_SECONDS);
        }

        public InventoryFolderBase GetRootFolder(UUID userID)
        {
            InventoryFolderBase root = null;
            if (m_RootFolders.TryGetValue(userID, out root))
                return root;

            return null;
        }

        public void Cache(UUID userID, AssetType type, InventoryFolderBase folder)
        {
            Dictionary<AssetType, InventoryFolderBase> ff = null;
            if (!m_FolderTypes.TryGetValue(userID, out ff))
            {
                ff = new Dictionary<AssetType, InventoryFolderBase>();
                m_FolderTypes.Add(userID, ff, CACHE_EXPIRATION_SECONDS);
            }

            // We need to lock here since two threads could potentially retrieve the same dictionary
            // and try to add a folder for that type simultaneously.  Dictionary<>.Add() is not described as thread-safe in the SDK
            // even if the folders are identical.
            lock (ff)
            {
                if (!ff.ContainsKey(type))
                    ff.Add(type, folder);
            }
        }

        public InventoryFolderBase GetFolderForType(UUID userID, AssetType type)
        {
            Dictionary<AssetType, InventoryFolderBase> ff = null;
            if (m_FolderTypes.TryGetValue(userID, out ff))
            {
                InventoryFolderBase f = null;

                lock (ff)
                {
                    if (ff.TryGetValue(type, out f))
                        return f;
                }
            }

            return null;
        }

        public void Cache(UUID userID, InventoryCollection inv)
        {
            m_Inventories.AddOrUpdate(userID, inv, 120);
        }

        public InventoryCollection GetUserInventory(UUID userID)
        {
            InventoryCollection inv = null;
            if (m_Inventories.TryGetValue(userID, out inv))
                return inv;
            return null;
        }

        public InventoryCollection GetFolderContent(UUID userID, UUID folderID)
        {
            InventoryCollection inv = null;
            InventoryCollection c;
            if (m_Inventories.TryGetValue(userID, out inv))
            {
                c = new InventoryCollection();
                c.UserID = userID;

                c.Folders = inv.Folders.FindAll(delegate(InventoryFolderBase f)
                {
                    return f.ParentID == folderID;
                });
                c.Items = inv.Items.FindAll(delegate(InventoryItemBase i)
                {
                    return i.Folder == folderID;
                });
                return c;
            }
            return null;
        }

        public List<InventoryItemBase> GetFolderItems(UUID userID, UUID folderID)
        {
            InventoryCollection inv = null;
            if (m_Inventories.TryGetValue(userID, out inv))
            {
                List<InventoryItemBase> items = inv.Items.FindAll(delegate(InventoryItemBase i)
                {
                    return i.Folder == folderID;
                });
                return items;
            }
            return null;
        }
    }
}
