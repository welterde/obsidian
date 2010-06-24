Command("cuboid wireframe","[<block>]","Draws a wireframe cuboid.",
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
    PlayerSetNextBlock(player,CuboidWireframeFirst,"Drawing wireframe cuboid",b)
    Message("&ePlace the first block."):Send(player)
  end
)

function CuboidWireframeFirst(player,args,holding,block)
  Message("&ePlace the second block."):Send(player)
  PlayerSetNextBlock(player,CuboidWireframeSecond,"Drawing wireframe cuboid",block,args)
  args.Abort = true
end

function CuboidWireframeSecond(player,second,holding,block,first)
  block = block or Blocktype.FindById(holding)
  local x1,y1,z1,x2,y2,z2 = math.min(first.X,second.X),math.min(first.Y,second.Y),math.min(first.Z,second.Z),
                            math.max(first.X,second.X)+1,math.max(first.Y,second.Y)+1,math.max(first.Z,second.Z)+1
  Message("&eWireframe cuboid drawn: "..block.Name.." "..x1..","..y1..","..z1.." => "..x2..","..y2..","..z2.."."):Send(player)
  player.Level:Cuboid(player,x1,y1,z1,x2,y2,z2,block.id,Level.DrawMode.Wireframe)
  second.Abort = true
end
