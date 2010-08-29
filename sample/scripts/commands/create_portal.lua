Command("create portal","","Creates a new portal.",
  function(command,player,message)
    if message ~= "" then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    Message("&ePlace the first portal entrance."):Send(player)
    PlayerSetNextBlock(player,CreatePortalFirst,"Portal creation")
  end
)

function CreatePortalFirst(player,args,holding)
  Message("&ePlace the second portal entrance."):Send(player)
  PlayerSetNextBlock(player,CreatePortalSecond,"Portal creation",args,
                     math.floor(player.Position.RotX/64+0.5)*64)
  args.Abort = true
end

function CreatePortalSecond(player,second,holding,first,rot)
  rot = rot - ((math.floor(player.Position.RotX/64+0.5)*64+128) % 256)
  Message("&ePortal created: "..first.X..","..first.Y..","..first.Z..
          " <=> "..second.X..","..second.Y..","..second.Z.."."):Send(player)
  player.Level:Cuboid(nil,first.X,first.Y,first.Z,first.X+1,first.Y+2,first.Z+1,0)
  player.Level:Cuboid(nil,second.X,second.Y,second.Z,second.X+1,second.Y+2,second.Z+1,0)
  local portal = Portal(player.Level,first,second)
  portal.Orientation = rot
  if levelPortals ~= nil then
    table.insert(levelPortals,portal)
  end
  if second.Type ~= 0 then second.Abort = true end
end
