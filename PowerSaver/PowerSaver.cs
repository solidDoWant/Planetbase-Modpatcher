using Planetbase;
using Redirection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace PowerSaver
{
    public class PowerSaver : IMod
    {
        public static string PRIORITY_LIST_PATH = @"Mods\Settings\PowerSaver.xml";
        public static string CONSOLE_ICON_PATH = @"Mods\Textures\GridManagementConsoleIcon.png";

        public static List<Type> DEFAULT_POWER_PRIORITY_LIST = new Type[]
        {
            typeof(ModuleTypeBasePad),
            typeof(ModuleTypeSignpost),
            typeof(ModuleTypeStarport),
            typeof(ModuleTypeLandingPad),
            typeof(ModuleTypeRadioAntenna),
            typeof(ModuleTypeStorage),
            typeof(ModuleTypeRoboticsFacility),
            typeof(ModuleTypeMine),
            typeof(ModuleTypeFactory),
            typeof(ModuleTypeProcessingPlant),
            typeof(ModuleTypeLab),
            typeof(ModuleTypeWaterTank),
            typeof(ModuleTypeBar),
            typeof(ModuleTypeMultiDome),
            typeof(ModuleTypeAntiMeteorLaser),
            typeof(ModuleTypeTelescope),
            typeof(ModuleTypeControlCenter),
            typeof(ModuleTypeDorm),
            typeof(ModuleTypeCabin),
            typeof(ModuleTypeSickBay),
            typeof(ModuleTypeCanteen),
            typeof(ModuleTypeBioDome),
            typeof(ModuleTypeAirlock),
            typeof(ModuleTypeOxygenGenerator),
            typeof(ModuleTypeWaterExtractor)
        }.ToList();

        public static List<Type> DEFAULT_WATER_PRIORITY_LIST = new Type[]
        {
            typeof(ModuleTypeLab),
            typeof(ModuleTypeBar),
            typeof(ModuleTypeMultiDome),
            typeof(ModuleTypeCanteen),
            typeof(ModuleTypeBioDome),
            typeof(ModuleTypeOxygenGenerator)
        }.ToList();

        public class SavingMode
        {
            public int trigger;
            public List<Type> typesToShutDown;

            public SavingMode(int trigger)
            {
                this.trigger = trigger;
                typesToShutDown = new List<Type>();
            }
        }

        public static List<SavingMode> mPowerSavingModes;
        public static List<SavingMode> mWaterSavingModes;
        public static SavingMode mActivePowerSavingMode;
        public static SavingMode mActiveWaterSavingMode;

        public static List<Type> mPowerPriorityList;
        public static List<Type> mWaterPriorityList;

        public void Init()
        {
            string settingsPath = Path.Combine(Util.getFilesFolder(), PRIORITY_LIST_PATH);
            string iconPath = Path.Combine(Util.getFilesFolder(), CONSOLE_ICON_PATH);
            if (!File.Exists(settingsPath))
            {
                Debug.Log("[MOD] PowerManager couldn't find the settings file.");
                return;
            }

            if (!File.Exists(iconPath))
            {
                Debug.Log("[MOD] PowerManager couldn't find the new console's icon.");
                return;
            }

            mPowerSavingModes = new List<SavingMode>();
            mWaterSavingModes = new List<SavingMode>();
            mPowerPriorityList = new List<Type>();
            mWaterPriorityList = new List<Type>();

            try
            {
                System.Reflection.Assembly gameAssembly = System.Reflection.Assembly.GetCallingAssembly();
                using (XmlReader reader = XmlReader.Create(settingsPath))
                {

                    // Read Power saving modes
                    reader.ReadToFollowing("PowerSavingModes");
                    XmlReader powerSavingModes = reader.ReadSubtree();
                    while (powerSavingModes.ReadToFollowing("SavingMode"))
                    {
                        powerSavingModes.MoveToFirstAttribute();
                        powerSavingModes.ReadAttributeValue();
                        if (!powerSavingModes.HasValue)
                            continue;

                        int trigger = Int32.Parse(powerSavingModes.Value);
                        if (powerSavingModes.ReadToFollowing("Module"))
                        {
                            SavingMode mode = new SavingMode(trigger);
                            do
                            {
                                Type type = gameAssembly.GetType("Planetbase.ModuleType" + powerSavingModes.ReadElementContentAsString(), false, true);
                                if (type != null)
                                    mode.typesToShutDown.Add(type);
                            } while (powerSavingModes.ReadToNextSibling("Module"));

                            mPowerSavingModes.Add(mode);
                        }
                    }

                    // Read water saving modes
                    reader.ReadToFollowing("WaterSavingModes");
                    XmlReader waterSavingModes = reader.ReadSubtree();
                    while (waterSavingModes.ReadToFollowing("SavingMode"))
                    {
                        waterSavingModes.MoveToFirstAttribute();
                        waterSavingModes.ReadAttributeValue();
                        if (!waterSavingModes.HasValue)
                            continue;

                        int trigger = Int32.Parse(waterSavingModes.Value);
                        if (waterSavingModes.ReadToFollowing("Module"))
                        {
                            SavingMode mode = new SavingMode(trigger);
                            do
                            {
                                Type type = gameAssembly.GetType("Planetbase.ModuleType" + waterSavingModes.ReadElementContentAsString(), false, true);
                                if (type != null)
                                    mode.typesToShutDown.Add(type);
                            } while (waterSavingModes.ReadToFollowing("Module"));

                            mWaterSavingModes.Add(mode);
                        }
                    }

                    // Read power priority list
                    reader.ReadToFollowing("PowerList");
                    XmlReader powerList = reader.ReadSubtree();
                    while (powerList.ReadToFollowing("Module"))
                    {
                        Type type = gameAssembly.GetType("Planetbase.ModuleType" + powerList.ReadElementContentAsString(), false, true);
                        if (type != null)
                            mPowerPriorityList.Add(type);
                    }

                    // Read water priority list
                    reader.ReadToFollowing("WaterList");
                    XmlReader waterList = reader.ReadSubtree();
                    while (waterList.ReadToFollowing("Module"))
                    {
                        Type type = gameAssembly.GetType("Planetbase.ModuleType" + waterList.ReadElementContentAsString(), false, true);
                        if (type != null)
                            mWaterPriorityList.Add(type);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("<MOD> PowerManager failed to load the settings file. Exception: " + e.Message);
                return;
            }

            mPowerSavingModes = mPowerSavingModes.OrderBy(m => m.trigger).ToList();
            mWaterSavingModes = mWaterSavingModes.OrderBy(m => m.trigger).ToList();
            mActivePowerSavingMode = null;
            mActiveWaterSavingMode = null;

            foreach (Type type in DEFAULT_POWER_PRIORITY_LIST)
            {
                if (!mPowerPriorityList.Contains(type))
                    mPowerPriorityList.Insert(0, type);
            }

            foreach (Type type in DEFAULT_WATER_PRIORITY_LIST)
            {
                if (!mWaterPriorityList.Contains(type))
                    mWaterPriorityList.Insert(0, type);
            }

            TypeList<ComponentType, ComponentTypeList>.getInstance().add(new GridManagementConsole());
            ModuleTypeControlCenter controlCenter = TypeList<ModuleType, ModuleTypeList>.find<ModuleTypeControlCenter>() as ModuleTypeControlCenter;
            List<ComponentType> components = controlCenter.mComponentTypes.ToList();
            components.Insert(3, TypeList<ComponentType, ComponentTypeList>.find<GridManagementConsole>());
            controlCenter.mComponentTypes = components.ToArray();

            Redirector.PerformRedirections();
            Debug.Log("[MOD] PowerSaver activated");
        }

        public void Update()
        {
            bool consoleExists = false;
            foreach (ConstructionComponent component in ConstructionComponent.mComponents)
            {
                bool lowCondition = component.mConditionIndicator.isValidValue() && component.mConditionIndicator.isExtremelyLow();
                if (component.getComponentType().GetType() == typeof(GridManagementConsole) && component.isBuilt() && !lowCondition && component.isEnabled() &&
                    component.mParentConstruction.isBuilt() && component.mParentConstruction.isEnabled() && !component.mParentConstruction.isExtremelyDamaged())
                {
                    consoleExists = true;
                    break;
                }
            }

            if (!consoleExists)
                return;

            Grid grid = Grid.getLargest();
            if (grid == null)
                return;

            if (mPowerSavingModes.Count > 0 && Module.getOverallPowerStorageCapacity() > 0f && (GetTotalConsumption(grid, GridResource.Power) > grid.getTotalPowerGeneration() || mActivePowerSavingMode != null))
            {
                float powerPercentage = (float)Module.getOverallPowerStorage() / Module.getOverallPowerStorageCapacity() * 100f;
                
                SavingMode newSavingMode = mPowerSavingModes.FirstOrDefault(m => powerPercentage <= m.trigger);
                if (newSavingMode != mActivePowerSavingMode)
                {
                    bool dontSwitch = false;
                    if (mActivePowerSavingMode != null && (newSavingMode == null || newSavingMode.trigger > mActivePowerSavingMode.trigger))
                        dontSwitch = powerPercentage < Mathf.Min(mActivePowerSavingMode.trigger * 1.2f, 100f);

                    if (!dontSwitch)
                        SwitchSavingMode(newSavingMode, GridResource.Power);
                }
            }

            if (mWaterSavingModes.Count > 0 && Module.getOverallWaterStorageCapacity() > 0f && (GetTotalConsumption(grid, GridResource.Water) > grid.getData(GridResource.Water).getGeneration() || mActiveWaterSavingMode != null))
            {
                float waterPercentage = (float)Module.getOverallWaterStorage() / Module.getOverallWaterStorageCapacity() * 100;

                SavingMode newSavingMode = mWaterSavingModes.FirstOrDefault(m => waterPercentage <= m.trigger);
                if (newSavingMode != mActiveWaterSavingMode)
                {
                    bool dontSwitch = false;
                    if (mActiveWaterSavingMode != null && (newSavingMode == null || newSavingMode.trigger > mActiveWaterSavingMode.trigger))
                        dontSwitch = waterPercentage < Mathf.Min(mActiveWaterSavingMode.trigger * 1.2f, 100f);

                    if (!dontSwitch)
                        SwitchSavingMode(newSavingMode, GridResource.Water);
                }
            }
        }

        public void SwitchSavingMode(SavingMode newSavingMode, GridResource resource)
        {
            Grid grid = Grid.getLargest();

            SavingMode currentSavingMode = resource == GridResource.Power ? mActivePowerSavingMode : mActiveWaterSavingMode;
            if (currentSavingMode != null)
            {
                // enable all types in this mode
                foreach (Type type in currentSavingMode.typesToShutDown)
                {
                    List<Construction> constructions = grid.mConstructions.Where(c => c is Module && (c as Module).mModuleType.GetType() == type).ToList();
                    foreach (Construction construction in constructions)
                    {
                        construction.setEnabled(true);
                    }
                }
            }

            if (newSavingMode != null)
            {
                // disable all types in the new mode
                bool skippedConsoleModule = false;
                foreach (Type type in newSavingMode.typesToShutDown)
                {
                    List<Construction> constructions = grid.mConstructions.Where(c => c is Module && (c as Module).mModuleType.GetType() == type).ToList();
                    foreach (Construction construction in constructions)
                    {
                        //if (module.mComponents.FirstOrDefault(c => c.mComponentType is GridManagementConsole) != null)
                        if (!skippedConsoleModule && type == typeof(ModuleTypeControlCenter) && construction.mComponents.FirstOrDefault(c => c.mComponentType is GridManagementConsole) != null)
                        {
                            skippedConsoleModule = true;
                            continue;
                        }
                        construction.setEnabled(false);
                    }
                }
            }

            if (resource == GridResource.Power)
                mActivePowerSavingMode = newSavingMode;
            else
                mActiveWaterSavingMode = newSavingMode;
        }

        public float GetTotalConsumption(Grid grid, GridResource resource)
        {
            float total = 0;
            foreach (Construction construction in grid.mConstructions)
            {
                if (construction.isBuilt() && !construction.isExtremelyDamaged())
                {
                    float generation = grid.getGeneration(construction, resource);
                    if (generation < 0f)
                        total -= generation;
                }
            }

            return total;
        }
    }

    public abstract class CustomGrid : Grid
    {
        [RedirectFrom(typeof(Grid))]
        public new void calculateBalance(GridResource gridResource)
        {
            bool consoleExists = false;
            foreach (ConstructionComponent component in ConstructionComponent.mComponents)
            {
                bool lowCondition = component.mConditionIndicator.isValidValue() && component.mConditionIndicator.isExtremelyLow();
                if (component.getComponentType().GetType() == typeof(GridManagementConsole) && component.isBuilt() && !lowCondition && component.isEnabled() &&
                    component.mParentConstruction.isBuilt() && component.mParentConstruction.isEnabled() && !component.mParentConstruction.isExtremelyDamaged())
                {
                    consoleExists = true;
                    break;
                }
            }

            if (consoleExists)
                advancedCalculateBalance(gridResource);
            else
                basicCalculateBalance(gridResource);
        }

        public void advancedCalculateBalance(GridResource gridResource)
        {
            Dictionary<Type, List<Module>> constructionsByType = new Dictionary<Type, List<Module>>();
            float resourceBalance = 0f;
            float amountCreated = 0f;
            float amountConsumed = 0f;

            foreach (Construction construction in mConstructions)
            {
                if (construction.isBuilt() && construction.isEnabled() && !construction.isExtremelyDamaged())
                {
                    // amountGenerated can be either created or consumed
                    float amountGenerated = getGeneration(construction, gridResource);
                    resourceBalance += amountGenerated;

                    if (amountGenerated > 0f)
                        amountCreated += amountGenerated;
                    else
                        amountConsumed -= amountGenerated;

                    setResourceAvailable(construction, gridResource, true);

                    if (construction is Connection)
                        continue;

                    Module module = construction as Module;
                    List<Module> list;
                    if (constructionsByType.TryGetValue(module.mModuleType.GetType(), out list))
                    {
                        list.Add(module);
                    }
                    else
                    {
                        list = new List<Module>();
                        list.Add(module);
                        constructionsByType[module.mModuleType.GetType()] = list;
                    }
                }
                else
                {
                    setResourceAvailable(construction, gridResource, false);
                }
            }

            GridResourceData resourceData = getData(gridResource);
            resourceData.setCollector(findCollector(gridResource, resourceBalance));
            resourceData.setBalance(resourceBalance);
            resourceData.setGeneration(amountCreated);
            resourceData.setConsumption(amountConsumed);

            if (resourceBalance < 0f && resourceData.getCollector() == null)
            {
                Module consoleModule = null;
                List<Type> priorityList = gridResource == GridResource.Power ? PowerSaver.mPowerPriorityList : PowerSaver.mWaterPriorityList;
                foreach (Type type in priorityList)
                {
                    List<Module> constructions;
                    if (constructionsByType.TryGetValue(type, out constructions))
                    {
                        foreach (Module module in constructions)
                        {
                            if (consoleModule == null && module.mModuleType is ModuleTypeControlCenter)
                            {
                                if (module.mComponents.FirstOrDefault(c => c.mComponentType is GridManagementConsole) != null)
                                {
                                    consoleModule = module;
                                    continue;
                                }
                            }

                            float generation = getGeneration(module, gridResource);
                            if (generation < 0f && module.isEnabled())
                            {
                                resourceBalance -= generation;
                                setResourceAvailable(module, gridResource, false);

                                if (resourceBalance > 0f)
                                    return;

                                foreach (Construction connection in module.getLinks())
                                {
                                    if (isResourceAvailable(connection, gridResource))
                                    {
                                        generation = getGeneration(connection, gridResource);
                                        resourceBalance -= generation;
                                        setResourceAvailable(connection, gridResource, false);

                                        if (resourceBalance > 0f)
                                            return;
                                    }
                                }
                            }
                        }
                    }
                }

                // if we reach this point, we still don't have a positive balance
                // and the only module active is the control center with the grid management console
                if (consoleModule != null)
                    setResourceAvailable(consoleModule, gridResource, false);
            }
        }

        public void basicCalculateBalance(GridResource gridResource)
        {
            HashSet<Construction> constructionsLackingResource = new HashSet<Construction>();
            GridResourceData resourceData = this.getData(gridResource);
            float resourceBalance = 0f;
            float amountCreated = 0f;
            float amountConsumed = 0f;

            foreach (Construction construction in mConstructions)
            {
                if (construction.isBuilt() && construction.isEnabled() && !construction.isExtremelyDamaged())
                {
                    // amountGenerated can be either created or consumed
                    float amountGenerated = getGeneration(construction, gridResource);
                    resourceBalance += amountGenerated;

                    if (amountGenerated > 0f)
                        amountCreated += amountGenerated;
                    else
                        amountConsumed -= amountGenerated;

                    if (!isResourceAvailable(construction, gridResource))
                        constructionsLackingResource.Add(construction);

                    setResourceAvailable(construction, gridResource, true);
                }
                else
                {
                    setResourceAvailable(construction, gridResource, false);
                }
            }

            // if resourceBalance is positive, returns the first collector that is not full.
            // Otherwise, returns the first collector that has available resource
            Construction collector = findCollector(gridResource, resourceBalance);

            if (resourceBalance < 0f && collector == null)
            {
                HashSet<Construction> constructionsToShutDown = new HashSet<Construction>();
                foreach (Construction construction in mConstructions)
                {
                    if (getGeneration(construction, gridResource) < 0f && construction.isEnabled())
                    {
                        setResourceAvailable(construction, gridResource, false);
                        if (!constructionsToShutDown.Contains(construction))
                            constructionsToShutDown.Add(construction);
                    }
                }

                float amountAvailable = amountCreated;
                bool somethingChanged = true;
                while (somethingChanged)
                {
                    somethingChanged = false;
                    foreach (Construction construction in mConstructions)
                    {
                        if (getGeneration(construction, gridResource) < 0f && construction.isEnabled() && !isResourceAvailable(construction, gridResource))
                        {
                            foreach (Construction linkedConstruction in construction.getLinks())
                            {
                                if (linkedConstruction.isPowered() || getGeneration(linkedConstruction, gridResource) > 0f)
                                {
                                    float absAmountUsed = -getGeneration(construction, gridResource);
                                    if (absAmountUsed < amountAvailable)
                                    {
                                        setResourceAvailable(construction, gridResource, true);
                                        if (constructionsToShutDown.Contains(construction))
                                        {
                                            constructionsToShutDown.Remove(construction);
                                        }
                                        amountAvailable -= absAmountUsed;
                                        somethingChanged = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                if ((gridResource == GridResource.Water && !MessageLog.getInstance().contains(Message.StructuresNoWater)) ||
                    (gridResource == GridResource.Power && !MessageLog.getInstance().contains(Message.StructuresNoPower)))
                {
                    constructionsToShutDown.UnionWith(constructionsLackingResource);
                    addMessage(constructionsToShutDown, gridResource);
                }
            }

            resourceData.setCollector(collector);
            resourceData.setBalance(resourceBalance);
            resourceData.setGeneration(amountCreated);
            resourceData.setConsumption(amountConsumed);
        }
    }

    public class GridManagementConsole : ComponentType
    {
        public static string NAME = "Grid Management Console";
        public static string DESCRIPTION = @"An Engineer can use this console to control the resource distribution in case of shortage. This will prevent your most vital modules from shutting down while there are non-vital modules still operating.";

        public GridManagementConsole()
        {
            this.mConstructionCosts = new ResourceAmounts();
            this.mConstructionCosts.add(ResourceTypeList.MetalInstance, 1);
            this.mConstructionCosts.add(ResourceTypeList.BioplasticInstance, 1);
            base.addUsageAnimation(CharacterAnimationType.WorkSeated, CharacterProp.Count, CharacterProp.Count);
            this.mOperatorSpecialization = TypeList<Specialization, SpecializationList>.find<Engineer>();
            this.mFlags = 264;
            this.mSurveyedConstructionCount = 20;
            this.mPrefabName = "PrefabRadioConsole";

            string language = Profile.getInstance().getLanguage();
            if (language == "en")
            {
                StringList.mStrings.Add("component_" + Util.camelCaseToLowercase(this.GetType().Name), NAME);
                StringList.mStrings.Add("tooltip_" + Util.camelCaseToLowercase(this.GetType().Name), DESCRIPTION);
            }
            // this is needed because the game doesn't use the fallback strings for tooltips
            else if (!StringList.exists("tooltip_" + Util.camelCaseToLowercase(this.GetType().Name)))
            {
                StringList.mStrings.Add("tooltip_" + Util.camelCaseToLowercase(this.GetType().Name), DESCRIPTION);
            }
            StringList.mFallbackStrings.Add("component_" + Util.camelCaseToLowercase(this.GetType().Name), NAME);
            StringList.mFallbackStrings.Add("tooltip_" + Util.camelCaseToLowercase(this.GetType().Name), DESCRIPTION);

            this.initStrings();

            string iconPath = Path.Combine(Util.getFilesFolder(), PowerSaver.CONSOLE_ICON_PATH);
            if (File.Exists(iconPath))
            {
                byte[] iconBytes = File.ReadAllBytes(iconPath);
                Texture2D tex = new Texture2D(0, 0);
                tex.LoadImage(iconBytes);
                this.mIcon = Util.applyColor(tex);
            }
        }
    }
}
