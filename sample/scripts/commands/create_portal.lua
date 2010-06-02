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
  local portal = Portal(player.Level,first,second)
  portal.Orientation = rot
  table.insert(levelPortals,portal)
  if second.Type ~= 0 then second.Abort = true end
end
