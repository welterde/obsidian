playerNextBlock = {}
playerCode = {}

function PlayerSetNextBlock(player,func,info,...)
  PlayerAbortNextBlock(player)
  playerNextBlock[player] = {
    handle = player.BlockEvent:Add(PlayerOnNextBlock),
    func = func, info = info, arg = arg
  }
end

function PlayerOnNextBlock(player,args,holding)
  local t = playerNextBlock[player]
  PlayerAbortNextBlock(player)
  t.func(player,args,holding,unpack(t.arg))
end

function PlayerAbortNextBlock(player)
  local t = playerNextBlock[player]
  if t then player.BlockEvent:Remove(t.handle) end
end

function PlayerNextBlockInfo(player)
  return playerNextBlock[player].info
end
