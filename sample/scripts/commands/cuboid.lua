Command("cuboid","[<block>]","Draws a cuboid.",
  function(command,player,message)
    local b = nil
    if message ~= "" then
      local n = tonumber(message)
      if n then b = Blocktype.FindById(n)
      else b = Blocktype.FindByName(message) end
      if not b then
        Message("&eUnknown blocktype '"..message.."'."):Send(player)
	    return
      end
    end
    if playerNextBlock[player] then player.BlockEvent:Remove(playerNextBlock[player].handle) end
    playerNextBlock[player] = { handle = player.BlockEvent:Add(CuboidFirst), name = "Drawing cuboid", block = b }
	Message("&ePlace the first block."):Send(player)
  end
)

function CuboidFirst(player,args)
  Message("&ePlace the second block."):Send(player)
  player.BlockEvent:Remove(playerNextBlock[player].handle)
  playerNextBlock[player] = { handle = player.BlockEvent:Add(CuboidSecond), name = "Drawing cuboid", block = playerNextBlock[player].block, first = args }
  args.Abort = true
end

function CuboidSecond(player,second,holding)
  local first = playerNextBlock[player].first
  local block = playerNextBlock[player].block or Blocktype.FindById(holding)
  player.BlockEvent:Remove(playerNextBlock[player].handle)
  playerNextBlock[player] = nil
  local x1,y1,z1,x2,y2,z2 = math.min(first.X,second.X),math.min(first.Y,second.Y),math.min(first.Z,second.Z),
                            math.max(first.X,second.X)+1,math.max(first.Y,second.Y)+1,math.max(first.Z,second.Z)+1
  Message("&eCuboid drawn: "..block.Name.." "..x1..","..y1..","..z1.." => "..x2..","..y2..","..z2.."."):Send(player)
  player.Level:Cuboid(player,x1,y1,z1,x2,y2,z2,block.id)
  second.Abort = true
end