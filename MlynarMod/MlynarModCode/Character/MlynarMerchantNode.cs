using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MlynarMod.MlynarModCode.Character;

[ScriptPath("res://MlynarMod/scenes/MlynarMerchantNode.cs")]
public partial class MlynarMerchantNode : Node2D
{
    public override void _Ready()
    {
        base._Ready();
        Node? sprite = FindSpineSprite(this);
        if (sprite == null)
            return;

        try
        {
            MegaSprite mega = new(sprite);
            mega.GetAnimationState().SetAnimation("Relax");
        }
        catch (System.Exception ex)
        {
            GD.PushWarning($"[MlynarMod] 商人入口动画失败: {ex.Message}");
        }
    }

    private static Node? FindSpineSprite(Node root)
    {
        if (root.GetClass() == "SpineSprite")
            return root;

        foreach (Node child in root.GetChildren())
        {
            Node? found = FindSpineSprite(child);
            if (found != null)
                return found;
        }

        return null;
    }
}
