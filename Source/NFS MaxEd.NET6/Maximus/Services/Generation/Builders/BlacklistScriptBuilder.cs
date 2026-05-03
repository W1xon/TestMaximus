using System.Text;
using Maximus.Services.IR;
using Maximus.Services.IR.Renderers;

namespace Maximus.Services;

public class BlacklistScriptBuilder : ScriptBuilder
{
    private readonly string _raceBin;

    public BlacklistScriptBuilder(string raceBin)
    {
        _raceBin = raceBin;
        Renderer = new BlacklistRenderer();
    }

    public BlacklistScriptBuilder AddRequiredBounty(int bounty)
    {
        Doc.AddInstruction(
            InstructionType.UpdateField,
            path: _raceBin,
            subject: "RequiredBounty",
            value: bounty.ToString()
            );
        
        //_commands.AppendLine($"update_field gameplay {_raceBin} RequiredBounty {bounty}");
        return this;
    }

    public BlacklistScriptBuilder AddRequiredChallenges(int challenges)
    {
        Doc.AddInstruction(
            InstructionType.UpdateField,
            path: _raceBin,
            subject: "RequiredChallenges",
            value: challenges.ToString());
        //_commands.AppendLine($"update_field gameplay {_raceBin} RequiredChallenges {challenges}");
        return this;
    }

    public BlacklistScriptBuilder AddRequiredRaceWon(int races)
    {
        Doc.AddInstruction(
            InstructionType.UpdateField,
            path: _raceBin,
            subject: "RequiredRacesWon",
            value: races.ToString());
        //_commands.AppendLine($"update_field gameplay {_raceBin} RequiredRacesWon {races}");
        return this;
    }

    public BlacklistScriptBuilder AddResizeField(string field, int newSize)
    {
        Doc.AddInstruction(
            InstructionType.ResizeField,
            path: _raceBin,
            subject: field,
            value: newSize.ToString());
        //_commands.AppendLine($"resize_field gameplay {_raceBin} {field} {newSize}");
        return this;
    }

    public BlacklistScriptBuilder AddUpdateField(string field, int index, string value)
    {
        Doc.AddInstruction(
            InstructionType.UpdateField,
            path: _raceBin,
            subject: $"{field}[{index}]",
            value: $"{_raceBin}/{value}");
        //_commands.AppendLine($"update_field gameplay {_raceBin} {field}[{index}] {_raceBin}/{value}");
        return this;
    }
    public BlacklistScriptBuilder AddResizeMilestoneField(int newSize)
    {
        string bin = _raceBin.Replace("race_","");
        Doc.AddInstruction(
            InstructionType.ResizeField,
            path: $"milestones/{bin}",
            subject: "Children",
            value: newSize.ToString());
        
        //_commands.AppendLine($"resize_field gameplay milestones/{bin} Children {newSize}");
        return this;
    }

    public BlacklistScriptBuilder AddUpdateMilestoneField(int index, string value)
    {
        string bin = _raceBin.Replace("race_","");
        Doc.AddInstruction(
            InstructionType.UpdateField,
            path: $"milestones/{bin}",
            subject: $"Children[{index}]",
            value: value);
        
        //_commands.AppendLine($"update_field gameplay milestones/{bin} Children[{index}] {value}");
        return this;
    }
}