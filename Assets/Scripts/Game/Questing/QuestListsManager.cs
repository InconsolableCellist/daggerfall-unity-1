// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2018 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Hazelnut
// Contributors:    

using System.Collections.Generic;
using System;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using System.IO;
using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace DaggerfallWorkshop.Game.Questing
{
    public enum MembershipStatus
    {
        Nonmember = 'N',
        Member   = 'M',
        Prospect = 'P',
        Akatosh  = 'T',
        Arkay    = 'A',
        Dibella  = 'D',
        Julianos = 'J',
        Kynareth = 'K',
        Mara     = 'R',
        Stendarr = 'S',
        Zenithar = 'Z',
    }

    struct QuestData
    {
        public string name;
        public string path;
        public string group;
        public char membership;
        public int minRep;
        public bool unitWildC;
    }

    /// <summary>
    /// Manager class for Quest Lists and Quest scripts
    /// 
    /// Quest lists are tables of quest names and metadata.
    /// They are discovered and loaded at startup time. (although loaded at runtime in editor)
    /// The files must be named: QuestList-{name}.txt
    /// 
    /// Quest scripts sit alongside list and must be uniquely named. They are loaded at runtime.
    /// </summary>
    public class QuestListsManager
    {
        public const string InitAtGameStart = "InitAtGameStart";
        public const string QuestListPrefix = "QuestList-";
        private const string QuestListPattern = QuestListPrefix + "*.txt";
        private const string QuestPacksFolderName = "QuestPacks";

        // Quest data tables
        private Dictionary<FactionFile.GuildGroups, List<QuestData>> guilds;
        private Dictionary<FactionFile.SocialGroups, List<QuestData>> social;
        private List<QuestData> init;

        // Registered quest lists
        private static List<string> questLists = new List<string>();

        // Constructor discovers and loads lists.
        public QuestListsManager()
        {
            DiscoverQuestPackLists();
            LoadQuestLists();
        }

        #region Quest Packs

        /// <summary>
        /// Gets Quest Packs folder in StreamingAssets.
        /// </summary>
        public string QuestPacksFolder
        {
            get { return Path.Combine(Application.streamingAssetsPath, QuestPacksFolderName); }
        }

        public void DiscoverQuestPackLists()
        {
            string[] listFiles = Directory.GetFiles(QuestPacksFolder, QuestListPattern, SearchOption.AllDirectories);
            foreach (string listFile in listFiles)
                if (!RegisterQuestList(listFile))
                    Debug.LogErrorFormat("QuestList already registered. {0}", listFile);
        }

        #endregion

        #region Quest Lists

        /// <summary>
        /// Register a quest list contained in a mod. Only pass the name of the list, not the full filename.
        /// </summary>
        public static bool RegisterQuestList(string name)
        {
            DaggerfallUnity.LogMessage("RegisterQuestList: " + name, true);
            if (questLists.Contains(name))
                return false;
            else
                questLists.Add(name);
            return true;
        }

        /// <summary>
        /// Loads all the quest lists: default, discovered and registered.
        /// </summary>
        public void LoadQuestLists()
        {
            guilds = new Dictionary<FactionFile.GuildGroups, List<QuestData>>();
            social = new Dictionary<FactionFile.SocialGroups, List<QuestData>>();
            init = new List<QuestData>();

            LoadQuestList(QuestListPrefix + "Classic", QuestMachine.QuestSourceFolder);
            LoadQuestList(QuestListPrefix + "DFU", QuestMachine.QuestSourceFolder);

            foreach (string questList in questLists)
                LoadQuestList(questList);
        }

        private void LoadQuestList(string questList)
        {
            // Attempt to load quest pack quest list
            if (File.Exists(questList))
            {
                string questsPath = questList.Substring(0, questList.LastIndexOf(Path.DirectorySeparatorChar));
                LoadQuestList(questList, questsPath);
            }
            else
            {
                // Seek from mods using pattern: QuestList-<packName>.txt
                TextAsset questListAsset;
                string fileName = QuestListPrefix + questList + ".txt";
                if (ModManager.Instance != null && ModManager.Instance.TryGetAsset(fileName, false, out questListAsset))
                {
                    List<string> lines = ModManager.GetTextAssetLines(questListAsset);
                    ParseQuestList(new Table(lines.ToArray()));
                }
                else
                {
                    Debug.LogErrorFormat("QuestList {0} not found in a mod or in quest packs folder.", questList);
                }
            }
        }

        private void LoadQuestList(string questListFilename, string questsPath)
        {
            Table table = new Table(QuestMachine.Instance.GetTableSourceText(questListFilename));
            ParseQuestList(table, questsPath);
        }

        private void ParseQuestList(Table questsTable, string questsPath = "")
        {
            for (int i = 0; i < questsTable.RowCount; i++)
            {
                QuestData questData = new QuestData();
                questData.path = questsPath;
                string minRep = questsTable.GetValue("minRep", i);
                if (minRep.EndsWith("X"))
                {
                    questData.unitWildC = true;
                    minRep = minRep.Replace("X", "0");
                }
                int d = 0;
                if (int.TryParse(minRep, out d))
                {
                    questData.name = questsTable.GetValue("name", i);
                    questData.group = questsTable.GetValue("group", i);
                    questData.membership = questsTable.GetValue("membership", i)[0];
                    questData.minRep = d;

                    // Is the group a guild group?
                    if (Enum.IsDefined(typeof(FactionFile.GuildGroups), questData.group))
                    {
                        FactionFile.GuildGroups guildGroup = (FactionFile.GuildGroups) Enum.Parse(typeof(FactionFile.GuildGroups), questData.group);
                        List<QuestData> guildQuests;
                        if (!guilds.TryGetValue(guildGroup, out guildQuests))
                        {
                            guildQuests = new List<QuestData>();
                            guilds.Add(guildGroup, guildQuests);
                        }
                        guildQuests.Add(questData);
                    }
                    // Is the group a social group?
                    else if (Enum.IsDefined(typeof(FactionFile.SocialGroups), questData.group))
                    {
                        FactionFile.SocialGroups socialGroup = (FactionFile.SocialGroups) Enum.Parse(typeof(FactionFile.SocialGroups), questData.group);
                        List<QuestData> socialQuests;
                        if (!social.TryGetValue(socialGroup, out socialQuests))
                        {
                            socialQuests = new List<QuestData>();
                            social.Add(socialGroup, socialQuests);
                        }
                        socialQuests.Add(questData);
                    }
                    // Is this a quest initialised when a new game is started?
                    else if (questData.group == InitAtGameStart)
                    {
                        init.Add(questData);
                    }
                    // else TODO other groups
                }
            }
        }

        #endregion

        #region Quest Loading

        /// <summary>
        /// Initialises and starts any quests marked InitAtGameStart
        /// </summary>
        public void InitAtGameStartQuests()
        {
            foreach (QuestData questData in init)
            {
                Quest quest = LoadQuest(questData, 0);
                if (quest == null)
                    continue;

                QuestMachine.Instance.InstantiateQuest(quest);
            }
        }

        /// <summary>
        /// Get a random quest for a guild from appropriate subset.
        /// </summary>
        public Quest GetGuildQuest(FactionFile.GuildGroups guildGroup, MembershipStatus status, int factionId, int rep)
        {
#if UNITY_EDITOR    // Reload every time when in editor
            LoadQuestLists();
#endif
            List<QuestData> guildQuests;
            if (guilds.TryGetValue(guildGroup, out guildQuests))
            {
                // Modifications for Temple dual membership status
                MembershipStatus tplMemb = (guildGroup == FactionFile.GuildGroups.HolyOrder && status != MembershipStatus.Nonmember) ? MembershipStatus.Member : status;
                // Underworld guilds don't expel and continue to offer std quests below zero reputation
                rep = ((guildGroup == FactionFile.GuildGroups.DarkBrotherHood || guildGroup == FactionFile.GuildGroups.GeneralPopulace) && rep < 0) ? 0 : rep;

                List<QuestData> pool = new List<QuestData>();
                foreach (QuestData quest in guildQuests)
                {
                    if ((status == (MembershipStatus)quest.membership || tplMemb == (MembershipStatus)quest.membership) &&
                        (status == MembershipStatus.Nonmember || (rep >= quest.minRep && (!quest.unitWildC || rep < quest.minRep + 10))))
                    {
                        pool.Add(quest);
                    }
                }
                return SelectQuest(pool, factionId);
            }
            return null;
        }

        public Quest GetSocialQuest(FactionFile.SocialGroups socialGroup, int factionId, int rep)
        {
#if UNITY_EDITOR    // Reload every time when in editor
            LoadQuestLists();
#endif
            List<QuestData> socialQuests;
            if (social.TryGetValue(socialGroup, out socialQuests))
            {
                List<QuestData> pool = new List<QuestData>();
                foreach (QuestData quest in socialQuests)
                {
                    if (rep >= quest.minRep && (!quest.unitWildC || rep < quest.minRep + 10))
                    {
                        pool.Add(quest);
                    }
                }
                return SelectQuest(pool, factionId);
            }
            return null;
        }

        private Quest SelectQuest(List<QuestData> pool, int factionId)
        {
            Debug.Log("Quest pool has " + pool.Count);
            // Choose random quest from pool and try to parse it
            if (pool.Count > 0)
            {
                QuestData questData = pool[UnityEngine.Random.Range(0, pool.Count)];
                try
                {
                    return LoadQuest(questData, factionId);
                }
                catch (Exception ex)
                {   // Log exception
                    DaggerfallUnity.LogMessage("Exception for quest " + questData.name + " during quest compile: " + ex.Message, true);
                }
            }
            return null;
        }

        private Quest LoadQuest(QuestData questData, int factionId)
        {
            // Append extension if not present
            string questName = questData.name;
            if (!questName.EndsWith(".txt"))
                questName += ".txt";

            // Attempt to load quest source file
            Quest quest;
            string questFile = Path.Combine(questData.path, questName);
            if (File.Exists(questFile))
            {
                quest = QuestMachine.Instance.ParseQuest(questName, File.ReadAllLines(questFile));
                if (quest == null)
                    return null;
            }
            else
            {
                // Seek from mods using name
                TextAsset questAsset;
                if (ModManager.Instance != null && ModManager.Instance.TryGetAsset(questName, false, out questAsset))
                {
                    List<string> lines = ModManager.GetTextAssetLines(questAsset);
                    quest = QuestMachine.Instance.ParseQuest(questName, lines.ToArray());
                    if (quest == null)
                        return null;
                }
                else
                    throw new Exception("Quest file " + questFile + " not found.");
            }
            quest.FactionId = factionId;
            return quest;
        }

        #endregion
    }
}