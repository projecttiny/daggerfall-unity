﻿// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2018 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Utility;

namespace DaggerfallWorkshop.Game.MagicAndEffects
{
    /// <summary>
    /// Interface to an entity effect.
    /// </summary>
    public interface IEntityEffect : IMacroContextProvider
    {
        /// <summary>
        /// Gets effect properties.
        /// </summary>
        EffectProperties Properties { get; }
        
        /// <summary>
        /// Gets or sets current effect settings.
        /// </summary>
        EffectSettings Settings { get; set; }

        /// <summary>
        /// Gets key from properties.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets display name from properties or construct one from Group+SubGroup text in properties.
        /// This allows effects to set a custom display name or just roll with automatic names.
        /// Daggerfall appears to use first token of spellmaker/spellbook description, but we want more control for effect mods.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets number of magic rounds remaining.
        /// </summary>
        int RoundsRemaining { get; }

        /// <summary>
        /// Gets array DaggerfallStats.Count items wide.
        /// Array items represent Strength, Intelligence, Willpower, etc.
        /// Effect implementation should set modifier values for stats when part of payload.
        /// For example, a "Damage Strength" effect would set the current modifier for Strength (such as -5 to Strength).
        /// Use (int)DFCareer.State.StatName to get index.
        /// </summary>
        int[] StatMods { get; }

        /// <summary>
        /// Get array DaggerfallSkills.Count items wide.
        /// Array items represent Medical, Etiquette, Streetwise, etc.
        /// Effect implementation should set modifier values for skills when part of payload.
        /// For example, a "Tongues" effect would set the current modifier for all language skills (such as +5 to Dragonish, +5 to Giantish, and so on).
        /// Use (int)DFCareer.Skills.SkillName to get index.
        /// </summary>
        int[] SkillMods { get; }

        /// <summary>
        /// Called to assign total lifetime of effect in magic rounds.
        /// If no caster is specified in bundle then duration will default to caster level 1.
        /// If duration not supported then effect will persist for 1 magic round only.
        /// </summary>
        void SetDuration(DaggerfallEntityBehaviour caster = null);

        /// <summary>
        /// Called by an EntityEffectManager when parent bundle is attached to an entity.
        /// Use this for setup or immediate work performed only once.
        /// </summary>
        void Start();

        /// <summary>
        /// Called when bundle lifetime is at an end.
        /// Use this for any wrap-up work.
        /// </summary>
        void End();

        /// <summary>
        /// Use this for any work performed every magic round.
        /// If no caster specified then effect will default to caster level 1.
        /// </summary>
        void MagicRound(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null);

        /// <summary>
        /// Removes a magic round from total lifetime.
        /// </summary>
        /// <returns>Total number of magic rounds remaining. 0 means effect is expired.</returns>
        int RemoveRound();
    }

    /// <summary>
    /// Base implementation of an entity effect.
    /// Entity effects are like "actions" for spells, potions, items, advantages, diseases, etc.
    /// They generally perform work against one or more entities (e.g. damage or restore health).
    /// Some effects perform highly custom operations unique to player (e.g. anchor/teleport UI).
    /// Magic effects are scripted in C# so they have full access to engine and UI as required.
    /// Classic magic effects are included in build for cross-platform compatibility.
    /// Custom effects can be added later using mod system (todo:).
    /// </summary>
    public abstract partial class BaseEntityEffect : IEntityEffect
    {
        #region Fields

        protected EffectProperties properties = new EffectProperties();
        protected EffectSettings settings = new EffectSettings();
        protected int[] statMods = new int[DaggerfallStats.Count];
        protected int[] skillMods = new int[DaggerfallSkills.Count];

        int roundsRemaining;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BaseEntityEffect()
        {
            // Set default properties
            properties.SupportDuration = false;
            properties.SupportChance = false;
            properties.SupportMagnitude = false;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = ElementTypes.Magic;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.None;

            // Set default settings
            settings = GetDefaultSettings();

            // Allow effect to set own properties
            SetProperties();
        }

        #endregion

        #region IEntityEffect Properties

        public EffectProperties Properties
        {
            get { return properties; }
        }

        public EffectSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public int RoundsRemaining
        {
            get { return roundsRemaining; }
        }

        public int[] StatMods
        {
            get { return statMods; }
        }

        public int[] SkillMods
        {
            get { return skillMods; }
        }

        public string Key
        {
            get { return properties.Key; }
        }

        public string DisplayName
        {
            get { return GetDisplayName(); }
        }

        #endregion

        #region IEntityEffect Virtual Methods

        public abstract void SetProperties();

        public virtual void SetDuration(DaggerfallEntityBehaviour caster = null)
        {
            if (caster == null)
                Debug.LogWarningFormat("SetDuration() for {0} has no caster.", properties.Key);

            if (properties.SupportDuration)
            {
                int casterLevel = (caster) ? caster.Entity.Level : 1;
                roundsRemaining = settings.DurationBase + settings.DurationPlus * (int)Mathf.Floor(casterLevel / settings.DurationPerLevel);
            }
            else
            {
                roundsRemaining = 1;
            }

            //Debug.LogFormat("Effect '{0}' will run for {1} magic rounds", Key, roundsRemaining);
        }

        public virtual void Start()
        {
        }

        public virtual void End()
        {
        }

        public virtual void MagicRound(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
        }

        public virtual int RemoveRound()
        {
            if (roundsRemaining == 0)
            {
                return 0;
            }
            else
            {
                roundsRemaining--;
                if (roundsRemaining == 0)
                    End();

                return roundsRemaining;
            }
        }

        #endregion

        #region Protected Helpers

        protected DaggerfallEntityBehaviour GetPeeredEntityBehaviour(EntityEffectManager manager)
        {
            if (manager == null)
                return null;

            return manager.GetComponent<DaggerfallEntityBehaviour>();
        }

        protected int GetMagnitude(DaggerfallEntityBehaviour caster = null)
        {
            if (caster == null)
                Debug.LogWarningFormat("GetMagnitude() for {0} has no caster.", properties.Key);

            int magnitude = 0;
            if (properties.SupportMagnitude)
            {
                int casterLevel = (caster) ? caster.Entity.Level : 1;
                int baseMagnitude = Random.Range(settings.MagnitudeBaseMin, settings.MagnitudeBaseMax + 1);
                int plusMagnitude = Random.Range(settings.MagnitudePlusMin, settings.MagnitudePlusMax + 1);
                int multiplier = (int)Mathf.Floor(casterLevel / settings.MagnitudePerLevel);
                magnitude = baseMagnitude + plusMagnitude * multiplier;
            }

            return magnitude;
        }

        #endregion

        #region Private Methods

        // Applies default settings when not specified
        EffectSettings GetDefaultSettings()
        {
            EffectSettings defaultSettings = new EffectSettings();

            // Default duration is 1 + 1 per level
            defaultSettings.DurationBase = 1;
            defaultSettings.DurationPlus = 1;
            defaultSettings.DurationPerLevel = 1;

            // Default chance is 1 + 1 per level
            defaultSettings.ChanceBase = 1;
            defaultSettings.ChancePlus = 1;
            defaultSettings.ChancePerLevel = 1;

            // Default magnitude is 1-1 + 1-1 per level
            defaultSettings.MagnitudeBaseMin = 1;
            defaultSettings.MagnitudeBaseMax = 1;
            defaultSettings.MagnitudePlusMin = 1;
            defaultSettings.MagnitudePlusMax = 1;
            defaultSettings.MagnitudePerLevel = 1;

            return defaultSettings;
        }

        string GetDisplayName()
        {
            // Get display name or manufacture a default from group names
            if (!string.IsNullOrEmpty(properties.DisplayName))
            {
                return properties.DisplayName;
            }
            else
            {
                if (!string.IsNullOrEmpty(properties.GroupName) && !string.IsNullOrEmpty(properties.SubGroupName))
                    return properties.DisplayName = string.Format("{0} {1}", properties.GroupName, properties.SubGroupName);
                else if (!string.IsNullOrEmpty(properties.GroupName) && string.IsNullOrEmpty(properties.SubGroupName))
                    return properties.DisplayName = properties.GroupName;
                else
                    return properties.DisplayName = TextManager.Instance.GetText("ClassicEffect", "noName");
            }
        }

        #endregion

        #region Static Methods

        public static int MakeClassicKey(byte groupIndex, byte subgroupIndex)
        {
            return groupIndex << 8 + subgroupIndex;
        }

        public static void ReverseClasicKey(int key, out byte groupIndex, out byte subgroupIndex)
        {
            groupIndex = (byte)(key >> 8);
            subgroupIndex = (byte)(key & 0xff);
        }

        public static EffectCosts MakeEffectCosts(float costA, float costB, float factor = 1, float offsetGold = 0)
        {
            EffectCosts costs = new EffectCosts();
            costs.OffsetGold = offsetGold;
            costs.Factor = factor;
            costs.CostA = costA;
            costs.CostB = costB;

            return costs;
        }

        #endregion
    }
}