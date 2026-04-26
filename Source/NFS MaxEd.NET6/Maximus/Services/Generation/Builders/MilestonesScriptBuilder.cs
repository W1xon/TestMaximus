using Maximus.Services.IR;
using Maximus.Services.IR.Renderers;

namespace Maximus.Services;

public class MilestonesScriptBuilder : ScriptBuilder
{
    private readonly string _raceBin;
    private int _binIndex;
    private string _collectionName;


    public MilestonesScriptBuilder(string raceBin)
    {
        Renderer = new MilestonesRenderer();
        _raceBin = raceBin;
        _binIndex = Convert.ToInt32(raceBin.Split("_")[1]);
    }

    public MilestonesScriptBuilder SetCollectionName(string milestoneType)
    {
        _collectionName = $"challenge_{_binIndex}_{milestoneType}";
        return this;
    }

    public MilestonesScriptBuilder AddNode(string milestoneType)
    {
        Doc.AddInstruction(
            InstrucionType.AddNode,
            subject: milestoneType,
            path: $"milestones/{_raceBin}/{_collectionName}");
        //_addCommands.Add($"add_node gameplay {milestoneType} milestones/{_raceBin}/{_collectionName}");
        return this;
    }

    public MilestonesScriptBuilder AddBinIndex()
    {
        Doc.AddInstruction(
            InstrucionType.AddField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "BinIndex");
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "BinIndex",
            value: _binIndex.ToString());
        //_addCommands.Add($"add_field gameplay milestones/{_raceBin}/{_collectionName} BinIndex");
        //_updateCommands.Add($"update_field gameplay milestones/{_raceBin}/{_collectionName} BinIndex {_binIndex}");
        return this;
    }

    public MilestonesScriptBuilder AddGoalAddPrevBest(int value)
    {
        Doc.AddInstruction(
            InstrucionType.AddField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "GoalAddPrevBest");
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "GoalAddPrevBest",
            value: value.ToString());
        //_addCommands.Add($"add_field gameplay milestones/{_raceBin}/{_collectionName} GoalAddPrevBest");
        //_updateCommands.Add($"update_field gameplay milestones/{_raceBin}/{_collectionName} GoalAddPrevBest {value}");
        return this;
    }

    public MilestonesScriptBuilder AddGoalEasy(int value)
    {
        Doc.AddInstruction(
            InstrucionType.AddField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "GoalEasy");
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "GoalEasy",
            value: value.ToString());
        
        //_addCommands.Add($"add_field gameplay milestones/{_raceBin}/{_collectionName} GoalEasy");
        //_updateCommands.Add($"update_field gameplay milestones/{_raceBin}/{_collectionName} GoalEasy {value}");
        return this;
    }

    public MilestonesScriptBuilder AddGoalHard(int value)
    {
        Doc.AddInstruction(
            InstrucionType.AddField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "GoalHard");
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "GoalHard",
            value: value.ToString());
        
        //_addCommands.Add($"add_field gameplay milestones/{_raceBin}/{_collectionName} GoalHard");
        //_updateCommands.Add($"update_field gameplay milestones/{_raceBin}/{_collectionName} GoalHard {value}");
        return this;
    }

    public MilestonesScriptBuilder AddTemplate()
    {
        Doc.AddInstruction(
            InstrucionType.AddField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "Template");
        //_addCommands.Add($"add_field gameplay milestones/{_raceBin}/{_collectionName} Template");
        return this;
    }

    public MilestonesScriptBuilder AddSpawnPoint(string spawnPoint)
    {
        Doc.AddInstruction(
            InstrucionType.AddField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "SpawnPoint");
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            path: $"milestones/{_raceBin}/{_collectionName}",
            subject: "SpawnPoint",
            value: spawnPoint);
        //_addCommands.Add($"add_field gameplay milestones/{_raceBin}/{_collectionName} SpawnPoint");
        //_updateCommands.Add($"update_field gameplay milestones/{_raceBin}/{_collectionName} SpawnPoint {spawnPoint}");
        return this;
    }

}
