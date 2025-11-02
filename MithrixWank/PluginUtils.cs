using RoR2;

namespace MithrixWank
{
    internal class PluginUtils
    {
        internal enum MonsterCategories
        {
            BasicMonsters, Minibosses, Champions
        }

        internal static DirectorCard BuildDirectorCard(CharacterSpawnCard spawnCard)
        {
            return BuildDirectorCard(spawnCard, 1, 0, DirectorCore.MonsterSpawnDistance.Standard);
        }

        internal static DirectorCard BuildDirectorCard(CharacterSpawnCard spawnCard, int weight, int minStages, DirectorCore.MonsterSpawnDistance spawnDistance)
        {
            DirectorCard dc = new DirectorCard
            {
                spawnCard = spawnCard,
                selectionWeight = weight,
                preventOverhead = false,
                minimumStageCompletions = minStages,
                spawnDistance = spawnDistance,
                forbiddenUnlockableDef = null,
                requiredUnlockableDef = null
            };
            return dc;
        }

        internal static bool AddMonsterDirectorCardToCategory(DirectorCardCategorySelection categorySelection, DirectorCard directorCard, MonsterCategories monsterCategory)
        {
            int categoryIndex = FindCategoryIndexByName(categorySelection, monsterCategory);
            if (categoryIndex >= 0)
            {
                categorySelection.AddCard(categoryIndex, directorCard);
                return true;
            }
            return false;
        }

        //Minibosses
        //Basic Monsters
        //Champions
        internal static int FindCategoryIndexByName(DirectorCardCategorySelection dcs, MonsterCategories category)
        {
            string categoryName;
            switch (category)
            {
                case MonsterCategories.BasicMonsters:
                    categoryName = "Basic Monsters";
                    break;
                case MonsterCategories.Minibosses:
                    categoryName = "Minibosses";
                    break;
                case MonsterCategories.Champions:
                    categoryName = "Champions";
                    break;
                default:
                    return -1;
                    break;
            }
            return FindCategoryIndexByName(dcs, categoryName);
        }

        internal static int FindCategoryIndexByName(DirectorCardCategorySelection dcs, string categoryName)
        {
            //Debug.Log("Dumping categories for " + dcs.name);
            for (int i = 0; i < dcs.categories.Length; i++)
            {
                //Debug.Log(dcs.categories[i].name);
                if (string.CompareOrdinal(dcs.categories[i].name, categoryName) == 0)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
