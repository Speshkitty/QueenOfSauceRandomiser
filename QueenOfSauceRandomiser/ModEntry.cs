using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace QueenOfSauceRandomiser
{
    public class ModEntry : Mod, IAssetEditor
    {
        //We need to save and load data about the shuffle

        private readonly string OurDataPath = "speshkitty.queenofsauceshuffle";
        private readonly string XNBDataPath = "Data/TV/CookingChannel";

        Dictionary<int, int> ShuffleData = new Dictionary<int, int>();
        
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.SaveCreating += GameLoop_SaveCreating;
        }

        //At this step, we want to shuffle the recipes as this is when the save is initially created
        private void GameLoop_SaveCreating(object sender, SaveCreatingEventArgs e)
        {
            //We need to find out how many QoS recipes there are by loading the file:
            IDictionary<string, string> BaseQoSRecipes = Helper.Content.Load<IDictionary<string, string>>(XNBDataPath, ContentSource.GameContent);

            //the format:
            // "IDNum" : "text data
            ShuffleData = new Dictionary<int, int>();

            List<int> AvailableIDs = Enumerable.Range(1, BaseQoSRecipes.Count).ToList();

            List<string> OldIDLists = BaseQoSRecipes.Keys.ToList();
            for (int i = 0; i < OldIDLists.Count; i++)
            {
                int oldID = int.Parse(OldIDLists[i]);
                int newID = AvailableIDs[Game1.random.Next(0, AvailableIDs.Count)];

                AvailableIDs.Remove(newID);

                ShuffleData.Add(oldID, newID);

            }
            Helper.Data.WriteSaveData(OurDataPath, ShuffleData);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            //Load the list of shuffled recipes
            ShuffleData = Helper.Data.ReadSaveData<Dictionary<int, int>>(OurDataPath);
        }

        public bool CanEdit<T>(IAssetInfo asset) => asset.AssetNameEquals(XNBDataPath) && ShuffleData != null && ShuffleData.Count != 0;
        
        public void Edit<T>(IAssetData asset)
        {
            IDictionary<string, string> BaseData = asset.AsDictionary<string, string>().Data;
            foreach (KeyValuePair<string, string> kvp in new Dictionary<string, string>(BaseData))
            {
                if(ShuffleData.TryGetValue(int.Parse(kvp.Key), out int NewID))
                {
                    BaseData[NewID.ToString()] = kvp.Value;
                }
                else
                {
                    BaseData[kvp.Key] = kvp.Value;
                }
            }
            
        }
    }
}
