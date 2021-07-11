namespace Quantum {
  public unsafe class ChooseMapNodeCommandSystem : SystemMainThread {
    public override void Update(Frame f) {
      for (int playerID = 0; playerID < f.Global -> Players.Length; playerID++) {
        ChooseMapNodeCommand command = f.GetPlayerCommand(playerID) as ChooseMapNodeCommand;
        if (command != null) {
          // If this play is reached node and this node has next destinations
          if (f.Global -> Players[playerID].ReachedNode) {
            SetPlayerNextDestination(f, command, playerID);
          }
        }
      }
    }

    private void SetPlayerNextDestination(Frame f, ChooseMapNodeCommand command, int playerID) {
      BattlePlayer* player = f.Global -> Players.GetPointer(playerID);
      MapNode mapNode = f.Get<MapNode>(player->TargetMapNode);
      if (mapNode.NextNodes.Length > command.NextNodeIndex &&
          mapNode.NextNodes[command.NextNodeIndex] != null) {
        player->TargetMapNode = mapNode.NextNodes[command.NextNodeIndex];
        player->ReachedNode = false;
      }
    }
  }
}